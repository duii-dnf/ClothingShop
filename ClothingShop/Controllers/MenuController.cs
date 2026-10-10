using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class MenuController : Controller
    {
        // mode: "desktop" (mega-menu), "mobile" (accordion), "footer" (danh sách link)
        [ChildActionOnly]
        public ActionResult CategoryMenu(string mode = "desktop")
        {
            using (var db = new ShopDbContext())
            {
                var all = db.Categories
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .Select(c => new { c.CategoryId, c.Name, c.ParentId })
                            .ToList();

                var menu = all.Where(c => c.ParentId == null)
                              .Select(p => new MenuItemViewModel
                              {
                                  Id = p.CategoryId,
                                  Name = p.Name,
                                  Children = all.Where(ch => ch.ParentId == p.CategoryId)
                                                .Select(ch => new MenuItemViewModel { Id = ch.CategoryId, Name = ch.Name })
                                                .ToList()
                              })
                              .ToList();

                string view = mode == "mobile" ? "_CategoryMenuMobile"
                            : mode == "footer" ? "_CategoryMenuFooter"
                            : "_CategoryMenu";
                return PartialView(view, menu);
            }
        }
    }
}