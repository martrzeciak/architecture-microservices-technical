import { Component, OnInit, OnDestroy, signal, computed, effect, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import {
  Chart,
  BarController, BarElement, CategoryScale, LinearScale,
  Tooltip, Legend,
} from 'chart.js';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

export interface SummaryMetrics {
  avg: number | null;
  med: number | null;
  p90: number | null;
  p95: number | null;
  p99: number | null;
  max: number | null;
  errorRate: number | null;
  totalReqs: number | null;
  reqsPerSec: number | null;
  dataReceived: number | null;
  dataReceivedRate: number | null;
  dataSent: number | null;
  thresholdsFailed: boolean;
}

export interface Summary {
  file: string;
  scenarioId: string;
  scenarioName: string;
  protocol: string | null;
  service: string | null;
  vu: number | null;
  pageSize: number | null;
  tls: boolean;
  created: string;
  metrics: SummaryMetrics | null;
}

const RUNNER_URL = 'http://localhost:3100';

const PROTOCOL_COLORS: Record<string, string> = {
  'REST':              '#3f51b5',
  'gRPC-Web/Envoy':   '#9c27b0',
  'gRPC-Web/Direct':  '#2e7d32',
};

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './results.component.html',
  styleUrl: './results.component.css',
})
export class ResultsComponent implements OnInit, OnDestroy, AfterViewInit {
  summaries = signal<Summary[]>([]);
  loading = signal(true);
  serverError = signal(false);

  filterService = signal<string>('all');
  filterVu      = signal<number | 'all'>('all');
  filterPs      = signal<number | 'all'>('all');
  filterTls     = signal<'all' | 'yes' | 'no'>('all');

  selectedFiles = signal<Set<string>>(new Set());

  readonly protocolColors = PROTOCOL_COLORS;

  @ViewChild('throughputCanvas') throughputCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('latencyCanvas')   latencyCanvas!: ElementRef<HTMLCanvasElement>;

  private throughputChart: Chart | null = null;
  private latencyChart: Chart | null = null;
  private chartsReady = false;

  constructor(private http: HttpClient) {
    // React to comparison changes and rebuild charts
    effect(() => {
      const cmp = this.comparison();
      if (this.chartsReady) {
        // Defer until after Angular updates the DOM (canvas may be display:none → block)
        requestAnimationFrame(() => this.updateCharts(cmp));
      }
    });
  }

  ngOnInit() { this.load(); }

  ngAfterViewInit() {
    this.chartsReady = true;
    this.updateCharts(this.comparison());
  }

  ngOnDestroy() {
    this.throughputChart?.destroy();
    this.latencyChart?.destroy();
  }

  load() {
    this.loading.set(true);
    this.http.get<Summary[]>(`${RUNNER_URL}/summaries`).subscribe({
      next: (data) => {
        this.summaries.set(data);
        this.loading.set(false);
        this.serverError.set(false);
        // Remove selections for files that no longer exist
        const files = new Set(data.map(s => s.file));
        const next = new Set([...this.selectedFiles()].filter(f => files.has(f)));
        this.selectedFiles.set(next);
      },
      error: () => { this.serverError.set(true); this.loading.set(false); },
    });
  }

  allServices = computed(() => {
    const s = new Set(this.summaries().map(s => s.service).filter(Boolean) as string[]);
    return ['all', ...Array.from(s).sort()];
  });

  allVus = computed(() => {
    const v = new Set(this.summaries().map(s => s.vu).filter(v => v !== null) as number[]);
    return [('all' as const), ...Array.from(v).sort((a, b) => a - b)];
  });

  allPs = computed(() => {
    const p = new Set(this.summaries().map(s => s.pageSize).filter(v => v !== null) as number[]);
    return [('all' as const), ...Array.from(p).sort((a, b) => a - b)];
  });

  filtered = computed(() => {
    let data = this.summaries().filter(s => s.metrics !== null);
    if (this.filterService() !== 'all') data = data.filter(s => s.service === this.filterService());
    if (this.filterVu() !== 'all')      data = data.filter(s => s.vu === this.filterVu());
    if (this.filterPs() !== 'all')      data = data.filter(s => s.pageSize === this.filterPs());
    if (this.filterTls() === 'yes')     data = data.filter(s => s.tls);
    if (this.filterTls() === 'no')      data = data.filter(s => !s.tls);
    return data;
  });

  // Comparison: only selected summaries, sorted by protocol
  comparison = computed(() => {
    const sel = this.selectedFiles();
    return this.summaries()
      .filter(s => sel.has(s.file) && s.metrics !== null)
      .sort((a, b) => {
        const order = ['REST', 'gRPC-Web/Envoy', 'gRPC-Web/Direct'];
        return order.indexOf(a.protocol ?? '') - order.indexOf(b.protocol ?? '');
      });
  });

  // Winner = lowest p95 among selected
  winnerFile = computed(() => {
    const cmp = this.comparison();
    if (!cmp.length) return null;
    let best = cmp[0];
    for (const s of cmp) {
      if ((s.metrics?.p95 ?? Infinity) < (best.metrics?.p95 ?? Infinity)) best = s;
    }
    return best.file;
  });

  maxP95 = computed(() => {
    const vals = this.comparison().map(s => s.metrics?.p95 ?? 0);
    return Math.max(...vals, 1);
  });

