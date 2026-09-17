# -*- coding: utf-8 -*-
"""Test rownolegle: 10 uzytkownikow x 10 prob, REST vs gRPC-Web Envoy vs Direct.
Symuluje 10 jednoczesnych przegladarek strzelajacych do serwera Hetzner."""
import time, statistics, ssl, urllib.request, struct, threading, sys

SERVER = "167.233.253.101"
VU = 10
ITER = 10

# Budowa ramki gRPC-Web: ListProductsRequest { page=1, page_size=100 }
PROTO = bytes([0x10, 0x01, 0x18, 0x64])
FRAME = struct.pack(">bI", 0, len(PROTO)) + PROTO

CTX = ssl._create_unverified_context()

TARGETS = {
    "REST": {
        "url": f"http://{SERVER}:5000/api/products?page=1&pageSize=100",
        "method": "GET",
        "body": None,
        "headers": {},
    },
    "gRPC-Web Envoy": {
        "url": f"http://{SERVER}:8080/product.ProductService/ListProducts",
        "method": "POST",
        "body": FRAME,
        "headers": {"Content-Type": "application/grpc-web+proto", "X-Grpc-Web": "1"},
    },
    "gRPC-Web Direct": {
        "url": f"https://{SERVER}:5002/product.ProductService/ListProducts",
        "method": "POST",
        "body": FRAME,
        "headers": {"Content-Type": "application/grpc-web+proto", "X-Grpc-Web": "1"},
    },
}


def worker(target, results):
    """Jeden 'uzytkownik' - robi ITER prob sekwencyjnie."""
    cfg = TARGETS[target]
    for _ in range(ITER):
        req = urllib.request.Request(cfg["url"], data=cfg["body"], method=cfg["method"])
        for k, v in cfg["headers"].items():
            req.add_header(k, v)
        start = time.perf_counter()
        try:
            with urllib.request.urlopen(req, context=CTX, timeout=15) as r:
                r.read()
        except Exception as e:
            results.append(-1)
            continue
        elapsed_ms = (time.perf_counter() - start) * 1000
        results.append(elapsed_ms)


def run_test(target):
    # Warmup (3 proby sekwencyjnie)
    warmup = []
    worker(target, warmup)

    # Glowny test: VU watkow rownoczesnie
    results = []
    threads = []
    for _ in range(VU):
        t = threading.Thread(target=worker, args=(target, results))
        threads.append(t)
    
    start_all = time.perf_counter()
    for t in threads:
        t.start()
    for t in threads:
        t.join()
    total_sec = time.perf_counter() - start_all

    valid = [r for r in results if r > 0]
    if not valid:
        print(f"  {target}: ZERO udanych pomiarow!")
        return

    valid.sort()
    n = len(valid)
    avg = statistics.mean(valid)
    med = valid[n // 2]
    p95 = valid[int(n * 0.95)]
    mn = valid[0]
    mx = valid[-1]
    rps = n / total_sec

    print(f"  {target:20s} | n={n:3d} | min={mn:5.0f} ms | avg={avg:5.1f} ms | "
          f"med={med:5.0f} ms | p95={p95:5.0f} ms | max={mx:5.0f} ms | "
          f"throughput={rps:.0f} req/s")


if __name__ == "__main__":
    print(f"=== TEST ROWNOLEGLE: {VU} uzytkownikow x {ITER} prob = {VU*ITER} pomiarow/protokol ===")
    print(f"=== Cel: {SERVER} (Hetzner, Norymberga) ===")
    print()
    for target in TARGETS:
        run_test(target)
    print()
    print("Gotowe.")
