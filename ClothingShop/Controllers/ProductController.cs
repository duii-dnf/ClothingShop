using ClothingShop.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace ClothingShop.Controllers
{
    public class ProductController : Controller
    {
        private ShopDbContext db = new ShopDbContext();


        public ActionResult Index(string searchString, int? categoryId, int? page)
        {
            var products = db.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            ViewBag.categoryId = new SelectList(db.Categories, "CategoryId", "Name", categoryId);
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentCategory = categoryId;

            // Sắp xếp
            products = products.OrderByDescending(p => p.CreatedAt);

            // Tính toán phân trang thủ công
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            int totalItems = products.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = pageNumber;

            // Lấy dữ liệu của trang hiện tại
            var pagedProducts = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return View(pagedProducts);
        }

        public ActionResult Details(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var relatedProducts = db.Products
                                    .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                                    .Take(4)
                                    .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
    }
}