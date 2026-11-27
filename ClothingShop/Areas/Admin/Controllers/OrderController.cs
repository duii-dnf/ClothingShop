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

            if (!string.IsNullOrWhiteSpace(status))
                orders = orders.Where(o => o.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
                orders = orders.Where(o => o.ReceiverName.Contains(keyword.Trim()) || o.Phone.Contains(keyword.Trim()));

            ViewBag.Status = status;
            ViewBag.Keyword = keyword;
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
            order.Status = status;

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                order.PaymentStatus = paymentStatus;
                if (paymentStatus == "Paid" && !order.PaymentDate.HasValue)
                    order.PaymentDate = DateTime.Now;
            }
            else if (status == OrderStatus.Completed && order.PaymentStatus == "Unpaid")
            {
                order.PaymentStatus = "Paid";
                order.PaymentDate = DateTime.Now;
            }

            // Hoàn tồn kho nếu chuyển sang trạng thái Hủy
            if (status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
            {
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
                                Note = $"Hủy đơn #{order.OrderId} (hoàn tồn kho)",
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

            TempData["Success"] = $"Đã cập nhật trạng thái đơn #{id} thành '{status}'.";
            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
