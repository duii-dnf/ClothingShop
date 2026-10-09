using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothingShop.ViewModels
{
    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<MenuItemViewModel> Children { get; set; } = new List<MenuItemViewModel>();
    }
}