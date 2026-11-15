using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothingShop.Web.ViewModels
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // Tự động tính toán, tránh lỗi nhân sai hoặc null
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}