using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothingShop.Models
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext() : base("ShopDbContext")
        {
            Database.SetInitializer<ShopDbContext>(null);
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<ProductStyle> ProductStyles { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Loại cha - loại con (tự tham chiếu), không xóa dây chuyền
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .WillCascadeOnDelete(false);

            // Không cho xóa loại nếu còn sản phẩm
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);

            // Quan hệ nhiều - nhiều giữa Sản phẩm và Phong cách
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Styles)
                .WithMany(s => s.Products)
                .Map(m =>
                {
                    m.ToTable("ProductStyleMappings");
                    m.MapLeftKey("ProductId");
                    m.MapRightKey("StyleId");
                });

            // Biến thể sản phẩm (ProductVariant)
            modelBuilder.Entity<ProductVariant>()
                .HasRequired(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ProductVariant>()
                .HasOptional(v => v.Size)
                .WithMany(s => s.Variants)
                .HasForeignKey(v => v.SizeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ProductVariant>()
                .HasOptional(v => v.Color)
                .WithMany(c => c.Variants)
                .HasForeignKey(v => v.ColorId)
                .WillCascadeOnDelete(false);

            // Sổ địa chỉ người dùng
            modelBuilder.Entity<UserAddress>()
                .HasRequired(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .WillCascadeOnDelete(true);

            // Lịch sử tồn kho
            modelBuilder.Entity<InventoryLog>()
                .HasRequired(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InventoryLog>()
                .HasOptional(l => l.Variant)
                .WithMany(v => v.InventoryLogs)
                .HasForeignKey(l => l.VariantId)
                .WillCascadeOnDelete(false);

            // Không xóa user/sản phẩm làm mất lịch sử đơn hàng
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderDetail>()
                .HasOptional(d => d.Variant)
                .WithMany(v => v.OrderDetails)
                .HasForeignKey(d => d.ProductVariantId)
                .WillCascadeOnDelete(false);

            // Xóa đơn hàng thì xóa luôn chi tiết đơn
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(d => d.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
