using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class UserAddress
    {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Tỉnh / Thành phố")]
        public string Province { get; set; }

        [StringLength(100)]
        [Display(Name = "Quận / Huyện")]
        public string District { get; set; }

        [StringLength(100)]
        [Display(Name = "Phường / Xã")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ chi tiết")]
        public string DetailAddress { get; set; }

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        [Display(Name = "Đặt làm mặc định")]
        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual AppUser User { get; set; }
    }
}
