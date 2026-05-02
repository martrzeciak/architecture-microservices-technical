// Shared config for all k6 scenarios.
// Set K6_ENV=docker to use Docker service names instead of localhost.

const isDocker = __ENV.K6_ENV === 'docker';

export const BASE_URLS = isDocker
  ? {
      productRest:      'http://product-service:5000',
      productRestTls:   'https://product-service:5002',
      orderRest:        'http://order-service:5003',
      orderRestTls:     'https://order-service:5005',
      envoy:            'http://envoy:8080',
      productDirect:    'https://product-service:5002',
      orderDirect:      'https://order-service:5005',
      productGrpc:      'product-service:5001',
      orderGrpc:        'order-service:5004',
    }
  : {
      productRest:      'http://localhost:5000',
      productRestTls:   'https://localhost:5002',
      orderRest:        'http://localhost:5003',
      orderRestTls:     'https://localhost:5005',
      envoy:            'http://localhost:8080',
      productDirect:    'https://localhost:5002',
      orderDirect:      'https://localhost:5005',
      productGrpc:      'localhost:5001',
      orderGrpc:        'localhost:5004',
    };

// thresholds for all HTTP scenarios
export const THRESHOLDS = {
  http_req_duration: ['p(95)<5000', 'p(99)<10000'],
  http_req_failed: ['rate<0.05'],
};

export const SUMMARY_TREND_STATS = ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'];

export const HTTP_OPTIONS = {
  timeout: '10s',
};

// page size — can be overridden with PAGE_SIZE env var
export const PAGE_SIZE = __ENV.PAGE_SIZE
  ? parseInt(__ENV.PAGE_SIZE)
  : 10;

// number of items per order — can be overridden with ORDER_ITEMS env var
export const ORDER_ITEMS = __ENV.ORDER_ITEMS
  ? parseInt(__ENV.ORDER_ITEMS)
  : 1;

// product category used in streaming test
export const STREAM_CATEGORY = __ENV.STREAM_CATEGORY || 'electronics';

// skip the cache when this flag is set
export const BYPASS_CACHE = __ENV.BYPASS_CACHE === '1';

export function getHeaders() {
  const headers = {};
  if (BYPASS_CACHE) {
    headers['X-Bypass-Cache'] = 'true';
  }
  return headers;
}

// warmup 30s → steady 120s → cooldown 10s
export function makeStages(targetVU) {
  return [
    { duration: '30s', target: targetVU },   // ramp up
    { duration: '120s', target: targetVU },  // measure
    { duration: '10s', target: 0 },          // ramp down
  ];
}

// small pause between requests to avoid hammering the server
export const PACING_MS = 0.01;

// products that exist in the seed data
export const PRODUCTS = [
  { productId: '1',  productName: 'Laptop Pro 15',       unitPrice: 2499.99 },
  { productId: '2',  productName: 'Wireless Mouse',       unitPrice: 49.99  },
  { productId: '3',  productName: 'Mechanical Keyboard',  unitPrice: 129.99 },
  { productId: '4',  productName: 'USB-C Hub',            unitPrice: 39.99  },
  { productId: '5',  productName: 'Monitor 27"',          unitPrice: 599.99 },
  { productId: '6',  productName: 'Gaming Headset',       unitPrice: 89.99  },
  { productId: '8',  productName: 'SSD 1TB',              unitPrice: 109.99 },
  { productId: '9',  productName: 'RAM 32GB DDR5',        unitPrice: 179.99 },
  { productId: '15', productName: 'USB Microphone',       unitPrice: 79.99  },
  { productId: '17', productName: 'Power Bank 20000mAh',  unitPrice: 59.99  },
];

// build a list of random order items from the product pool
export function buildOrderItems(count) {
  const items = [];
  for (let i = 0; i < count; i++) {
    const p = PRODUCTS[Math.floor(Math.random() * PRODUCTS.length)];
    items.push({
      productId:   p.productId,
      productName: p.productName,
      quantity:    Math.ceil(Math.random() * 3),
      unitPrice:   p.unitPrice,
    });
  }
  return items;
}

// encode a ListProducts protobuf message with the given page size
export function buildListProductsProto(pageSize) {
  const ps = [];
  // field 2 (page) = 1
  ps.push(0x10, 0x01);
  // field 3 (page_size)
  ps.push(0x18);
  let v = pageSize;
  while (v > 127) {
    ps.push((v & 0x7f) | 0x80);
    v >>= 7;
  }
  ps.push(v);
  return new Uint8Array(ps);
}

// wrap protobuf bytes in a gRPC-Web frame
export function buildGrpcWebFrame(protoBytes) {
  const len = protoBytes.length;
  const frame = new Uint8Array(5 + len);
  frame[0] = 0x00;
  frame[1] = (len >> 24) & 0xff;
  frame[2] = (len >> 16) & 0xff;
  frame[3] = (len >> 8) & 0xff;
  frame[4] = len & 0xff;
  frame.set(protoBytes, 5);
  return frame;
}

// helpers for encoding protobuf fields manually
export function encodeString(fieldNum, str) {
  const bytes = [];
  for (let i = 0; i < str.length; i++) {
    const code = str.charCodeAt(i);
    if (code < 0x80) {
      bytes.push(code);
    } else if (code < 0x800) {
      bytes.push(0xc0 | (code >> 6), 0x80 | (code & 0x3f));
    } else {
      bytes.push(0xe0 | (code >> 12), 0x80 | ((code >> 6) & 0x3f), 0x80 | (code & 0x3f));
    }
  }
  const tag = (fieldNum << 3) | 2;
  return [tag, bytes.length, ...bytes];
}

export function encodeVarint(fieldNum, value) {
  const tag = (fieldNum << 3) | 0;
  const result = [tag];
  while (value > 0x7f) {
    result.push((value & 0x7f) | 0x80);
    value >>= 7;
  }
  result.push(value);
  return result;
}

export function encodeDouble(fieldNum, value) {
  const tag = (fieldNum << 3) | 1;
  const buf = new ArrayBuffer(8);
  new DataView(buf).setFloat64(0, value, true);
  return [tag, ...new Uint8Array(buf)];
}

export function encodeEmbedded(fieldNum, bytes) {
  const tag = (fieldNum << 3) | 2;
  return [tag, bytes.length, ...bytes];
}

// encode a CreateOrder request with multiple items
export function buildCreateOrderProto(customerId, items) {
  const customerBytes = encodeString(1, customerId);
  const allItemBytes = [];
  for (const item of items) {
    const itemBytes = [
      ...encodeString(1, item.productId),
      ...encodeString(2, item.productName),
      ...encodeVarint(3, item.quantity),
      ...encodeDouble(4, item.unitPrice),
    ];
    allItemBytes.push(...encodeEmbedded(2, itemBytes));
  }
  return new Uint8Array([...customerBytes, ...allItemBytes]);
}
