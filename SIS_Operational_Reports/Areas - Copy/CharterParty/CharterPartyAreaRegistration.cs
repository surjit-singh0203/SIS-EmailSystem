using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.CharterParty
{
    public class CharterPartyAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CharterParty";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CharterParty_default",
                "CharterParty/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}