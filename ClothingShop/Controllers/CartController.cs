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

        // 1. Hiển thị trang giỏ hàng
        public ActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ (Thích hợp gọi qua AJAX từ trang Chi tiết SP)
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

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = _cartService.GetCart().Count });
        }

        // 3. Cập nhật số lượng sản phẩm
        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }

        // 4. Xóa sản phẩm khỏi giỏ
        public ActionResult Remove(int productId)
        {
            _cartService.RemoveItem(productId);
            return RedirectToAction("Index");
        }
    }
}