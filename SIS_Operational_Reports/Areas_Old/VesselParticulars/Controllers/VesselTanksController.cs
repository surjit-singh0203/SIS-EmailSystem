using DataBuildingLayer;
using Newtonsoft.Json;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SIS_Operational_Reports.Areas.VesselParticulars.Controllers
{
    [Authorize]
    public class VesselTanksController : Controller
    {
        // GET: VesselParticulars/VesselTanks
        public ActionResult Index(int? ImoNo)
        {
            TanksType tanks = new TanksType();
            Session["ImoNo"] = ImoNo;
            ViewBag.VesselName = CommonMethods.GetVesselName(ImoNo);
            tanks.GetTanksTypeList = CommonMethods.GetTankTypeList();
            return View(tanks);
        }

        public ActionResult inserttank(int typeid)
        {
            List<TanksAndHolds> tandH = new List<TanksAndHolds>();
            return View(tandH);
           
        }

        public JsonResult inserttankandholds(List<TanksAndHolds> tanks)
        {   
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(tanks);

            TanksAndHolds cls = new TanksAndHolds();
            cls.VesselId = Convert.ToInt32(Session["ImoNo"]);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cls.Name = dt.Rows[i]["Name"].ToString();
                cls.Height =Convert.ToDecimal(dt.Rows[i]["Height"]);
                cls.Capacity = Convert.ToDecimal(dt.Rows[i]["Capacity"]);
                cls.TanksTypeId = Convert.ToInt32(dt.Rows[i]["TanksTypeId"]);

                int checkExistense = CommonMethods.CheckTHsNameExistence(cls, 0, 0, "Insert");
                if (checkExistense == 0)
                {
                    CommonMethods.InsertUpdateTanksAndHolds(cls, "Insert");
                    TempData["Success"] = "Record saved successfully";
                }
                else
                {
                    //TempData["Error"] = ""+ cls.Name + " tank already exist ! ";

                    cls.TanksTypeId = 0;
                }
            
                
            }
            //int insertedRecords = dt.Rows.Count;
            int insertedRecords = cls.TanksTypeId;

            //return Json(new
            //{
            //    redirectUrl = Url.Action("viewtanks?typeid=1", "/VesselTanks/", new { area = "VesselParticulars" }),
            //    isRedirect = true
            //});
            return Json(insertedRecords);
        }

        public ActionResult viewtanks(int typeid)
        {
            int VesselId = Convert.ToInt32(Session["ImoNo"]);
            TanksAndHolds mode = new TanksAndHolds();
            mode.GetTanksandHoldList = CommonMethods.viewTanksandHoldsList(typeid, VesselId);
            return View(mode);
        }

        public ActionResult edit(int id)
        {
            TanksAndHolds tanksAndHold = new TanksAndHolds();
            tanksAndHold.GetTanksandHoldList = CommonMethods.editTanksandHoldsList(id);
            tanksAndHold.Id = id;
            tanksAndHold.Name = tanksAndHold.GetTanksandHoldList.Select(x => x.Name).SingleOrDefault();
            tanksAndHold.Height = tanksAndHold.GetTanksandHoldList.Select(x => x.Height).SingleOrDefault();
            tanksAndHold.Capacity = tanksAndHold.GetTanksandHoldList.Select(x => x.Capacity).SingleOrDefault();
            tanksAndHold.TanksTypeId = tanksAndHold.GetTanksandHoldList.Select(x => x.TanksTypeId).SingleOrDefault();
            return View(tanksAndHold);
        }

        [HttpPost]
        public ActionResult edit(TanksAndHolds cls)
        {

            int checkExistense = CommonMethods.CheckTHsNameExistence(cls, 0, cls.Id, "Update");
            if (checkExistense == 0)
            {
                CommonMethods.InsertUpdateTanksAndHolds(cls, "Update");
                TempData["Success"] = "Record update successfully ";
            }
            else
            {
                TempData["Error"] = "Name already exists ! ";
            }
            return RedirectToAction("viewtanks",new { typeid=cls.TanksTypeId });
        }
        public ActionResult pumpandtank(int? pageNo, string firstVal, string dateF, string dateT)
        {
            VesselDetail mode = new VesselDetail();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);


            TempData["CurrentPage"] = currPage;


            if (firstVal == null)
            {
                mode.GetVesselList = CommonMethods.GetVesselList( currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetVesselList = CommonMethods.GetVesselList( currPage, pageSize);
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetVesselList = CommonMethods.SearchVesselPartList(0, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_pandtank", mode);
            }



           // mode.GetVesselList = CommonMethods.GetVesselList( currPage, pageSize);
            return View(mode);
        }
        public ActionResult Delete(int id,int tankstypeid)
        {
            CommonMethods.CommonDelete(id, "TanksandHolds");
            TempData["Success"] = "Record deleted successfully";

            //int imono = Convert.ToInt32(Session["ImoNo"]);
            //return RedirectToAction("Index", new { ImoNo = imono });
            return RedirectToAction("viewtanks", new { typeid = tankstypeid });
        }

        [HttpPost]

        public ActionResult InsertInOrder(string tanklist)
        {
            int tpid = 0;
            try
            {
                int VesselId = Convert.ToInt32(Session["ImoNo"]);
                var Result = tanklist;
                string json = Result.ToString();
                string data = json;

                int count = 0;
                var Json = JsonConvert.DeserializeObject<List<TanksAndHolds>>(data);
                foreach (var rootObject in Json)
                {
                    count++;
                    tpid = rootObject.TanksTypeId;
                    CommonMethods.InsertinOrder(count, rootObject.Name, rootObject.TanksTypeId,VesselId, "TanksandHolds");                 
                }
            }
            catch { }

            TempData["Success"] = "Order saved successfully !";          
            return Json(Url.Action("viewtanks",new { typeid= tpid }));
         
        }

        public JsonResult MaintainROB(int mainID,int Id)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            using (SqlDataAdapter sda = new SqlDataAdapter("update TanksType set MaintainingROBs="+mainID+" where  id=" + Id + "", ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);                
            }            
            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }
    }
}