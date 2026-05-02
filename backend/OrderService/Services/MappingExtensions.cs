using OrderService.Data;

namespace OrderService.Services;

public static class MappingExtensions
{
    public static EShop.OrderService.Order ToProto(this OrderEntity entity)
    {
        var proto = new EShop.OrderService.Order
        {
            Id = entity.Id.ToString(),
            CustomerId = entity.CustomerId,
            TotalPrice = (double)entity.TotalPrice,
            CreatedAt = entity.CreatedAt.ToString("o")
        };
        proto.Items.AddRange(entity.Items.Select(i => new EShop.OrderService.OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = (double)i.UnitPrice
        }));
        return proto;
    }
}
