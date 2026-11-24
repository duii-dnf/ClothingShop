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
                Gender = "Unisex",
                Role = AppRoles.Customer,
                CreatedAt = DateTime.Now
            };
            db.Users.Add(user);
            db.SaveChanges();

            // Create initial address in UserAddresses
            if (!string.IsNullOrEmpty(model.Address))
            {
                var initialAddress = new UserAddress
                {
                    UserId = user.UserId,
                    ReceiverName = user.FullName,
                    ReceiverPhone = user.Phone ?? "0987654321",
                    DetailAddress = model.Address,
                    IsDefault = true,
                    CreatedAt = DateTime.Now
                };
                db.UserAddresses.Add(initialAddress);
                db.SaveChanges();
            }

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
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.IsActive);

            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa");
                return View(model);
            }

            SessionHelper.SignIn(Session, new SessionUser
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Avatar = user.Avatar,
                Role = user.Role
            });

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

            if (user.Role == AppRoles.Staff)
                return RedirectToAction("Index", "Order", new { area = "Admin" });

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

        // ---------- HỒ SƠ CÁ NHÂN & SỔ ĐỊA CHỈ ----------
        [HttpGet]
        public new ActionResult Profile()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account") });
            }

            var user = db.Users.Find(currentUser.UserId);
            if (user == null) return RedirectToAction("Login");

            var addresses = db.UserAddresses
                .Where(a => a.UserId == user.UserId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            var recentOrders = db.Orders
                .Include("OrderDetails.Product")
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            var vm = new UserProfileVM
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender ?? "Nam",
                BirthDate = user.BirthDate,
                Avatar = user.Avatar,
                Addresses = addresses,
                RecentOrders = recentOrders
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UserProfileVM model)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            var user = db.Users.Find(currentUser.UserId);
            if (user == null) return RedirectToAction("Login");

            if (!string.IsNullOrEmpty(model.FullName)) user.FullName = model.FullName.Trim();
            user.Phone = model.Phone?.Trim();
            user.Address = model.Address?.Trim();
            user.Gender = model.Gender;
            user.BirthDate = model.BirthDate;
            if (!string.IsNullOrEmpty(model.Avatar)) user.Avatar = model.Avatar;

            // Password change check
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    TempData["Error"] = "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu.";
                    return RedirectToAction("Profile");
                }

                if (!PasswordHasher.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    TempData["Error"] = "Mật khẩu hiện tại không chính xác.";
                    return RedirectToAction("Profile");
                }

                user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            }

            db.SaveChanges();

            // Sync session
            currentUser.FullName = user.FullName;
            currentUser.Phone = user.Phone;
            currentUser.Address = user.Address;
            currentUser.Gender = user.Gender;
            currentUser.Avatar = user.Avatar;
            SessionHelper.SignIn(Session, currentUser);

            TempData["Success"] = "Cập nhật thông tin hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        // ---------- QUẢN LÝ SỔ ĐỊA CHỈ ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(UserAddress address)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(address.ReceiverName) || string.IsNullOrWhiteSpace(address.ReceiverPhone) || string.IsNullOrWhiteSpace(address.DetailAddress))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ tên người nhận, số điện thoại và địa chỉ chi tiết.";
                return RedirectToAction("Profile");
            }

            address.UserId = currentUser.UserId;
            address.CreatedAt = DateTime.Now;

            bool isFirst = !db.UserAddresses.Any(a => a.UserId == currentUser.UserId);
            if (isFirst || address.IsDefault)
            {
                address.IsDefault = true;
                // Clear default on other addresses
                var existingDefaults = db.UserAddresses.Where(a => a.UserId == currentUser.UserId && a.IsDefault).ToList();
                foreach (var ex in existingDefaults) ex.IsDefault = false;
            }

            db.UserAddresses.Add(address);
            db.SaveChanges();

            TempData["Success"] = "Đã thêm địa chỉ mới vào sổ địa chỉ!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAddress(int addressId)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            var addr = db.UserAddresses.FirstOrDefault(a => a.AddressId == addressId && a.UserId == currentUser.UserId);
            if (addr != null)
            {
                bool wasDefault = addr.IsDefault;
                db.UserAddresses.Remove(addr);
                db.SaveChanges();

                // If deleted default, promote another address to default
                if (wasDefault)
                {
                    var nextDefault = db.UserAddresses.FirstOrDefault(a => a.UserId == currentUser.UserId);
                    if (nextDefault != null)
                    {
                        nextDefault.IsDefault = true;
                        db.SaveChanges();
                    }
                }
                TempData["Success"] = "Đã xóa địa chỉ thành công!";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetDefaultAddress(int addressId)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            var addresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
            foreach (var a in addresses)
            {
                a.IsDefault = (a.AddressId == addressId);
            }
            db.SaveChanges();

            TempData["Success"] = "Đã đặt địa chỉ làm mặc định!";
            return RedirectToAction("Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
