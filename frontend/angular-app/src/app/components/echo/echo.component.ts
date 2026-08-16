import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { ProtocolSwitcherService, Protocol, Product } from '../../services/protocol-switcher.service';

/**
 * Scenariusz ECHO — czysty narzut protokolu.
 *
 * Endpoint echo zwraca dane wbudowane w kod uslugi: bez zapytania do bazy i bez
 * cache Redis. Dzieki temu zmierzony czas zawiera wylacznie podroz przez siec,
 * serializacje po stronie serwera i deserializacje w przegladarce.
 *
 * Komponent swiadomie NIE renderuje pozycji w tabeli. Wyrysowanie 200 wierszy
 * kosztuje tyle samo dla kazdego protokolu, wiec dodawaloby staly skladnik,
 * ktory tylko rozcienczalby mierzona roznice miedzy protokolami.
 */
@Component({
  selector: 'app-echo',
  imports: [DecimalPipe],
  templateUrl: './echo.component.html',
  styleUrl: './echo.component.css',
})
export class EchoComponent implements OnInit, OnDestroy {
  private switcher = inject(ProtocolSwitcherService);
  private route = inject(ActivatedRoute);

  /** Liczba rekordow w odpowiedzi, z URL: /demo/echo?count=200 */
  readonly count = signal(200);

  readonly products = signal<Product[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly activeProtocol = this.switcher.activeProtocol;
  readonly protocolLabel = this.switcher.protocolLabel;

  readonly received = computed(() => this.products().length);

  readonly totalPrice = computed(() =>
    this.products().reduce((sum, p) => sum + p.price, 0)
  );

  /** Zwiekszany po naniesieniu odpowiedzi na DOM — punkt zaczepienia dla benchmarku */
  readonly loadId = signal(0);

  private sub?: Subscription;
  private routeSub?: Subscription;

  readonly protocols: { value: Protocol; label: string }[] = [
    { value: 'rest', label: 'REST' },
    { value: 'grpc', label: 'gRPC-Web (Envoy)' },
    { value: 'connect', label: 'gRPC-Web (Direct)' },
  ];

  ngOnInit(): void {
    this.routeSub = this.route.queryParamMap.subscribe((params) => {
      const n = Number(params.get('count'));
      this.count.set(Number.isFinite(n) && n > 0 ? n : 200);
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routeSub?.unsubscribe();
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

    this.sub = this.switcher.echoProducts(this.count()).subscribe({
      next: (data) => {
        // REST zwraca { products, totalCount }, gRPC/Connect zwraca tablice
        const list = Array.isArray(data) ? data : (data as any).products ?? [];
        this.products.set(list);
        this.isLoading.set(false);
        this.loadId.update((v) => v + 1);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Blad polaczenia');
        this.isLoading.set(false);
        this.loadId.update((v) => v + 1);
      },
    });
  }
}
