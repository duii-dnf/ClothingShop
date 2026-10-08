namespace ClothingShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryId)
                .ForeignKey("dbo.Categories", t => t.ParentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPrice = c.Decimal(precision: 18, scale: 2),
                        ImageUrl = c.String(maxLength: 300),
                        Gender = c.String(maxLength: 10),
                        IsNew = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        Sizes = c.String(maxLength: 100),
                        Colors = c.String(maxLength: 100),
                        Stock = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Coupons",
                c => new
                    {
                        CouponId = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 30),
                        DiscountPercent = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        MinOrder = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UsageLimit = c.Int(nullable: false),
                        UsedCount = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CouponId)
                .Index(t => t.Code, unique: true);
            
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderDetailId = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        Size = c.String(maxLength: 20),
                        Color = c.String(maxLength: 30),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.OrderDetailId)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.OrderId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        ReceiverName = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(nullable: false, maxLength: 20),
                        Address = c.String(nullable: false, maxLength: 250),
                        PaymentMethod = c.String(maxLength: 30),
                        Status = c.String(nullable: false, maxLength: 20),
                        Note = c.String(maxLength: 300),
                        CouponId = c.Int(),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Coupons", t => t.CouponId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.CouponId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false, maxLength: 150),
                        PasswordHash = c.String(nullable: false, maxLength: 200),
                        Phone = c.String(maxLength: 20),
                        Address = c.String(maxLength: 250),
                        Role = c.String(nullable: false, maxLength: 20),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.Email, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetails", "ProductId", "dbo.Products");
            DropForeignKey("dbo.OrderDetails", "OrderId", "dbo.Orders");
            DropForeignKey("dbo.Orders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Orders", "CouponId", "dbo.Coupons");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Categories", "ParentId", "dbo.Categories");
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.Orders", new[] { "CouponId" });
            DropIndex("dbo.Orders", new[] { "UserId" });
            DropIndex("dbo.OrderDetails", new[] { "ProductId" });
            DropIndex("dbo.OrderDetails", new[] { "OrderId" });
            DropIndex("dbo.Coupons", new[] { "Code" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.Categories", new[] { "ParentId" });
            DropTable("dbo.Users");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderDetails");
            DropTable("dbo.Coupons");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
