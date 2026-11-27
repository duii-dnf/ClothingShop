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
                    decimal subTotal = _cartService.GetCartTotal();
                    decimal shippingFee = subTotal >= 500000m || subTotal == 0 ? 0m : 30000m;
                    decimal discountAmount = 0m;
                    int? couponId = null;

                    if (!string.IsNullOrWhiteSpace(model.CouponCode))
                    {
                        var check = EvaluateCoupon(model.CouponCode, subTotal);
                        if (!check.IsValid)
                        {
                            ModelState.AddModelError("CouponCode", check.Message);
                            ViewBag.CartItems = cart;
                            ViewBag.SubTotal = subTotal;
                            return View(model);
                        }
                        discountAmount = check.DiscountAmount;
                        couponId = check.Coupon.CouponId;
                        check.Coupon.UsedCount += 1;
                    }

                    decimal totalAmount = Math.Max(0m, subTotal + shippingFee - discountAmount);

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
                        Subtotal = subTotal,
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

        // Hàm kiểm tra điều kiện Coupon dùng chung cho cả AJAX và lúc bấm Đặt hàng
        private (bool IsValid, string Message, Coupon Coupon, decimal DiscountAmount) EvaluateCoupon(string couponCode, decimal subTotal)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return (false, "Vui lòng nhập mã giảm giá.", null, 0m);

            string normalizedCode = couponCode.Trim().ToUpper();
            var coupon = db.Coupons.FirstOrDefault(c => c.Code.ToUpper() == normalizedCode);

            if (coupon == null)
                return (false, $"Mã ưu đãi \"{normalizedCode}\" không tồn tại.", null, 0m);

            if (!coupon.IsActive)
                return (false, $"Mã ưu đãi \"{normalizedCode}\" hiện không khả dụng.", null, 0m);

            var now = DateTime.Now;
            if (now < coupon.StartDate)
                return (false, $"Mã \"{normalizedCode}\" chỉ áp dụng từ ngày {coupon.StartDate:dd/MM/yyyy}.", null, 0m);

            if (now > coupon.EndDate)
                return (false, $"Mã \"{normalizedCode}\" đã hết hạn vào ngày {coupon.EndDate:dd/MM/yyyy}.", null, 0m);

            if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
                return (false, $"Mã \"{normalizedCode}\" đã hết lượt sử dụng.", null, 0m);

            if (subTotal < coupon.MinOrder)
                return (false, $"Đơn hàng tối thiểu {coupon.MinOrder:N0} VNĐ để dùng mã \"{normalizedCode}\".", null, 0m);

            decimal discount = Math.Round(subTotal * coupon.DiscountPercent / 100m, 0);
            if (discount > subTotal) discount = subTotal;

            return (true, $"Áp dụng thành công mã {coupon.Code} (giảm {coupon.DiscountPercent}%).", coupon, discount);
        }

        // Action AJAX gọi khi bấm nút "ÁP DỤNG"
        [HttpPost]
        public JsonResult CheckCoupon(string couponCode)
        {
            var cart = _cartService.GetCart();
            decimal subTotal = _cartService.GetCartTotal();
            decimal shippingFee = subTotal > 500000m ? 0m : 30000m;

            var result = EvaluateCoupon(couponCode, subTotal);
            decimal finalTotal = Math.Max(0m, subTotal + shippingFee - result.DiscountAmount);

            return Json(new
            {
                success = result.IsValid,
                message = result.Message,
                code = result.Coupon?.Code,
                discountPercent = result.Coupon?.DiscountPercent ?? 0,
                discountAmount = result.DiscountAmount,
                finalTotal = finalTotal
            });
        }

        [HttpGet]
        public JsonResult GetCoupons()
        {
            decimal subTotal = _cartService.GetCartTotal();
            DateTime now = DateTime.Now;
            var allCoupons = db.Coupons.ToList();

            var usable = new System.Collections.Generic.List<object>();
            var unavailable = new System.Collections.Generic.List<object>();

            // Tính trước danh sách dùng được để tìm mã giảm nhiều nhất
            var usableEntities = allCoupons
                .Where(c => c.IsActive
                            && now >= c.StartDate
                            && now <= c.EndDate
                            && (c.UsageLimit <= 0 || c.UsedCount < c.UsageLimit)
                            && subTotal >= c.MinOrder)
                .OrderByDescending(c => Math.Round(subTotal * c.DiscountPercent / 100m, 0))
                .ThenBy(c => c.EndDate)
                .ToList();

            int? bestCouponId = usableEntities.Any() ? usableEntities.First().CouponId : (int?)null;

            foreach (var c in usableEntities)
            {
                decimal discount = Math.Round(subTotal * c.DiscountPercent / 100m, 0);
                if (discount > subTotal) discount = subTotal;

                int usedPercent = c.UsageLimit > 0
                    ? Math.Min(100, (int)Math.Round(c.UsedCount * 100.0 / c.UsageLimit))
                    : 0;

                usable.Add(new
                {
                    couponId = c.CouponId,
                    code = c.Code,
                    discountPercent = c.DiscountPercent,
                    discountAmount = discount,
                    minOrder = c.MinOrder,
                    endDate = c.EndDate.ToString("dd/MM/yyyy"),
                    usedPercent = usedPercent,
                    usageLimit = c.UsageLimit,
                    usedCount = c.UsedCount,
                    isBest = (c.CouponId == bestCouponId)
                });
            }

            var unavailableEntities = allCoupons
                .Where(c => !usableEntities.Any(u => u.CouponId == c.CouponId))
                .OrderByDescending(c => c.EndDate)
                .ToList();

            foreach (var c in unavailableEntities)
            {
                string reason;
                if (!c.IsActive)
                    reason = "Mã ưu đãi hiện tạm ngưng áp dụng.";
                else if (now > c.EndDate)
                    reason = $"Mã đã hết hạn vào ngày {c.EndDate:dd/MM/yyyy}.";
                else if (now < c.StartDate)
                    reason = $"Mã chỉ bắt đầu áp dụng từ ngày {c.StartDate:dd/MM/yyyy}.";
                else if (c.UsageLimit > 0 && c.UsedCount >= c.UsageLimit)
                    reason = "Mã đã hết lượt sử dụng.";
                else if (subTotal < c.MinOrder)
                    reason = $"Mua thêm {(c.MinOrder - subTotal):N0}đ để đạt đơn tối thiểu {c.MinOrder:N0}đ.";
                else
                    reason = "Đơn hàng chưa thỏa điều kiện áp dụng voucher.";

                int usedPercent = c.UsageLimit > 0
                    ? Math.Min(100, (int)Math.Round(c.UsedCount * 100.0 / c.UsageLimit))
                    : 0;

                unavailable.Add(new
                {
                    couponId = c.CouponId,
                    code = c.Code,
                    discountPercent = c.DiscountPercent,
                    minOrder = c.MinOrder,
                    endDate = c.EndDate.ToString("dd/MM/yyyy"),
                    usedPercent = usedPercent,
                    reason = reason
                });
            }

            return Json(new
            {
                subTotal = subTotal,
                usable = usable,
                unavailable = unavailable
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
