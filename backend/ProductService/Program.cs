using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

var app = builder.Build();

app.MapGrpcService<ProductGrpcService>();

app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/healthz/ready");

// REST endpoints — ten sam backend, do porównania wydajności z gRPC
app.MapGet("/api/products", (string? categoryId, int page = 1, int pageSize = 10) =>
{
    var products = ProductGrpcService.GetProducts();
    if (!string.IsNullOrEmpty(categoryId))
        products = products.Where(p => p.CategoryId == categoryId);
    var paged = products.Skip((page - 1) * pageSize).Take(pageSize);
    return Results.Ok(new { products = paged, totalCount = products.Count() });
});

app.MapGet("/api/products/{id}", (string id) =>
{
    var product = ProductGrpcService.GetProducts().FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/api/products", (CreateProductDto dto) =>
{
    var product = ProductGrpcService.AddProduct(dto.Name, dto.Description, dto.Price, dto.CategoryId, dto.Stock);
    return Results.Created($"/api/products/{product.Id}", product);
});

app.Run();

record CreateProductDto(string Name, string Description, double Price, string CategoryId, int Stock);
