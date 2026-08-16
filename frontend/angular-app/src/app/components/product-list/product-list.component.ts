import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
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
  private route = inject(ActivatedRoute);

  /**
   * Parametry scenariusza czytane z URL, np. /demo/products?pageSize=200&cold=1
   * Odpowiadaja zmiennym PAGE_SIZE i BYPASS_CACHE ze scenariuszy k6, dzieki czemu
   * ten sam wymiar badania da sie odtworzyc z poziomu przegladarki.
   */
  readonly pageSize = signal(10);
  readonly coldCache = signal(false);

  readonly products = signal<Product[]>([]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  /**
   * Incremented every time a load finishes (success or failure) and the DOM has
   * been updated. Exposed as data-load-id so automated benchmarks can detect
   * completion of THIS request without polling for element visibility, which is
   * too slow to observe sub-100ms round trips reliably.
   */
  readonly loadId = signal(0);
  readonly activeProtocol = this.switcher.activeProtocol;
  readonly protocolLabel = this.switcher.protocolLabel;

  /**
   * Suma po WSZYSTKICH odebranych rekordach — wymusza pelne przejscie odpowiedzi,
   * takze tych pozycji, ktore nie trafiaja do tabeli.
   */
  readonly totalPrice = computed(() =>
    this.products().reduce((sum, p) => sum + p.price, 0)
  );

  /**
   * Gorny limit wierszy renderowanych w tabeli.
   *
   * Kazdy wiersz to szesc komorek, wywolanie DecimalPipe i instancja RouterLink.
   * Przy duzych stronach (1000-2000 rekordow) koszt samego rysowania siega setek
   * milisekund i jest identyczny dla wszystkich protokolow, wiec zalewalby
   * mierzona roznice miedzy nimi. Odpowiedz jest deserializowana w calosci
   * (patrz totalPrice powyzej), renderowany jest tylko poczatek listy.
   */
  static readonly RenderLimit = 100;

  readonly visibleProducts = computed(() =>
    this.products().slice(0, ProductListComponent.RenderLimit)
  );

  readonly isTruncated = computed(
    () => this.products().length > ProductListComponent.RenderLimit
  );

  readonly renderLimit = ProductListComponent.RenderLimit;

  private sub?: Subscription;
  private routeSub?: Subscription;

  ngOnInit(): void {
    this.routeSub = this.route.queryParamMap.subscribe((params) => {
      const ps = Number(params.get('pageSize'));
      this.pageSize.set(Number.isFinite(ps) && ps > 0 ? ps : 10);
      this.coldCache.set(params.get('cold') === '1');
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

    this.sub = this.switcher.listProducts(this.pageSize(), this.coldCache()).subscribe({
      next: (data) => {
        // REST zwraca { products: [], totalCount: n }, gRPC/Connect zwraca tablicę
        const list = Array.isArray(data) ? data : (data as any).products ?? [];
        this.products.set(list);
        this.isLoading.set(false);
        this.loadId.update((v) => v + 1);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Błąd połączenia');
        this.isLoading.set(false);
        this.loadId.update((v) => v + 1);
      },
    });
  }

  readonly protocols: { value: Protocol; label: string }[] = [
    { value: 'rest', label: 'REST' },
    { value: 'grpc', label: 'gRPC-Web (Envoy)' },
    { value: 'connect', label: 'gRPC-Web (Direct)' },
  ];
}
