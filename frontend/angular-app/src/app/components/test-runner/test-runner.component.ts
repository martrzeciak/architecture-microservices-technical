import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Scenario {
  id: string;
  name: string;
  description: string;
  protocol: 'REST' | 'gRPC-Web/Envoy' | 'gRPC-Web/Direct';
  service: string;
  file: string;
}

interface ResultFile {
  file: string;
  scenario: string;
  vu: number | null;
  pageSize: number | null;
  tls: boolean;
  size: number;
  created: string;
}

interface HealthService {
  id: string;
  label: string;
  ok: boolean;
  latency: number;
}

interface QuickSummary {
  reqsPerSec: number | null;
  p95: number | null;
  errorRate: number | null;
  avg: number | null;
}

interface LogEvent {
  type: 'start' | 'log' | 'done';
  data: unknown;
}

const RUNNER_URL = 'http://localhost:3100';
const VU_OPTIONS = [10, 50, 100, 500];
const TEST_DURATION_SEC = 160; // 30s warmup + 120s steady + 10s cooldown

const PROTOCOL_COLORS: Record<string, string> = {
  'REST': 'badge-rest',
  'gRPC-Web/Envoy': 'badge-envoy',
  'gRPC-Web/Direct': 'badge-direct',
};

@Component({
  selector: 'app-test-runner',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './test-runner.component.html',
  styleUrl: './test-runner.component.css',
})
export class TestRunnerComponent implements OnInit, OnDestroy {
  scenarios    = signal<Scenario[]>([]);
  results      = signal<ResultFile[]>([]);
  logs         = signal<string[]>([]);
  running      = signal(false);
  lastExitCode = signal<number | null>(null);
  lastResultFile  = signal<string | null>(null);
  lastSummaryFile = signal<string | null>(null);
  serverError  = signal(false);
  quickSummary = signal<QuickSummary | null>(null);

  // Health
  health       = signal<HealthService[]>([]);
  healthLoading = signal(false);
  private healthTimer: ReturnType<typeof setInterval> | null = null;

  // Progress timer
  elapsedSec   = signal(0);
  private progressTimer: ReturnType<typeof setInterval> | null = null;
  readonly totalSec = TEST_DURATION_SEC;

  // Run-all queue
  runAllQueue  = signal<string[]>([]);
  runAllActive = signal(false);
  matrixMode   = signal(false);

  selectedScenarioId = signal<string | null>(null);
  selectedVu         = signal(50);
  selectedPageSize   = signal(10);
  restTls            = signal(false);
  bypassCache        = signal(false);

  readonly vuOptions       = VU_OPTIONS;
  readonly pageSizeOptions = [10, 50, 100, 200, 500];
  readonly protocolColors  = PROTOCOL_COLORS;

