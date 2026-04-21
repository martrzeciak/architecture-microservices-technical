import { Injectable, signal, computed } from '@angular/core';
import { Observable } from 'rxjs';
import { RestProductService, Product as RestProduct } from './rest-product.service';
import { GrpcProductService } from './grpc-product.service';
import { ConnectProductService } from './connect-product.service';

export type Protocol = 'rest' | 'grpc' | 'connect';

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  categoryId?: string;
  stock: number;
}

@Injectable({ providedIn: 'root' })
export class ProtocolSwitcherService {
  readonly activeProtocol = signal<Protocol>('grpc');
  readonly protocolLabel = computed(() => {
    const labels: Record<Protocol, string> = {
      rest: 'REST (HTTP/JSON → :5000)',
      grpc: 'gRPC-Web via Envoy (:8080 → :5001)',
      connect: 'gRPC-Web Direct (:5002, bez proxy)',
    };
    return labels[this.activeProtocol()];
  });

  constructor(
    private rest: RestProductService,
    private grpc: GrpcProductService,
    private connect: ConnectProductService,
  ) {}

  setProtocol(p: Protocol): void {
    this.activeProtocol.set(p);
  }

  listProducts(): Observable<Product[]> {
    switch (this.activeProtocol()) {
      case 'rest':
        return this.rest.listProducts() as unknown as Observable<Product[]>;
      case 'connect':
        return this.connect.listProducts() as unknown as Observable<Product[]>;
      case 'grpc':
      default:
        return this.grpc.listProducts() as unknown as Observable<Product[]>;
    }
  }

  getProduct(id: string): Observable<Product> {
    switch (this.activeProtocol()) {
      case 'rest':
        return this.rest.getProduct(id) as unknown as Observable<Product>;
      case 'connect':
        return this.connect.getProduct(id) as unknown as Observable<Product>;
      case 'grpc':
      default:
        return this.grpc.getProduct(id) as unknown as Observable<Product>;
    }
  }
}
