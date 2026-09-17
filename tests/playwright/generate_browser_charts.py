# -*- coding: utf-8 -*-
"""
Generate charts for the thesis (chapter 7) from the browser benchmark result file.

Run:  python generate_browser_charts.py
Deps: pip install matplotlib numpy

Input : ../../hetzner-results/browser-benchmark-2026-08-16T13-47-21.json
Output: ../../../architecture-microservices-tex/figures/chart_browser_*.png
"""
import json
import os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
RESULT_FILE = os.path.join(
    HERE, '..', '..', 'hetzner-results',
    'browser-benchmark-2026-08-16T13-47-21.json')
OUTPUT_DIR = os.path.join(
    HERE, '..', '..', '..', 'architecture-microservices-tex', 'figures')

PROTOCOLS = [
    ('rest', 'REST (JSON, HTTP/1.1)', '#2196F3', 'o'),
    ('grpc_web_envoy', 'gRPC-Web (Envoy, HTTP/1.1)', '#FF9800', 's'),
    ('grpc_web_direct', 'gRPC-Web (Direct, HTTP/2)', '#4CAF50', '^'),
]

RTT_MS = 26.2

with open(RESULT_FILE, 'r', encoding='utf-8') as f:
    DATA = json.load(f)
CELLS = DATA['cells']


def med(cell_key, proto):
    """Mediana [ms] po wszystkich przebiegach, po odrzuceniu zaciec."""
    cell = CELLS.get(cell_key)
    if cell is None:
        return None
    p = cell['protocols'].get(proto)
    if p is None:
        return None
    return p['stats']['med']


def cv(cell_key, proto):
    """Wspolczynnik zmiennosci median poszczegolnych przebiegow [%]."""
    cell = CELLS.get(cell_key)
    if cell is None:
        return None
    p = cell['protocols'].get(proto)
    if p is None:
        return None
    return p.get('inter_run', {}).get('cv')


# Student's t, two-sided 95%, 4 degrees of freedom (5 runs).
T_CRIT = 2.776


def ci_half(cell_key, proto):
    """Polowkowa szerokosc 95% przedzialu ufnosci dla wartosci centralnej.

    Jednostka analizy: mediana pojedynczego przebiegu (probki w obrebie
    przebiegu nie sa niezalezne). Patrz rozdzial o metodyce.
    """
    cell = CELLS.get(cell_key)
    if cell is None:
        return 0.0
    p = cell['protocols'].get(proto)
    if p is None:
        return 0.0
    meds = [r['med'] for r in p.get('per_run', [])]
    k = len(meds)
    if k < 2:
        return 0.0
    mean = sum(meds) / k
    var = sum((m - mean) ** 2 for m in meds) / (k - 1)
    return T_CRIT * (var ** 0.5) / (k ** 0.5)


plt.style.use('seaborn-v0_8-whitegrid')
plt.rcParams['figure.dpi'] = 150
plt.rcParams['font.size'] = 10

os.makedirs(OUTPUT_DIR, exist_ok=True)


def save(fig, name):
    path = os.path.join(OUTPUT_DIR, name)
    fig.savefig(path, bbox_inches='tight')
    plt.close(fig)
    print('zapisano:', os.path.normpath(path))


# ==========================================================================
# Wykres 1: Echo, VU=10 - skokowy wzrost opoznienia wraz z rozmiarem odpowiedzi
# ==========================================================================
echo_sizes = [10, 100, 200, 500, 2000]
# Plotno 9x5 cala dla wykresow jednopanelowych; w pracy wstawiane jako
# 0.7\textwidth. Wykresy dwupanelowe maja 13x5 i sa wstawiane na pelna
# szerokosc. Iloraz szerokosci plotna do szerokosci wstawienia jest w obu
# przypadkach taki sam, dzieki czemu napisy na wszystkich rysunkach maja
# w PDF identyczna wielkosc.
fig, ax = plt.subplots(figsize=(9, 5))
x = np.arange(len(echo_sizes))

