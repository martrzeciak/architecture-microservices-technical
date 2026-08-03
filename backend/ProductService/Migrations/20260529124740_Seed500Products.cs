using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class Seed500Products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { "201", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 11", 63.990000000000002, 70 },
                    { "202", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 11GB", 17.989999999999998, 77 },
                    { "203", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 11m", 14.99, 84 },
                    { "204", "networking", "PCIe network interface card", "Network Card 11GbE", 32.990000000000002, 91 },
                    { "205", "accessories", "Laptop cooling pad with fans", "Cooling Pad 11", 29.989999999999998, 98 },
                    { "206", "accessories", "Privacy slide cover for webcam", "Webcam Cover 11", 3.9900000000000002, 105 },
                    { "207", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 11", 8.9900000000000002, 112 },
                    { "208", "components", "DDR4 3200MHz memory module", "DDR4 RAM 11GB", 49.990000000000002, 119 },
                    { "209", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 11GB", 98.989999999999995, 126 },
                    { "210", "furniture", "LED desk lamp with dimmer", "Desk Lamp 11W", 41.990000000000002, 133 },
                    { "211", "accessories", "Under-desk cable organizer", "Cable Management Kit 11", 15.99, 140 },
                    { "212", "mobile", "Qi wireless charging pad", "Wireless Charger 11W", 22.489999999999998, 147 },
                    { "213", "mobile", "Adjustable phone holder", "Phone Stand 11", 14.99, 154 },
                    { "214", "networking", "WiFi smart plug with timer", "Smart Plug 11", 14.289999999999999, 161 },
                    { "215", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat11", 10.789999999999999, 168 },
                    { "216", "audio", "Portable Bluetooth speaker", "Mini Speaker 11", 31.989999999999998, 175 },
                    { "217", "mobile", "Tempered glass screen protector", "Screen Protector 11", 8.9900000000000002, 182 },
                    { "218", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 11", 59.990000000000002, 189 },
                    { "219", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 12", 87.989999999999995, 196 },
                    { "220", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 12GB", 23.989999999999998, 203 },
                    { "221", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 12m", 11.99, 10 },
                    { "222", "networking", "PCIe network interface card", "Network Card 12GbE", 26.989999999999998, 17 },
                    { "223", "accessories", "Laptop cooling pad with fans", "Cooling Pad 12", 24.989999999999998, 24 },
                    { "224", "accessories", "Privacy slide cover for webcam", "Webcam Cover 12", 5.4900000000000002, 31 },
                    { "225", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 12", 11.99, 38 },
                    { "226", "components", "DDR4 3200MHz memory module", "DDR4 RAM 12GB", 39.990000000000002, 45 },
                    { "227", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 12GB", 80.989999999999995, 52 },
                    { "228", "furniture", "LED desk lamp with dimmer", "Desk Lamp 12W", 34.990000000000002, 59 },
                    { "229", "accessories", "Under-desk cable organizer", "Cable Management Kit 12", 21.989999999999998, 66 },
                    { "230", "mobile", "Qi wireless charging pad", "Wireless Charger 12W", 29.989999999999998, 73 },
                    { "231", "mobile", "Adjustable phone holder", "Phone Stand 12", 11.99, 80 },
                    { "232", "networking", "WiFi smart plug with timer", "Smart Plug 12", 11.69, 87 },
                    { "233", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat12", 8.9900000000000002, 94 },
                    { "234", "audio", "Portable Bluetooth speaker", "Mini Speaker 12", 43.990000000000002, 101 },
                    { "235", "mobile", "Tempered glass screen protector", "Screen Protector 12", 11.99, 108 },
                    { "236", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 12", 47.990000000000002, 115 },
                    { "237", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 13", 71.989999999999995, 122 },
                    { "238", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 13GB", 19.989999999999998, 129 },
                    { "239", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 13m", 16.489999999999998, 136 },
                    { "240", "networking", "PCIe network interface card", "Network Card 13GbE", 35.990000000000002, 143 },
                    { "241", "accessories", "Laptop cooling pad with fans", "Cooling Pad 13", 19.989999999999998, 150 },
                    { "242", "accessories", "Privacy slide cover for webcam", "Webcam Cover 13", 4.4900000000000002, 157 },
                    { "243", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 13", 9.9900000000000002, 164 },
                    { "244", "components", "DDR4 3200MHz memory module", "DDR4 RAM 13GB", 54.990000000000002, 171 },
                    { "245", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 13GB", 107.98999999999999, 178 },
                    { "246", "furniture", "LED desk lamp with dimmer", "Desk Lamp 13W", 27.989999999999998, 185 },
                    { "247", "accessories", "Under-desk cable organizer", "Cable Management Kit 13", 17.989999999999998, 192 },
                    { "248", "mobile", "Qi wireless charging pad", "Wireless Charger 13W", 24.989999999999998, 199 },
                    { "249", "mobile", "Adjustable phone holder", "Phone Stand 13", 16.489999999999998, 206 },
                    { "250", "networking", "WiFi smart plug with timer", "Smart Plug 13", 15.59, 13 },
                    { "251", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat13", 7.1900000000000004, 20 },
                    { "252", "audio", "Portable Bluetooth speaker", "Mini Speaker 13", 35.990000000000002, 27 },
                    { "253", "mobile", "Tempered glass screen protector", "Screen Protector 13", 9.9900000000000002, 34 },
                    { "254", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 13", 65.989999999999995, 41 },
                    { "255", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 14", 95.989999999999995, 48 },
                    { "256", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 14GB", 15.99, 55 },
                    { "257", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 14m", 13.49, 62 },
                    { "258", "networking", "PCIe network interface card", "Network Card 14GbE", 29.989999999999998, 69 },
                    { "259", "accessories", "Laptop cooling pad with fans", "Cooling Pad 14", 27.489999999999998, 76 },
                    { "260", "accessories", "Privacy slide cover for webcam", "Webcam Cover 14", 5.9900000000000002, 83 },
                    { "261", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 14", 7.9900000000000002, 90 },
                    { "262", "components", "DDR4 3200MHz memory module", "DDR4 RAM 14GB", 44.990000000000002, 97 },
                    { "263", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 14GB", 89.989999999999995, 104 },
                    { "264", "furniture", "LED desk lamp with dimmer", "Desk Lamp 14W", 38.490000000000002, 111 },
                    { "265", "accessories", "Under-desk cable organizer", "Cable Management Kit 14", 23.989999999999998, 118 },
                    { "266", "mobile", "Qi wireless charging pad", "Wireless Charger 14W", 19.989999999999998, 125 },
                    { "267", "mobile", "Adjustable phone holder", "Phone Stand 14", 13.49, 132 },
                    { "268", "networking", "WiFi smart plug with timer", "Smart Plug 14", 12.99, 139 },
                    { "269", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat14", 9.8900000000000006, 146 },
                    { "270", "audio", "Portable Bluetooth speaker", "Mini Speaker 14", 47.990000000000002, 153 },
                    { "271", "mobile", "Tempered glass screen protector", "Screen Protector 14", 7.9900000000000002, 160 },
                    { "272", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 14", 53.990000000000002, 167 },
                    { "273", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 15", 79.989999999999995, 174 },
                    { "274", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 15GB", 21.989999999999998, 181 },
                    { "275", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 15m", 17.989999999999998, 188 },
                    { "276", "networking", "PCIe network interface card", "Network Card 15GbE", 23.989999999999998, 195 },
                    { "277", "accessories", "Laptop cooling pad with fans", "Cooling Pad 15", 22.489999999999998, 202 },
                    { "278", "accessories", "Privacy slide cover for webcam", "Webcam Cover 15", 4.9900000000000002, 209 },
                    { "279", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 15", 10.99, 16 },
                    { "280", "components", "DDR4 3200MHz memory module", "DDR4 RAM 15GB", 59.990000000000002, 23 },
                    { "281", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 15GB", 71.989999999999995, 30 },
                    { "282", "furniture", "LED desk lamp with dimmer", "Desk Lamp 15W", 31.489999999999998, 37 },
                    { "283", "accessories", "Under-desk cable organizer", "Cable Management Kit 15", 19.989999999999998, 44 },
                    { "284", "mobile", "Qi wireless charging pad", "Wireless Charger 15W", 27.489999999999998, 51 },
                    { "285", "mobile", "Adjustable phone holder", "Phone Stand 15", 17.989999999999998, 58 },
                    { "286", "networking", "WiFi smart plug with timer", "Smart Plug 15", 10.390000000000001, 65 },
                    { "287", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat15", 8.0899999999999999, 72 },
                    { "288", "audio", "Portable Bluetooth speaker", "Mini Speaker 15", 39.990000000000002, 79 },
                    { "289", "mobile", "Tempered glass screen protector", "Screen Protector 15", 10.99, 86 },
                    { "290", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 15", 71.989999999999995, 93 },
                    { "291", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 16", 63.990000000000002, 100 },
                    { "292", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 16GB", 17.989999999999998, 107 },
                    { "293", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 16m", 14.99, 114 },
                    { "294", "networking", "PCIe network interface card", "Network Card 16GbE", 32.990000000000002, 121 },
                    { "295", "accessories", "Laptop cooling pad with fans", "Cooling Pad 16", 29.989999999999998, 128 },
                    { "296", "accessories", "Privacy slide cover for webcam", "Webcam Cover 16", 3.9900000000000002, 135 },
                    { "297", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 16", 8.9900000000000002, 142 },
                    { "298", "components", "DDR4 3200MHz memory module", "DDR4 RAM 16GB", 49.990000000000002, 149 },
                    { "299", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 16GB", 98.989999999999995, 156 },
                    { "300", "furniture", "LED desk lamp with dimmer", "Desk Lamp 16W", 41.990000000000002, 163 },
                    { "301", "accessories", "Under-desk cable organizer", "Cable Management Kit 16", 15.99, 170 },
                    { "302", "mobile", "Qi wireless charging pad", "Wireless Charger 16W", 22.489999999999998, 177 },
                    { "303", "mobile", "Adjustable phone holder", "Phone Stand 16", 14.99, 184 },
                    { "304", "networking", "WiFi smart plug with timer", "Smart Plug 16", 14.289999999999999, 191 },
                    { "305", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat16", 10.789999999999999, 198 },
                    { "306", "audio", "Portable Bluetooth speaker", "Mini Speaker 16", 31.989999999999998, 205 },
                    { "307", "mobile", "Tempered glass screen protector", "Screen Protector 16", 8.9900000000000002, 12 },
                    { "308", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 16", 59.990000000000002, 19 },
                    { "309", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 17", 87.989999999999995, 26 },
                    { "310", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 17GB", 23.989999999999998, 33 },
                    { "311", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 17m", 11.99, 40 },
                    { "312", "networking", "PCIe network interface card", "Network Card 17GbE", 26.989999999999998, 47 },
                    { "313", "accessories", "Laptop cooling pad with fans", "Cooling Pad 17", 24.989999999999998, 54 },
                    { "314", "accessories", "Privacy slide cover for webcam", "Webcam Cover 17", 5.4900000000000002, 61 },
                    { "315", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 17", 11.99, 68 },
                    { "316", "components", "DDR4 3200MHz memory module", "DDR4 RAM 17GB", 39.990000000000002, 75 },
                    { "317", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 17GB", 80.989999999999995, 82 },
                    { "318", "furniture", "LED desk lamp with dimmer", "Desk Lamp 17W", 34.990000000000002, 89 },
                    { "319", "accessories", "Under-desk cable organizer", "Cable Management Kit 17", 21.989999999999998, 96 },
                    { "320", "mobile", "Qi wireless charging pad", "Wireless Charger 17W", 29.989999999999998, 103 },
                    { "321", "mobile", "Adjustable phone holder", "Phone Stand 17", 11.99, 110 },
                    { "322", "networking", "WiFi smart plug with timer", "Smart Plug 17", 11.69, 117 },
                    { "323", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat17", 8.9900000000000002, 124 },
                    { "324", "audio", "Portable Bluetooth speaker", "Mini Speaker 17", 43.990000000000002, 131 },
                    { "325", "mobile", "Tempered glass screen protector", "Screen Protector 17", 11.99, 138 },
                    { "326", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 17", 47.990000000000002, 145 },
                    { "327", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 18", 71.989999999999995, 152 },
                    { "328", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 18GB", 19.989999999999998, 159 },
                    { "329", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 18m", 16.489999999999998, 166 },
                    { "330", "networking", "PCIe network interface card", "Network Card 18GbE", 35.990000000000002, 173 },
                    { "331", "accessories", "Laptop cooling pad with fans", "Cooling Pad 18", 19.989999999999998, 180 },
                    { "332", "accessories", "Privacy slide cover for webcam", "Webcam Cover 18", 4.4900000000000002, 187 },
                    { "333", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 18", 9.9900000000000002, 194 },
                    { "334", "components", "DDR4 3200MHz memory module", "DDR4 RAM 18GB", 54.990000000000002, 201 },
                    { "335", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 18GB", 107.98999999999999, 208 },
                    { "336", "furniture", "LED desk lamp with dimmer", "Desk Lamp 18W", 27.989999999999998, 15 },
                    { "337", "accessories", "Under-desk cable organizer", "Cable Management Kit 18", 17.989999999999998, 22 },
                    { "338", "mobile", "Qi wireless charging pad", "Wireless Charger 18W", 24.989999999999998, 29 },
                    { "339", "mobile", "Adjustable phone holder", "Phone Stand 18", 16.489999999999998, 36 },
                    { "340", "networking", "WiFi smart plug with timer", "Smart Plug 18", 15.59, 43 },
                    { "341", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat18", 7.1900000000000004, 50 },
                    { "342", "audio", "Portable Bluetooth speaker", "Mini Speaker 18", 35.990000000000002, 57 },
                    { "343", "mobile", "Tempered glass screen protector", "Screen Protector 18", 9.9900000000000002, 64 },
                    { "344", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 18", 65.989999999999995, 71 },
                    { "345", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 19", 95.989999999999995, 78 },
                    { "346", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 19GB", 15.99, 85 },
                    { "347", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 19m", 13.49, 92 },
                    { "348", "networking", "PCIe network interface card", "Network Card 19GbE", 29.989999999999998, 99 },
                    { "349", "accessories", "Laptop cooling pad with fans", "Cooling Pad 19", 27.489999999999998, 106 },
                    { "350", "accessories", "Privacy slide cover for webcam", "Webcam Cover 19", 5.9900000000000002, 113 },
                    { "351", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 19", 7.9900000000000002, 120 },
                    { "352", "components", "DDR4 3200MHz memory module", "DDR4 RAM 19GB", 44.990000000000002, 127 },
                    { "353", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 19GB", 89.989999999999995, 134 },
                    { "354", "furniture", "LED desk lamp with dimmer", "Desk Lamp 19W", 38.490000000000002, 141 },
                    { "355", "accessories", "Under-desk cable organizer", "Cable Management Kit 19", 23.989999999999998, 148 },
                    { "356", "mobile", "Qi wireless charging pad", "Wireless Charger 19W", 19.989999999999998, 155 },
                    { "357", "mobile", "Adjustable phone holder", "Phone Stand 19", 13.49, 162 },
                    { "358", "networking", "WiFi smart plug with timer", "Smart Plug 19", 12.99, 169 },
                    { "359", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat19", 9.8900000000000006, 176 },
                    { "360", "audio", "Portable Bluetooth speaker", "Mini Speaker 19", 47.990000000000002, 183 },
                    { "361", "mobile", "Tempered glass screen protector", "Screen Protector 19", 7.9900000000000002, 190 },
                    { "362", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 19", 53.990000000000002, 197 },
                    { "363", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 20", 79.989999999999995, 204 },
                    { "364", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 20GB", 21.989999999999998, 11 },
                    { "365", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 20m", 17.989999999999998, 18 },
                    { "366", "networking", "PCIe network interface card", "Network Card 20GbE", 23.989999999999998, 25 },
                    { "367", "accessories", "Laptop cooling pad with fans", "Cooling Pad 20", 22.489999999999998, 32 },
                    { "368", "accessories", "Privacy slide cover for webcam", "Webcam Cover 20", 4.9900000000000002, 39 },
                    { "369", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 20", 10.99, 46 },
                    { "370", "components", "DDR4 3200MHz memory module", "DDR4 RAM 20GB", 59.990000000000002, 53 },
                    { "371", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 20GB", 71.989999999999995, 60 },
                    { "372", "furniture", "LED desk lamp with dimmer", "Desk Lamp 20W", 31.489999999999998, 67 },
                    { "373", "accessories", "Under-desk cable organizer", "Cable Management Kit 20", 19.989999999999998, 74 },
                    { "374", "mobile", "Qi wireless charging pad", "Wireless Charger 20W", 27.489999999999998, 81 },
                    { "375", "mobile", "Adjustable phone holder", "Phone Stand 20", 17.989999999999998, 88 },
                    { "376", "networking", "WiFi smart plug with timer", "Smart Plug 20", 10.390000000000001, 95 },
                    { "377", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat20", 8.0899999999999999, 102 },
                    { "378", "audio", "Portable Bluetooth speaker", "Mini Speaker 20", 39.990000000000002, 109 },
                    { "379", "mobile", "Tempered glass screen protector", "Screen Protector 20", 10.99, 116 },
                    { "380", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 20", 71.989999999999995, 123 },
                    { "381", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 21", 63.990000000000002, 130 },
                    { "382", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 21GB", 17.989999999999998, 137 },
                    { "383", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 21m", 14.99, 144 },
                    { "384", "networking", "PCIe network interface card", "Network Card 21GbE", 32.990000000000002, 151 },
                    { "385", "accessories", "Laptop cooling pad with fans", "Cooling Pad 21", 29.989999999999998, 158 },
                    { "386", "accessories", "Privacy slide cover for webcam", "Webcam Cover 21", 3.9900000000000002, 165 },
                    { "387", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 21", 8.9900000000000002, 172 },
                    { "388", "components", "DDR4 3200MHz memory module", "DDR4 RAM 21GB", 49.990000000000002, 179 },
                    { "389", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 21GB", 98.989999999999995, 186 },
                    { "390", "furniture", "LED desk lamp with dimmer", "Desk Lamp 21W", 41.990000000000002, 193 },
                    { "391", "accessories", "Under-desk cable organizer", "Cable Management Kit 21", 15.99, 200 },
                    { "392", "mobile", "Qi wireless charging pad", "Wireless Charger 21W", 22.489999999999998, 207 },
                    { "393", "mobile", "Adjustable phone holder", "Phone Stand 21", 14.99, 14 },
                    { "394", "networking", "WiFi smart plug with timer", "Smart Plug 21", 14.289999999999999, 21 },
                    { "395", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat21", 10.789999999999999, 28 },
                    { "396", "audio", "Portable Bluetooth speaker", "Mini Speaker 21", 31.989999999999998, 35 },
                    { "397", "mobile", "Tempered glass screen protector", "Screen Protector 21", 8.9900000000000002, 42 },
                    { "398", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 21", 59.990000000000002, 49 },
                    { "399", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 22", 87.989999999999995, 56 },
                    { "400", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 22GB", 23.989999999999998, 63 },
                    { "401", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 22m", 11.99, 70 },
                    { "402", "networking", "PCIe network interface card", "Network Card 22GbE", 26.989999999999998, 77 },
                    { "403", "accessories", "Laptop cooling pad with fans", "Cooling Pad 22", 24.989999999999998, 84 },
                    { "404", "accessories", "Privacy slide cover for webcam", "Webcam Cover 22", 5.4900000000000002, 91 },
                    { "405", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 22", 11.99, 98 },
                    { "406", "components", "DDR4 3200MHz memory module", "DDR4 RAM 22GB", 39.990000000000002, 105 },
                    { "407", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 22GB", 80.989999999999995, 112 },
                    { "408", "furniture", "LED desk lamp with dimmer", "Desk Lamp 22W", 34.990000000000002, 119 },
                    { "409", "accessories", "Under-desk cable organizer", "Cable Management Kit 22", 21.989999999999998, 126 },
                    { "410", "mobile", "Qi wireless charging pad", "Wireless Charger 22W", 29.989999999999998, 133 },
                    { "411", "mobile", "Adjustable phone holder", "Phone Stand 22", 11.99, 140 },
                    { "412", "networking", "WiFi smart plug with timer", "Smart Plug 22", 11.69, 147 },
                    { "413", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat22", 8.9900000000000002, 154 },
                    { "414", "audio", "Portable Bluetooth speaker", "Mini Speaker 22", 43.990000000000002, 161 },
                    { "415", "mobile", "Tempered glass screen protector", "Screen Protector 22", 11.99, 168 },
                    { "416", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 22", 47.990000000000002, 175 },
                    { "417", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 23", 71.989999999999995, 182 },
                    { "418", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 23GB", 19.989999999999998, 189 },
                    { "419", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 23m", 16.489999999999998, 196 },
                    { "420", "networking", "PCIe network interface card", "Network Card 23GbE", 35.990000000000002, 203 },
                    { "421", "accessories", "Laptop cooling pad with fans", "Cooling Pad 23", 19.989999999999998, 10 },
                    { "422", "accessories", "Privacy slide cover for webcam", "Webcam Cover 23", 4.4900000000000002, 17 },
                    { "423", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 23", 9.9900000000000002, 24 },
                    { "424", "components", "DDR4 3200MHz memory module", "DDR4 RAM 23GB", 54.990000000000002, 31 },
                    { "425", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 23GB", 107.98999999999999, 38 },
                    { "426", "furniture", "LED desk lamp with dimmer", "Desk Lamp 23W", 27.989999999999998, 45 },
                    { "427", "accessories", "Under-desk cable organizer", "Cable Management Kit 23", 17.989999999999998, 52 },
                    { "428", "mobile", "Qi wireless charging pad", "Wireless Charger 23W", 24.989999999999998, 59 },
                    { "429", "mobile", "Adjustable phone holder", "Phone Stand 23", 16.489999999999998, 66 },
                    { "430", "networking", "WiFi smart plug with timer", "Smart Plug 23", 15.59, 73 },
                    { "431", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat23", 7.1900000000000004, 80 },
                    { "432", "audio", "Portable Bluetooth speaker", "Mini Speaker 23", 35.990000000000002, 87 },
                    { "433", "mobile", "Tempered glass screen protector", "Screen Protector 23", 9.9900000000000002, 94 },
                    { "434", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 23", 65.989999999999995, 101 },
                    { "435", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 24", 95.989999999999995, 108 },
                    { "436", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 24GB", 15.99, 115 },
                    { "437", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 24m", 13.49, 122 },
                    { "438", "networking", "PCIe network interface card", "Network Card 24GbE", 29.989999999999998, 129 },
                    { "439", "accessories", "Laptop cooling pad with fans", "Cooling Pad 24", 27.489999999999998, 136 },
                    { "440", "accessories", "Privacy slide cover for webcam", "Webcam Cover 24", 5.9900000000000002, 143 },
                    { "441", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 24", 7.9900000000000002, 150 },
                    { "442", "components", "DDR4 3200MHz memory module", "DDR4 RAM 24GB", 44.990000000000002, 157 },
                    { "443", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 24GB", 89.989999999999995, 164 },
                    { "444", "furniture", "LED desk lamp with dimmer", "Desk Lamp 24W", 38.490000000000002, 171 },
                    { "445", "accessories", "Under-desk cable organizer", "Cable Management Kit 24", 23.989999999999998, 178 },
                    { "446", "mobile", "Qi wireless charging pad", "Wireless Charger 24W", 19.989999999999998, 185 },
                    { "447", "mobile", "Adjustable phone holder", "Phone Stand 24", 13.49, 192 },
                    { "448", "networking", "WiFi smart plug with timer", "Smart Plug 24", 12.99, 199 },
                    { "449", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat24", 9.8900000000000006, 206 },
                    { "450", "audio", "Portable Bluetooth speaker", "Mini Speaker 24", 47.990000000000002, 13 },
                    { "451", "mobile", "Tempered glass screen protector", "Screen Protector 24", 7.9900000000000002, 20 },
                    { "452", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 24", 53.990000000000002, 27 },
                    { "453", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 25", 79.989999999999995, 34 },
                    { "454", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 25GB", 21.989999999999998, 41 },
                    { "455", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 25m", 17.989999999999998, 48 },
                    { "456", "networking", "PCIe network interface card", "Network Card 25GbE", 23.989999999999998, 55 },
                    { "457", "accessories", "Laptop cooling pad with fans", "Cooling Pad 25", 22.489999999999998, 62 },
                    { "458", "accessories", "Privacy slide cover for webcam", "Webcam Cover 25", 4.9900000000000002, 69 },
                    { "459", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 25", 10.99, 76 },
                    { "460", "components", "DDR4 3200MHz memory module", "DDR4 RAM 25GB", 59.990000000000002, 83 },
                    { "461", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 25GB", 71.989999999999995, 90 },
                    { "462", "furniture", "LED desk lamp with dimmer", "Desk Lamp 25W", 31.489999999999998, 97 },
                    { "463", "accessories", "Under-desk cable organizer", "Cable Management Kit 25", 19.989999999999998, 104 },
                    { "464", "mobile", "Qi wireless charging pad", "Wireless Charger 25W", 27.489999999999998, 111 },
                    { "465", "mobile", "Adjustable phone holder", "Phone Stand 25", 17.989999999999998, 118 },
                    { "466", "networking", "WiFi smart plug with timer", "Smart Plug 25", 10.390000000000001, 125 },
                    { "467", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat25", 8.0899999999999999, 132 },
                    { "468", "audio", "Portable Bluetooth speaker", "Mini Speaker 25", 39.990000000000002, 139 },
                    { "469", "mobile", "Tempered glass screen protector", "Screen Protector 25", 10.99, 146 },
                    { "470", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 25", 71.989999999999995, 153 },
                    { "471", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 26", 63.990000000000002, 160 },
                    { "472", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 26GB", 17.989999999999998, 167 },
                    { "473", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 26m", 14.99, 174 },
                    { "474", "networking", "PCIe network interface card", "Network Card 26GbE", 32.990000000000002, 181 },
                    { "475", "accessories", "Laptop cooling pad with fans", "Cooling Pad 26", 29.989999999999998, 188 },
                    { "476", "accessories", "Privacy slide cover for webcam", "Webcam Cover 26", 3.9900000000000002, 195 },
                    { "477", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 26", 8.9900000000000002, 202 },
                    { "478", "components", "DDR4 3200MHz memory module", "DDR4 RAM 26GB", 49.990000000000002, 209 },
                    { "479", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 26GB", 98.989999999999995, 16 },
                    { "480", "furniture", "LED desk lamp with dimmer", "Desk Lamp 26W", 41.990000000000002, 23 },
                    { "481", "accessories", "Under-desk cable organizer", "Cable Management Kit 26", 15.99, 30 },
                    { "482", "mobile", "Qi wireless charging pad", "Wireless Charger 26W", 22.489999999999998, 37 },
                    { "483", "mobile", "Adjustable phone holder", "Phone Stand 26", 14.99, 44 },
                    { "484", "networking", "WiFi smart plug with timer", "Smart Plug 26", 14.289999999999999, 51 },
                    { "485", "networking", "Shielded ethernet cable 3m", "Ethernet Cable Cat26", 10.789999999999999, 58 },
                    { "486", "audio", "Portable Bluetooth speaker", "Mini Speaker 26", 31.989999999999998, 65 },
                    { "487", "mobile", "Tempered glass screen protector", "Screen Protector 26", 8.9900000000000002, 72 },
                    { "488", "peripherals", "Graphics drawing tablet USB", "Drawing Tablet 26", 59.990000000000002, 79 },
                    { "489", "audio", "Bluetooth 5.3 earbuds with ANC", "Wireless Earbuds 27", 87.989999999999995, 86 },
                    { "490", "storage", "USB 3.2 Gen2 flash drive", "USB Flash Drive 27GB", 23.989999999999998, 93 },
                    { "491", "accessories", "DP 1.4 HBR3 cable", "DisplayPort Cable 27m", 11.99, 100 },
                    { "492", "networking", "PCIe network interface card", "Network Card 27GbE", 26.989999999999998, 107 },
                    { "493", "accessories", "Laptop cooling pad with fans", "Cooling Pad 27", 24.989999999999998, 114 },
                    { "494", "accessories", "Privacy slide cover for webcam", "Webcam Cover 27", 5.4900000000000002, 121 },
                    { "495", "accessories", "USB-C to USB-A adapter", "Type-C Adapter 27", 11.99, 128 },
                    { "496", "components", "DDR4 3200MHz memory module", "DDR4 RAM 27GB", 39.990000000000002, 135 },
                    { "497", "storage", "NVMe M.2 2280 SSD", "PCIe SSD 27GB", 80.989999999999995, 142 },
                    { "498", "furniture", "LED desk lamp with dimmer", "Desk Lamp 27W", 34.990000000000002, 149 },
                    { "499", "accessories", "Under-desk cable organizer", "Cable Management Kit 27", 21.989999999999998, 156 },
                    { "500", "mobile", "Qi wireless charging pad", "Wireless Charger 27W", 29.989999999999998, 163 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "201");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "202");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "203");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "204");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "205");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "206");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "207");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "208");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "209");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "210");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "211");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "212");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "213");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "214");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "215");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "216");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "217");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "218");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "219");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "220");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "221");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "222");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "223");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "224");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "225");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "226");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "227");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "228");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "229");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "230");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "231");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "232");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "233");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "234");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "235");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "236");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "237");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "238");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "239");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "240");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "241");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "242");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "243");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "244");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "245");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "246");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "247");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "248");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "249");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "250");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "251");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "252");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "253");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "254");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "255");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "256");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "257");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "258");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "259");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "260");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "261");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "262");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "263");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "264");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "265");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "266");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "267");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "268");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "269");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "270");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "271");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "272");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "273");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "274");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "275");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "276");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "277");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "278");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "279");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "280");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "281");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "282");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "283");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "284");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "285");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "286");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "287");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "288");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "289");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "290");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "291");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "292");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "293");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "294");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "295");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "296");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "297");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "298");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "299");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "300");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "301");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "302");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "303");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "304");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "305");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "306");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "307");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "308");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "309");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "310");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "311");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "312");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "313");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "314");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "315");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "316");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "317");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "318");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "319");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "320");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "321");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "322");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "323");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "324");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "325");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "326");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "327");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "328");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "329");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "330");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "331");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "332");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "333");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "334");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "335");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "336");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "337");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "338");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "339");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "340");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "341");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "342");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "343");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "344");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "345");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "346");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "347");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "348");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "349");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "350");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "351");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "352");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "353");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "354");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "355");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "356");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "357");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "358");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "359");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "360");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "361");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "362");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "363");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "364");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "365");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "366");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "367");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "368");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "369");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "370");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "371");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "372");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "373");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "374");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "375");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "376");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "377");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "378");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "379");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "380");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "381");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "382");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "383");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "384");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "385");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "386");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "387");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "388");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "389");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "390");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "391");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "392");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "393");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "394");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "395");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "396");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "397");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "398");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "399");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "400");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "401");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "402");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "403");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "404");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "405");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "406");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "407");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "408");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "409");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "410");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "411");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "412");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "413");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "414");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "415");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "416");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "417");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "418");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "419");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "420");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "421");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "422");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "423");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "424");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "425");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "426");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "427");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "428");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "429");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "430");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "431");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "432");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "433");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "434");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "435");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "436");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "437");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "438");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "439");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "440");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "441");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "442");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "443");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "444");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "445");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "446");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "447");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "448");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "449");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "450");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "451");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "452");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "453");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "454");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "455");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "456");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "457");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "458");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "459");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "460");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "461");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "462");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "463");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "464");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "465");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "466");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "467");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "468");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "469");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "470");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "471");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "472");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "473");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "474");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "475");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "476");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "477");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "478");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "479");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "480");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "481");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "482");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "483");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "484");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "485");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "486");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "487");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "488");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "489");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "490");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "491");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "492");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "493");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "494");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "495");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "496");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "497");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "498");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "499");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: "500");
        }
    }
}
