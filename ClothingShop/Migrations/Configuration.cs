namespace ClothingShop.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Collections.Generic;
    using ClothingShop.Models;
    using ClothingShop.Services;

    internal sealed class Configuration : DbMigrationsConfiguration<ClothingShop.Models.ShopDbContext>
    {
        private const string DefaultImage = "/Content/images/products/default.jpg";

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ShopDbContext context)
        {
            SeedUsers(context);
            SeedCategories(context);
            SeedProducts(context);
            SeedCoupons(context);
            SeedOrders(context);
        }

        private static void SeedUsers(ShopDbContext context)
        {
            if (context.Users.Any()) return;

            context.Users.AddRange(new[]
            {
                new AppUser { FullName = "Quản trị viên", Email = "admin@shop.com",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    Phone = "0900000001", Address = "TP. Hồ Chí Minh", Role = AppRoles.Admin },
                new AppUser { FullName = "Nguyễn Văn An", Email = "khach1@shop.com",
                    PasswordHash = PasswordHasher.Hash("Khach@123"),
                    Phone = "0900000002", Address = "12 Nguyễn Huệ, Quận 1, TP.HCM", Role = AppRoles.Customer },
                new AppUser { FullName = "Trần Thị Bình", Email = "khach2@shop.com",
                    PasswordHash = PasswordHasher.Hash("Khach@123"),
                    Phone = "0900000003", Address = "45 Lê Lợi, Quận 3, TP.HCM", Role = AppRoles.Customer }
            });
            context.SaveChanges();
        }

        private static void SeedCategories(ShopDbContext context)
        {
            if (context.Categories.Any()) return;

            var ao = new Category { Name = "Áo" };
            var quan = new Category { Name = "Quần" };
            var vay = new Category { Name = "Váy - Đầm" };
            var phuKien = new Category { Name = "Phụ kiện" };
            context.Categories.AddRange(new[] { ao, quan, vay, phuKien });
            context.SaveChanges(); // để có CategoryId của loại cha

            context.Categories.AddRange(new[]
            {
                new Category { Name = "Áo thun", ParentId = ao.CategoryId },
                new Category { Name = "Áo sơ mi", ParentId = ao.CategoryId },
                new Category { Name = "Áo khoác", ParentId = ao.CategoryId },
                new Category { Name = "Quần jean", ParentId = quan.CategoryId },
                new Category { Name = "Quần short", ParentId = quan.CategoryId },
                new Category { Name = "Quần tây", ParentId = quan.CategoryId },
                new Category { Name = "Váy ngắn", ParentId = vay.CategoryId },
                new Category { Name = "Đầm dự tiệc", ParentId = vay.CategoryId },
                new Category { Name = "Nón", ParentId = phuKien.CategoryId },
                new Category { Name = "Thắt lưng", ParentId = phuKien.CategoryId },
                new Category { Name = "Túi xách", ParentId = phuKien.CategoryId }
            });
            context.SaveChanges();
        }

        private static void SeedProducts(ShopDbContext context)
        {
            if (context.Products.Any()) return;

            var cats = context.Categories.ToDictionary(c => c.Name, c => c.CategoryId);
            int day = 0;

            Product P(string name, string cat, decimal price, decimal? sale, string gender,
                      bool isNew, bool featured, string sizes, string colors, int stock)
            {
                day++;
                return new Product
                {
                    Name = name,
                    CategoryId = cats[cat],
                    Price = price,
                    DiscountPrice = sale,
                    Gender = gender,
                    IsNew = isNew,
                    IsFeatured = featured,
                    Sizes = sizes,
                    Colors = colors,
                    Stock = stock,
                    Description = "Chất liệu thoáng mát, thiết kế hiện đại, dễ phối đồ.",
                    ImageUrl = DefaultImage,
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-day)
                };
            }

            const string clothSizes = "S,M,L,XL";
            const string pantSizes = "28,29,30,31,32";

            context.Products.AddRange(new[]
            {
                // Áo thun (4)
                P("Áo thun cotton basic", "Áo thun", 150000, null, "Unisex", false, true, clothSizes, "Đen,Trắng,Xám", 50),
                P("Áo thun polo nam", "Áo thun", 250000, 199000, "Nam", false, false, clothSizes, "Xanh navy,Trắng", 40),
                P("Áo thun oversize form rộng", "Áo thun", 220000, null, "Unisex", true, true, "M,L,XL", "Đen,Be", 35),
                P("Áo thun nữ croptop", "Áo thun", 180000, 149000, "Nữ", true, false, "S,M,L", "Hồng,Trắng", 30),
                // Áo sơ mi (3)
                P("Áo sơ mi trắng công sở", "Áo sơ mi", 320000, null, "Nam", false, true, clothSizes, "Trắng", 25),
                P("Áo sơ mi kẻ sọc", "Áo sơ mi", 350000, 290000, "Nam", false, false, clothSizes, "Xanh,Trắng", 20),
                P("Áo sơ mi nữ lụa", "Áo sơ mi", 380000, null, "Nữ", true, false, "S,M,L", "Kem,Hồng nhạt", 18),
                // Áo khoác (3)
                P("Áo khoác bomber", "Áo khoác", 550000, 480000, "Unisex", true, true, "M,L,XL", "Đen,Xanh rêu", 22),
                P("Áo khoác jean", "Áo khoác", 620000, null, "Unisex", false, false, "M,L,XL", "Xanh đậm", 15),
                P("Áo khoác gió nữ", "Áo khoác", 450000, 399000, "Nữ", false, false, "S,M,L", "Be,Hồng", 20),
                // Quần jean (3)
                P("Quần jean slim fit", "Quần jean", 480000, null, "Nam", false, true, pantSizes, "Xanh đậm,Đen", 30),
                P("Quần jean ống rộng", "Quần jean", 520000, 450000, "Nữ", true, false, pantSizes, "Xanh nhạt", 25),
                P("Quần jean rách gối", "Quần jean", 500000, null, "Nam", false, false, pantSizes, "Xanh", 18),
                // Quần short (2)
                P("Quần short kaki", "Quần short", 220000, null, "Nam", false, false, pantSizes, "Be,Đen,Xám", 40),
                P("Quần short thể thao", "Quần short", 180000, 150000, "Unisex", false, false, clothSizes, "Đen,Xanh", 45),
                // Quần tây (2)
                P("Quần tây công sở", "Quần tây", 420000, null, "Nam", false, true, pantSizes, "Đen,Xám", 28),
                P("Quần tây ống đứng nữ", "Quần tây", 440000, 380000, "Nữ", true, false, "S,M,L", "Đen,Be", 20),
                // Váy ngắn (2)
                P("Váy chữ A xếp ly", "Váy ngắn", 290000, null, "Nữ", true, true, "S,M,L", "Đen,Kem", 24),
                P("Váy jean mini", "Váy ngắn", 260000, 220000, "Nữ", false, false, "S,M,L", "Xanh", 20),
                // Đầm dự tiệc (2)
                P("Đầm dạ hội ôm dáng", "Đầm dự tiệc", 890000, 790000, "Nữ", false, true, "S,M,L", "Đỏ,Đen", 10),
                P("Đầm công chúa dự tiệc", "Đầm dự tiệc", 950000, null, "Nữ", true, false, "S,M,L", "Hồng,Trắng", 8),
                // Nón (2)
                P("Nón lưỡi trai", "Nón", 120000, null, "Unisex", false, false, "Free size", "Đen,Trắng,Xanh", 60),
                P("Nón bucket", "Nón", 140000, 110000, "Unisex", true, false, "Free size", "Be,Đen", 50),
                // Thắt lưng (1)
                P("Thắt lưng da bò", "Thắt lưng", 280000, null, "Nam", false, false, "100cm,110cm,120cm", "Đen,Nâu", 30),
                // Túi xách (1)
                P("Túi xách đeo chéo", "Túi xách", 350000, 299000, "Nữ", true, true, "Free size", "Đen,Nâu,Kem", 25)
            });
            context.SaveChanges();
        }

        private static void SeedCoupons(ShopDbContext context)
        {
            if (context.Coupons.Any()) return;

            context.Coupons.AddRange(new[]
            {
                new Coupon { Code = "SALE10", DiscountPercent = 10,
                    StartDate = DateTime.Today.AddMonths(-1), EndDate = DateTime.Today.AddMonths(6),
                    MinOrder = 200000, UsageLimit = 100, IsActive = true },
                new Coupon { Code = "NEWUSER", DiscountPercent = 15,
                    StartDate = DateTime.Today.AddMonths(-1), EndDate = DateTime.Today.AddMonths(6),
                    MinOrder = 300000, UsageLimit = 50, IsActive = true },
                // Mã đã hết hạn, dùng để kiểm thử trường hợp mã sai/hết hạn
                new Coupon { Code = "HETHAN5", DiscountPercent = 5,
                    StartDate = DateTime.Today.AddMonths(-3), EndDate = DateTime.Today.AddMonths(-1),
                    MinOrder = 0, UsageLimit = 100, IsActive = true }
            });
            context.SaveChanges();
        }

        private static void SeedOrders(ShopDbContext context)
        {
            if (context.Orders.Any()) return;

            var customers = context.Users.Where(u => u.Role == AppRoles.Customer).ToList();
            var products = context.Products.ToList();
            var sale10 = context.Coupons.First(c => c.Code == "SALE10");
            var rnd = new Random(1); // cố định để mọi máy sinh dữ liệu giống nhau

            int[] daysAgo = { 40, 33, 27, 20, 14, 10, 6, 3, 1, 0 };
            string[] statuses =
            {
                OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed,
                OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed,
                OrderStatus.Shipping, OrderStatus.Confirmed, OrderStatus.Pending, OrderStatus.Cancelled
            };

            for (int i = 0; i < daysAgo.Length; i++)
            {
                var customer = customers[i % customers.Count];
                var order = new Order
                {
                    UserId = customer.UserId,
                    OrderDate = DateTime.Now.AddDays(-daysAgo[i]),
                    ReceiverName = customer.FullName,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    PaymentMethod = (i % 2 == 0) ? PaymentMethods.COD : PaymentMethods.BankTransfer,
                    Status = statuses[i]
                };

                decimal total = 0;
                int lines = rnd.Next(1, 4);
                for (int j = 0; j < lines; j++)
                {
                    var p = products[rnd.Next(products.Count)];
                    int qty = rnd.Next(1, 4);
                    decimal price = p.DiscountPrice ?? p.Price;

                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = p.ProductId,
                        Size = FirstOf(p.Sizes),
                        Color = FirstOf(p.Colors),
                        Quantity = qty,
                        UnitPrice = price
                    });
                    total += price * qty;
                }

                if (i == 2) // một đơn có dùng mã SALE10
                {
                    order.CouponId = sale10.CouponId;
                    order.DiscountAmount = Math.Round(total * sale10.DiscountPercent / 100m, 0);
                }
                order.TotalAmount = total - order.DiscountAmount;

                context.Orders.Add(order);
            }
            context.SaveChanges();
        }

        private static string FirstOf(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return null;
            return csv.Split(',')[0].Trim();
        }
    }
}

