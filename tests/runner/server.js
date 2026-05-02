// Express server that runs k6 tests inside Docker and streams the output.
// The Angular UI talks to this server on port 3100.
const express = require('express');
const cors = require('cors');
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

const app = express();
const PORT = 3100;

const K6_SCRIPTS_DIR = path.resolve(__dirname, '../k6');
const RESULTS_DIR = path.resolve(__dirname, '../k6/results');

function toDockerPath(winPath) {
  return winPath
    .replace(/\\/g, '/')
    .replace(/^([A-Za-z]):\//, (_, d) => `/${d.toLowerCase()}/`);
}

const DOCKER_K6_DIR      = toDockerPath(K6_SCRIPTS_DIR);
const DOCKER_RESULTS_DIR = toDockerPath(RESULTS_DIR);
const DOCKER_NETWORK     = 'architecture-microservices-technical_default';

app.use(cors());
app.use(express.json());

fs.mkdirSync(RESULTS_DIR, { recursive: true });

const SCENARIOS = [
  // product scenarios — main parameter is pageSize
  {
    id: 'products-rest',
    name: 'Produkty: REST',
    description: 'GET /api/products przez HTTP/1.1 REST',
    file: 'scenario-products-rest.js',
    protocol: 'REST',
    service: 'ProductService',
    paramType: 'pageSize',
  },
  {
    id: 'products-grpc-envoy',
    name: 'Produkty: gRPC-Web (Envoy)',
    description: 'ListProducts przez gRPC-Web via Envoy :8080',
    file: 'scenario-products-grpc-envoy.js',
    protocol: 'gRPC-Web/Envoy',
    service: 'ProductService',
    paramType: 'pageSize',
  },
  {
    id: 'products-grpc-direct',
    name: 'Produkty: gRPC-Web (Direct)',
    description: 'ListProducts przez gRPC-Web bezpośrednio do .NET HTTPS :5002',
    file: 'scenario-products-grpc-direct.js',
    protocol: 'gRPC-Web/Direct',
    service: 'ProductService',
    paramType: 'pageSize',
  },
  {
    id: 'products-grpc-native',
    name: 'Produkty: gRPC Native',
    description: 'ListProducts przez natywny gRPC HTTP/2 :5001 (bez proxy)',
    file: 'scenario-products-grpc-native.js',
    protocol: 'gRPC Native',
    service: 'ProductService',
    paramType: 'pageSize',
  },
  {
    id: 'products-grpc-stream',
    name: 'Produkty: gRPC Streaming',
    description: 'StreamProducts — server-side streaming per kategoria (grpc_req_duration w summary)',
    file: 'scenario-products-grpc-stream.js',
    protocol: 'gRPC Stream',
    service: 'ProductService',
    paramType: 'streamCategory',
  },
  // order scenarios — main parameter is orderItems
  {
    id: 'orders-rest',
    name: 'Zamówienia: REST',
    description: 'POST /api/orders przez HTTP/1.1 REST',
    file: 'scenario-orders-rest.js',
    protocol: 'REST',
    service: 'OrderService',
    paramType: 'orderItems',
  },
  {
    id: 'orders-grpc-envoy',
    name: 'Zamówienia: gRPC-Web (Envoy)',
    description: 'CreateOrder przez gRPC-Web via Envoy :8080',
    file: 'scenario-orders-grpc-envoy.js',
    protocol: 'gRPC-Web/Envoy',
    service: 'OrderService',
    paramType: 'orderItems',
  },
  {
    id: 'orders-grpc-direct',
    name: 'Zamówienia: gRPC-Web (Direct)',
    description: 'CreateOrder przez gRPC-Web bezpośrednio do .NET HTTPS :5005',
    file: 'scenario-orders-grpc-direct.js',
    protocol: 'gRPC-Web/Direct',
    service: 'OrderService',
    paramType: 'orderItems',
  },
  {
    id: 'orders-grpc-native',
    name: 'Zamówienia: gRPC Native',
    description: 'CreateOrder przez natywny gRPC HTTP/2 :5004 (bez proxy)',
    file: 'scenario-orders-grpc-native.js',
    protocol: 'gRPC Native',
    service: 'OrderService',
    paramType: 'orderItems',
  },
];

app.get('/scenarios', (_req, res) => res.json(SCENARIOS));

// POST /run — starts a k6 test in Docker and streams logs via SSE

app.post('/run', (req, res) => {
  const {
    scenarioId,
    vu            = 50,
    pageSize      = 10,
    orderItems    = 1,
    streamCategory = 'electronics',
    restTls       = false,
    bypassCache   = false,
  } = req.body;

  const scenario = SCENARIOS.find((s) => s.id === scenarioId);
  if (!scenario) return res.status(400).json({ error: 'Unknown scenario' });

  // build a filename tag based on which parameter this scenario uses
  const paramTag = (() => {
    if (scenario.paramType === 'orderItems')    return `_OI${orderItems}`;
    if (scenario.paramType === 'streamCategory') return `_CAT${streamCategory}`;
    return `_PS${pageSize}`;
  })();
  const tlsTag   = restTls && scenario.protocol === 'REST' ? '_TLS' : '';
  const cacheTag = bypassCache ? '_COLD' : '';
  const timestamp      = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
  const summaryFileName = `scenario-${scenarioId}_VU${vu}${paramTag}${tlsTag}${cacheTag}_${timestamp}-summary.json`;

  res.setHeader('Content-Type', 'text/event-stream');
  res.setHeader('Cache-Control', 'no-cache');
  res.setHeader('Connection', 'keep-alive');

  const send = (type, data) => res.write(`data: ${JSON.stringify({ type, data })}\n\n`);

  send('start', { scenario: scenario.name, vu, pageSize, orderItems, streamCategory, restTls, bypassCache, timestamp });

  const dockerArgs = [
    'run', '--rm',
    '-v', `${DOCKER_K6_DIR}:/scripts`,
    '-v', `${DOCKER_RESULTS_DIR}:/results`,
    '--network', DOCKER_NETWORK,
    '-e', `VU=${vu}`,
    '-e', 'K6_ENV=docker',
  ];

  if (bypassCache) {
    dockerArgs.push('-e', 'BYPASS_CACHE=1');
  }

  // pass the right env variable depending on scenario type
  if (scenario.paramType === 'pageSize') {
    dockerArgs.push('-e', `PAGE_SIZE=${pageSize}`);
  } else if (scenario.paramType === 'orderItems') {
    dockerArgs.push('-e', `ORDER_ITEMS=${orderItems}`);
  } else if (scenario.paramType === 'streamCategory') {
    dockerArgs.push('-e', `STREAM_CATEGORY=${streamCategory}`);
  }

  if (restTls && scenario.protocol === 'REST') {
    dockerArgs.push('-e', 'REST_TLS=1');
  }

  dockerArgs.push(
    'grafana/k6',
    'run',
    '--insecure-skip-tls-verify',
    '--summary-export', `/results/${summaryFileName}`,
    `/scripts/${scenario.file}`,
  );

  console.log(`[RUN] docker ${dockerArgs.join(' ')}`);

  const k6 = spawn('docker', dockerArgs);

  k6.on('error', (err) => {
    const msg = err.code === 'ENOENT'
      ? 'ERROR: Docker not found in PATH.\n'
      : `Error starting Docker: ${err.message}\n`;
    send('log', msg);
    send('done', { code: 1, summaryFile: null, success: false });
    res.end();
  });

  k6.stdout.on('data', (chunk) => { const t = chunk.toString(); process.stdout.write(t); send('log', t); });
  k6.stderr.on('data', (chunk) => { const t = chunk.toString(); process.stderr.write(t); send('log', t); });

  k6.on('close', (code) => {
    console.log(`[RUN] Docker exit code: ${code}`);
    const summaryExists = fs.existsSync(path.join(RESULTS_DIR, summaryFileName));
    const success = summaryExists;
    const msg = code === 0
      ? `\nTest finished (exit 0)\n`
      : summaryExists
        ? `\nTest finished with threshold failures (exit ${code}), data saved\n`
        : `\nTest failed (exit ${code})\n`;
    send('log', msg);
    send('done', { code, summaryFile: summaryFileName, success });
    res.end();
  });

  res.on('close', () => { try { k6.kill(); } catch (_) {} });
});

// parse scenario id, VU count and params from the result filename
function parseFileName(f) {
  const base = f.replace('-summary.json', '').replace('.json', '');
  const m = base.match(/^scenario-(.+?)_VU(\d+)(?:_PS(\d+))?(?:_OI(\d+))?(?:_CAT([^_]+))?(?:_TLS)?(?:_COLD)?/);
  return {
    scenarioId:     m ? m[1] : base,
    vu:             m ? parseInt(m[2]) : null,
    pageSize:       m && m[3] ? parseInt(m[3]) : null,
    orderItems:     m && m[4] ? parseInt(m[4]) : null,
    streamCategory: m && m[5] ? m[5] : null,
    tls:            base.includes('_TLS'),
    cold:           base.includes('_COLD'),
  };
}

// pull key metrics out of a k6 summary JSON file
// REST and gRPC-Web use http_req_duration, native gRPC uses grpc_req_duration
function extractMetrics(raw) {
  const dur  = raw.metrics?.http_req_duration  ?? raw.metrics?.grpc_req_duration ?? null;
  const fail = raw.metrics?.http_req_failed    ?? raw.metrics?.grpc_stream_errors ?? null;
  const reqs = raw.metrics?.http_reqs          ?? raw.metrics?.grpc_reqs          ?? null;
  const rx   = raw.metrics?.data_received      ?? null;
  const tx   = raw.metrics?.data_sent          ?? null;

  return {
    avg:              dur?.avg       ?? null,
    med:              dur?.med       ?? null,
    p90:              dur?.['p(90)'] ?? null,
    p95:              dur?.['p(95)'] ?? null,
    p99:              dur?.['p(99)'] ?? null,
    max:              dur?.max       ?? null,
    errorRate:        fail?.value    ?? null,
    totalReqs:        reqs?.count    ?? null,
    reqsPerSec:       reqs?.rate     ?? null,
    dataReceived:     rx?.count      ?? null,
    dataReceivedRate: rx?.rate       ?? null,
    dataSent:         tx?.count      ?? null,
    thresholdsFailed: Object.values(raw.metrics ?? {})
      .some((m) => m.thresholds && Object.values(m.thresholds).some((v) => v === true)),
  };
}

app.get('/results', (_req, res) => {
  const files = fs.existsSync(RESULTS_DIR)
    ? fs.readdirSync(RESULTS_DIR).filter((f) => f.endsWith('.json'))
    : [];

  const results = files.map((f) => {
    const stat = fs.statSync(path.join(RESULTS_DIR, f));
    return { file: f, ...parseFileName(f), size: stat.size, created: stat.birthtime };
  });

  results.sort((a, b) => new Date(b.created) - new Date(a.created));
  res.json(results);
});

app.get('/results/:file', (req, res) => {
  const filePath = path.join(RESULTS_DIR, req.params.file);
  if (!fs.existsSync(filePath)) return res.status(404).json({ error: 'Not found' });
  res.sendFile(filePath);
});

app.delete('/results/:file', (req, res) => {
  const file = req.params.file;
  const filePath = path.join(RESULTS_DIR, file);
  if (!fs.existsSync(filePath)) return res.status(404).json({ error: 'Not found' });
  try {
    fs.unlinkSync(filePath);
    if (!file.endsWith('-summary.json')) {
      const summaryPath = path.join(RESULTS_DIR, file.replace('.json', '-summary.json'));
      if (fs.existsSync(summaryPath)) fs.unlinkSync(summaryPath);
    }
    res.json({ ok: true });
  } catch (err) {
    res.status(500).json({ error: err.message });
  }
});

app.get('/summaries', (_req, res) => {
  const files = fs.existsSync(RESULTS_DIR)
    ? fs.readdirSync(RESULTS_DIR).filter((f) => f.endsWith('-summary.json'))
    : [];

  const summaries = files.map((f) => {
    const stat   = fs.statSync(path.join(RESULTS_DIR, f));
    const parsed = parseFileName(f);
    const scenario = SCENARIOS.find((s) => s.id === parsed.scenarioId);

    let metrics = null;
    try {
      const raw = JSON.parse(fs.readFileSync(path.join(RESULTS_DIR, f), 'utf8'));
      metrics = extractMetrics(raw);
    } catch { /* ignoruj uszkodzone pliki */ }

    return {
      file: f,
      ...parsed,
      scenarioName: scenario?.name      ?? parsed.scenarioId,
      protocol:     scenario?.protocol  ?? null,
      service:      scenario?.service   ?? null,
      paramType:    scenario?.paramType ?? null,
      created:      stat.birthtime,
      metrics,
    };
  });

  summaries.sort((a, b) => new Date(b.created) - new Date(a.created));
  res.json(summaries);
});

app.get('/summaries/:file', (req, res) => {
  const filePath = path.join(RESULTS_DIR, req.params.file);
  if (!fs.existsSync(filePath)) return res.status(404).json({ error: 'Not found' });
  try {
    res.json(JSON.parse(fs.readFileSync(filePath, 'utf8')));
  } catch {
    res.status(500).json({ error: 'Invalid JSON' });
  }
});

// GET /check — returns docker version and whether the docker network exists
app.get('/check', (_req, res) => {
  const { execSync } = require('child_process');
  const checks = {};
  try { checks.docker = execSync('docker version --format "{{.Client.Version}}"').toString().trim(); }
  catch { checks.docker = 'not found'; }
  try {
    const nets = execSync('docker network ls --format "{{.Name}}"').toString();
    checks.network = nets.includes(DOCKER_NETWORK) ? 'OK' : `missing (expected: ${DOCKER_NETWORK})`;
  } catch { checks.network = 'not found'; }
  checks.k6ScriptsDir  = K6_SCRIPTS_DIR;
  checks.dockerK6Dir   = DOCKER_K6_DIR;
  checks.resultsDir    = RESULTS_DIR;
  res.json(checks);
});

const http = require('http');
const SERVICES = [
  { id: 'product-service', label: 'ProductService (REST)', url: 'http://localhost:5000/api/products?pageSize=1', expectedStatus: 200 },
  { id: 'order-service',   label: 'OrderService (REST)',   url: 'http://localhost:5003/api/orders',             expectedStatus: 200 },
  { id: 'envoy',           label: 'Envoy Proxy',           url: 'http://localhost:8080/',                       expectedStatus: 404 },
  { id: 'prometheus',      label: 'Prometheus',            url: 'http://localhost:9090/-/healthy',               expectedStatus: 200 },
  { id: 'grafana',         label: 'Grafana',               url: 'http://localhost:3000/api/health',              expectedStatus: 200 },
];

function checkUrl(url, expectedStatus, timeoutMs = 3000) {
  return new Promise((resolve) => {
    const start = Date.now();
    const req = http.get(url, { timeout: timeoutMs }, (resp) => {
      const ok = resp.statusCode === expectedStatus;
      resp.resume();
      resolve({ ok, status: resp.statusCode, latency: Date.now() - start });
    });
    req.on('error', (err) => resolve({ ok: false, status: null, error: err.message, latency: Date.now() - start }));
    req.on('timeout', () => { req.destroy(); resolve({ ok: false, status: null, error: 'timeout', latency: timeoutMs }); });
  });
}

app.get('/health', async (_req, res) => {
  const results = await Promise.all(
    SERVICES.map(async (svc) => ({ ...svc, ...await checkUrl(svc.url, svc.expectedStatus) }))
  );
  res.json(results);
});

app.listen(PORT, () => {
  console.log(`k6 Runner Server listening on http://localhost:${PORT}`);
});
