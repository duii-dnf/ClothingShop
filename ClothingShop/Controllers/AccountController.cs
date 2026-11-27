using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
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
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

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

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(model.Password),
                Gender = "Unisex",
                IsActive = true,
                Role = AppRoles.Customer,
                CreatedAt = DateTime.Now
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
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.IsActive);

            if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
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

            var favIds = db.Favorites
                .Where(f => f.UserId == user.UserId)
                .Select(f => f.ProductId)
                .ToList();
            Session["FavIds"] = favIds;
            Session["FavCount"] = favIds.Count;

            if (model.RememberMe)
            {
                Response.Cookies.Add(new HttpCookie(RememberCookie, HttpUtility.UrlEncode(user.Email))
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true
                });
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
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account") });

            var user = db.Users.Find(currentUser.UserId);
            if (user == null) return RedirectToAction("Login");

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
                Addresses = db.UserAddresses
                    .Where(a => a.UserId == user.UserId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToList(),
                RecentOrders = db.Orders
                    .Include("OrderDetails.Product")
                    .Where(o => o.UserId == user.UserId)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UserProfileVM model)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            var user = db.Users.Find(currentUser.UserId);
            if (user == null) return RedirectToAction("Login");

            string cleanPhone = model.Phone != null ? model.Phone.Trim() : null;
            if (!string.IsNullOrEmpty(cleanPhone) && !Regex.IsMatch(cleanPhone, @"^0\d{9}$"))
            {
                TempData["Error"] = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.";
                return RedirectToAction("Profile");
            }

            if (!string.IsNullOrWhiteSpace(model.FullName)) user.FullName = model.FullName.Trim();
            user.Phone = string.IsNullOrEmpty(cleanPhone) ? null : cleanPhone;
            user.Address = model.Address?.Trim();
            user.Gender = model.Gender;
            user.BirthDate = model.BirthDate;
            if (!string.IsNullOrWhiteSpace(model.Avatar)) user.Avatar = model.Avatar.Trim();

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || !PasswordHasher.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    TempData["Error"] = "Mật khẩu hiện tại không chính xác.";
                    return RedirectToAction("Profile");
                }
                if (model.NewPassword != model.ConfirmPassword)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                    return RedirectToAction("Profile");
                }
                user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            }

            db.SaveChanges();

            currentUser.FullName = user.FullName;
            currentUser.Phone = user.Phone;
            currentUser.Address = user.Address;
            currentUser.Gender = user.Gender;
            currentUser.Avatar = user.Avatar;
            SessionHelper.SignIn(Session, currentUser);

            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAddress(UserAddress address)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(address.ReceiverName) || string.IsNullOrWhiteSpace(address.ReceiverPhone) || string.IsNullOrWhiteSpace(address.DetailAddress))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ chi tiết.";
                return RedirectToAction("Profile");
            }

            address.ReceiverPhone = address.ReceiverPhone.Trim();
            if (!Regex.IsMatch(address.ReceiverPhone, @"^0\d{9}$"))
            {
                TempData["Error"] = "Số điện thoại nhận hàng phải gồm đúng 10 chữ số và bắt đầu bằng số 0.";
                return RedirectToAction("Profile");
            }

            address.UserId = currentUser.UserId;
            address.CreatedAt = DateTime.Now;

            bool isFirst = !db.UserAddresses.Any(a => a.UserId == currentUser.UserId);
            if (isFirst || address.IsDefault)
            {
                address.IsDefault = true;
                foreach (var ex in db.UserAddresses.Where(a => a.UserId == currentUser.UserId && a.IsDefault))
                    ex.IsDefault = false;
            }

            db.UserAddresses.Add(address);
            db.SaveChanges();

            TempData["Success"] = "Đã thêm địa chỉ mới.";
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

                if (wasDefault)
                {
                    var next = db.UserAddresses.FirstOrDefault(a => a.UserId == currentUser.UserId);
                    if (next != null) { next.IsDefault = true; db.SaveChanges(); }
                }
                TempData["Success"] = "Đã xóa địa chỉ.";
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetDefaultAddress(int addressId)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login");

            foreach (var a in db.UserAddresses.Where(x => x.UserId == currentUser.UserId))
                a.IsDefault = (a.AddressId == addressId);

            db.SaveChanges();
            TempData["Success"] = "Đã đặt làm địa chỉ mặc định.";
            return RedirectToAction("Profile");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
