import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface ServiceStatus {
  id: string;
  label: string;
  url: string;
  ok: boolean;
  status: number | null;
  latency: number;
  error?: string;
}

const RUNNER_URL = 'http://localhost:3100';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status.component.html',
  styleUrl: './status.component.css',
})
export class StatusComponent implements OnInit, OnDestroy {
  services = signal<ServiceStatus[]>([]);
  loading = signal(true);
  lastChecked = signal<Date | null>(null);
  runnerOk = signal(true);

  private interval: ReturnType<typeof setInterval> | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.check();
    this.interval = setInterval(() => this.check(), 10_000);
  }

  ngOnDestroy() {
    if (this.interval) clearInterval(this.interval);
  }

  check() {
    this.loading.set(true);
    this.http.get<ServiceStatus[]>(`${RUNNER_URL}/health`).subscribe({
      next: (data) => {
        this.services.set(data);
        this.lastChecked.set(new Date());
        this.loading.set(false);
        this.runnerOk.set(true);
      },
      error: () => {
        this.runnerOk.set(false);
        this.loading.set(false);
      },
    });
  }

  allOk = () => this.services().every((s) => s.ok);
  okCount = () => this.services().filter((s) => s.ok).length;
}
