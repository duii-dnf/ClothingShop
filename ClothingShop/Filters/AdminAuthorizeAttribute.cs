using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClothingShop.Services;

namespace ClothingShop.Filters
{
    // Chỉ tài khoản Role = Admin mới vào được
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = SessionHelper.GetCurrentUser(httpContext.Session);
            return user != null && user.IsAdmin;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var user = SessionHelper.GetCurrentUser(filterContext.HttpContext.Session);

            if (user == null)
            {
                // Chưa đăng nhập -> về trang đăng nhập, đăng nhập xong quay lại trang đang vào
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
            }
            else
            {
                // Đã đăng nhập nhưng không phải Admin
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "AccessDenied"
                }));
            }
        }
    }
}