for proto, label, color, marker in PROTOCOLS:
    ys, es = [], []
    for n in echo_sizes:
        key = f'echo_VU10_N{n}'
        ys.append(med(key, proto))
        es.append(ci_half(key, proto))
    ax.errorbar(x, ys, yerr=es, label=label, color=color, marker=marker,
                capsize=3, linewidth=1.8, markersize=6)

ax.set_xticks(x)
ax.set_xticklabels([str(n) for n in echo_sizes])
ax.set_xlabel('Liczba rekordów w odpowiedzi')
ax.set_ylabel('Mediana opóźnienia [ms]')
ax.set_title('Scenariusz Echo, 10 równoległych klientów')
ax.legend(loc='upper left')

# adnotacja: skok o jeden obieg pakietu miedzy 200 i 500 rekordami dla REST
r200, r500 = med('echo_VU10_N200', 'rest'), med('echo_VU10_N500', 'rest')
if r200 and r500:
    ax.annotate(f'+{r500 - r200:.1f} ms\n(jeden obieg pakietu, RTT = {RTT_MS} ms)',
                xy=(3, r500), xytext=(1.15, r500 + 26),
                arrowprops=dict(arrowstyle='->', color='#555555'),
                fontsize=9, color='#333333')
save(fig, 'chart_browser_echo.png')


# ==========================================================================
# Wykres 2: Products (ciepla pamiec podreczna) - mediana wzgledem rozmiaru strony
# ==========================================================================
page_sizes = [10, 100, 200, 500, 1000, 2000]
fig, axes = plt.subplots(1, 2, figsize=(13, 5))

for ax, vu, sizes in ((axes[0], 10, page_sizes), (axes[1], 20, page_sizes[:-1])):
    xs = np.arange(len(sizes))
    width = 0.26
    for i, (proto, label, color, _m) in enumerate(PROTOCOLS):
        ys, es = [], []
        for ps in sizes:
            key = f'products_VU{vu}_PS{ps}_CACHEWARM'
            ys.append(med(key, proto) or 0)
            es.append(ci_half(key, proto))
        bars = ax.bar(xs + (i - 1) * width, ys, width, label=label, color=color,
                      yerr=es, capsize=3,
                      error_kw={'elinewidth': 1, 'ecolor': '#555555'})
        ax.bar_label(bars, fmt='%.0f', fontsize=7, padding=3)
    ax.set_xticks(xs)
    ax.set_xticklabels([str(s) for s in sizes])
    ax.set_xlabel('Rozmiar strony wyników [rekordów]')
    ax.set_ylabel('Mediana opóźnienia [ms]')
    ax.set_title(f'{vu} równoległych klientów')
    ax.set_ylim(0, 180)

axes[0].legend(loc='upper left', fontsize=9)
fig.suptitle('Scenariusz Products, ciepła pamięć podręczna', y=1.0)
save(fig, 'chart_browser_products.png')


# ==========================================================================
# Wykres 3: dekompozycja przewagi na skladnik serializacyjny i transportowy
# ==========================================================================
configs = [
    ('Echo\n2000 rek., VU=10', 'echo_VU10_N2000'),
    ('Products\nPS=500, VU=10', 'products_VU10_PS500_CACHEWARM'),
    ('Products\nPS=1000, VU=10', 'products_VU10_PS1000_CACHEWARM'),
    ('Products\nPS=2000, VU=10', 'products_VU10_PS2000_CACHEWARM'),
    ('Products\nPS=500, VU=20', 'products_VU20_PS500_CACHEWARM'),
]

labels, ser_ms, tr_ms, ser_pct, tr_pct = [], [], [], [], []
for label, key in configs:
    rest = med(key, 'rest')
    envoy = med(key, 'grpc_web_envoy')
    direct = med(key, 'grpc_web_direct')
    labels.append(label)
    ser_ms.append(rest - envoy)
    tr_ms.append(envoy - direct)
    ser_pct.append(100.0 * (rest - envoy) / rest)
    tr_pct.append(100.0 * (envoy - direct) / rest)

