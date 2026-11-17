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
            var products = db.Products.Include(p => p.Category).AsQueryable();

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
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
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
                else
                {
                    product.ImageUrl = "default.jpg";
                }

                product.CreatedAt = DateTime.Now;
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công.";
                return RedirectToAction("Index");
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

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
