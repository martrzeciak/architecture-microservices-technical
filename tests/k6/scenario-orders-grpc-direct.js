// Scenario 6: gRPC-Web CreateOrder directly to .NET (:5005), no proxy
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import {
  BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS,
  ORDER_ITEMS, buildOrderItems, buildCreateOrderProto, buildGrpcWebFrame,
} from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
  insecureSkipTLSVerify: true,
};

const latency = new Trend('grpc_web_direct_orders_latency', true);
const errors  = new Rate('grpc_web_direct_orders_errors');

export default function () {
  const items = buildOrderItems(ORDER_ITEMS);
  const protoBytes = buildCreateOrderProto(`customer-${__VU}`, items);
  const body = buildGrpcWebFrame(protoBytes);

  const res = http.post(
    `${BASE_URLS.orderDirect}/order.OrderService/CreateOrder`,
    body.buffer,
    {
      headers: {
        'Content-Type': 'application/grpc-web+proto',
        'X-Grpc-Web': '1',
      },
    }
  );

  const ok = check(res, {
    'status 200': (r) => r.status === 200,
    'has body':   (r) => r.body && r.body.length > 0,
  });

  latency.add(res.timings.duration);
  errors.add(!ok);
  sleep(PACING_MS);
}
