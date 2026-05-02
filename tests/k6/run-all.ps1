#!/usr/bin/env pwsh
# Runs all k6 scenarios sequentially inside Docker.
# Results are saved as JSON files in tests/k6/results/
#
# Usage:
#   .\run-all.ps1
#   .\run-all.ps1 -VuLevels 10,50,100,500
#   .\run-all.ps1 -PageSizes 10,200
#   .\run-all.ps1 -Runs 3

param(
    [int[]]$VuLevels   = @(10, 50, 100),
    [int[]]$PageSizes  = @(10, 50, 100, 200),
    [int[]]$OrderItems = @(1, 5, 10),
    [int]$Runs         = 1,
    [string]$ResultsDir = "$PSScriptRoot\results"
)

# product scenarios — loop over PAGE_SIZE
$productScenarios = @(
    "scenario-products-rest",
    "scenario-products-grpc-envoy",
    "scenario-products-grpc-direct"
)

# streaming scenario — only loops over VU levels
$streamScenarios = @(
    "scenario-products-grpc-stream"
)

# order scenarios — loop over ORDER_ITEMS
$orderScenarios = @(
    "scenario-orders-rest",
    "scenario-orders-grpc-envoy",
    "scenario-orders-grpc-direct"
)

$DOCKER_NETWORK    = 'architecture-microservices-technical_default'
$COOLDOWN_SECONDS  = 30
$STREAM_CATEGORY   = 'electronics'

function ConvertTo-DockerPath([string]$WinPath) {
    $WinPath.Replace('\', '/') -replace '^([A-Za-z]):/', { "/$($_.Groups[1].Value.ToLower())/" }
}

$k6Dir          = Resolve-Path "$PSScriptRoot"
$dockerK6Dir    = ConvertTo-DockerPath $k6Dir

New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null
$dockerResultsDir = ConvertTo-DockerPath (Resolve-Path $ResultsDir)

$timestamp    = Get-Date -Format "yyyyMMdd_HHmmss"
$summaryFile  = "$ResultsDir\summary_$timestamp.txt"

$productTests = $productScenarios.Count * $VuLevels.Count * $PageSizes.Count * $Runs
$streamTests  = $streamScenarios.Count  * $VuLevels.Count * $Runs
$orderTests   = $orderScenarios.Count   * $VuLevels.Count * $OrderItems.Count * $Runs
$totalTests   = $productTests + $streamTests + $orderTests
$completedTests = 0

Add-Content $summaryFile "eShop Performance Benchmark — REST vs gRPC-Web vs Native gRPC"
Add-Content $summaryFile "Data: $(Get-Date)"
Add-Content $summaryFile "Docker network: $DOCKER_NETWORK"
Add-Content $summaryFile "VU levels: $($VuLevels -join ', ')"
Add-Content $summaryFile "Page sizes (products): $($PageSizes -join ', ')"
Add-Content $summaryFile "Order items: $($OrderItems -join ', ')"
Add-Content $summaryFile "Stream category: $STREAM_CATEGORY"
Add-Content $summaryFile "Runs per scenario: $Runs"
Add-Content $summaryFile "Total tests: $totalTests"
Add-Content $summaryFile "Stages: 30s warmup + 120s steady + 10s cooldown = 160s"
Add-Content $summaryFile "=================================================="

Write-Host ""
Write-Host "=== eShop Performance Benchmark ===" -ForegroundColor Cyan
Write-Host "VU: [$($VuLevels -join ', ')] | PageSize: [$($PageSizes -join ', ')] | OrderItems: [$($OrderItems -join ', ')] | Runs: $Runs"
Write-Host "Lacznie: $totalTests testow, ~$([math]::Round($totalTests * (160 + $COOLDOWN_SECONDS) / 60)) minut"
Write-Host ""

function Invoke-K6 {
    param(
        [string]$Scenario,
        [string]$Label,
        [string]$JsonFile,
        [hashtable]$EnvVars
    )

    $envArgs = @()
    foreach ($kv in $EnvVars.GetEnumerator()) {
        $envArgs += '-e'; $envArgs += "$($kv.Key)=$($kv.Value)"
    }

    $script:completedTests++
    Write-Host "[$script:completedTests/$totalTests] $Label" -ForegroundColor Cyan

    docker run --rm `
        -v "${dockerK6Dir}:/scripts" `
        -v "${dockerResultsDir}:/results" `
        --network $DOCKER_NETWORK `
        @envArgs `
        grafana/k6 `
        run `
        --summary-export "/results/$JsonFile" `
        "/scripts/$Scenario.js"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  OK" -ForegroundColor Green
        Add-Content $summaryFile "OK   | $Label"
    } else {
        Write-Host "  FAIL (exit $LASTEXITCODE)" -ForegroundColor Red
        Add-Content $summaryFile "FAIL | $Label"
    }

    if ($script:completedTests -lt $totalTests) {
        Write-Host "  Cooldown ${COOLDOWN_SECONDS}s..." -ForegroundColor DarkGray
        Start-Sleep $COOLDOWN_SECONDS
    }
}

