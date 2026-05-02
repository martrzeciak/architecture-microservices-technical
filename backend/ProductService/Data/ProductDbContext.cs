using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasMaxLength(1000);
            e.Property(p => p.CategoryId).HasMaxLength(100);
            e.HasIndex(p => p.CategoryId);
        });

        modelBuilder.Entity<ProductEntity>().HasData(GenerateSeedProducts());
    }

    private static ProductEntity[] GenerateSeedProducts()
    {
        var categories = new[] { "electronics", "storage", "components", "accessories", "furniture", "audio", "mobile", "networking", "peripherals", "software" };

        var baseProducts = new ProductEntity[]
        {
            new() { Id = "1", Name = "Laptop Pro 15", Description = "High-performance laptop", Price = 2499.99, CategoryId = "electronics", Stock = 10 },
            new() { Id = "2", Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Price = 49.99, CategoryId = "electronics", Stock = 100 },
            new() { Id = "3", Name = "Mechanical Keyboard", Description = "TKL mechanical keyboard", Price = 129.99, CategoryId = "electronics", Stock = 50 },
            new() { Id = "4", Name = "USB-C Hub", Description = "7-in-1 USB-C hub", Price = 39.99, CategoryId = "electronics", Stock = 200 },
            new() { Id = "5", Name = "Monitor 27\"", Description = "4K IPS display 27 inch", Price = 599.99, CategoryId = "electronics", Stock = 25 },
            new() { Id = "6", Name = "Gaming Headset", Description = "7.1 surround sound headset", Price = 89.99, CategoryId = "electronics", Stock = 75 },
            new() { Id = "7", Name = "Webcam 4K", Description = "4K UHD webcam with autofocus", Price = 149.99, CategoryId = "electronics", Stock = 40 },
            new() { Id = "8", Name = "SSD 1TB", Description = "NVMe PCIe Gen4 SSD", Price = 109.99, CategoryId = "storage", Stock = 150 },
            new() { Id = "9", Name = "RAM 32GB DDR5", Description = "DDR5 5600MHz dual channel", Price = 179.99, CategoryId = "components", Stock = 60 },
            new() { Id = "10", Name = "RTX 4070 GPU", Description = "NVIDIA GeForce RTX 4070 12GB", Price = 1299.99, CategoryId = "components", Stock = 15 },
            new() { Id = "11", Name = "HDMI Cable 2m", Description = "4K 60Hz HDMI 2.1 cable", Price = 12.99, CategoryId = "accessories", Stock = 500 },
            new() { Id = "12", Name = "Mouse Pad XL", Description = "Extended gaming mouse pad", Price = 24.99, CategoryId = "accessories", Stock = 300 },
            new() { Id = "13", Name = "Standing Desk", Description = "Electric height-adjustable desk", Price = 499.99, CategoryId = "furniture", Stock = 20 },
            new() { Id = "14", Name = "Office Chair", Description = "Ergonomic lumbar support chair", Price = 349.99, CategoryId = "furniture", Stock = 30 },
            new() { Id = "15", Name = "USB Microphone", Description = "Condenser microphone for streaming", Price = 79.99, CategoryId = "audio", Stock = 80 },
            new() { Id = "16", Name = "Laptop Stand", Description = "Aluminium adjustable laptop stand", Price = 34.99, CategoryId = "accessories", Stock = 120 },
            new() { Id = "17", Name = "Power Bank 20000mAh", Description = "PD 65W fast charge power bank", Price = 59.99, CategoryId = "mobile", Stock = 90 },
            new() { Id = "18", Name = "Smart Switch 8-port", Description = "Managed gigabit switch", Price = 89.99, CategoryId = "networking", Stock = 45 },
            new() { Id = "19", Name = "Thunderbolt 4 Dock", Description = "14-in-1 Thunderbolt 4 docking station", Price = 249.99, CategoryId = "electronics", Stock = 35 },
            new() { Id = "20", Name = "Portable SSD 2TB", Description = "USB-C portable SSD 1050 MB/s", Price = 139.99, CategoryId = "storage", Stock = 65 },
        };

        // Generate 180 additional products for payload variation benchmarks (pageSize=50/100/200)
        var productTemplates = new[]
        {
            ("Wireless Earbuds {0}", "Bluetooth 5.3 earbuds with ANC", 79.99, "audio"),
            ("USB Flash Drive {0}GB", "USB 3.2 Gen2 flash drive", 19.99, "storage"),
            ("DisplayPort Cable {0}m", "DP 1.4 HBR3 cable", 14.99, "accessories"),
            ("Network Card {0}GbE", "PCIe network interface card", 29.99, "networking"),
            ("Cooling Pad {0}", "Laptop cooling pad with fans", 24.99, "accessories"),
            ("Webcam Cover {0}", "Privacy slide cover for webcam", 4.99, "accessories"),
            ("Type-C Adapter {0}", "USB-C to USB-A adapter", 9.99, "accessories"),
            ("DDR4 RAM {0}GB", "DDR4 3200MHz memory module", 49.99, "components"),
            ("PCIe SSD {0}GB", "NVMe M.2 2280 SSD", 89.99, "storage"),
            ("Desk Lamp {0}W", "LED desk lamp with dimmer", 34.99, "furniture"),
            ("Cable Management Kit {0}", "Under-desk cable organizer", 19.99, "accessories"),
            ("Wireless Charger {0}W", "Qi wireless charging pad", 24.99, "mobile"),
            ("Phone Stand {0}", "Adjustable phone holder", 14.99, "mobile"),
            ("Smart Plug {0}", "WiFi smart plug with timer", 12.99, "networking"),
            ("Ethernet Cable Cat{0}", "Shielded ethernet cable 3m", 8.99, "networking"),
            ("Mini Speaker {0}", "Portable Bluetooth speaker", 39.99, "audio"),
            ("Screen Protector {0}", "Tempered glass screen protector", 9.99, "mobile"),
            ("Drawing Tablet {0}", "Graphics drawing tablet USB", 59.99, "peripherals"),
        };

        var generated = new ProductEntity[180];
        for (var i = 0; i < 180; i++)
        {
            var template = productTemplates[i % productTemplates.Length];
            var variant = (i / productTemplates.Length) + 1;
            generated[i] = new ProductEntity
            {
                Id = (21 + i).ToString(),
                Name = string.Format(template.Item1, variant),
                Description = template.Item2,
                Price = Math.Round(template.Item3 * (0.8 + (i % 5) * 0.1), 2),
                CategoryId = template.Item4,
                Stock = 10 + (i * 7) % 200,
            };
        }

        var all = new ProductEntity[200];
        baseProducts.CopyTo(all, 0);
        generated.CopyTo(all, 20);
        return all;
    }
}
