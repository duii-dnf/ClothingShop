using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.ViewModels;
using ClothingShop.Services;

namespace ClothingShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index(string sort = null)
        {
            var vm = new HomeViewModel { Sort = ProductSorting.Normalize(sort) };

            // Ô danh mục: mỗi loại cha lấy ảnh của sản phẩm mới nhất thuộc loại đó (hoặc loại con)
            var cats = db.Categories.Where(c => c.IsActive).ToList();
            foreach (var parent in cats.Where(c => c.ParentId == null).OrderBy(c => c.CategoryId))
            {
                var ids = cats.Where(c => c.ParentId == parent.CategoryId).Select(c => c.CategoryId).ToList();
                ids.Add(parent.CategoryId);

                var image = db.Products
                              .Where(p => p.IsActive && ids.Contains(p.CategoryId))
                              .OrderByDescending(p => p.CreatedAt)
                              .Select(p => p.ImageUrl)
                              .FirstOrDefault();

                vm.Categories.Add(new HomeCategoryTile { Id = parent.CategoryId, Name = parent.Name, ImageUrl = image });
            }

            // 1. Phân khu 1: Sản phẩm mới về (New Arrivals) - ưu tiên IsNew hoặc mới nhất
            vm.NewProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.IsNew)
                .ThenByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            // 2. Phân khu 2: Sản phẩm bán chạy (Best Sellers) - tính từ OrderDetails thực tế + IsFeatured
            var topSoldIds = db.OrderDetails
                .GroupBy(d => d.ProductId)
                .OrderByDescending(g => g.Sum(x => x.Quantity))
                .Select(g => g.Key)
                .Take(8)
                .ToList();

            var bestSellers = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && topSoldIds.Contains(p.ProductId))
                .ToList();

            if (bestSellers.Count < 8)
            {
                var existingIds = bestSellers.Select(p => p.ProductId).ToList();
                var fallback = db.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !existingIds.Contains(p.ProductId))
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(8 - bestSellers.Count)
                    .ToList();
                bestSellers.AddRange(fallback);
            }
            vm.BestSellerProducts = bestSellers;

            // 3. Phân khu 3: Sản phẩm khuyến mãi (Flash Sale / Hot Deals) - thực sự có DiscountPrice < Price
            var saleList = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price)
                .ToList();

            vm.SaleProducts = saleList
                .OrderByDescending(p => p.Price > 0 ? (p.Price - p.DiscountPrice.Value) / p.Price : 0)
                .Take(8)
                .ToList();

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}