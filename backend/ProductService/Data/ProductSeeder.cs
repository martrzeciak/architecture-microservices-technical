using Microsoft.EntityFrameworkCore;

namespace ProductService.Data;

/// <summary>
/// Tops the Products table up to a target row count at startup.
///
/// Why this is not done with HasData like the first 500 rows: EF Core writes
/// HasData contents as literal InsertData arrays into the migration and repeats
/// them in the model snapshot. Seeding thousands of rows that way produces
/// multi-megabyte generated files, slows compilation and slows every startup.
/// Topping up at runtime keeps the migrations small and makes the target count
/// configurable (SEED_PRODUCT_COUNT).
///
/// Idempotent: counts existing rows first and inserts only what is missing, so
/// restarting the container does not duplicate data.
/// </summary>
public static class ProductSeeder
{
    private static readonly (string NameFormat, string Description, double Price, string CategoryId)[] Templates =
    [
        ("Wireless Earbuds {0}",      "Bluetooth 5.3 earbuds with ANC",     79.99, "audio"),
        ("USB Flash Drive {0}GB",     "USB 3.2 Gen2 flash drive",           19.99, "storage"),
        ("DisplayPort Cable {0}m",    "DP 1.4 HBR3 cable",                  14.99, "accessories"),
        ("Network Card {0}GbE",       "PCIe network interface card",        29.99, "networking"),
        ("Cooling Pad {0}",           "Laptop cooling pad with fans",       24.99, "accessories"),
        ("Webcam Cover {0}",          "Privacy slide cover for webcam",      4.99, "accessories"),
        ("Type-C Adapter {0}",        "USB-C to USB-A adapter",              9.99, "accessories"),
        ("DDR4 RAM {0}GB",            "DDR4 3200MHz memory module",         49.99, "components"),
        ("PCIe SSD {0}GB",            "NVMe M.2 2280 SSD",                  89.99, "storage"),
        ("Desk Lamp {0}W",            "LED desk lamp with dimmer",          34.99, "furniture"),
        ("Cable Management Kit {0}",  "Under-desk cable organizer",         19.99, "accessories"),
        ("Wireless Charger {0}W",     "Qi wireless charging pad",           24.99, "mobile"),
        ("Phone Stand {0}",           "Adjustable phone holder",            14.99, "mobile"),
        ("Smart Plug {0}",            "WiFi smart plug with timer",         12.99, "networking"),
        ("Ethernet Cable Cat{0}",     "Shielded ethernet cable 3m",          8.99, "networking"),
        ("Mini Speaker {0}",          "Portable Bluetooth speaker",         39.99, "audio"),
        ("Screen Protector {0}",      "Tempered glass screen protector",     9.99, "mobile"),
        ("Drawing Tablet {0}",        "Graphics drawing tablet USB",        59.99, "peripherals"),
    ];

    /// <summary>Number of rows created by the HasData seed in migrations.</summary>
    private const int MigrationSeedCount = 500;

    public static void EnsureProductCount(ProductDbContext db, int target, ILogger? logger = null)
    {
        if (target <= MigrationSeedCount) return;

        var existing = db.Products.Count();
        if (existing >= target) return;

        // Ids of the migration seed are the strings "1".."500", so continue from there.
        var missing = new List<ProductEntity>(target - existing);
        for (var id = existing + 1; id <= target; id++)
        {
            var index = id - MigrationSeedCount - 1;
            var template = Templates[index % Templates.Length];
            var variant = (index / Templates.Length) + 1;

            missing.Add(new ProductEntity
            {
                Id = id.ToString(),
                Name = string.Format(template.NameFormat, variant),
                Description = template.Description,
                Price = Math.Round(template.Price * (0.8 + (index % 5) * 0.1), 2),
                CategoryId = template.CategoryId,
                Stock = 10 + (index * 7) % 200,
            });
        }

        db.Products.AddRange(missing);
        db.SaveChanges();

        logger?.LogInformation(
            "Seeded {Added} products ({Existing} -> {Target})", missing.Count, existing, target);
    }
}
