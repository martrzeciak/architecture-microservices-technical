using EShop.ProductService;
using Grpc.Core;

namespace ProductService.Services;

public class ProductGrpcService : EShop.ProductService.ProductService.ProductServiceBase
{
    private static readonly List<Product> _products =
    [
        new Product { Id = "1", Name = "Laptop Pro 15", Description = "High-performance laptop", Price = 2499.99, CategoryId = "electronics", Stock = 10 },
        new Product { Id = "2", Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 49.99, CategoryId = "electronics", Stock = 100 },
        new Product { Id = "3", Name = "Mechanical Keyboard", Description = "TKL mechanical keyboard", Price = 129.99, CategoryId = "electronics", Stock = 50 },
        new Product { Id = "4", Name = "USB-C Hub", Description = "7-in-1 USB-C hub", Price = 39.99, CategoryId = "electronics", Stock = 200 },
        new Product { Id = "5", Name = "Monitor 27\"", Description = "4K IPS display 27 inch", Price = 599.99, CategoryId = "electronics", Stock = 25 },
    ];

    public static IEnumerable<Product> GetProducts() => _products;

    public static Product AddProduct(string name, string description, double price, string categoryId, int stock)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            Stock = stock,
        };
        _products.Add(product);
        return product;
    }

    public override Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var products = _products.AsEnumerable();

        if (!string.IsNullOrEmpty(request.CategoryId))
            products = products.Where(p => p.CategoryId == request.CategoryId);

        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var page = request.Page > 0 ? request.Page : 1;

        var paged = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var response = new ListProductsResponse { TotalCount = products.Count() };
        response.Products.AddRange(paged);

        return Task.FromResult(response);
    }

    public override Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var product = _products.FirstOrDefault(p => p.Id == request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Product '{request.Id}' not found."));

        return Task.FromResult(product);
    }

    public override Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = AddProduct(request.Name, request.Description, request.Price, request.CategoryId, request.Stock);
        return Task.FromResult(product);
    }

    public override async Task StreamProducts(StreamProductsRequest request, IServerStreamWriter<Product> responseStream, ServerCallContext context)
    {
        var products = _products.AsEnumerable();

        if (!string.IsNullOrEmpty(request.CategoryId))
            products = products.Where(p => p.CategoryId == request.CategoryId);

        foreach (var product in products)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await responseStream.WriteAsync(product);
            await Task.Delay(100, context.CancellationToken);
        }
    }
}
