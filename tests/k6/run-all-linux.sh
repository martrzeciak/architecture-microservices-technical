#!/bin/bash
# Runs all k6 scenarios sequentially inside Docker (Linux equivalent of run-all.ps1).
# Results are saved as JSON files in tests/k6/results/
#
# Usage:
#   ./run-all-linux.sh                      # full benchmark
#   ./run-all-linux.sh --quick              # smoke test (VU=10,100 PS=10,100)
#   ./run-all-linux.sh --vu 10,50,100,500
#   ./run-all-linux.sh --page-sizes 10,200
#   ./run-all-linux.sh --runs 3

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$SCRIPT_DIR/results"
DOCKER_NETWORK="architecture-microservices-technical_default"
COOLDOWN_SECONDS=30
STREAM_CATEGORY="electronics"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Defaults
VU_LEVELS=(10 50 100 500)
PAGE_SIZES=(10 50 100 200)
ORDER_ITEMS=(1 5 10)
CACHE_STATES=(0 1)
RUNS=3

# Parse arguments
QUICK=false
REMOTE=false
BACKEND_IP=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --quick)
      QUICK=true
      VU_LEVELS=(10 100)
      PAGE_SIZES=(10 100)
      ORDER_ITEMS=(1 5)
      CACHE_STATES=(0 1)
      RUNS=1
      shift
      ;;
    --remote)
      REMOTE=true
      shift
      ;;
    --backend-ip)
      BACKEND_IP="$2"
      shift 2
      ;;
    --vu)
      IFS=',' read -ra VU_LEVELS <<< "$2"
      shift 2
      ;;
    --page-sizes)
      IFS=',' read -ra PAGE_SIZES <<< "$2"
      shift 2
      ;;
    --order-items)
      IFS=',' read -ra ORDER_ITEMS <<< "$2"
      shift 2
      ;;
    --cache-states)
      IFS=',' read -ra CACHE_STATES <<< "$2"
      shift 2
      ;;
    --runs)
      RUNS="$2"
      shift 2
      ;;
    --cooldown)
      COOLDOWN_SECONDS="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Validate remote mode
if [[ "$REMOTE" == "true" && -z "$BACKEND_IP" ]]; then
  echo "ERROR: --remote requires --backend-ip <IP>"
  echo "Usage: ./run-all-linux.sh --remote --backend-ip 168.119.x.x"
  exit 1
fi

# Set K6 environment mode
if [[ "$REMOTE" == "true" ]]; then
  K6_ENV_MODE="remote"
  DOCKER_NETWORK=""  # no Docker network needed — k6 connects via public IP
  log "Mode: REMOTE (backend at $BACKEND_IP)"
else
  K6_ENV_MODE="docker"
  log "Mode: DOCKER (local network)"
fi

# Scenario definitions
PRODUCT_SCENARIOS=(
  "scenario-products-rest"
  "scenario-products-grpc-envoy"
  "scenario-products-grpc-direct"
  "scenario-products-grpc-native"
)

STREAM_SCENARIOS=(
  "scenario-products-grpc-stream"
)

ORDER_SCENARIOS=(
  "scenario-orders-rest"
  "scenario-orders-grpc-envoy"
  "scenario-orders-grpc-direct"
  "scenario-orders-grpc-native"
)

# Echo scenarios — no DB, pure protocol overhead
ECHO_SCENARIOS=(
  "scenario-echo-rest"
  "scenario-echo-grpc-envoy"
  "scenario-echo-grpc-direct"
  "scenario-echo-grpc-native"
)

