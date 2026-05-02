// Scenario 3: gRPC-Web ListProducts directly to .NET (:5002), no proxy
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import {
  BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS, PAGE_SIZE,
  buildListProductsProto, buildGrpcWebFrame, getHeaders
} from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
  insecureSkipTLSVerify: true,
};

const latency = new Trend('grpc_web_direct_latency', true);
const errors  = new Rate('grpc_web_direct_errors');

const requestProto = buildListProductsProto(PAGE_SIZE);
const grpcWebBody = buildGrpcWebFrame(requestProto);

export default function () {
  const res = http.post(
    `${BASE_URLS.productDirect}/product.ProductService/ListProducts`,
    grpcWebBody.buffer,
    {
      headers: {
        'Content-Type': 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
        ...getHeaders()
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
