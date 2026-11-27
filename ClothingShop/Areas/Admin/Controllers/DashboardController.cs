using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Filters;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    [AdminAuthorize(Roles = AppRoles.Admin)]
    public class DashboardController : AdminBaseController
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        // GET: Admin/Dashboard
        public ActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            var orders = db.Orders.AsQueryable();

            if (fromDate.HasValue)
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1);
                orders = orders.Where(o => o.OrderDate < end);
            }

            // Doanh thu chỉ tính đơn đã Hoàn thành (Completed)
            ViewBag.TongDoanhThu = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.TongDonHang = orders.Count();
            ViewBag.DonChoXacNhan = orders.Count(o => o.Status == OrderStatus.Pending);
            ViewBag.DonDaXacNhan = orders.Count(o => o.Status == OrderStatus.Confirmed);
            ViewBag.DonDangGiao = orders.Count(o => o.Status == OrderStatus.Shipping);
            ViewBag.DonHoanThanh = orders.Count(o => o.Status == OrderStatus.Completed);
            ViewBag.DonDaHuy = orders.Count(o => o.Status == OrderStatus.Cancelled);

            ViewBag.TongSanPham = db.Products.Count();
            ViewBag.TongKhachHang = db.Users.Count(u => u.Role == AppRoles.Customer);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            var recentOrders = orders.Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            return View(recentOrders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
