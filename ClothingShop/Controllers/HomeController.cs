using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class HomeController : Controller
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ActionResult Index(string sort = null)
        {
            var vm = new HomeViewModel { Sort = ProductSorting.Normalize(sort) };

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

            vm.NewProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.IsNew)
                .ThenByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

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
                bestSellers.AddRange(db.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !existingIds.Contains(p.ProductId))
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(8 - bestSellers.Count)
                    .ToList());
            }
            vm.BestSellerProducts = bestSellers;

            vm.SaleProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price)
                .OrderByDescending(p => p.CreatedAt)
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
