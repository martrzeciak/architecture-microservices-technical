# =============================================================================
# Browser Benchmark — one-command runner
# Odpala Angular + Playwright benchmark, wynik zapisuje do hetzner-results/
#
# Uzycie:
#   .\run-browser-benchmark.ps1              # pelny pomiar (~60-90 min)
#   .\run-browser-benchmark.ps1 -Quick       # smoke test (~2 min)
#   .\run-browser-benchmark.ps1 -VU 10 -Iter 20 -Runs 3
# =============================================================================
param(
    [string]$VUList = "10,50",
    [string]$PageSizes = "10,100,200,500,1000,2000",
    [string]$CacheStates = "warm,cold",
    [string]$OrderItems = "1,5,10",
    [string]$EchoSizes = "10,100,200,500,2000,5000",
    [int]$Iter = 20,
    [int]$Runs = 5,
    [int]$Cooldown = 15,
    [switch]$Quick,
    [switch]$SkipPreflight
)

$ErrorActionPreference = "Stop"

if ($Quick) {
    $VUList = "2"; $PageSizes = "10"; $CacheStates = "warm"; $OrderItems = "1"
    $EchoSizes = "200"
    $Iter = 3; $Runs = 1; $Cooldown = 2
}

$ROOT = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ANGULAR_DIR = Join-Path $ROOT "frontend\angular-app"
$PLAYWRIGHT_DIR = $PSScriptRoot
$LOG_DIR = Join-Path $ROOT ".logs"
$BACKEND_HOST = "167.233.253.101"

New-Item -ItemType Directory -Path $LOG_DIR -Force | Out-Null

# Komorki = VU x echoSizes  +  VU x (pageSize x cache)  +  VU x orderItems
# Pusta lista wylacza dany scenariusz, np. -PageSizes "" -OrderItems ""
function Count-List([string]$s) {
    return @($s -split ',' | Where-Object { $_.Trim() -ne '' }).Count
}
$vuCount = Count-List $VUList
$cellCount = $vuCount * (Count-List $EchoSizes) `
           + $vuCount * (Count-List $PageSizes) * (Count-List $CacheStates) `
           + $vuCount * (Count-List $OrderItems)
$testCount = $cellCount * 3 * $Runs

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Browser Benchmark: REST vs gRPC-Web (Envoy) vs Direct" -ForegroundColor Cyan
Write-Host " VU:          $VUList" -ForegroundColor Cyan
Write-Host " EchoSizes:   $EchoSizes" -ForegroundColor Cyan
Write-Host " PageSize:    $PageSizes" -ForegroundColor Cyan
Write-Host " Cache:       $CacheStates" -ForegroundColor Cyan
Write-Host " OrderItems:  $OrderItems" -ForegroundColor Cyan
Write-Host " Iter/VU: $Iter   Runs: $Runs   Cooldown: ${Cooldown}s" -ForegroundColor Cyan
Write-Host " Matrix:      $cellCount komorek -> $testCount testow (przed filtrem MAX_VU_ROWS)" -ForegroundColor Cyan
Write-Host " Target:      $BACKEND_HOST (Hetzner, Norymberga)" -ForegroundColor Cyan
if ($Quick) { Write-Host " MODE: QUICK SMOKE TEST" -ForegroundColor Magenta }
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$angularProc = $null

function Stop-Angular {
    if ($script:angularProc -and -not $script:angularProc.HasExited) {
        # Kill the whole process tree (cmd.exe -> node -> ng serve)
        & taskkill.exe /PID $script:angularProc.Id /T /F 2>&1 | Out-Null
    }
    # Free port 4200 if anything is still holding it
    Get-NetTCPConnection -LocalPort 4200 -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
}