  private currentReader: ReadableStreamDefaultReader | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadScenarios();
    this.loadResults();
    this.loadHealth();
    // Refresh health every 30s
    this.healthTimer = setInterval(() => this.loadHealth(), 30_000);
  }

  ngOnDestroy() {
    if (this.healthTimer) clearInterval(this.healthTimer);
    if (this.progressTimer) clearInterval(this.progressTimer);
    this.currentReader?.cancel();
  }

  loadScenarios() {
    this.http.get<Scenario[]>(`${RUNNER_URL}/scenarios`).subscribe({
      next: (s) => {
        this.scenarios.set(s);
        if (s.length) this.selectedScenarioId.set(s[0].id);
        this.serverError.set(false);
      },
      error: () => this.serverError.set(true),
    });
  }

  loadResults() {
    this.http.get<ResultFile[]>(`${RUNNER_URL}/results`).subscribe({
      next: (r) => this.results.set(r),
      error: () => {},
    });
  }

  loadHealth() {
    this.healthLoading.set(true);
    this.http.get<HealthService[]>(`${RUNNER_URL}/health`).subscribe({
      next: (h) => { this.health.set(h); this.healthLoading.set(false); },
      error: () => this.healthLoading.set(false),
    });
  }

  selectScenario(id: string) {
    this.selectedScenarioId.set(id);
  }

  selectedScenario = computed(() =>
    this.scenarios().find((s) => s.id === this.selectedScenarioId())
  );

  isRestScenario = computed(() =>
    this.selectedScenario()?.protocol === 'REST'
  );

  progressPct = computed(() =>
    Math.min(100, Math.round((this.elapsedSec() / this.totalSec) * 100))
  );

  progressColor = computed(() => {
    const p = this.progressPct();
    if (p < 20) return '#3f51b5'; // warmup — blue
    if (p > 93) return '#9c27b0'; // cooldown — purple
    return '#2e7d32';             // steady — green
  });

  progressLabel = computed(() => {
    const e = this.elapsedSec();
    if (e < 30)  return 'Rozgrzewka…';
    if (e < 150) return 'Pomiar…';
    return 'Schładzanie…';
  });

  servicesAllUp = computed(() =>
    this.health().length > 0 && this.health().every(h => h.ok)
  );

  runTest(scenarioId?: string, vu?: number) {
    if (this.running() && !this.runAllActive()) return;
    const sid = scenarioId ?? this.selectedScenarioId();
    if (!sid) return;

    this.running.set(true);
    this.logs.set([]);
    this.lastExitCode.set(null);
    this.lastResultFile.set(null);
    this.lastSummaryFile.set(null);
    this.quickSummary.set(null);
    this.elapsedSec.set(0);

    const runVu = vu ?? this.selectedVu();

    // Start progress timer
    if (this.progressTimer) clearInterval(this.progressTimer);
    this.progressTimer = setInterval(() => {
      this.elapsedSec.update(s => s + 1);
    }, 1000);

    fetch(`${RUNNER_URL}/run`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        scenarioId: sid,
        vu: runVu,
        pageSize: this.selectedPageSize(),
        restTls: this.restTls(),
        bypassCache: this.bypassCache(),
      }),
    }).then((response) => {
      const reader = response.body!.getReader();
      this.currentReader = reader;
      const decoder = new TextDecoder();
      let buffer = '';

      const read = () => {
        reader.read().then(({ done, value }) => {
          if (done) {
            this.finishTest();
            return;
          }
          buffer += decoder.decode(value, { stream: true });
          const lines = buffer.split('\n');
          buffer = lines.pop() ?? '';

          for (const line of lines) {
            if (!line.startsWith('data:')) continue;
            try {
              const event: LogEvent = JSON.parse(line.slice(5).trim());
              if (event.type === 'log') {
                this.logs.update((l) => [...l, event.data as string]);
                setTimeout(() => {
                  const box = document.querySelector('.log-box');
                  if (box) box.scrollTop = box.scrollHeight;
                }, 0);
              } else if (event.type === 'done') {
                const d = event.data as { code: number; resultFile: string; summaryFile: string; success: boolean };
                this.lastExitCode.set(d.code);
                this.lastResultFile.set(d.resultFile);
                this.lastSummaryFile.set(d.summaryFile);
                if (d.success && d.summaryFile) this.fetchQuickSummary(d.summaryFile);
              }
            } catch { /* ignoruj */ }
          }
          read();
        }).catch(() => this.finishTest());
      };
      read();
    }).catch(() => {
      this.logs.update((l) => [...l, 'Błąd połączenia z runner serverem.']);
      this.finishTest();
    });
  }

  private finishTest() {
    if (this.progressTimer) { clearInterval(this.progressTimer); this.progressTimer = null; }
    this.running.set(false);
    this.loadResults();

    // Run-all / Matrix: advance queue
    if (this.runAllActive()) {
      const queue = [...this.runAllQueue()];
      queue.shift();
      this.runAllQueue.set(queue);
      
      if (queue.length > 0) {
        setTimeout(() => this.startNextInQueue(), 3000); // 3s pause between runs
      } else {
        this.runAllActive.set(false);
        this.matrixMode.set(false);
      }
    }
  }

  private fetchQuickSummary(summaryFile: string) {
    this.http.get<Record<string, unknown>>(`${RUNNER_URL}/summaries/${summaryFile}`).subscribe({
      next: (raw: Record<string, unknown>) => {
        const metrics = raw['metrics'] as Record<string, Record<string, number>> | undefined;
        // gRPC native uses grpc_req_duration, others use http_req_duration
        const dur  = metrics?.['http_req_duration'] ?? metrics?.['grpc_req_duration'];
        const fail = metrics?.['http_req_failed'] ?? metrics?.['grpc_stream_errors'];
        const reqs = metrics?.['http_reqs'] ?? metrics?.['grpc_reqs'];
        this.quickSummary.set({
          reqsPerSec: reqs?.['rate'] ?? null,
          avg:        dur?.['avg'] ?? null,
          p95:        dur?.['p(95)'] ?? null,
          errorRate:  fail?.['value'] ?? null,
        });
      },
      error: () => {},
    });
  }

  stopTest() {
    this.currentReader?.cancel();
    this.runAllActive.set(false);
    this.runAllQueue.set([]);
    this.matrixMode.set(false);
  }

  runAll() {
    if (this.running()) return;
    const ids = this.scenarios().map(s => s.id);
    if (!ids.length) return;
    
    this.runAllActive.set(true);
    this.matrixMode.set(false);
    this.runAllQueue.set([...ids]);
    
    this.startNextInQueue();
  }

  runMatrix() {
    if (this.running()) return;
    const ids = this.scenarios().map(s => s.id);
    if (!ids.length) return;

    // Create a queue of [scenarioId, vu] pairs
    const queue: string[] = [];
    for (const vu of [10, 50, 100]) {
      for (const id of ids) {
        queue.push(`${id}|${vu}`);
      }
    }

    this.runAllActive.set(true);
    this.matrixMode.set(true);
    this.runAllQueue.set(queue);

    this.startNextInQueue();
  }

  private startNextInQueue() {
    const queue = this.runAllQueue();
    if (queue.length === 0) {
      this.runAllActive.set(false);
      this.matrixMode.set(false);
      return;
    }

    const next = queue[0];
    if (this.matrixMode()) {
      const [id, vu] = next.split('|');
      this.selectedScenarioId.set(id);
      this.selectedVu.set(+vu);
      this.runTest(id, +vu);
    } else {
      this.selectedScenarioId.set(next);
      this.runTest(next);
    }
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    return `${(bytes / 1024).toFixed(1)} KB`;
  }

  fmt(v: number | null, d = 2): string {
    return v === null ? 'N/A' : v.toFixed(d);
  }

  fmtMs(v: number | null): string {
    return v === null ? 'N/A' : `${v.toFixed(2)} ms`;
  }

  trackById(_: number, item: Scenario) { return item.id; }
}
