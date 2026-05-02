namespace OrderService.Events;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime CreatedAt { get; init; }
}
