using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.SyncToEmailReport
{
    public class SyncToEmailReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "SyncToEmailReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "SyncToEmailReport_default",
                "SyncToEmailReport/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}