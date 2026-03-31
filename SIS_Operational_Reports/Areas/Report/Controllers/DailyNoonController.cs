using ClosedXML.Excel;
using DataBuildingLayer;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using SIS_Operational_Reports.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class DailyNoonController : BaseController
    {
        int check = 0; string FCcheck = "";
        // GET: Report/DailyNoon
        public ActionResult Index()
        {
            // DailyNoonReport dnR = new DailyNoonReport();



            // TempData["noonReportId"] = null;
            // int vslid = Convert.ToInt32(Session["VesselID"]);

            // string[] returnvalue = CommonMethods.GetVesselName1(vslid);
            // ViewBag.VesselName = returnvalue[0];
            // ViewBag.Displacement = returnvalue[1];

            //dnR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            // int id = 0;
            // string dtcheck = Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString("yyyy-MM-dd");

            // using (SqlDataAdapter adp = new SqlDataAdapter("select * from dailynoonreport where isactive=1 and VesselId=" + vslid + " and Date='" + dtcheck + "'", ConnectionBulder.con))
            // {
            //     DataTable dt = new DataTable();
            //     adp.Fill(dt);
            //     if (dt.Rows.Count > 0)
            //     {
            //         id = Convert.ToInt32(dt.Rows[0]["Id"]);
            //     }
            // }
            // if (id != 0)
            // {
            //     dnR.GetNoonRList = CommonMethods.editnoonRList(id, "DailyNoonReport");
            //     var noonRBind = dnR.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);

            //     noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            //     ViewBag.PreviousR = 1;

            //     ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
            //     return View(noonRBind);
            // }
            // else
            // {
            //     ViewBag.PreviousR = 0;
            //     ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
            //     return View(dnR);
            // }



            DailyNoonReport dnR = new DailyNoonReport();

            TempData["Edit"] = 0;
            TempData["noonReportId"] = null;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);

            string[] returnvalue = CommonMethods.GetVesselName1(vslid);
            ViewBag.VesselName = returnvalue[0];
            ViewBag.Displacement = returnvalue[1];
            decimal CP_Speed = 0.00m;
            dnR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            string DateReport = "";

            using (SqlDataAdapter adp1 = new SqlDataAdapter("select max(Date) as Date from dailynoonreport where isactive=1 and VesselId=" + vslid + "", ConnectionBulder.con))
            {
                DataTable dt1 = new DataTable();
                adp1.Fill(dt1);
                if (dt1.Rows.Count > 0)
                {
                    DateTime PrevDateReport = Convert.ToDateTime(dt1.Rows[0]["Date"] == DBNull.Value ? DateTime.Now : dt1.Rows[0]["Date"]);
                    DateReport = PrevDateReport.ToString("yyyy-MM-dd");
                }
            }

            int id = 0; string Latitude = "", Lat1 = "", Lat2 = "", Lat3 = "", Longitude = "", Long1 = "", Long2 = "", Long3 = "";
            string dtcheck = Convert.ToDateTime(DateTime.Now.AddDays(-1)).ToString("yyyy-MM-dd");

            using (SqlDataAdapter adp = new SqlDataAdapter("select * from dailynoonreport where isactive=1 and VesselId=" + vslid + " and Date='" + dtcheck + "'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    id = Convert.ToInt32(dt.Rows[0]["Id"]);
                    CP_Speed = Convert.ToDecimal(dt.Rows[0]["CP_Speed"]);
                    Latitude = dt.Rows[0]["Latitude"].ToString();
                    Lat1 = dt.Rows[0]["Lat1"].ToString();
                    Lat2 = dt.Rows[0]["Lat2"].ToString();
                    Lat3 = dt.Rows[0]["Lat3"].ToString();
                    Longitude = dt.Rows[0]["Longitude"].ToString();
                    Long1 = dt.Rows[0]["Long1"].ToString();
                    Long2 = dt.Rows[0]["Long2"].ToString();
                    Long3 = dt.Rows[0]["Long3"].ToString();
                }
            }
            if (id != 0)
            {

                dnR.GetNoonRList = CommonMethods.editnoonRList(id, "DailyNoonReport");
                var noonRBind = dnR.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
                noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
                noonRBind.LegPortList = BindLegPR(noonRBind.VoyageId);

                ViewBag.PreviousR = 1;
                noonRBind.CP_Speed = CP_Speed;
                try
                {
                    using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed),sum(NoonToNoonDMG_Dist) as TtlDistance from DailyNoonReport where IsActive=1 and SaveDraft=0 and VoyageId=" + noonRBind.VoyageId + " and LegPortId=" + noonRBind.LegPortId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                    {

                        DataTable dtvv = new DataTable();
                        adpvv.Fill(dtvv);
                        if (dtvv.Rows.Count > 0)
                        {
                            //noonRBind.Gen_Avg_Speed = Convert.ToDecimal(dtvv.Rows[0][0]);
                            //noonRBind.TotalDistance = Convert.ToDecimal(dtvv.Rows[0][1]);

                            noonRBind.Gen_Avg_Speed = Convert.ToDecimal(dtvv.Rows[0][0] == DBNull.Value ? 0 : dtvv.Rows[0][0]);
                            noonRBind.TotalDistance = Convert.ToDecimal(dtvv.Rows[0][1] == DBNull.Value ? 0 : dtvv.Rows[0][1]);
                        }

                    }
                    ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");

                    noonRBind.Latitude = Latitude;
                    noonRBind.Lat1 = Lat1;
                    noonRBind.Lat2 = Lat2;
                    noonRBind.Lat3 = Lat3;
                    noonRBind.Longitude = Longitude;
                    noonRBind.Long1 = Long1;
                    noonRBind.Long2 = Long2;
                    noonRBind.Long3 = Long3;

                    noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
                    noonRBind.CargoTanks = GetCargoTankList(id, vslid);
                    noonRBind.BallastTanks = GetBallastTankList(id, vslid);
                    noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

                    int legportid = noonRBind.LegPortId;

                    ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", id);
                    ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
                    ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", id);
                    ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

                }
                catch { }
                return View(noonRBind);
            }

            else if (!string.IsNullOrEmpty(DateReport))
            {
                using (SqlDataAdapter adp = new SqlDataAdapter("select * from dailynoonreport where isactive=1 and VesselId=" + vslid + " and Date='" + DateReport + "'", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        id = Convert.ToInt32(dt.Rows[0]["Id"]);
                        CP_Speed = Convert.ToDecimal(dt.Rows[0]["CP_Speed"]);
                        Latitude = dt.Rows[0]["Latitude"].ToString();
                        Lat1 = dt.Rows[0]["Lat1"].ToString();
                        Lat2 = dt.Rows[0]["Lat2"].ToString();
                        Lat3 = dt.Rows[0]["Lat3"].ToString();
                        Longitude = dt.Rows[0]["Longitude"].ToString();
                        Long1 = dt.Rows[0]["Long1"].ToString();
                        Long2 = dt.Rows[0]["Long2"].ToString();
                        Long3 = dt.Rows[0]["Long3"].ToString();
                    }
                }
                if (id != 0)
                {

                    dnR.GetNoonRList = CommonMethods.editnoonRList(id, "DailyNoonReport");
                    var noonRBind = dnR.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
                    DateTime date = Convert.ToDateTime(noonRBind.Date);
                    date = date.AddDays(1);
                    noonRBind.Date = date;
                    noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
                    noonRBind.LegPortList = BindLegPR(noonRBind.VoyageId);

                    ViewBag.PreviousR = 1;
                    noonRBind.CP_Speed = CP_Speed;
                    try
                    {
                        using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed),sum(NoonToNoonDMG_Dist) as TtlDistance from DailyNoonReport where IsActive=1 and SaveDraft=0 and VoyageId=" + noonRBind.VoyageId + " and LegPortId=" + noonRBind.LegPortId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {

                            DataTable dtvv = new DataTable();
                            adpvv.Fill(dtvv);
                            if (dtvv.Rows.Count > 0)
                            {
                                //noonRBind.Gen_Avg_Speed = Convert.ToDecimal(dtvv.Rows[0][0]);
                                //noonRBind.TotalDistance = Convert.ToDecimal(dtvv.Rows[0][1]);

                                noonRBind.Gen_Avg_Speed = Convert.ToDecimal(dtvv.Rows[0][0] == DBNull.Value ? 0 : dtvv.Rows[0][0]);
                                noonRBind.TotalDistance = Convert.ToDecimal(dtvv.Rows[0][1] == DBNull.Value ? 0 : dtvv.Rows[0][1]);
                            }

                        }
                        ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");

                        noonRBind.Latitude = Latitude;
                        noonRBind.Lat1 = Lat1;
                        noonRBind.Lat2 = Lat2;
                        noonRBind.Lat3 = Lat3;
                        noonRBind.Longitude = Longitude;
                        noonRBind.Long1 = Long1;
                        noonRBind.Long2 = Long2;
                        noonRBind.Long3 = Long3;

                        noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
                        noonRBind.CargoTanks = GetCargoTankList(id, vslid);
                        noonRBind.BallastTanks = GetBallastTankList(id, vslid);
                        noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

                        int legportid = noonRBind.LegPortId;

                        ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", id);
                        ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
                        ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", id);
                        ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

                    }
                    catch (Exception ex) { }
                    return View(noonRBind);
                }
                else
                {
                    ViewBag.PreviousR = 0;
                    ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
                    return View(dnR);
                }
            }
            else
            {
                ViewBag.PreviousR = 0;
                ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
                return View(dnR);
            }


        }
        public List<SelectListItem> BindLegPR(int Voyid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "'", ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {

                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["leg"].ToString(), Value = tbl.Rows[i]["id"].ToString() });

                    }

                }
            }

            return jst;
        }
        public ActionResult noonRlist(int? pageNo, string firstVal, string dateF, string dateT)
        {
            DailyNoonReport dnR = new DailyNoonReport();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["noonReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                dnR.GetNoonRList = CommonMethods.GetNoonReportList(vslid, currPage, pageSize);
                return View(dnR);
            }
            else if (firstVal == "" && dateF == "")
            {
                dnR.GetNoonRList = CommonMethods.GetNoonReportList(vslid, currPage, pageSize);
                return View(dnR);
            }
            else if (firstVal != null)
            {
                dnR.GetNoonRList = CommonMethods.SearchNoonReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchnoonR", dnR);
            }
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();

            //TempData["TotalRecords"] = noonRBind.;
            // dnR.GetNoonRList = dnR.GetNoonRList.Skip((10 * currPage) - 10).Take(10).ToList();

            return View(dnR);

            // return PartialView("_searchnoonR", dnR);
        }

        public ActionResult _searchnoonR(int? pageNo, string firstVal, string dateF, string dateT)
        {
            DailyNoonReport dnR = new DailyNoonReport();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["noonReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);


            dnR.GetNoonRList = CommonMethods.SearchNoonReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);

            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();
            TempData["CurrentPage"] = currPage;
            //TempData["TotalRecords"] = noonRBind.;
            // dnR.GetNoonRList = dnR.GetNoonRList.Skip((10 * currPage) - 10).Take(10).ToList();


            return PartialView("_searchnoonR", dnR);


        }

        //[HttpPost]
        //public ActionResult noonRlist(DailyNoonReport dnR)
        //{
        //    //DailyNoonReport dnR = new DailyNoonReport();
        //    int vslid = Convert.ToInt32(Session["VesselID"]);
        //    ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
        //    TempData["noonReportId"] = null;


        //    //int currPage = TempData["CurrentPage"] == null ? 1 : Convert.ToInt32(TempData["CurrentPage"]);

        //    int currPage = 1;


        //    TempData["CurrentPage"] = currPage;
        //    TempData["TotalRecords"] = 50;


        //    dnR.GetNoonRList = CommonMethods.GetNoonReportList(vslid);
        //    return View(dnR);
        //}

        [HttpPost]
        public ActionResult insertNoonR(DailyNoonReport _noonR)
        {

            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _noonR.VesselId = vslid;

            if (_noonR.Id == 0)
            {
                Session["NR_ID"] = "";

                using (SqlDataAdapter adp = new SqlDataAdapter("select date from DailyNoonReport where IsActive=1 and date='" + _noonR.Date + "' and VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //TempData["Error1"] = "This report date already exists !";
                        return Json(new { result = "Error", url = Url.Action("index", "DailyNoon") });
                    }
                }

                CommonMethods.InsertUpdateDailyNoonReport(_noonR, "Insert");
                if (_noonR.SaveDraft == false)
                {
                    TempData["Success"] = "Record saved successfully";

                }
            }
            if (_noonR.Id != 0)
            {
                // _noonR.Id = Convert.ToInt32(TempData["noonReportId"]);
                Session["NR_ID"] = _noonR.Id;
                CommonMethods.InsertUpdateDailyNoonReport(_noonR, "Update");
                if (_noonR.SaveDraft == false)
                {
                    TempData["Success"] = "Record updated successfully";
                }
            }

            //ViewBag.FConsumptionInsertion = string.Format("FConsumptionInsertion1('{0}');", 0);
            //ViewBag.CargoTankInsertion = string.Format("CargoTankInsertion('{0}');", 0);
            //ViewBag.BallastTankInsertion = string.Format("BallastTankInsertion('{0}');", 0);
            //ViewBag.VoidSpaceInsertion = string.Format("VoidSpaceInsertion('{0}');", 0);
            //ViewBag.FuelROBInsertion = string.Format("FuelROBInsertion('{0}');", 0);
            //ViewBag.NonRoutineInsertion = string.Format("NonRoutineInsertion('{0}');", 0);
            //ViewBag.NRCargoInsertion = string.Format("NRCargoInsertion('{0}');", 0);

            //return Json(_noonR);

            return Json(new { result = "Redirect", url = Url.Action("noonRlist", "DailyNoon") });
        }

        [HttpPost]
        public ActionResult Index(DailyNoonReport cls)
        {

            if (cls.Id == 0)
            {
                int vslid = Convert.ToInt32(Session["VesselID"]);
                ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
                cls.VesselId = vslid;


                CommonMethods.InsertUpdateDailyNoonReport(cls, "Insert");

                TempData["Success"] = "Record saved successfully";
            }
            if (cls.Id != 0)
            {
                CommonMethods.InsertUpdateDailyNoonReport(cls, "Update");
                TempData["Success"] = "Record update successfully ";
            }

            //return JavaScript(script);
            return RedirectToAction("noonRlist");
        }

        public ActionResult InsertFuelConsumption(string fuelconsumptions)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<FCons> FconsList = new List<FCons>();
            var Result = fuelconsumptions;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<FCons>>(dd);
            foreach (var rootObject in Json)
            {
                var FuelType = rootObject.FuelType;

                SqlDataAdapter adp = new SqlDataAdapter("select Id from tblFuelType where fueltype='" + FuelType + "'", ConnectionBulder.con);
                DataTable dt = new DataTable();
                adp.Fill(dt);

                int fueltypeid = Convert.ToInt32(dt.Rows[0][0]);

                var AE_ACT_BERTH = rootObject.AE_ACT_BERTH;
                var AE_ACT_MAN = rootObject.AE_ACT_MAN;
                var AE_ACT_SEA = rootObject.AE_ACT_SEA;
                var AE_ACT_WAIT = rootObject.AE_ACT_WAIT;
                var AE_CP = rootObject.AE_CP;
                var ME_ACT_BERTH = rootObject.ME_ACT_BERTH;
                var ME_ACT_MAN = rootObject.ME_ACT_MAN;
                var ME_ACT_SEA = rootObject.ME_ACT_SEA;
                var ME_ACT_WAIT = rootObject.ME_ACT_WAIT;
                var ME_CP = rootObject.ME_CP;
                var BLR_ACT_BERTH = rootObject.BLR_ACT_BERTH;
                var BLR_ACT_MAN = rootObject.BLR_ACT_MAN;
                var BLR_ACT_SEA = rootObject.BLR_ACT_SEA;
                var BLR_ACT_WAIT = rootObject.BLR_ACT_WAIT;
                var FRAMO_ACT_BERTH = rootObject.FRAMO_ACT_BERTH;
                var FRAMO_ACT_MAN = rootObject.FRAMO_ACT_MAN;
                var FRAMO_ACT_SEA = rootObject.FRAMO_ACT_SEA;
                var FRAMO_ACT_WAIT = rootObject.FRAMO_ACT_WAIT;


                var IGG = rootObject.IGG;
                var Incinerator = rootObject.Incinerator;
                var StopageAtSea = rootObject.StopageAtSea;
                var Deviation = rootObject.Deviation;
                var SlowSteaming = rootObject.SlowSteaming;
                var BadWeather = rootObject.BadWeather;
                var COTPrep = rootObject.COTPrep;
                var CargoHeating = rootObject.CargoHeating;
                var BWExchange = rootObject.BWExchange;
                var Others = rootObject.Others;


                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 1, vslid, "Insert", "NoonReport");

                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 1, vslid, "Insert", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 28, Incinerator, 1, vslid, "Insert", "NoonReport");
                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 1, ME_CP, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 2, ME_ACT_SEA, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 3, ME_ACT_MAN, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 4, ME_ACT_WAIT, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 5, ME_ACT_BERTH, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 6, AE_CP, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 7, AE_ACT_SEA, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 8, AE_ACT_MAN, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 9, AE_ACT_WAIT, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 10, AE_ACT_BERTH, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 11, BLR_ACT_SEA, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 12, BLR_ACT_MAN, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 13, BLR_ACT_WAIT, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 14, BLR_ACT_BERTH, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 15, FRAMO_ACT_SEA, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 16, FRAMO_ACT_MAN, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 17, FRAMO_ACT_WAIT, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 18, FRAMO_ACT_BERTH, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 19, IGG, 1, vslid, "Update", "NoonReport");

                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 20, StopageAtSea, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 21, Deviation, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 22, SlowSteaming, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 23, BadWeather, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 24, COTPrep, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 25, CargoHeating, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 26, BWExchange, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 27, Others, 1, vslid, "Update", "NoonReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 28, Incinerator, 1, vslid, "Update", "NoonReport");
                }



                //if (AE_ACT_BERTH.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, AE_ACT_BERTH,1, vslid, "Insert", "NoonReport");
                //}
                //if (AE_CP.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 1, vslid, "Insert", "NoonReport");
                //}
                //if (AE_ACT_SEA.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                //}
                //if (AE_ACT_MAN.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                //}
                //if (AE_ACT_WAIT.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                //}
                //if (AE_ACT_BERTH.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                //}
                //if (ME_ACT_BERTH.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                //}
                //if (ME_ACT_MAN.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                //}
                //if (ME_ACT_SEA.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                //}
                //if (ME_ACT_WAIT.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                //}
                //if (ME_CP.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 1, vslid, "Insert", "NoonReport");
                //}
                //if (BLR_ACT_SEA.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                //}
                //if (BLR_ACT_MAN.ToString() != "0.00")
                //{
                //  CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                //}
                //if (BLR_ACT_WAIT.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                //}
                //if (BLR_ACT_BERTH.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 1, vslid,  "Insert", "NoonReport");
                //}



                //if (FRAMO_ACT_SEA.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 1, vslid, "Insert", "NoonReport");
                //}
                //if (FRAMO_ACT_MAN.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 1, vslid, "Insert", "NoonReport");
                //}

                //if (FRAMO_ACT_WAIT.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 1, vslid, "Insert", "NoonReport");
                //}
                //if (FRAMO_ACT_BERTH.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 1, vslid, "Insert", "NoonReport");
                //}




                //if (IGG.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 1, vslid, "Insert", "NoonReport");
                //}

                //if (StopageAtSea.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 1, vslid, "Insert", "NoonReport");
                //}
                //if (Deviation.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 1, vslid, "Insert", "NoonReport");
                //}
                //if (SlowSteaming.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 1, vslid, "Insert", "NoonReport");
                //}
                //if (BadWeather.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 1, vslid, "Insert", "NoonReport");
                //}
                //if (COTPrep.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 1, vslid, "Insert", "NoonReport");
                //}
                //if (CargoHeating.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 1, vslid, "Insert", "NoonReport");
                //}

                //if (BWExchange.ToString() != "0.00")
                //{
                //CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 1, vslid, "Insert", "NoonReport");
                //}
                //if (Others.ToString() != "0.00")
                //{
                // CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 1, vslid, "Insert", "NoonReport");
                //}



                //var meAtSea= rootObject.ME_ACT_SEA
            }
            return View();
        }


        public ActionResult InsertNR_CargoTank(string cargotank)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<DNR_Cargo_Tank> DNR_Cargo_List = new List<DNR_Cargo_Tank>();
            var Result = cargotank;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<DNR_Cargo_Tank>>(dd);
            foreach (var rootObject in Json)
            {
                int CargoTank_Id = rootObject.CargoTank_Id;

                decimal Ullage = rootObject.Ullage;
                decimal Qty_MT = rootObject.Qty_MT;
                decimal Oxygen = rootObject.Oxygen;
                decimal H2S = rootObject.H2S;
                decimal HC = rootObject.HC;

                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateDNR_CargoTank(0, CargoTank_Id, Ullage, Qty_MT, Oxygen, H2S, HC, vslid, "Insert");
                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);
                    CommonMethods.InsertUpdateDNR_CargoTank(noonReportId, CargoTank_Id, Ullage, Qty_MT, Oxygen, H2S, HC, vslid, "Update");
                }

            }

            return View();
        }


        public static List<DNR_Cargo_Tank> GetCargoTankList(int noonRId, int vslid)
        {

            List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();


            //using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
            using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on a.CargoTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
            {
                DataTable dt1 = new DataTable();
                adp1.Fill(dt1);
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Cargo_Tank
                    {
                        CargoTank_Id = Convert.ToInt32(dt1.Rows[i]["CargoTank_Id"]),
                        TankName = dt1.Rows[i]["Name"].ToString(),
                        Ullage = 0.00m,
                        Qty_MT = 0.00m,
                        Oxygen = 0.00m,
                        H2S = 0.00m,
                        HC = 0.00m,
                    });
                }

            }

            //using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Cargo_Tank a inner join TanksAndHolds b on  a.CargoTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Cargo_Tank a inner join TanksAndHolds b on  a.CargoTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where a.DNR_Id=" + noonRId + " and b.TanksTypeId=1 and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                //if (dt.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        ftype.Add(new DNR_Cargo_Tank
                //        {
                //           // CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
                //           // TankName = dt.Rows[i]["Name"].ToString(),
                //            Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]),
                //            Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]),
                //            Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]),
                //            H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]),
                //            HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),
                //        });
                //    }
                //}
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int cargoTankId = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]);

                        DNR_Cargo_Tank existingTank = ftype.FirstOrDefault(tank => tank.CargoTank_Id == cargoTankId);

                        if (existingTank != null)
                        {

                            existingTank.Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]);
                            existingTank.Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]);
                            existingTank.Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]);
                            existingTank.H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]);
                            existingTank.HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]);
                        }
                        else
                        {
                            ftype.Add(new DNR_Cargo_Tank
                            {
                                CargoTank_Id = cargoTankId,
                                TankName = dt.Rows[i]["Name"].ToString(),
                                Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]),
                                Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]),
                                Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]),
                                H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]),
                                HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),
                            });
                        }
                    }
                }

            }

            return ftype;
        }


        //public static List<DNR_Cargo_Tank> GetCargoTankList(int noonRId, int vslid)
        //{

        //    List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();


        //    using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
        //    {
        //        DataTable dt1 = new DataTable();
        //        adp1.Fill(dt1);
        //        for (int i = 0; i < dt1.Rows.Count; i++)
        //        {
        //            ftype.Add(new DNR_Cargo_Tank
        //            {
        //                CargoTank_Id = Convert.ToInt32(dt1.Rows[i]["CargoTank_Id"]),
        //                TankName = dt1.Rows[i]["Name"].ToString(),
        //                Ullage = 0.00m,
        //                Qty_MT = 0.00m,
        //                Oxygen = 0.00m,
        //                H2S = 0.00m,
        //                HC = 0.00m,
        //            });
        //        }

        //    }

        //    using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Cargo_Tank a inner join TanksAndHolds b on  a.CargoTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
        //    {
        //        // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);
        //        if (dt.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                ftype.Add(new DNR_Cargo_Tank
        //                {
        //                    // CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
        //                    // TankName = dt.Rows[i]["Name"].ToString(),
        //                    Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]),
        //                    Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]),
        //                    Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]),
        //                    H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]),
        //                    HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),
        //                });
        //            }
        //        }


        //    }

        //    return ftype;
        //}


        public static List<DNR_Ballast_Tank> GetBallastTankList(int noonRId, int vslid)
        {
            List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();

            //using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
            using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on a.BallastTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
            {
                DataTable dt1 = new DataTable();
                adp1.Fill(dt1);
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Ballast_Tank
                    {
                        BallastTank_Id = Convert.ToInt32(dt1.Rows[i]["BallastTank_Id"]),
                        TankName = dt1.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,
                        Qty_Vol = 0.00m,
                        HC = 0.00m
                    });
                }
            }

            //using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Ballast_Tank a inner join TanksAndHolds b on a.BallastTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Ballast_Tank a inner join TanksAndHolds b on a.BallastTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where a.DNR_Id=" + noonRId + " and b.TanksTypeId=3 and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DNR_Ballast_Tank tankToUpdate = ftype.Find(t => t.BallastTank_Id == Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]));
                        if (tankToUpdate != null)
                        {
                            tankToUpdate.Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"] == DBNull.Value ? 0.00m : dt.Rows[i]["Sounding"]);
                            tankToUpdate.Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_Vol"]);
                            tankToUpdate.HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]);
                        }
                        else
                        {

                            ftype.Add(new DNR_Ballast_Tank
                            {
                                BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
                                TankName = dt.Rows[i]["Name"].ToString(),
                                Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"] == DBNull.Value ? 0.00m : dt.Rows[i]["Sounding"]),
                                Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_Vol"]),
                                HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"])
                            });
                        }
                    }
                }
            }

            return ftype;
        }


        //public static List<DNR_Ballast_Tank> GetBallastTankList(int noonRId, int vslid)
        //{
        //    List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();

        //    using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
        //    {
        //        DataTable dt1 = new DataTable();
        //        adp1.Fill(dt1);
        //        for (int i = 0; i < dt1.Rows.Count; i++)
        //        {
        //            ftype.Add(new DNR_Ballast_Tank
        //            {
        //                BallastTank_Id = Convert.ToInt32(dt1.Rows[i]["BallastTank_Id"]),
        //                TankName = dt1.Rows[i]["Name"].ToString(),
        //                Sounding = 0.00m,
        //                Qty_Vol = 0.00m,
        //                HC = 0.00m,

        //            });
        //        }

        //    }

        //    using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Ballast_Tank a inner join TanksAndHolds b on  a.BallastTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
        //    {
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);
        //        if (dt.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                ftype.Add(new DNR_Ballast_Tank
        //                {
        //                    // BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
        //                    // TankName = dt.Rows[i]["Name"].ToString(),
        //                    Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"] == DBNull.Value ? 0.00m : dt.Rows[i]["Sounding"]),
        //                    Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_Vol"]),
        //                    HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),

        //                });
        //            }
        //        }

        //    }

        //    return ftype;
        //}

        public static List<DNR_Void_Space> GetVoid_SpaceList(int noonRId, int vslid)
        {
            List<DNR_Void_Space> ftype = new List<DNR_Void_Space>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Void_Space a inner join TanksAndHolds b on a.Void_Space_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Void_Space a inner join TanksAndHolds b on a.Void_Space_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Void_Space
                    {
                        Void_Space_Id = Convert.ToInt32(dt.Rows[i]["Void_Space_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"]),

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public ActionResult InsertNR_BallastTank(string ballasttank)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<DNR_Ballast_Tank> DNR_Cargo_List = new List<DNR_Ballast_Tank>();
            var Result = ballasttank;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<DNR_Ballast_Tank>>(dd);
            foreach (var rootObject in Json)
            {
                int BallastTank_Id = rootObject.BallastTank_Id;

                decimal Sounding = rootObject.Sounding;
                decimal Qty_Vol = rootObject.Qty_Vol;
                decimal HC = rootObject.HC;




                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateDNR_BallastTank(0, BallastTank_Id, Sounding, Qty_Vol, HC, vslid, "Insert");
                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);
                    CommonMethods.InsertUpdateDNR_BallastTank(noonReportId, BallastTank_Id, Sounding, Qty_Vol, HC, vslid, "Update");
                }

            }

            return View();
        }

        public ActionResult InsertVoidSpaceSounding(string voidspacesounding)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<DNR_Void_Space> DNR_VoidSpace_List = new List<DNR_Void_Space>();
            var Result = voidspacesounding;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<DNR_Void_Space>>(dd);
            foreach (var rootObject in Json)
            {
                int Void_Space_Id = rootObject.Void_Space_Id;

                decimal? Sounding = rootObject.Sounding;




                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateDNR_VoidSpace(0, Void_Space_Id, Sounding, vslid, "Insert");
                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);
                    CommonMethods.InsertUpdateDNR_VoidSpace(noonReportId, Void_Space_Id, Sounding, vslid, "Update");
                }

            }

            return View();
        }



        public ActionResult InsertNREvents(string nonroutine)
        {
            int nrevntid = 1;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<NonRoutineEventClass> NREvents = new List<NonRoutineEventClass>();
            var Result = nonroutine;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<NonRoutineEventClass>>(dd);
            foreach (var rootObject in Json)
            {


                string ChartererAccount = rootObject.ChartererAccount;
                string Hours = rootObject.Hours;

                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateNonRoutineEvents(0, 1, nrevntid, ChartererAccount, Hours, vslid, "NoonReport", "Insert");
                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);
                    CommonMethods.InsertUpdateNonRoutineEvents(noonReportId, 1, nrevntid, ChartererAccount, Hours, vslid, "NoonReport", "Update");
                }

                nrevntid++;
            }
            return View();
        }

        public ActionResult Edit(int id)
        {

            DailyNoonReport vd = new DailyNoonReport();
            vd.GetNoonRList = CommonMethods.editnoonRList(id, "DailyNoonReport");
            var date = CommonMethods.editnoonR(id, "DailyNoonReport_SaveDraft");

            var noonRBind = vd.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["noonReportId"] = id;

            int vslid = Convert.ToInt32(Session["VesselID"]);
            var d_date = noonRBind.Date;
            DateTime givenDate = DateTime.Parse(d_date.ToString());
            string previousDate = givenDate.AddDays(-1).ToString("yyyy-MM-dd");
            string prevReportDate = "";

            if (date != "")
            {
                givenDate = DateTime.Parse(date.ToString());
                prevReportDate = givenDate.ToString("yyyy-MM-dd");
            }

            if (previousDate == prevReportDate)
            {
                noonRBind.IsSubmit = true;
            }
            else
            {
                noonRBind.IsSubmit = false;
            }
            string Rdate = "";
            string By_PassDate = "";

            using (SqlDataAdapter adp1 = new SqlDataAdapter("select Noon_date from NoonReport_allow where Vessel_Id=" + vslid + "", ConnectionBulder.con))
            {
                DataTable dtm1 = new DataTable();
                adp1.Fill(dtm1);
                if (dtm1.Rows.Count > 0)
                {
                    for (int j = 0; j < dtm1.Rows.Count; j++)
                    {
                        By_PassDate = NoonReport_dateDecode(dtm1.Rows[j]["Noon_date"].ToString());
                        // By_PassDate = Convert.ToDateTime(dtm1.Rows[0][0]).ToString("yyyy-MM-dd");

                        if (By_PassDate.Trim() == Convert.ToDateTime(noonRBind.Date).ToString("yyyy-MM-dd").Trim())
                        {
                            if (Rdate != Convert.ToDateTime(noonRBind.Date).ToString("yyyy-MM-dd").Trim())
                            {
                                noonRBind.IsSubmit = true;
                            }

                        }

                    }

                }
            }

            string[] returnvalue = CommonMethods.GetVesselName1(vslid);
            ViewBag.VesselName = returnvalue[0];
            ViewBag.Displacement = returnvalue[1];

            noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            noonRBind.CargoTanks = GetCargoTankList(id, vslid);
            noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = noonRBind.LegPortId;
            ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", id);
            ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", id);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);
            TempData["Edit"] = id;

            return View("Index", noonRBind);

        }

        public JsonResult GetDisByLeg(int LegId, int CPID)
        {
            using (SqlDataAdapter sda = new SqlDataAdapter("select COALESCE(SUM(NoonToNoonDMG_Dist),0) as totalsum ,COALESCE(SUM(stmgtime),0) as totaltime , (select dtg from VoyageLeg  where id=" + LegId + " and IsActive=1) as dtg,(select CP_SOG from VoyageLeg  where id=" + LegId + " and IsActive=1) as cpSpeed from dailynoonreport  where LegPortId=" + LegId + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                sda.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    decimal laden = 0; decimal ballast = 0;
                    int totalSum = Convert.ToInt32(dt.Rows[0]["totalsum"] == DBNull.Value ? 0 : dt.Rows[0]["totalsum"]);

                    ViewBag.TotalSum = 1234;

                    int dtg = Convert.ToInt32(dt.Rows[0]["dtg"] == DBNull.Value ? 0 : dt.Rows[0]["dtg"]);
                    decimal cpsped = Convert.ToDecimal(dt.Rows[0]["cpSpeed"] == DBNull.Value ? 0.00 : dt.Rows[0]["cpSpeed"]);
                    int totalTime = Convert.ToInt32(dt.Rows[0]["totaltime"] == DBNull.Value ? 0 : dt.Rows[0]["totaltime"]);

                    using (SqlDataAdapter pp = new SqlDataAdapter("select * from CPpart2 where cpid=" + CPID + " and Speed=" + cpsped + "", ConnectionBulder.con))
                    {
                        DataTable dp = new DataTable();
                        pp.Fill(dp);
                        if (dp.Rows.Count > 0)
                        {
                            laden = Convert.ToDecimal(dp.Rows[0]["ME_LADEN"]);
                            ballast = Convert.ToDecimal(dp.Rows[0]["ME_BALLAST"]);



                            ViewBag.Id5 = Convert.ToDecimal(dp.Rows[0]["AE_VLSFO"]);
                            ViewBag.Id6 = Convert.ToDecimal(dp.Rows[0]["AE_VLSFO"]);
                            ViewBag.Id7 = Convert.ToDecimal(dp.Rows[0]["AE_VLSFO"]);
                            ViewBag.Id8 = Convert.ToDecimal(dp.Rows[0]["AE_VLSFO"]);

                            ViewBag.Id29 = Convert.ToDecimal(dp.Rows[0]["AE_DO"]);
                            ViewBag.Id30 = Convert.ToDecimal(dp.Rows[0]["AE_DO"]);
                            ViewBag.Id31 = Convert.ToDecimal(dp.Rows[0]["AE_DO"]);
                            ViewBag.Id32 = Convert.ToDecimal(dp.Rows[0]["AE_DO"]);

                        }
                    }

                    using (SqlDataAdapter adpp = new SqlDataAdapter("select * from CPpart3 where cpid=" + CPID + " and isactive=1", ConnectionBulder.con))
                    {
                        DataTable dtt = new DataTable();
                        adpp.Fill(dtt);
                        if (dtt.Rows.Count > 0)
                        {
                            //ViewBag.Id5 = Convert.ToDecimal(dtt.Rows[2]["AE_VLSFO"]);
                            //ViewBag.Id6 = Convert.ToDecimal(dtt.Rows[3]["AE_VLSFO"]);
                            //ViewBag.Id7 = Convert.ToDecimal(dtt.Rows[4]["AE_VLSFO"]);
                            //ViewBag.Id8 = Convert.ToDecimal(dtt.Rows[5]["AE_VLSFO"]);

                            //ViewBag.Id29 = Convert.ToDecimal(dtt.Rows[2]["BOILER_MDO"]);
                            //ViewBag.Id30 = Convert.ToDecimal(dtt.Rows[3]["BOILER_MDO"]);
                            //ViewBag.Id31 = Convert.ToDecimal(dtt.Rows[4]["BOILER_MDO"]);
                            //ViewBag.Id32 = Convert.ToDecimal(dtt.Rows[5]["BOILER_MDO"]);

                            //ViewBag.Id33 = Convert.ToDecimal(dtt.Rows[2]["AE_MDO"]);
                            //ViewBag.Id34 = Convert.ToDecimal(dtt.Rows[3]["AE_MDO"]);
                            //ViewBag.Id35 = Convert.ToDecimal(dtt.Rows[4]["AE_MDO"]);
                            //ViewBag.Id36 = Convert.ToDecimal(dtt.Rows[5]["AE_MDO"]);
                        }
                    }


                    return Json(new
                    {
                        Result = true,
                        ttlsum = totalSum,
                        ttltime = totalTime,
                        DTG = dtg,
                        CPSpeed = cpsped,
                        Laden = laden,
                        Ballast = ballast,
                        Id5 = ViewBag.Id5,
                        Id6 = ViewBag.Id6,
                        Id7 = ViewBag.Id7,
                        Id8 = ViewBag.Id8,
                        Id29 = ViewBag.Id29,
                        Id30 = ViewBag.Id30,
                        Id31 = ViewBag.Id31,
                        Id32 = ViewBag.Id32,
                        Id33 = ViewBag.Id33,
                        Id34 = ViewBag.Id34,
                        Id35 = ViewBag.Id35,
                        Id36 = ViewBag.Id36,
                    }, JsonRequestBehavior.AllowGet);
                }
            }




            return Json(new { Result = true, ttlsum = "", DTG = "", Message = "" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getfuelcons(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList arrName = new ArrayList();
            ArrayList CpValue = new ArrayList();
            ArrayList FCValue = new ArrayList();
            ArrayList FRobValue = new ArrayList();
            ArrayList FBunkerValue = new ArrayList();

            IList<string> ft = new List<string>();
            IList<string> cp = new List<string>();
            IList<string> fc = new List<string>();
            IList<string> frob = new List<string>();

            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.FuelType from FuelConsumption a inner join tblFuelType b on a.FuelTypeId=b.Id and a.IsActive=1 and a.VoyageId=" + 9 + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        arrName.Add(dt.Rows[i]["FuelType"]);
                        CpValue.Add(dt.Rows[i]["CP_cons_perday_HFO"]);


                    }

                    ViewBag.Ftype = arrName;
                    ViewBag.CPValue = CpValue;

                }
                ;

                // Edit for Fuel Consumption
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=1  and ConsTypeId not in (1,6)", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    int k = 0;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int gg = dt.Rows.Count;

                        if (i == 25)
                        {
                            if (dt.Rows[i]["ConsTypeId"].ToString() != "28")
                            {
                                FCValue.Add(0.000);
                            }
                        }

                        k++;

                        FCValue.Add(dt.Rows[i]["value"]);

                        if (gg == 50 && k == 50)
                        {
                            if (dt.Rows[i]["ConsTypeId"].ToString() != "28")
                            {
                                FCValue.Add(0.000);
                            }
                            // FCValue.Add(0.000);
                        }

                    }

                    ViewBag.FcValue = FCValue;
                }
                ;

                // Edit for Fuel Rob
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select otherrob from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=1", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FRobValue.Add(dt.Rows[i]["otherrob"]);
                    }
                    ViewBag.FrobValue = FRobValue;
                }
                ;

                // Edit for Receiver
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Receipt from tbl_BunkerLReceipt where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=1", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FBunkerValue.Add(dt.Rows[i]["Receipt"]);
                    }
                    ViewBag.FBunkerValue = FBunkerValue;
                }
                ;


            }
            catch { }

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, fc = ViewBag.FcValue, frob = ViewBag.FrobValue, fBunker = ViewBag.FBunkerValue }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNREvents(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList charterer = new ArrayList();
            ArrayList hours = new ArrayList();
            IList<string> chrterer = new List<string>();
            IList<string> hrs = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select * from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + NoonReportId + " and VesselId=" + vslid + " and IsActive=1", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        charterer.Add(dt.Rows[i]["ChartererAccount"]);
                        hours.Add(dt.Rows[i]["Hours"]);
                    }

                    ViewBag.ChartererA = charterer;
                    ViewBag.Hours = hours;

                }
                ;
            }
            catch { }

            return Json(new { Result = true, chrterer = ViewBag.ChartererA, hrs = ViewBag.Hours }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNRCargoEdit(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList nrCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vslid + " and a.NoonReport_Id=" + NoonReportId + "", ConnectionBulder.con))
                    {
                        objCMD.Fill(dt);
                    }
                }
                catch
                {
                    using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.lr_cargo_id=b.Id where a.VesselId=" + vslid + " and a.noonreport_id=" + NoonReportId + "", ConnectionBulder.con))
                    {
                        objCMD.Fill(dt);
                    }
                }

                ViewBag.CountNRCargo = dt.Rows.Count;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string colCargo = dt.Columns.Contains("CargoName") ? "CargoName" : "cargoname";
                    string colPort = dt.Columns.Contains("PortName") ? "PortName" : "PortName";
                    string colBL = dt.Columns.Contains("BL_Qty") ? "BL_Qty" : "BL_Qty";
                    string colLoad = dt.Columns.Contains("LoadPortalActual") ? "LoadPortalActual" : "LoadPortalActual";
                    string colToday = dt.Columns.Contains("TodaysActual") ? "TodaysActual" : "TodaysActual";
                    string colQtyDiff = dt.Columns.Contains("Qty_Diff") ? "Qty_Diff" : "Qty_Diff";
                    string colReason = dt.Columns.Contains("Reasonfor_Qty_Diff") ? "Reasonfor_Qty_Diff" : "Reasonfor_Qty_Diff";
                    string colTemp = dt.Columns.Contains("Cargo_Temp") ? "Cargo_Temp" : "Cargo_Temp";
                    string colLR = dt.Columns.Contains("LR_Cargo_Id") ? "LR_Cargo_Id" : "lr_cargo_id";
                    string cName = (dt.Rows[i][colCargo] == DBNull.Value || dt.Rows[i][colCargo].ToString().Trim() == "") ? "Cargo" : dt.Rows[i][colCargo].ToString();
                    string pName = dt.Rows[i][colPort] == DBNull.Value ? "" : dt.Rows[i][colPort].ToString();
                    nrCargoName.Add(cName + " ( " + pName + " ) ");
                    nrCargoName.Add(FormatCargoDecimal(dt.Rows[i][colBL]));
                    nrCargoName.Add(FormatCargoDecimal(dt.Rows[i][colLoad]));
                    nrCargoName.Add(FormatCargoDecimal(dt.Rows[i][colToday]));
                    nrCargoName.Add(FormatCargoDecimal(dt.Rows[i][colQtyDiff]));
                    nrCargoName.Add(dt.Rows[i][colReason] == DBNull.Value ? "" : dt.Rows[i][colReason]);
                    nrCargoName.Add(FormatCargoDecimal(dt.Rows[i][colTemp]));
                    nrCargoName.Add(dt.Rows[i][colLR] == DBNull.Value ? 0 : dt.Rows[i][colLR]);
                }

                ViewBag.NRCargoList = nrCargoName;
            }
            catch { }

            return Json(new { Result = true, nrc = ViewBag.NRCargoList, cntnrc = ViewBag.CountNRCargo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNR_Cargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                //using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId , a.PortName from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        // cargoName.Add(dt.Rows[i]["CargoName"]);
                        cargoName.Add(dt.Rows[i]["CargoName"] == DBNull.Value ? "" : dt.Rows[i]["CargoName"].ToString() + " ( " +
                         (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ");
                    }
                    ViewBag.CName = cargoName;
                }
                ;
            }
            catch { }

            return Json(new { Result = true, cn = ViewBag.CName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNRCargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                //using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId  from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.LegPortId=" + LegId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        cargoName.Add(dt.Rows[i]["CargoName"]);
                    }
                    ViewBag.CName = cargoName;
                }
                ;
            }
            catch { }

            return Json(new { Result = true, cn = ViewBag.CName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditGetfuelcons(int VoyId)
        {
            ArrayList arrName = new ArrayList();
            ArrayList CpValue = new ArrayList();

            IList<string> ft = new List<string>();
            IList<string> cp = new List<string>();

            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*, b.fueltype,c.cons_type from Fuel_Cons_NR a left join tblFuelType b on a.FuelTypeId=b.Id inner join  tblConsType c on a.ConsTypeId=c.Id where  Noon_Report_Id=31 and a.ConsTypeId not in (1,6)", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        arrName.Add(dt.Rows[i]["FuelType"]);
                        CpValue.Add(dt.Rows[i]["CP_cons_perday_HFO"]);


                    }

                    ViewBag.Ftype = arrName;
                    ViewBag.CPValue = CpValue;

                }
                ;


            }
            catch { }

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id)
        {
            CommonMethods.CommonDelete(id, "DailyNoonReport");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("noonRlist");
        }
        public JsonResult BindLeg1(int Voyid)
        {
            var legportList = CommonMethods.bindleg(Voyid);



            return Json(new { Result = true, Data = legportList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindLeg(int Voyid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            int cpid = 0;
            try
            {

                using (SqlDataAdapter adp = new SqlDataAdapter("select cpid from VoyageDetails where id=" + Voyid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        cpid = Convert.ToInt32(dt.Rows[0][0]);
                    }
                }


                //using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "'", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["leg"].ToString(), Value = tbl.Rows[i]["id"].ToString() });

                        }

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, Data = jst, CPID = cpid }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertBunkerRec(string bunkerRec)
        {
            try
            {
                int k = 0; int NRID = 0;
                if (Session["NR_ID"].ToString() != "")
                {
                    NRID = Convert.ToInt32(Session["NR_ID"]);
                }
                int vslid = Convert.ToInt32(Session["VesselID"]);
                List<FuelROB> FrobList = new List<FuelROB>();
                var Result = bunkerRec;
                string json = Result.ToString();
                string dd = json;
                var Json = JsonConvert.DeserializeObject<List<FuelROB>>(dd);
                foreach (var rootObject in Json)
                {
                    var FuelType = rootObject.FuelType;

                    SqlDataAdapter adp = new SqlDataAdapter("select Id from tblFuelType where fueltype='" + FuelType + "'", ConnectionBulder.con);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    int fueltypeid = Convert.ToInt32(dt.Rows[0][0]);

                    var Rec = rootObject.Rec;


                    if (k == 0)
                    {
                        if (Session["NR_ID"].ToString() != "")
                        {
                            using (SqlDataAdapter adp1 = new SqlDataAdapter("delete from tbl_BunkerLReceipt where TableMax_Id=" + NRID + " and VesselId=" + vslid + " and ReportType_Id=1", ConnectionBulder.con))
                            {
                                DataTable dt1 = new DataTable();
                                adp1.Fill(dt1);
                                k = 1;
                            }

                        }
                    }

                    if (Session["NR_ID"].ToString() == "")
                    {

                        CommonMethods.InsertUpdateBunkerReceipt(0, fueltypeid, Rec, 1, vslid, "Insert", "NoonReport");



                    }
                    if (Session["NR_ID"].ToString() != "")
                    {

                        CommonMethods.InsertUpdateBunkerReceipt(NRID, fueltypeid, Rec, 1, vslid, "Insert", "NoonReport");

                    }

                }
            }
            catch { }
            return View();
        }

        public ActionResult InsertFuelROB(string fuelrob)
        {
            try
            {
                int vslid = Convert.ToInt32(Session["VesselID"]);
                List<FuelROB> FrobList = new List<FuelROB>();
                var Result = fuelrob;
                string json = Result.ToString();
                string dd = json;
                var Json = JsonConvert.DeserializeObject<List<FuelROB>>(dd);
                foreach (var rootObject in Json)
                {
                    var FuelType = rootObject.FuelType;

                    SqlDataAdapter adp = new SqlDataAdapter("select Id from tblFuelType where fueltype='" + FuelType + "'", ConnectionBulder.con);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    int fueltypeid = Convert.ToInt32(dt.Rows[0][0]);

                    var Rec = rootObject.Rec;




                    if (Session["NR_ID"].ToString() == "")
                    {
                        CommonMethods.InsertUpdateFuelROB(0, fueltypeid, 0, 0, Rec, 1, vslid, "Insert", "NoonReport");

                    }
                    if (Session["NR_ID"].ToString() != "")
                    {
                        int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                        CommonMethods.InsertUpdateFuelROB(noonReportId, fueltypeid, 0, 0, Rec, 1, vslid, "Update", "NoonReport");

                    }



                }
            }
            catch { }
            return View();
        }

        public ActionResult InsertNRCargo(string nrcargo)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<NRCargo> FrobList = new List<NRCargo>();
            var Result = nrcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<NRCargo>>(dd);

            int k = 0;
            foreach (var rootObject in Json)
            {

                NRCargo cls = new NRCargo();

                var Cname = rootObject.CargoName;
                string portName = "";
                var VoyId = rootObject.VoyId;
                int LR_CargoID = 0;
                int lrcrgid = 0;
                int startIndex = Cname.IndexOf('(');
                int endIndex = Cname.IndexOf(')');

                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    portName = Cname.Substring(startIndex + 1, endIndex - startIndex - 1);
                    portName = portName.Trim();
                }
                Cname = Regex.Replace(Cname, @"\s+\(.*\)", "");

                if (Cname != "")
                {
                    //  if (rootObject.LR_Cargo_Id == 0)
                    //  {

                    //SqlDataAdapter adp = new SqlDataAdapter("select Id from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + "", ConnectionBulder.con);
                    SqlDataAdapter adp = new SqlDataAdapter("select MAX(Id) from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + " and VoyageId=" + VoyId + " and PortName='" + portName + "'", ConnectionBulder.con);

                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                    lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                    // }
                    //else
                    //{
                    //    lrcrgid = rootObject.LR_Cargo_Id;
                    //}
                    cls.LR_Cargo_Id = lrcrgid;
                    //cls.LR_Cargo_Id = rootObject.LR_Cargo_Id;
                    cls.VesselId = vslid;
                    cls.BL_Qty = rootObject.BL_Qty;
                    cls.LoadPortalActual = rootObject.LoadPortalActual;
                    cls.TodaysActual = rootObject.TodaysActual;
                    cls.Qty_Diff = rootObject.Qty_Diff;
                    cls.Reasonfor_Qty_Diff = rootObject.Reasonfor_Qty_Diff;
                    cls.Cargo_Temp = rootObject.Cargo_Temp;
                    // cls.Id = rootObject.Id;

                }


                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateNRCargo(0, cls, "Insert", "NoonReport");

                }
                if (Session["NR_ID"].ToString() != "")
                {

                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    if (k == 0)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from NR_Cargo where NoonReport_Id=" + noonReportId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }
                    }

                    //CommonMethods.InsertUpdateNRCargo(noonReportId, cls, "Update", "NoonReport");
                    CommonMethods.InsertUpdateNRCargo(noonReportId, cls, "Insert", "NoonReport");
                }



            }
            return View();
        }

        public JsonResult CheckLastNoonR(int LegId, string Sdate)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            int msg = 1;
            string Rdate = "";
            bool saveDraft = false;
            string Mdate = "";
            string By_PassDate = "";
            try
            {
                DateTime dts = Convert.ToDateTime(Sdate);
                string dt = dts.AddDays(-1).ToString("yyyy-MM-dd");
                //using (SqlDataAdapter adpvv = new SqlDataAdapter("select Date from DailyNoonReport where IsActive=1  and Date = '" + dt + "' and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter adpvv = new SqlDataAdapter("select Date, SaveDraft from DailyNoonReport where IsActive=1  and Date = '" + dt + "' and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        DateTime Ldate = Convert.ToDateTime(dtvv.Rows[0][0]);
                        saveDraft = Convert.ToBoolean(dtvv.Rows[0][1]);
                        msg = 0;

                    }
                    // else
                    //  {

                    //using (SqlDataAdapter adp = new SqlDataAdapter("select max(Date) from DailyNoonReport where IsActive=1  and  LegPortId=" + LegId + "  and VesselId=" + vslid + "", ConnectionBulder.con))
                    using (SqlDataAdapter adp = new SqlDataAdapter("select Date, SaveDraft from DailyNoonReport where IsActive=1  and Date = '" + Sdate + "' and VesselId=" + vslid + "", ConnectionBulder.con))
                    {

                        DataTable dtm = new DataTable();
                        adp.Fill(dtm);
                        if (dtm.Rows.Count > 0)
                        {
                            //Rdate = Convert.ToDateTime(dtm.Rows[0][0]).ToString("yyyy-MM-dd");
                            Rdate = dtm.Rows[0][0].ToString();
                            if (string.IsNullOrEmpty(Rdate))
                            {
                                msg = 1;
                            }
                            else
                            {
                                Rdate = Convert.ToDateTime(dtm.Rows[0][0]).ToString("yyyy-MM-dd");
                                msg = 2;
                                saveDraft = Convert.ToBoolean(dtm.Rows[0][1]);
                            }

                        }
                        //else
                        //{
                        //    msg = 0;
                        //}

                    }


                    using (SqlDataAdapter adp2 = new SqlDataAdapter("select max(Date) from DailyNoonReport where IsActive=1  and Date <= '" + Sdate + "' and VesselId=" + vslid + "", ConnectionBulder.con))
                    {

                        DataTable dtm2 = new DataTable();
                        adp2.Fill(dtm2);
                        if (dtm2.Rows.Count > 0)
                        {
                            Mdate = Convert.ToDateTime(dtm2.Rows[0][0]).ToString("yyyy-MM-dd");

                            using (SqlDataAdapter adp3 = new SqlDataAdapter("select SaveDraft from DailyNoonReport where IsActive=1  and Date = '" + Mdate + "' and VesselId=" + vslid + "", ConnectionBulder.con))
                            {

                                DataTable dtm3 = new DataTable();
                                adp3.Fill(dtm3);
                                if (dtm3.Rows.Count > 0)
                                {
                                    saveDraft = Convert.ToBoolean(dtm3.Rows[0][0] == DBNull.Value ? false : dtm3.Rows[0][0]);
                                }
                            }
                        }

                    }

                    if (msg == 0)
                    {
                        using (SqlDataAdapter adp2 = new SqlDataAdapter("select Date from DailyNoonReport where IsActive=1  and Date = '" + dt + "' and VesselId=" + vslid + " and SaveDraft = 1", ConnectionBulder.con))
                        {

                            DataTable dtm3 = new DataTable();
                            adp2.Fill(dtm3);
                            if (dtm3.Rows.Count > 0)
                            {
                                Mdate = Convert.ToDateTime(dtm3.Rows[0][0]).ToString("yyyy-MM-dd");
                                msg = 3;
                            }

                        }
                    }

                    using (SqlDataAdapter adp1 = new SqlDataAdapter("select Noon_date from NoonReport_allow where Vessel_Id=" + vslid + "", ConnectionBulder.con))
                    {
                        DataTable dtm1 = new DataTable();
                        adp1.Fill(dtm1);
                        if (dtm1.Rows.Count > 0)
                        {
                            for (int j = 0; j < dtm1.Rows.Count; j++)
                            {
                                By_PassDate = NoonReport_dateDecode(dtm1.Rows[j]["Noon_date"].ToString());
                                // By_PassDate = Convert.ToDateTime(dtm1.Rows[0][0]).ToString("yyyy-MM-dd");

                                if (By_PassDate.Trim() == Sdate.Trim())
                                {
                                    if (Rdate != Sdate.Trim())
                                    {
                                        msg = 0;
                                    }

                                }

                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Json(new { Result = true, Data = msg, Rdate = Mdate, saveDraft = saveDraft }, JsonRequestBehavior.AllowGet);
        }

        public string NoonReport_dateDecode(string date)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(date);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string decryptdate = new String(decoded_char);
            return decryptdate;
        }

        public JsonResult GetDToGo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            decimal VoyageLeg_DTG = 0.00m;
            decimal AvgSpeed = 0.00m; decimal CP_Log_Speed = 0.00m; decimal TotalDistance = 0.00m;
            try
            {
                //select AVG(Act_Speed) from DailyNoonReport where VoyageId=16 and LegPortId=19 and VesselId=1234567
                using (SqlDataAdapter adpvv = new SqlDataAdapter("select DTG,CP_Log_Speed from VoyageLeg where VoyageId=" + VoyageId + " and id=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        VoyageLeg_DTG = Convert.ToDecimal(dtvv.Rows[0]["DTG"]);
                        CP_Log_Speed = Convert.ToDecimal(dtvv.Rows[0]["CP_Log_Speed"]);
                    }

                }

                using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed),sum(NoonToNoonDMG_Dist) as TtlDistance  from DailyNoonReport where IsActive=1 and SaveDraft=0 and VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        AvgSpeed = Convert.ToDecimal(dtvv.Rows[0][0]);
                        TotalDistance = Convert.ToDecimal(dtvv.Rows[0][1]);
                    }

                }
            }
            catch { }
            SpeedDistance Data = new SpeedDistance()
            {
                DistToGo = VoyageLeg_DTG,
                GenAvgSpeed = AvgSpeed,
                CP_Log_Speed = CP_Log_Speed,
                TotalDistance = TotalDistance,
            };

            return Json(new { Result = true, Data = Data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SyncReportToGetEmail(int VoyageId, int id, string ReportDate)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);

            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand("USP_GetEmailBYvessel", ConnectionBulder.con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VesselId", vslid);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            var emailList = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                emailList.Add(new
                {
                    Id = row["Id"],
                    EmailTo = row["EmailTo"],
                    EmailCC = row["EmailCC"],
                    VesselId = row["VesselId"]
                });
            }

            return Json(new { success = true, data = emailList }, JsonRequestBehavior.AllowGet);
        }



        //public ActionResult SyncReportDownload(int VoyageId, int id, string ReportDate)
        //{
        //    int vslid = Convert.ToInt32(Session["VesselID"]);
        //    DataTable dt = new DataTable();

        //    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
        //    {
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@VoyageId", VoyageId);
        //        cmd.Parameters.AddWithValue("@ReportDate", ReportDate);
        //        cmd.Parameters.AddWithValue("@VesselId", vslid);
        //        cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
        //        cmd.Parameters.AddWithValue("@id", id);

        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        da.Fill(dt);
        //    }

        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        var ws = wb.Worksheets.Add(dt, "DailyNoonReport");
        //        foreach (DataColumn col in dt.Columns)
        //        {
        //            if (col.DataType == typeof(DateTime))
        //            {
        //                ws.Column(col.Ordinal + 1).Style.DateFormat.Format = "dd-MMM-yyyy";
        //            }
        //        }
        //        ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //        using (MemoryStream stream = new MemoryStream())
        //        {
        //            wb.SaveAs(stream);
        //            stream.Position = 0;

        //            string fileName = "DailyNoonReport_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";

        //            return File(
        //                stream.ToArray(),
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                fileName
        //            );
        //        }
        //    }
        //}

        /// <param name="tabFilter">Optional: "Navigation", "Engine", or "Cargo" - export only that tab's sheet. Empty = all tabs.</param>
        public ActionResult SyncReportDownload(int VoyageId, int id, string ReportDate, string tabFilter = "")
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@VoyageId", VoyageId);
                cmd.Parameters.AddWithValue("@ReportDate", ReportDate);
                cmd.Parameters.AddWithValue("@VesselId", vslid);
                cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
                cmd.Parameters.AddWithValue("@id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            DailyNoonReport noonRBind = null;
            List<DNR_Cargo_Tank> cargoTanks = new List<DNR_Cargo_Tank>();
            List<DNR_Ballast_Tank> ballastTanks = new List<DNR_Ballast_Tank>();
            List<DNR_Void_Space> voidSpaces = new List<DNR_Void_Space>();
            DataTable dtFuelCons = new DataTable();
            DataTable dtFuelROB = new DataTable();
            DataTable dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable();
            DataTable dtNRCargo = new DataTable();

            try
            {
                var vd = new DailyNoonReport();
                vd.GetNoonRList = CommonMethods.editnoonRList(id, "DailyNoonReport");
                noonRBind = vd.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
                if (noonRBind != null)
                {
                    cargoTanks = GetCargoTankList(id, vslid);
                    ballastTanks = GetBallastTankList(id, vslid);
                    voidSpaces = GetVoid_SpaceList(id, vslid);

                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vslid + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    {
                        adp.Fill(dtFuelCons);
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vslid + " and a.ReportType_Id=1", ConnectionBulder.con))
                    {
                        adp.Fill(dtFuelROB);
                    }
                    // Fallback: if join returns empty, use same query as edit page (Getfuelcons) and pair with FuelType
                    if (dtFuelROB.Rows.Count == 0)
                    {
                        try
                        {
                            DataTable dtFuelTypes = new DataTable();
                            using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            {
                                adp.Fill(dtFuelTypes);
                            }
                            using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, OtherROB from tbl_FuelROB where TableMax_Id=" + id + " and VesselId=" + vslid + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                            {
                                DataTable dtRob = new DataTable();
                                adp.Fill(dtRob);
                                foreach (DataRow r in dtRob.Rows)
                                {
                                    int ftId = Convert.ToInt32(r["FuelType_Id"]);
                                    var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                                    string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                                    string val = r["OtherROB"]?.ToString() ?? "";
                                    dtFuelROB.Rows.Add(fuelType, val);
                                }
                            }
                        }
                        catch { }
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vslid + " and a.ReportType_Id=1", ConnectionBulder.con))
                    {
                        adp.Fill(dtBunker);
                    }
                    // Fallback for Bunker when join returns empty
                    if (dtBunker.Rows.Count == 0)
                    {
                        try
                        {
                            DataTable dtFuelTypes = new DataTable();
                            using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            {
                                adp.Fill(dtFuelTypes);
                            }
                            using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vslid + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                            {
                                DataTable dtBunk = new DataTable();
                                adp.Fill(dtBunk);
                                foreach (DataRow r in dtBunk.Rows)
                                {
                                    int ftId = Convert.ToInt32(r["FuelType_Id"]);
                                    var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                                    string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                                    dtBunker.Rows.Add(fuelType, r["Receipt"]?.ToString());
                                }
                            }
                        }
                        catch { }
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vslid + " and IsActive=1 order by Id", ConnectionBulder.con))
                    {
                        adp.Fill(dtNonRoutine);
                    }
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vslid + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                        {
                            adp.Fill(dtNRCargo);
                        }
                    }
                    catch
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.lr_cargo_id=b.Id where a.VesselId=" + vslid + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                        {
                            adp.Fill(dtNRCargo);
                        }
                    }
                }
            }
            catch { }

            using (XLWorkbook wb = new XLWorkbook())
            {
                var tab = (tabFilter ?? "").Trim();
                if (string.IsNullOrEmpty(tab) || tab.Equals("Navigation", StringComparison.OrdinalIgnoreCase))
                    AddNavigationSheet(wb, noonRBind, dtNonRoutine, dt);
                if (string.IsNullOrEmpty(tab) || tab.Equals("Engine", StringComparison.OrdinalIgnoreCase))
                    AddEngineSheet(wb, noonRBind, dtFuelCons, dtFuelROB, dtBunker);
                if (string.IsNullOrEmpty(tab) || tab.Equals("Cargo", StringComparison.OrdinalIgnoreCase))
                    AddCargoSheet(wb, noonRBind, cargoTanks, ballastTanks, voidSpaces, dtNRCargo);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = "DailyNoonReport" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
        }

        /// <summary>
        /// Navigation sheet - ONLY fields from Navigation tab (as shown in form): Header info, Speed-Distance-Time, Non-Routine Events, Weather, Remarks
        /// No Engine or Cargo columns - explicit whitelist to match form layout exactly.
        /// </summary>
        private void AddNavigationSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ApplyGridTitleStyle(rngNav);
            row += 2;

            // Header section - ONLY fields shown above tabs in form (explicit whitelist, no dtMain loop to avoid wrong columns)
            if (r != null)
            {
                string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
                string legText = "";
                string portStatusText = r.PortStatus?.ToString() ?? "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
                    if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                    if (dtMain.Columns.Contains("Status") && string.IsNullOrEmpty(portStatusText)) portStatusText = dr["Status"]?.ToString() ?? portStatusText;
                }
                AddKeyValueSingle(ws, ref row, "Voy No.", voyNo);
                AddKeyValueSingle(ws, ref row, "Status", r.VesselStatus ?? "");
                AddKeyValueSingle(ws, ref row, "Latitude", r.Latitude ?? "");
                AddKeyValueSingle(ws, ref row, "Longitude", r.Longitude ?? "");
                AddKeyValueSingle(ws, ref row, "At Sea/In Port", r.AtSeaOrPort ?? "");
                AddKeyValueSingle(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueSingle(ws, ref row, "Displacement(MT)", r.Displacement);
                AddKeyValueSingle(ws, ref row, "CP Speed(Kts)", r.CP_Speed);
                AddKeyValueSingle(ws, ref row, "Leg", legText);
                AddKeyValueSingle(ws, ref row, "Report Date", r.Date != null ? Convert.ToDateTime(r.Date).ToString(CommonMethods.ExcelDateFormat) : "");
                AddKeyValueSingle(ws, ref row, "ETA", r.ETA != null ? Convert.ToDateTime(r.ETA).ToString(CommonMethods.ExcelDateTimeFormat) : "");
                AddKeyValueSingle(ws, ref row, "Draft Fwd(Mtrs)", r.DraftFwd);
                AddKeyValueSingle(ws, ref row, "Draft Mid(Mtrs)", r.DraftMid);
                AddKeyValueSingle(ws, ref row, "Draft Aft(Mtrs)", r.DraftAft);
            }
            row++;

            // Speed - Distance - Time (Navigation tab only)

            ws.Cell(row, 1).Value = "Speed - Distance - Time";
            var rngSpeed = ws.Range(row, 1, row, 3);
            rngSpeed.Merge();
            rngSpeed.Style.Font.Bold = true;
            rngSpeed.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngSpeed.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngSpeed);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist);
                AddKeyValueSingle(ws, ref row, "Log Dist(NM)", r.LogDist);
                AddKeyValueSingle(ws, ref row, "Engine Dist(NM)", r.EngineDist);
                AddKeyValueSingle(ws, ref row, "Total Distance (Dep to Curr)(NM)", r.TotalDistance);
                AddKeyValueSingle(ws, ref row, "Dist to Go (DTG)(NM)", r.DistToGo_DTG);
                AddKeyValueSingle(ws, ref row, "Stmg Time Noon to Noon(Hrs)", r.StmgTime);
                AddKeyValueSingle(ws, ref row, "Total Time (Dep to Curr)(Hrs)", r.TotalTime);
                AddKeyValueSingle(ws, ref row, "Actual Speed Noon to Noon(Kts)", r.Act_Speed);
                AddKeyValueSingle(ws, ref row, "Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed);
            }
            row++;

            // Non-Routine Events (Navigation tab only)
            ws.Cell(row, 1).Value = "Non-Routine Events";
            var rngNRE = ws.Range(row, 1, row, 3);
            rngNRE.Merge();
            rngNRE.Style.Font.Bold = true;
            rngNRE.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngNRE.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngNRE);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                ws.Cell(row, 3).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["Hours"]?.ToString() ?? "") : "";
                row++;
            }
            row++;

            // Weather (Navigation tab only)
            ws.Cell(row, 1).Value = "Weather";
            var rngWeather = ws.Range(row, 1, row, 3);
            rngWeather.Merge();
            rngWeather.Style.Font.Bold = true;
            rngWeather.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngWeather.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngWeather);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueSingle(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueSingle(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueSingle(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueSingle(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueSingle(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueSingle(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            // Noon Report Remarks (Navigation tab only)
            ws.Cell(row, 1).Value = "Noon Report Remarks";
            var rngRemarks = ws.Range(row, 1, row, 3);
            rngRemarks.Merge();
            rngRemarks.Style.Font.Bold = true;
            rngRemarks.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngRemarks.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngRemarks);
            row++;
            AddKeyValueSingle(ws, ref row, "Remarks", r?.Remarks ?? "");

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Engine sheet - ONLY fields from Engine tab: Engine params, Fuel ROB, Bunker, Other ROB, Aux Engine, Boilers, LO/HO, E/R Tanks, Fuel Consumption
        /// </summary>
        private void AddEngineSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Engine";
            var rngEngineHdr = ws.Range(row, 1, row, 3);
            rngEngineHdr.Merge();
            rngEngineHdr.Style.Font.Bold = true;
            rngEngineHdr.Style.Font.FontSize = 16;
            rngEngineHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ApplyGridTitleStyle(rngEngineHdr);
            row += 2;

            // Engine section
            ws.Cell(row, 1).Value = "Engine";
            var rngEngine = ws.Range(row, 1, row, 3);
            rngEngine.Merge();
            rngEngine.Style.Font.Bold = true;
            rngEngine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngEngine.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngEngine);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueSingle(ws, ref row, "RPM", r.RPM);
                AddKeyValueSingle(ws, ref row, "BHP(hp)", r.BHP);
                AddKeyValueSingle(ws, ref row, "MCR%", r.MCR);
                AddKeyValueSingle(ws, ref row, "M/E Control Location", r.ME_ControlLoc);
                AddKeyValueSingle(ws, ref row, "SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress);
                AddKeyValueSingle(ws, ref row, "SCAV. Temp (Deg Centigrade)", r.SCAV_Temp);
                AddKeyValueSingle(ws, ref row, "Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp);
                AddKeyValueSingle(ws, ref row, "Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp);
                AddKeyValueSingle(ws, ref row, "SW Temp (Deg Centigrade)", r.SW_Temp);
                AddKeyValueSingle(ws, ref row, "ER Temp (Deg Centigrade)", r.ER_Temp);
            }
            row++;

            // Fuel ROB in MT
            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            var rngFuelROB = ws.Range(row, 1, row, 3);
            rngFuelROB.Merge();
            rngFuelROB.Style.Font.Bold = true;
            rngFuelROB.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngFuelROB.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngFuelROB);
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string robVal = dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"]?.ToString() : (dr.Table.Columns.Contains("OtherRob") ? dr["OtherRob"]?.ToString() : (dr.Table.Columns.Contains("otherrob") ? dr["otherrob"]?.ToString() : ""));
                    AddKeyValueSingle(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal ?? "");
                }
            }
            row++;

            // Bunker Received in MT
            ws.Cell(row, 1).Value = "Bunker Received in MT";
            var rngBunker = ws.Range(row, 1, row, 3);
            rngBunker.Merge();
            rngBunker.Style.Font.Bold = true;
            rngBunker.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngBunker.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngBunker);
            row++;
            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
            {
                    AddKeyValueSingle(ws, ref row, dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "");
                }
            }
            row++;

            // Other ROB (match form: columns Full, In Use, Empty; rows Oxygen (Bottles), Acetylene (Bottles))
            ws.Cell(row, 1).Value = "Other ROB";
            var rngOtherROB = ws.Range(row, 1, row, 4);
            rngOtherROB.Merge();
            rngOtherROB.Style.Font.Bold = true;
            rngOtherROB.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngOtherROB.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngOtherROB);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.OT_ROB_OXY_Full;
                ws.Cell(row, 3).Value = r.OT_ROB_OXY_InUse;
                ws.Cell(row, 4).Value = r.OT_ROB_OXY_Empty;
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.OT_ROB_ACYT_Full;
                ws.Cell(row, 3).Value = r.OT_ROB_ACYT_InUse;
                ws.Cell(row, 4).Value = r.OT_ROB_ACYT_Empty;
                row++;
            }
            row++;

            // Aux. Engine
            ws.Cell(row, 1).Value = "Aux. Engine";
            var rngAux = ws.Range(row, 1, row, 3);
            rngAux.Merge();
            rngAux.Style.Font.Bold = true;
            rngAux.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngAux.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngAux);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Running Hrs No.1", r.AE_RungHrs_No1);
                AddKeyValueSingle(ws, ref row, "Running Hrs No.2", r.AE_RungHrs_No2);
                AddKeyValueSingle(ws, ref row, "Running Hrs No.3", r.AE_RungHrs_No3);
                AddKeyValueSingle(ws, ref row, "Running Hrs No.4", r.AE_RungHrs_No4);
                AddKeyValueSingle(ws, ref row, "Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen);
                AddKeyValueSingle(ws, ref row, "Load No.1 (KW)", r.AE_Load_No1);
                AddKeyValueSingle(ws, ref row, "Load No.2 (KW)", r.AE_Load_No2);
                AddKeyValueSingle(ws, ref row, "Load No.3 (KW)", r.AE_Load_No3);
                AddKeyValueSingle(ws, ref row, "Load No.4 (KW)", r.AE_Load_No4);
                AddKeyValueSingle(ws, ref row, "Load Shaft Gen (KW)", r.AE_Load_ShaftGen);
                AddKeyValueSingle(ws, ref row, "Extra Run Reason", r.AE_Extra_Run_Reason);
            }
            row++;

            // Boiler's
            ws.Cell(row, 1).Value = "Boiler's";
            var rngBoiler = ws.Range(row, 1, row, 3);
            rngBoiler.Merge();
            rngBoiler.Style.Font.Bold = true;
            rngBoiler.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngBoiler.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngBoiler);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Boiler No.1 Running Hrs", r.BR_RungHrs_No1);
                AddKeyValueSingle(ws, ref row, "Boiler No.2 Running Hrs", r.BR_RungHrs_No2);
                AddKeyValueSingle(ws, ref row, "Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1);
                AddKeyValueSingle(ws, ref row, "Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2);
            }
            row++;

            // LO & HO Consumptions (match form: columns Consumption, ROB; rows MECC, MECYL, AECC, HYDRAULIC Oil)
            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            var rngLOHO = ws.Range(row, 1, row, 3);
            rngLOHO.Merge();
            rngLOHO.Style.Font.Bold = true;
            rngLOHO.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngLOHO.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngLOHO);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Consumption";
                ws.Cell(row, 3).Value = "ROB";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "MECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.LO_HO_Cons_MECC;
                ws.Cell(row, 3).Value = r.LO_HO_Cons_MECC_ROB;
                row++;
                ws.Cell(row, 1).Value = "MECYL (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.LO_HO_Cons_MECYL;
                ws.Cell(row, 3).Value = r.LO_HO_Cons_MECYL_ROB;
                row++;
                ws.Cell(row, 1).Value = "AECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.LO_HO_Cons_AECC;
                ws.Cell(row, 3).Value = r.LO_HO_Cons_AECC_ROB;
                row++;
                ws.Cell(row, 1).Value = "HYDRAULIC Oil (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.LO_HO_Cons_HYDR_Oil;
                ws.Cell(row, 3).Value = r.LO_HO_Cons_HYDR_Oil_ROB;
                row++;
            }
            row++;

            // E/R Tanks (match form: row "ROB (m3)" with columns Bilge, Sludge, Waste Oil)
            ws.Cell(row, 1).Value = "E/R Tanks";
            var rngERTanks = ws.Range(row, 1, row, 4);
            rngERTanks.Merge();
            rngERTanks.Style.Font.Bold = true;
            rngERTanks.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngERTanks.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngERTanks);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Bilge";
                ws.Cell(row, 3).Value = "Sludge";
                ws.Cell(row, 4).Value = "Waste Oil";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.ER_Bilge_ROB;
                ws.Cell(row, 3).Value = r.ER_Sludge_ROB;
                ws.Cell(row, 4).Value = r.ER_WasteOil_ROB;
                row++;
            }
            row++;

            // Fuel Consumption in MT (match form tblFuelC - 3-level header: Main Engine, Aux Eng, Boiler, Framo System, IGG, Incinerator, Events, TOTAL)
            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            var rngFuelCons = ws.Range(row, 1, row, 30);
            rngFuelCons.Merge();
            rngFuelCons.Style.Font.Bold = true;
            rngFuelCons.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngFuelCons);
            row++;
            // Row 1 - Level 1 (Main Engine, Aux Eng, Boiler, Framo System, IGG, Incinerator, Events, TOTAL) - match edit page design
            ws.Cell(row, 1).Value = "";
            ws.Range(row, 2, row, 6).Merge();
            ws.Cell(row, 2).Value = "Main Engine";
            ws.Range(row, 7, row, 11).Merge();
            ws.Cell(row, 7).Value = "Aux Eng";
            ws.Range(row, 12, row, 15).Merge();
            ws.Cell(row, 12).Value = "Boiler";
            ws.Range(row, 16, row, 19).Merge();
            ws.Cell(row, 16).Value = "Framo System";
            ws.Cell(row, 20).Value = "IGG";
            ws.Cell(row, 21).Value = "Incinerator";
            ws.Range(row, 22, row, 29).Merge();
            ws.Cell(row, 22).Value = "Events";
            ws.Range(row, 30, row + 2, 30).Merge();
            ws.Cell(row, 30).Value = "TOTAL";
            ws.Range(row, 1, row, 29).Style.Fill.BackgroundColor = XLColor.FromArgb(218, 218, 218);
            ws.Cell(row, 30).Style.Fill.BackgroundColor = XLColor.FromArgb(218, 218, 218);
            ws.Range(row, 1, row, 30).Style.Font.Bold = true;
            ws.Range(row, 1, row, 30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;
            // Row 2 - Level 2 (ACT under each main category)
            ws.Cell(row, 1).Value = "";
            ws.Range(row, 2, row, 6).Merge();
            ws.Cell(row, 2).Value = "ACT";
            ws.Range(row, 7, row, 11).Merge();
            ws.Cell(row, 7).Value = "ACT";
            ws.Range(row, 12, row, 15).Merge();
            ws.Cell(row, 12).Value = "ACT";
            ws.Range(row, 16, row, 19).Merge();
            ws.Cell(row, 16).Value = "ACT";
            ws.Cell(row, 20).Value = "";
            ws.Cell(row, 21).Value = "";
            ws.Cell(row, 22).Value = "Stoppage at Sea";
            ws.Cell(row, 23).Value = "Deviation";
            ws.Cell(row, 24).Value = "Slow Steaming";
            ws.Cell(row, 25).Value = "Bad Weather";
            ws.Cell(row, 26).Value = "COT Prep";
            ws.Cell(row, 27).Value = "Cargo Heating";
            ws.Cell(row, 28).Value = "BW Exchange";
            ws.Cell(row, 29).Value = "Others";
            ws.Range(row, 1, row, 30).Style.Font.Bold = true;
            ws.Range(row, 1, row, 30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;
            // Row 3 - Level 3 (AT SEA, MANOEUV, ANCHOR/WAIT, BERTH, SUB TOTAL per category)
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "AT SEA";
            ws.Cell(row, 3).Value = "MANOEUV";
            ws.Cell(row, 4).Value = "ANCHOR/WAIT";
            ws.Cell(row, 5).Value = "BERTH";
            ws.Cell(row, 6).Value = "SUB TOTAL";
            ws.Cell(row, 7).Value = "AT SEA";
            ws.Cell(row, 8).Value = "MANOEUV";
            ws.Cell(row, 9).Value = "ANCHOR/WAIT";
            ws.Cell(row, 10).Value = "BERTH";
            ws.Cell(row, 11).Value = "SUB TOTAL";
            ws.Cell(row, 12).Value = "AT SEA";
            ws.Cell(row, 13).Value = "MANOEUV";
            ws.Cell(row, 14).Value = "ANCHOR/WAIT";
            ws.Cell(row, 15).Value = "BERTH";
            ws.Cell(row, 16).Value = "AT SEA";
            ws.Cell(row, 17).Value = "MANOEUV";
            ws.Cell(row, 18).Value = "ANCHOR/WAIT";
            ws.Cell(row, 19).Value = "BERTH";
            ws.Cell(row, 20).Value = "";
            ws.Cell(row, 21).Value = "";
            ws.Cell(row, 22).Value = "";
            ws.Cell(row, 23).Value = "";
            ws.Cell(row, 24).Value = "";
            ws.Cell(row, 25).Value = "";
            ws.Cell(row, 26).Value = "";
            ws.Cell(row, 27).Value = "";
            ws.Cell(row, 28).Value = "";
            ws.Cell(row, 29).Value = "";
            ws.Range(row, 1, row, 30).Style.Font.Bold = true;
            ws.Range(row, 1, row, 30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;
            WriteDailyNoonFuelConsRow(ws, ref row, "VLSFO", dtFuelCons);
            WriteDailyNoonFuelConsRow(ws, ref row, "MDO", dtFuelCons);

            ws.Columns().AdjustToContents();
        }

        /// <summary>Writes one Fuel Consumption row (VLSFO or MDO) using dtFuelCons - same formula as edit page.</summary>
        private void WriteDailyNoonFuelConsRow(IXLWorksheet ws, ref int row, string fuelType, DataTable dtFuelCons)
        {
            var vals = GetFuelConsByFuelType(dtFuelCons, fuelType);
            decimal meSub = vals[0] + vals[1] + vals[2] + vals[3];
            decimal aeSub = vals[4] + vals[5] + vals[6] + vals[7];
            decimal total = meSub + aeSub + vals[8] + vals[9] + vals[10] + vals[11] + vals[12] + vals[13] + vals[14] + vals[15] + vals[16] + vals[25] + vals[17] + vals[18] + vals[19] + vals[20] + vals[21] + vals[22] + vals[23] + vals[24];
            ws.Cell(row, 1).Value = fuelType;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = FormatFuelConsDecimal(vals[0]);
            ws.Cell(row, 3).Value = FormatFuelConsDecimal(vals[1]);
            ws.Cell(row, 4).Value = FormatFuelConsDecimal(vals[2]);
            ws.Cell(row, 5).Value = FormatFuelConsDecimal(vals[3]);
            ws.Cell(row, 6).Value = FormatFuelConsDecimal(meSub);
            ws.Cell(row, 7).Value = FormatFuelConsDecimal(vals[4]);
            ws.Cell(row, 8).Value = FormatFuelConsDecimal(vals[5]);
            ws.Cell(row, 9).Value = FormatFuelConsDecimal(vals[6]);
            ws.Cell(row, 10).Value = FormatFuelConsDecimal(vals[7]);
            ws.Cell(row, 11).Value = FormatFuelConsDecimal(aeSub);
            ws.Cell(row, 12).Value = FormatFuelConsDecimal(vals[8]);
            ws.Cell(row, 13).Value = FormatFuelConsDecimal(vals[9]);
            ws.Cell(row, 14).Value = FormatFuelConsDecimal(vals[10]);
            ws.Cell(row, 15).Value = FormatFuelConsDecimal(vals[11]);
            ws.Cell(row, 16).Value = FormatFuelConsDecimal(vals[12]);
            ws.Cell(row, 17).Value = FormatFuelConsDecimal(vals[13]);
            ws.Cell(row, 18).Value = FormatFuelConsDecimal(vals[14]);
            ws.Cell(row, 19).Value = FormatFuelConsDecimal(vals[15]);
            ws.Cell(row, 20).Value = FormatFuelConsDecimal(vals[16]);
            ws.Cell(row, 21).Value = FormatFuelConsDecimal(vals[25]);
            ws.Cell(row, 22).Value = FormatFuelConsDecimal(vals[17]);
            ws.Cell(row, 23).Value = FormatFuelConsDecimal(vals[18]);
            ws.Cell(row, 24).Value = FormatFuelConsDecimal(vals[19]);
            ws.Cell(row, 25).Value = FormatFuelConsDecimal(vals[20]);
            ws.Cell(row, 26).Value = FormatFuelConsDecimal(vals[21]);
            ws.Cell(row, 27).Value = FormatFuelConsDecimal(vals[22]);
            ws.Cell(row, 28).Value = FormatFuelConsDecimal(vals[23]);
            ws.Cell(row, 29).Value = FormatFuelConsDecimal(vals[24]);
            ws.Cell(row, 30).Value = FormatFuelConsDecimal(total);
            row++;
        }

        private static string FormatFuelConsDecimal(decimal val)
        {
            return val.ToString("0.000");
        }

        /// <summary>Get fuel consumption values for a fuel type. Returns 26 values in column order: ME_ACT_SEA,MAN,WAIT,BERTH, AE_ACT_SEA,MAN,WAIT,BERTH, BLR_ACT_SEA,MAN,WAIT,BERTH, FRAMO_ACT_SEA,MAN,WAIT,BERTH, IGG, StopageAtSea,Deviation,SlowSteaming,BadWeather,COTPrep,CargoHeating,BWExchange,Others, Incinerator.</summary>
        private decimal[] GetFuelConsByFuelType(DataTable dt, string fuelType)
        {
            var result = new decimal[26];
            if (dt == null || dt.Rows.Count == 0) return result;
            int[] consTypeIds = { 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
            for (int i = 0; i < consTypeIds.Length; i++)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase) &&
                        Convert.ToInt32(dr["ConsTypeId"]) == consTypeIds[i])
                    {
                        var val = dr["Value"];
                        result[i] = val != null && val != DBNull.Value ? Convert.ToDecimal(val) : 0m;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Cargo sheet - ONLY fields from Cargo tab: Cargo, Slops ROB, Ballast, Void Spaces, Fresh Water, Other Soundings, Cargo Tanks, Ballast Tanks
        /// </summary>
        private void AddCargoSheet(XLWorkbook wb, DailyNoonReport r, List<DNR_Cargo_Tank> cargoTanks, List<DNR_Ballast_Tank> ballastTanks, List<DNR_Void_Space> voidSpaces, DataTable dtNRCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Cargo";
            var rngCargoHdr = ws.Range(row, 1, row, 3);
            rngCargoHdr.Merge();
            rngCargoHdr.Style.Font.Bold = true;
            rngCargoHdr.Style.Font.FontSize = 16;
            rngCargoHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ApplyGridTitleStyle(rngCargoHdr);
            row += 2;

            // Cargo (match form: columns B/L QTY, Load Portal Actual, Today's Actual, QTY diff, Reason for QTY diff, Cargo Temp.)
            ws.Cell(row, 1).Value = "Cargo";
            var rngCargo = ws.Range(row, 1, row, 7);
            rngCargo.Merge();
            rngCargo.Style.Font.Bold = true;
            rngCargo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngCargo);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "B/L QTY";
            ws.Cell(row, 3).Value = "Load Portal Actual";
            ws.Cell(row, 4).Value = "Today's Actual";
            ws.Cell(row, 5).Value = "QTY diff b/w Load Portal & Today";
            ws.Cell(row, 6).Value = "Reason for QTY diff";
            ws.Cell(row, 7).Value = "Cargo Temp.";
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            row++;
            if (dtNRCargo != null && dtNRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtNRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    ws.Cell(row, 2).Value = FormatCargoDecimal(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    ws.Cell(row, 3).Value = FormatCargoDecimal(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    ws.Cell(row, 4).Value = FormatCargoDecimal(dr.Table.Columns.Contains("TodaysActual") ? dr["TodaysActual"] : null);
                    ws.Cell(row, 5).Value = FormatCargoDecimal(dr.Table.Columns.Contains("Qty_Diff") ? dr["Qty_Diff"] : null);
                    ws.Cell(row, 6).Value = dr.Table.Columns.Contains("Reasonfor_Qty_Diff") ? (dr["Reasonfor_Qty_Diff"]?.ToString() ?? "") : "";
                    ws.Cell(row, 7).Value = FormatCargoDecimal(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
                    row++;
                }
            }
            else
            {
                ws.Cell(row, 1).Value = "Cargo";
                ws.Cell(row, 2).Value = "0.00";
                ws.Cell(row, 3).Value = "0.00";
                ws.Cell(row, 4).Value = "0.00";
                ws.Cell(row, 5).Value = "0.00";
                ws.Cell(row, 6).Value = "";
                ws.Cell(row, 7).Value = "0.00";
                row++;
            }
            row++;

            // Slops ROB (match form: columns Oil, Water, Total; row ROB (m3))
            ws.Cell(row, 1).Value = "Slops ROB";
            var rngSlops = ws.Range(row, 1, row, 4);
            rngSlops.Merge();
            rngSlops.Style.Font.Bold = true;
            rngSlops.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngSlops.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngSlops);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = r.SLOPS_ROB_OXY_Oil;
                ws.Cell(row, 3).Value = r.SLOPS_ROB_OXY_Water;
                ws.Cell(row, 4).Value = r.SLOPS_ROB_OXY_Total;
                row++;
            }
            row++;

            // Ballast
            ws.Cell(row, 1).Value = "Ballast";
            var rngBallast = ws.Range(row, 1, row, 3);
            rngBallast.Merge();
            rngBallast.Style.Font.Bold = true;
            rngBallast.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngBallast.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngBallast);
            row++;
            AddKeyValueSingle(ws, ref row, "ROB (MT)", r?.Ballast_ROB);
            row++;

            // Void Spaces Soundings
            ws.Cell(row, 1).Value = "Void Spaces Soundings in mtrs";
            var rngVoid = ws.Range(row, 1, row, 3);
            rngVoid.Merge();
            rngVoid.Style.Font.Bold = true;
            rngVoid.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngVoid.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngVoid);
            row++;
            foreach (var vs in voidSpaces ?? new List<DNR_Void_Space>())
            {
                AddKeyValueSingle(ws, ref row, vs.TankName ?? "", vs.Sounding);
            }
            row++;

            // Fresh Water
            ws.Cell(row, 1).Value = "Fresh Water";
            var rngFW = ws.Range(row, 1, row, 3);
            rngFW.Merge();
            rngFW.Style.Font.Bold = true;
            rngFW.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngFW.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngFW);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueSingle(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueSingle(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            row++;

            // Other Soundings
            ws.Cell(row, 1).Value = "Other Soundings in mtrs";
            var rngOther = ws.Range(row, 1, row, 3);
            rngOther.Merge();
            rngOther.Style.Font.Bold = true;
            rngOther.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngOther.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ApplyGridTitleStyle(rngOther);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Pump Room bilge max sounding", r.PumpRoomMaxSounding);
                AddKeyValueSingle(ws, ref row, "Chain Locker 1", r.ChainLocker1);
                AddKeyValueSingle(ws, ref row, "Chain Locker 2", r.ChainLocker2);
            }
            row++;

            // Cargo Tanks (match form: row "Tank" with tank names as columns; rows Ullage, MT Qty, Oxygen, H2S, HC)
            ws.Cell(row, 1).Value = "Cargo Tanks";
            var rngCTanks = ws.Range(row, 1, row, Math.Max(2, (cargoTanks?.Count ?? 0) + 1));
            rngCTanks.Merge();
            rngCTanks.Style.Font.Bold = true;
            rngCTanks.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngCTanks);
            row++;
            var cTanks = cargoTanks ?? new List<DNR_Cargo_Tank>();
            if (cTanks.Count > 0)
            {
                ws.Cell(row, 1).Value = "Tank";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].TankName ?? "";
                ws.Range(row, 1, row, cTanks.Count + 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Ullage (mtrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].Ullage;
                row++;
                ws.Cell(row, 1).Value = "MT Qty";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].Qty_MT;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].Oxygen;
                row++;
                ws.Cell(row, 1).Value = "H2S (PPM)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].H2S;
                row++;
                ws.Cell(row, 1).Value = "HC (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].HC;
                row++;
            }
            row++;

            // Ballast Tanks (match form: row "Tank" with tank names; rows Sounding, Cubic Vol, HC)
            ws.Cell(row, 1).Value = "Ballast Tanks";
            var rngBTanks = ws.Range(row, 1, row, Math.Max(2, (ballastTanks?.Count ?? 0) + 1));
            rngBTanks.Merge();
            rngBTanks.Style.Font.Bold = true;
            rngBTanks.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngBTanks);
            row++;
            var bTanks = ballastTanks ?? new List<DNR_Ballast_Tank>();
            if (bTanks.Count > 0)
            {
                ws.Cell(row, 1).Value = "Tank";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = bTanks[c].TankName ?? "";
                ws.Range(row, 1, row, bTanks.Count + 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Sounding (mtrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = bTanks[c].Sounding;
                row++;
                ws.Cell(row, 1).Value = "Cubic Vol";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = bTanks[c].Qty_Vol;
                row++;
                ws.Cell(row, 1).Value = "HC (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = bTanks[c].HC;
                row++;
            }

            ws.Columns().AdjustToContents();
        }

        private void AddKeyValueSingle(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = "";
            ws.Cell(row, 3).Value = value != null ? value.ToString() : "";
            row++;
        }

        private void AddKeyValue(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 2).Value = value != null ? value.ToString() : "";
            row++;
        }

        /// <summary>Format cargo decimal for grid display - ensures 0 shows as "0.00".</summary>
        private static string FormatCargoDecimal(object val)
        {
            if (val == null || val == DBNull.Value) return "0.00";
            decimal d;
            if (decimal.TryParse(val.ToString(), out d)) return d.ToString("0.00");
            return "0.00";
        }

        /// <summary>Apply grey background and white text to grid title range.</summary>
        private static void ApplyGridTitleStyle(IXLRange range)
        {
            range.Style.Fill.BackgroundColor = XLColor.Gray;
            range.Style.Font.FontColor = XLColor.White;
        }
    }


}


public class SpeedDistance
{
    public decimal DistToGo { get; set; }
    public decimal GenAvgSpeed { get; set; }
    public decimal CP_Log_Speed { get; set; }
    public decimal TotalDistance { get; set; }
}