try {
    # --- Step 0: Preflight — is the Hetzner backend reachable? ---
    if (-not $SkipPreflight) {
        Write-Host "[0/4] Preflight: checking backend..." -ForegroundColor Yellow
        $checks = @(
            @{ Name = "REST Products (:5000)"; Url = "http://${BACKEND_HOST}:5000/api/products?pageSize=1" }
            @{ Name = "REST Orders   (:5003)"; Url = "http://${BACKEND_HOST}:5003/api/orders?pageSize=1" }
            @{ Name = "Envoy proxy   (:8080)"; Url = "http://${BACKEND_HOST}:8080/" }
            @{ Name = "Direct gRPC-Web (:5002)"; Url = "https://${BACKEND_HOST}:5002/" }
            @{ Name = "Direct gRPC-Web (:5005)"; Url = "https://${BACKEND_HOST}:5005/" }
        )
        $failed = @()
        foreach ($c in $checks) {
            try {
                $null = Invoke-WebRequest -Uri $c.Url -UseBasicParsing -TimeoutSec 10 `
                    -SkipCertificateCheck -ErrorAction Stop
                Write-Host "      OK        $($c.Name)" -ForegroundColor Green
            } catch {
                # Any HTTP status (even 404) means the port is alive and serving
                if ($_.Exception.Response.StatusCode.value__) {
                    Write-Host "      OK        $($c.Name)" -ForegroundColor Green
                } else {
                    Write-Host "      NO REPLY  $($c.Name)" -ForegroundColor Red
                    $failed += $c.Name
                }
            }
        }
        if ($failed.Count -gt 0) {
            Write-Host ""
            Write-Host "ERROR: backend unreachable: $($failed -join ', ')" -ForegroundColor Red
            Write-Host "Start the stack on the server, or rerun with -SkipPreflight." -ForegroundColor Red
            exit 1
        }
    }

    # --- Step 1: Install Playwright if needed ---
    if (-not (Test-Path (Join-Path $PLAYWRIGHT_DIR "node_modules\playwright"))) {
        Write-Host "[1/4] Installing Playwright (first run, may take a few minutes)..." -ForegroundColor Yellow
        Push-Location $PLAYWRIGHT_DIR
        try {
            & npm install --no-fund --no-audit
            if ($LASTEXITCODE -ne 0) { throw "npm install failed" }
            & npx playwright install chromium
            if ($LASTEXITCODE -ne 0) { throw "playwright browser download failed" }
        } finally { Pop-Location }
    } else {
        Write-Host "[1/4] Playwright already installed." -ForegroundColor Green
    }

    # --- Step 2: Start Angular dev server in background ---
    Stop-Angular  # make sure port 4200 is free
    Write-Host "[2/4] Starting Angular dev server..." -ForegroundColor Yellow

    # cmd.exe /c is used deliberately: on Windows `npx` resolves to a .ps1/.cmd
    # shim that Start-Process cannot launch directly.
    $angularProc = Start-Process -FilePath "$env:ComSpec" `
        -ArgumentList "/c", "npm start" `
        -WorkingDirectory $ANGULAR_DIR -PassThru -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $LOG_DIR "ng-bench-out.log") `
        -RedirectStandardError  (Join-Path $LOG_DIR "ng-bench-err.log")
    $script:angularProc = $angularProc

    $timeout = 120
    $elapsed = 0
    $ready = $false
    Write-Host "      Waiting for http://localhost:4200 (max ${timeout}s)..." -NoNewline
    while ($elapsed -lt $timeout) {
        if ($angularProc.HasExited) {
            Write-Host ""
            Write-Host "ERROR: Angular process exited (code $($angularProc.ExitCode))." -ForegroundColor Red
            Write-Host "--- ng-bench-err.log ---" -ForegroundColor DarkGray
            Get-Content (Join-Path $LOG_DIR "ng-bench-err.log") -Tail 30 -ErrorAction SilentlyContinue
            exit 1
        }
        Start-Sleep -Seconds 2
        $elapsed += 2
        try {
            $null = Invoke-WebRequest -Uri "http://localhost:4200" -UseBasicParsing `
                -TimeoutSec 3 -ErrorAction Stop
            $ready = $true
            break
        } catch { Write-Host "." -NoNewline }
    }

    if (-not $ready) {
        Write-Host ""
        Write-Host "ERROR: Angular did not respond within ${timeout}s." -ForegroundColor Red
        Write-Host "--- ng-bench-err.log ---" -ForegroundColor DarkGray
        Get-Content (Join-Path $LOG_DIR "ng-bench-err.log") -Tail 30 -ErrorAction SilentlyContinue
        exit 1
    }
    Write-Host " ready!" -ForegroundColor Green

    # --- Step 3: Run benchmark ---
    Write-Host "[3/4] Running browser benchmark..." -ForegroundColor Yellow
    Write-Host ""

    $env:FRONTEND_URL = "http://localhost:4200"
    $env:VU_LIST = $VUList
    $env:PAGE_SIZES = $PageSizes
    $env:CACHE_STATES = $CacheStates
    $env:ORDER_ITEMS = $OrderItems
    $env:ECHO_SIZES = $EchoSizes
    $env:ITER = $Iter
    $env:RUNS = $Runs
    $env:COOLDOWN = $Cooldown

    Push-Location $PLAYWRIGHT_DIR
    try {
        & node browser-benchmark.js
        $exitCode = $LASTEXITCODE
    } finally { Pop-Location }

    # --- Step 4: Report ---
    Write-Host ""
    if ($exitCode -eq 0) {
        Write-Host "============================================================" -ForegroundColor Green
        Write-Host " DONE" -ForegroundColor Green
        $latest = Get-ChildItem (Join-Path $ROOT "hetzner-results\browser-benchmark-*.json") `
            -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($latest) { Write-Host " Results: $($latest.FullName)" -ForegroundColor Green }
        Write-Host "============================================================" -ForegroundColor Green
    } else {
        Write-Host "Benchmark failed with exit code $exitCode" -ForegroundColor Red
    }
}
finally {
    Write-Host ""
    Write-Host "[4/4] Stopping Angular dev server..." -ForegroundColor Yellow
    Stop-Angular
    Write-Host "      Stopped." -ForegroundColor Green
}

if ($exitCode -ne 0) { exit $exitCode }
