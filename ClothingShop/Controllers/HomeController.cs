using System;
using System.Collections.Generic;
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

            // Lưới chính: sắp xếp toàn bộ sản phẩm đang bán rồi lấy 12 sản phẩm đầu
            vm.NewProducts = ProductSorting.Apply(db.Products.Where(p => p.IsActive), vm.Sort)
                                           .Take(12).ToList();

            // Lưới phụ "nổi bật" chỉ hiện khi để sắp xếp mặc định (tránh lẫn với danh sách người dùng đã chọn)
            if (vm.Sort == ProductSorting.Newest)
            {
                var shownIds = vm.NewProducts.Select(p => p.ProductId).ToList();
                vm.FeaturedProducts = db.Products
                                        .Where(p => p.IsActive && !shownIds.Contains(p.ProductId))
                                        .OrderByDescending(p => p.IsFeatured)
                                        .ThenByDescending(p => p.CreatedAt)
                                        .Take(8).ToList();
            }

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}