// Scenario 9: gRPC server-side streaming — StreamProducts
// The server sends products one by one with a 100ms delay between each.
// Use STREAM_CATEGORY to filter by category (default: electronics).
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import { BASE_URLS, makeStages, PACING_MS, STREAM_CATEGORY } from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 10;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: {
    // streaming is slower than unary calls so we use a higher threshold
    grpc_req_duration: ['p(95)<30000'],
  },
};

const streamErrors = new Rate('grpc_stream_errors');

const client = new grpc.Client();
client.load(['./protos'], 'product.proto');

export default function () {
  if (__ITER === 0) {
    client.connect(BASE_URLS.productGrpc, { plaintext: true, timeout: '30s' });
  }

  let hadError = false;

  const stream = new grpc.Stream(client, 'product.ProductService/StreamProducts');

  // must register data handler or k6 won't process incoming messages
  stream.on('data', () => {});
  stream.on('error', () => { hadError = true; });

  stream.write({ category_id: STREAM_CATEGORY });
  stream.end();

  const ok = check(null, {
    'no stream error': () => !hadError,
  });

  streamErrors.add(!ok);
  sleep(PACING_MS);
}

export function teardown() {
  client.close();
}
