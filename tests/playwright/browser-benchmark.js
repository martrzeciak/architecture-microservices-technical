// =============================================================================
// Browser Benchmark: REST vs gRPC-Web (Envoy) vs gRPC-Web (Direct)
// =============================================================================
// Odtwarza macierz badawcza scenariuszy k6 w warunkach przegladarkowych.
// Mierzy czas od klikniecia w aplikacji Angular do naniesienia odpowiedzi na DOM.
//
// Wymiary odtworzone z k6:
//   - rozmiar strony (PAGE_SIZE):        10, 50, 100, 200
//   - stan cache (BYPASS_CACHE):         cieply, zimny
//   - pozycje zamowienia (ORDER_ITEMS):  1, 5, 10
//   - liczba uzytkownikow (VU):          10, 50  (patrz README: 100/500 nie
//                                        mieszcza sie na jednej stacji roboczej)
//
// Wymiary nieodtwarzalne w przegladarce (pozostaja w k6):
//   - natywne gRPC (HTTP/2 z trailerami — niedostepne z fetch/XHR)
//   - strumieniowanie serwerowe natywnego gRPC
//
// Uruchamiane przez run-browser-benchmark.ps1 (patrz README.md).
// Wynik: ../../hetzner-results/browser-benchmark-TIMESTAMP.json
// =============================================================================

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

// === PARAMETRY ===
const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:4200';
const ITERATIONS = int(process.env.ITER, 20);
const RUNS = int(process.env.RUNS, 5);
const COOLDOWN_SEC = int(process.env.COOLDOWN, 15);
// Rozmiary stron pokrywaja trzy rezimy pomiarowe: zdominowany przez RTT (10-200),
// skokowy od progow slow startu TCP (500-1000) i proporcjonalny do bajtow (2000).
// Baza zasiewowa ma 2000 rekordow (SEED_PRODUCT_COUNT).
const PAGE_SIZES = list(process.env.PAGE_SIZES, '10,100,200,500,1000,2000').map(Number);
const CACHE_STATES = list(process.env.CACHE_STATES, 'warm,cold');
// 50 i 200 wykracza poza macierz k6 (1/5/10) — rozszerza wymiar rozmiaru ladunku
// takze na sciezke zapisu. Uwaga: w zapisie dominuje staly koszt serwerowy
// (transakcja, outbox, saga), wiec ten wymiar dokumentuje wplyw rozmiaru zadania,
// a nie izoluje narzutu protokolu — do tego sluzy scenariusz echo.
const ORDER_ITEMS = list(process.env.ORDER_ITEMS, '1,10,50,200').map(Number);
// 500-5000 wykracza poza macierz k6 (10/100/200) — dodane, bo przelom w roznicy
// miedzy protokolami wypada powyzej 200 rekordow (patrz README).
const ECHO_SIZES = list(process.env.ECHO_SIZES, '10,100,200,500,2000,5000').map(Number);
const VU_LIST = list(process.env.VU_LIST, '10,50').map(Number);

// Gorny limit iloczynu VU x liczba rekordow na zadanie.
// Powyzej tego progu waskim gardlem przestaje byc protokol, a staje sie baza
// (Products) lub procesor serwera (Echo) — mierzylibysmy nasycenie, nie protokol.
// Takie komorki sa pomijane i wypisane na wejsciu.
const MAX_VU_ROWS = int(process.env.MAX_VU_ROWS, 25000);

// Osobny, znacznie nizszy limit dla scenariusza zapisu: kazda pozycja zamowienia
// to trwaly wiersz w bazie, a nie odczyt. Bez tego 200 pozycji przy 50 VU
// wygenerowaloby miliony wierszy w OrderItems, co rozdymaloby tabele w trakcie
// pomiaru i zaburzalo pozniejsze przebiegi.
const MAX_VU_ITEMS = int(process.env.MAX_VU_ITEMS, 5000);

