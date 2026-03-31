using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.ImportExport
{
    public class ImportExportAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ImportExport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ImportExport_default",
                "ImportExport/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}