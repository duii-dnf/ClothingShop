using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingShop.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        [StringLength(20)]
        public string Size { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        // Giá tại thời điểm mua (không phụ thuộc giá hiện tại của sản phẩm)
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Subtotal
        {
            get { return UnitPrice * Quantity; }
        }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}