// === BUDZET PASMA KLIENTA ===
// Zapotrzebowanie komorki na pasmo to VU x rozmiar_ladunku / dlugosc_cyklu.
// Przy 100 Mbit/s i czasie namyslu 100 ms najciezsze komorki zadalyby ~230 Mbit/s,
// czyli lacze klienta stalo by sie waskim gardlem. Wtedy REST — wysylajacy o ~43%
// wiecej bajtow — spuchlby bardziej niz protobuf, co ZAWYZYLOBY przewage gRPC:
// mierzylibysmy przepustowosc lacza, nie protokol.
//
// Mierzona wielkoscia jest opoznienie pojedynczego zadania, nie przepustowosc,
// dlatego zamiast usuwac komorki wydluzamy czas namyslu tak, aby zapotrzebowanie
// zmiescilo sie w budzecie. Kazde zadanie mierzy sie identycznie, tylko rzadziej.
const LINK_MBPS = int(process.env.LINK_MBPS, 100);      // pasmo w dol
const UPLINK_MBPS = int(process.env.UPLINK_MBPS, 20);   // pasmo w gore (scenariusz zapisu)
const LINK_BUDGET = 0.5;                                // wykorzystujemy najwyzej polowe
const BASE_THINK_MS = 100;

// Rozmiary wyznaczone pomiarem odpowiedzi REST na wdrozonym backendzie:
// products 26 405 B / 200 rek. i 265 159 B / 2000 rek. -> 133 B/rekord
// echo     36 242 B / 200 rek.                          -> 181 B/rekord
const BYTES_PER_RECORD = { echo: 181, products: 133 };
const BYTES_PER_ORDER_ITEM = 90;

function thinkTimeMs(cell) {
  let bytes;
  let budgetBps;

  if (cell.scenario === 'orders') {
    // Scenariusz zapisu obciaza kierunek w gore, ktory na laczach asymetrycznych
    // jest slabszy — dlatego osobny, nizszy budzet.
    bytes = Math.max(1, cell.items) * BYTES_PER_ORDER_ITEM;
    budgetBps = UPLINK_MBPS * 1e6 * LINK_BUDGET;
  } else {
    const rows = cell.scenario === 'echo' ? cell.echoSize : cell.pageSize;
    bytes = rows * BYTES_PER_RECORD[cell.scenario];
    budgetBps = LINK_MBPS * 1e6 * LINK_BUDGET;
  }

  // Cykl o dlugosci T daje agregat VU*bytes/T; wymagamy agregatu <= budzet.
  // Pomijamy opoznienie w mianowniku, wiec wynik jest zachowawczy: rzeczywisty
  // cykl (namysl + opoznienie) jest dluzszy, a wiec agregat jeszcze nizszy.
  const requiredCycleMs = (cell.vu * bytes * 8 / budgetBps) * 1000;
  return Math.max(BASE_THINK_MS, Math.round(requiredCycleMs));
}

const WARMUP_REQUESTS = 20; // na protokol, przed rozpoczeciem pomiarow
const PRIME_REQUESTS = 3;   // na worker w kazdej komorce, nierejestrowane

const PROTOCOLS = [
  { key: 'rest', label: 'REST' },
  { key: 'grpc_web_envoy', label: 'gRPC-Web (Envoy)' },
  { key: 'grpc_web_direct', label: 'gRPC-Web (Direct)' },
];

// Kwadrat lacinski — rotacja kolejnosci protokolow miedzy przebiegami
const ROTATIONS = [
  [0, 1, 2],
  [1, 2, 0],
  [2, 0, 1],
  [0, 2, 1],
  [2, 1, 0],
];

// === HELPERS ===
function int(v, def) {
  const n = parseInt(v ?? '', 10);
  return Number.isFinite(n) ? n : def;
}

function list(v, def) {
  return String(v ?? def).split(',').map(s => s.trim()).filter(Boolean);
}

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function percentile(sorted, p) {
  const idx = Math.floor(sorted.length * p);
  return sorted[Math.min(idx, sorted.length - 1)];
}

function round(v) {
  return Math.round(v * 10) / 10;
}

function stats(times) {
  const sorted = [...times].sort((a, b) => a - b);
  const n = sorted.length;
  return {
    count: n,
    min: sorted[0],
    avg: round(times.reduce((a, b) => a + b, 0) / n),
    med: sorted[Math.floor(n / 2)],
    p90: percentile(sorted, 0.90),
    p95: percentile(sorted, 0.95),
    p99: percentile(sorted, 0.99),
    max: sorted[n - 1],
  };
}

