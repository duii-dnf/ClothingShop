using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothingShop.Web.ViewModels
{
    public class CartItemVM
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string SKU { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string ColorHex { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public int MaxStock { get; set; } = 999;

        public string ItemKey => $"{ProductId}_{ProductVariantId ?? 0}_{Size ?? ""}_{Color ?? ""}";

        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
