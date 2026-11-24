using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class ProductController : AdminBaseController
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Admin/Product
        public ActionResult Index(string keyword, int? categoryId)
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Variants)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                products = products.Where(p => p.Name.Contains(keyword.Trim()));

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value);

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryId", "Name", categoryId);
            ViewBag.Keyword = keyword;

            return View(products.OrderByDescending(p => p.ProductId).ToList());
        }

        // GET: Admin/Product/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products
                .Include(p => p.Category)
                .Include("Variants.Size")
                .Include("Variants.Color")
                .Include("Styles")
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // GET: Admin/Product/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryId", "Name");
            return View(new Product { Gender = "Unisex", IsActive = true, Stock = 10 });
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Product product, HttpPostedFileBase fileUpload)
        {
            if (ModelState.IsValid)
            {
                if (fileUpload != null && fileUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileUpload.FileName);
                    string folder = Server.MapPath("~/Content/images/products");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    fileUpload.SaveAs(Path.Combine(folder, fileName));
                    product.ImageUrl = fileName;
                }
                else if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    product.ImageUrl = product.ImageUrl.Trim();
                }
                else
                {
                    product.ImageUrl = "default.jpg";
                }

                product.CreatedAt = DateTime.Now;
                db.Products.Add(product);
                db.SaveChanges();

                // Generate default Freesize / Black variant if none
                var defaultSize = db.ProductSizes.FirstOrDefault(s => s.Code == "Freesize") ?? db.ProductSizes.FirstOrDefault();
                var defaultColor = db.ProductColors.FirstOrDefault(c => c.Code == "BLACK") ?? db.ProductColors.FirstOrDefault();
                var initialVariant = new ProductVariant
                {
                    ProductId = product.ProductId,
                    SizeId = defaultSize?.SizeId,
                    ColorId = defaultColor?.ColorId,
                    SKU = $"SP{product.ProductId:03d}-DEF",
                    Stock = product.Stock,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                db.ProductVariants.Add(initialVariant);
                db.SaveChanges();

                db.InventoryLogs.Add(new InventoryLog
                {
                    ProductId = product.ProductId,
                    VariantId = initialVariant.VariantId,
                    ChangeType = "InitialStock",
                    QuantityChange = product.Stock,
                    StockAfter = product.Stock,
                    ReferenceType = "AdminProduct",
                    ReferenceId = product.ProductId,
                    Note = "Khởi tạo sản phẩm mới",
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Admin"
                });
                db.SaveChanges();

                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction("Variants", new { id = product.ProductId });
            }

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product, HttpPostedFileBase fileUpload)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Products.Find(product.ProductId);
                if (existing == null) return HttpNotFound();

                if (fileUpload != null && fileUpload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileUpload.FileName);
                    string folder = Server.MapPath("~/Content/images/products");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    fileUpload.SaveAs(Path.Combine(folder, fileName));
                    existing.ImageUrl = fileName;
                }
                else if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    existing.ImageUrl = product.ImageUrl.Trim();
                }

                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.DiscountPrice = product.DiscountPrice;
                existing.Gender = product.Gender;
                existing.IsNew = product.IsNew;
                existing.IsFeatured = product.IsFeatured;
                existing.Sizes = product.Sizes;
                existing.Colors = product.Colors;
                existing.Stock = product.Stock;
                existing.IsActive = product.IsActive;
                existing.CategoryId = product.CategoryId;

                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories.OrderBy(c => c.Name), "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            if (db.OrderDetails.Any(d => d.ProductId == id))
            {
                TempData["Error"] = "Không thể xóa sản phẩm đã có trong đơn hàng.";
                return RedirectToAction("Index");
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Product/Variants/5
        public ActionResult Variants(int id)
        {
            var product = db.Products
                .Include(p => p.Category)
                .Include("Variants.Size")
                .Include("Variants.Color")
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return HttpNotFound();

            ViewBag.Sizes = db.ProductSizes.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToList();
            ViewBag.Colors = db.ProductColors.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();
            ViewBag.Logs = db.InventoryLogs
                .Where(l => l.ProductId == id)
                .OrderByDescending(l => l.CreatedAt)
                .Take(25)
                .ToList();

            return View(product);
        }

        // POST: Admin/Product/UpdateVariantStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateVariantStock(int variantId, int stock, decimal? price, bool isActive = true)
        {
            var variant = db.ProductVariants.Include(v => v.Product).FirstOrDefault(v => v.VariantId == variantId);
            if (variant == null) return HttpNotFound();

            int oldStock = variant.Stock;
            int diff = stock - oldStock;

            variant.Stock = stock;
            variant.Price = price;
            variant.IsActive = isActive;

            if (diff != 0)
            {
                db.InventoryLogs.Add(new InventoryLog
                {
                    ProductId = variant.ProductId,
                    VariantId = variant.VariantId,
                    ChangeType = "AdminAdjustment",
                    QuantityChange = diff,
                    StockAfter = stock,
                    ReferenceType = "Admin",
                    ReferenceId = variant.VariantId,
                    Note = $"Admin điều chỉnh tồn kho: {oldStock} -> {stock}",
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Admin"
                });
            }

            db.SaveChanges();

            // Recalculate total product stock
            var totalStock = db.ProductVariants.Where(v => v.ProductId == variant.ProductId && v.IsActive).Sum(v => (int?)v.Stock) ?? 0;
            variant.Product.Stock = totalStock;
            db.SaveChanges();

            TempData["Success"] = $"Đã cập nhật biến thể SKU '{variant.SKU}' (Tồn kho: {stock}).";
            return RedirectToAction("Variants", new { id = variant.ProductId });
        }

        // POST: Admin/Product/CreateVariant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateVariant(int productId, int? sizeId, int? colorId, string sku, int stock, decimal? price)
        {
            var product = db.Products.Find(productId);
            if (product == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(sku))
            {
                var sCode = sizeId.HasValue ? db.ProductSizes.Find(sizeId.Value)?.Code : "FS";
                var cCode = colorId.HasValue ? db.ProductColors.Find(colorId.Value)?.Code : "COL";
                sku = $"SP{productId:03d}-{sCode}-{cCode}";
            }

            if (db.ProductVariants.Any(v => v.ProductId == productId && v.SKU == sku))
            {
                TempData["Error"] = $"Mã SKU '{sku}' đã tồn tại cho sản phẩm này.";
                return RedirectToAction("Variants", new { id = productId });
            }

            var variant = new ProductVariant
            {
                ProductId = productId,
                SizeId = sizeId,
                ColorId = colorId,
                SKU = sku.Trim(),
                Stock = stock,
                Price = price,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            db.ProductVariants.Add(variant);
            db.SaveChanges();

            db.InventoryLogs.Add(new InventoryLog
            {
                ProductId = productId,
                VariantId = variant.VariantId,
                ChangeType = "VariantCreated",
                QuantityChange = stock,
                StockAfter = stock,
                ReferenceType = "Admin",
                ReferenceId = variant.VariantId,
                Note = $"Tạo biến thể mới SKU: {sku}",
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "Admin"
            });

            var totalStock = db.ProductVariants.Where(v => v.ProductId == productId && v.IsActive).Sum(v => (int?)v.Stock) ?? 0;
            product.Stock = totalStock;
            db.SaveChanges();

            TempData["Success"] = $"Đã thêm biến thể mới '{sku}' thành công!";
            return RedirectToAction("Variants", new { id = productId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