/**
 * Rozdziela probki na wlasciwe i zaciecia.
 *
 * Zaciecia (~2,6 s przy medianie ~33 ms) sa skupione w czasie i dotykaja
 * wszystkich workerow jednoczesnie, a diagnostyka wykazala, ze nie wypadaja
 * przy nawiazywaniu polaczenia (iteracje 0-1). Wskazuje to na przejsciowe
 * zdarzenia w sieci dostepowej klienta, niezalezne od protokolu — trafiaja ten,
 * ktory akurat jest mierzony.
 *
 * Statystyki glowne liczone sa bez nich, ale odrzucenie jest jawne: liczba,
 * odsetek i prog trafiaja do wyniku, a statystyki nieodfiltrowane sa zachowane
 * w polu stats_all. Nic nie jest ukrywane.
 */
function splitStalls(times) {
  const kept = [];
  const stalls = [];
  for (const t of times) {
    if (t > STALL_THRESHOLD_MS) stalls.push(t); else kept.push(t);
  }
  return { kept, stalls };
}

function stdev(values) {
  if (values.length < 2) return null;
  const avg = values.reduce((a, b) => a + b, 0) / values.length;
  const variance = values.reduce((s, v) => s + (v - avg) ** 2, 0) / (values.length - 1);
  return { sd: Math.sqrt(variance), avg };
}

// === MACIERZ BADAWCZA ===
function buildCells() {
  const cells = [];
  const skipped = [];

  // ECHO — czysty narzut protokolu, bez bazy i bez cache
  for (const vu of VU_LIST) {
    for (const size of ECHO_SIZES) {
      if (vu * size > MAX_VU_ROWS) {
        skipped.push(`ECHO VU=${vu} n=${size}`);
        continue;
      }
      cells.push({
        scenario: 'echo',
        vu,
        echoSize: size,
        url: `/demo/echo?count=${size}`,
        key: `echo_VU${vu}_N${size}`,
        label: `ECHO     VU=${vu} n=${size}`,
      });
    }
  }

  for (const vu of VU_LIST) {
    for (const pageSize of PAGE_SIZES) {
      if (vu * pageSize > MAX_VU_ROWS) {
        for (const cache of CACHE_STATES) {
          skipped.push(`PRODUCTS VU=${vu} PS=${pageSize} cache=${cache}`);
        }
        continue;
      }
      for (const cache of CACHE_STATES) {
        cells.push({
          scenario: 'products',
          vu,
          pageSize,
          cache,
          url: `/demo/products?pageSize=${pageSize}&cold=${cache === 'cold' ? 1 : 0}`,
          key: `products_VU${vu}_PS${pageSize}_CACHE${cache.toUpperCase()}`,
          label: `PRODUCTS VU=${vu} PS=${pageSize} cache=${cache}`,
        });
      }
    }
  }

  for (const vu of VU_LIST) {
    for (const items of ORDER_ITEMS) {
      if (vu * items > MAX_VU_ITEMS) {
        skipped.push(`ORDERS VU=${vu} items=${items}`);
        continue;
      }
      cells.push({
        scenario: 'orders',
        vu,
        items,
        url: `/demo/orders?items=${items}`,
        key: `orders_VU${vu}_IT${items}`,
        label: `ORDERS   VU=${vu} items=${items}`,
      });
    }
  }

  for (const cell of cells) {
    cell.thinkMs = thinkTimeMs(cell);
  }

  cells.skipped = skipped;
  return cells;
}

