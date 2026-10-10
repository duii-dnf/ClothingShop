using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class AccountController : Controller
    {
        private const string RememberCookie = "remember_email";
        private readonly ShopDbContext db = new ShopDbContext();

        // ---------- ĐĂNG KÝ ----------
        [HttpGet]
        public ActionResult Register()
        {
            if (SessionHelper.CurrentUser != null) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            if (db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký");
                return View(model);
            }

            var user = new AppUser
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(model.Password),
                Phone = model.Phone,
                Address = model.Address,
                Role = AppRoles.Customer
            };
            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ---------- ĐĂNG NHẬP ----------
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (SessionHelper.CurrentUser != null) return RedirectToAction("Index", "Home");

            var model = new LoginViewModel();
            var cookie = Request.Cookies[RememberCookie];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                model.Email = HttpUtility.UrlDecode(cookie.Value);
                model.RememberMe = true;
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            // Thông báo chung, không tiết lộ email có tồn tại hay không
            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            SessionHelper.SignIn(Session, new SessionUser
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            });

            // Cookie: chỉ nhớ email (không lưu mật khẩu)
            if (model.RememberMe)
            {
                var cookie = new HttpCookie(RememberCookie, HttpUtility.UrlEncode(user.Email))
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true
                };
                Response.Cookies.Add(cookie);
            }
            else if (Request.Cookies[RememberCookie] != null)
            {
                Response.Cookies.Add(new HttpCookie(RememberCookie) { Expires = DateTime.Now.AddDays(-1) });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role == AppRoles.Admin)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }

        // ---------- ĐĂNG XUẤT ----------
        public ActionResult Logout()
        {
            SessionHelper.SignOut(Session);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}