using EShop.OrderService;
using Grpc.Core;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Events;

namespace OrderService.Services;

public class OrderGrpcService(OrdersDbContext db, IPublishEndpoint publishEndpoint) : EShop.OrderService.OrderService.OrderServiceBase
{
    public override async Task<Order> CreateOrder(CreateOrderRequest request, ServerCallContext context)
    {
        var entity = new OrderEntity
        {
            CustomerId = request.CustomerId,
            TotalPrice = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            Items = request.Items.Select(i => new OrderItemEntity
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList(),
        };

        db.Orders.Add(entity);

        await publishEndpoint.Publish(new OrderCreated
        {
            OrderId = entity.Id,
            CustomerId = entity.CustomerId,
            TotalPrice = (decimal)entity.TotalPrice,
            CreatedAt = entity.CreatedAt
        }, context.CancellationToken);

        await db.SaveChangesAsync(context.CancellationToken);

        return entity.ToProto();
    }

    public override async Task<Order> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format."));

        var entity = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Order '{request.Id}' not found."));

        return entity.ToProto();
    }
}
