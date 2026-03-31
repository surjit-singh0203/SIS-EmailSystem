using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.adminDashboard
{
    public class adminDashboardAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "adminDashboard";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "adminDashboard_default",
                "adminDashboard/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}