using System;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Web.Services;
using ClothingShop.Web.ViewModels;
using ClothingShop.Models; // Sử dụng đúng namespace chứa entity Models
using ClothingShop.Services;

namespace ClothingShop.Controllers
{
    public class OrderController : Controller
    {
        private CartService _cartService = new CartService();
        private ShopDbContext db = new ShopDbContext(); // Đã mở khóa kết nối DB

        // 1. Trang Checkout (GET)
        public ActionResult Checkout()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            var cart = _cartService.GetCart();
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.CartItems = cart;
            ViewBag.SubTotal = _cartService.GetCartTotal();

            return View(new CheckoutVM());
        }

        // 2. Xử lý đặt hàng (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            var cart = _cartService.GetCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            if (ModelState.IsValid)
            {
                decimal totalAmount = _cartService.GetCartTotal();
                decimal discountAmount = 0;
                int? couponId = null;

                // Xử lý Coupon: Truy xuất từ bảng Coupons (nếu người dùng có nhập mã)
                if (!string.IsNullOrEmpty(model.CouponCode))
                {
                    var coupon = db.Coupons.FirstOrDefault(c => c.Code == model.CouponCode && c.IsActive);
                    if (coupon != null)
                    {
                        // Giả sử Coupon có trường DiscountPercent
                        discountAmount = totalAmount * (coupon.DiscountPercent / 100m);
                        couponId = coupon.CouponId;
                    }
                }

                totalAmount -= discountAmount;

                // Khởi tạo Order (không tự gán OrderId vì PK tự tăng)
                var order = new Order
                {
                    UserId = currentUser.UserId,
                    OrderDate = DateTime.Now,
                    ReceiverName = model.ReceiverName,
                    Phone = model.Phone,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod ?? "COD",
                    Status = "Pending",
                    CouponId = couponId,
                    DiscountAmount = discountAmount,
                    TotalAmount = totalAmount
                };

                db.Orders.Add(order);

                // Khởi tạo OrderDetails
                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                        // Color & Size sẽ xử lý thêm nếu bạn bổ sung chúng vào CartItemVM sau này
                    };

                    // Thêm thông qua Navigation property, Entity Framework sẽ tự ánh xạ OrderId
                    order.OrderDetails.Add(detail);
                }

                // Lưu tất cả vào Database cùng 1 lượt transaction
                db.SaveChanges();

                // Dọn Session giỏ hàng
                _cartService.ClearCart();
                Session[SessionHelper.CartCountKey] = 0;
                Session["ShoppingCart"] = null;
                return RedirectToAction("Success", new { id = order.OrderId }); // Truyền OrderId thật cho View Success
            }

            // Trả về view nếu nhập thiếu Form
            ViewBag.CartItems = cart;
            ViewBag.SubTotal = _cartService.GetCartTotal();
            return View(model);
        }

        // 3. Trang thông báo thành công
        public ActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // 4. Lịch sử đơn hàng của tôi
        public ActionResult History()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Order") });
            }

            var orders = db.Orders
                .Where(o => o.UserId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}