# =============================================================================
# Pomiar przepustowosci lacza klienta do backendu
# =============================================================================
# Wynik sluzy do ustawienia -LinkMbps / -UplinkMbps w run-browser-benchmark.ps1.
# Jest to istotne metodologicznie: jesli lacze klienta sie nasyci, opoznienie
# rosnie proporcjonalnie do rozmiaru ladunku, a REST przesyla o ~43% wiecej
# bajtow niz protobuf. Zmierzona "przewaga protokolu" bylaby wtedy czesciowo
# odbiciem przepustowosci lacza, w kierunku zawyzajacym wynik gRPC.
#
# Uzycie:
#   .\measure-link.ps1                  # tylko pobieranie (bez skutkow ubocznych)
#   .\measure-link.ps1 -IncludeUplink   # dodatkowo wysylanie (tworzy zamowienia!)
# =============================================================================
param(
    [string]$BackendHost = "167.233.253.101",
    [switch]$IncludeUplink
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http

$ECHO_BYTES_PER_RECORD = 181   # zmierzone: 36 242 B / 200 rekordow

function New-Client {
    $c = [System.Net.Http.HttpClient]::new()
    $c.Timeout = [TimeSpan]::FromMinutes(5)
    return $c
}

# Pobiera odpowiedz strumieniowo i zwraca liczbe bajtow — bez budowania stringa,
# zeby nie mierzyc narzutu parsowania.
function Get-ByteCount($client, [string]$url) {
    $resp = $client.GetAsync($url, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
    try {
        $stream = $resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $buf = New-Object byte[] 131072
        $total = 0
        while (($read = $stream.Read($buf, 0, $buf.Length)) -gt 0) { $total += $read }
        $stream.Dispose()
        return $total
    } finally { $resp.Dispose() }
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Pomiar lacza: $BackendHost" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$client = New-Client

# --- RTT ---
$ping = Test-Connection -TargetName $BackendHost -Count 10 -ErrorAction SilentlyContinue
if ($ping) {
    $lat = $ping | ForEach-Object { $_.Latency }
    $rttAvg = [math]::Round((($lat | Measure-Object -Average).Average), 1)
    Write-Host ("RTT (ICMP):  min {0} ms   sr. {1} ms   max {2} ms" -f `
        ($lat | Measure-Object -Minimum).Minimum, $rttAvg, ($lat | Measure-Object -Maximum).Maximum)
} else {
    Write-Host "RTT: ICMP zablokowany"
}
Write-Host ""

Write-Host "rozgrzewka (ustanowienie polaczenia)..." -NoNewline
$null = Get-ByteCount $client "http://${BackendHost}:5000/api/echo?count=1000"
Write-Host " ok"
Write-Host ""

# --- DOWNLINK: pojedynczy strumien ---
Write-Host "=== POBIERANIE: pojedynczy strumien ===" -ForegroundColor Yellow
$singleBest = 0
foreach ($n in @(30000, 100000)) {
    $url = "http://${BackendHost}:5000/api/echo?count=$n"
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $bytes = Get-ByteCount $client $url
    $sw.Stop()
    $mbps = ($bytes * 8 / 1e6) / $sw.Elapsed.TotalSeconds
    if ($mbps -gt $singleBest) { $singleBest = $mbps }
    Write-Host ("  {0,6} rek.  {1,7:N2} MB  {2,7:N0} ms  ->  {3,6:N1} Mbit/s" -f `
        $n, ($bytes / 1MB), $sw.Elapsed.TotalMilliseconds, $mbps)
}
Write-Host ""

# --- DOWNLINK: rownolegle strumienie ---
# Pojedyncze polaczenie bywa ograniczone oknem TCP, nie pojemnoscia lacza.
# Dopiero rownoleglosc pokazuje sufit, a benchmark uzywa 10-50 kontekstow.
Write-Host "=== POBIERANIE: rownolegle strumienie ===" -ForegroundColor Yellow
$aggBest = 0
foreach ($streams in @(4, 8, 16)) {
    $url = "http://${BackendHost}:5000/api/echo?count=50000"
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $results = 1..$streams | ForEach-Object -ThrottleLimit $streams -Parallel {
        Add-Type -AssemblyName System.Net.Http
        $c = [System.Net.Http.HttpClient]::new()
        $c.Timeout = [TimeSpan]::FromMinutes(5)
        $resp = $c.GetAsync($using:url, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
        $stream = $resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult()
        $buf = New-Object byte[] 131072
        $total = 0
        while (($read = $stream.Read($buf, 0, $buf.Length)) -gt 0) { $total += $read }
        $stream.Dispose(); $resp.Dispose(); $c.Dispose()
        return $total
    }
    $sw.Stop()
    $totalBytes = ($results | Measure-Object -Sum).Sum
    $mbps = ($totalBytes * 8 / 1e6) / $sw.Elapsed.TotalSeconds
    if ($mbps -gt $aggBest) { $aggBest = $mbps }
    Write-Host ("  {0,2} strumieni  {1,7:N2} MB  {2,7:N0} ms  ->  {3,6:N1} Mbit/s" -f `
        $streams, ($totalBytes / 1MB), $sw.Elapsed.TotalMilliseconds, $mbps)
}
Write-Host ""

# --- UPLINK (opcjonalnie) ---
$uplinkMbps = $null
if ($IncludeUplink) {
    Write-Host "=== WYSYLANIE ===" -ForegroundColor Yellow
    # Celowo niedomkniety JSON: System.Text.Json musi wczytac cale cialo zadania,
    # zeby dojsc do konca strumienia i dopiero wtedy zglasza blad. Uzyskujemy
    # transfer w gore bez zapisu do bazy.
    #
    # Pierwsza wersja tego pomiaru wysylala poprawne zamowienia i byla bezuzyteczna:
    # jedno zamowienie z 2000 pozycjami zajmowalo ~1,76 s, z czego niemal calosc to
    # zapis do bazy, saga i outbox — transmisja 154 KB to przy 10 Mbit/s ~0,12 s.
    $item = '{"productId":"1","productName":"Product 1","quantity":1,"unitPrice":9.99},'
    $sb = [System.Text.StringBuilder]::new()
    $null = $sb.Append('{"customerId":"link-measure","items":[')
    for ($i = 0; $i -lt 40000; $i++) { $null = $sb.Append($item) }
    # brak zamykajacego "]}" — parser dojdzie do konca strumienia i zwroci 400
    $json = $sb.ToString()
    $bodyBytes = [System.Text.Encoding]::UTF8.GetByteCount($json)

    $reps = 3
    $totalSent = 0
    $statuses = @()
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    for ($i = 0; $i -lt $reps; $i++) {
        $content = [System.Net.Http.StringContent]::new($json, [System.Text.Encoding]::UTF8, "application/json")
        $resp = $client.PostAsync("http://${BackendHost}:5003/api/orders", $content).GetAwaiter().GetResult()
        $null = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $statuses += [int]$resp.StatusCode
        $resp.Dispose()
        $totalSent += $bodyBytes
    }
    $sw.Stop()
    $uplinkMbps = ($totalSent * 8 / 1e6) / $sw.Elapsed.TotalSeconds
    Write-Host ("  {0} zadania x {1:N2} MB  {2,7:N0} ms  ->  {3,6:N1} Mbit/s" -f `
        $reps, ($bodyBytes / 1MB), $sw.Elapsed.TotalMilliseconds, $uplinkMbps)
    Write-Host ("  Kody odpowiedzi: {0} (oczekiwane 400 — zadanie odrzucone, nic nie zapisano)" -f ($statuses -join ', ')) -ForegroundColor DarkGray
    if ($statuses | Where-Object { $_ -ne 400 }) {
        Write-Host "  UWAGA: nieoczekiwany kod odpowiedzi — sprawdz, czy nic nie zostalo zapisane." -ForegroundColor Yellow
    }
    Write-Host ""
}

$client.Dispose()

# --- REKOMENDACJA ---
$recommend = [math]::Floor([math]::Max($singleBest, $aggBest) * 0.9)
Write-Host "============================================================" -ForegroundColor Green
Write-Host " WYNIK" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ("  Pojedynczy strumien:  {0,6:N1} Mbit/s" -f $singleBest)
Write-Host ("  Agregat rownolegly:   {0,6:N1} Mbit/s" -f $aggBest)
if ($uplinkMbps) { Write-Host ("  Wysylanie (dolna gr.):{0,6:N1} Mbit/s" -f $uplinkMbps) }
Write-Host ""
Write-Host "  Do uzycia w benchmarku (90% zmierzonego agregatu):" -ForegroundColor Green
if ($uplinkMbps) {
    $upRec = [math]::Max(5, [math]::Floor($uplinkMbps * 0.9))
    Write-Host ("    .\run-browser-benchmark.ps1 -LinkMbps {0} -UplinkMbps {1}" -f $recommend, $upRec) -ForegroundColor White
} else {
    Write-Host ("    .\run-browser-benchmark.ps1 -LinkMbps {0}" -f $recommend) -ForegroundColor White
    Write-Host "    (upload niezmierzony — uruchom z -IncludeUplink, jesli chcesz)" -ForegroundColor DarkGray
}
Write-Host ""
Write-Host "  Sprawdz plan przed pomiarem:" -ForegroundColor Green
Write-Host ("    .\run-browser-benchmark.ps1 -LinkMbps {0} -DryRun" -f $recommend) -ForegroundColor White