// === POMIAR WEWNATRZ PRZEGLADARKI ===
// Klikniecie i wykrycie zakonczenia dzieja sie w przegladarce, mierzone przez
// performance.now(). Odpytywanie DOM z Node.js przez CDP kosztuje jedna podroz
// tam i z powrotem na proba, wiec nie rozdziela odpowiedzi ponizej ~100 ms i
// przy przegapionym stanie przejsciowym doklada staly narzut rowny limitowi czasu.
// Komponent zwieksza data-load-id / data-submit-id po naniesieniu odpowiedzi na
// DOM, a MutationObserver wychwytuje to w tym samym takcie.
async function measureInPage(page, cfg) {
  return page.evaluate(async (c) => {
    const host = document.querySelector(`[${c.attr}]`);
    if (!host) return { ok: false, reason: 'host-missing' };

    let trigger;
    if (c.label) {
      trigger = Array.from(document.querySelectorAll('button.protocol-btn'))
        .find(b => (b.textContent || '').trim() === c.label);
    } else {
      trigger = document.querySelector(c.triggerSelector);
    }
    if (!trigger) return { ok: false, reason: 'trigger-missing' };
    if (trigger.disabled) return { ok: false, reason: 'trigger-disabled' };

    const prev = host.getAttribute(c.attr);
    const done = new Promise((resolve) => {
      const timer = setTimeout(() => { obs.disconnect(); resolve(null); }, c.timeoutMs);
      const obs = new MutationObserver(() => {
        if (host.getAttribute(c.attr) !== prev) {
          clearTimeout(timer);
          obs.disconnect();
          resolve(performance.now());
        }
      });
      obs.observe(host, { attributes: true, attributeFilter: [c.attr] });
    });

    const t0 = performance.now();
    trigger.click();
    const t1 = await done;

    if (t1 === null) return { ok: false, reason: 'timeout' };
    if (host.getAttribute(c.statusAttr) === 'error') return { ok: false, reason: 'app-error' };
    return { ok: true, ms: t1 - t0 };
  }, cfg);
}

const PRODUCTS_MEASURE = {
  attr: 'data-load-id',
  statusAttr: 'data-load-status',
  timeoutMs: 30000,
};

const ORDERS_MEASURE = {
  attr: 'data-submit-id',
  statusAttr: 'data-submit-status',
  triggerSelector: 'button.submit-btn',
  label: null,
  timeoutMs: 30000,
};

// Wybor protokolu bez pomiaru — dopasowanie po dokladnym tekscie przycisku
async function selectProtocol(page, protocolLabel) {
  return page.evaluate((label) => {
    const btn = Array.from(document.querySelectorAll('button.protocol-btn'))
      .find(b => (b.textContent || '').trim() === label);
    if (!btn) return false;
    btn.click();
    return true;
  }, protocolLabel);
}

// === WORKER: JEDNA PRZEGLADARKA, JEDNA KOMORKA MACIERZY ===
// Prog, powyzej ktorego probka jest traktowana jako zaciecie i raportowana
// osobno. W pomiarach kontrolnych ~2,5% probek wychodzilo ~2,6 s przy medianie
// ~33 ms, niezaleznie od protokolu. Diagnostyka notuje numer iteracji, zeby
// ustalic, czy zaciecia wypadaja przy nawiazywaniu polaczenia (pierwsze iteracje)
// czy losowo w trakcie.
const STALL_THRESHOLD_MS = 1000;

async function worker(browser, cell, protocol) {
  const times = [];
  const stalls = [];
  let errors = 0;
  const isOrders = cell.scenario === 'orders';
  const measure = isOrders
    ? ORDERS_MEASURE
    : { ...PRODUCTS_MEASURE, label: protocol.label };

  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page = await context.newPage();

  try {
    await page.goto(`${FRONTEND_URL}${cell.url}`, {
      waitUntil: 'networkidle',
      timeout: 60000,
    });

    // W scenariuszu zapisu protokol wybieramy raz; mierzonym wyzwalaczem jest
    // przycisk zlozenia zamowienia. W scenariuszu odczytu samo klikniecie
    // przycisku protokolu wywoluje pobranie listy, wiec jest wyzwalaczem.
    if (isOrders) {
      const selected = await selectProtocol(page, protocol.label);
      if (!selected) throw new Error(`protocol button not found: ${protocol.label}`);
      await sleep(200);
    }

    // Zapytania przygotowawcze — wypelniaja cache dla tego rozmiaru strony
    // i stabilizuja polaczenie. Nie sa rejestrowane.
    for (let i = 0; i < PRIME_REQUESTS; i++) {
      await measureInPage(page, measure);
      await sleep(50);
    }

    for (let i = 0; i < ITERATIONS; i++) {
      const r = await measureInPage(page, measure);
      if (r.ok) {
        times.push(round(r.ms));
        if (r.ms > STALL_THRESHOLD_MS) stalls.push({ iteration: i, ms: round(r.ms) });
      } else {
        errors++;
        if (errors === 1) {
          console.error(`      ! ${cell.label} ${protocol.label}: ${r.reason}`);
        }
      }
      await sleep(cell.thinkMs); // czas namyslu, dopasowany do budzetu pasma
    }
  } catch (err) {
    console.error(`      ! worker error (${cell.label} ${protocol.label}): ${err.message}`);
    errors++;
  } finally {
    await context.close();
  }

  return { times, errors, stalls };
}

