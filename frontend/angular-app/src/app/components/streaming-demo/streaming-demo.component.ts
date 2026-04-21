import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GrpcProductService } from '../../services/grpc-product.service';
import type { Product } from '../../services/protocol-switcher.service';

@Component({
  selector: 'app-streaming-demo',
  imports: [RouterLink],
  templateUrl: './streaming-demo.component.html',
  styleUrl: './streaming-demo.component.css',
})
export class StreamingDemoComponent implements OnDestroy {
  private grpc = inject(GrpcProductService);

  readonly streamedProducts = signal<Product[]>([]);
  readonly isStreaming = signal(false);
  readonly isDone = signal(false);
  readonly error = signal<string | null>(null);

  private sub?: { unsubscribe(): void };

  startStream(): void {
    this.streamedProducts.set([]);
    this.isDone.set(false);
    this.error.set(null);
    this.isStreaming.set(true);

    this.sub = this.grpc.streamProducts().subscribe({
      next: (product) => {
        this.streamedProducts.update((list) => [...list, product]);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Błąd streamu');
        this.isStreaming.set(false);
      },
      complete: () => {
        this.isStreaming.set(false);
        this.isDone.set(true);
      },
    });
  }

  stopStream(): void {
    this.sub?.unsubscribe();
    this.isStreaming.set(false);
  }

  ngOnDestroy(): void {
    this.stopStream();
  }
}
