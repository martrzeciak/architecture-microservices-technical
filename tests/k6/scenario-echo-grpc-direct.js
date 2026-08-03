// Scenario: Echo gRPC-Web Direct — EchoProducts directly to .NET :5002 (no proxy)
// Returns hardcoded product data WITHOUT DB or cache.
// Isolates pure gRPC-Web/Protobuf + ASP.NET Core gRPC-Web middleware overhead.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import {
  BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS, PAGE_SIZE,
  buildGrpcWebFrame, encodeVarint,
} from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;
const ECHO_COUNT = PAGE_SIZE;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
  insecureSkipTLSVerify: true,
};

const latency = new Trend('echo_grpc_direct_latency', true);
const errors = new Rate('echo_grpc_direct_errors');

// EchoProductsRequest: field 1 (count) = varint
function buildEchoProto(count) {
  const bytes = encodeVarint(1, count);
  return new Uint8Array(bytes);
}

const grpcWebBody = buildGrpcWebFrame(buildEchoProto(ECHO_COUNT));

export default function () {
  const res = http.post(
    `${BASE_URLS.productDirect}/product.ProductService/EchoProducts`,
    grpcWebBody.buffer,
    {
      headers: {
        'Content-Type': 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
      },
    }
  );

  const ok = check(res, {
    'status 200': (r) => r.status === 200,
    'has body': (r) => r.body && r.body.length > 0,
  });

  latency.add(res.timings.duration);
  errors.add(!ok);
  sleep(PACING_MS);
}
