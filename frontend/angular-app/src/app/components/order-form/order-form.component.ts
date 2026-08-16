import { Component, signal, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProtocolSwitcherService, Protocol } from '../../services/protocol-switcher.service';
import { RestOrderService, OrderRequest } from '../../services/rest-order.service';
import { GrpcOrderService } from '../../services/grpc-order.service';
import { ConnectOrderService } from '../../services/connect-order.service';
import { Subscription } from 'rxjs';

/**
 * Pula produktow zgodna z danymi zasiewowymi (seed) uslugi ProductService.
 * Identyczna z tablica PRODUCTS w tests/k6/config.js, aby ladunek zadania
 * mial ten sam rozklad i rozmiar co w testach k6.
 */
const PRODUCTS = [
  { productId: '1', productName: 'Laptop Pro 15', unitPrice: 2499.99 },
  { productId: '2', productName: 'Wireless Mouse', unitPrice: 49.99 },
  { productId: '3', productName: 'Mechanical Keyboard', unitPrice: 129.99 },
  { productId: '4', productName: 'USB-C Hub', unitPrice: 39.99 },
  { productId: '5', productName: 'Monitor 27"', unitPrice: 599.99 },
  { productId: '6', productName: 'Gaming Headset', unitPrice: 89.99 },
  { productId: '8', productName: 'SSD 1TB', unitPrice: 109.99 },
  { productId: '9', productName: 'RAM 32GB DDR5', unitPrice: 179.99 },
  { productId: '15', productName: 'USB Microphone', unitPrice: 79.99 },
  { productId: '17', productName: 'Power Bank 20000mAh', unitPrice: 59.99 },
];

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.css',
})
export class OrderFormComponent implements OnInit, OnDestroy {
  private switcher = inject(ProtocolSwitcherService);
  private restOrder = inject(RestOrderService);
  private grpcOrder = inject(GrpcOrderService);
  private connectOrder = inject(ConnectOrderService);
  private route = inject(ActivatedRoute);

  /**
   * Liczba pozycji zamowienia, czytana z URL: /demo/orders?items=5
   * Odpowiada zmiennej ORDER_ITEMS ze scenariuszy k6.
   */
  readonly itemCount = signal(1);

  readonly activeProtocol = this.switcher.activeProtocol;
  readonly protocolLabel = this.switcher.protocolLabel;

  readonly isSubmitting = signal(false);
  readonly lastOrderId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly orderCount = signal(0);

  /**
   * Incremented every time a submit finishes (success or failure). Exposed as
   * data-submit-id so automated benchmarks can detect completion of THIS request
   * precisely, instead of polling for a banner to appear.
   */
  readonly submitId = signal(0);

  private sub?: Subscription;
  private routeSub?: Subscription;

  ngOnInit(): void {
    this.routeSub = this.route.queryParamMap.subscribe((params) => {
      const n = Number(params.get('items'));
      this.itemCount.set(Number.isFinite(n) && n > 0 ? n : 1);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }

  readonly protocols: { value: Protocol; label: string }[] = [
    { value: 'rest', label: 'REST' },
    { value: 'grpc', label: 'gRPC-Web (Envoy)' },
    { value: 'connect', label: 'gRPC-Web (Direct)' },
  ];

  setProtocol(p: Protocol): void {
    this.switcher.setProtocol(p);
  }

  submitOrder(): void {
    this.sub?.unsubscribe();
    this.isSubmitting.set(true);
    this.error.set(null);
    this.lastOrderId.set(null);

    // Losowe pozycje z puli produktow zasiewowych — tak samo jak buildOrderItems() w k6
    const order: OrderRequest = {
      customerId: `customer-${(this.orderCount() % 3) + 1}`,
      items: Array.from({ length: this.itemCount() }, () => {
        const p = PRODUCTS[Math.floor(Math.random() * PRODUCTS.length)];
        return {
          productId: p.productId,
          productName: p.productName,
          quantity: Math.ceil(Math.random() * 3),
          unitPrice: p.unitPrice,
        };
      }),
    };

    let obs;
    switch (this.activeProtocol()) {
      case 'rest':
        obs = this.restOrder.createOrder(order);
        break;
      case 'connect':
        obs = this.connectOrder.createOrder(order);
        break;
      case 'grpc':
      default:
        obs = this.grpcOrder.createOrder(order);
        break;
    }

    this.sub = obs.subscribe({
      next: (res: any) => {
        this.lastOrderId.set(res.id || res.Id || 'OK');
        this.isSubmitting.set(false);
        this.orderCount.update(c => c + 1);
        this.submitId.update(v => v + 1);
      },
      error: (err: any) => {
        this.error.set(err?.message ?? 'Order failed');
        this.isSubmitting.set(false);
        this.submitId.update(v => v + 1);
      },
    });
  }
}
