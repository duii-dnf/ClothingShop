using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int OrderId { get; set; }

        public int UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; }

        [StringLength(30)]
        [Display(Name = "Thanh toán")]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = OrderStatus.Pending;

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        public int? CouponId { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        public virtual AppUser User { get; set; }
        public virtual Coupon Coupon { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}