using ClothingShop.Models;
using ClothingShop.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ClothingShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Search(string keyword, int? page = null)
        {
            return RedirectToAction("Index", new { keyword = keyword, searchString = keyword, page = page });
        }

        public ActionResult Index(
            int? categoryId,
            string keyword,
            string searchString,
            string sort,
            string gender,
            decimal? minPrice,
            decimal? maxPrice,
            bool? onSale,
            bool? isNew,
            int? page)
        {
            string term = !string.IsNullOrWhiteSpace(keyword) ? keyword.Trim() : (searchString ?? string.Empty).Trim();
            string cleanGender = !string.IsNullOrWhiteSpace(gender) ? gender.Trim() : null;
            string currentSort = ProductSorting.Normalize(sort);

            var query = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            Category activeCategory = null;
            if (categoryId.HasValue)
            {
                activeCategory = db.Categories
                    .Include(c => c.Parent)
                    .FirstOrDefault(c => c.CategoryId == categoryId.Value && c.IsActive);

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

            if (!string.IsNullOrEmpty(cleanGender))
            {
                query = query.Where(p => p.Gender != null && p.Gender.Equals(cleanGender, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) <= maxPrice.Value);
            }

            if (onSale == true)
            {
                query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price);
            }

            if (isNew == true)
            {
                query = query.Where(p => p.IsNew);
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
            ViewBag.Gender = cleanGender;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.OnSale = onSale == true;
            ViewBag.IsNew = isNew == true;
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
                .Include(p => p.Category.Parent)
                .FirstOrDefault(p => p.ProductId == id.Value && p.IsActive);

            if (product == null) return HttpNotFound();

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
                var siblingCatIds = db.Categories
                    .Where(c => c.IsActive && (c.CategoryId == rootCatId || c.ParentId == rootCatId))
                    .Select(c => c.CategoryId)
                    .ToList();

                var existingIds = relatedProducts.Select(r => r.ProductId).Concat(new[] { product.ProductId }).ToList();
                int needed = 4 - relatedProducts.Count;

                var moreFromParent = db.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && siblingCatIds.Contains(p.CategoryId) && !existingIds.Contains(p.ProductId))
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(needed)
                    .ToList();

                relatedProducts.AddRange(moreFromParent);
            }

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