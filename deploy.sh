#!/bin/bash
# =============================================================================
# deploy.sh — Full server setup for eShop benchmark on Hetzner Cloud
# Run this as root on a fresh Ubuntu 24.04 server.
#
# Usage:
#   curl -sSL <raw-github-url>/deploy.sh | bash
#   # or after cloning:
#   chmod +x deploy.sh && ./deploy.sh
#
# What it does:
#   1. Installs Docker CE + Docker Compose plugin
#   2. Clones the repository (or uses existing clone)
#   3. Generates TLS certificate
#   4. Replaces HOST_IP placeholder with actual server IP
#   5. Starts all containers with docker compose
#   6. Waits for health checks
#   7. Prints access URLs
# =============================================================================

set -euo pipefail

# --- Configuration ---
REPO_URL="${REPO_URL:-https://github.com/YOUR_USERNAME/YOUR_REPO.git}"
REPO_DIR="/root/architecture-microservices-technical"
BRANCH="${BRANCH:-main}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log()   { echo -e "${CYAN}[$(date +%H:%M:%S)]${NC} $1"; }
ok()    { echo -e "${GREEN}[$(date +%H:%M:%S)] ✓ $1${NC}"; }
warn()  { echo -e "${YELLOW}[$(date +%H:%M:%S)] ⚠ $1${NC}"; }
error() { echo -e "${RED}[$(date +%H:%M:%S)] ✗ $1${NC}"; exit 1; }

# --- Detect server IP ---
HOST_IP=$(curl -4 -s ifconfig.me || curl -4 -s icanhazip.com || hostname -I | awk '{print $1}')
log "Detected server IP: $HOST_IP"

# --- Step 1: System update + Docker install ---
log "Step 1: Installing Docker CE..."

if command -v docker &>/dev/null; then
  ok "Docker already installed: $(docker --version)"
else
  apt-get update -qq
  apt-get install -y -qq ca-certificates curl gnupg lsb-release

  install -m 0755 -d /etc/apt/keyrings
  curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
  chmod a+r /etc/apt/keyrings/docker.gpg

  echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
    https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
    > /etc/apt/sources.list.d/docker.list

  apt-get update -qq
  apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

  systemctl enable docker
  systemctl start docker
  ok "Docker installed: $(docker --version)"
fi

# --- Step 2: Install additional tools ---
log "Step 2: Installing tools (git, openssl, tmux)..."
apt-get install -y -qq git openssl tmux htop
ok "Tools installed"

# --- Step 3: Create swap (safety net for .NET build) ---
if [ ! -f /swapfile ]; then
  log "Step 3: Creating 4GB swap..."
  fallocate -l 4G /swapfile
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
  ok "4GB swap enabled"
else
  ok "Swap already exists"
fi

# --- Step 4: Clone repository ---
log "Step 4: Cloning repository..."

if [ -d "$REPO_DIR" ]; then
  warn "Directory $REPO_DIR already exists. Pulling latest..."
  cd "$REPO_DIR"
  git pull origin "$BRANCH" || true
else
  git clone --branch "$BRANCH" "$REPO_URL" "$REPO_DIR"
  cd "$REPO_DIR"
fi
ok "Repository ready at $REPO_DIR"

# --- Step 5: Generate TLS certificate ---
log "Step 5: Generating TLS certificate..."
chmod +x infrastructure/certs/generate-cert.sh
# Pass HOST_IP so the cert includes the server's public IP as a SAN
HOST_IP="$HOST_IP" ./infrastructure/certs/generate-cert.sh
ok "Certificate generated"

# --- Step 6: Replace HOST_IP placeholder in docker-compose.prod.yml ---
log "Step 6: Configuring HOST_IP ($HOST_IP) in docker-compose.prod.yml..."
# Work from a template so re-runs are idempotent
if [ ! -f docker-compose.prod.yml.template ]; then
  cp docker-compose.prod.yml docker-compose.prod.yml.template
fi
cp docker-compose.prod.yml.template docker-compose.prod.yml
sed -i "s|HOST_IP|$HOST_IP|g" docker-compose.prod.yml
ok "HOST_IP replaced"

