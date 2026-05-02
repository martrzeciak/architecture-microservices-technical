// Scenario 7: native gRPC ListProducts over HTTP/2 (:5001)
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { BASE_URLS, makeStages, PACING_MS, PAGE_SIZE } from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: {
    grpc_req_duration: ['p(95)<5000', 'p(99)<10000'],
    grpc_native_products_errors: ['rate<0.05'],
  },
};

const latency = new Trend('grpc_native_products_latency', true);
const errors  = new Rate('grpc_native_products_errors');

const client = new grpc.Client();
client.load(['./protos'], 'product.proto');

export default function () {
  if (__ITER === 0) {
    client.connect(BASE_URLS.productGrpc, { plaintext: true, timeout: '10s' });
  }

  const startTime = Date.now();
  const res = client.invoke(
    'product.ProductService/ListProducts',
    { page: 1, page_size: PAGE_SIZE },
  );
  const duration = Date.now() - startTime;

  const ok = check(res, {
    'status OK': (r) => r && r.status === grpc.StatusOK,
    'has products': (r) => r && r.message && r.message.products && r.message.products.length > 0,
  });

  latency.add(duration);
  errors.add(!ok);
  sleep(PACING_MS);
}

export function teardown() {
  client.close();
}
