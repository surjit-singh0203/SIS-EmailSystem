using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.CharterParty.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class CPController : BaseController
    {
        // GET: CharterParty/CP
        public static int NewMaxCPid { get; set; }
        public ActionResult Index(int? pageNo, string firstVal, string dateF, string dateT)
        {
            
            CharterPartyClass cpm = new CharterPartyClass();


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);


            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                cpm.CPVesselInfo = CommonMethods.GetCharterPartyListAdmin( currPage, pageSize);
                return View(cpm);
            }
            else if (firstVal == "" && dateF == "")
            {
                cpm.CPVesselInfo = CommonMethods.GetCharterPartyListAdmin( currPage, pageSize);
                return View(cpm);
            }
            else if (firstVal != null)
            {
                cpm.CPVesselInfo = CommonMethods.SearchCPVoyageDList(0, currPage, pageSize, firstVal, dateF, dateT, "CPAdmin");
                return PartialView("_searchCP", cpm);
            }


            //cpm.CPVesselInfo = CommonMethods.GetCharterPartyList();
            return View(cpm);
        }



       
        public ActionResult EditCPContract(int? Id)
        {
          
                return View("AddCPContract", CommonMethods.GetCharterPartyDetails(Convert.ToInt32(Id)));
           
        }
        public ActionResult AddCPContract()
        {
           
                CharterPartyClass cpm = new CharterPartyClass();
                //cpm.VesselList = CommonClass.GetVesselList();
                return View(cpm);
            
        }


        [HttpPost]
        public ActionResult AddCPContract(CharterPartyClass CPs)
        {

            // Save CP
            string Action = CPs.Id != 0 ? "Update" : "Insert";

            //int vslid = Convert.ToInt32(Session["VesselID"]);
            //CPs.VesselID = vslid;
            int Exist = 0;
            if (Action == "Insert")
            {
                CommonMethods.InsertCPContrect(CPs, Action, out Exist);
                TempData["Success"] = "Record saved successfully";
            }
            if (Action == "Update")
            {
                CommonMethods.InsertCPContrect(CPs, Action, out Exist);
                TempData["Success"] = "Record update successfully";
            }
            if (Exist == 0)
                return RedirectToAction("Index");
            else
            {
                ModelState.AddModelError("", "Charter Party ID already exist.");
                return View(CPs);
            }
        }

        public ActionResult ViewCPDetail(int Id)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            // CharterPartyClass cpm = new CharterPartyClass();
            return View(CommonMethods.GetCharterPartyDetails(Id));
        }
        public ActionResult AddCP_Part1(int id)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            TempData["CPID"] = id;
            CharterPartyClass cpm = new CharterPartyClass();
            cpm.Id = id;
            cpm.VesselList = CommonClass.GetVesselList();
            return View(cpm);
        }
        public ActionResult AddCP_Part2(int id)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            TempData["CPID"] = id;
            CharterPartyClass cpm = new CharterPartyClass();
            cpm.Id = id;
            return View(cpm);
        }
        public ActionResult AddCP_Part3(int id)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            TempData["CPID"] = id;
            CharterPartyClass cpm = new CharterPartyClass();
            cpm.Id = id;
            return View(cpm);
        }

        public JsonResult VesselDupCheck(int vesselid,int cpid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            int cpid1 = 0;
            try
            {
                //int CPId = (int)TempData["CPID"];
                using (SqlDataAdapter adp = new SqlDataAdapter("select * from CPpart1 where IsActive=1 and VesselID="+ vesselid + " and CPId="+ cpid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        cpid1 = 1;
                    }
                }


               
            }
            catch { }
            // return jst;

            return Json(new { Result = true,  CPID = cpid1 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InsertCharterParty1(List<CPVessel> CPVesselInfo)
        {
            var pathid = (int)TempData["CPID"];

            int checkOverlapdt = 0;

            //NewMaxCPid = CommonClass.GetMaxId("CPpart1") +1;
            if (CPVesselInfo.Count > 0 || CPVesselInfo != null)
            {
                foreach (var item in CPVesselInfo)
                {
                    item.CPId = (int)TempData["CPID"];
                    if (checkOverlapdt == 1)
                    {
                        checkOverlapdt = 0;
                    }
                    var startdt = item.StartDate.ToString("yyyy-MM-dd");
                    var enddt = item.EndDate.ToString("yyyy-MM-dd");

                    using (SqlDataAdapter adp=new SqlDataAdapter ("SELECT * FROM CPpart1 WHERE '"+ startdt + "' >= StartDate AND EndDate >= '"+ startdt + "' and IsActive=1 and VesselID="+ item.VesselID + " and CPId="+ item.CPId + "", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        if(dt.Rows.Count > 0)
                        {
                            checkOverlapdt = 1;
                        }
                        //else
                        //{
                        //    checkOverlapdt = 0;
                        //}
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM CPpart1 WHERE '" + enddt + "' >= StartDate AND EndDate >= '" + enddt + "' and IsActive=1 and VesselID=" + item.VesselID + " and CPId=" + item.CPId + "", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            checkOverlapdt = 1;
                        }
                        //else
                        //{
                        //    checkOverlapdt = 0;
                        //}
                    }
                    if (checkOverlapdt == 0)
                    {
                        CommonMethods.InsertCPpart1(item, "Insert");
                    }
                }

                // return Json(new { Success = true, Data = "AddCP_Part2" });
                return Json(new { Success = true,OverLap= checkOverlapdt, Data = "ViewCPDetail/" + pathid });
            }
            else
            {
                return Json(new { Success = false, Message = "Error" });
            }

        }

        public JsonResult InsertCharterParty2(List<CPMainCONSUMPTIONClass> MainCONSUMPTION)
        {

            //int NewMaxid = CommonClass.GetMaxId("CPpart1") + 1;
            var pathid = (int)TempData["CPID"];
            if (MainCONSUMPTION.Count > 0 || MainCONSUMPTION != null)
            {
                foreach (var item in MainCONSUMPTION)
                {
                    item.CPId = (int)TempData["CPID"];
                    CommonMethods.InsertCPpart2(item, "Insert");
                }

                return Json(new { Success = true, Data = "ViewCPDetail/" + pathid });
            }
            else
            {
                return Json(new { Success = false, Data = "AddCP_Part1" });
            }
        }
        public JsonResult InsertCharterParty3(List<CPOtherCONSUMPTIONClass> OtherCONSUMPTION)
        {

            var pathid = (int)TempData["CPID"];
            if (OtherCONSUMPTION.Count > 0 || OtherCONSUMPTION != null)
            {
                foreach (var item in OtherCONSUMPTION)
                {
                    item.CPId = (int)TempData["CPID"];
                    CommonMethods.InsertCPpart3(item, "Insert");
                }

                return Json(new { Success = true, Data = "ViewCPDetail/" + pathid });
            }
            else
            {
                return Json(new { Success = false, Message = "Error" });
            }
        }

        public ActionResult EditVesselinfo(int id,int CPId)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            CPVessel cp = new CPVessel();
            cp.VesselList1 = CommonClass.GetVesselList();
            DataTable dtp = CommonMethods.GetCharterPartyByID(id, CPId, "CPpart1");
            if(dtp.Rows.Count>0)
            {
                cp.Id = id;
                cp.CPId = CPId;
                cp.Svid = Convert.ToInt32(dtp.Rows[0]["VesselID"]);
                cp.VesselID = Convert.ToInt32(dtp.Rows[0]["VesselID"]);
                cp.StartDate = Convert.ToDateTime(dtp.Rows[0]["StartDate"]);
                cp.EndDate = Convert.ToDateTime(dtp.Rows[0]["EndDate"]);
            }
            return View(cp);
        }

        [HttpPost]
        public ActionResult EditVesselinfo(CPVessel cpv)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            //TempData["CPID"] = id;
            //CharterPartyClass cpm = new CharterPartyClass();
            //cpm.Id = id;
            // return View(cpm);
            int checkOverlapdt = 0;

            using (SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM CPpart1 WHERE '" + cpv.StartDate + "' > StartDate AND EndDate > '" + cpv.StartDate + "' and IsActive=1 and VesselID=" + cpv.VesselID + " and CPId=" + cpv.CPId + "  and id not in ("+ cpv .Id+ ")", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    checkOverlapdt = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("SELECT * FROM CPpart1 WHERE '" + cpv.EndDate + "' > StartDate AND EndDate > '" + cpv.EndDate + "' and IsActive=1 and VesselID=" + cpv.VesselID + " and CPId=" + cpv.CPId + "  and id not in (" + cpv.Id + ")", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    checkOverlapdt = 1;
                }
            }
            if (checkOverlapdt == 0)
            {
                CommonMethods.InsertCPpart1(cpv, "Update");
                return RedirectToAction("ViewCPDetail", new { id = cpv.CPId });
            }
            else
            {
                TempData["InvalidDate"] = 1;
                return RedirectToAction("EditVesselinfo", new { id = cpv.Id, CPId = cpv.CPId });
            }

            
           
        }

        public ActionResult EditMainConsumption(int id, int CPId)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            CPMainCONSUMPTIONClass cp = new CPMainCONSUMPTIONClass();
            DataTable dtp = CommonMethods.GetCharterPartyByID(id, CPId, "CPpart2");
            if (dtp.Rows.Count > 0)
            {
                cp.Id = id;
                cp.CPId = CPId;
                cp.Speed = Convert.ToDecimal(dtp.Rows[0]["Speed"]);
                cp.ME_LADEN = Convert.ToDecimal(dtp.Rows[0]["ME_LADEN"]);
                cp.ME_BALLAST = Convert.ToDecimal(dtp.Rows[0]["ME_BALLAST"]);
                cp.AE_VLSFO = Convert.ToDecimal(dtp.Rows[0]["AE_VLSFO"]);
                cp.AE_DO = Convert.ToDecimal(dtp.Rows[0]["AE_DO"]);
            }
            return View(cp);
        }

        [HttpPost]
        public ActionResult EditMainConsumption(CPMainCONSUMPTIONClass cpv)
        {
            // List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            //TempData["CPID"] = id;
            //CharterPartyClass cpm = new CharterPartyClass();
            //cpm.Id = id;
            // return View(cpm);
            CommonMethods.InsertCPpart2(cpv, "Update");
            return RedirectToAction("ViewCPDetail", new { id = cpv.CPId });
        }

        public ActionResult EditOtherConsumption(int id, int CPId)
        {
            //Delete List<CPMainCONSUMPTIONClass> cpm = new List<CPMainCONSUMPTIONClass>();
            CPOtherCONSUMPTIONClass cp = new CPOtherCONSUMPTIONClass();
            DataTable dtp = CommonMethods.GetCharterPartyByID(id, CPId, "CPpart3");
            if (dtp.Rows.Count > 0)
            {
                cp.Id = id;
                cp.CPId = CPId;
                cp.Consumption = dtp.Rows[0]["Consumption"].ToString();
                cp.AE_VLSFO = Convert.ToDecimal(dtp.Rows[0]["AE_VLSFO"]);
                cp.AE_MDO = Convert.ToDecimal(dtp.Rows[0]["AE_MDO"]);
                cp.BOILER_MDO = Convert.ToDecimal(dtp.Rows[0]["BOILER_MDO"]);
            }
            return View(cp);
        }






        [HttpPost]
        public ActionResult EditOtherConsumption(CPOtherCONSUMPTIONClass cpv)
        {
            CommonMethods.InsertCPpart3(cpv, "Update");
            return RedirectToAction("ViewCPDetail", new { id = cpv.CPId });
        }

        public ActionResult Delete(int id, int CPId,string tbl)
        {
            CommonMethods.DeleteCharterPartyByID(id, CPId, tbl);
            return RedirectToAction("ViewCPDetail", new { id = CPId });
        }
        public ActionResult DeleteCP(int id)
        {
            CommonMethods.CommonDelete(id, "CPContract");
            return RedirectToAction("Index");
        }

    }
}