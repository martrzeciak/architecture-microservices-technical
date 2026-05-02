using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductService.Data;
using ProductService.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// pre-warm thread pool so the first requests don't queue up
ThreadPool.SetMinThreads(100, 100);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// use a pool of db contexts instead of creating a new one per request
builder.Services.AddDbContextPool<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "ProductService_";
});

// set up metrics and tracing
var resourceBuilder = ResourceBuilder.CreateDefault().AddService("ProductService");
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter())
    .WithTracing(tracing => tracing
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(opt => opt.Endpoint = new Uri("http://jaeger:4317")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // REST
    options.ListenAnyIP(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2); // gRPC
    options.ListenAnyIP(5002, o => // gRPC-Web over HTTPS
    {
        o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
        o.UseHttps("/certs/server.pfx", "benchmark");
    });

    options.Limits.MaxConcurrentConnections = 10_000;
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
});

var app = builder.Build();

// run pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseGrpcWeb();

app.MapGrpcService<ProductGrpcService>().EnableGrpcWeb();
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/healthz/ready");

app.MapGet("/api/products", async (HttpContext context, ProductDbContext db, IDistributedCache cache, string? categoryId, int page = 1, int pageSize = 10) =>
{
    bool bypassCache = context.Request.Headers.ContainsKey("X-Bypass-Cache");
    string cacheKey = $"products_{categoryId}_{page}_{pageSize}";

    if (!bypassCache)
    {
        var cachedData = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData)) return Results.Content(cachedData, "application/json");
    }

    var query = db.Products.AsNoTracking().AsQueryable();
    if (!string.IsNullOrEmpty(categoryId)) query = query.Where(p => p.CategoryId == categoryId);

    var total = await query.CountAsync();
    var products = await query.OrderBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    var response = new { products = products.Select(e => e.ToProto()), totalCount = total };
    var jsonResponse = JsonSerializer.Serialize(response);

    // cache result for 5 minutes
    await cache.SetStringAsync(cacheKey, jsonResponse, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    return Results.Content(jsonResponse, "application/json");
});

app.MapGet("/api/products/{id}", async (ProductDbContext db, string id) =>
{
    var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product.ToProto());
});

app.MapPost("/api/products", async (ProductDbContext db, CreateProductDto dto) =>
{
    var entity = new ProductEntity
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        CategoryId = dto.CategoryId,
        Stock = dto.Stock,
    };
    db.Products.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{entity.Id}", entity.ToProto());
});

app.Run();
