using System.Web;
using System.Web.Mvc;
using ClothingShop.Filters;

namespace ClothingShop
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AdminAreaAuthorizeFilter());
        }
    }
}
