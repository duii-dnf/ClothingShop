using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ClothingShop.Models;
using ClothingShop.ViewModels;

namespace ClothingShop.Controllers
{
    public class MenuController : Controller
    {
        [ChildActionOnly]
        public ActionResult SearchProductData()
        {
            using (var db = new ClothingShopDBEntities())
            {
                var items = db.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Name,
                        imageUrl = p.ImageUrl,
                        price = p.Price,
                        discountPrice = p.DiscountPrice
                    })
                    .ToList()
                    .Select(p => new
                    {
                        p.id,
                        p.name,
                        imageUrl = string.IsNullOrEmpty(p.imageUrl)
                            ? "/Content/images/products/default.jpg"
                            : (p.imageUrl.StartsWith("http") || p.imageUrl.StartsWith("/")
                                ? p.imageUrl
                                : "/Content/images/products/" + p.imageUrl),
                        p.price,
                        p.discountPrice
                    })
                    .ToList();

                ViewBag.ProductsJson = new JavaScriptSerializer().Serialize(items);
                return PartialView("_SearchProductData");
            }
        }

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
