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

        public ActionResult Index(int? categoryId, string keyword, string searchString, string sort, string filter, int? page)
        {
            string term = !string.IsNullOrWhiteSpace(keyword) ? keyword.Trim() : (searchString ?? string.Empty).Trim();
            string currentSort = ProductSorting.Normalize(sort);

            var query = db.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().ToLowerInvariant();
                if (filter == "sale" || filter == "deal")
                {
                    query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price);
                }
                else if (filter == "new")
                {
                    query = query.Where(p => p.IsNew);
                }
                else if (filter == "bestseller" || filter == "hot" || filter == "featured")
                {
                    query = query.Where(p => p.IsFeatured);
                }
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