
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.UserHelp.Controllers
{
    [Authorize]
    public class UserHelpController : Controller
    {
        // GET: UserHelp/UserHelp
        public UserHelpController()
        {
            //CommonClass.TopeMenuID = "Menu5";
        }
        public ActionResult Index()
        {
            //  string url = "HelpManual/1.0_ABOUT.htm";
           
            //var path = Server.MapPath("~/WebHelp/index.htm");
            //var fullpath = Path.Combine(path, "myfile.txt");
           // string url = "WebHelp/index.htm";
           // ViewBag.help = path;

             return View();

            
        }
    }
}