using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class InventoryLog
    {
        [Key]
        public int LogId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại thay đổi")]
        public string ChangeType { get; set; }

        [Display(Name = "Số lượng thay đổi")]
        public int QuantityChange { get; set; }

        [Display(Name = "Tồn kho sau thay đổi")]
        public int StockAfter { get; set; }

        [StringLength(50)]
        [Display(Name = "Loại tham chiếu")]
        public string ReferenceType { get; set; }

        [Display(Name = "Mã tham chiếu")]
        public int? ReferenceId { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Người thực hiện")]
        public string CreatedBy { get; set; }

        public virtual Product Product { get; set; }
        public virtual ProductVariant Variant { get; set; }
    }
}
