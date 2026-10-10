using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothingShop.Models;

namespace ClothingShop.ViewModels
{
    public class HomeCategoryTile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class HomeViewModel
    {
        public List<HomeCategoryTile> Categories { get; set; } = new List<HomeCategoryTile>();
        public List<Product> NewProducts { get; set; } = new List<Product>();       // lưới chính (12 sản phẩm, theo tiêu chí sắp xếp)
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();  // lưới phụ (8 sản phẩm), chỉ hiện khi sắp xếp mặc định
        public string Sort { get; set; } = "newest";
    }
}