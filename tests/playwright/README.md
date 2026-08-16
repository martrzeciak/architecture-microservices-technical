# Browser Benchmark — REST vs gRPC-Web (Envoy) vs gRPC-Web (Direct)

Pomiar wydajności protokołów z poziomu **prawdziwej przeglądarki**, w warunkach
sieciowych (klient lokalny → serwer Hetzner w Norymberdze). Zastępuje pomiar k6,
który zawyżał czasy gRPC z powodu
[błędu w implementacji klienta gRPC](https://github.com/grafana/k6/issues/1846).

Odtwarza macierz badawczą scenariuszy k6 — w zakresie, w jakim da się ją
odtworzyć w przeglądarce.

## Uruchomienie

```powershell
cd tests\playwright
.\run-browser-benchmark.ps1
```

Skrypt sam: sprawdza dostępność backendu, instaluje Playwright przy pierwszym
uruchomieniu, startuje Angulara, czeka aż wstanie, przechodzi całą macierz,
zapisuje wynik i zatrzymuje Angulara (także po Ctrl+C). W trakcie wypisuje
postęp i szacowany czas do końca.

### Szybki test poprawności (~1 min)

```powershell
.\run-browser-benchmark.ps1 -Quick
```

### Własny zakres

```powershell
.\run-browser-benchmark.ps1 -VUList "10,50" -PageSizes "10,200" -CacheStates "warm,cold" -OrderItems "1,10"
```

| Parametr | Domyślnie | Odpowiednik w k6 |
|---|---|---|
| `-EchoSizes` | `10,100,200,500,2000,5000` | `count` w scenariuszach echo |
| `-PageSizes` | `10,100,200,500,1000,2000` | `PAGE_SIZE` |
| `-CacheStates` | `warm,cold` | `BYPASS_CACHE` |
| `-OrderItems` | `1,10,50,200` | `ORDER_ITEMS` |
| `-VUList` | `10,50` | `VU` |
| `-LinkMbps` | 100 | pasmo łącza klienta w dół |
| `-UplinkMbps` | 20 | pasmo łącza klienta w górę (scenariusz zapisu) |
| `-Iter` | 20 | liczba żądań na VU w komórce |
| `-Runs` | 5 | powtórzenia całej macierzy |
| `-Cooldown` | 15 | sekund przerwy między przebiegami |

Dwa filtry odfiltrowują kombinacje, w których wąskim gardłem przestaje być
protokół. `MAX_VU_ROWS` (domyślnie 25 000) dotyczy odczytu — iloczyn VU × liczba
rekordów na żądanie; powyżej progu decyduje baza (Products) albo procesor serwera
(Echo). `MAX_VU_ITEMS` (domyślnie 5 000) dotyczy zapisu i jest znacznie niższy,
bo każda pozycja zamówienia to trwały wiersz w bazie: bez tego 200 pozycji przy
50 VU wygenerowałoby ponad 3 mln wierszy w `OrderItems`, rozdymając tabelę
w trakcie pomiaru. Pominięte komórki są wypisywane na starcie i zapisywane
w `config.skipped_cells`.

Tryb planowania — wypisuje macierz, liczbę próbek i liczbę zapisów, bez
uruchamiania Angulara i bez dotykania backendu:

```powershell
.\run-browser-benchmark.ps1 -DryRun
```

Skrypt pokazuje bieżący postęp i ETA. Liczba komórek zależy od parametrów
i filtra `MAX_VU_ROWS`; przy domyślnych wartościach jest to kilkadziesiąt komórek
i kilka godzin pomiaru.

Rozmiary powyżej 200 rekordów wykraczają poza macierz k6 (10/100/200) i zostały
dodane świadomie: przełom w różnicy między protokołami wypada dopiero powyżej
200 rekordów — patrz sekcja o reżimach pomiarowych. Baza zasiewowa ma 2000
rekordów (`SEED_PRODUCT_COUNT`), co wyznacza sufit dla `pageSize`.

Pusta lista wyłącza scenariusz — np. sam pomiar narzutu protokołu:

```powershell
.\run-browser-benchmark.ps1 -PageSizes "" -CacheStates "" -OrderItems ""
```

## Macierz badawcza

| Wymiar | Wartości | Status |
|---|---|---|
| Rozmiar odpowiedzi echo | 10 / 100 / 200 / 500 / 2000 / 5000 | odtworzone i rozszerzone |
| Rozmiar strony | 10 / 100 / 200 / 500 / 1000 / 2000 | odtworzone i rozszerzone |
| Stan cache | ciepły / zimny (`X-Bypass-Cache`) | odtworzone |
| Pozycje zamówienia | 1 / 10 / 50 / 200 | odtworzone i rozszerzone |
| Protokoły | REST, gRPC-Web/Envoy, gRPC-Web/Direct | odtworzone |
| Liczba użytkowników | 10 / 50 | częściowo — patrz niżej |
| Natywne gRPC | — | **nieodtwarzalne w przeglądarce** |
| Strumieniowanie serwerowe | — | **nieodtwarzalne w przeglądarce** |

Natywne gRPC wymaga kontroli nad trailerami HTTP/2, czego przeglądarkowe API
`fetch`/`XHR` nie udostępnia — to jest właśnie powód istnienia gRPC-Web. Te dwa
wymiary pozostają w pomiarach k6 jako punkt odniesienia.

Poziomy 100 i 500 VU z k6 nie mieszczą się na jednej stacji roboczej: każdy
kontekst Chromium to kilkadziesiąt MB pamięci i własny wątek renderujący, więc
powyżej ~50 kontekstów opóźnienie pochodziłoby z klienta, nie z serwera.

Scenariusze steruje się parametrami URL, więc każdą konfigurację można też
otworzyć ręcznie w przeglądarce:

```
/demo/echo?count=200
/demo/products?pageSize=200&cold=1
/demo/orders?items=10
```

## Co jest mierzone

| Scenariusz | Strona | Operacja |
|---|---|---|
| **Echo** (narzut protokołu) | `/demo/echo` | dane wbudowane w kod, bez bazy i cache |
| **Products** (odczyt) | `/demo/products` | pobranie listy produktów |
| **Orders** (zapis) | `/demo/orders` | złożenie zamówienia (Saga + Outbox) |

Scenariusz echo celowo **nie renderuje** pozycji w tabeli, tylko podsumowanie
liczbowe. Wyrysowanie 200 wierszy kosztuje tyle samo dla każdego protokołu, więc
dodawałoby stały składnik rozcieńczający mierzoną różnicę. Pozostałe scenariusze
renderują normalnie, bo tam celem jest realistyczna ścieżka użytkownika.

Ładunek zamówienia jest losowany z tej samej puli produktów zasiewowych co
`buildOrderItems()` w `tests/k6/config.js`, więc rozkład i rozmiar żądania się
zgadzają.

Domyślne wartości po stronie serwera są identyczne na obu ścieżkach: REST ma
`int pageSize = 10`, a gRPC `request.PageSize > 0 ? request.PageSize : 10` — więc
porównanie protokołów dotyczy tej samej liczby zwracanych rekordów.

## Metodyka

**Rozgrzewka.** 20 żądań na protokół w każdym scenariuszu przed pomiarem, aby JIT
.NET i pule połączeń były w stanie ustalonym.

**Zapytania przygotowawcze.** 3 nierejestrowane żądania na worker w każdej
komórce — wypełniają cache dla danego rozmiaru strony, żeby scenariusz „ciepły"
nie zaczynał się od chybienia.

**Rotacja kolejności (kwadrat łaciński).** Kolejność protokołów zmienia się
między przebiegami, co eliminuje *order effect* — uprzywilejowanie protokołu
mierzonego jako ostatni, na „rozgrzanym" systemie.

**Przerwa między przebiegami.** 15 s, aby GC i pule połączeń nie przenosiły stanu.

**Pomiar czasu.** Wykonywany **wewnątrz przeglądarki**, nie z Node.js. Komponent
zwiększa atrybut `data-load-id` / `data-submit-id` w chwili naniesienia odpowiedzi
na DOM; skrypt wychwytuje to przez `MutationObserver` i mierzy różnicę
`performance.now()` od kliknięcia.

Pierwotna wersja odpytywała DOM z Node.js przez CDP. Okazało się to
niewystarczające: każde odpytanie to jedna podróż tam i z powrotem, więc przy RTT
~30 ms stan przejściowy bywał przegapiony, a próbka dostawała stałą karę równą
limitowi czasu. W pomiarze kontrolnym **wszystkie** próbki gRPC-Web/Envoy wyszły
~3010 ms przy rzeczywistym czasie ~34 ms. Obecna metoda daje zmienność między
przebiegami na poziomie CV < 4%.

## Budżet pasma łącza klienta

Zapotrzebowanie komórki na pasmo to VU × rozmiar ładunku ÷ długość cyklu. Przy
stałym czasie namysłu 100 ms najcięższe komórki żądałyby około **230 Mbit/s** —
więcej, niż ma typowe łącze domowe.

Nasycenie łącza byłoby tu groźniejsze niż zwykły szum, bo opóźnienie rosłoby
**proporcjonalnie do rozmiaru ładunku**. REST przesyła o ~43% więcej bajtów, więc
ucierpiałby bardziej niż protobuf, a zmierzona „przewaga protokołu" byłaby
częściowo odbiciem przepustowości łącza klienta — w kierunku **zawyżającym** wynik
gRPC.

Ponieważ mierzoną wielkością jest opóźnienie pojedynczego żądania, a nie
przepustowość, rozwiązaniem nie jest usuwanie komórek, lecz **adaptacyjny czas
namysłu**: skrypt wydłuża przerwę między żądaniami tak, aby zapotrzebowanie
zmieściło się w połowie zadeklarowanego pasma. Każde żądanie mierzy się
identycznie, tylko rzadziej. Przy domyślnych 100 Mbit/s czas namysłu waha się od
100 ms do 724 ms, a szczytowe zapotrzebowanie żadnej komórki nie przekracza
50 Mbit/s.

Rozmiary ładunków wyznaczono pomiarem odpowiedzi na wdrożonym backendzie:
133 B na rekord dla Products i 181 B dla Echo.

> **Konsekwencja przy interpretacji:** `throughput_rps` nie jest porównywalny
> **między komórkami**, bo mają różny czas namysłu. Porównywalny pozostaje między
> protokołami w obrębie jednej komórki. Użyty czas namysłu jest zapisany w
> `cells[].think_time_ms`. Opóźnienia (mediana, percentyle) są tym niezależne.

Wartość nominalną łącza warto zastąpić zmierzoną — do pracy też lepiej wpisać
pomiar niż deklarację operatora:

```powershell
.\measure-link.ps1                  # RTT + pobieranie (bez skutków ubocznych)
.\measure-link.ps1 -IncludeUplink   # dodatkowo wysyłanie (tworzy 3 zamówienia)
```

Skrypt mierzy pojedynczy strumień oraz agregat z 4, 8 i 16 strumieni, bo jedno
połączenie bywa ograniczone oknem TCP, a nie pojemnością łącza — dopiero
równoległość pokazuje sufit. Na końcu wypisuje gotowe polecenie z zalecanymi
wartościami (90% zmierzonego agregatu).

Jeśli znasz swoje łącze, podaj je wprost: `-LinkMbps 300 -UplinkMbps 50`. Warto
podać zwłaszcza realny upload, bo scenariusz Orders obciąża kierunek w górę,
który na łączach asymetrycznych jest znacznie słabszy.

## Zmierzona charakterystyka łącza

Pomiar `measure-link.ps1` z komputera klienckiego do serwera w Norymberdze:

Pomiar powtórzony trzykrotnie; rozrzut poniżej 5%.

| Wielkość | Wartość |
|---|---|
| RTT (ICMP, 10 prób) | 24–30 ms, średnio **26,2–26,9 ms** |
| Pobieranie, 1 strumień | **19,6–20,8 Mbit/s** |
| Pobieranie, 4 strumienie | 74–79 Mbit/s |
| Pobieranie, 8 strumieni | 143–146 Mbit/s |
| Pobieranie, 16 strumieni | **269–273 Mbit/s** (bez osiągnięcia sufitu) |
| Wysyłanie, 1 strumień | **19,2 Mbit/s** |

Kluczowa obserwacja: pojedyncze połączenie jest ograniczone do ~20 Mbit/s, choć
agregat skaluje się niemal liniowo. Iloczyn opóźnienia i przepustowości wynosi
20 Mbit/s × 0,027 s = **67,5 KB**, co odpowiada oknu odbiorczemu TCP o rozmiarze
64 KB: 65 536 B × 8 ÷ 0,027 s = 19,4 Mbit/s. Ograniczeniem pojedynczego żądania
nie jest więc pojemność łącza, lecz rozmiar okna.

Wysyłanie jednym połączeniem daje niemal identyczny wynik co pobieranie (19,2 vs
20,8 Mbit/s), co potwierdza, że okno ogranicza symetrycznie oba kierunki — jest to
niezależne potwierdzenie mechanizmu opisanego niżej.

> Pomiar wysyłania wymagał obejścia. Pierwsza wersja wysyłała poprawne zamówienia
> i dała bezużyteczne 0,7 Mbit/s: jedno zamówienie z 2000 pozycjami zajmowało
> ~1,76 s, z czego niemal całość to zapis do bazy, saga i outbox, a nie transmisja
> 154 KB. Obecna wersja wysyła celowo niedomknięty JSON — serwer musi wczytać całe
> ciało żądania, żeby dojść do końca strumienia, i dopiero wtedy zwraca 400, więc
> mierzymy transfer bez zapisu do bazy.

Wartości pochodzące z pojedynczego strumienia (w tym wysyłanie) są **dolnym
ograniczeniem** pojemności łącza, nie jej miarą — pokazuje to różnica 20 vs 273
Mbit/s w pobieraniu. Dla wysyłania nie mierzono agregatu, dlatego zalecane
`-UplinkMbps 17` jest zachowawcze.

## Mechanizm przewagi protokołu

Okno 64 KB wyjaśnia obserwowane różnice ilościowo. Transfer odpowiedzi wymaga
`ceil(rozmiar / 64 KB)` okien, a każde okno kosztuje jedno RTT. Przewaga protobuf
sprowadza się do liczby zaoszczędzonych okien:

| Rekordów (echo) | REST: okien / zmierzone RTT | Direct: okien / zmierzone RTT |
|---|---|---|
| 500 | 1,38 / 1,0 | 0,92 / 0,1 |
| 1000 | 2,76 / 2,1 | 1,85 / 1,2 |
| 2000 | 5,52 / 5,2 | 3,69 / 3,2 |
| 5000 | 13,80 / 14,3 | 9,23 / 9,1 |

Zgodność jest bardzo dobra, zwłaszcza przy większych ładunkach. To także tłumaczy,
dlaczego poniżej ~200 rekordów przewaga wynosiła zaledwie ~2 ms i nie zależała od
rozmiaru: takie odpowiedzi mieszczą się w jednym oknie, więc liczba RTT jest
identyczna, a pozostała różnica pochodzi z narzutu ramkowania i warstwy transportowej.

Wniosek metodologiczny: **przewaga gRPC-Web rośnie z opóźnieniem sieci**, bo każde
zaoszczędzone okno to jedno RTT mniej. W sieci lokalnej (RTT < 1 ms) niemal zanika,
przez internet jest znacząca — co uzasadnia pomiar przez rzeczywiste łącze zamiast
lokalnie.

## Zacięcia sieciowe i polityka ich odrzucania

W pomiarach pojawiają się sporadyczne próbki rzędu **2,6 s** przy medianie ~33 ms.
Diagnostyka wskazuje, że nie są własnością protokołu:

- występują **skupiskami** — w jednym teście 8 zacięć, w ośmiu pozostałych zero;
- dotykają **wszystkich workerów jednocześnie** (8 zacięć wśród 10 workerów),
  czyli jedno zdarzenie zamraża wszystkie żądania w locie;
- **nie wypadają przy nawiązywaniu połączenia** — licznik `iter<=1` wynosił 0,
  więc nie chodzi o koszt zestawienia TCP ani TLS;
- trafiają ten protokół, który akurat jest mierzony, więc w kolejnych przebiegach
  wypadają w różnych miejscach.

Wskazuje to na przejściowe zdarzenia w sieci dostępowej klienta. Próbki powyżej
**1000 ms** są więc odrzucane ze statystyk głównych, ale odrzucenie jest jawne
i audytowalne:

| Pole w wyniku | Zawartość |
|---|---|
| `stats` | mediana, percentyle, średnia — **bez zacięć** |
| `stats.stats_all` | te same statystyki **ze wszystkimi** próbkami |
| `stats.stall_count`, `stats.stall_rate_pct` | liczba i odsetek odrzuconych |
| `stats.stall_threshold_ms` | użyty próg (1000 ms) |
| `config.stall_policy` | próg, zakres stosowania i uzasadnienie |
| `throughput_rps` | liczone ze **wszystkich** próbek — polityka go nie dotyczy |

Dwie uwagi do interpretacji. Percentyle `p95` i `p99` po odrzuceniu opisują
rozkład **warunkowo na brak zacięcia**, co należy zaznaczyć przy raportowaniu.
Odsetek zacięć sam jest wynikiem obserwacyjnym — warto go podać, bo mówi coś
o warunkach pomiaru.

## Trzy reżimy pomiarowe

Stąd trzy reżimy zachowania:

- **do ~200 rekordów** (jedno okno) — dominuje RTT, ok. 84% pomiaru. Mierzony jest
  stały narzut na żądanie: obsługa połączenia, ramkowanie, nagłówki, przeskok
  w proxy. Efekt rzędu 2 ms (6%), wymaga ≥300 próbek na komórkę. To jedyny reżim,
  w którym ścieżka przez Envoy przegrywa z REST.
- **500–1000 rekordów** (2–3 okna) — reżim skokowy, różnica rośnie skokami o całe
  RTT, w pomiarach do −45%.
- **od 2000 rekordów** (wiele okien) — liczba okien jest proporcjonalna do bajtów,
  więc różnica zbiega do stosunku rozmiarów (~33%). Wystarcza ~100 próbek.

### Ścieżka zapisu zachowuje się inaczej

W scenariuszu Orders rozmiar ładunku **nie** jest dźwignią. W pomiarze kontrolnym
(VU=3, n=30) przejście z 10 na 50 pozycji nie zmieniło niczego — wszystkie trzy
protokoły siedziały na plateau 92–97 ms, mimo pięciokrotnie większego żądania.
Dominuje stały koszt serwerowy: transakcja, wpis do outboxa, saga i commit, czyli
około 60–70 ms ponad RTT. Wymiar liczby pozycji dokumentuje więc wpływ rozmiaru
żądania na ścieżkę zapisu, ale nie izoluje narzutu protokołu — do tego służy
scenariusz Echo, wolny od bazy i cache.

## Ograniczenia (do zaznaczenia w pracy)

- **Inna wielkość mierzona niż w k6.** k6 raportował `http_req_duration` (sieć +
  serwer). Tutaj mierzony jest czas od kliknięcia do naniesienia danych na DOM,
  więc obejmuje dodatkowo deserializację w JS (protobuf albo JSON), change
  detection Angulara i renderowanie. To nadzbiór — porównywalna jest relacja
  między protokołami, nie wartości bezwzględne względem dawnych liczb z k6.
- **Przepustowość nie jest przepustowością nasycenia.** Między iteracjami jest
  100 ms przerwy (model *closed-loop* z czasem namysłu), więc `throughput_rps`
  mierzy przepustowość przy zadanym profilu obciążenia, nie maksimum serwera.
- **Klient dzieli zasoby.** Wszystkie konteksty Chromium działają na jednej
  maszynie; przy 50 VU część opóźnienia może pochodzić z klienta.
- **Certyfikat self-signed** na ścieżce Direct wymaga `ignoreHTTPSErrors`; narzut
  nawiązania TLS jest widoczny w ogonie rozkładu (p95).
- **Różna warstwa transportowa między protokołami.** REST i ścieżka przez Envoy
  idą po zwykłym HTTP (`:5000`, `:8080`), a ścieżka Direct po TLS (`:5002`), gdzie
  przeglądarka negocjuje HTTP/2 przez ALPN. Część przewagi ścieżki Direct może
  więc pochodzić z wersji protokołu HTTP, nie z samego formatu serializacji.
- **Zacięcia sieciowe są odrzucane z agregacji** — patrz osobna sekcja niżej.
- **Resztkowa asymetria cache.** `ProductGrpcService` trzyma w Redisie bajty
  protobuf, więc trafienie w cache to `ParseFrom` zamiast deserializacji JSON.
  Nadal nie jest to pełne zrównanie: REST zwraca gotowy string bez żadnej
  serializacji (zero pracy), a gRPC wykonuje `ParseFrom` plus serializację
  frameworka. Pełne zrównanie wymagałoby zwracania surowych bajtów z pominięciem
  serializatora `Grpc.AspNetCore`, czego framework nie udostępnia wprost.
  Scenariusz Echo jest od tego wolny — nie dotyka cache.
- **Renderowanie ograniczone do 100 wierszy.** `product-list` deserializuje całą
  odpowiedź (suma cen liczona po wszystkich rekordach), ale rysuje tylko pierwsze
  100 pozycji. Bez tego przy 2000 rekordach koszt rysowania tabeli — identyczny
  dla wszystkich protokołów — sięgałby setek milisekund i zalewał mierzoną różnicę.
  W zamian pomiar nie obejmuje pełnego kosztu renderowania dużych list.

## Wynik

JSON w `hetzner-results/browser-benchmark-<timestamp>.json`:

```
config                     — pełna macierz i parametry metodyki
cells
  └── echo_VU10_N200
        ├── scenario, vu, echo_size
        └── protocols.<...>
  └── products_VU10_PS200_CACHECOLD     (klucz jak w nazwach plików k6)
        ├── scenario, vu, page_size, cache
        └── protocols.<rest|grpc_web_envoy|grpc_web_direct>
              ├── total_measurements, errors
              ├── stats     — min / avg / med / p90 / p95 / p99 / max
              ├── per_run[] — te same statystyki osobno dla każdego przebiegu
              └── inter_run — mediany przebiegów, SD, CV
  └── orders_VU10_IT10
        ├── scenario, vu, order_items
        └── protocols.<...>
```

## Wymagania

- Node.js (sprawdzone na v22)
- Uruchomiony stack na serwerze (porty 5000, 5003, 8080, 5002, 5005)
- Wolny port 4200 lokalnie — skrypt zwalnia go sam przed startem
