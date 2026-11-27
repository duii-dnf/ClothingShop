using System.Web.Mvc;
using ClothingShop.Services;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class DefaultController : AdminBaseController
    {
        public ActionResult Index()
        {
            var user = SessionHelper.GetCurrentUser(Session);
            if (user != null && user.IsAdmin)
                return RedirectToAction("Index", "Dashboard");
            return RedirectToAction("Index", "Order");
        }
    }
}
