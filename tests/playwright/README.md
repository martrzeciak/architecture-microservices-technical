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
| `-OrderItems` | `1,5,10` | `ORDER_ITEMS` |
| `-VUList` | `10,50` | `VU` |
| `-Iter` | 20 | liczba żądań na VU w komórce |
| `-Runs` | 5 | powtórzenia całej macierzy |
| `-Cooldown` | 15 | sekund przerwy między przebiegami |

Zmienna `MAX_VU_ROWS` (domyślnie 25 000) odfiltrowuje kombinacje, w których
iloczyn VU × liczba rekordów na żądanie jest tak duży, że wąskim gardłem
przestaje być protokół, a staje się baza (Products) albo procesor serwera (Echo).
Pominięte komórki są wypisywane na starcie i zapisywane w `config.skipped_cells`.

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
| Pozycje zamówienia | 1 / 5 / 10 | odtworzone |
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

## Trzy reżimy pomiarowe

Różnica między protokołami nie skaluje się liniowo z rozmiarem odpowiedzi.
Po odjęciu czasu bazowego i podzieleniu przez RTT (25,9 ms) nadwyżki okazują się
całkowitymi wielokrotnościami RTT:

| Rekordów (echo) | REST — nadwyżka | Direct — nadwyżka |
|---|---|---|
| 500 | +26,9 ms = 1,0 × RTT | +2,2 ms = 0 × RTT |
| 1000 | +55,1 ms = 2,1 × RTT | +30,3 ms = 1,2 × RTT |
| 2000 | +135,5 ms = 5,2 × RTT | +83,0 ms = 3,2 × RTT |
| 5000 | +372,6 ms = 14,3 × RTT | +237,7 ms = 9,1 × RTT |

To nie efekt przepustowości: 54 KB różnicy przy ~200 Mbit/s to około 2 ms, nie 27.
Mechanizm to okno przeciążenia TCP — większy ładunek JSON przekracza kolejne progi
slow startu i wymaga dodatkowych podróży tam i z powrotem, mniejszy protobuf ich
nie przekracza. Stąd trzy reżimy:

- **do ~200 rekordów** — dominuje RTT (ok. 84% pomiaru), mierzony jest stały
  narzut na żądanie: obsługa połączenia, ramkowanie, nagłówki, przeskok w proxy.
  Efekt rzędu 2 ms (6%), wymaga ≥300 próbek na komórkę. To jedyny reżim, w którym
  ścieżka przez Envoy przegrywa z REST.
- **500–1000 rekordów** — reżim skokowy, różnica rośnie skokami o całe RTT,
  w pomiarach do −45%.
- **od 2000 rekordów** — różnica proporcjonalna do bajtów, zbiega do stosunku
  rozmiarów (~33%). Wystarcza ~100 próbek.

Wniosek: przewaga gRPC-Web zależy głównie od **opóźnienia sieci**, nie od szybkości
serwera. W sieci lokalnej niemal zanika, przez internet jest znacząca — co
uzasadnia pomiar przez rzeczywiste łącze zamiast lokalnie.

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
- **Wartości średnie bywają skażone pojedynczymi zdarzeniami.** W pomiarach
  kontrolnych zdarzały się pojedyncze zacięcia (`avg` 165 ms przy medianie 32 ms).
  Do wnioskowania należy używać mediany i percentyli, nie średniej.
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
