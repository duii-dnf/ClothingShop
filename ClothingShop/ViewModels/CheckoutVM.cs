using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ClothingShop.Models;

namespace ClothingShop.Web.ViewModels
{
    public class CheckoutVM
    {
        public int? SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [Display(Name = "Họ tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Tỉnh / Thành phố")]
        public string Province { get; set; }

        [Display(Name = "Quận / Huyện")]
        public string District { get; set; }

        [Display(Name = "Phường / Xã")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string Address { get; set; }

        [Display(Name = "Ghi chú giao hàng")]
        public string Note { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [Display(Name = "Mã giảm giá")]
        public string CouponCode { get; set; }

        [Display(Name = "Lưu vào sổ địa chỉ")]
        public bool SaveToAddressBook { get; set; } = false;

        public List<UserAddress> SavedAddresses { get; set; } = new List<UserAddress>();
    }
}
