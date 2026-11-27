using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;

namespace ClothingShop.Controllers
{
    public class FavoriteController : Controller
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        // GET: /Favorite
        public ActionResult Index()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Favorite") });

            var favProducts = db.Favorites
                .Where(f => f.UserId == currentUser.UserId && f.Product.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Product)
                .Include(p => p.Category)
                .ToList();

            var favIds = favProducts.Select(p => p.ProductId).ToList();
            Session["FavIds"] = favIds;
            Session["FavCount"] = favIds.Count;

            return View(favProducts);
        }

        // POST: /Favorite/Toggle (Form POST thuần, KHÔNG AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Toggle(int productId, string returnUrl = null)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để lưu sản phẩm yêu thích.";
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action("Details", "Product", new { id = productId }) });
            }

            var existing = db.Favorites.FirstOrDefault(f => f.UserId == currentUser.UserId && f.ProductId == productId);
            if (existing != null)
            {
                db.Favorites.Remove(existing);
                db.SaveChanges();
                TempData["Success"] = "Đã bỏ sản phẩm khỏi danh sách yêu thích.";
            }
            else
            {
                var prod = db.Products.Find(productId);
                if (prod != null && prod.IsActive)
                {
                    db.Favorites.Add(new Favorite
                    {
                        UserId = currentUser.UserId,
                        ProductId = productId,
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                    TempData["Success"] = $"Đã thêm \"{prod.Name}\" vào danh sách yêu thích!";
                }
            }

            var favIds = db.Favorites
                .Where(f => f.UserId == currentUser.UserId)
                .Select(f => f.ProductId)
                .ToList();
            Session["FavIds"] = favIds;
            Session["FavCount"] = favIds.Count;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
