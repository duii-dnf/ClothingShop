using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class ProductSize
    {
        public ProductSize()
        {
            Variants = new HashSet<ProductVariant>();
        }

        [Key]
        public int SizeId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã kích cỡ")]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tên kích cỡ")]
        public string Name { get; set; }

        [Display(Name = "Thứ tự sắp xếp")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
