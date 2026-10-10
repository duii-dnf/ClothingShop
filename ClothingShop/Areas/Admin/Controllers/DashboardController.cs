using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothingShop.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        // TV4 sẽ bổ sung thống kê doanh thu vào đây
        public ActionResult Index()
        {
            return View();
        }
    }
}