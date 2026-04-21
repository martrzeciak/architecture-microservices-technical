import { Component, OnInit, signal, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { Subscription } from 'rxjs';
import { ProtocolSwitcherService, Product } from '../../services/protocol-switcher.service';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
})
export class ProductDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private switcher = inject(ProtocolSwitcherService);

  readonly product = signal<Product | null>(null);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly protocolLabel = this.switcher.protocolLabel;

  private sub?: Subscription;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.isLoading.set(true);
    this.sub = this.switcher.getProduct(id).subscribe({
      next: (p) => {
        this.product.set(p as unknown as Product);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err?.message ?? 'Produkt nie znaleziony');
        this.isLoading.set(false);
      },
    });
  }
}
