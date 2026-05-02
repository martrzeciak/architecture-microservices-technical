using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Saga;

namespace OrderService.Data;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OrderSagaState> OrderSagas => Set<OrderSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<OrderSagaState>(e =>
        {
            e.HasKey(s => s.CorrelationId);
            e.Property(s => s.CurrentState).HasMaxLength(64);
        });

        modelBuilder.Entity<OrderEntity>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.CustomerId).IsRequired().HasMaxLength(200);
            e.HasIndex(o => o.CreatedAt);
            e.HasMany(o => o.Items)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemEntity>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.ProductId).IsRequired().HasMaxLength(200);
            e.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
        });
    }
}
