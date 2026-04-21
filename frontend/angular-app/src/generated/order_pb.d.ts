import * as jspb from 'google-protobuf'



export class Order extends jspb.Message {
  getId(): string;
  setId(value: string): Order;

  getCustomerId(): string;
  setCustomerId(value: string): Order;

  getItemsList(): Array<OrderItem>;
  setItemsList(value: Array<OrderItem>): Order;
  clearItemsList(): Order;
  addItems(value?: OrderItem, index?: number): OrderItem;

  getTotalPrice(): number;
  setTotalPrice(value: number): Order;

  getStatus(): OrderStatus;
  setStatus(value: OrderStatus): Order;

  getCreatedAt(): string;
  setCreatedAt(value: string): Order;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Order.AsObject;
  static toObject(includeInstance: boolean, msg: Order): Order.AsObject;
  static serializeBinaryToWriter(message: Order, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Order;
  static deserializeBinaryFromReader(message: Order, reader: jspb.BinaryReader): Order;
}

export namespace Order {
  export type AsObject = {
    id: string,
    customerId: string,
    itemsList: Array<OrderItem.AsObject>,
    totalPrice: number,
    status: OrderStatus,
    createdAt: string,
  }
}

export class OrderItem extends jspb.Message {
  getProductId(): string;
  setProductId(value: string): OrderItem;

  getProductName(): string;
  setProductName(value: string): OrderItem;

  getQuantity(): number;
  setQuantity(value: number): OrderItem;

  getUnitPrice(): number;
  setUnitPrice(value: number): OrderItem;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): OrderItem.AsObject;
  static toObject(includeInstance: boolean, msg: OrderItem): OrderItem.AsObject;
  static serializeBinaryToWriter(message: OrderItem, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): OrderItem;
  static deserializeBinaryFromReader(message: OrderItem, reader: jspb.BinaryReader): OrderItem;
}

export namespace OrderItem {
  export type AsObject = {
    productId: string,
    productName: string,
    quantity: number,
    unitPrice: number,
  }
}

export class CreateOrderRequest extends jspb.Message {
  getCustomerId(): string;
  setCustomerId(value: string): CreateOrderRequest;

  getItemsList(): Array<OrderItem>;
  setItemsList(value: Array<OrderItem>): CreateOrderRequest;
  clearItemsList(): CreateOrderRequest;
  addItems(value?: OrderItem, index?: number): OrderItem;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): CreateOrderRequest.AsObject;
  static toObject(includeInstance: boolean, msg: CreateOrderRequest): CreateOrderRequest.AsObject;
  static serializeBinaryToWriter(message: CreateOrderRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): CreateOrderRequest;
  static deserializeBinaryFromReader(message: CreateOrderRequest, reader: jspb.BinaryReader): CreateOrderRequest;
}

export namespace CreateOrderRequest {
  export type AsObject = {
    customerId: string,
    itemsList: Array<OrderItem.AsObject>,
  }
}

export class GetOrderRequest extends jspb.Message {
  getId(): string;
  setId(value: string): GetOrderRequest;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): GetOrderRequest.AsObject;
  static toObject(includeInstance: boolean, msg: GetOrderRequest): GetOrderRequest.AsObject;
  static serializeBinaryToWriter(message: GetOrderRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): GetOrderRequest;
  static deserializeBinaryFromReader(message: GetOrderRequest, reader: jspb.BinaryReader): GetOrderRequest;
}

export namespace GetOrderRequest {
  export type AsObject = {
    id: string,
  }
}

export enum OrderStatus { 
  ORDER_STATUS_UNSPECIFIED = 0,
  ORDER_STATUS_PENDING = 1,
  ORDER_STATUS_CONFIRMED = 2,
  ORDER_STATUS_SHIPPED = 3,
  ORDER_STATUS_DELIVERED = 4,
  ORDER_STATUS_CANCELLED = 5,
}
