using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ShopDbContext db = new ShopDbContext();

        public ActionResult Index(string keyword)
        {
            var query = db.Categories.Include(c => c.Parent).Include(c => c.Products).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string term = keyword.Trim();
                query = query.Where(c => c.Name.Contains(term));
                ViewBag.Keyword = term;
            }

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
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();
            return View(category);
        }

        public ActionResult Create()
        {
            ViewBag.ParentId = new SelectList(db.Categories.Where(c => c.ParentId == null && c.IsActive), "CategoryId", "Name");
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

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm loại sản phẩm mới.";
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Categories.Where(c => c.ParentId == null && c.IsActive), "CategoryId", "Name", category.ParentId);
            return View(category);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.CategoryId != category.CategoryId),
                "CategoryId", "Name", category.ParentId);

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,Name,ParentId,IsActive")] Category category)
        {
            if (category.ParentId == category.CategoryId)
            {
                ModelState.AddModelError("ParentId", "Danh mục cha không được trùng với chính nó.");
            }

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật loại sản phẩm.";
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(
                db.Categories.Where(c => c.ParentId == null && c.CategoryId != category.CategoryId),
                "CategoryId", "Name", category.ParentId);

            return View(category);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
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
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return HttpNotFound();

            if (category.Children.Any())
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