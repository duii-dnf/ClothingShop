using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, 100000000, ErrorMessage = "Giá không hợp lệ")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giá khuyến mãi không hợp lệ")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? DiscountPrice { get; set; }

        [StringLength(300)]
        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; }

        // Radio button: Nam / Nữ / Unisex
        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        // Checkbox
        [Display(Name = "Sản phẩm mới")]
        public bool IsNew { get; set; }

        // Checkbox
        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        // Danh sách size/màu cách nhau bằng dấu phẩy, vd "S,M,L,XL"
        [StringLength(100)]
        [Display(Name = "Kích cỡ")]
        public string Sizes { get; set; }

        [StringLength(100)]
        [Display(Name = "Màu sắc")]
        public string Colors { get; set; }

        [Range(0, 100000, ErrorMessage = "Tồn kho không hợp lệ")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn loại")]
        [Display(Name = "Loại")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }
    }
}