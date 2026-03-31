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
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    [Authorize]
    public class DepartureController : Controller
    {
        // GET: Report/Departure
        public ActionResult Index()
        {
            DepartureReport depR = new DepartureReport();
            TempData["departureReportId"] = null;
            ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
            return View(depR);
        }

        [HttpPost]
        public ActionResult insertDepartureR(DepartureReport _departure)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _departure.VesselId = vslid;
           


            if (_departure.Id == 0)
            {
                Session["DR_ID"] = "";
                CommonMethods.InsertUpdateDepartureReport(_departure, "Insert");
                TempData["Success"] = "Record saved successfully";
            }
            if (_departure.Id != 0)
            {
                //_departure.Id = Convert.ToInt32(TempData["departureReportId"]);
                Session["DR_ID"] = _departure.Id;
                CommonMethods.InsertUpdateDepartureReport(_departure, "Update");
                TempData["Success"] = "Record updated successfully";
            }

            return Json(_departure);
        }

        public JsonResult BindLeg(int Voyid,int VID)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



                //using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where VesselId = "+ VID + " and voyageid='" + Voyid + "'", ConnectionBulder.con))
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

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BindPort(int Legid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id="+Legid+"", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["port"].ToString(), Value = tbl.Rows[i]["id"].ToString() });

                        }

                    }
                }

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where id=" + Legid + "", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["port"].ToString(), Value = tbl.Rows[i]["id"].ToString() });

                        }

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }

       
        //public ActionResult InsertFuelConsumption(string fuelconsumptions)
        //{
        //    int vslid = Convert.ToInt32(Session["VesselID"]);
        //    List<FCons> FconsList = new List<FCons>();
        //    var Result = fuelconsumptions;
        //    string json = Result.ToString();
        //    string dd = json;
        //    var Json = JsonConvert.DeserializeObject<List<FCons>>(dd);
        //    foreach (var rootObject in Json)
        //    {
        //        var FuelType = rootObject.FuelType;

        //        SqlDataAdapter adp = new SqlDataAdapter("select Id from tblFuelType where fueltype='" + FuelType + "'", ConnectionBulder.con);
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);

        //        int fueltypeid = Convert.ToInt32(dt.Rows[0][0]);

        //        var AE_ACT_BERTH = rootObject.AE_ACT_BERTH;
        //        var AE_ACT_MAN = rootObject.AE_ACT_MAN;
        //        var AE_ACT_SEA = rootObject.AE_ACT_SEA;
        //        var AE_ACT_WAIT = rootObject.AE_ACT_WAIT;
        //        var AE_CP = rootObject.AE_CP;
        //        var ME_ACT_BERTH = rootObject.ME_ACT_BERTH;
        //        var ME_ACT_MAN = rootObject.ME_ACT_MAN;
        //        var ME_ACT_SEA = rootObject.ME_ACT_SEA;
        //        var ME_ACT_WAIT = rootObject.ME_ACT_WAIT;
        //        var ME_CP = rootObject.ME_CP;
        //        var BLR_ACT_BERTH = rootObject.BLR_ACT_BERTH;
        //        var BLR_ACT_MAN = rootObject.BLR_ACT_MAN;
        //        var BLR_ACT_SEA = rootObject.BLR_ACT_SEA;
        //        var BLR_ACT_WAIT = rootObject.BLR_ACT_WAIT;
        //        var FRAMO_ACT_BERTH = rootObject.FRAMO_ACT_BERTH;
        //        var FRAMO_ACT_MAN = rootObject.FRAMO_ACT_MAN;
        //        var FRAMO_ACT_SEA = rootObject.FRAMO_ACT_SEA;
        //        var FRAMO_ACT_WAIT = rootObject.FRAMO_ACT_WAIT;
        //        var IGG = rootObject.IGG;

        //        var StopageAtSea = rootObject.StopageAtSea;
        //        var Deviation = rootObject.Deviation;
        //        var SlowSteaming = rootObject.SlowSteaming;
        //        var BadWeather = rootObject.BadWeather;
        //        var COTPrep = rootObject.COTPrep;
        //        var CargoHeating = rootObject.CargoHeating;
        //        var BWExchange = rootObject.BWExchange;
        //        var Others = rootObject.Others;


        //        if (AE_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, AE_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (AE_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (AE_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (AE_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (AE_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (AE_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (ME_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (ME_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (ME_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (ME_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (ME_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (BLR_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (BLR_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (BLR_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (BLR_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
        //        }



        //        if (FRAMO_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (FRAMO_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
        //        }

        //        if (FRAMO_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (FRAMO_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (IGG.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 3, vslid, "Insert", "DepartureReport");
        //        }




        //        if (StopageAtSea.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (Deviation.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (SlowSteaming.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (BadWeather.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (COTPrep.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (CargoHeating.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 3, vslid, "Insert", "DepartureReport");
        //        }

        //        if (BWExchange.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 3, vslid, "Insert", "DepartureReport");
        //        }
        //        if (Others.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 3, vslid, "Insert","DepartureReport");
        //        }



        //        //var meAtSea= rootObject.ME_ACT_SEA
        //    }
        //    return View();
        //}

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
                var StopageAtSea = rootObject.StopageAtSea;
                var Deviation = rootObject.Deviation;
                var SlowSteaming = rootObject.SlowSteaming;
                var BadWeather = rootObject.BadWeather;
                var COTPrep = rootObject.COTPrep;
                var CargoHeating = rootObject.CargoHeating;
                var BWExchange = rootObject.BWExchange;
                var Others = rootObject.Others;


                if (Session["DR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 3, vslid, "Insert", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 3, vslid, "Insert", "DepartureReport");
                }
                if (Session["DR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["DR_ID"]);
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 1, ME_CP, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 2, ME_ACT_SEA, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 3, ME_ACT_MAN, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 4, ME_ACT_WAIT, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 5, ME_ACT_BERTH, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 6, AE_CP, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 7, AE_ACT_SEA, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 8, AE_ACT_MAN, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 9, AE_ACT_WAIT, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 10, AE_ACT_BERTH, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 11, BLR_ACT_SEA, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 12, BLR_ACT_MAN, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 13, BLR_ACT_WAIT, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 14, BLR_ACT_BERTH, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 15, FRAMO_ACT_SEA, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 16, FRAMO_ACT_MAN, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 17, FRAMO_ACT_WAIT, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 18, FRAMO_ACT_BERTH, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 19, IGG, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 20, StopageAtSea, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 21, Deviation, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 22, SlowSteaming, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 23, BadWeather, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 24, COTPrep, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 25, CargoHeating, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 26, BWExchange, 3, vslid, "Update", "DepartureReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 27, Others, 3, vslid, "Update", "DepartureReport");
                }
            }
            return View();
        }
        public ActionResult InsertFuelROB(string fuelRob)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<FuelROB> FrobList = new List<FuelROB>();
            var Result = fuelRob;
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

                var SBE_ROB = rootObject.SBE_ROB;
                var RFA_ROB = rootObject.RFA_ROB;

             


                if (Session["DR_ID"].ToString() == "")
                {

                

                    CommonMethods.InsertUpdateFuelROB(0, fueltypeid, SBE_ROB, RFA_ROB, 0, 3, vslid, "Insert", "DepartureReport");
                }
                if (Session["DR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["DR_ID"]);

                    CommonMethods.InsertUpdateFuelROB(noonReportId, fueltypeid, SBE_ROB, RFA_ROB, 0, 3, vslid, "Update", "DepartureReport");
                    

                }



            }
            return View();
        }

        public ActionResult InsertBunkerRec(string bunkerRec)
        {
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
               

              

                if (Session["DR_ID"].ToString() == "")
                {

                    CommonMethods.InsertUpdateBunkerReceipt(0, fueltypeid, Rec, 3, vslid, "Insert", "DepartureReport");


                   
                }
                if (Session["DR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["DR_ID"]);

                    CommonMethods.InsertUpdateBunkerReceipt(noonReportId, fueltypeid, Rec, 3, vslid, "Update", "DepartureReport");

                  

                }




            }
            return View();
        }

        public ActionResult departurelist(int? pageNo, string firstVal, string dateF, string dateT)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
           // ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            DepartureReport depR = new DepartureReport();
            TempData["departureReportId"] = null;


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;
            if (string.IsNullOrEmpty(firstVal))
            {
                //depR.GetDepRList = CommonMethods.GetDepartureReportList(StaticHelper.PermittedVessel, currPage, pageSize);
                depR.GetDepRList = CommonMethods.GetDepartureReportList("", currPage, pageSize);
                return View(depR);
            }
            else //if (firstVal != null)
            {
                depR.GetDepRList = CommonMethods.SearchDepartureReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchdepartureR", depR);
            }


            // depR.GetDepRList = CommonMethods.GetDepartureReportList(vslid, currPage, pageSize);



            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();
           



            //depR.GetDepRList = CommonMethods.GetDepartureReportList(vslid);
            return View(depR);
        }

        public JsonResult Getfuelcons(int NoonReportId,int VID)
        {
            int vslid = VID; // Convert.ToInt32(Session["VesselID"]);
            ArrayList arrName = new ArrayList();
            ArrayList CpValue = new ArrayList();
            ArrayList FCValue = new ArrayList();
            ArrayList FRobValue = new ArrayList();
            ArrayList FRobValue1 = new ArrayList();

            IList<string> ft = new List<string>();
            IList<string> cp = new List<string>();
            IList<string> fc = new List<string>();
            IList<string> frob = new List<string>();
            IList<string> frob1 = new List<string>();

            try
            {
                //using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.FuelType from FuelConsumption a inner join tblFuelType b on a.FuelTypeId=b.Id and a.IsActive=1 and a.VoyageId=" + 9 + "", ConnectionBulder.con))
                //{
                //    DataTable dt = new DataTable();
                //    objCMD.Fill(dt);

                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        arrName.Add(dt.Rows[i]["FuelType"]);
                //        CpValue.Add(dt.Rows[i]["CP_cons_perday_HFO"]);


                //    }

                //    ViewBag.Ftype = arrName;
                //    ViewBag.CPValue = CpValue;

                //};

                // Edit for Fuel Consumption
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=3  and ConsTypeId not in (1,6)", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FCValue.Add(dt.Rows[i]["value"]);
                    }
                    ViewBag.FcValue = FCValue;
                };

                // Edit for Fuel Rob
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select eosp from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=3", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FRobValue.Add(dt.Rows[i]["eosp"]);
                    }
                    ViewBag.FrobValue = FRobValue;
                };

                using (SqlDataAdapter objCMD = new SqlDataAdapter("select FWE from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=3", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FRobValue1.Add(dt.Rows[i]["FWE"]);
                    }
                    ViewBag.FrobValue1 = FRobValue1;
                };


            }
            catch { }

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, fc = ViewBag.FcValue, frob = ViewBag.FrobValue, frob1 = ViewBag.FrobValue1 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id,int VID)
        {
            
            DepartureReport vd = new DepartureReport();
            vd.GetDepRList = CommonMethods.editdepartureRList(id, VID, "DepartureReport");
            var depRBind = vd.GetDepRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["departureReportId"] = id;
            TempData["VesselName"] = CommonMethods.GetVesselName(VID);
            int vslid = Convert.ToInt32(Session["VesselID"]);

            //arrRBind.CargoTanks = GetCargoTankList(id, vslid);
            //arrRBind.BallastTanks = GetBallastTankList(id, vslid);
            //arrRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int deplegportid = depRBind.DepLegPortId;
            int nextlegportid = depRBind.NextLegPortId;
            ViewBag.EditDepFC = string.Format("Getfuelcons11('{0}','{1}');", id,VID);
            ViewBag.EditLeg = string.Format("GetLegEdit('{0}','{1}');", deplegportid, VID);
            ViewBag.EditLegNext = string.Format("GetLegEditNext('{0}','{1}');", nextlegportid, VID);
            ViewBag.EditDRCargo = string.Format("GetDRCargoEdit('{0}','{1}');", id, VID);

            return View("Index", depRBind);

        }

        public JsonResult GetDRCargoEdit(int DepReportId,int VID)
        {
            int vslid = VID; // Convert.ToInt32(Session["VesselID"]);
            List<DRCargo> drCargoNameLsit = new List<DRCargo>();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,convert(varchar,  a.Completion_DateT, 120) as Completion_Date,b.cargoname from DR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id  where  a.VesselId=" + vslid + " and depreport_id=" + DepReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    //ViewBag.CountDRCargo = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DRCargo drCargoName = new DRCargo();

                        drCargoName.CargoName = dt.Rows[i]["CargoName"].ToString();
                        drCargoName.BL_Qty = Convert.ToDecimal(dt.Rows[i]["BL_Qty"]);
                        drCargoName.LoadPortalActual = Convert.ToDecimal(dt.Rows[i]["LoadPortalActual"]);
                        drCargoName.Cargo_Temp= Convert.ToDecimal(dt.Rows[i]["Cargo_Temp"]);
                        drCargoName.Completion_Date = dt.Rows[i]["Completion_Date"].ToString();
                        drCargoName.Rate = Convert.ToDecimal(dt.Rows[i]["Rate"]);
                        drCargoNameLsit.Add(drCargoName);
                    }

                   // ViewBag.DRCargoList = drCargoName;
                };




            }
            catch { }

            return Json(new { Result = true, drc = drCargoNameLsit }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertDRCargo(string drcargo)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
        
            var Result = drcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<DRCargo>>(dd);
            foreach (var rootObject in Json)
            {

                DRCargo cls = new DRCargo();

                var Cname = rootObject.CargoName;

                if (Cname != "")
                {
                    SqlDataAdapter adp = new SqlDataAdapter("select Id from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + "", ConnectionBulder.con);
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    int LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                    int lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                    cls.LR_Cargo_Id = lrcrgid;
                    cls.VesselId = vslid;
                    cls.BL_Qty = rootObject.BL_Qty;
                    cls.LoadPortalActual = rootObject.LoadPortalActual;
                    cls.Cargo_Temp = rootObject.Cargo_Temp;
                    cls.Completion_DateT = rootObject.Completion_DateT;
                    cls.Rate = rootObject.Rate;

                }



                if (Session["DR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateDRCargo(0, cls, "Insert", "DepartureReport");

                }
                if (Session["DR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["DR_ID"]);

                    CommonMethods.InsertUpdateDRCargo(noonReportId, cls, "Update", "DepartureReport");

                }



            }
            return View();
        }

        public JsonResult GetDRCargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
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

        public ActionResult Delete(int id)
        {
            CommonMethods.CommonDelete(id, "DepartureReport");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("departurelist");
        }

    }
}