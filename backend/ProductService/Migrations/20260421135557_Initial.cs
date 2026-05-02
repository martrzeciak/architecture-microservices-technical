using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    CategoryId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { "1", "electronics", "High-performance laptop", "Laptop Pro 15", 2499.9899999999998, 10 },
                    { "10", "components", "NVIDIA GeForce RTX 4070 12GB", "RTX 4070 GPU", 1299.99, 15 },
                    { "11", "accessories", "4K 60Hz HDMI 2.1 cable", "HDMI Cable 2m", 12.99, 500 },
                    { "12", "accessories", "Extended gaming mouse pad", "Mouse Pad XL", 24.989999999999998, 300 },
                    { "13", "furniture", "Electric height-adjustable desk", "Standing Desk", 499.99000000000001, 20 },
                    { "14", "furniture", "Ergonomic lumbar support chair", "Office Chair", 349.99000000000001, 30 },
                    { "15", "audio", "Condenser microphone for streaming", "USB Microphone", 79.989999999999995, 80 },
                    { "16", "accessories", "Aluminium adjustable laptop stand", "Laptop Stand", 34.990000000000002, 120 },
                    { "17", "mobile", "PD 65W fast charge power bank", "Power Bank 20000mAh", 59.990000000000002, 90 },
                    { "18", "networking", "Managed gigabit switch", "Smart Switch 8-port", 89.989999999999995, 45 },
                    { "19", "electronics", "14-in-1 Thunderbolt 4 docking station", "Thunderbolt 4 Dock", 249.99000000000001, 35 },
                    { "2", "electronics", "Ergonomic wireless mouse", "Wireless Mouse", 49.990000000000002, 100 },
                    { "20", "storage", "USB-C portable SSD 1050 MB/s", "Portable SSD 2TB", 139.99000000000001, 65 },
                    { "3", "electronics", "TKL mechanical keyboard", "Mechanical Keyboard", 129.99000000000001, 50 },
                    { "4", "electronics", "7-in-1 USB-C hub", "USB-C Hub", 39.990000000000002, 200 },
                    { "5", "electronics", "4K IPS display 27 inch", "Monitor 27\"", 599.99000000000001, 25 },
                    { "6", "electronics", "7.1 surround sound headset", "Gaming Headset", 89.989999999999995, 75 },
                    { "7", "electronics", "4K UHD webcam with autofocus", "Webcam 4K", 149.99000000000001, 40 },
                    { "8", "storage", "NVMe PCIe Gen4 SSD", "SSD 1TB", 109.98999999999999, 150 },
                    { "9", "components", "DDR5 5600MHz dual channel", "RAM 32GB DDR5", 179.99000000000001, 60 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
