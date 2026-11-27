using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;
using ClothingShop.Web.Services;
using ClothingShop.Web.ViewModels;

namespace ClothingShop.Controllers
{
    public class OrderController : Controller
    {
        private CartService _cartService = new CartService();
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ActionResult Checkout()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });

            var cart = _cartService.GetCart();
            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            PopulateCheckoutViewBag(cart);

            var savedAddresses = db.UserAddresses
                .Where(a => a.UserId == currentUser.UserId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            var defaultAddr = savedAddresses.FirstOrDefault(a => a.IsDefault) ?? savedAddresses.FirstOrDefault();

            var model = new CheckoutVM
            {
                SavedAddresses = savedAddresses,
                SelectedAddressId = defaultAddr?.AddressId,
                ReceiverName = defaultAddr?.ReceiverName ?? currentUser.FullName,
                Phone = defaultAddr?.ReceiverPhone ?? currentUser.Phone,
                Province = defaultAddr?.Province,
                District = defaultAddr?.District,
                Ward = defaultAddr?.Ward,
                Address = defaultAddr?.DetailAddress ?? currentUser.Address ?? "",
                Note = defaultAddr?.Notes ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });

            var cart = _cartService.GetCart();
            if (cart == null || cart.Count == 0)
                return RedirectToAction("Index", "Cart");

            if (model.SelectedAddressId.HasValue && model.SelectedAddressId.Value > 0)
            {
                var addr = db.UserAddresses.FirstOrDefault(a => a.AddressId == model.SelectedAddressId.Value && a.UserId == currentUser.UserId);
                if (addr != null)
                {
                    model.ReceiverName = addr.ReceiverName;
                    model.Phone = addr.ReceiverPhone;
                    model.Province = addr.Province;
                    model.District = addr.District;
                    model.Ward = addr.Ward;
                    model.Address = addr.DetailAddress;
                    ModelState.Remove("ReceiverName");
                    ModelState.Remove("Phone");
                    ModelState.Remove("Address");
                }
            }

            // Kiểm tra tồn kho
            foreach (var item in cart)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var v = db.ProductVariants.Find(item.ProductVariantId.Value);
                    if (v == null || !v.IsActive || v.Stock < item.Quantity)
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' ({item.Size} - {item.Color}) không đủ tồn kho.");
                }
                else
                {
                    var p = db.Products.Find(item.ProductId);
                    if (p == null || !p.IsActive || p.Stock < item.Quantity)
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' không đủ tồn kho.");
                }
            }

            // Kiểm tra mã khuyến mãi (nếu có nhập)
            decimal subtotal = _cartService.GetCartTotal();
            decimal shippingFee = subtotal >= 500000m || subtotal == 0 ? 0m : 30000m;
            decimal discountAmount = 0m;
            Coupon validCoupon = null;

            if (!string.IsNullOrWhiteSpace(model.CouponCode))
            {
                string code = model.CouponCode.Trim().ToUpper();
                var today = DateTime.Today;
                var coupon = db.Coupons.FirstOrDefault(c => c.Code == code && c.IsActive);
                if (coupon == null)
                    ModelState.AddModelError("CouponCode", "Mã giảm giá không tồn tại hoặc đã bị khóa.");
                else if (today < coupon.StartDate.Date || today > coupon.EndDate.Date)
                    ModelState.AddModelError("CouponCode", "Mã giảm giá chưa đến hạn hoặc đã hết hạn.");
                else if (coupon.UsedCount >= coupon.UsageLimit)
                    ModelState.AddModelError("CouponCode", "Mã giảm giá đã hết lượt sử dụng.");
                else if (subtotal < coupon.MinOrder)
                    ModelState.AddModelError("CouponCode", $"Đơn hàng tối thiểu {PriceFormatter.Vnd(coupon.MinOrder)} để dùng mã này.");
                else
                {
                    validCoupon = coupon;
                    discountAmount = Math.Round(subtotal * (coupon.DiscountPercent / 100m), 0);
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateCheckoutViewBag(cart);
                model.SavedAddresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
                return View(model);
            }

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    decimal totalAmount = Math.Max(0m, subtotal + shippingFee - discountAmount);
                    bool isOnline = model.PaymentMethod == PaymentMethods.BankTransfer || model.PaymentMethod == "Online";

                    var fullAddress = string.Join(", ", new[] { model.Address, model.Ward, model.District, model.Province }
                        .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));

                    var order = new Order
                    {
                        UserId = currentUser.UserId,
                        OrderDate = DateTime.Now,
                        PaymentDate = isOnline ? (DateTime?)DateTime.Now : null,
                        PaymentStatus = isOnline ? "Paid" : "Unpaid",
                        ReceiverName = model.ReceiverName.Trim(),
                        Phone = model.Phone.Trim(),
                        Address = fullAddress,
                        PaymentMethod = model.PaymentMethod ?? PaymentMethods.COD,
                        Status = OrderStatus.Pending,
                        Note = model.Note,
                        CouponId = validCoupon?.CouponId,
                        Subtotal = subtotal,
                        ShippingFee = shippingFee,
                        DiscountAmount = discountAmount,
                        TotalAmount = totalAmount
                    };

                    db.Orders.Add(order);
                    db.SaveChanges();

                    if (validCoupon != null)
                        validCoupon.UsedCount += 1;

                    foreach (var item in cart)
                    {
                        db.OrderDetails.Add(new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            ProductVariantId = item.ProductVariantId,
                            Size = item.Size,
                            Color = item.Color,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });

                        if (item.ProductVariantId.HasValue)
                        {
                            var v = db.ProductVariants.Find(item.ProductVariantId.Value);
                            if (v != null)
                            {
                                v.Stock = Math.Max(0, v.Stock - item.Quantity);
                                db.InventoryLogs.Add(new InventoryLog
                                {
                                    ProductId = item.ProductId,
                                    VariantId = v.VariantId,
                                    ChangeType = "OrderPlaced",
                                    QuantityChange = -item.Quantity,
                                    StockAfter = v.Stock,
                                    ReferenceType = "Order",
                                    ReferenceId = order.OrderId,
                                    Note = $"Đặt hàng #{order.OrderId}",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = currentUser.FullName
                                });
                            }
                        }

                        var prod = db.Products.Find(item.ProductId);
                        if (prod != null)
                            prod.Stock = Math.Max(0, prod.Stock - item.Quantity);
                    }

                    if (model.SaveToAddressBook && (!model.SelectedAddressId.HasValue || model.SelectedAddressId.Value == 0))
                    {
                        db.UserAddresses.Add(new UserAddress
                        {
                            UserId = currentUser.UserId,
                            ReceiverName = model.ReceiverName.Trim(),
                            ReceiverPhone = model.Phone.Trim(),
                            Province = model.Province,
                            District = model.District,
                            Ward = model.Ward,
                            DetailAddress = model.Address.Trim(),
                            Notes = model.Note,
                            IsDefault = !db.UserAddresses.Any(a => a.UserId == currentUser.UserId),
                            CreatedAt = DateTime.Now
                        });
                    }

                    db.SaveChanges();
                    tx.Commit();

                    _cartService.ClearCart();
                    Session[SessionHelper.CartCountKey] = 0;

                    TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng #{order.OrderId}.";
                    return RedirectToAction("Success", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    ModelState.AddModelError("", "Có lỗi khi đặt hàng: " + ex.Message);
                    PopulateCheckoutViewBag(cart);
                    model.SavedAddresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
                    return View(model);
                }
            }
        }

        public ActionResult Success(int id)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            var order = db.Orders.FirstOrDefault(o => o.OrderId == id);
            if (order != null && currentUser != null && order.UserId != currentUser.UserId && !currentUser.CanAccessAdmin)
                return RedirectToAction("Index", "Home");

            ViewBag.OrderId = id;
            return View(order);
        }

        public ActionResult History()
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Order") });

            var orders = db.Orders
                .Include("OrderDetails.Product")
                .Where(o => o.UserId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int orderId, string reason = null)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var order = db.Orders.Include("OrderDetails")
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUser.UserId);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng đang ở trạng thái Chờ xác nhận.";
                return RedirectToAction("History");
            }

            order.Status = OrderStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(reason))
                order.Note = (order.Note + " [Lý do hủy: " + reason.Trim() + "]").Trim();

            foreach (var d in order.OrderDetails)
            {
                if (d.ProductVariantId.HasValue)
                {
                    var v = db.ProductVariants.Find(d.ProductVariantId.Value);
                    if (v != null)
                    {
                        v.Stock += d.Quantity;
                        db.InventoryLogs.Add(new InventoryLog
                        {
                            ProductId = d.ProductId,
                            VariantId = v.VariantId,
                            ChangeType = "OrderCancelled",
                            QuantityChange = d.Quantity,
                            StockAfter = v.Stock,
                            ReferenceType = "Order",
                            ReferenceId = order.OrderId,
                            Note = $"Khách hủy đơn #{order.OrderId}",
                            CreatedAt = DateTime.Now,
                            CreatedBy = currentUser.FullName
                        });
                    }
                }
                var prod = db.Products.Find(d.ProductId);
                if (prod != null) prod.Stock += d.Quantity;
            }

            db.SaveChanges();
            TempData["Success"] = $"Đã hủy đơn hàng #{order.OrderId} và hoàn lại tồn kho.";
            return RedirectToAction("History");
        }

        private void PopulateCheckoutViewBag(List<CartItemVM> cart)
        {
            decimal subtotal = _cartService.GetCartTotal();
            decimal shippingFee = subtotal >= 500000m || subtotal == 0 ? 0m : 30000m;
            ViewBag.CartItems = cart;
            ViewBag.SubTotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subtotal + shippingFee;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
