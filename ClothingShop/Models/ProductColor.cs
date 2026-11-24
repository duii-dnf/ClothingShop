using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class ProductColor
    {
        public ProductColor()
        {
            Variants = new HashSet<ProductVariant>();
        }

        [Key]
        public int ColorId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Mã màu")]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Tên màu")]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã Hex")]
        public string HexCode { get; set; }

        [StringLength(300)]
        [Display(Name = "Ảnh đại diện màu")]
        public string ImageUrl { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
