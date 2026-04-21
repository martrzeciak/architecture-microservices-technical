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
  private readonly baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  listProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }
}
