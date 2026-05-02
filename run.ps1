#!/usr/bin/env pwsh
# Unified Lifecycle Script for eShop Microservices
# Usage: .\run.ps1        -- start everything
#        .\run.ps1 -Stop  -- stop everything
#        .\run.ps1 -Logs  -- tail logs

param (
    [switch]$Stop,
    [switch]$Logs
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$RUNNER_DIR  = "$PSScriptRoot\tests\runner"
$ANGULAR_DIR = "$PSScriptRoot\frontend\angular-app"
$LOG_DIR     = "$PSScriptRoot\.logs"

function Log($text, $color = "White") {
    Write-Host "[$((Get-Date).ToString('HH:mm:ss'))] $text" -ForegroundColor $color
}

function Wait-Port($port, $timeoutSec = 120) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $timeoutSec) {
        $ok = Test-NetConnection -ComputerName localhost -Port $port `
              -WarningAction SilentlyContinue -InformationLevel Quiet
        if ($ok) { return $true }
        Start-Sleep 2
    }
    return $false
}

# LOGS
if ($Logs) {
    Log "Tailing logs -- Ctrl+C to stop" "Cyan"
    $files = @(
        "$LOG_DIR\runner-out.log",
        "$LOG_DIR\ng-out.log",
        "$LOG_DIR\ng-err.log"
    ) | Where-Object { Test-Path $_ }
    if (-not $files) { Log "No log files found in $LOG_DIR" "Yellow"; exit 0 }
    Get-Content $files -Wait -Tail 20
    exit 0
}

# STOP
if ($Stop) {
    Log "STOPPING ALL SYSTEMS..." "Red"

    Log "Stopping Docker containers..." "Yellow"
    docker compose down

    Log "Killing runner and Angular processes..." "Yellow"
    Get-CimInstance Win32_Process -Filter "Name='node.exe'" | ForEach-Object {
        $cmd = $_.CommandLine
        if ($cmd -like "*tests\runner*" -or $cmd -like "*tests/runner*" -or
            $cmd -like "*ng*serve*" -or $cmd -like "*@angular*") {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
            Log "Stopped node PID $($_.ProcessId)" "Gray"
        }
    }
    Get-CimInstance Win32_Process -Filter "Name='cmd.exe'" | ForEach-Object {
        $cmd = $_.CommandLine
        if ($cmd -like "*angular-app*" -or $cmd -like "*ng serve*") {
            Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
            Log "Stopped cmd PID $($_.ProcessId)" "Gray"
        }
    }

    Log "ALL SYSTEMS STOPPED." "Green"
    exit 0
}

# START
try {
    New-Item -ItemType Directory -Force -Path $LOG_DIR | Out-Null

    Log "STARTING ALL SYSTEMS..." "Cyan"

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw "Docker not found in PATH." }
    try { docker info | Out-Null } catch { throw "Docker Desktop is not running." }

    # 1. Docker Compose
    Log "1. Starting Docker Compose..." "Yellow"
    docker compose up -d --build
    if ($LASTEXITCODE -ne 0) { throw "docker compose up failed (exit $LASTEXITCODE)." }

    Log "Waiting for PostgreSQL..." "Yellow"
    $healthy = $false
    for ($i = 0; $i -lt 30; $i++) {
        $status = docker compose ps postgres --format "{{.Health}}" 2>$null
        if ($status -eq "healthy") { $healthy = $true; break }
        Start-Sleep 2
    }
    if (-not $healthy) { throw "PostgreSQL failed to become healthy." }
    Log "PostgreSQL healthy." "Green"

    # 2. k6 Runner Server -- node.exe is a real .exe, works with Start-Process
    Log "2. Starting k6 Runner Server..." "Yellow"
    if (-not (Test-Path "$RUNNER_DIR\node_modules")) {
        Log "Installing runner deps..." "Gray"
        Push-Location $RUNNER_DIR
        & cmd /c "npm install --silent"
        Pop-Location
    }
    Start-Process node -ArgumentList "server.js" `
        -WorkingDirectory $RUNNER_DIR `
        -RedirectStandardOutput "$LOG_DIR\runner-out.log" `
        -RedirectStandardError  "$LOG_DIR\runner-err.log" `
        -WindowStyle Hidden

    if (-not (Wait-Port 3100 30)) {
        Get-Content "$LOG_DIR\runner-err.log" -Tail 10 | ForEach-Object { Log "  $_" "Gray" }
        throw "k6 Runner did not start on :3100 after 30s."
    }
    Log "k6 Runner OK (:3100)." "Green"

    # 3. Angular Frontend -- npm is .cmd on Windows, must use cmd.exe /c
    Log "3. Starting Angular Frontend..." "Yellow"
    if (-not (Test-Path "$ANGULAR_DIR\node_modules")) {
        Log "Installing frontend deps (may take a few minutes)..." "Gray"
        Push-Location $ANGULAR_DIR
        & cmd /c "npm install --silent"
        Pop-Location
    }
    Start-Process cmd -ArgumentList "/c npm start" `
        -WorkingDirectory $ANGULAR_DIR `
        -RedirectStandardOutput "$LOG_DIR\ng-out.log" `
        -RedirectStandardError  "$LOG_DIR\ng-err.log" `
        -WindowStyle Hidden

    Log "Waiting for Angular -- first compile may take up to 90s..." "Yellow"
    if (-not (Wait-Port 4200 120)) {
        Log "Angular logs:" "Red"
        Get-Content "$LOG_DIR\ng-err.log" -Tail 20 | ForEach-Object { Log "  $_" "Gray" }
        throw "Angular did not respond on :4200 after 120s. See: $LOG_DIR\ng-err.log"
    }
    Log "Angular OK (:4200)." "Green"

    Log "--------------------------------------------------------" "Green"
    Log "SUCCESS: All systems running!" "Green"
    Log "UI:         http://localhost:4200" "Cyan"
    Log "Grafana:    http://localhost:3000" "White"
    Log "Jaeger:     http://localhost:16686" "White"
    Log "Prometheus: http://localhost:9090" "White"
    Log "Runner API: http://localhost:3100" "White"
    Log "--------------------------------------------------------" "Green"
    Log "Stop:  .\run.ps1 -Stop" "Gray"
    Log "Logs:  .\run.ps1 -Logs" "Gray"

} catch {
    Log "--------------------------------------------------------" "Red"
    Log "FATAL ERROR: $($_.Exception.Message)" "Red"
    Log "--------------------------------------------------------" "Red"
    exit 1
}
