namespace OrderService.Data;

public enum OrderStatus
{
    Unspecified = 0,
    Pending     = 1,
    Confirmed   = 2,
    Shipped     = 3,
    Delivered   = 4,
    Cancelled   = 5,
}