# --- PRODUKTY: PageSize × VU × Runs × scenariusze ---
Write-Host ">>> Produkty ($productTests testow)" -ForegroundColor Yellow
foreach ($pageSize in $PageSizes) {
    foreach ($vu in $VuLevels) {
        foreach ($run in 1..$Runs) {
            foreach ($scenario in $productScenarios) {
                $label   = "$scenario | VU=$vu | PS=$pageSize | Run=$run"
                $jsonFile = "${scenario}_VU${vu}_PS${pageSize}_run${run}_${timestamp}-summary.json"
                Invoke-K6 -Scenario $scenario -Label $label -JsonFile $jsonFile -EnvVars @{
                    VU        = $vu
                    PAGE_SIZE = $pageSize
                    K6_ENV    = 'docker'
                }
            }
        }
    }
}

# streaming — only parameterized by VU
Write-Host ""
Write-Host ">>> Streaming ($streamTests testow)" -ForegroundColor Yellow
foreach ($vu in $VuLevels) {
    foreach ($run in 1..$Runs) {
        foreach ($scenario in $streamScenarios) {
            $label    = "$scenario | VU=$vu | CAT=$STREAM_CATEGORY | Run=$run"
            $jsonFile = "${scenario}_VU${vu}_CAT${STREAM_CATEGORY}_run${run}_${timestamp}-summary.json"
            Invoke-K6 -Scenario $scenario -Label $label -JsonFile $jsonFile -EnvVars @{
                VU              = $vu
                STREAM_CATEGORY = $STREAM_CATEGORY
                K6_ENV          = 'docker'
            }
        }
    }
}

# orders — parameterized by ORDER_ITEMS and VU
Write-Host ""
Write-Host ">>> Zamowienia ($orderTests testow)" -ForegroundColor Yellow
foreach ($orderItems in $OrderItems) {
    foreach ($vu in $VuLevels) {
        foreach ($run in 1..$Runs) {
            foreach ($scenario in $orderScenarios) {
                $label    = "$scenario | VU=$vu | OI=$orderItems | Run=$run"
                $jsonFile = "${scenario}_VU${vu}_OI${orderItems}_run${run}_${timestamp}-summary.json"
                Invoke-K6 -Scenario $scenario -Label $label -JsonFile $jsonFile -EnvVars @{
                    VU          = $vu
                    ORDER_ITEMS = $orderItems
                    K6_ENV      = 'docker'
                }
            }
        }
    }
}

Write-Host ""
Write-Host "=== Zakonczone ===" -ForegroundColor Yellow
Write-Host "Wyniki JSON: $ResultsDir" -ForegroundColor Yellow
Write-Host "Podsumowanie: $summaryFile" -ForegroundColor Yellow
