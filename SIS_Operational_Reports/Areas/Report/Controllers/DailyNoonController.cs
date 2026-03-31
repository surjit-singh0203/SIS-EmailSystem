using DataBuildingLayer;
using Newtonsoft.Json;
using SIS_Operational_Reports.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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


            TempData["noonReportId"] = null;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);

            string[] returnvalue = CommonMethods.GetVesselName1(vslid);
            ViewBag.VesselName = returnvalue[0];
            ViewBag.Displacement = returnvalue[1];
            decimal CP_Speed = 0.00m;
            dnR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);


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
                dnR.GetNoonRList = CommonMethods.editnoonRList(id,0, "DailyNoonReport");
                var noonRBind = dnR.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
                noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
                noonRBind.LegPortList = BindLegPR(noonRBind.VoyageId);

                ViewBag.PreviousR = 1;
                noonRBind.CP_Speed = CP_Speed;
                using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed) from DailyNoonReport where IsActive=1 and SaveDraft=0 and VoyageId=" + noonRBind.VoyageId + " and LegPortId=" + noonRBind.LegPortId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        noonRBind.Gen_Avg_Speed = Convert.ToDecimal(dtvv.Rows[0][0]);
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
                return View(noonRBind);
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
        public ActionResult noonRlist(int? pageNo, string firstVal, string dateF, string dateT, string Vessel)
        {
            DailyNoonReport dnR = new DailyNoonReport();
            dnR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if(vslid.ToString() == "")
            {
                vslid= Convert.ToString(Session["VesselID"]);
            }

            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["noonReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                dnR.GetNoonRList = CommonMethods.GetNoonReportList(vslid, currPage, pageSize);
                dnR.VesselId = Convert.ToInt32(Vessel);
                return View(dnR);
            }
            else if (firstVal == "" && dateF == "" && Vessel == "")
            {
                dnR.GetNoonRList = CommonMethods.GetNoonReportList(vslid, currPage, pageSize);
                dnR.VesselId = Convert.ToInt32(Vessel);
                return View(dnR);
            }
            else if (firstVal != null || dateF != "" || Vessel != "")
            {
                dnR.GetNoonRList = CommonMethods.SearchNoonReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                dnR.VesselId = Convert.ToInt32(Vessel);
                return PartialView("_searchnoonR", dnR);
            }
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();

            //TempData["TotalRecords"] = noonRBind.;
            // dnR.GetNoonRList = dnR.GetNoonRList.Skip((10 * currPage) - 10).Take(10).ToList();

            return View(dnR);

            // return PartialView("_searchnoonR", dnR);
        }

        public ActionResult _searchnoonR(int? pageNo, string firstVal, string dateF, string dateT, string Vessel)
        {
            DailyNoonReport dnR = new DailyNoonReport();
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
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

            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _noonR.VesselId = vslid;

            if (_noonR.Id == 0)
            {
                Session["NR_ID"] = "";
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



            //return Json(_noonR);

            return Json(new { result = "Redirect", url = Url.Action("noonRlist", "DailyNoon") });
        }

        [HttpPost]
        public ActionResult Index(DailyNoonReport cls)
        {

            if (cls.Id == 0)
            {
                //int vslid = Convert.ToInt32(Session["VesselID"]);
                int vslid = Convert.ToInt32(Session["EditVesselID"]);
                
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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

            }
            return View();
        }


        public ActionResult InsertNR_CargoTank(string cargotank)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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

        //public static List<DNR_Cargo_Tank> GetCargoTankList(int noonRId, int vslid)
        //{

        //    List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();


        //    using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1 and b.VesselId=" + vslid + "", ConnectionBulder.con))
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
        //        //if (dt.Rows.Count > 0)
        //        //{
        //        //    for (int i = 0; i < dt.Rows.Count; i++)
        //        //    {
        //        //        ftype.Add(new DNR_Cargo_Tank
        //        //        {
        //        //           // CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
        //        //           // TankName = dt.Rows[i]["Name"].ToString(),
        //        //            Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]),
        //        //            Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]),
        //        //            Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]),
        //        //            H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]),
        //        //            HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),
        //        //        });
        //        //    }
        //        //}
        //        if (dt.Rows.Count > 0)
        //        {
        //            for (int i = 0; i < dt.Rows.Count; i++)
        //            {
        //                int cargoTankId = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]);

        //                DNR_Cargo_Tank existingTank = ftype.FirstOrDefault(tank => tank.CargoTank_Id == cargoTankId);

        //                if (existingTank != null)
        //                {

        //                    existingTank.Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]);
        //                    existingTank.Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]);
        //                    existingTank.Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]);
        //                    existingTank.H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]);
        //                    existingTank.HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]);
        //                }
        //                else
        //                {
        //                    ftype.Add(new DNR_Cargo_Tank
        //                    {
        //                        CargoTank_Id = cargoTankId,
        //                        TankName = dt.Rows[i]["Name"].ToString(),
        //                        Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"] == DBNull.Value ? 0.00m : dt.Rows[i]["Ullage"]),
        //                        Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_MT"]),
        //                        Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"] == DBNull.Value ? 0.00m : dt.Rows[i]["Oxygen"]),
        //                        H2S = Convert.ToDecimal(dt.Rows[i]["H2S"] == DBNull.Value ? 0.00m : dt.Rows[i]["H2S"]),
        //                        HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]),
        //                    });
        //                }
        //            }
        //        }

        //    }

        //    return ftype;
        //}


        public static List<DNR_Cargo_Tank> GetCargoTankList(int noonRId, int vslid)
        {

            List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Cargo_Tank a inner join TanksAndHolds b on  a.CargoTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))

            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Cargo_Tank a inner join TanksAndHolds b on  a.CargoTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where b.TanksTypeId=1 and a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Cargo_Tank
                    {
                        CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Ullage = Convert.ToDecimal(dt.Rows[i]["Ullage"]),
                        Qty_MT = Convert.ToDecimal(dt.Rows[i]["Qty_MT"]),
                        Oxygen = Convert.ToDecimal(dt.Rows[i]["Oxygen"]),
                        H2S = Convert.ToDecimal(dt.Rows[i]["H2S"]),
                        HC = Convert.ToDecimal(dt.Rows[i]["HC"]),
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        //public static List<DNR_Ballast_Tank> GetBallastTankList(int noonRId, int vslid)
        //{
        //    List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();

        //    using (SqlDataAdapter adp1 = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1 and b.VesselId=" + vslid + "", ConnectionBulder.con))
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
        //                HC = 0.00m
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
        //                DNR_Ballast_Tank tankToUpdate = ftype.Find(t => t.BallastTank_Id == Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]));
        //                if (tankToUpdate != null)
        //                {
        //                    tankToUpdate.Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"] == DBNull.Value ? 0.00m : dt.Rows[i]["Sounding"]);
        //                    tankToUpdate.Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_Vol"]);
        //                    tankToUpdate.HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"]);
        //                }
        //                else
        //                {

        //                    ftype.Add(new DNR_Ballast_Tank
        //                    {
        //                        BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
        //                        TankName = dt.Rows[i]["Name"].ToString(),
        //                        Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"] == DBNull.Value ? 0.00m : dt.Rows[i]["Sounding"]),
        //                        Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"] == DBNull.Value ? 0.00m : dt.Rows[i]["Qty_Vol"]),
        //                        HC = Convert.ToDecimal(dt.Rows[i]["HC"] == DBNull.Value ? 0.00m : dt.Rows[i]["HC"])
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return ftype;
        //}

        public static List<DNR_Ballast_Tank> GetBallastTankList(int noonRId, int vslid)
        {
            List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();
            //  using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Ballast_Tank a inner join TanksAndHolds b on a.BallastTank_Id = b.Id where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))

            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Ballast_Tank a inner join TanksAndHolds b on a.BallastTank_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where b.TanksTypeId=3 and a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Ballast_Tank
                    {
                        BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = Convert.ToDecimal(dt.Rows[i]["Sounding"]),
                        Qty_Vol = Convert.ToDecimal(dt.Rows[i]["Qty_Vol"]),
                        HC = Convert.ToDecimal(dt.Rows[i]["HC"]),

                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Void_Space> GetVoid_SpaceList(int noonRId, int vslid)
        {
            List<DNR_Void_Space> ftype = new List<DNR_Void_Space>();
           // using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,b.Name from NR_Void_Space a inner join TanksAndHolds b on  a.Void_Space_Id = b.Id  where a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.Name from NR_Void_Space a inner join TanksAndHolds b on  a.Void_Space_Id = b.Id and a.VesselId=b.VesselId Left join TanksType c on b.TanksTypeId=c.Id where b.TanksTypeId=9 and a.DNR_Id=" + noonRId + " and b.IsActive=1 and a.VesselId=" + vslid + "", ConnectionBulder.con))
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            List<DNR_Void_Space> DNR_VoidSpace_List = new List<DNR_Void_Space>();
            var Result = voidspacesounding;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<DNR_Void_Space>>(dd);
            foreach (var rootObject in Json)
            {
                int Void_Space_Id = rootObject.Void_Space_Id;

                decimal Sounding = rootObject.Sounding;


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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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

        public ActionResult Edit(int id,int vesselid)
        {
            DailyNoonReport vd = new DailyNoonReport();
            vd.GetNoonRList = CommonMethods.editnoonRList(id, vesselid, "DailyNoonReport");
            var noonRBind = vd.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["noonReportId"] = id;

           int vslid = vesselid;
            Session["EditVesselID"] = vesselid;

            string[] returnvalue = CommonMethods.GetVesselName1(vesselid);
            ViewBag.VesselName = returnvalue[0];
            ViewBag.Displacement = returnvalue[1];

            //Session["VesselID"] = vesselid;


            //string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            //if (vslid.ToString() == "")
            //{
            //    vslid = Convert.ToString(Session["VesselID"]);
            //}


            noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            noonRBind.CargoTanks = GetCargoTankList(id, vslid);
            noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = noonRBind.LegPortId;
            ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", id);
            ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", id);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

            return View("Index", noonRBind);

           

        }

        public JsonResult GetDisByLeg(int LegId, int CPID)
        {
            using (SqlDataAdapter sda = new SqlDataAdapter("select COALESCE(SUM(NoonToNoonDMG_Dist),0) as totalsum ,COALESCE(SUM(stmgtime),0) as totaltime , (select dtg from VoyageLeg  where id=" + LegId + " and IsActive=1) as dtg,(select cp_log_speed from VoyageLeg  where id=" + LegId + " and IsActive=1) as cpSpeed from dailynoonreport  where LegPortId=" + LegId + " and IsActive=1", ConnectionBulder.con))
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

        public ActionResult InsertBunkerRec(string bunkerRec)
        {
            try
            {
                int k = 0; int NRID = 0;
                if (Session["NR_ID"].ToString() != "")
                {
                    NRID = Convert.ToInt32(Session["NR_ID"]);
                }

                // int vslid = Convert.ToInt32(Session["VesselID"]);
                int vslid = Convert.ToInt32(Session["EditVesselID"]);

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


                    //if (k == 0)
                    //{
                    //    if (Session["NR_ID"].ToString() != "")
                    //    {
                    //        using (SqlDataAdapter adp1 = new SqlDataAdapter("delete from tbl_BunkerLReceipt where TableMax_Id=" + NRID + " and VesselId=" + vslid + " and ReportType_Id=1", ConnectionBulder.con))
                    //        {
                    //            DataTable dt1 = new DataTable();
                    //            adp1.Fill(dt1);
                    //            k = 1;
                    //        }

                    //    }
                    //}

                    if (Session["NR_ID"].ToString() == "")
                    {

                        CommonMethods.InsertUpdateBunkerReceipt(0, fueltypeid, Rec, 1, vslid, "Insert", "NoonReport");


                    }
                    if (Session["NR_ID"].ToString() != "")
                    {

                        //CommonMethods.InsertUpdateBunkerReceipt(NRID, fueltypeid, Rec, 1, vslid, "Insert", "NoonReport");
                        CommonMethods.InsertUpdateBunkerReceipt(NRID, fueltypeid, Rec, 1, vslid, "Update_NR", "NoonReport");

                    }

                }
            }
            
            catch { }
            return View();
        }

        public JsonResult Getfuelcons(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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

                };

                // Edit for Fuel Consumption
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=1  and ConsTypeId not in (1,6) order by Id asc", ConnectionBulder.con))
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
                };

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
                };

                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Receipt from tbl_BunkerLReceipt where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=1", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FBunkerValue.Add(dt.Rows[i]["Receipt"]);
                    }
                   
                    ViewBag.FBunkerValue = FBunkerValue;
                };


            }
            catch { }

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, fc = ViewBag.FcValue, frob = ViewBag.FrobValue, fBunker = ViewBag.FBunkerValue }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNREvents(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
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

                };
            }
            catch { }

            return Json(new { Result = true, chrterer = ViewBag.ChartererA, hrs = ViewBag.Hours }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNRCargoEdit(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            ArrayList nrCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.cargoname, b.PortName from NR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id  where  a.VesselId=" + vslid + " and b.VesselId=" + vslid + "  and noonreport_id=" + NoonReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    ViewBag.CountNRCargo = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        nrCargoName.Add(dt.Rows[i]["CargoName"] + " ( " +
                         (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) " );
                        nrCargoName.Add(dt.Rows[i]["BL_Qty"]);
                        nrCargoName.Add(dt.Rows[i]["LoadPortalActual"]);
                        nrCargoName.Add(dt.Rows[i]["TodaysActual"]);
                        nrCargoName.Add(dt.Rows[i]["Qty_Diff"]);
                        nrCargoName.Add(dt.Rows[i]["Reasonfor_Qty_Diff"]);
                        nrCargoName.Add(dt.Rows[i]["Cargo_Temp"]);
                        nrCargoName.Add(dt.Rows[i]["LR_Cargo_Id"]);
                    }

                    ViewBag.NRCargoList = nrCargoName;
                };




            }
            catch { }

            return Json(new { Result = true, nrc = ViewBag.NRCargoList, cntnrc = ViewBag.CountNRCargo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNR_Cargo(int LegId, int VoyageId)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        cargoName.Add(dt.Rows[i]["CargoName"]);
                    }

                    ViewBag.CName = cargoName;
                };
            }
            catch { }

            return Json(new { Result = true, cn = ViewBag.CName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNRCargo(int LegId, int VoyageId)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        cargoName.Add(dt.Rows[i]["CargoName"]);
                    }
                   
                    ViewBag.CName = cargoName;
                };
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

                };


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

                int vesselid = Convert.ToInt32(Session["EditVesselID"]);

                using (SqlDataAdapter adp = new SqlDataAdapter("select cpid from VoyageDetails where id=" + Voyid + " and VesselId='"+vesselid+"'", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        cpid = Convert.ToInt32(dt.Rows[0][0]);
                    }
                }


                //using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "' and VesselId='" + vesselid + "'", ConnectionBulder.con))
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


        public ActionResult InsertFuelROB(string fuelrob)
        {
            try
            {
                //int vslid = Convert.ToInt32(Session["VesselID"]);
                int vslid = Convert.ToInt32(Session["EditVesselID"]);
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
           // int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            List<NRCargo> FrobList = new List<NRCargo>();
            var Result = nrcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<NRCargo>>(dd);
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

                    if (rootObject.LR_Cargo_Id == 0)
                    {

                        SqlDataAdapter adp = new SqlDataAdapter("select Max(Id) from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + " and VoyageId=" + VoyId + " and PortName='" + portName + "'", ConnectionBulder.con);
                        DataTable dt = new DataTable();
                        adp.Fill(dt);

                         LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                         lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                    }
                    else
                    {
                        lrcrgid = rootObject.LR_Cargo_Id;
                    }
                    cls.LR_Cargo_Id = lrcrgid;
                    cls.VesselId = vslid;
                    cls.BL_Qty = rootObject.BL_Qty;
                    cls.LoadPortalActual = rootObject.LoadPortalActual;
                    cls.TodaysActual = rootObject.TodaysActual;
                    cls.Qty_Diff = rootObject.Qty_Diff;
                    cls.Reasonfor_Qty_Diff = rootObject.Reasonfor_Qty_Diff;
                    cls.Cargo_Temp = rootObject.Cargo_Temp;

                }


                if (Session["NR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateNRCargo(0, cls, "Insert", "NoonReport");

                }
                if (Session["NR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateNRCargo(noonReportId, cls, "Update", "NoonReport");

                }



            }
            return View();
        }

        public JsonResult CheckLastNoonR(int LegId, string Sdate)
        {
            
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            int msg = 0;
            string Rdate = "";
            try
            {
                DateTime dts = Convert.ToDateTime(Sdate);
                string dt = dts.AddDays(-1).ToString("yyyy-MM-dd");
                using (SqlDataAdapter adpvv = new SqlDataAdapter("select Date from DailyNoonReport where IsActive=1 and SaveDraft=0 and Date = '" + dt + "' and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        DateTime Ldate = Convert.ToDateTime(dtvv.Rows[0][0]);

                        msg = 0;


                    }
                    else
                    {

                        using (SqlDataAdapter adp = new SqlDataAdapter("select max(Date) from DailyNoonReport where IsActive=1 and SaveDraft=0 and  LegPortId=" + LegId + "  and VesselId=" + vslid + "", ConnectionBulder.con))
                        {

                            DataTable dtm = new DataTable();
                            adp.Fill(dtm);
                            if (dtm.Rows.Count > 0)
                            {
                                //Rdate = Convert.ToDateTime(dtm.Rows[0][0]).ToString("yyyy-MM-dd");
                                Rdate = dtm.Rows[0][0].ToString();
                                if (string.IsNullOrEmpty(Rdate))
                                {
                                    msg = 0;

                                }
                                else
                                {
                                    Rdate = Convert.ToDateTime(dtm.Rows[0][0]).ToString("yyyy-MM-dd");
                                    msg = 1;
                                }
                            }
                            else
                            {
                                msg = 0;
                            }

                        }

                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
            return Json(new { Result = true, Data = msg, Rdate = Rdate }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDToGo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselID"]);
            decimal VoyageLeg_DTG = 0.00m;
            decimal AvgSpeed = 0.00m; decimal CP_Log_Speed = 0.00m;
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

                using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed) from DailyNoonReport where IsActive=1 and SaveDraft=0 and VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                {

                    DataTable dtvv = new DataTable();
                    adpvv.Fill(dtvv);
                    if (dtvv.Rows.Count > 0)
                    {
                        AvgSpeed = Convert.ToDecimal(dtvv.Rows[0][0]);
                    }

                }
            }
            
            catch { }
            SpeedDistance Data = new SpeedDistance()
            {
                DistToGo = VoyageLeg_DTG,
                GenAvgSpeed = AvgSpeed,
                CP_Log_Speed = CP_Log_Speed
            };

            return Json(new { Result = true, Data = Data }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditFromDashboard(string reportdate, int vesselid)
        {
            DailyNoonReport vd = new DailyNoonReport();
            vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselid, "DailyNoonReport");
            string[] returnvalue = CommonMethods.GetVesselName1(vesselid);
            ViewBag.VesselName = returnvalue[0];
            ViewBag.Displacement = returnvalue[1];

            var ID = vd.GetNoonRList.Select(x => x.Id).FirstOrDefault();

            var noonRBind = vd.GetNoonRList.Where(x => x.Id == ID).FirstOrDefault(e => e.Id == ID);

            TempData["noonReportId"] = ID;

            int vslid = vesselid;
            Session["EditVesselID"] = vesselid;


            noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            noonRBind.CargoTanks = GetCargoTankList(ID, vslid);
            noonRBind.BallastTanks = GetBallastTankList(ID, vslid);
            noonRBind.Void_SpaceTanks = GetVoid_SpaceList(ID, vslid);

            int legportid = noonRBind.LegPortId;
            ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", ID);
            ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", ID);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", ID);

           return View("Index", noonRBind);


            //return View("Index", "DailyNoon", new { Area = "Report", noonRBind });
        }

        public ActionResult NoonReportallowList(int? pageNo, string firstVal, string secondVal)
        {
            DailyNoonReportAllow By_pass = new DailyNoonReportAllow();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

             By_pass.ByPass_NDPList = CommonMethods.GetNoonReportallowList(currPage, pageSize, firstVal, secondVal);
             return View(By_pass);
           
        }

        public ActionResult _noonReport_BypassDate(int? pageNo, string firstVal, string secondVal)
        {
            DailyNoonReportAllow By_pass = new DailyNoonReportAllow();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            By_pass.ByPass_NDPList = CommonMethods.GetNoonReportallowList(currPage, pageSize, firstVal, secondVal);
            ViewBag.search1 = firstVal;
            ViewBag.search2 = secondVal;
            return PartialView("_noonReport_BypassDate", By_pass);

        }

        public ActionResult AddNoonReportallow()
        {
            DailyNoonReportAllow dnp = new DailyNoonReportAllow();
            //CharterPartyClass cpm = new CharterPartyClass();
            dnp.VesselList = CommonClass.GetVesselList();
            return View(dnp);

        }

        [HttpPost]
        public ActionResult AddNoonReportallow(DailyNoonReportAllow _dnp)
        {

            string Action = _dnp.Id != 0 ? "Update" : "Insert";

            int Exist = 0;
            if (Session["user_Name"] != null)
            {
                _dnp.userName = Session["user_Name"].ToString();
            }
            
            if (Action == "Insert")
            {
                CommonMethods.InsertNoonReportallow(_dnp, Action, out Exist);
                TempData["Success"] = "Record saved successfully";
            }
            if (Action == "Update")
            {
                CommonMethods.InsertNoonReportallow(_dnp, Action, out Exist);
                TempData["Success"] = "Record update successfully";
            }
            
            return RedirectToAction("NoonReportallowList");
           
        }

        public ActionResult DeleteNoonReport_ByPassDate(int id)
        {
            try
            {
                CommonMethods.CommonDelete(id, "ByPass_Noondate");
                TempData["Success"] = "Record Deleted successfully";
            }
            catch (Exception e)
            {
                TempData["Error"] = e.Message;
                throw;
            }

            return RedirectToAction("NoonReportallowList");
        }

    }


}


public class SpeedDistance
{
    public decimal DistToGo { get; set; }
    public decimal GenAvgSpeed { get; set; }
    public decimal CP_Log_Speed { get; set; }
}

