import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { createClient } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';
import { ProductService } from '../../generated-connect/product_connect.js';
import type { Product } from '../../generated-connect/product_pb.js';

// Natywny gRPC-Web przez Envoy proxy (:8080) — transport identyczny z grpc-web,
// ale używamy connect-web (ESM) zamiast google/grpc-web (CommonJS, niekompatybilny z esbuild)
const transport = createGrpcWebTransport({
  baseUrl: 'http://167.233.253.101:8080', // przez Envoy
});

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const client: any = createClient(ProductService, transport);

@Injectable({ providedIn: 'root' })
export class GrpcProductService {
  /**
   * @param pageSize liczba produktow na strone (parytet z PAGE_SIZE w testach k6)
   * @param bypassCache pomija cache Redis po stronie serwera (scenariusz zimnego cache)
   */
  listProducts(pageSize = 10, bypassCache = false): Observable<Product[]> {
    const options = bypassCache ? { headers: { 'x-bypass-cache': 'true' } } : undefined;
    const p: Promise<Product[]> = client
      .listProducts({ page: 1, pageSize }, options)
      .then((r: any) => r.products as Product[]);
    return from(p);
  }

  getProduct(id: string): Observable<Product> {
    return from(client.getProduct({ id }) as Promise<Product>);
  }

  /**
   * Endpoint echo: dane wbudowane w kod, bez bazy i bez cache.
   * Izoluje czysty narzut protokolu (parytet ze scenariuszami echo w k6).
   */
  echoProducts(count = 200): Observable<Product[]> {
    const p: Promise<Product[]> = client
      .echoProducts({ count })
      .then((r: any) => r.products as Product[]);
    return from(p);
  }

  streamProducts(): Observable<Product> {
    return new Observable((observer) => {
      (async () => {
        try {
          for await (const product of client.streamProducts({})) {
            observer.next(product as Product);
          }
          observer.complete();
        } catch (err) {
          observer.error(err);
        }
      })();
    });
  }
}
