using System.Web;
using System.Web.Optimization;

namespace ClothingShop
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.bundle.min.js"));

            bundles.Add(new Bundle("~/Content/css").Include(
                        "~/Content/bootstrap.css",
                        "~/Content/Site.css"));
        }
    }
}
