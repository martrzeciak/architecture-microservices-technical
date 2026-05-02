namespace OrderService.Data;

public class OrderEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerId { get; set; } = string.Empty;
    public double TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItemEntity> Items { get; set; } = [];
}
