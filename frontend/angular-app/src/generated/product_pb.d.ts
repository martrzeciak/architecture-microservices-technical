import * as jspb from 'google-protobuf'



export class Product extends jspb.Message {
  getId(): string;
  setId(value: string): Product;

  getName(): string;
  setName(value: string): Product;

  getDescription(): string;
  setDescription(value: string): Product;

  getPrice(): number;
  setPrice(value: number): Product;

  getCategoryId(): string;
  setCategoryId(value: string): Product;

  getStock(): number;
  setStock(value: number): Product;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): Product.AsObject;
  static toObject(includeInstance: boolean, msg: Product): Product.AsObject;
  static serializeBinaryToWriter(message: Product, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): Product;
  static deserializeBinaryFromReader(message: Product, reader: jspb.BinaryReader): Product;
}

export namespace Product {
  export type AsObject = {
    id: string,
    name: string,
    description: string,
    price: number,
    categoryId: string,
    stock: number,
  }
}

export class ListProductsRequest extends jspb.Message {
  getCategoryId(): string;
  setCategoryId(value: string): ListProductsRequest;

  getPage(): number;
  setPage(value: number): ListProductsRequest;

  getPageSize(): number;
  setPageSize(value: number): ListProductsRequest;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ListProductsRequest.AsObject;
  static toObject(includeInstance: boolean, msg: ListProductsRequest): ListProductsRequest.AsObject;
  static serializeBinaryToWriter(message: ListProductsRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ListProductsRequest;
  static deserializeBinaryFromReader(message: ListProductsRequest, reader: jspb.BinaryReader): ListProductsRequest;
}

export namespace ListProductsRequest {
  export type AsObject = {
    categoryId: string,
    page: number,
    pageSize: number,
  }
}

export class ListProductsResponse extends jspb.Message {
  getProductsList(): Array<Product>;
  setProductsList(value: Array<Product>): ListProductsResponse;
  clearProductsList(): ListProductsResponse;
  addProducts(value?: Product, index?: number): Product;

  getTotalCount(): number;
  setTotalCount(value: number): ListProductsResponse;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): ListProductsResponse.AsObject;
  static toObject(includeInstance: boolean, msg: ListProductsResponse): ListProductsResponse.AsObject;
  static serializeBinaryToWriter(message: ListProductsResponse, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): ListProductsResponse;
  static deserializeBinaryFromReader(message: ListProductsResponse, reader: jspb.BinaryReader): ListProductsResponse;
}

export namespace ListProductsResponse {
  export type AsObject = {
    productsList: Array<Product.AsObject>,
    totalCount: number,
  }
}

export class GetProductRequest extends jspb.Message {
  getId(): string;
  setId(value: string): GetProductRequest;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): GetProductRequest.AsObject;
  static toObject(includeInstance: boolean, msg: GetProductRequest): GetProductRequest.AsObject;
  static serializeBinaryToWriter(message: GetProductRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): GetProductRequest;
  static deserializeBinaryFromReader(message: GetProductRequest, reader: jspb.BinaryReader): GetProductRequest;
}

export namespace GetProductRequest {
  export type AsObject = {
    id: string,
  }
}

export class CreateProductRequest extends jspb.Message {
  getName(): string;
  setName(value: string): CreateProductRequest;

  getDescription(): string;
  setDescription(value: string): CreateProductRequest;

  getPrice(): number;
  setPrice(value: number): CreateProductRequest;

  getCategoryId(): string;
  setCategoryId(value: string): CreateProductRequest;

  getStock(): number;
  setStock(value: number): CreateProductRequest;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): CreateProductRequest.AsObject;
  static toObject(includeInstance: boolean, msg: CreateProductRequest): CreateProductRequest.AsObject;
  static serializeBinaryToWriter(message: CreateProductRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): CreateProductRequest;
  static deserializeBinaryFromReader(message: CreateProductRequest, reader: jspb.BinaryReader): CreateProductRequest;
}

export namespace CreateProductRequest {
  export type AsObject = {
    name: string,
    description: string,
    price: number,
    categoryId: string,
    stock: number,
  }
}

export class StreamProductsRequest extends jspb.Message {
  getCategoryId(): string;
  setCategoryId(value: string): StreamProductsRequest;

  serializeBinary(): Uint8Array;
  toObject(includeInstance?: boolean): StreamProductsRequest.AsObject;
  static toObject(includeInstance: boolean, msg: StreamProductsRequest): StreamProductsRequest.AsObject;
  static serializeBinaryToWriter(message: StreamProductsRequest, writer: jspb.BinaryWriter): void;
  static deserializeBinary(bytes: Uint8Array): StreamProductsRequest;
  static deserializeBinaryFromReader(message: StreamProductsRequest, reader: jspb.BinaryReader): StreamProductsRequest;
}

export namespace StreamProductsRequest {
  export type AsObject = {
    categoryId: string,
  }
}

