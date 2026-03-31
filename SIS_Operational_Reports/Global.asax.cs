using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SIS_Operational_Reports
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            CultureInfo culure = new CultureInfo("en-US");
            culure.DateTimeFormat.DateSeparator = "-";
            culure.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culure.DateTimeFormat.LongDatePattern = "yyyy-MM-dd HH:mm:ss";
            culure.DateTimeFormat.ShortTimePattern = "HH:mm";
            culure.DateTimeFormat.LongTimePattern = "HH:mm:ss";


            Thread.CurrentThread.CurrentCulture = culure;
            Thread.CurrentThread.CurrentUICulture = culure;


            CultureInfo.DefaultThreadCurrentCulture = culure;
            CultureInfo.DefaultThreadCurrentUICulture = culure;
           // SetApplicationCulture("en-US");
           // SetDateFormat("yyyy-MM-dd");


        }

        //private void SetApplicationCulture(string culture)
        //{
        //    var cultureInfo = new CultureInfo(culture);
        //    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        //    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        //    Thread.CurrentThread.CurrentCulture = cultureInfo;
        //    Thread.CurrentThread.CurrentUICulture = cultureInfo;
        //}

        //private void SetDateFormat(string dateFormat)
        //{
        //    Application["DateFormat"] = dateFormat;
        //}


    }
}
