using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClothingShop.Filters;
using ClothingShop.Models;

namespace ClothingShop.Areas.Admin.Controllers
{
    [AdminAuthorize(Roles = AppRoles.Admin)]
    public class CouponController : AdminBaseController
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        // GET: Admin/Coupon
        public ActionResult Index()
        {
            return View(db.Coupons.OrderByDescending(c => c.CouponId).ToList());
        }

        // GET: Admin/Coupon/Create
        public ActionResult Create()
        {
            return View(new Coupon
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                DiscountPercent = 10,
                UsageLimit = 50,
                IsActive = true
            });
        }

        // POST: Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Coupon coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
                ModelState.AddModelError("Code", "Vui lòng nhập mã khuyến mãi.");
            else
            {
                coupon.Code = coupon.Code.Trim().ToUpper();
                if (db.Coupons.Any(c => c.Code == coupon.Code))
                    ModelState.AddModelError("Code", "Mã khuyến mãi này đã tồn tại.");
            }

            if (coupon.DiscountPercent < 1 || coupon.DiscountPercent > 100)
                ModelState.AddModelError("DiscountPercent", "Phần trăm giảm giá phải từ 1% đến 100%.");

            if (coupon.MinOrder < 0)
                ModelState.AddModelError("MinOrder", "Đơn tối thiểu không được âm.");

            if (coupon.UsageLimit < 1)
                ModelState.AddModelError("UsageLimit", "Giới hạn lượt dùng phải lớn hơn 0.");

            if (coupon.EndDate < coupon.StartDate)
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            if (ModelState.IsValid)
            {
                db.Coupons.Add(coupon);
                db.SaveChanges();
                TempData["Success"] = "Thêm mã khuyến mãi thành công.";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();
            return View(coupon);
        }

        // POST: Admin/Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Coupon coupon)
        {
            if (string.IsNullOrWhiteSpace(coupon.Code))
                ModelState.AddModelError("Code", "Vui lòng nhập mã khuyến mãi.");
            else
            {
                coupon.Code = coupon.Code.Trim().ToUpper();
                if (db.Coupons.Any(c => c.Code == coupon.Code && c.CouponId != coupon.CouponId))
                    ModelState.AddModelError("Code", "Mã khuyến mãi này đã tồn tại.");
            }

            if (coupon.DiscountPercent < 1 || coupon.DiscountPercent > 100)
                ModelState.AddModelError("DiscountPercent", "Phần trăm giảm giá phải từ 1% đến 100%.");

            if (coupon.MinOrder < 0)
                ModelState.AddModelError("MinOrder", "Đơn tối thiểu không được âm.");

            if (coupon.UsageLimit < 1)
                ModelState.AddModelError("UsageLimit", "Giới hạn lượt dùng phải lớn hơn 0.");

            if (coupon.EndDate < coupon.StartDate)
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            if (ModelState.IsValid)
            {
                db.Entry(coupon).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật mã khuyến mãi thành công.";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();
            return View(coupon);
        }

        // POST: Admin/Coupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();

            if (db.Orders.Any(o => o.CouponId == id))
            {
                TempData["Error"] = "Mã khuyến mãi đã được dùng trong đơn hàng, không thể xóa.";
                return RedirectToAction("Index");
            }

            db.Coupons.Remove(coupon);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa mã khuyến mãi.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
