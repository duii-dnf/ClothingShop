using System;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Filters;
using ClothingShop.Models;
using ClothingShop.Services;

namespace ClothingShop.Areas.Admin.Controllers
{
    [AdminAuthorize(Roles = AppRoles.Admin)]
    public class UserController : AdminBaseController
    {
        private ShopDbContext db = new ShopDbContext();

        // GET: Admin/User
        public ActionResult Index()
        {
            var users = db.Users.OrderBy(u => u.Role).ThenBy(u => u.UserId).ToList();
            return View(users);
        }

        // GET: Admin/User/Create
        public ActionResult Create()
        {
            return View(new AppUser { Role = AppRoles.Staff });
        }

        // POST: Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppUser model, string password)
        {
            ModelState.Remove("PasswordHash");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                ModelState.AddModelError("", "Mật khẩu phải từ 6 ký tự trở lên.");

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                string email = model.Email.Trim().ToLower();
                if (db.Users.Any(u => u.Email == email))
                    ModelState.AddModelError("Email", "Email này đã tồn tại.");
                model.Email = email;
            }

            if (model.Role != AppRoles.Admin && model.Role != AppRoles.Staff && model.Role != AppRoles.Customer)
                model.Role = AppRoles.Staff;

            if (ModelState.IsValid)
            {
                model.PasswordHash = PasswordHasher.Hash(password);
                model.CreatedAt = DateTime.Now;
                db.Users.Add(model);
                db.SaveChanges();
                TempData["Success"] = $"Đã tạo tài khoản {model.Email} ({model.Role}).";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Admin/User/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(int id, string role)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            var current = SessionHelper.GetCurrentUser(Session);
            if (current != null && current.UserId == user.UserId && role != AppRoles.Admin)
            {
                TempData["Error"] = "Không thể tự hạ quyền tài khoản Admin đang đăng nhập.";
                return RedirectToAction("Index");
            }

            if (role == AppRoles.Admin || role == AppRoles.Staff || role == AppRoles.Customer)
            {
                user.Role = role;
                db.SaveChanges();
                TempData["Success"] = $"Đã đổi vai trò của {user.Email} thành {role}.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
