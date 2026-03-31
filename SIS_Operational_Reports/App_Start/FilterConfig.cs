using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
