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

  /**
   * @param pageSize liczba produktow na strone
   * @param bypassCache pomija cache Redis (scenariusz zimnego cache)
   */
  listProducts(pageSize = 10, bypassCache = false): Observable<Product[]> {
    switch (this.activeProtocol()) {
      case 'rest':
        return this.rest.listProducts(pageSize, bypassCache) as unknown as Observable<Product[]>;
      case 'connect':
        return this.connect.listProducts(pageSize, bypassCache) as unknown as Observable<Product[]>;
      case 'grpc':
      default:
        return this.grpc.listProducts(pageSize, bypassCache) as unknown as Observable<Product[]>;
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

  /**
   * Endpoint echo — bez bazy i bez cache, izoluje narzut protokolu.
   * @param count liczba zwracanych rekordow
   */
  echoProducts(count = 200): Observable<Product[]> {
    switch (this.activeProtocol()) {
      case 'rest':
        return this.rest.echoProducts(count) as unknown as Observable<Product[]>;
      case 'connect':
        return this.connect.echoProducts(count) as unknown as Observable<Product[]>;
      case 'grpc':
      default:
        return this.grpc.echoProducts(count) as unknown as Observable<Product[]>;
    }
  }
}
