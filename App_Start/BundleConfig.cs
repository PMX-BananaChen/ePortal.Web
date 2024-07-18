using System.Web;
using System.Web.Optimization;

namespace ePortal.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-1.*",
                        "~/Scripts/jquery.nicescroll.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryuimultiselect").Include(
                        "~/Scripts/multiselect/jquery.multiselect.js"));
            bundles.Add(new ScriptBundle("~/bundles/timepicker").Include(
                        "~/Scripts/jquery-ui-timepicker-addon.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                       
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/themes/hot-sneaks/menu").Include(
                        "~/Content/themes/hot-sneaks/menu.css"));

            bundles.Add(new StyleBundle("~/Content/themes/hot-sneaks/css").Include(
                        "~/Content/themes/hot-sneaks/jquery.ui.core.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.resizable.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.selectable.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.accordion.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.autocomplete.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.button.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.dialog.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.slider.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.tabs.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.datepicker.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.progressbar.css",
                        "~/Content/themes/hot-sneaks/jquery.ui.theme.css",
                        "~/Content/themes/hot-sneaks/jquery.multiselect.css"));

            
        }
    }
}