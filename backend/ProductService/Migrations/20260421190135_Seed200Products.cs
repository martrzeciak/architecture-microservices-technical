using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class Seed200Products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { "100", "components", "DDR4 3200MHz memory module", "DDR4 RAM 5GB", 59.990000000000002, 163 },
                    { "101", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 5GB", 71.989999999999995, 170 },
                    { "102", "furniture", "LED desk lamp with dimmer", "Desk Lamp 5W", 31.489999999999998, 177 },
                    { "103", "accessories", "Under-desk cable organizer", "Cable Management Kit 5", 19.989999999999998, 184 },
                    { "104", "mobile", "Qi wireless charging pad", "Wireless Charger 5W", 27.489999999999998, 191 },
                    { "105", "mobile", "Adjustable phone holder", "Phone Stand 5", 17.989999999999998, 198 },
                    { "106", "networking", "WiFi smart plug with timer", "Smart Plug 5", 10.390000000000001, 205 },
                    { "107", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat5", 8.0899999999999999, 12 },
                    { "108", "audio", "Portable Bluetooth speaker", "Mini Speaker 5", 39.990000000000002, 19 },
                    { "109", "mobile", "Tempered glass screen protector", "Screen Protector 5", 10.99, 26 },
                    { "110", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 5", 71.989999999999995, 33 },
                    { "111", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 6", 63.990000000000002, 40 },
                    { "112", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 6GB", 17.989999999999998, 47 },
                    { "113", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 6m", 14.99, 54 },
                    { "114", "networking", "PCIe network interface card", "Network Card 6GbE", 32.990000000000002, 61 },
                    { "115", "accessories", "Laptop cooling pad with fans", "Cooling Pad 6", 29.989999999999998, 68 },
                    { "116", "accessories", "Privacy slide cover for webcam", "Webcam Cover 6", 3.9900000000000002, 75 },
                    { "117", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 6", 8.9900000000000002, 82 },
                    { "118", "components", "DDR4 3200MHz memory module", "DDR4 RAM 6GB", 49.990000000000002, 89 },
                    { "119", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 6GB", 98.989999999999995, 96 },
                    { "120", "furniture", "LED desk lamp with dimmer", "Desk Lamp 6W", 41.990000000000002, 103 },
                    { "121", "accessories", "Under-desk cable organizer", "Cable Management Kit 6", 15.99, 110 },
                    { "122", "mobile", "Qi wireless charging pad", "Wireless Charger 6W", 22.489999999999998, 117 },
                    { "123", "mobile", "Adjustable phone holder", "Phone Stand 6", 14.99, 124 },
                    { "124", "networking", "WiFi smart plug with timer", "Smart Plug 6", 14.289999999999999, 131 },
                    { "125", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat6", 10.789999999999999, 138 },
                    { "126", "audio", "Portable Bluetooth speaker", "Mini Speaker 6", 31.989999999999998, 145 },
                    { "127", "mobile", "Tempered glass screen protector", "Screen Protector 6", 8.9900000000000002, 152 },
                    { "128", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 6", 59.990000000000002, 159 },
                    { "129", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 7", 87.989999999999995, 166 },
                    { "130", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 7GB", 23.989999999999998, 173 },
                    { "131", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 7m", 11.99, 180 },
                    { "132", "networking", "PCIe network interface card", "Network Card 7GbE", 26.989999999999998, 187 },
                    { "133", "accessories", "Laptop cooling pad with fans", "Cooling Pad 7", 24.989999999999998, 194 },
                    { "134", "accessories", "Privacy slide cover for webcam", "Webcam Cover 7", 5.4900000000000002, 201 },
                    { "135", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 7", 11.99, 208 },
                    { "136", "components", "DDR4 3200MHz memory module", "DDR4 RAM 7GB", 39.990000000000002, 15 },
                    { "137", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 7GB", 80.989999999999995, 22 },
                    { "138", "furniture", "LED desk lamp with dimmer", "Desk Lamp 7W", 34.990000000000002, 29 },
                    { "139", "accessories", "Under-desk cable organizer", "Cable Management Kit 7", 21.989999999999998, 36 },
                    { "140", "mobile", "Qi wireless charging pad", "Wireless Charger 7W", 29.989999999999998, 43 },
                    { "141", "mobile", "Adjustable phone holder", "Phone Stand 7", 11.99, 50 },
                    { "142", "networking", "WiFi smart plug with timer", "Smart Plug 7", 11.69, 57 },
                    { "143", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat7", 8.9900000000000002, 64 },
                    { "144", "audio", "Portable Bluetooth speaker", "Mini Speaker 7", 43.990000000000002, 71 },
                    { "145", "mobile", "Tempered glass screen protector", "Screen Protector 7", 11.99, 78 },
                    { "146", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 7", 47.990000000000002, 85 },
                    { "147", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 8", 71.989999999999995, 92 },
                    { "148", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 8GB", 19.989999999999998, 99 },
                    { "149", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 8m", 16.489999999999998, 106 },
                    { "150", "networking", "PCIe network interface card", "Network Card 8GbE", 35.990000000000002, 113 },
                    { "151", "accessories", "Laptop cooling pad with fans", "Cooling Pad 8", 19.989999999999998, 120 },
                    { "152", "accessories", "Privacy slide cover for webcam", "Webcam Cover 8", 4.4900000000000002, 127 },
                    { "153", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 8", 9.9900000000000002, 134 },
                    { "154", "components", "DDR4 3200MHz memory module", "DDR4 RAM 8GB", 54.990000000000002, 141 },
                    { "155", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 8GB", 107.98999999999999, 148 },
                    { "156", "furniture", "LED desk lamp with dimmer", "Desk Lamp 8W", 27.989999999999998, 155 },
                    { "157", "accessories", "Under-desk cable organizer", "Cable Management Kit 8", 17.989999999999998, 162 },
                    { "158", "mobile", "Qi wireless charging pad", "Wireless Charger 8W", 24.989999999999998, 169 },
                    { "159", "mobile", "Adjustable phone holder", "Phone Stand 8", 16.489999999999998, 176 },
                    { "160", "networking", "WiFi smart plug with timer", "Smart Plug 8", 15.59, 183 },
                    { "161", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat8", 7.1900000000000004, 190 },
                    { "162", "audio", "Portable Bluetooth speaker", "Mini Speaker 8", 35.990000000000002, 197 },
                    { "163", "mobile", "Tempered glass screen protector", "Screen Protector 8", 9.9900000000000002, 204 },
                    { "164", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 8", 65.989999999999995, 11 },
                    { "165", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 9", 95.989999999999995, 18 },
                    { "166", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 9GB", 15.99, 25 },
                    { "167", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 9m", 13.49, 32 },
                    { "168", "networking", "PCIe network interface card", "Network Card 9GbE", 29.989999999999998, 39 },
                    { "169", "accessories", "Laptop cooling pad with fans", "Cooling Pad 9", 27.489999999999998, 46 },
                    { "170", "accessories", "Privacy slide cover for webcam", "Webcam Cover 9", 5.9900000000000002, 53 },
                    { "171", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 9", 7.9900000000000002, 60 },
                    { "172", "components", "DDR4 3200MHz memory module", "DDR4 RAM 9GB", 44.990000000000002, 67 },
                    { "173", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 9GB", 89.989999999999995, 74 },
                    { "174", "furniture", "LED desk lamp with dimmer", "Desk Lamp 9W", 38.490000000000002, 81 },
                    { "175", "accessories", "Under-desk cable organizer", "Cable Management Kit 9", 23.989999999999998, 88 },
                    { "176", "mobile", "Qi wireless charging pad", "Wireless Charger 9W", 19.989999999999998, 95 },
                    { "177", "mobile", "Adjustable phone holder", "Phone Stand 9", 13.49, 102 },
                    { "178", "networking", "WiFi smart plug with timer", "Smart Plug 9", 12.99, 109 },
                    { "179", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat9", 9.8900000000000006, 116 },
                    { "180", "audio", "Portable Bluetooth speaker", "Mini Speaker 9", 47.990000000000002, 123 },
                    { "181", "mobile", "Tempered glass screen protector", "Screen Protector 9", 7.9900000000000002, 130 },
                    { "182", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 9", 53.990000000000002, 137 },
                    { "183", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 10", 79.989999999999995, 144 },
                    { "184", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 10GB", 21.989999999999998, 151 },
                    { "185", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 10m", 17.989999999999998, 158 },
                    { "186", "networking", "PCIe network interface card", "Network Card 10GbE", 23.989999999999998, 165 },
                    { "187", "accessories", "Laptop cooling pad with fans", "Cooling Pad 10", 22.489999999999998, 172 },
                    { "188", "accessories", "Privacy slide cover for webcam", "Webcam Cover 10", 4.9900000000000002, 179 },
                    { "189", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 10", 10.99, 186 },
                    { "190", "components", "DDR4 3200MHz memory module", "DDR4 RAM 10GB", 59.990000000000002, 193 },
                    { "191", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 10GB", 71.989999999999995, 200 },
                    { "192", "furniture", "LED desk lamp with dimmer", "Desk Lamp 10W", 31.489999999999998, 207 },
                    { "193", "accessories", "Under-desk cable organizer", "Cable Management Kit 10", 19.989999999999998, 14 },
                    { "194", "mobile", "Qi wireless charging pad", "Wireless Charger 10W", 27.489999999999998, 21 },
                    { "195", "mobile", "Adjustable phone holder", "Phone Stand 10", 17.989999999999998, 28 },
                    { "196", "networking", "WiFi smart plug with timer", "Smart Plug 10", 10.390000000000001, 35 },
                    { "197", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat10", 8.0899999999999999, 42 },
                    { "198", "audio", "Portable Bluetooth speaker", "Mini Speaker 10", 39.990000000000002, 49 },
                    { "199", "mobile", "Tempered glass screen protector", "Screen Protector 10", 10.99, 56 },
                    { "200", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 10", 71.989999999999995, 63 },
                    { "21", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 1", 63.990000000000002, 10 },
                    { "22", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 1GB", 17.989999999999998, 17 },
                    { "23", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 1m", 14.99, 24 },
                    { "24", "networking", "PCIe network interface card", "Network Card 1GbE", 32.990000000000002, 31 },
                    { "25", "accessories", "Laptop cooling pad with fans", "Cooling Pad 1", 29.989999999999998, 38 },
                    { "26", "accessories", "Privacy slide cover for webcam", "Webcam Cover 1", 3.9900000000000002, 45 },
                    { "27", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 1", 8.9900000000000002, 52 },
                    { "28", "components", "DDR4 3200MHz memory module", "DDR4 RAM 1GB", 49.990000000000002, 59 },
                    { "29", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 1GB", 98.989999999999995, 66 },
                    { "30", "furniture", "LED desk lamp with dimmer", "Desk Lamp 1W", 41.990000000000002, 73 },
                    { "31", "accessories", "Under-desk cable organizer", "Cable Management Kit 1", 15.99, 80 },
                    { "32", "mobile", "Qi wireless charging pad", "Wireless Charger 1W", 22.489999999999998, 87 },
                    { "33", "mobile", "Adjustable phone holder", "Phone Stand 1", 14.99, 94 },
                    { "34", "networking", "WiFi smart plug with timer", "Smart Plug 1", 14.289999999999999, 101 },
                    { "35", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat1", 10.789999999999999, 108 },
                    { "36", "audio", "Portable Bluetooth speaker", "Mini Speaker 1", 31.989999999999998, 115 },
                    { "37", "mobile", "Tempered glass screen protector", "Screen Protector 1", 8.9900000000000002, 122 },
                    { "38", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 1", 59.990000000000002, 129 },
                    { "39", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 2", 87.989999999999995, 136 },
                    { "40", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 2GB", 23.989999999999998, 143 },
                    { "41", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 2m", 11.99, 150 },
                    { "42", "networking", "PCIe network interface card", "Network Card 2GbE", 26.989999999999998, 157 },
                    { "43", "accessories", "Laptop cooling pad with fans", "Cooling Pad 2", 24.989999999999998, 164 },
                    { "44", "accessories", "Privacy slide cover for webcam", "Webcam Cover 2", 5.4900000000000002, 171 },
                    { "45", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 2", 11.99, 178 },
                    { "46", "components", "DDR4 3200MHz memory module", "DDR4 RAM 2GB", 39.990000000000002, 185 },
                    { "47", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 2GB", 80.989999999999995, 192 },
                    { "48", "furniture", "LED desk lamp with dimmer", "Desk Lamp 2W", 34.990000000000002, 199 },
                    { "49", "accessories", "Under-desk cable organizer", "Cable Management Kit 2", 21.989999999999998, 206 },
                    { "50", "mobile", "Qi wireless charging pad", "Wireless Charger 2W", 29.989999999999998, 13 },
                    { "51", "mobile", "Adjustable phone holder", "Phone Stand 2", 11.99, 20 },
                    { "52", "networking", "WiFi smart plug with timer", "Smart Plug 2", 11.69, 27 },
                    { "53", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat2", 8.9900000000000002, 34 },
                    { "54", "audio", "Portable Bluetooth speaker", "Mini Speaker 2", 43.990000000000002, 41 },
                    { "55", "mobile", "Tempered glass screen protector", "Screen Protector 2", 11.99, 48 },
                    { "56", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 2", 47.990000000000002, 55 },
                    { "57", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 3", 71.989999999999995, 62 },
                    { "58", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 3GB", 19.989999999999998, 69 },
                    { "59", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 3m", 16.489999999999998, 76 },
                    { "60", "networking", "PCIe network interface card", "Network Card 3GbE", 35.990000000000002, 83 },
                    { "61", "accessories", "Laptop cooling pad with fans", "Cooling Pad 3", 19.989999999999998, 90 },
                    { "62", "accessories", "Privacy slide cover for webcam", "Webcam Cover 3", 4.4900000000000002, 97 },
                    { "63", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 3", 9.9900000000000002, 104 },
                    { "64", "components", "DDR4 3200MHz memory module", "DDR4 RAM 3GB", 54.990000000000002, 111 },
                    { "65", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 3GB", 107.98999999999999, 118 },
                    { "66", "furniture", "LED desk lamp with dimmer", "Desk Lamp 3W", 27.989999999999998, 125 },
                    { "67", "accessories", "Under-desk cable organizer", "Cable Management Kit 3", 17.989999999999998, 132 },
                    { "68", "mobile", "Qi wireless charging pad", "Wireless Charger 3W", 24.989999999999998, 139 },
                    { "69", "mobile", "Adjustable phone holder", "Phone Stand 3", 16.489999999999998, 146 },
                    { "70", "networking", "WiFi smart plug with timer", "Smart Plug 3", 15.59, 153 },
                    { "71", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat3", 7.1900000000000004, 160 },
                    { "72", "audio", "Portable Bluetooth speaker", "Mini Speaker 3", 35.990000000000002, 167 },
                    { "73", "mobile", "Tempered glass screen protector", "Screen Protector 3", 9.9900000000000002, 174 },
                    { "74", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 3", 65.989999999999995, 181 },
                    { "75", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 4", 95.989999999999995, 188 },
                    { "76", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 4GB", 15.99, 195 },
                    { "77", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 4m", 13.49, 202 },
                    { "78", "networking", "PCIe network interface card", "Network Card 4GbE", 29.989999999999998, 209 },
                    { "79", "accessories", "Laptop cooling pad with fans", "Cooling Pad 4", 27.489999999999998, 16 },
                    { "80", "accessories", "Privacy slide cover for webcam", "Webcam Cover 4", 5.9900000000000002, 23 },
                    { "81", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 4", 7.9900000000000002, 30 },
                    { "82", "components", "DDR4 3200MHz memory module", "DDR4 RAM 4GB", 44.990000000000002, 37 },
                    { "83", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 4GB", 89.989999999999995, 44 },
                    { "84", "furniture", "LED desk lamp with dimmer", "Desk Lamp 4W", 38.490000000000002, 51 },
                    { "85", "accessories", "Under-desk cable organizer", "Cable Management Kit 4", 23.989999999999998, 58 },
                    { "86", "mobile", "Qi wireless charging pad", "Wireless Charger 4W", 19.989999999999998, 65 },
                    { "87", "mobile", "Adjustable phone holder", "Phone Stand 4", 13.49, 72 },
                    { "88", "networking", "WiFi smart plug with timer", "Smart Plug 4", 12.99, 79 },
                    { "89", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat4", 9.8900000000000006, 86 },
                    { "90", "audio", "Portable Bluetooth speaker", "Mini Speaker 4", 47.990000000000002, 93 },
                    { "91", "mobile", "Tempered glass screen protector", "Screen Protector 4", 7.9900000000000002, 100 },
                    { "92", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 4", 53.990000000000002, 107 },
                    { "93", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 5", 79.989999999999995, 114 },
                    { "94", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 5GB", 21.989999999999998, 121 },
                    { "95", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 5m", 17.989999999999998, 128 },
                    { "96", "networking", "PCIe network interface card", "Network Card 5GbE", 23.989999999999998, 135 },
                    { "97", "accessories", "Laptop cooling pad with fans", "Cooling Pad 5", 22.489999999999998, 142 },
                    { "98", "accessories", "Privacy slide cover for webcam", "Webcam Cover 5", 4.9900000000000002, 149 },
                    { "99", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 5", 10.99, 156 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "100");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "101");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "102");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "103");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "104");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "105");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "106");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "107");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "108");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "109");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "110");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "111");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "112");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "113");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "114");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "115");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "116");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "117");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "118");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "119");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "120");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "121");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "122");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "123");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "124");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "125");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "126");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "127");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "128");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "129");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "130");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "131");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "132");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "133");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "134");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "135");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "136");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "137");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "138");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "139");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "140");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "141");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "142");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "143");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "144");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "145");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "146");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "147");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "148");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "149");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "150");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "151");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "152");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "153");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "154");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "155");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "156");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "157");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "158");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "159");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "160");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "161");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "162");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "163");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "164");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "165");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "166");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "167");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "168");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "169");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "170");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "171");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "172");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "173");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "174");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "175");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "176");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "177");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "178");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "179");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "180");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "181");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "182");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "183");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "184");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "185");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "186");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "187");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "188");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "189");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "190");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "191");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "192");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "193");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "194");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "195");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "196");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "197");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "198");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "199");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "200");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "21");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "22");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "23");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "24");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "25");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "26");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "27");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "28");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "29");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "30");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "31");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "32");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "33");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "34");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "35");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "36");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "37");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "38");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "39");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "40");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "41");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "42");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "43");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "44");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "45");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "46");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "47");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "48");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "49");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "50");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "51");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "52");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "53");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "54");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "55");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "56");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "57");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "58");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "59");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "60");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "61");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "62");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "63");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "64");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "65");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "66");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "67");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "68");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "69");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "70");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "71");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "72");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "73");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "74");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "75");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "76");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "77");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "78");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "79");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "80");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "81");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "82");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "83");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "84");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "85");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "86");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "87");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "88");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "89");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "90");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "91");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "92");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "93");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "94");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "95");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "96");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "97");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "98");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "99");
        }
    }
}
