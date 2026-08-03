namespace OrderService.Events;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PaymentReceived
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record OrderCompleted
{
    public Guid OrderId { get; init; }
}

public record OrderCancelled
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
