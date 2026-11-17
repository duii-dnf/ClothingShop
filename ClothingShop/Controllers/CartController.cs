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
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

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

        [HttpPost]
        public ActionResult Update(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);

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
            return RedirectToAction("Index");
        }
    }
}