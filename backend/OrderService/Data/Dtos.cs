namespace OrderService.Data;

public record CreateOrderItemDto(string ProductId, string ProductName, int Quantity, double UnitPrice);
public record CreateOrderDto(string CustomerId, List<CreateOrderItemDto> Items);
