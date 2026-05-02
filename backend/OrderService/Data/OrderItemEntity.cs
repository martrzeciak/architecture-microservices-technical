namespace OrderService.Data;

public class OrderItemEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }

    public OrderEntity Order { get; set; } = null!;
}
