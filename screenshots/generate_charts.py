"""
Generate benchmark charts from k6 JSON results.
Run: python generate_charts.py
Requires: pip install matplotlib numpy
"""
import json
import os
import matplotlib.pyplot as plt
import numpy as np

RESULTS_DIR = os.path.join(os.path.dirname(__file__), '..', 'hetzner-results')
OUTPUT_DIR = os.path.dirname(__file__)

# Helper to load metric from k6 summary JSON
def load_metric(filename, metric_path='http_req_duration'):
    filepath = os.path.join(RESULTS_DIR, filename)
    with open(filepath, 'r') as f:
        data = json.load(f)
    metrics = data.get('metrics', {})
    m = metrics.get(metric_path, {})
    return {
        'avg': m.get('avg', 0),
        'med': m.get('med', 0),
        'p95': m.get('p(95)', 0),
        'p99': m.get('p(99)', 0),
    }

def load_throughput(filename):
    filepath = os.path.join(RESULTS_DIR, filename)
    with open(filepath, 'r') as f:
        data = json.load(f)
    metrics = data.get('metrics', {})
    reqs = metrics.get('http_reqs', metrics.get('iterations', {}))
    return reqs.get('rate', 0)

# Color scheme
COLORS = {
    'REST': '#2196F3',
    'gRPC-Web Envoy': '#FF9800',
    'gRPC-Web Direct': '#4CAF50',
    'gRPC Native': '#9C27B0',
}

PROTOCOLS = ['REST', 'gRPC-Web Envoy', 'gRPC-Web Direct', 'gRPC Native']
TIMESTAMP = '20260805_122120'

plt.style.use('seaborn-v0_8-whitegrid')
plt.rcParams['figure.dpi'] = 150
plt.rcParams['font.size'] = 10

# ============================================================
# Chart 1: Echo Latency Comparison (VU=100, different COUNT)
# ============================================================
fig, axes = plt.subplots(1, 2, figsize=(14, 6))

