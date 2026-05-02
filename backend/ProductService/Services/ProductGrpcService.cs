using EShop.ProductService;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;

namespace ProductService.Services;

public class ProductGrpcService(ProductDbContext db) : EShop.ProductService.ProductService.ProductServiceBase
{
    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(request.CategoryId))
            query = query.Where(p => p.CategoryId == request.CategoryId);

        var totalCount = await query.CountAsync(context.CancellationToken);
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var page = request.Page > 0 ? request.Page : 1;

        var entities = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(context.CancellationToken);

        var response = new ListProductsResponse { TotalCount = totalCount };
        response.Products.AddRange(entities.Select(e => e.ToProto()));
        return response;
    }

    public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var entity = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product '{request.Id}' not found."));

        return entity.ToProto();
    }

    public override async Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var entity = new ProductEntity
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            Stock = request.Stock,
        };
        db.Products.Add(entity);
        await db.SaveChangesAsync(context.CancellationToken);
        return entity.ToProto();
    }

    public override async Task StreamProducts(StreamProductsRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(request.CategoryId))
            query = query.Where(p => p.CategoryId == request.CategoryId);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(context.CancellationToken))
        {
            await responseStream.WriteAsync(entity.ToProto());
            // simulate some processing delay per item
            await Task.Delay(100, context.CancellationToken);
        }
    }
}

