import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { GrpcProductService } from './services/grpc-product.service';
import { RestProductService } from './services/rest-product.service';
import { ConnectProductService } from './services/connect-product.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  protected readonly title = signal('angular-app');

  constructor(
    private grpcProductService: GrpcProductService,
    private restProductService: RestProductService,
    private connectProductService: ConnectProductService,
  ) {}

  ngOnInit(): void {
    console.log('[REST] Calling GET /api/products...');
    this.restProductService.listProducts().subscribe({
      next: (products) => console.log('[REST] Products:', products),
      error: (err) => console.error('[REST] Error:', err),
    });

    console.log('[gRPC-Web] Calling ListProducts via Envoy...');
    this.grpcProductService.listProducts().subscribe({
      next: (products) => console.log('[gRPC-Web] Products:', products),
      error: (err) => console.error('[gRPC-Web] Error:', err),
    });

    console.log('[Connect] Calling ListProducts via ConnectRPC...');
    this.connectProductService.listProducts().subscribe({
      next: (products) => console.log('[Connect] Products:', products),
      error: (err) => console.error('[Connect] Error:', err),
    });
  }
}