// === TEST JEDNEJ KOMORKI DLA JEDNEGO PROTOKOLU ===
async function testCell(browser, cell, protocol) {
  const workers = [];
  const startAll = Date.now();
  for (let v = 0; v < cell.vu; v++) {
    workers.push(worker(browser, cell, protocol));
  }

  const results = await Promise.all(workers);
  const durationSec = (Date.now() - startAll) / 1000;
  const allTimes = results.flatMap(r => r.times);
  const errors = results.reduce((s, r) => s + r.errors, 0);
  const allStalls = results.flatMap(r => r.stalls);

  if (allTimes.length === 0) {
    console.log(`      ${protocol.label.padEnd(18)} BRAK PROBEK (bledow: ${errors})`);
    return { stats: null, times: [], errors };
  }

  // Statystyki glowne bez zaciec; wersja pelna zachowana w stats_all.
  const { kept, stalls } = splitStalls(allTimes);
  const s = kept.length > 0 ? stats(kept) : stats(allTimes);
  s.stats_all = stats(allTimes);
  s.stall_count = stalls.length;
  s.stall_rate_pct = round((stalls.length / allTimes.length) * 100);
  s.stall_threshold_ms = STALL_THRESHOLD_MS;
  // Przepustowosc liczona ze WSZYSTKICH probek i rzeczywistego czasu trwania,
  // wiec polityka odrzucania jej nie dotyczy.
  s.throughput_rps = round(allTimes.length / durationSec);
  s.total_duration_sec = round(durationSec);
  s.errors = errors;

  const stallInfo = stalls.length > 0
    ? `  zaciec=${stalls.length} (${s.stall_rate_pct}%, odrzucone)`
    : '';

  console.log(
    `      ${protocol.label.padEnd(18)} n=${String(s.count).padStart(5)} err=${String(errors).padStart(3)}` +
    ` med=${String(s.med).padStart(7)}ms avg=${String(s.avg).padStart(7)}ms` +
    ` p95=${String(s.p95).padStart(7)}ms  ${s.throughput_rps} ops/s${stallInfo}`
  );

  return { stats: s, times: allTimes, errors, stalls: allStalls };
}

// === ROZGRZEWKA ===
async function warmup(browser) {
  console.log(`Rozgrzewka: ${WARMUP_REQUESTS} zapytan na protokol...`);
  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page = await context.newPage();

  try {
    // Echo — osobna metoda po stronie serwera, wiec wymaga wlasnej kompilacji JIT
    await page.goto(`${FRONTEND_URL}/demo/echo?count=200`, {
      waitUntil: 'networkidle',
      timeout: 60000,
    });
    for (const proto of PROTOCOLS) {
      for (let i = 0; i < WARMUP_REQUESTS; i++) {
        await measureInPage(page, { ...PRODUCTS_MEASURE, label: proto.label, timeoutMs: 15000 });
        await sleep(50);
      }
    }

    // Odczyt
    await page.goto(`${FRONTEND_URL}/demo/products?pageSize=10&cold=0`, {
      waitUntil: 'networkidle',
      timeout: 60000,
    });
    for (const proto of PROTOCOLS) {
      for (let i = 0; i < WARMUP_REQUESTS; i++) {
        await measureInPage(page, { ...PRODUCTS_MEASURE, label: proto.label, timeoutMs: 15000 });
        await sleep(50);
      }
    }

    // Zapis
    await page.goto(`${FRONTEND_URL}/demo/orders?items=1`, {
      waitUntil: 'networkidle',
      timeout: 60000,
    });
    for (const proto of PROTOCOLS) {
      await selectProtocol(page, proto.label);
      await sleep(200);
      for (let i = 0; i < WARMUP_REQUESTS; i++) {
        await measureInPage(page, { ...ORDERS_MEASURE, timeoutMs: 15000 });
        await sleep(50);
      }
    }
  } finally {
    await context.close();
  }
  console.log('Rozgrzewka zakonczona.');
}

