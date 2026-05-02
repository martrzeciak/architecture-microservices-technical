using MassTransit;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderService.Data;
using OrderService.Events;
using OrderService.Saga;
using OrderService.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// pre-warm thread pool so the first requests don't queue up
ThreadPool.SetMinThreads(100, 100);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

// set up MassTransit with RabbitMQ and the EF Core outbox
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrdersDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddSagaStateMachine<OrderProcessingSaga, OrderSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrdersDbContext>();
            r.UsePostgres();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});

// set up metrics and tracing
var resourceBuilder = ResourceBuilder.CreateDefault().AddService("OrderService");
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
        .AddSource("MassTransit")
        .AddOtlpExporter(opt => opt.Endpoint = new Uri("http://jaeger:4317")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003); // REST
    options.ListenAnyIP(5004, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2); // gRPC
    options.ListenAnyIP(5005, o => // gRPC-Web over HTTPS
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
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseGrpcWeb();

app.MapGrpcService<OrderGrpcService>().EnableGrpcWeb();
app.MapPrometheusScrapingEndpoint();
app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/healthz/ready");

app.MapPost("/api/orders", async (OrdersDbContext db, IPublishEndpoint publishEndpoint, CreateOrderDto dto) =>
{
    var entity = new OrderEntity
    {
        CustomerId = dto.CustomerId,
        TotalPrice = dto.Items.Sum(i => i.UnitPrice * i.Quantity),
        Items = dto.Items.Select(i => new OrderItemEntity
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
        }).ToList(),
    };
    db.Orders.Add(entity);

    // publish event via outbox so it's saved atomically with the order
    await publishEndpoint.Publish(new OrderCreated
    {
        OrderId = entity.Id,
        CustomerId = entity.CustomerId,
        TotalPrice = (decimal)entity.TotalPrice,
        CreatedAt = entity.CreatedAt
    });

    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{entity.Id}", entity.ToProto());
});

app.MapGet("/api/orders/{id}", async (OrdersDbContext db, string id) =>
{
    if (!Guid.TryParse(id, out var guid)) return Results.BadRequest("Invalid ID");
    var order = await db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == guid);
    return order is null ? Results.NotFound() : Results.Ok(order.ToProto());
});

app.MapGet("/api/orders", async (OrdersDbContext db, int page = 1, int pageSize = 10) =>
{
    var total = await db.Orders.AsNoTracking().CountAsync();
    var orders = await db.Orders.AsNoTracking().Include(o => o.Items)
        .OrderByDescending(o => o.CreatedAt)
        .Skip((page - 1) * pageSize).Take(pageSize)
        .ToListAsync();
    return Results.Ok(new { orders = orders.Select(o => o.ToProto()), totalCount = total });
});

app.Run();