# Calculate total tests
PRODUCT_TESTS=$(( ${#PRODUCT_SCENARIOS[@]} * ${#VU_LEVELS[@]} * ${#PAGE_SIZES[@]} * ${#CACHE_STATES[@]} * RUNS ))
STREAM_TESTS=$(( ${#STREAM_SCENARIOS[@]} * ${#VU_LEVELS[@]} * RUNS ))
ORDER_TESTS=$(( ${#ORDER_SCENARIOS[@]} * ${#VU_LEVELS[@]} * ${#ORDER_ITEMS[@]} * RUNS ))
ECHO_TESTS=$(( ${#ECHO_SCENARIOS[@]} * ${#VU_LEVELS[@]} * ${#PAGE_SIZES[@]} * RUNS ))
TOTAL_TESTS=$(( PRODUCT_TESTS + STREAM_TESTS + ORDER_TESTS + ECHO_TESTS ))
COMPLETED=0

mkdir -p "$RESULTS_DIR"
SUMMARY_FILE="$RESULTS_DIR/summary_${TIMESTAMP}.txt"

# Logging
log() { echo "[$(date +%H:%M:%S)] $1"; }
log_color() { echo -e "[$(date +%H:%M:%S)] \033[${1}m${2}\033[0m"; }

# Summary header
cat >> "$SUMMARY_FILE" <<EOF
eShop Performance Benchmark — REST vs gRPC-Web vs Native gRPC
Date: $(date)
Docker network: $DOCKER_NETWORK
VU levels: ${VU_LEVELS[*]}
Page sizes (products): ${PAGE_SIZES[*]}
Order items: ${ORDER_ITEMS[*]}
Cache states: $(for s in "${CACHE_STATES[@]}"; do [[ $s == 1 ]] && echo -n "COLD " || echo -n "WARM "; done)
Stream category: $STREAM_CATEGORY
Runs per scenario: $RUNS
Total tests: $TOTAL_TESTS
Stages: 30s warmup + 120s steady + 10s cooldown = 160s
==================================================
EOF

echo ""
log_color "36" "=== eShop Performance Benchmark ==="
echo "VU: [${VU_LEVELS[*]}] | PageSize: [${PAGE_SIZES[*]}] | OrderItems: [${ORDER_ITEMS[*]}] | Runs: $RUNS"
ESTIMATED_MIN=$(( TOTAL_TESTS * (160 + COOLDOWN_SECONDS) / 60 ))
echo "Total: $TOTAL_TESTS tests, ~${ESTIMATED_MIN} minutes"
echo ""

# Run a single k6 scenario
run_k6() {
  local scenario="$1"
  local label="$2"
  local json_file="$3"
  shift 3
  local env_args=("$@")

  # Add K6_ENV and BACKEND_IP to env args
  env_args+=(-e "K6_ENV=$K6_ENV_MODE")
  if [[ "$REMOTE" == "true" ]]; then
    env_args+=(-e "BACKEND_IP=$BACKEND_IP")
  fi

  COMPLETED=$((COMPLETED + 1))
  log_color "36" "[$COMPLETED/$TOTAL_TESTS] $label"

  local exit_code=0
  docker run --rm \
    -v "$SCRIPT_DIR:/scripts:ro" \
    -v "$RESULTS_DIR:/results" \
    ${REMOTE:+--add-host=host.docker.internal:host-gateway} \
    ${DOCKER_NETWORK:+--network "$DOCKER_NETWORK"} \
    "${env_args[@]}" \
    grafana/k6 \
    run \
    --insecure-skip-tls-verify \
    --summary-export "/results/$json_file" \
    "/scripts/$scenario.js" \
    || exit_code=$?

  if [[ $exit_code -eq 0 ]]; then
    log_color "32" "  OK"
    echo "OK   | $label" >> "$SUMMARY_FILE"
  else
    log_color "31" "  FAIL (exit $exit_code)"
    echo "FAIL | $label" >> "$SUMMARY_FILE"
  fi

  if [[ $COMPLETED -lt $TOTAL_TESTS ]]; then
    log "  Cooldown ${COOLDOWN_SECONDS}s..."
    sleep "$COOLDOWN_SECONDS"
  fi
}

# --- PRODUCTS: CacheState × PageSize × VU × Runs × scenarios ---
log_color "33" ">>> Products ($PRODUCT_TESTS tests)"
for bypass in "${CACHE_STATES[@]}"; do
  if [[ $bypass -eq 1 ]]; then cache_label="COLD"; else cache_label="WARM"; fi
  for page_size in "${PAGE_SIZES[@]}"; do
    for vu in "${VU_LEVELS[@]}"; do
      for run in $(seq 1 $RUNS); do
        for scenario in "${PRODUCT_SCENARIOS[@]}"; do
          label="$scenario | VU=$vu | PS=$page_size | Cache=$cache_label | Run=$run"
          json_file="${scenario}_VU${vu}_PS${page_size}_CACHE${cache_label}_run${run}_${TIMESTAMP}-summary.json"
          run_k6 "$scenario" "$label" "$json_file" \
            -e "VU=$vu" \
            -e "PAGE_SIZE=$page_size" \
            -e "BYPASS_CACHE=$bypass"
        done
      done
    done
  done
done

# --- STREAMING: VU × Runs ---
echo ""
log_color "33" ">>> Streaming ($STREAM_TESTS tests)"
for vu in "${VU_LEVELS[@]}"; do
  for run in $(seq 1 $RUNS); do
    for scenario in "${STREAM_SCENARIOS[@]}"; do
      label="$scenario | VU=$vu | CAT=$STREAM_CATEGORY | Run=$run"
      json_file="${scenario}_VU${vu}_CAT${STREAM_CATEGORY}_run${run}_${TIMESTAMP}-summary.json"
      run_k6 "$scenario" "$label" "$json_file" \
        -e "VU=$vu" \
        -e "STREAM_CATEGORY=$STREAM_CATEGORY"
    done
  done
done

# --- ORDERS: OrderItems × VU × Runs ---
echo ""
log_color "33" ">>> Orders ($ORDER_TESTS tests)"
for order_items in "${ORDER_ITEMS[@]}"; do
  for vu in "${VU_LEVELS[@]}"; do
    for run in $(seq 1 $RUNS); do
      for scenario in "${ORDER_SCENARIOS[@]}"; do
        label="$scenario | VU=$vu | OI=$order_items | Run=$run"
        json_file="${scenario}_VU${vu}_OI${order_items}_run${run}_${TIMESTAMP}-summary.json"
        run_k6 "$scenario" "$label" "$json_file" \
          -e "VU=$vu" \
          -e "ORDER_ITEMS=$order_items"
      done
    done
  done
done

echo ""
log_color "33" ">>> Echo — pure protocol overhead ($ECHO_TESTS tests)"
for page_size in "${PAGE_SIZES[@]}"; do
  for vu in "${VU_LEVELS[@]}"; do
    for run in $(seq 1 $RUNS); do
      for scenario in "${ECHO_SCENARIOS[@]}"; do
        label="$scenario | VU=$vu | COUNT=$page_size | Run=$run"
        json_file="${scenario}_VU${vu}_COUNT${page_size}_run${run}_${TIMESTAMP}-summary.json"
        run_k6 "$scenario" "$label" "$json_file" \
          -e "VU=$vu" \
          -e "PAGE_SIZE=$page_size"
      done
    done
  done
done

echo ""
log_color "33" "=== Finished ==="
log_color "33" "Results JSON: $RESULTS_DIR"
log_color "33" "Summary: $SUMMARY_FILE"
echo ""
echo "Total tests: $COMPLETED/$TOTAL_TESTS"
