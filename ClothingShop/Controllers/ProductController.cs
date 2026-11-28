using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;

namespace ClothingShop.Controllers
{
    public class ProductController : Controller
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ActionResult Search(string keyword, int? page = null)
        {
            return RedirectToAction("Index", new { keyword = keyword, page = page });
        }

        public ActionResult Index(
            int? categoryId = null,
            string keyword = null,
            string searchString = null,
            string sort = null,
            string filter = null,
            string gender = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? onSale = null,
            bool? isNew = null,
            int? page = null)
        {
            string term = !string.IsNullOrWhiteSpace(keyword) ? keyword.Trim() : (searchString ?? string.Empty).Trim();
            string cleanGender = !string.IsNullOrWhiteSpace(gender) ? gender.Trim() : null;
            string currentSort = ProductSorting.Normalize(sort);

            var query = db.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().ToLowerInvariant();
                if (filter == "sale" || filter == "deal") onSale = true;
                else if (filter == "new") isNew = true;
                else if (filter == "bestseller" || filter == "hot" || filter == "featured")
                    query = query.Where(p => p.IsFeatured);
            }

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
                query = query.Where(p => p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));

            if (!string.IsNullOrEmpty(cleanGender))
                query = query.Where(p => p.Gender == cleanGender);

            if (minPrice.HasValue)
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) <= maxPrice.Value);

            if (onSale == true)
                query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price);

            if (isNew == true)
                query = query.Where(p => p.IsNew);

            query = ProductSorting.Apply(query, currentSort);

            int pageSize = 12;
            int pageNumber = (page.HasValue && page.Value > 0) ? page.Value : 1;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages > 0 && pageNumber > totalPages) pageNumber = totalPages;

            var products = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.AllCategories = db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryId).ToList();
            ViewBag.CurrentCategory = activeCategory;
            ViewBag.CategoryId = categoryId;
            ViewBag.Keyword = term;
            ViewBag.Sort = currentSort;
            ViewBag.Gender = cleanGender;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.OnSale = onSale == true;
            ViewBag.IsNew = isNew == true;
            ViewBag.Filter = filter;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(products);
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return HttpNotFound();

            var product = db.Products
                .Include(p => p.Category)
                .Include("ProductVariants.ProductSize")
                .Include("ProductVariants.ProductColor")
                .Include("ProductStyles")
                .FirstOrDefault(p => p.ProductId == id.Value && p.IsActive);

            if (product == null) return HttpNotFound();

            Category parentCategory = null;
            if (product.Category != null && product.Category.ParentId.HasValue)
            {
                parentCategory = db.Categories.FirstOrDefault(c => c.CategoryId == product.Category.ParentId.Value);
            }

            var relatedProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToList();

            if (relatedProducts.Count < 4 && product.Category != null)
            {
                int rootCatId = product.Category.ParentId ?? product.CategoryId;
                var siblingIds = db.Categories
                    .Where(c => c.IsActive && (c.CategoryId == rootCatId || c.ParentId == rootCatId))
                    .Select(c => c.CategoryId)
                    .ToList();

                var excludeIds = relatedProducts.Select(r => r.ProductId).Concat(new[] { product.ProductId }).ToList();
                relatedProducts.AddRange(db.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && siblingIds.Contains(p.CategoryId) && !excludeIds.Contains(p.ProductId))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(4 - relatedProducts.Count)
                    .ToList());
            }

            ViewBag.ParentCategory = parentCategory;
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