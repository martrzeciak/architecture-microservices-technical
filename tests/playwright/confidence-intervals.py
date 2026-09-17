# -*- coding: utf-8 -*-
"""
Confidence intervals for the browser benchmark.

Unit of analysis: the median of a single run (5 independent replications of the
whole matrix). Within-run samples are not independent (shared cache state,
shared browser context, clustered network stalls), so the run median is the
correct unit -- this also matches the experiment design principles of Jain.

Estimator: mean of run medians +/- t(0.975, 4) * s / sqrt(5), with s the sample
standard deviation (n-1 divisor). Protocol differences are paired within a run,
which removes between-run variability.

Run:  python confidence-intervals.py
Deps: numpy (matplotlib not required)

Output: console table + LaTeX table body ready to paste into chapter 7.
"""
import json
import math
import os

HERE = os.path.dirname(os.path.abspath(__file__))
RESULT_FILE = os.path.join(
    HERE, '..', '..', 'hetzner-results',
    'browser-benchmark-2026-08-16T13-47-21.json')

# Student's t, two-sided 95%, 4 degrees of freedom (5 runs).
T_CRIT = 2.776
RUNS_EXPECTED = 5

with open(RESULT_FILE, 'r', encoding='utf-8') as f:
    DATA = json.load(f)
CELLS = DATA['cells']

PROTO_LABEL = {
    'rest': 'REST',
    'grpc_web_envoy': 'Envoy',
    'grpc_web_direct': 'Direct',
}

# Configurations underpinning the conclusions of the thesis.
CONFIGS = [
    ('Echo, n=200, VU=10', 'echo_VU10_N200'),
    ('Echo, n=500, VU=10', 'echo_VU10_N500'),
    ('Echo, n=2000, VU=10', 'echo_VU10_N2000'),
    ('Echo, n=500, VU=20', 'echo_VU20_N500'),
    ('Products, PS=200, VU=10', 'products_VU10_PS200_CACHEWARM'),
    ('Products, PS=500, VU=10', 'products_VU10_PS500_CACHEWARM'),
    ('Products, PS=1000, VU=10', 'products_VU10_PS1000_CACHEWARM'),
    ('Products, PS=2000, VU=10', 'products_VU10_PS2000_CACHEWARM'),
    ('Products, PS=500, VU=20', 'products_VU20_PS500_CACHEWARM'),
    ('Products, PS=1000, VU=20', 'products_VU20_PS1000_CACHEWARM'),
    ('Orders, poz.=10, VU=10', 'orders_VU10_IT10'),
    ('Orders, poz.=50, VU=10', 'orders_VU10_IT50'),
    ('Orders, poz.=10, VU=20', 'orders_VU20_IT10'),
    ('Orders, poz.=50, VU=20', 'orders_VU20_IT50'),
]


def run_medians(cell_key, proto):
    """Median of every run for one protocol in one cell."""
    cell = CELLS[cell_key]
    p = cell['protocols'][proto]
    return [r['med'] for r in p['per_run']]


def mean_sd(values):
    n = len(values)
    mean = sum(values) / n
    if n < 2:
        return mean, 0.0
    var = sum((v - mean) ** 2 for v in values) / (n - 1)
    return mean, math.sqrt(var)


def ci(values):
    """Mean, sample SD and half-width of the two-sided 95% CI."""
    n = len(values)
    mean, sd = mean_sd(values)
    half = T_CRIT * sd / math.sqrt(n)
    return mean, sd, half


def paired_ci(a, b):
    """CI for the mean paired difference a-b (same run in both series)."""
    diffs = [x - y for x, y in zip(a, b)]
    return ci(diffs)


def fmt(value, half):
    return f'{value:.1f} +/- {half:.1f}'


def tex(value, half):
    return f'{value:.1f} $\\pm$ {half:.1f}'.replace('.', ',')


print('Przedzialy ufnosci 95% dla wartosci centralnej (n = 5 przebiegow, t = 2,776)')
print('=' * 104)
header = f'{"Konfiguracja":26s} ' + ' '.join(f'{PROTO_LABEL[p]:>18s}' for p in PROTO_LABEL)
print(header)
print('-' * 104)

