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
        public List<Product> NewProducts { get; set; } = new List<Product>();       // lưới 1: 12 sản phẩm mới nhất
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();  // lưới 2: 8 sản phẩm nổi bật
    }
}