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
        public List<Product> NewProducts { get; set; } = new List<Product>();
        public List<Product> BestSellerProducts { get; set; } = new List<Product>();
        public List<Product> SaleProducts { get; set; } = new List<Product>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public string Sort { get; set; } = "newest";
    }
}