// === MAIN ===
async function main() {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
  const cells = buildCells();
  const totalTests = cells.length * PROTOCOLS.length * RUNS;

  console.log('='.repeat(78));
  console.log('Browser Benchmark: REST vs gRPC-Web (Envoy) vs gRPC-Web (Direct)');
  console.log(`Cel:            ${FRONTEND_URL} -> backend Hetzner`);
  console.log(`Rozmiary echo:   ${ECHO_SIZES.join(', ')}`);
  console.log(`Rozmiary strony: ${PAGE_SIZES.join(', ')}`);
  console.log(`Stan cache:      ${CACHE_STATES.join(', ')}`);
  console.log(`Pozycje zamowien:${ORDER_ITEMS.join(', ')}`);
  console.log(`Uzytkownicy (VU):${VU_LIST.join(', ')}`);
  console.log(`Iteracje/VU:     ${ITERATIONS}   Przebiegi: ${RUNS}   Przerwa: ${COOLDOWN_SEC}s`);
  console.log(`Komorek macierzy:${cells.length}  ->  ${totalTests} wykonanych testow`);
  if (cells.skipped.length > 0) {
    console.log(`Pominietych:     ${cells.skipped.length}` +
      ` (odczyt: VU x rekordy > ${MAX_VU_ROWS}, zapis: VU x pozycje > ${MAX_VU_ITEMS})`);
    for (const s of cells.skipped) console.log(`                 - ${s}`);
  }
  console.log(`Budzet pasma:    ${LINK_MBPS} Mbit/s w dol, ${UPLINK_MBPS} Mbit/s w gore` +
    ` (wykorzystanie do ${LINK_BUDGET * 100}%)`);
  console.log('Kolejnosc protokolow: rotacja (kwadrat lacinski)');
  console.log('='.repeat(78));
  console.log();

  // Tryb planowania: wypisz macierz i szacunki, nic nie mierz.
  if (process.env.DRY_RUN === '1') {
    let samples = 0;
    let requests = 0;
    let estSec = 0;
    console.log('KOMORKI (szczytowe pasmo liczone zachowawczo, bez opoznienia w cyklu):');
    for (const cell of cells) {
      const perTest = cell.vu * ITERATIONS;
      samples += perTest * PROTOCOLS.length * RUNS;
      requests += (cell.vu * (ITERATIONS + PRIME_REQUESTS)) * PROTOCOLS.length * RUNS;

      const rows = cell.scenario === 'echo' ? cell.echoSize
                 : cell.scenario === 'products' ? cell.pageSize
                 : Math.max(1, cell.items);
      const bytes = cell.scenario === 'orders'
        ? rows * BYTES_PER_ORDER_ITEM
        : rows * BYTES_PER_RECORD[cell.scenario];
      const mbps = (cell.vu * bytes * 8) / (cell.thinkMs / 1000) / 1e6;

      // Zachowawczo: cykl = namysl + ~50 ms narzutu, plus ~4 s na start kontekstow
      const testSec = (ITERATIONS + PRIME_REQUESTS) * (cell.thinkMs + 50) / 1000 + 4;
      estSec += testSec * PROTOCOLS.length * RUNS;

      console.log(`   ${cell.label.padEnd(30)} ${String(perTest).padStart(5)} probek` +
        `  namysl ${String(cell.thinkMs).padStart(5)} ms` +
        `  szczyt ${mbps.toFixed(0).padStart(3)} Mbit/s`);
    }
    const warmupReqs = WARMUP_REQUESTS * PROTOCOLS.length * 3;
    console.log();
    console.log(`Komorek:            ${cells.length}`);
    console.log(`Testow:             ${totalTests}`);
    console.log(`Probek zmierzonych: ${samples.toLocaleString('pl-PL')}`);
    console.log(`Zadan lacznie:      ${(requests + warmupReqs).toLocaleString('pl-PL')} (z rozgrzewka i zapytaniami przygotowawczymi)`);

    const orderCells = cells.filter(c => c.scenario === 'orders');
    const orderWrites = orderCells.reduce(
      (s, c) => s + c.vu * (ITERATIONS + PRIME_REQUESTS) * PROTOCOLS.length * RUNS, 0);
    console.log(`Zamowien do zapisu: ${orderWrites.toLocaleString('pl-PL')} (rosnaca tabela — patrz README)`);
    console.log(`Szacowany czas:     ~${(estSec / 3600).toFixed(1)} h` +
      ` (bez przerw miedzy przebiegami: ${((RUNS - 1) * COOLDOWN_SEC / 60).toFixed(0)} min)`);
    return;
  }

  if (cells.length === 0) {
    console.error('Brak komorek do zmierzenia — wszystkie listy puste lub odfiltrowane');
    console.error(`Sprawdz parametry oraz MAX_VU_ROWS (${MAX_VU_ROWS}).`);
    process.exit(1);
  }

  const browser = await chromium.launch({ headless: true });

  await warmup(browser);
  console.log();

  // acc[cellKey][protoKey] = { runs: [], allTimes: [], errors }
  const acc = {};
  for (const cell of cells) {
    acc[cell.key] = {};
    for (const proto of PROTOCOLS) {
      acc[cell.key][proto.key] = { runs: [], allTimes: [], errors: 0 };
    }
  }

  const startedAt = Date.now();
  let done = 0;

  for (let run = 0; run < RUNS; run++) {
    const rotation = ROTATIONS[run % ROTATIONS.length];
    const ordered = rotation.map(i => PROTOCOLS[i]);
    console.log(`### Przebieg ${run + 1}/${RUNS} (kolejnosc: ${ordered.map(p => p.key).join(' -> ')})`);

    for (const cell of cells) {
      console.log(`   ${cell.label}`);
      for (const proto of ordered) {
        const r = await testCell(browser, cell, proto);
        const slot = acc[cell.key][proto.key];
        if (r.stats) slot.runs.push(r.stats);
        slot.allTimes.push(...r.times);
        slot.errors += r.errors;

        done++;
        const elapsedMin = (Date.now() - startedAt) / 60000;
        const etaMin = (elapsedMin / done) * (totalTests - done);
        process.stdout.write(
          `      [${done}/${totalTests}] uplynelo ${elapsedMin.toFixed(1)} min,` +
          ` pozostalo ~${etaMin.toFixed(0)} min\n`
        );
      }
    }

    if (run < RUNS - 1) {
      console.log(`   Przerwa ${COOLDOWN_SEC}s...`);
      await sleep(COOLDOWN_SEC * 1000);
    }
    console.log();
  }

  await browser.close();

  // === AGREGACJA ===
  const summary = {
    test: 'browser-benchmark',
    timestamp,
    config: {
      frontend_url: FRONTEND_URL,
      page_sizes: PAGE_SIZES,
      cache_states: CACHE_STATES,
      order_items: ORDER_ITEMS,
      echo_sizes: ECHO_SIZES,
      vu_list: VU_LIST,
      max_vu_rows: MAX_VU_ROWS,
      max_vu_items: MAX_VU_ITEMS,
      skipped_cells: cells.skipped,
      iterations_per_vu: ITERATIONS,
      runs: RUNS,
      cooldown_sec: COOLDOWN_SEC,
      warmup_requests_per_protocol: WARMUP_REQUESTS,
      prime_requests_per_worker: PRIME_REQUESTS,
      think_time_ms: 'adaptacyjny — per komorka, patrz cells[].think_time_ms',
      link_mbps: LINK_MBPS,
      uplink_mbps: UPLINK_MBPS,
      link_budget: LINK_BUDGET,
      rotation: 'latin_square',
      measurement: 'click -> DOM updated (in-page performance.now)',
      stall_policy: {
        threshold_ms: STALL_THRESHOLD_MS,
        excluded_from: 'stats (mediana, percentyle, srednia)',
        retained_in: 'stats_all oraz stall_count / stall_rate_pct',
        applies_to_throughput: false,
        rationale: 'Zaciecia ~2,6 s sa skupione w czasie, dotykaja wszystkich '
          + 'workerow jednoczesnie i nie wypadaja przy nawiazywaniu polaczenia, '
          + 'co wskazuje na przejsciowe zdarzenia w sieci dostepowej klienta, '
          + 'niezalezne od protokolu.',
      },
      not_reproducible_in_browser: ['grpc_native', 'grpc_native_streaming'],
    },
    cells: {},
  };

  console.log('='.repeat(78));
  console.log('WYNIKI KONCOWE (wszystkie przebiegi razem)');
  console.log('='.repeat(78));

  for (const cell of cells) {
    console.log();
    console.log(cell.label);

    const cellOut = {
      scenario: cell.scenario,
      vu: cell.vu,
      // Czas namyslu rozni sie miedzy komorkami (budzet pasma), dlatego
      // throughput_rps NIE jest porownywalny miedzy komorkami — tylko miedzy
      // protokolami w obrebie jednej komorki.
      think_time_ms: cell.thinkMs,
      protocols: {},
    };
    if (cell.scenario === 'products') {
      cellOut.page_size = cell.pageSize;
      cellOut.cache = cell.cache;
    } else if (cell.scenario === 'orders') {
      cellOut.order_items = cell.items;
    } else if (cell.scenario === 'echo') {
      cellOut.echo_size = cell.echoSize;
    }

    for (const proto of PROTOCOLS) {
      const slot = acc[cell.key][proto.key];

      if (slot.allTimes.length === 0) {
        console.log(`   ${proto.label.padEnd(18)} BRAK DANYCH (bledow: ${slot.errors})`);
        cellOut.protocols[proto.key] = {
          label: proto.label, total_measurements: 0, errors: slot.errors,
          stats: null, per_run: [], inter_run: null,
        };
        continue;
      }

      // Ta sama polityka co w pojedynczym tescie: statystyki glowne bez zaciec,
      // pelne zachowane w stats_all, liczba odrzuconych jawnie raportowana.
      const split = splitStalls(slot.allTimes);
      const s = split.kept.length > 0 ? stats(split.kept) : stats(slot.allTimes);
      s.stats_all = stats(slot.allTimes);
      s.stall_count = split.stalls.length;
      s.stall_rate_pct = round((split.stalls.length / slot.allTimes.length) * 100);
      s.stall_threshold_ms = STALL_THRESHOLD_MS;

      const medians = slot.runs.map(r => r.med);
      const sdInfo = stdev(medians);
      const interRun = sdInfo
        ? { medians, sd: round(sdInfo.sd), cv: round((sdInfo.sd / sdInfo.avg) * 100) }
        : { medians, sd: null, cv: null };

      cellOut.protocols[proto.key] = {
        label: proto.label,
        total_measurements: s.count,
        errors: slot.errors,
        stats: s,
        per_run: slot.runs,
        inter_run: interRun,
      };

      console.log(
        `   ${proto.label.padEnd(18)} n=${String(s.count).padStart(5)} err=${String(slot.errors).padStart(3)}` +
        ` med=${String(s.med).padStart(7)}ms avg=${String(s.avg).padStart(7)}ms` +
        ` p95=${String(s.p95).padStart(7)}ms` +
        (interRun.cv !== null ? `  CV=${interRun.cv}%` : '') +
        (s.stall_count > 0 ? `  zaciec=${s.stall_count} (${s.stall_rate_pct}%)` : '')
      );
    }

    summary.cells[cell.key] = cellOut;
  }

  // === ZAPIS ===
  const outDir = path.resolve(__dirname, '..', '..', 'hetzner-results');
  const outFile = path.join(outDir, `browser-benchmark-${timestamp}.json`);
  fs.mkdirSync(outDir, { recursive: true });
  fs.writeFileSync(outFile, JSON.stringify(summary, null, 2));

  const totalMin = ((Date.now() - startedAt) / 60000).toFixed(1);
  console.log();
  console.log(`Czas calkowity: ${totalMin} min`);
  console.log(`Wynik zapisany: ${outFile}`);
}

main().catch(err => {
  console.error('Blad krytyczny:', err);
  process.exit(1);
});
