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
        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string paymentStatus = null)
        {
            var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderId == id);
            if (order == null) return HttpNotFound();

            string oldStatus = order.Status;
            string oldPaymentStatus = order.PaymentStatus;

            // 1. Khóa đơn đã kết thúc (tránh sửa ngược làm lệch tồn kho & doanh thu)
            if (oldStatus == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Đơn hàng đã bị hủy, không thể thay đổi trạng thái nữa.";
                return RedirectToAction("Details", new { id });
            }
            if (oldStatus == OrderStatus.Completed)
            {
                TempData["Error"] = "Đơn hàng đã hoàn thành, không thể chuyển về trạng thái khác.";
                return RedirectToAction("Details", new { id });
            }

            // 2. Chặn các tổ hợp hoàn tiền vô lý
            if (paymentStatus == "Refunded")
            {
                if (status != OrderStatus.Cancelled)
                {
                    TempData["Error"] = "Chỉ được chọn 'Đã hoàn tiền' khi trạng thái đơn hàng là Đã hủy (Cancelled).";
                    return RedirectToAction("Details", new { id });
                }
                if (oldPaymentStatus == "Unpaid")
                {
                    TempData["Error"] = "Đơn hàng chưa thanh toán nên không thể chuyển sang Đã hoàn tiền.";
                    return RedirectToAction("Details", new { id });
                }
            }

            order.Status = status;

            // 3. Xử lý nhánh Hủy đơn
            if (status == OrderStatus.Cancelled)
            {
                // Đơn đã trả tiền bắt buộc sang Refunded; đơn chưa trả tiền giữ nguyên Unpaid
                order.PaymentStatus = (oldPaymentStatus == "Paid") ? "Refunded" : "Unpaid";

                // Hoàn lượt sử dụng mã giảm giá (nếu có)
                if (order.CouponId.HasValue)
                {
                    var coupon = db.Coupons.Find(order.CouponId.Value);
                    if (coupon != null && coupon.UsedCount > 0)
                        coupon.UsedCount -= 1;
                }

                // Hoàn tồn kho & ghi log
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
            else
            {
                // 4. Các trạng thái đang xử lý / hoàn thành
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