// Scenario 1: REST GET /api/products
// Tests how fast the REST endpoint responds under load.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS, PAGE_SIZE, getHeaders } from './config.js';

const VU_COUNT = __ENV.VU      ? parseInt(__ENV.VU) : 50;
const USE_TLS  = __ENV.REST_TLS === '1';

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
  insecureSkipTLSVerify: USE_TLS,
};

const latency = new Trend('rest_products_latency', true);
const errors  = new Rate('rest_products_errors');

const BASE = USE_TLS ? BASE_URLS.productRestTls : BASE_URLS.productRest;

export default function () {
  const res = http.get(`${BASE}/api/products?pageSize=${PAGE_SIZE}`, {
    headers: getHeaders(),
  });

  const ok = check(res, {
    'status 200': (r) => r.status === 200,
    'has body': (r) => r.body && r.body.length > 0,
  });

  latency.add(res.timings.duration);
  errors.add(!ok);
  sleep(PACING_MS);
}
