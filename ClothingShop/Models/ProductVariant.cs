using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingShop.Models
{
    public class ProductVariant
    {
        public ProductVariant()
        {
            OrderDetails = new HashSet<OrderDetail>();
            InventoryLogs = new HashSet<InventoryLog>();
        }

        [Key]
        public int VariantId { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }

        [Display(Name = "Kích cỡ")]
        public int? SizeId { get; set; }

        [Display(Name = "Màu sắc")]
        public int? ColorId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giá không hợp lệ")]
        [Display(Name = "Giá ghi đè (nếu có)")]
        public decimal? Price { get; set; }

        [Range(0, 100000, ErrorMessage = "Tồn kho không hợp lệ")]
        [Display(Name = "Số lượng tồn")]
        public int Stock { get; set; } = 0;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Product Product { get; set; }
        public virtual ProductSize Size { get; set; }
        public virtual ProductColor Color { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<InventoryLog> InventoryLogs { get; set; }
    }
}
