#!/bin/bash
# Generates a self-signed certificate for benchmark TLS (gRPC-Web over HTTP/2).
# SANs: localhost, product-service, order-service (Docker service names).
# Linux equivalent of generate-cert.ps1

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PFX_PATH="$SCRIPT_DIR/server.pfx"
PFX_PASSWORD="benchmark"

echo "Generating self-signed TLS certificate..."

# Build SAN list — always include localhost and Docker service names.
# If HOST_IP env var is set, also include the server's public IP.
SAN_LIST="DNS:localhost,DNS:product-service,DNS:order-service"
if [ -n "${HOST_IP:-}" ]; then
  SAN_LIST="$SAN_LIST,IP:$HOST_IP"
  echo "Including public IP in SAN: $HOST_IP"
fi

openssl req -x509 -nodes -days 730 \
    -newkey rsa:2048 \
    -keyout "$SCRIPT_DIR/server.key" \
    -out "$SCRIPT_DIR/server.crt" \
    -subj "/CN=eshop-benchmark" \
    -addext "subjectAltName=$SAN_LIST"

openssl pkcs12 -export \
    -out "$PFX_PATH" \
    -inkey "$SCRIPT_DIR/server.key" \
    -in "$SCRIPT_DIR/server.crt" \
    -passout "pass:$PFX_PASSWORD"

# Clean up intermediate files
rm -f "$SCRIPT_DIR/server.key" "$SCRIPT_DIR/server.crt"

echo "Certificate generated: $PFX_PATH"
echo "SANs: localhost, product-service, order-service"
echo "Password: $PFX_PASSWORD"
