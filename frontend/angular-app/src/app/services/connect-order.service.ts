import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { createClient } from '@connectrpc/connect';
import { createGrpcWebTransport } from '@connectrpc/connect-web';
import { OrderService } from '../../generated-connect/order_connect.js';
import type { Order } from '../../generated-connect/order_pb.js';

// gRPC-Web Direct (no proxy, straight to .NET middleware)
const transport = createGrpcWebTransport({
  baseUrl: 'https://167.233.253.101:5005',
});

const client: any = createClient(OrderService, transport);

export interface OrderRequest {
  customerId: string;
  items: { productId: string; productName: string; quantity: number; unitPrice: number }[];
}

@Injectable({ providedIn: 'root' })
export class ConnectOrderService {
  createOrder(order: OrderRequest): Observable<Order> {
    return from(client.createOrder(order) as Promise<Order>);
  }
}
