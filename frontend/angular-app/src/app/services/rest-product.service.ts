import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
}

@Injectable({ providedIn: 'root' })
export class RestProductService {
  private readonly baseUrl = 'http://167.233.253.101:5000/api';

  constructor(private http: HttpClient) {}

  /**
   * @param pageSize liczba produktow na strone (parytet z PAGE_SIZE w testach k6)
   * @param bypassCache pomija cache Redis po stronie serwera (scenariusz zimnego cache)
   */
  listProducts(pageSize = 10, bypassCache = false): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`, {
      params: { page: 1, pageSize },
      headers: bypassCache ? { 'X-Bypass-Cache': 'true' } : {},
    });
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }

  /**
   * Endpoint echo: dane wbudowane w kod, bez bazy i bez cache.
   * Izoluje czysty narzut protokolu (parytet ze scenariuszami echo w k6).
   */
  echoProducts(count = 200): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/echo`, { params: { count } });
  }
}
