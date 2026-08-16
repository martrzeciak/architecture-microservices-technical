using EShop.ProductService;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ProductService.Data;

namespace ProductService.Services;

public class ProductGrpcService(ProductDbContext db, IDistributedCache cache) : EShop.ProductService.ProductService.ProductServiceBase
{
    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var page = request.Page > 0 ? request.Page : 1;
        bool bypassCache = context.RequestHeaders.Get("x-bypass-cache") != null;
        // Separate cache namespace per protocol: the REST endpoint stores JSON under
        // its own key. The "pb" marker distinguishes this binary format from the JSON
        // one used by earlier versions of this handler.
        string cacheKey = $"grpcpb_products_{request.CategoryId}_{page}_{pageSize}";

        if (!bypassCache)
        {
            // Cached as protobuf bytes rather than JSON. The previous version stored
            // JSON, which forced a JSON deserialization plus a protobuf serialization
            // on every cache hit, while the REST handler returned its cached JSON
            // string verbatim. That asymmetry penalised gRPC in benchmarks for reasons
            // unrelated to the protocols themselves.
            var cached = await cache.GetAsync(cacheKey, context.CancellationToken);
            if (cached is { Length: > 0 })
            {
                try
                {
                    return ListProductsResponse.Parser.ParseFrom(cached);
                }
                catch (InvalidProtocolBufferException)
                {
                    // Unreadable entry: fall through to the database instead of
                    // failing the call with an empty response.
                }
            }
        }

        var query = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(request.CategoryId))
            query = query.Where(p => p.CategoryId == request.CategoryId);

        var totalCount = await query.CountAsync(context.CancellationToken);
        var entities = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(context.CancellationToken);

        var response = new ListProductsResponse { TotalCount = totalCount };
        response.Products.AddRange(entities.Select(e => e.ToProto()));

        await cache.SetAsync(cacheKey,
            response.ToByteArray(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
            context.CancellationToken);

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
        }
    }

    /// <summary>
    /// Echo endpoint: returns hardcoded product data without DB or cache.
    /// Used to isolate pure protocol serialization overhead in benchmarks.
    /// </summary>
    public override Task<ListProductsResponse> EchoProducts(EchoProductsRequest request, ServerCallContext context)
    {
        var count = request.Count > 0 ? request.Count : 200;
        var response = new ListProductsResponse { TotalCount = count };

        for (int i = 1; i <= count; i++)
        {
            response.Products.Add(new Product
            {
                Id = i.ToString(),
                Name = $"Echo Product {i}",
                Description = $"This is a hardcoded echo product number {i} for benchmarking protocol overhead",
                Price = 9.99 * i,
                CategoryId = "echo",
                Stock = 100,
            });
        }

        return Task.FromResult(response);
    }
}