counts = [10, 100, 200]
echo_data = {}
for count in counts:
    echo_data[count] = {
        'REST': load_metric(f'scenario-echo-rest_VU100_COUNT{count}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Envoy': load_metric(f'scenario-echo-grpc-envoy_VU100_COUNT{count}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Direct': load_metric(f'scenario-echo-grpc-direct_VU100_COUNT{count}_run1_{TIMESTAMP}-summary.json'),
        'gRPC Native': load_metric(f'scenario-echo-grpc-native_VU100_COUNT{count}_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration'),
    }

# Bar chart: avg latency per protocol at COUNT=100
ax = axes[0]
x = np.arange(len(PROTOCOLS))
avgs = [echo_data[100][p]['avg'] for p in PROTOCOLS]
bars = ax.bar(x, avgs, color=[COLORS[p] for p in PROTOCOLS], width=0.6)
ax.set_xticks(x)
ax.set_xticklabels(PROTOCOLS, rotation=15, ha='right')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Echo: Average Latency (VU=100, COUNT=100)')
for bar, val in zip(bars, avgs):
    ax.text(bar.get_x() + bar.get_width()/2, bar.get_height() + 0.5, f'{val:.1f}ms', ha='center', va='bottom', fontsize=9)

# Line chart: latency scaling with payload size
ax = axes[1]
for protocol in PROTOCOLS:
    latencies = [echo_data[c][protocol]['avg'] for c in counts]
    ax.plot(counts, latencies, marker='o', label=protocol, color=COLORS[protocol], linewidth=2)
ax.set_xlabel('Response Size (number of items)')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Echo: Latency Scaling with Payload Size (VU=100)')
ax.legend(loc='upper left')
ax.set_xticks(counts)

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_echo_latency.png'), bbox_inches='tight')
print('Saved: chart_echo_latency.png')

# ============================================================
# Chart 2: Echo Throughput Comparison
# ============================================================
fig, ax = plt.subplots(figsize=(10, 6))

vu_levels = [10, 100, 500]
throughput_data = {}
for vu in vu_levels:
    throughput_data[vu] = {
        'REST': load_throughput(f'scenario-echo-rest_VU{vu}_COUNT100_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Envoy': load_throughput(f'scenario-echo-grpc-envoy_VU{vu}_COUNT100_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Direct': load_throughput(f'scenario-echo-grpc-direct_VU{vu}_COUNT100_run1_{TIMESTAMP}-summary.json'),
        'gRPC Native': load_throughput(f'scenario-echo-grpc-native_VU{vu}_COUNT100_run1_{TIMESTAMP}-summary.json'),
    }

x = np.arange(len(vu_levels))
width = 0.2
for i, protocol in enumerate(PROTOCOLS):
    values = [throughput_data[vu][protocol] for vu in vu_levels]
    ax.bar(x + i * width, values, width, label=protocol, color=COLORS[protocol])

ax.set_xticks(x + width * 1.5)
ax.set_xticklabels([f'VU={vu}' for vu in vu_levels])
ax.set_ylabel('Throughput (req/s)')
ax.set_title('Echo: Throughput by VU Level (COUNT=100)')
ax.legend()

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_echo_throughput.png'), bbox_inches='tight')
print('Saved: chart_echo_throughput.png')

# ============================================================
# Chart 3: Products Latency — Warm vs Cold Cache
# ============================================================
fig, axes = plt.subplots(1, 2, figsize=(14, 6))

for idx, (cache, title) in enumerate([('CACHEWARM', 'Warm Cache (Redis hit)'), ('CACHECOLD', 'Cold Cache (DB query)')]):
    ax = axes[idx]
    data_ps100 = {
        'REST': load_metric(f'scenario-products-rest_VU100_PS100_{cache}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Envoy': load_metric(f'scenario-products-grpc-envoy_VU100_PS100_{cache}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Direct': load_metric(f'scenario-products-grpc-direct_VU100_PS100_{cache}_run1_{TIMESTAMP}-summary.json'),
        'gRPC Native': load_metric(f'scenario-products-grpc-native_VU100_PS100_{cache}_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration'),
    }

    x = np.arange(len(PROTOCOLS))
    avgs = [data_ps100[p]['avg'] for p in PROTOCOLS]
    p95s = [data_ps100[p]['p95'] for p in PROTOCOLS]

    bars = ax.bar(x - 0.15, avgs, 0.3, label='Avg', color=[COLORS[p] for p in PROTOCOLS], alpha=0.8)
    ax.bar(x + 0.15, p95s, 0.3, label='p95', color=[COLORS[p] for p in PROTOCOLS], alpha=0.4)

    ax.set_xticks(x)
    ax.set_xticklabels(PROTOCOLS, rotation=15, ha='right')
    ax.set_ylabel('Latency (ms)')
    ax.set_title(f'Products: VU=100, PS=100 — {title}')
    ax.legend()

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_products_cache.png'), bbox_inches='tight')
print('Saved: chart_products_cache.png')

# ============================================================
# Chart 4: Orders Latency — VU=100 vs VU=500
# ============================================================
fig, axes = plt.subplots(1, 2, figsize=(14, 6))

for idx, (vu, oi) in enumerate([(100, 1), (500, 10)]):
    ax = axes[idx]
    data_orders = {
        'REST': load_metric(f'scenario-orders-rest_VU{vu}_OI{oi}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Envoy': load_metric(f'scenario-orders-grpc-envoy_VU{vu}_OI{oi}_run1_{TIMESTAMP}-summary.json'),
        'gRPC-Web Direct': load_metric(f'scenario-orders-grpc-direct_VU{vu}_OI{oi}_run1_{TIMESTAMP}-summary.json'),
        'gRPC Native': load_metric(f'scenario-orders-grpc-native_VU{vu}_OI{oi}_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration'),
    }

    x = np.arange(len(PROTOCOLS))
    avgs = [data_orders[p]['avg'] for p in PROTOCOLS]
    p95s = [data_orders[p]['p95'] for p in PROTOCOLS]

    ax.bar(x - 0.15, avgs, 0.3, label='Avg', color=[COLORS[p] for p in PROTOCOLS], alpha=0.8)
    ax.bar(x + 0.15, p95s, 0.3, label='p95', color=[COLORS[p] for p in PROTOCOLS], alpha=0.4)

    ax.set_xticks(x)
    ax.set_xticklabels(PROTOCOLS, rotation=15, ha='right')
    ax.set_ylabel('Latency (ms)')
    ax.set_title(f'Orders: VU={vu}, OrderItems={oi}')
    ax.legend()

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_orders_latency.png'), bbox_inches='tight')
print('Saved: chart_orders_latency.png')

# ============================================================
# Chart 5: Products VU Scaling (PS=100, WARM)
# ============================================================
fig, ax = plt.subplots(figsize=(10, 6))

for protocol in PROTOCOLS:
    latencies = []
    for vu in vu_levels:
        if protocol == 'gRPC Native':
            m = load_metric(f'scenario-products-grpc-native_VU{vu}_PS100_CACHEWARM_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration')
        elif protocol == 'gRPC-Web Envoy':
            m = load_metric(f'scenario-products-grpc-envoy_VU{vu}_PS100_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        elif protocol == 'gRPC-Web Direct':
            m = load_metric(f'scenario-products-grpc-direct_VU{vu}_PS100_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        else:
            m = load_metric(f'scenario-products-rest_VU{vu}_PS100_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        latencies.append(m['avg'])
    ax.plot(vu_levels, latencies, marker='o', label=protocol, color=COLORS[protocol], linewidth=2)

ax.set_xlabel('Virtual Users (VU)')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Products: Latency Scaling with Load (PS=100, Warm Cache)')
ax.legend()
ax.set_xticks(vu_levels)

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_products_scaling.png'), bbox_inches='tight')
print('Saved: chart_products_scaling.png')

# ============================================================
# Chart 6: Native Deserialization Overhead Evidence
# ============================================================
fig, ax = plt.subplots(figsize=(10, 6))

counts = [10, 100, 200]
rest_avgs = [echo_data[c]['REST']['avg'] for c in counts]
native_avgs = [echo_data[c]['gRPC Native']['avg'] for c in counts]

ax.plot(counts, rest_avgs, marker='s', label='REST (k6/http — no body parsing)', color=COLORS['REST'], linewidth=2)
ax.plot(counts, native_avgs, marker='o', label='gRPC Native (k6/grpc — full protobuf deserialization)', color=COLORS['gRPC Native'], linewidth=2)

# Add annotations
for i, c in enumerate(counts):
    ratio = native_avgs[i] / rest_avgs[i]
    ax.annotate(f'{ratio:.1f}x', (c, native_avgs[i]), textcoords="offset points", xytext=(10, 5), fontsize=9)

ax.set_xlabel('Response Size (number of items)')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Evidence: k6 gRPC Module Deserialization Overhead (VU=100)')
ax.legend()
ax.set_xticks(counts)

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_native_overhead.png'), bbox_inches='tight')
print('Saved: chart_native_overhead.png')

print('\n✓ All charts generated in:', OUTPUT_DIR)


# ============================================================
# Chart 7: Percentiles Comparison — Echo VU=100, COUNT=100
# ============================================================
fig, ax = plt.subplots(figsize=(12, 6))

percentile_labels = ['Median', 'p90', 'p95', 'p99']
x = np.arange(len(percentile_labels))
width = 0.2

for i, protocol in enumerate(PROTOCOLS):
    if protocol == 'gRPC Native':
        m = load_metric(f'scenario-echo-grpc-native_VU100_COUNT100_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration')
    elif protocol == 'gRPC-Web Envoy':
        m = load_metric(f'scenario-echo-grpc-envoy_VU100_COUNT100_run1_{TIMESTAMP}-summary.json')
    elif protocol == 'gRPC-Web Direct':
        m = load_metric(f'scenario-echo-grpc-direct_VU100_COUNT100_run1_{TIMESTAMP}-summary.json')
    else:
        m = load_metric(f'scenario-echo-rest_VU100_COUNT100_run1_{TIMESTAMP}-summary.json')

    values = [m['med'], m.get('p90', 0), m['p95'], m['p99']]
    # Load p90 separately since our helper doesn't grab it
    filepath = os.path.join(RESULTS_DIR, f'scenario-echo-{"rest" if protocol == "REST" else "grpc-envoy" if protocol == "gRPC-Web Envoy" else "grpc-direct" if protocol == "gRPC-Web Direct" else "grpc-native"}_VU100_COUNT100_run1_{TIMESTAMP}-summary.json')
    with open(filepath, 'r') as f:
        raw = json.load(f)
    metric_key = 'grpc_req_duration' if protocol == 'gRPC Native' else 'http_req_duration'
    raw_m = raw['metrics'][metric_key]
    values = [raw_m['med'], raw_m['p(90)'], raw_m['p(95)'], raw_m['p(99)']]

    bars = ax.bar(x + i * width, values, width, label=protocol, color=COLORS[protocol])

ax.set_xticks(x + width * 1.5)
ax.set_xticklabels(percentile_labels)
ax.set_ylabel('Latency (ms)')
ax.set_title('Echo: Latency Distribution (VU=100, COUNT=100) — Median, p90, p95, p99')
ax.legend()

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_echo_percentiles.png'), bbox_inches='tight')
print('Saved: chart_echo_percentiles.png')

# ============================================================
# Chart 8: Products — PageSize Impact (VU=100, Warm Cache)
# ============================================================
fig, ax = plt.subplots(figsize=(10, 6))

page_sizes = [10, 100, 200]
for protocol in PROTOCOLS:
    latencies = []
    for ps in page_sizes:
        if protocol == 'gRPC Native':
            m = load_metric(f'scenario-products-grpc-native_VU100_PS{ps}_CACHEWARM_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration')
        elif protocol == 'gRPC-Web Envoy':
            m = load_metric(f'scenario-products-grpc-envoy_VU100_PS{ps}_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        elif protocol == 'gRPC-Web Direct':
            m = load_metric(f'scenario-products-grpc-direct_VU100_PS{ps}_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        else:
            m = load_metric(f'scenario-products-rest_VU100_PS{ps}_CACHEWARM_run1_{TIMESTAMP}-summary.json')
        latencies.append(m['avg'])
    ax.plot(page_sizes, latencies, marker='o', label=protocol, color=COLORS[protocol], linewidth=2)

ax.set_xlabel('Page Size (number of products)')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Products: Latency vs Response Size (VU=100, Warm Cache)')
ax.legend()
ax.set_xticks(page_sizes)

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_products_pagesize.png'), bbox_inches='tight')
print('Saved: chart_products_pagesize.png')

# ============================================================
# Chart 9: Orders — OI=1 vs OI=10 (VU=100)
# ============================================================
fig, ax = plt.subplots(figsize=(10, 6))

order_items = [1, 10]
x = np.arange(len(PROTOCOLS))
width = 0.35

for idx, oi in enumerate(order_items):
    avgs = []
    for protocol in PROTOCOLS:
        if protocol == 'gRPC Native':
            m = load_metric(f'scenario-orders-grpc-native_VU100_OI{oi}_run1_{TIMESTAMP}-summary.json', 'grpc_req_duration')
        elif protocol == 'gRPC-Web Envoy':
            m = load_metric(f'scenario-orders-grpc-envoy_VU100_OI{oi}_run1_{TIMESTAMP}-summary.json')
        elif protocol == 'gRPC-Web Direct':
            m = load_metric(f'scenario-orders-grpc-direct_VU100_OI{oi}_run1_{TIMESTAMP}-summary.json')
        else:
            m = load_metric(f'scenario-orders-rest_VU100_OI{oi}_run1_{TIMESTAMP}-summary.json')
        avgs.append(m['avg'])
    ax.bar(x + idx * width, avgs, width, label=f'OrderItems={oi}', alpha=0.8 if idx == 0 else 0.5,
           color=[COLORS[p] for p in PROTOCOLS], edgecolor='black' if idx == 1 else 'none', linewidth=0.5)

ax.set_xticks(x + width / 2)
ax.set_xticklabels(PROTOCOLS, rotation=15, ha='right')
ax.set_ylabel('Avg Latency (ms)')
ax.set_title('Orders: Impact of Payload Size — OI=1 vs OI=10 (VU=100)')

# Custom legend
from matplotlib.patches import Patch
legend_elements = [Patch(facecolor='gray', alpha=0.8, label='OrderItems=1'),
                   Patch(facecolor='gray', alpha=0.5, edgecolor='black', linewidth=0.5, label='OrderItems=10')]
ax.legend(handles=legend_elements)

plt.tight_layout()
plt.savefig(os.path.join(OUTPUT_DIR, 'chart_orders_payload_impact.png'), bbox_inches='tight')
print('Saved: chart_orders_payload_impact.png')

print('\n✓ All 9 charts generated in:', OUTPUT_DIR)