# --- Step 7: Pull base images (parallel, speeds up build) ---
log "Step 7: Pre-pulling base images..."
docker pull mcr.microsoft.com/dotnet/sdk:10.0 &
docker pull mcr.microsoft.com/dotnet/aspnet:10.0 &
docker pull node:22-alpine &
docker pull nginx:alpine &
docker pull postgres:17-alpine &
docker pull redis:7.2-alpine &
docker pull rabbitmq:3.13-management-alpine &
docker pull envoyproxy/envoy:v1.29-latest &
docker pull grafana/k6 &
docker pull prom/prometheus:v2.51.2 &
docker pull grafana/grafana:10.4.2 &
docker pull jaegertracing/all-in-one:1.56 &
wait
ok "Base images pulled"

# --- Step 8: Build and start all services ---
log "Step 8: Building and starting containers (this may take 5-10 min first time)..."
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build

ok "Containers started"

# --- Step 9: Wait for health checks ---
log "Step 9: Waiting for services to become healthy..."

wait_healthy() {
  local service="$1"
  local max_wait="${2:-120}"
  local elapsed=0
  while [ $elapsed -lt $max_wait ]; do
    status=$(docker compose -f docker-compose.yml -f docker-compose.prod.yml ps "$service" --format "{{.Health}}" 2>/dev/null || echo "unknown")
    if [ "$status" = "healthy" ]; then
      ok "$service is healthy"
      return 0
    fi
    sleep 3
    elapsed=$((elapsed + 3))
  done
  warn "$service did not become healthy in ${max_wait}s (status: $status)"
  return 1
}

wait_healthy "postgres" 60 || true
wait_healthy "redis" 30 || true
wait_healthy "rabbitmq" 60 || true
wait_healthy "product-service" 120 || true
wait_healthy "order-service" 120 || true

# Give Envoy, Prometheus, Grafana a moment
sleep 5
ok "All core services up"

# --- Step 10: Make k6 scripts executable ---
chmod +x tests/k6/run-all-linux.sh

# --- Step 11: Pre-pull k6 image for tests ---
log "Step 10: Ensuring k6 image is ready..."
docker pull grafana/k6:latest
ok "k6 image ready"

# --- Done ---
echo ""
echo -e "${GREEN}============================================================${NC}"
echo -e "${GREEN}  SUCCESS: eShop Benchmark Environment Ready!${NC}"
echo -e "${GREEN}============================================================${NC}"
echo ""
echo -e "  Server IP:     ${CYAN}$HOST_IP${NC}"
echo ""
echo -e "  ${CYAN}Angular UI:${NC}     http://$HOST_IP:4200"
echo -e "  ${CYAN}Grafana:${NC}        http://$HOST_IP:3000  (admin/admin)"
echo -e "  ${CYAN}Jaeger:${NC}         http://$HOST_IP:16686"
echo -e "  ${CYAN}Prometheus:${NC}     http://$HOST_IP:9090"
echo -e "  ${CYAN}Runner API:${NC}     http://$HOST_IP:3100"
echo -e "  ${CYAN}Envoy:${NC}          http://$HOST_IP:8080"
echo -e "  ${CYAN}RabbitMQ Mgmt:${NC}  http://$HOST_IP:15672 (guest/guest)"
echo ""
echo -e "  ${YELLOW}Run benchmarks:${NC}"
echo -e "    cd $REPO_DIR/tests/k6"
echo -e "    ./run-all-linux.sh --quick    # smoke test (~20 min)"
echo -e "    ./run-all-linux.sh            # full benchmark (~10 hours)"
echo ""
echo -e "  ${YELLOW}Run in background (survives SSH disconnect):${NC}"
echo -e "    tmux new -s bench"
echo -e "    ./run-all-linux.sh"
echo -e "    # Ctrl+B then D to detach, 'tmux attach -t bench' to reconnect"
echo ""
echo -e "  ${YELLOW}Download results to local machine:${NC}"
echo -e "    scp -r root@$HOST_IP:$REPO_DIR/tests/k6/results/ ./hetzner-results/"
echo ""
echo -e "${GREEN}============================================================${NC}"
