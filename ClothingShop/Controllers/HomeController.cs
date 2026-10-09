using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var vm = new HomeViewModel();

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

            // Lưới 1: 12 sản phẩm mới nhất
            vm.NewProducts = db.Products
                               .Where(p => p.IsActive)
                               .OrderByDescending(p => p.CreatedAt)
                               .Take(12).ToList();

            var shownIds = vm.NewProducts.Select(p => p.ProductId).ToList();

            // Lưới 2: 8 sản phẩm, ưu tiên "nổi bật", không trùng lưới 1
            vm.FeaturedProducts = db.Products
                                    .Where(p => p.IsActive && !shownIds.Contains(p.ProductId))
                                    .OrderByDescending(p => p.IsFeatured)
                                    .ThenByDescending(p => p.CreatedAt)
                                    .Take(8).ToList();

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}