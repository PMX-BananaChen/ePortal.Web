using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using ePortal.Web.Core.ActionFilters;
using ePortal.Web.Core.Authentication;
using ePortal.Web.Core.Models;
using ePortal.Web.Core.Job;
using System.IO;
using System.Net;
using System.Threading;

namespace ePortal.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new UserFilter());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
        public override void Init()
        {
            this.PostAuthenticateRequest += this.PostAuthenticateRequestHandler;
            this.AcquireRequestState += MvcApplication_AcquireRequestState;
            base.Init();
            

        }

        void MvcApplication_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                if (Session["lan"] != null)
                {
                    System.Globalization.CultureInfo culture = (System.Globalization.CultureInfo)Session["lan"]; 

                    System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                    System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                }
            }
        }


        private void PostAuthenticateRequestHandler(object sender, EventArgs e)
        {

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                HttpCookie authCookie = this.Context.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (IsValidAuthCookie(authCookie))
                {
                    var formsAuthentication = DependencyResolver.Current.GetService<IFormsAuthentication>();
                    var ticket = formsAuthentication.Decrypt(authCookie.Value);
                    var ePortalUser = new Identity(ticket);
                    string[] userRoles = { ePortalUser.RoleName };
                    this.Context.User = new GenericPrincipal(ePortalUser, userRoles);
                    formsAuthentication.SetAuthCookie(this.Context, ticket);
                }
            }
        }
        private static bool IsValidAuthCookie(HttpCookie authCookie)
        {
            return authCookie != null && !String.IsNullOrEmpty(authCookie.Value);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Bootstrapper.Run();
           //新考勤門禁稽核報表不需要執行發郵件Job
           //本程序主要實現定時任務:
           //1.每月15號計算與更新上月考勤稽核數據
           //2.每天計算當月考勤數據
          // QuartzScheduler jobs= new QuartzScheduler();
          // jobs.Start();
           LogHelper.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":Application Start!");
        }

        protected void Application_End()
        {

            LogHelper.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":Application End!");

            //这里设置你的web地址，可以随便指向你的任意一个aspx页面甚至不存在的页面，目的是要激发Application_Start

            Thread.Sleep(1000);

            string url = "http://10.40.1.184:9005";

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

            Stream receiveStream = myHttpWebResponse.GetResponseStream();//得到回写的字节流           

        }
    }
}