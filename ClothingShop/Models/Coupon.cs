using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ClothingShop.Models
{
    public class Coupon
    {
        public int CouponId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã")]
        [StringLength(30)]
        [Index(IsUnique = true)]
        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; }

        [Range(1, 100, ErrorMessage = "Phần trăm giảm từ 1 đến 100")]
        [Display(Name = "Giảm (%)")]
        public int DiscountPercent { get; set; }

        [Display(Name = "Từ ngày")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Đến ngày")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Đơn tối thiểu")]
        public decimal MinOrder { get; set; }

        [Display(Name = "Số lượt tối đa")]
        public int UsageLimit { get; set; }

        [Display(Name = "Đã dùng")]
        public int UsedCount { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}