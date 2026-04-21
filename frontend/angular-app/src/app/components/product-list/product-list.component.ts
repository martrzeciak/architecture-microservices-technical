import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { ProtocolSwitcherService, Protocol, Product } from '../../services/protocol-switcher.service';

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css',
})
export class ProductListComponent implements OnInit, OnDestroy {
  private switcher = inject(ProtocolSwitcherService);

  readonly products = signal<Product[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly activeProtocol = this.switcher.activeProtocol;
  readonly protocolLabel = this.switcher.protocolLabel;
  readonly totalPrice = computed(() =>
    this.products().reduce((sum, p) => sum + p.price, 0)
  );

  private sub?: Subscription;

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  setProtocol(p: Protocol): void {
    this.switcher.setProtocol(p);
    this.load();
  }

  load(): void {
    this.sub?.unsubscribe();
    this.isLoading.set(true);
    this.error.set(null);
    this.products.set([]);

    this.sub = this.switcher.listProducts().subscribe({
      next: (data) => {
        // REST zwraca { products: [], totalCount: n }, gRPC/Connect zwraca tablicę
        const list = Array.isArray(data) ? data : (data as any).products ?? [];
        this.products.set(list);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Błąd połączenia');
        this.isLoading.set(false);
      },
    });
  }

  readonly protocols: { value: Protocol; label: string }[] = [
    { value: 'rest', label: 'REST' },
    { value: 'grpc', label: 'gRPC-Web (Envoy)' },
    { value: 'connect', label: 'gRPC-Web (Direct)' },
  ];
}
