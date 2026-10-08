using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClothingShop.Models
{
    public class Category
    {
        public Category()
        {
            Children = new HashSet<Category>();
            Products = new HashSet<Product>();
        }

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên loại")]
        [StringLength(100, ErrorMessage = "Tên loại tối đa 100 ký tự")]
        [Display(Name = "Tên loại")]
        public string Name { get; set; }

        [Display(Name = "Loại cha")]
        public int? ParentId { get; set; }

        [Display(Name = "Hiển thị")]
        public bool IsActive { get; set; } = true;

        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}