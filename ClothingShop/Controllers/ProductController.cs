using ClothingShop.Models;
using ClothingShop.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ClothingShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index(int? categoryId, string keyword, string searchString, string sort, int? page)
        {
            string term = !string.IsNullOrWhiteSpace(keyword) ? keyword.Trim() : (searchString ?? string.Empty).Trim();
            string currentSort = ProductSorting.Normalize(sort);

            var query = db.Products.Include(p => p.Category).Where(p => p.IsActive);

            Category activeCategory = null;
            if (categoryId.HasValue)
            {
                activeCategory = db.Categories.FirstOrDefault(c => c.CategoryId == categoryId.Value && c.IsActive);
                var catIds = db.Categories
                    .Where(c => c.IsActive && (c.CategoryId == categoryId.Value || c.ParentId == categoryId.Value))
                    .Select(c => c.CategoryId)
                    .ToList();

                query = query.Where(p => catIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));
            }

            query = ProductSorting.Apply(query, currentSort);

            int pageSize = 12;
            int pageNumber = (page.HasValue && page.Value > 0) ? page.Value : 1;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages > 0 && pageNumber > totalPages) pageNumber = totalPages;

            var products = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = db.Categories
                .Include(c => c.Children)
                .Where(c => c.IsActive && c.ParentId == null)
                .OrderBy(c => c.CategoryId)
                .ToList();

            ViewBag.CurrentCategory = activeCategory;
            ViewBag.CategoryId = categoryId;
            ViewBag.Keyword = term;
            ViewBag.Sort = currentSort;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(products);
        }

        public ActionResult Search(string keyword)
        {
            return RedirectToAction("Index", new { keyword = keyword });
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return HttpNotFound();

            var product = db.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id.Value && p.IsActive);

            if (product == null) return HttpNotFound();

            var relatedProducts = db.Products
                .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}