using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.VesselParticulars
{
    public class VesselParticularsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "VesselParticulars";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "VesselParticulars_default",
                "VesselParticulars/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}