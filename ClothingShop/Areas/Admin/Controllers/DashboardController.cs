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

            // Doanh thu thực thu: chỉ tính đơn Hoàn thành & Đã thanh toán (không dính đơn hoàn tiền)
            ViewBag.TongDoanhThu = orders
                .Where(o => o.Status == OrderStatus.Completed && o.PaymentStatus == "Paid")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;

            // Thống kê hoàn tiền (đơn chuyển khoản đã duyệt hủy)
            ViewBag.TongTienDaHoan = orders
                .Where(o => o.Status == OrderStatus.Cancelled && o.PaymentStatus == "Refunded")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            ViewBag.DonDaHoanTien = orders
                .Count(o => o.Status == OrderStatus.Cancelled && o.PaymentStatus == "Refunded");

            ViewBag.TongDonHang = orders.Count();

            // Tách riêng đơn Chờ xác nhận thường và đơn Yêu cầu hủy để tổng 6 ô khớp 100% với Tổng đơn hàng
            ViewBag.DonChoXacNhan = orders.Count(o => o.Status == OrderStatus.Pending
                                                      && (o.Note == null || !o.Note.Contains("[YÊU CẦU HỦY")));
            ViewBag.DonYeuCauHuy = orders.Count(o => o.Status == OrderStatus.Pending
                                                     && o.Note != null
                                                     && o.Note.Contains("[YÊU CẦU HỦY"));
            ViewBag.DonDaXacNhan = orders.Count(o => o.Status == OrderStatus.Confirmed);
            ViewBag.DonDangGiao = orders.Count(o => o.Status == OrderStatus.Shipping);
            ViewBag.DonHoanThanh = orders.Count(o => o.Status == OrderStatus.Completed);
            ViewBag.DonDaHuy = orders.Count(o => o.Status == OrderStatus.Cancelled);

            ViewBag.TongSanPham = db.Products.Count(p => p.IsActive);
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