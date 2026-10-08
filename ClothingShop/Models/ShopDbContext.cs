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
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

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