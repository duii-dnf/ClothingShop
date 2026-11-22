using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClothingShop.Services;

namespace ClothingShop.Filters
{
    // Phân quyền khu vực quản trị: mặc định cho phép Admin và Staff; có thể giới hạn Roles = AppRoles.Admin
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = SessionHelper.GetCurrentUser(httpContext.Session);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var allowed = Roles.Split(',').Select(r => r.Trim());
                return allowed.Any(r => string.Equals(r, user.Role, StringComparison.OrdinalIgnoreCase));
            }

            return user.CanAccessAdmin;
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
                // Đã đăng nhập nhưng không đủ quyền
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "AccessDenied"
                }));
            }
        }
    }

    // Bộ lọc toàn cục: bảo vệ toàn bộ các Controller trong Area "Admin" (cho Admin & Staff)
    public class AdminAreaAuthorizeFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var area = filterContext.RouteData.DataTokens["area"] as string;
            if (string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var user = SessionHelper.GetCurrentUser(filterContext.HttpContext.Session);
                if (user == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        area = "",
                        controller = "Account",
                        action = "Login",
                        returnUrl = filterContext.HttpContext.Request.RawUrl
                    }));
                }
                else if (!user.CanAccessAdmin)
                {
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
}