fig, ax = plt.subplots(figsize=(9, 5))
xs = np.arange(len(labels))
ax.bar(xs, ser_ms, 0.5, color='#FF9800',
       label='Serializacja binarna (REST $\\rightarrow$ Envoy)')
ax.bar(xs, tr_ms, 0.5, bottom=ser_ms, color='#4CAF50',
       label='HTTP/2, TLS i pominięcie proxy (Envoy $\\rightarrow$ Direct)')

for i in range(len(labels)):
    ax.text(xs[i], ser_ms[i] / 2, f'{ser_ms[i]:.1f} ms\n({ser_pct[i]:.0f}\u2009%)',
            ha='center', va='center', fontsize=8)
    ax.text(xs[i], ser_ms[i] + tr_ms[i] + 0.7,
            f'{tr_ms[i]:.1f} ms ({tr_pct[i]:.0f}\u2009%)',
            ha='center', va='bottom', fontsize=8, color='#2E7D32')

ax.set_xticks(xs)
ax.set_xticklabels(labels, fontsize=9)
ax.set_ylabel('Skrócenie czasu odpowiedzi względem REST [ms]')
ax.set_title('Dekompozycja przewagi gRPC-Web na składnik serializacyjny i transportowy')
ax.legend(loc='upper right', fontsize=9)
ax.set_ylim(0, max(np.array(ser_ms) + np.array(tr_ms)) * 1.3)
save(fig, 'chart_browser_dekompozycja.png')


# ==========================================================================
# Wykres 4: nasycenie stanowiska pomiarowego
# Zrodlo: odrebny pomiar pilotazowy (PS=100, ciepla pamiec podreczna),
# wartosci zgodne z tabela "Skalowanie z liczba rownoleglych klientow".
# ==========================================================================
SCALING = {
    'vu': [10, 20, 30, 50],
    'latency': {
        'rest': [38.1, 50.9, 64.8, 140.3],
        'grpc_web_envoy': [41.4, 50.0, 61.8, 122.5],
        'grpc_web_direct': [39.5, 44.5, 54.9, 113.2],
    },
    'throughput': {
        'rest': [40.7, 40.8, 72.3, 75.4],
        'grpc_web_envoy': [40.7, 58.7, 69.9, 63.8],
        'grpc_web_direct': [38.3, 59.3, 68.3, 64.6],
    },
}

fig, axes = plt.subplots(1, 2, figsize=(13, 5))
vus = SCALING['vu']

for proto, label, color, marker in PROTOCOLS:
    axes[0].plot(vus, SCALING['latency'][proto], label=label, color=color,
                 marker=marker, linewidth=1.8, markersize=6)
    axes[1].plot(vus, SCALING['throughput'][proto], label=label, color=color,
                 marker=marker, linewidth=1.8, markersize=6)

axes[0].set_xlabel('Liczba równoległych klientów')
axes[0].set_ylabel('Mediana opóźnienia [ms]')
axes[0].set_title('Opóźnienie')
axes[0].legend(loc='upper left', fontsize=9)

axes[1].set_xlabel('Liczba równoległych klientów')
axes[1].set_ylabel('Przepustowość [operacji/s]')
axes[1].set_title('Przepustowość')
axes[1].axhspan(63, 76, color='#9E9E9E', alpha=0.15)
axes[1].axvspan(20, 30, color='#F44336', alpha=0.08)
axes[1].annotate('wspólny pułap około 70 operacji/s', xy=(30, 70), xytext=(10.5, 86),
                 arrowprops=dict(arrowstyle='->', color='#555555'), fontsize=9)
axes[1].annotate('punkt przegięcia', xy=(21, 34), fontsize=9, color='#B71C1C')
axes[1].set_ylim(30, 95)

for ax in axes:
    ax.set_xticks(vus)

