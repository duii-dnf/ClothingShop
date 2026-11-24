using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class ProductStyle
    {
        public ProductStyle()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        public int StyleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên phong cách")]
        public string Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; }
    }
}