rows_central = []
rows_diff = []

for label, key in CONFIGS:
    if key not in CELLS:
        print(f'{label:26s} BRAK KOMORKI {key}')
        continue

    cell_out = []
    series = {}
    for proto in PROTO_LABEL:
        vals = run_medians(key, proto)
        if len(vals) != RUNS_EXPECTED:
            print(f'  uwaga: {key}/{proto} ma {len(vals)} przebiegow')
        series[proto] = vals
        mean, sd, half = ci(vals)
        cell_out.append(fmt(mean, half))
    print(f'{label:26s} ' + ' '.join(f'{c:>18s}' for c in cell_out))

    rows_central.append((label, key, series))

    # paired differences against REST
    d_env = paired_ci(series['rest'], series['grpc_web_envoy'])
    d_dir = paired_ci(series['rest'], series['grpc_web_direct'])
    d_ed = paired_ci(series['grpc_web_envoy'], series['grpc_web_direct'])
    rows_diff.append((label, d_env, d_dir, d_ed))

print()
print('Przedzialy ufnosci 95% dla roznic sparowanych w obrebie przebiegu [ms]')
print('=' * 104)
print(f'{"Konfiguracja":26s} {"REST-Envoy":>20s} {"REST-Direct":>20s} {"Envoy-Direct":>20s}   rozstrzygniete')
print('-' * 104)

for label, d_env, d_dir, d_ed in rows_diff:
    marks = []
    for mean, sd, half in (d_env, d_dir, d_ed):
        # CI excludes zero -> difference resolved at the 95% level
        marks.append('tak' if abs(mean) > half else 'nie')
    print(f'{label:26s} '
          f'{fmt(d_env[0], d_env[2]):>20s} '
          f'{fmt(d_dir[0], d_dir[2]):>20s} '
          f'{fmt(d_ed[0], d_ed[2]):>20s}   '
          + '/'.join(marks))

print()
print('=' * 104)
print('LaTeX -- wiersze tabeli wartosci centralnych')
print('=' * 104)
for label, key, series in rows_central:
    cols = []
    for proto in PROTO_LABEL:
        mean, sd, half = ci(series[proto])
        cols.append(tex(mean, half))
    print(f'{label} & ' + ' & '.join(cols) + ' \\\\')

print()
print('=' * 104)
print('LaTeX -- wiersze tabeli roznic sparowanych')
print('=' * 104)
for label, d_env, d_dir, d_ed in rows_diff:
    resolved = 'tak' if abs(d_dir[0]) > d_dir[2] else 'nie'
    print(f'{label} & '
          f'{tex(d_env[0], d_env[2])} & '
          f'{tex(d_dir[0], d_dir[2])} & '
          f'{tex(d_ed[0], d_ed[2])} & {resolved} \\\\')


# --------------------------------------------------------------------------
# Wiersze tabeli "Products, ciepla pamiec podreczna" z wspolczynnikiem
# zmiennosci median przebiegow (kolumna w nawiasach).
# --------------------------------------------------------------------------
print()
print('=' * 104)
print('LaTeX -- Products CACHEWARM: mediana (CV) dla kazdego protokolu')
print('=' * 104)
for vu in (10, 20):
    for ps in (10, 100, 200, 500, 1000, 2000):
        key = f'products_VU{vu}_PS{ps}_CACHEWARM'
        if key not in CELLS:
            continue
        cols = []
        for proto in PROTO_LABEL:
            st = CELLS[key]['protocols'][proto]
            med = st['stats']['med']
            cvv = st['inter_run']['cv']
            cols.append(f'{med:.1f} ({cvv:.1f}\\%)'.replace('.', ','))
        rest = CELLS[key]['protocols']['rest']['stats']['med']
        direct = CELLS[key]['protocols']['grpc_web_direct']['stats']['med']
        delta = 100.0 * (direct - rest) / rest
        print(f'{vu} & {ps} & ' + ' & '.join(cols)
              + f' & {delta:.0f}\\%'.replace('.', ',') + ' \\\\')
