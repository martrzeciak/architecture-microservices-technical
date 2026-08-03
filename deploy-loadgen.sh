#!/bin/bash
# =============================================================================
# deploy-loadgen.sh — Setup for the k6 load generator server (Server 2)
# Run this as root on a fresh Ubuntu 24.04 CX22 instance.
#
# This is the lightweight counterpart to deploy.sh (which sets up the backend).
# This server only needs Docker (to run k6) and the test scripts.
#
# Usage:
#   export REPO_URL="https://github.com/YOUR_USERNAME/YOUR_REPO.git"
#   export BACKEND_IP="168.119.x.x"  # IP of Server 1 (backend)
#   ./deploy-loadgen.sh
# =============================================================================

set -euo pipefail

# --- Configuration ---
REPO_URL="${REPO_URL:-https://github.com/YOUR_USERNAME/YOUR_REPO.git}"
REPO_DIR="/root/architecture-microservices-technical"
BRANCH="${BRANCH:-main}"
BACKEND_IP="${BACKEND_IP:-}"

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

# --- Validate BACKEND_IP ---
if [ -z "$BACKEND_IP" ]; then
  error "BACKEND_IP is not set. Export it before running:
  export BACKEND_IP=168.119.x.x
  ./deploy-loadgen.sh"
fi

log "Load Generator setup for backend at: $BACKEND_IP"

# --- Step 1: Install Docker ---
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

# --- Step 2: Install tools ---
log "Step 2: Installing tools..."
apt-get install -y -qq git tmux htop curl
ok "Tools installed"

# --- Step 3: Clone repository (only need tests/k6 dir, but clone all) ---
log "Step 3: Cloning repository..."

if [ -d "$REPO_DIR" ]; then
  warn "Directory exists. Pulling latest..."
  cd "$REPO_DIR"
  git pull origin "$BRANCH" || true
else
  git clone --branch "$BRANCH" "$REPO_URL" "$REPO_DIR"
  cd "$REPO_DIR"
fi
ok "Repository ready"

# --- Step 4: Pre-pull k6 image ---
log "Step 4: Pulling k6 Docker image..."
docker pull grafana/k6:latest
ok "k6 image ready"

# --- Step 5: Make scripts executable ---
chmod +x tests/k6/run-all-linux.sh

# --- Step 6: Verify connectivity to backend ---
log "Step 5: Checking connectivity to backend ($BACKEND_IP)..."

check_port() {
  local port=$1
  local label=$2
  if curl -sf --max-time 5 "http://$BACKEND_IP:$port" >/dev/null 2>&1 || \
     curl -skf --max-time 5 "https://$BACKEND_IP:$port" >/dev/null 2>&1; then
    ok "$label (:$port) — reachable"
  else
    warn "$label (:$port) — not reachable (may not be started yet)"
  fi
}

check_port 5000 "ProductService REST"
check_port 5003 "OrderService REST"
check_port 8080 "Envoy gRPC-Web"
check_port 5001 "ProductService gRPC"

# --- Done ---
echo ""
echo -e "${GREEN}============================================================${NC}"
echo -e "${GREEN}  Load Generator Ready!${NC}"
echo -e "${GREEN}============================================================${NC}"
echo ""
echo -e "  Backend IP: ${CYAN}$BACKEND_IP${NC}"
echo ""
echo -e "  ${YELLOW}Quick smoke test:${NC}"
echo -e "    cd $REPO_DIR/tests/k6"
echo -e "    ./run-all-linux.sh --remote --backend-ip $BACKEND_IP --quick --runs 1"
echo ""
echo -e "  ${YELLOW}Full benchmark (3 runs, ~30 hours):${NC}"
echo -e "    tmux new -s bench"
echo -e "    cd $REPO_DIR/tests/k6"
echo -e "    ./run-all-linux.sh --remote --backend-ip $BACKEND_IP"
echo -e "    # Ctrl+B, D to detach"
echo ""
echo -e "  ${YELLOW}Reduced benchmark (1 run, ~10 hours):${NC}"
echo -e "    ./run-all-linux.sh --remote --backend-ip $BACKEND_IP --runs 1"
echo ""
echo -e "  ${YELLOW}Download results to local machine:${NC}"
echo -e "    scp -r root@$(curl -4 -s ifconfig.me):$REPO_DIR/tests/k6/results/ ./hetzner-results/"
echo ""
echo -e "${GREEN}============================================================${NC}"
