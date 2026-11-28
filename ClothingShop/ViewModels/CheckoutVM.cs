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

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên người nhận tối thiểu 2 ký tự.")]
        [RegularExpression(@"^[\p{L}\s'.-]{2,100}$", ErrorMessage = "Họ tên người nhận không được chứa ký tự đặc biệt bất thường.")]
        [Display(Name = "Họ tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (gồm 10 chữ số, bắt đầu bằng số 0).")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập Tỉnh / Thành phố.")]
        [Display(Name = "Tỉnh / Thành phố")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập Quận / Huyện.")]
        [Display(Name = "Quận / Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hoặc nhập Phường / Xã.")]
        [Display(Name = "Phường / Xã")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể (số nhà, tên đường...).")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Địa chỉ chi tiết phải có ít nhất 5 ký tự.")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string Address { get; set; }

        [Display(Name = "Ghi chú giao hàng")]
        public string Note { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [Display(Name = "Mã giảm giá")]
        public string CouponCode { get; set; }

        [Display(Name = "Lưu vào sổ địa chỉ")]
        public bool SaveToAddressBook { get; set; } = false;

        public List<UserAddress> SavedAddresses { get; set; } = new List<UserAddress>();
    }
}
