// Scenario 4: REST POST /api/orders
// Use ORDER_ITEMS to control how many line items each order has.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import {
  BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS,
  ORDER_ITEMS, buildOrderItems,
} from './config.js';

const VU_COUNT = __ENV.VU      ? parseInt(__ENV.VU) : 50;
const USE_TLS  = __ENV.REST_TLS === '1';

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
  insecureSkipTLSVerify: USE_TLS,
};

const latency = new Trend('rest_orders_latency', true);
const errors  = new Rate('rest_orders_errors');

const BASE = USE_TLS ? BASE_URLS.orderRestTls : BASE_URLS.orderRest;

export default function () {
  const items = buildOrderItems(ORDER_ITEMS);
  const payload = JSON.stringify({
    customerId: `customer-${__VU}`,
    items: items.map(i => ({
      productId:   i.productId,
      productName: i.productName,
      quantity:    i.quantity,
      unitPrice:   i.unitPrice,
    })),
  });

  const res = http.post(`${BASE}/api/orders`, payload, {
    headers: { 'Content-Type': 'application/json' },
  });

  const ok = check(res, {
    'status 201': (r) => r.status === 201,
    'has body':   (r) => r.body && r.body.length > 0,
  });

  latency.add(res.timings.duration);
  errors.add(!ok);
  sleep(PACING_MS);
}
