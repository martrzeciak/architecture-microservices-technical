// Scenario 8: native gRPC CreateOrder over HTTP/2 (:5004)
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { BASE_URLS, makeStages, PACING_MS, ORDER_ITEMS, buildOrderItems } from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: {
    grpc_req_duration: ['p(95)<5000', 'p(99)<10000'],
    grpc_native_orders_errors: ['rate<0.05'],
  },
};

const latency = new Trend('grpc_native_orders_latency', true);
const errors  = new Rate('grpc_native_orders_errors');

const client = new grpc.Client();
client.load(['./protos'], 'order.proto');

export default function () {
  if (__ITER === 0) {
    client.connect(BASE_URLS.orderGrpc, { plaintext: true, timeout: '10s' });
  }

  const items = buildOrderItems(ORDER_ITEMS);
  const startTime = Date.now();
  const res = client.invoke(
    'order.OrderService/CreateOrder',
    {
      customer_id: `customer-${__VU}`,
      items: items.map(i => ({
        product_id:   i.productId,
        product_name: i.productName,
        quantity:     i.quantity,
        unit_price:   i.unitPrice,
      })),
    },
  );
  const duration = Date.now() - startTime;

  const ok = check(res, {
    'status OK':    (r) => r && r.status === grpc.StatusOK,
    'has order id': (r) => r && r.message && r.message.id,
  });

  latency.add(duration);
  errors.add(!ok);
  sleep(PACING_MS);
}

export function teardown() {
  client.close();
}
