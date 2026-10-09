using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClothingShop.Filters;

namespace ClothingShop.Areas.Admin.Controllers
{
    // Mọi controller trong Area Admin PHẢI kế thừa lớp này để tự động được bảo vệ
    [AdminAuthorize]
    public abstract class AdminBaseController : Controller
    {
        
    }
}