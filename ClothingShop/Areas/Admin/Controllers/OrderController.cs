using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class OrderController : AdminBaseController
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        // GET: Admin/Order
        public ActionResult Index(string status, string keyword)
        {
            var orders = db.Orders.Include(o => o.User).Include(o => o.Coupon).AsQueryable();

            if (status == "CancelRequested")
            {
                orders = orders.Where(o => o.Status == OrderStatus.Pending
                                           && o.Note != null
                                           && o.Note.Contains("[YÊU CẦU HỦY"));
            }
            else if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword.Trim();
                orders = orders.Where(o => o.ReceiverName.Contains(kw) || o.Phone.Contains(kw));
            }

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
            ViewBag.CancelRequestCount = db.Orders.Count(o => o.Status == OrderStatus.Pending
                                                              && o.Note != null
                                                              && o.Note.Contains("[YÊU CẦU HỦY"));

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // GET: Admin/Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Coupon)
                .Include("OrderDetails.Product")
                .Include("OrderDetails.ProductVariant.ProductSize")
                .Include("OrderDetails.ProductVariant.ProductColor")
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return HttpNotFound();
            return View(order);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string paymentStatus = null)
        {
            var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderId == id);
            if (order == null) return HttpNotFound();

            string oldStatus = order.Status;
            string oldPaymentStatus = order.PaymentStatus;
            order.Status = status;

            // Cập nhật trạng thái thanh toán theo lựa chọn của Admin
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                order.PaymentStatus = paymentStatus;
                if (paymentStatus == "Paid" && !order.PaymentDate.HasValue)
                    order.PaymentDate = DateTime.Now;
            }

            // Đơn COD chuyển sang Hoàn thành -> tự động ghi nhận Đã thanh toán
            if (status == OrderStatus.Completed && order.PaymentStatus == "Unpaid")
            {
                order.PaymentStatus = "Paid";
                order.PaymentDate = DateTime.Now;
            }

            // Duyệt hủy đơn: hoàn tồn kho + hoàn lượt mã giảm giá + chuyển Refunded nếu đã thanh toán
            if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
                if (oldPaymentStatus == "Paid" && (string.IsNullOrEmpty(paymentStatus) || paymentStatus == "Paid"))
                {
                    order.PaymentStatus = "Refunded";
                }

                // Hoàn lại lượt sử dụng mã khuyến mãi (nếu đơn có áp mã)
                if (order.CouponId.HasValue)
                {
                    var coupon = db.Coupons.Find(order.CouponId.Value);
                    if (coupon != null && coupon.UsedCount > 0)
                        coupon.UsedCount -= 1;
                }

                var staff = SessionHelper.GetCurrentUser(Session);
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.ProductVariantId.HasValue)
                    {
                        var variant = db.ProductVariants.Find(detail.ProductVariantId.Value);
                        if (variant != null)
                        {
                            variant.Stock += detail.Quantity;
                            db.InventoryLogs.Add(new InventoryLog
                            {
                                ProductId = detail.ProductId,
                                VariantId = variant.VariantId,
                                ChangeType = "OrderCancelled",
                                QuantityChange = detail.Quantity,
                                StockAfter = variant.Stock,
                                ReferenceType = "AdminOrder",
                                ReferenceId = order.OrderId,
                                Note = $"Duyệt hủy đơn #{order.OrderId} (hoàn tồn kho)",
                                CreatedAt = DateTime.Now,
                                CreatedBy = staff != null ? staff.Email : "Admin"
                            });
                        }
                    }

                    var prod = db.Products.Find(detail.ProductId);
                    if (prod != null)
                        prod.Stock += detail.Quantity;
                }
            }

            db.SaveChanges();

            if (status == OrderStatus.Cancelled && order.PaymentStatus == "Refunded")
                TempData["Success"] = $"Đã duyệt hủy đơn #{id}, xác nhận hoàn tiền và hoàn lại tồn kho.";
            else if (status == OrderStatus.Cancelled)
                TempData["Success"] = $"Đã duyệt hủy đơn #{id} và hoàn lại tồn kho.";
            else
                TempData["Success"] = $"Đã cập nhật đơn #{id} thành '{status}' ({order.PaymentStatus}).";

            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}