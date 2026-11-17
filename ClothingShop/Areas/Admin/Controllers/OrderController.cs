using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class OrderController : AdminBaseController
    {
        private ShopDbContext db = new ShopDbContext();

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
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return HttpNotFound();
            return View(order);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();

            order.Status = status;
            db.SaveChanges();

            TempData["Success"] = $"Đã cập nhật trạng thái đơn #{id}.";
            return RedirectToAction("Details", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
