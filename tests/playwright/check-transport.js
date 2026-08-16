// =============================================================================
// Kontrola warstwy transportowej kazdej ze sciezek
// =============================================================================
// Sprawdza, jaka wersje HTTP przegladarka faktycznie negocjuje dla REST,
// gRPC-Web przez Envoy i gRPC-Web Direct, oraz ile polaczen TCP otwiera.
//
// Powod: REST i Envoy sa wystawione po zwyklym HTTP, a Direct po TLS, gdzie
// przegladarka moze negocjowac HTTP/2 przez ALPN. Jesli tak jest, czesc przewagi
// sciezki Direct pochodzi z wersji protokolu HTTP, a nie z formatu serializacji —
// i trzeba to jawnie zaznaczyc w pracy, a nie zakladac.
//
// Uzycie: node check-transport.js   (wymaga uruchomionego Angulara na :4200)
// =============================================================================

const { chromium } = require('playwright');

const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:4200';

const PROTOCOLS = [
  { key: 'rest', label: 'REST' },
  { key: 'grpc_web_envoy', label: 'gRPC-Web (Envoy)' },
  { key: 'grpc_web_direct', label: 'gRPC-Web (Direct)' },
];

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ ignoreHTTPSErrors: true });
  const page = await context.newPage();

  // CDP daje dostep do pola protocol w Network.responseReceived, ktorego
  // publiczne API Playwrighta nie udostepnia.
  const cdp = await context.newCDPSession(page);
  await cdp.send('Network.enable');

  const seen = [];
  cdp.on('Network.responseReceived', (e) => {
    const url = e.response.url;
    if (url.includes('167.233.253.101')) {
      seen.push({
        url,
        protocol: e.response.protocol,
        reused: e.response.connectionReused,
        connectionId: e.response.connectionId,
        remote: `${e.response.remoteIPAddress}:${e.response.remotePort}`,
      });
    }
  });

  await page.goto(`${FRONTEND_URL}/demo/products?pageSize=100&cold=0`, {
    waitUntil: 'networkidle',
    timeout: 60000,
  });

  console.log('='.repeat(78));
  console.log('Warstwa transportowa per sciezka');
  console.log('='.repeat(78));

  for (const proto of PROTOCOLS) {
    seen.length = 0;

    // 5 zadan, zeby zobaczyc czy polaczenie jest wznawiane
    for (let i = 0; i < 5; i++) {
      await page.evaluate((label) => {
        const btn = Array.from(document.querySelectorAll('button.protocol-btn'))
          .find(b => (b.textContent || '').trim() === label);
        btn?.click();
      }, proto.label);
      await sleep(600);
    }

    const calls = seen.filter(s => !s.url.endsWith('/'));
    if (calls.length === 0) {
      console.log(`\n${proto.label}: brak przechwyconych odpowiedzi`);
      continue;
    }

    const protocols = [...new Set(calls.map(c => c.protocol))];
    const connIds = [...new Set(calls.map(c => c.connectionId))];
    const reusedCount = calls.filter(c => c.reused).length;

    console.log(`\n${proto.label}`);
    console.log(`  adres:            ${calls[0].remote}`);
    console.log(`  wersja HTTP:      ${protocols.join(', ')}`);
    console.log(`  odpowiedzi:       ${calls.length}`);
    console.log(`  polaczen TCP:     ${connIds.length}`);
    console.log(`  wznowien:         ${reusedCount}/${calls.length}`);
  }

  console.log();
  console.log('='.repeat(78));
  console.log('Interpretacja: jesli Direct raportuje h2, a REST i Envoy http/1.1,');
  console.log('to roznica warstwy transportowej jest realnym czynnikiem zakłócającym');
  console.log('i musi byc zaznaczona jako ograniczenie badania.');

  await context.close();
  await browser.close();
}

main().catch(err => { console.error('Blad:', err); process.exit(1); });