fig.suptitle('Nasycenie stanowiska pomiarowego (Products, PS=100, ciepła pamięć podręczna)',
             y=1.0)
save(fig, 'chart_browser_nasycenie.png')

print('\nWartosci uzyte na wykresie 3 (kontrola zgodnosci z tabela dekompozycji):')
for (label, key), s, t in zip(configs, ser_pct, tr_pct):
    r = med(key, 'rest')
    e = med(key, 'grpc_web_envoy')
    d = med(key, 'grpc_web_direct')
    print(f'  {label.replace(chr(10), " "):28s} REST={r:6.1f} Envoy={e:6.1f} '
          f'Direct={d:6.1f} serializacja=-{s:.0f}% transport=-{t:.0f}%')

# ==========================================================================
# Wykres 5: Products (zimna pamiec podreczna) - mediana wraz z przedzialem
# ufnosci. Szerokie przedzialy sa tu wnioskiem, nie usterka: pokazuja, ze
# przy malych ladunkach kolejnosc protokolow nie jest rozstrzygalna.
# ==========================================================================
cold_sizes_vu10 = [10, 100, 200, 500, 1000, 2000]
cold_sizes_vu20 = [500, 1000]
fig, axes = plt.subplots(1, 2, figsize=(13, 5))

for ax, vu, sizes in ((axes[0], 10, cold_sizes_vu10), (axes[1], 20, cold_sizes_vu20)):
    xs = np.arange(len(sizes))
    for proto, label, color, marker in PROTOCOLS:
        ys, es = [], []
        for ps in sizes:
            key = f'products_VU{vu}_PS{ps}_CACHECOLD'
            ys.append(med(key, proto))
            es.append(ci_half(key, proto))
        ax.errorbar(xs, ys, yerr=es, label=label, color=color, marker=marker,
                    capsize=3, linewidth=1.8, markersize=6)
    ax.set_xticks(xs)
    ax.set_xticklabels([str(s) for s in sizes])
    ax.set_xlabel('Rozmiar strony wyników [rekordów]')
    ax.set_ylabel('Mediana opóźnienia [ms]')
    ax.set_title(f'{vu} równoległych klientów')

axes[0].legend(loc='upper left', fontsize=9)
fig.suptitle('Scenariusz Products, zimna pamięć podręczna', y=1.0)
save(fig, 'chart_browser_products_cold.png')


# ==========================================================================
# Wykres 6: Orders - wynik negatywny. Przedzialy ufnosci nachodza na siebie
# we wszystkich konfiguracjach, co jest wlasnie trescia wniosku o braku
# wplywu protokolu na czas zapisu.
# ==========================================================================
order_items = [1, 10, 50, 200]
fig, axes = plt.subplots(1, 2, figsize=(13, 5))

for ax, vu in ((axes[0], 10), (axes[1], 20)):
    xs = np.arange(len(order_items))
    width = 0.26
    for i, (proto, label, color, _m) in enumerate(PROTOCOLS):
        ys, es = [], []
        for it in order_items:
            key = f'orders_VU{vu}_IT{it}'
            ys.append(med(key, proto) or 0)
            es.append(ci_half(key, proto))
        bars = ax.bar(xs + (i - 1) * width, ys, width, label=label, color=color,
                      yerr=es, capsize=3, error_kw={'elinewidth': 1, 'ecolor': '#555555'})
        ax.bar_label(bars, fmt='%.0f', fontsize=7, padding=3)
    ax.set_xticks(xs)
    ax.set_xticklabels([str(it) for it in order_items])
    ax.set_xlabel('Liczba pozycji zamówienia')
    ax.set_ylabel('Mediana opóźnienia [ms]')
    ax.set_title(f'{vu} równoległych klientów')
    ax.set_ylim(0, 470)

axes[0].legend(loc='upper left', fontsize=9)
fig.suptitle('Scenariusz Orders, ścieżka zapisu', y=1.0)
save(fig, 'chart_browser_orders.png')
