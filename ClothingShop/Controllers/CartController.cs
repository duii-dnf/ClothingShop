using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Web.Services;
using ClothingShop.Web.ViewModels;

namespace ClothingShop.Controllers
{
    public class CartController : Controller
    {
        private CartService _cartService = new CartService();

        public ActionResult Index()
        {
            var cart = _cartService.GetCart();
            Session["CartCount"] = cart.Sum(x => x.Quantity);
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ (Thích hợp gọi qua AJAX hoặc submit Form từ trang Chi tiết SP)

        [HttpPost]
        public ActionResult Add(int productId, string productName, string imageUrl, decimal price, int quantity = 1)
        {
            var item = new CartItemVM
            {
                ProductId = productId,
                ProductName = productName,
                ImageUrl = imageUrl,
                UnitPrice = price,
                Quantity = quantity
            };
            _cartService.AddItem(item);
            Session["CartCount"] = _cartService.GetCart().Sum(x => x.Quantity);

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = _cartService.GetCart().Sum(x => x.Quantity) });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            Session["CartCount"] = _cartService.GetCart().Sum(x => x.Quantity);

            if (Request.IsAjaxRequest())
            {
                var cart = _cartService.GetCart();
                var item = cart.Find(x => x.ProductId == productId);
                return Json(new
                {
                    success = true,
                    itemTotal = item != null ? item.TotalPrice.ToString("N0") + " VNĐ" : "0 VNĐ",
                    cartTotal = _cartService.GetCartTotal().ToString("N0") + " VNĐ",
                    cartCount = cart.Count
                });
            }

            return RedirectToAction("Index");
        }

        public ActionResult Remove(int productId)
        {
            _cartService.RemoveItem(productId);
            Session["CartCount"] = _cartService.GetCart().Sum(x => x.Quantity);
            return RedirectToAction("Index");
        }
    }
}