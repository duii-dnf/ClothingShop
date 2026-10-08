using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingShop.Models
{
    [Table("Users")]
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        [Index(IsUnique = true)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = AppRoles.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}