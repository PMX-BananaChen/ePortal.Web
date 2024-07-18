using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ePortal.Web.Core.ActionFilters;
using ePortal.Data.Repositories;
using ePortal.Models;
using ePortal.Web.ViewModels;
using ePortal.Web.Helpers;
namespace ePortal.Web.Controllers
{
    [Authorizer]
    public class HomeController : Controller
    {
        private readonly IEntityRepository<SystemUser> userRepository;


        public HomeController(IEntityRepository<SystemUser> userRepository)
        {
            this.userRepository = userRepository;
        }
        [OutputCache(CacheProfile = "Home/Index")]
        public ActionResult Index()
        {
            var menus = userRepository.SqlQuery<NestedMenuItem>("exec [sp_MenuItems] {0}", HttpContext.User.Identity.Name);
            return View(menus.ToMenuItem());
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Language(string id)
        {
            string lan = "zh-TW";
            switch (id)
            {
                case "Traditional": lan = "zh-TW"; break;
                case "Simplified": lan = "zh-CN"; break;
                case "English": lan = "en-US"; break;
            }

            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture(lan);
            Session["lan"] = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            return new JavaScriptResult { Script = "location.reload()" };
        }


    }
}