  toggleSelect(file: string) {
    const next = new Set(this.selectedFiles());
    if (next.has(file)) next.delete(file);
    else next.add(file);
    this.selectedFiles.set(next);
  }

  isSelected(file: string) {
    return this.selectedFiles().has(file);
  }

  selectAll() {
    this.selectedFiles.set(new Set(this.filtered().map(s => s.file)));
  }

  clearSelection() {
    this.selectedFiles.set(new Set());
  }

  deleteRun(summary: Summary) {
    this.http.delete(`${RUNNER_URL}/results/${summary.file}`).subscribe({
      next: () => this.load(),
      error: () => alert('Błąd usuwania pliku.'),
    });
  }

  private updateCharts(cmp: Summary[]) {
    if (!this.throughputCanvas || !this.latencyCanvas) return;
    if (cmp.length === 0) {
      this.throughputChart?.destroy(); this.throughputChart = null;
      this.latencyChart?.destroy();   this.latencyChart = null;
      return;
    }
    const labels = cmp.map(s => {
      const proto = s.protocol ?? s.scenarioName;
      const tags = [s.vu ? `VU${s.vu}` : '', s.pageSize ? `PS${s.pageSize}` : '', s.tls ? 'TLS' : ''].filter(Boolean).join(' ');
      return tags ? `${proto}\n${tags}` : proto;
    });
    const colors = cmp.map(s => this.protocolColor(s.protocol));
    const borderColors = colors.map(c => c);

    // ---- Throughput chart ----
    this.throughputChart?.destroy();
    this.throughputChart = new Chart(this.throughputCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Throughput (req/s)',
          data: cmp.map(s => s.metrics?.reqsPerSec ?? 0),
          backgroundColor: colors.map(c => c + 'cc'),
          borderColor: borderColors,
          borderWidth: 2,
          borderRadius: 6,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: { callbacks: { label: ctx => ` ${(ctx.parsed.y as number).toFixed(1)} req/s` } },
        },
        scales: {
          y: { beginAtZero: true, title: { display: true, text: 'req/s' }, grid: { color: '#f0f0f0' } },
          x: { grid: { display: false } },
        },
      },
    });

    // ---- Latency percentiles chart ----
    this.latencyChart?.destroy();
    this.latencyChart = new Chart(this.latencyCanvas.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Avg',
            data: cmp.map(s => s.metrics?.avg ?? 0),
            backgroundColor: 'rgba(100,149,237,0.7)',
            borderColor: 'rgba(100,149,237,1)',
            borderWidth: 1,
            borderRadius: 4,
          },
          {
            label: 'p(90)',
            data: cmp.map(s => s.metrics?.p90 ?? 0),
            backgroundColor: 'rgba(255,167,38,0.7)',
            borderColor: 'rgba(255,167,38,1)',
            borderWidth: 1,
            borderRadius: 4,
          },
          {
            label: 'p(95)',
            data: cmp.map(s => s.metrics?.p95 ?? 0),
            backgroundColor: 'rgba(239,83,80,0.7)',
            borderColor: 'rgba(239,83,80,1)',
            borderWidth: 1,
            borderRadius: 4,
          },
          {
            label: 'p(99)',
            data: cmp.map(s => s.metrics?.p99 ?? 0),
            backgroundColor: 'rgba(156,39,176,0.7)',
            borderColor: 'rgba(156,39,176,1)',
            borderWidth: 1,
            borderRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', labels: { boxWidth: 12, font: { size: 11 } } },
          tooltip: { callbacks: { label: ctx => ` ${ctx.dataset.label}: ${(ctx.parsed.y as number).toFixed(2)} ms` } },
        },
        scales: {
          y: { beginAtZero: true, title: { display: true, text: 'ms' }, grid: { color: '#f0f0f0' } },
          x: { grid: { display: false } },
        },
      },
    });
  }

  barWidth(p95: number | null): string {
    if (p95 === null) return '0%';
    return `${Math.min(100, (p95 / this.maxP95()) * 100)}%`;
  }

  fmt(v: number | null, decimals = 2): string {
    if (v === null) return 'N/A';
    return v.toFixed(decimals);
  }

  fmtMs(v: number | null): string {
    if (v === null) return 'N/A';
    return `${v.toFixed(2)} ms`;
  }

  fmtBytes(bytes: number | null): string {
    if (bytes === null) return 'N/A';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
  }

  fmtBytesPerSec(rate: number | null): string {
    if (rate === null) return 'N/A';
    if (rate < 1024) return `${rate.toFixed(0)} B/s`;
    if (rate < 1024 * 1024) return `${(rate / 1024).toFixed(1)} KB/s`;
    return `${(rate / 1024 / 1024).toFixed(2)} MB/s`;
  }

  fmtRate(v: number | null): string {
    if (v === null) return 'N/A';
    return `${(v * 100).toFixed(2)}%`;
  }

  protocolColor(protocol: string | null): string {
    return protocol ? (PROTOCOL_COLORS[protocol] ?? '#999') : '#999';
  }

  setFilterVu(value: string) {
    this.filterVu.set(value === 'all' ? 'all' : +value);
  }

  setFilterPs(value: string) {
    this.filterPs.set(value === 'all' ? 'all' : +value);
  }
}
