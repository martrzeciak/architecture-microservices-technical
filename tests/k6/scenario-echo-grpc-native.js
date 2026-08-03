// Scenario: Echo gRPC Native — EchoProducts over HTTP/2 (:5001)
// Returns hardcoded product data WITHOUT DB or cache.
// Isolates pure gRPC/Protobuf serialization + HTTP/2 protocol overhead.
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { BASE_URLS, makeStages, PACING_MS, PAGE_SIZE, SUMMARY_TREND_STATS } from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;
const ECHO_COUNT = PAGE_SIZE; // use same count as PAGE_SIZE for fair comparison

export const options = {
  stages: makeStages(VU_COUNT),
  summaryTrendStats: SUMMARY_TREND_STATS,
  thresholds: {
    grpc_req_duration: ['p(95)<5000', 'p(99)<10000'],
    echo_grpc_errors: ['rate<0.05'],
  },
};

const latency = new Trend('echo_grpc_latency', true);
const errors = new Rate('echo_grpc_errors');

const client = new grpc.Client();
client.load(['./protos'], 'product.proto');

export default function () {
  if (__ITER === 0) {
    client.connect(BASE_URLS.productGrpc, { plaintext: true, timeout: '10s' });
  }

  const startTime = Date.now();
  const res = client.invoke(
    'product.ProductService/EchoProducts',
    { count: ECHO_COUNT },
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
