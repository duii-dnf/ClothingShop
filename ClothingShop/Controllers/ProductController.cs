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

        [HttpGet]
        public ActionResult Suggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new { success = true, count = 0, items = new object[0] }, JsonRequestBehavior.AllowGet);
            }

            string cleanTerm = term.Trim().ToLower();

            var matches = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (p.Name.ToLower().Contains(cleanTerm) || (p.Description != null && p.Description.ToLower().Contains(cleanTerm))))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(6)
                .ToList();

            var items = matches.Select(p =>
            {
                var onSale = p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price;
                var currentPrice = onSale ? p.DiscountPrice.Value : p.Price;
                var rawImg = (p.ImageUrl ?? string.Empty).Trim();
                var fallbackImg = Url.Content("~/Content/images/products/default.jpg");
                var imgSrc = string.IsNullOrEmpty(rawImg)
                    ? fallbackImg
                    : (rawImg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || rawImg.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        ? rawImg
                        : Url.Content(rawImg.StartsWith("~") || rawImg.StartsWith("/") ? rawImg : "~/Content/images/products/" + rawImg));

                int discountPercent = onSale && p.Price > 0
                    ? (int)Math.Round((p.Price - p.DiscountPrice.Value) / p.Price * 100)
                    : 0;

                return new
                {
                    id = p.ProductId,
                    name = p.Name,
                    category = p.Category != null ? p.Category.Name : string.Empty,
                    price = PriceFormatter.Vnd(currentPrice),
                    oldPrice = onSale ? PriceFormatter.Vnd(p.Price) : null,
                    discountPercent = discountPercent > 0 ? ("-" + discountPercent + "%") : null,
                    imageUrl = imgSrc,
                    url = Url.Action("Details", "Product", new { area = "", id = p.ProductId })
                };
            }).ToList();

            return Json(new { success = true, count = items.Count, items = items }, JsonRequestBehavior.AllowGet);
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

            var query = db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().ToLowerInvariant();
                if (filter == "sale" || filter == "deal")
                {
                    onSale = true;
                }
                else if (filter == "new")
                {
                    isNew = true;
                }
                else if (filter == "bestseller" || filter == "hot" || filter == "featured")
                {
                    query = query.Where(p => p.IsFeatured);
                }
            }

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
                query = query.Where(p => p.Gender != null && p.Gender == cleanGender);
            }

            if (minPrice.HasValue)
            {
                decimal min = minPrice.Value;
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) >= min);
            }

            if (maxPrice.HasValue)
            {
                decimal max = maxPrice.Value;
                query = query.Where(p => ((p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price) ? p.DiscountPrice.Value : p.Price) <= max);
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