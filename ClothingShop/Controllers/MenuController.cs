using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class MenuController : Controller
    {
        [ChildActionOnly]
        public ActionResult CategoryMenu(string mode = "desktop")
        {
            using (var db = new ClothingShopDBEntities())
            {
                var all = db.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryId)
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

                string view = mode == "footer" ? "_CategoryMenuFooter" : "_CategoryMenu";
                return PartialView(view, menu);
            }
        }
    }
}
