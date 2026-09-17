# -*- coding: utf-8 -*-
"""
Effect of HTTP body compression on the measured payload volume.

The deployed system serves responses uncompressed (no response-compression
middleware in either service, no compressor filter in Envoy), so every size
reported in the thesis is an uncompressed size. This script quantifies what
compression would change.

The Echo payload is generated deterministically in ProductService, so it can be
reconstructed byte-for-byte offline:

  id          = i.ToString()
  name        = $"Echo Product {i}"
  description = $"This is a hardcoded echo product number {i} for benchmarking
                 protocol overhead"
  price       = 9.99 * i
  categoryId  = "echo"
  stock       = 100

Reconstruction is validated against the sizes measured on the deployed system
(36 242 B for JSON and 24 104 B for protobuf at 200 records). Only after the
reconstruction matches are the compressed sizes meaningful.

Run:  python payload-compression.py
Deps: standard library only (gzip, zlib); brotli optional.
"""
import gzip
import json
import struct

WINDOW = 65536  # 64 KiB - TCP receive window measured for the link

# Sizes measured on the deployed system (chapter "Wolumen przesylanych danych").
MEASURED = {200: {'json': 36242, 'protobuf': 24104}}


def echo_records(n):
    for i in range(1, n + 1):
        yield {
            'id': str(i),
            'name': f'Echo Product {i}',
            'description': (f'This is a hardcoded echo product number {i} '
                            'for benchmarking protocol overhead'),
            'price': 9.99 * i,
            'categoryId': 'echo',
            'stock': 100,
        }


def json_payload(n):
    """System.Text.Json writes camelCase without whitespace."""
    body = {'products': list(echo_records(n)), 'totalCount': n}
    return json.dumps(body, separators=(',', ':'),
                      ensure_ascii=False).encode('utf-8')


# --- minimal protobuf writer (proto3, default values omitted) --------------

def _varint(value):
    out = bytearray()
    while True:
        bits = value & 0x7F
        value >>= 7
        if value:
            out.append(bits | 0x80)
        else:
            out.append(bits)
            return bytes(out)


def _key(field, wire):
    return _varint((field << 3) | wire)


def _string(field, text):
    raw = text.encode('utf-8')
    return _key(field, 2) + _varint(len(raw)) + raw


def _double(field, value):
    return _key(field, 1) + struct.pack('<d', value)


def _int32(field, value):
    return _key(field, 0) + _varint(value)


def _message(field, body):
    return _key(field, 2) + _varint(len(body)) + body


def protobuf_payload(n):
    """product.Product / product.ListProductsResponse from protos/product.proto."""
    out = bytearray()
    for r in echo_records(n):
        product = (_string(1, r['id'])
                   + _string(2, r['name'])
                   + _string(3, r['description'])
                   + _double(4, r['price'])
                   + _string(5, r['categoryId'])
                   + _int32(6, r['stock']))
        out += _message(1, product)          # repeated Product products = 1
    out += _int32(2, n)                      # int32 total_count = 2
    return bytes(out)


def gz(raw):
    return len(gzip.compress(raw, 9))


def br(raw):
    try:
        import brotli
    except ImportError:
        return None
    return len(brotli.compress(raw, quality=5))


def windows(size):
    return size / WINDOW


print('Walidacja odtworzenia ladunku wzgledem pomiaru na wdrozonym systemie')
print('=' * 78)
for n, ref in MEASURED.items():
    j = len(json_payload(n))
    p = len(protobuf_payload(n))
    print(f'n={n}: JSON odtworzony {j} B, zmierzony {ref["json"]} B '
          f'(odchylenie {100*(j-ref["json"])/ref["json"]:+.1f}%)')
    print(f'      protobuf odtworzony {p} B, zmierzony {ref["protobuf"]} B '
          f'(odchylenie {100*(p-ref["protobuf"])/ref["protobuf"]:+.1f}%)')

print()
print('Wplyw kompresji gzip na wolumen i liczbe okien odbiorczych')
print('=' * 104)
print(f'{"n":>6} {"JSON":>9} {"JSON gz":>9} {"krotn.":>7} '
      f'{"proto":>9} {"proto gz":>9} {"krotn.":>7} '
      f'{"okien JSON":>11} {"okien JSONgz":>13} {"okien proto":>12}')
print('-' * 104)

rows = []
for n in (200, 500, 2000):
    j = json_payload(n)
    p = protobuf_payload(n)
    jg, pg = gz(j), gz(p)
    rows.append((n, len(j), jg, len(p), pg))
    print(f'{n:6d} {len(j):9d} {jg:9d} {len(j)/jg:6.1f}x '
          f'{len(p):9d} {pg:9d} {len(p)/pg:6.1f}x '
          f'{windows(len(j)):11.2f} {windows(jg):13.2f} {windows(len(p)):12.2f}')

print()
print('Wiersze tabeli LaTeX (n, JSON, JSON gz, protobuf, protobuf gz, redukcja gz)')
print('=' * 104)
for n, j, jg, p, pg in rows:
    red = 100 * (pg - jg) / jg  # protobuf gz wzgledem JSON gz
    fmt = lambda v: f'{v:\u00a0}'.replace(',', '\\,')
    print(f'{n} & {j:,} & {jg:,} & {p:,} & {pg:,} & {red:+.1f}\\% \\\\'
          .replace(',', '\\,'))

print()
print('Wniosek liczbowy:')
for n, j, jg, p, pg in rows:
    print(f'  n={n}: skompresowany JSON ({jg} B) jest '
          f'{"mniejszy" if jg < p else "wiekszy"} od nieskompresowanego '
          f'protobuf ({p} B); okien: {windows(jg):.2f} wobec {windows(p):.2f}')
