import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface OrderRequest {
  customerId: string;
  items: { productId: string; productName: string; quantity: number; unitPrice: number }[];
}

export interface OrderResponse {
  id: string;
  customerId: string;
  totalPrice: number;
  status: number;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class RestOrderService {
  private readonly baseUrl = 'http://167.233.253.101:5003/api';
  private http = inject(HttpClient);

  createOrder(order: OrderRequest): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(`${this.baseUrl}/orders`, order);
  }
}
