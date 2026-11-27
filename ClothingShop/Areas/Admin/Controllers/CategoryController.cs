using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class CategoryController : AdminBaseController
    {
        private readonly ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ActionResult Index(string keyword, string level, string status)
        {
            var allCategories = db.Categories.ToList();
            ViewBag.TotalCount = allCategories.Count;
            ViewBag.RootCount = allCategories.Count(c => c.ParentId == null);
            ViewBag.ChildCount = allCategories.Count(c => c.ParentId != null);
            ViewBag.ActiveCount = allCategories.Count(c => c.IsActive);

            var query = db.Categories
                .Include(c => c.Category1)
                .Include(c => c.Categories1)
                .Include(c => c.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string term = keyword.Trim();
                query = query.Where(c => c.Name.Contains(term));
                ViewBag.Keyword = term;
            }

            if (level == "root")
                query = query.Where(c => c.ParentId == null);
            else if (level == "child")
                query = query.Where(c => c.ParentId != null);

            if (status == "active")
                query = query.Where(c => c.IsActive);
            else if (status == "hidden")
                query = query.Where(c => !c.IsActive);

            ViewBag.Level = level;
            ViewBag.Status = status;

            var list = query
                .OrderBy(c => c.ParentId ?? c.CategoryId)
                .ThenBy(c => c.ParentId.HasValue)
                .ThenBy(c => c.CategoryId)
                .ToList();

            return View(list);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories
                .Include(c => c.Category1)
                .Include(c => c.Categories1)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();
            return View(category);
        }

        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.IsActive).OrderBy(c => c.CategoryId),
                "CategoryId", "Name");
            return View(new Category { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,Name,ParentId,IsActive")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên loại sản phẩm.");
            }
            else
            {
                string cleanName = category.Name.Trim();
                bool isDuplicate = db.Categories.Any(c =>
                    c.ParentId == category.ParentId &&
                    c.Name.ToLower() == cleanName.ToLower());

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "Tên loại sản phẩm này đã tồn tại trong cùng cấp danh mục.");
                }
            }

            if (category.ParentId.HasValue)
            {
                var parent = db.Categories.Find(category.ParentId.Value);
                if (parent == null || parent.ParentId != null)
                {
                    ModelState.AddModelError("ParentId", "Danh mục cha không hợp lệ (hệ thống chỉ hỗ trợ tối đa 2 cấp).");
                }
            }

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm loại sản phẩm mới.";
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.IsActive).OrderBy(c => c.CategoryId),
                "CategoryId", "Name", category.ParentId);
            return View(category);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories
                .Include(c => c.Categories1)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();

            ViewBag.HasChildren = category.Categories1 != null && category.Categories1.Any();
            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.CategoryId != category.CategoryId).OrderBy(c => c.CategoryId),
                "CategoryId", "Name", category.ParentId);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,Name,ParentId,IsActive")] Category category)
        {
            bool hasChildren = db.Categories.Any(c => c.ParentId == category.CategoryId);

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Vui lòng nhập tên loại sản phẩm.");
            }
            else
            {
                string cleanName = category.Name.Trim();
                bool isDuplicate = db.Categories.Any(c =>
                    c.CategoryId != category.CategoryId &&
                    c.ParentId == category.ParentId &&
                    c.Name.ToLower() == cleanName.ToLower());

                if (isDuplicate)
                {
                    ModelState.AddModelError("Name", "Tên loại sản phẩm này đã tồn tại trong cùng cấp danh mục.");
                }
            }

            if (category.ParentId == category.CategoryId)
            {
                ModelState.AddModelError("ParentId", "Danh mục cha không được trùng với chính nó.");
            }
            else if (hasChildren && category.ParentId.HasValue)
            {
                ModelState.AddModelError("ParentId", "Loại này đang có danh mục con nên phải giữ là danh mục gốc.");
            }

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật loại sản phẩm.";
                return RedirectToAction("Index");
            }

            ViewBag.HasChildren = hasChildren;
            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.CategoryId != category.CategoryId).OrderBy(c => c.CategoryId),
                "CategoryId", "Name", category.ParentId);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            category.IsActive = !category.IsActive;
            db.SaveChanges();
            TempData["Success"] = category.IsActive
                ? $"Đã bật hiển thị loại \"{category.Name}\"."
                : $"Đã ẩn loại \"{category.Name}\" khỏi cửa hàng.";

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories
                .Include(c => c.Category1)
                .Include(c => c.Categories1)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories
                .Include(c => c.Categories1)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();

            if (category.Categories1.Any())
            {
                TempData["Error"] = "Không thể xóa vì loại này đang có danh mục con.";
                return RedirectToAction("Index");
            }

            if (category.Products.Any())
            {
                TempData["Error"] = "Không thể xóa vì đang có sản phẩm thuộc loại này.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa loại sản phẩm.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
