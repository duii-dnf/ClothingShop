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
        private ShopDbContext db = new ShopDbContext();

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

            decimal subtotal = _cartService.GetCartTotal();
            decimal shippingFee = subtotal >= 500000m || subtotal == 0 ? 0m : 30000m;

            ViewBag.CartItems = cart;
            ViewBag.SubTotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subtotal + shippingFee;

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
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // If user selected an existing address from address book, populate fields
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
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateCheckoutViewBag(cart);
                model.SavedAddresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
                return View(model);
            }

            // Real-time stock validation for all cart items
            foreach (var item in cart)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = db.ProductVariants.Find(item.ProductVariantId.Value);
                    if (variant == null || !variant.IsActive)
                    {
                        ModelState.AddModelError("", $"Biến thể của sản phẩm '{item.ProductName}' không còn tồn tại.");
                    }
                    else if (variant.Stock < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName} ({item.Size} - {item.Color})' chỉ còn {variant.Stock} sản phẩm trong kho (bạn đặt {item.Quantity}).");
                    }
                }
                else
                {
                    var prod = db.Products.Find(item.ProductId);
                    if (prod == null || !prod.IsActive)
                    {
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' không còn kinh doanh.");
                    }
                    else if (prod.Stock < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' chỉ còn {prod.Stock} trong kho.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateCheckoutViewBag(cart);
                model.SavedAddresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
                return View(model);
            }

            // Begin DB transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal subtotal = _cartService.GetCartTotal();
                    decimal shippingFee = subtotal >= 500000m || subtotal == 0 ? 0m : 30000m;
                    decimal discountAmount = 0m;
                    int? couponId = null;

                    if (!string.IsNullOrEmpty(model.CouponCode))
                    {
                        var coupon = db.Coupons.FirstOrDefault(c => c.Code == model.CouponCode && c.IsActive);
                        if (coupon != null)
                        {
                            discountAmount = subtotal * (coupon.DiscountPercent / 100m);
                            couponId = coupon.CouponId;
                        }
                    }

                    decimal totalAmount = Math.Max(0m, subtotal + shippingFee - discountAmount);

                    bool isOnlinePayment = model.PaymentMethod == "Online" || model.PaymentMethod == "VNPay" || model.PaymentMethod == "Momo";
                    string paymentStatus = isOnlinePayment ? "Paid" : "Unpaid";
                    DateTime? paymentDate = isOnlinePayment ? (DateTime?)DateTime.Now : null;

                    var order = new Order
                    {
                        UserId = currentUser.UserId,
                        OrderDate = DateTime.Now,
                        PaymentDate = paymentDate,
                        PaymentStatus = paymentStatus,
                        ReceiverName = model.ReceiverName.Trim(),
                        Phone = model.Phone.Trim(),
                        Address = model.Address.Trim(),
                        PaymentMethod = model.PaymentMethod ?? "COD",
                        Status = OrderStatus.Pending,
                        Note = model.Note,
                        CouponId = couponId,
                        Subtotal = subtotal,
                        ShippingFee = shippingFee,
                        DiscountAmount = discountAmount,
                        TotalAmount = totalAmount
                    };

                    db.Orders.Add(order);
                    db.SaveChanges(); // Get generated OrderId

                    // Process details and deduct stock
                    foreach (var item in cart)
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = item.ProductId,
                            ProductVariantId = item.ProductVariantId,
                            Size = item.Size,
                            Color = item.Color,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };
                        db.OrderDetails.Add(detail);

                        // Deduct variant stock and log inventory
                        if (item.ProductVariantId.HasValue)
                        {
                            var variant = db.ProductVariants.Find(item.ProductVariantId.Value);
                            if (variant != null)
                            {
                                variant.Stock = Math.Max(0, variant.Stock - item.Quantity);

                                var invLog = new InventoryLog
                                {
                                    ProductId = item.ProductId,
                                    VariantId = variant.VariantId,
                                    ChangeType = "OrderPlaced",
                                    QuantityChange = -item.Quantity,
                                    StockAfter = variant.Stock,
                                    ReferenceType = "Order",
                                    ReferenceId = order.OrderId,
                                    Note = $"Đặt hàng #{order.OrderId} ({item.ProductName} - {item.Size} - {item.Color})",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = currentUser.FullName
                                };
                                db.InventoryLogs.Add(invLog);
                            }
                        }

                        // Deduct overall product stock
                        var prod = db.Products.Find(item.ProductId);
                        if (prod != null)
                        {
                            prod.Stock = Math.Max(0, prod.Stock - item.Quantity);
                        }
                    }

                    // Save address to user's address book if requested
                    if (model.SaveToAddressBook && (!model.SelectedAddressId.HasValue || model.SelectedAddressId.Value == 0))
                    {
                        var userAddr = new UserAddress
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
                        };
                        db.UserAddresses.Add(userAddr);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    // Clear cart & session
                    _cartService.ClearCart();
                    Session[SessionHelper.CartCountKey] = 0;
                    Session["CartCount"] = 0;
                    Session["ShoppingCart"] = null;

                    TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{order.OrderId}.";
                    return RedirectToAction("Success", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo đơn hàng: " + ex.Message);
                    PopulateCheckoutViewBag(cart);
                    model.SavedAddresses = db.UserAddresses.Where(a => a.UserId == currentUser.UserId).ToList();
                    return View(model);
                }
            }
        }

        // 3. Trang thông báo thành công
        public ActionResult Success(int id)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            var order = db.Orders.FirstOrDefault(o => o.OrderId == id);

            if (order != null && currentUser != null && order.UserId != currentUser.UserId && !currentUser.CanAccessAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.OrderId = id;
            return View(order);
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
                .Include("OrderDetails.Product")
                .Include("OrderDetails.Variant.Size")
                .Include("OrderDetails.Variant.Color")
                .Where(o => o.UserId == currentUser.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // 5. Hủy đơn hàng và hoàn tồn kho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int orderId, string reason = null)
        {
            var currentUser = SessionHelper.GetCurrentUser(Session);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include("OrderDetails")
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId == currentUser.UserId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng cần hủy.";
                return RedirectToAction("History");
            }

            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = $"Không thể hủy đơn hàng đang ở trạng thái '{order.Status}'.";
                return RedirectToAction("History");
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    order.Status = OrderStatus.Cancelled;
                    if (!string.IsNullOrEmpty(reason))
                    {
                        order.Note = (order.Note + " [Lý do hủy: " + reason + "]").Trim();
                    }

                    // Restore inventory for each item
                    foreach (var detail in order.OrderDetails)
                    {
                        if (detail.ProductVariantId.HasValue)
                        {
                            var variant = db.ProductVariants.Find(detail.ProductVariantId.Value);
                            if (variant != null)
                            {
                                variant.Stock += detail.Quantity;

                                var invLog = new InventoryLog
                                {
                                    ProductId = detail.ProductId,
                                    VariantId = variant.VariantId,
                                    ChangeType = "OrderCancelled",
                                    QuantityChange = detail.Quantity,
                                    StockAfter = variant.Stock,
                                    ReferenceType = "Order",
                                    ReferenceId = order.OrderId,
                                    Note = $"Hoàn tồn kho do hủy đơn hàng #{order.OrderId}",
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = currentUser.FullName
                                };
                                db.InventoryLogs.Add(invLog);
                            }
                        }

                        var prod = db.Products.Find(detail.ProductId);
                        if (prod != null)
                        {
                            prod.Stock += detail.Quantity;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = $"Đơn hàng #{order.OrderId} đã được hủy thành công và số lượng tồn kho đã được hoàn lại!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
                }
            }

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
