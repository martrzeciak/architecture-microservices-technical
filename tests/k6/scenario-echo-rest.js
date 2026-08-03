// Scenario: Echo REST — GET /api/echo?count=N
// Returns hardcoded product data WITHOUT DB or cache.
// Isolates pure REST/JSON serialization + HTTP/1.1 protocol overhead.
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';
import { BASE_URLS, THRESHOLDS, SUMMARY_TREND_STATS, makeStages, PACING_MS, PAGE_SIZE } from './config.js';

const VU_COUNT = __ENV.VU ? parseInt(__ENV.VU) : 50;
const ECHO_COUNT = PAGE_SIZE; // use same count as PAGE_SIZE for fair comparison

export const options = {
  stages: makeStages(VU_COUNT),
  thresholds: THRESHOLDS,
  summaryTrendStats: SUMMARY_TREND_STATS,
};

const latency = new Trend('echo_rest_latency', true);
const errors = new Rate('echo_rest_errors');

export default function () {
  const res = http.get(`${BASE_URLS.productRest}/api/echo?count=${ECHO_COUNT}`);

  const ok = check(res, {
    'status 200': (r) => r.status === 200,
    'has body': (r) => r.body && r.body.length > 0,
  });

  latency.add(res.timings.duration);
  errors.add(!ok);
  sleep(PACING_MS);
}
