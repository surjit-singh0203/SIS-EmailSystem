using ClosedXML.Excel;
using DataBuildingLayer;
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
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class ArrivalController : BaseController
    {
        // GET: Report/Arrival

        // Neeraj -----
        public ActionResult Index()
        {
            TempData["arrivalReportId"] = null;
            ArrivalReport arrival = new ArrivalReport();

            int vslid = Convert.ToInt32(Session["VesselID"]);
            arrival.VoyageNumberList = CommonMethods.GetVoyageList(vslid);


            ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
            return View(arrival);
        }

        [HttpPost]
        public ActionResult insertArrivalR(ArrivalReport _arrivalR)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _arrivalR.VesselId = vslid;
                      

            if (_arrivalR.Id == 0)
            {
                Session["AR_ID"] = "";
                CommonMethods.InsertUpdateArrivalReport(_arrivalR, "Insert");
                TempData["Success"] = "Record saved successfully";
            }
            if (_arrivalR.Id != 0)
            {
                //_arrivalR.Id = Convert.ToInt32(TempData["arrivalReportId"]);
                Session["AR_ID"] = _arrivalR.Id;
                CommonMethods.InsertUpdateArrivalReport(_arrivalR, "Update");
                TempData["Success"] = "Record updated successfully";
            }



            return Json(_arrivalR);
        }

        public JsonResult Bind_Port(int Voyid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            int vslid = Convert.ToInt32(Session["VesselID"]);

            try
            {

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where VoyageId=" + Voyid + " and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where VoyageId=" + Voyid + " and VesselId=" + vslid + " and  IsActive=1", ConnectionBulder.con))
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

                distinctTextList = jst.Select(item => item.Text).Distinct().Select(text => jst.First(item => item.Text == text)).ToList();

            }
            catch { }
            // return jst;

            return Json(new { Result = true,/* Data = jst*/Data = distinctTextList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindPort(int Legid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id=" + Legid + "", ConnectionBulder.con))
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
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, AE_ACT_BERTH, 2,vslid, "Insert", "ArrivalReport");
        //        }
        //        if (AE_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (AE_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (AE_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (AE_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (AE_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (ME_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (ME_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (ME_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (ME_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (ME_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (BLR_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (BLR_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (BLR_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (BLR_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
        //        }



        //        if (FRAMO_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (FRAMO_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
        //        }

        //        if (FRAMO_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (FRAMO_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (IGG.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 2, vslid, "Insert", "ArrivalReport");
        //        }




        //        if (StopageAtSea.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (Deviation.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (SlowSteaming.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (BadWeather.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (COTPrep.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (CargoHeating.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 2, vslid, "Insert", "ArrivalReport");
        //        }

        //        if (BWExchange.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 2, vslid, "Insert", "ArrivalReport");
        //        }
        //        if (Others.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 2, vslid, "Insert", "ArrivalReport");
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
                var Incinerator = rootObject.Incinerator;
                var StopageAtSea = rootObject.StopageAtSea;
                var Deviation = rootObject.Deviation;
                var SlowSteaming = rootObject.SlowSteaming;
                var BadWeather = rootObject.BadWeather;
                var COTPrep = rootObject.COTPrep;
                var CargoHeating = rootObject.CargoHeating;
                var BWExchange = rootObject.BWExchange;
                var Others = rootObject.Others;


                if (Session["AR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 2, vslid, "Insert", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 28, Incinerator, 2, vslid, "Insert", "ArrivalReport");
                }
                if (Session["AR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["AR_ID"]);
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 1, ME_CP, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 2, ME_ACT_SEA, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 3, ME_ACT_MAN, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 4, ME_ACT_WAIT, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 5, ME_ACT_BERTH, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 6, AE_CP, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 7, AE_ACT_SEA, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 8, AE_ACT_MAN, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 9, AE_ACT_WAIT, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 10, AE_ACT_BERTH, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 11, BLR_ACT_SEA, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 12, BLR_ACT_MAN, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 13, BLR_ACT_WAIT, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 14, BLR_ACT_BERTH, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 15, FRAMO_ACT_SEA, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 16, FRAMO_ACT_MAN, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 17, FRAMO_ACT_WAIT, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 18, FRAMO_ACT_BERTH, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 19, IGG, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 20, StopageAtSea, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 21, Deviation, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 22, SlowSteaming, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 23, BadWeather, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 24, COTPrep, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 25, CargoHeating, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 26, BWExchange, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 27, Others, 2, vslid, "Update", "ArrivalReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 28, Incinerator, 2, vslid, "Update", "ArrivalReport");
                }
            }
            return View();
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


        public ActionResult InsertFuelROB(string fuelRob)
        {
            try
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

                    var EOSP_ROB = rootObject.EOSP_ROB;
                    var FWE_ROB = rootObject.FWE_ROB;




                    if (Session["AR_ID"].ToString() == "")
                    {

                        CommonMethods.InsertUpdateFuelROB(0, fueltypeid, EOSP_ROB, FWE_ROB, 0, 2, vslid, "Insert", "ArrivalReport");

                    }
                    if (Session["AR_ID"].ToString() != "")
                    {
                        int noonReportId = Convert.ToInt32(Session["AR_ID"]);

                        CommonMethods.InsertUpdateFuelROB(noonReportId, fueltypeid, EOSP_ROB, FWE_ROB, 0, 2, vslid, "Update", "ArrivalReport");

                    }

                }
            }
            catch { }
            return View();
        }
        public ActionResult arrivallist(int? pageNo, string firstVal, string dateF, string dateT)
        {
            ArrivalReport arrR = new ArrivalReport();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["arrivalReportId"] = null;


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
                return View(arrR);
            }
            else if (firstVal == "" && dateF == "")
            {
                arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
                return View(arrR);
            }
            else if (firstVal != null)
            {
                arrR.GetArrivalRList = CommonMethods.SearchArrivalReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searcharrivalR", arrR);
            }


            //arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();
            

           
           
            return View(arrR);
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

       
        public JsonResult Getfuelcons(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=2  and ConsTypeId not in (1,6)", ConnectionBulder.con))
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

                           // FCValue.Add(2.000);
                        }

                    }

                    ViewBag.FcValue = FCValue;
                };

                // Edit for Fuel Rob
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select eosp from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=2", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FRobValue.Add(dt.Rows[i]["eosp"]);
                    }
                    ViewBag.FrobValue = FRobValue;
                };

                using (SqlDataAdapter objCMD = new SqlDataAdapter("select FWE from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=2", ConnectionBulder.con))
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

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, fc = ViewBag.FcValue, frob = ViewBag.FrobValue , frob1 = ViewBag.FrobValue1 }, JsonRequestBehavior.AllowGet);
        }
      
        public ActionResult Edit(int id)
        {
            ArrivalReport vd = new ArrivalReport();
            vd.GetArrivalRList = CommonMethods.editarrivalRList(id, "ArrivalReport");
            var arrRBind = vd.GetArrivalRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["arrivalReportId"] = id;

            Session["arrivalEditId"] = id;

            int vslid = Convert.ToInt32(Session["VesselID"]);

            //arrRBind.CargoTanks = GetCargoTankList(id, vslid);
            //arrRBind.BallastTanks = GetBallastTankList(id, vslid);
            //arrRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);
            arrRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            int legportid = arrRBind.LegPortId;
            int Voyid = arrRBind.VoyageId;

            ViewBag.EditArrivalFC = string.Format("Getfuelcons11('{0}');", id);
            ViewBag.EditLeg = string.Format("GetLegEdit('{0}');", legportid);
            //ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", Voyid);
            ViewBag.EditARCargoEdit = string.Format("GetARCargoEdit('{0}');", id);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

            return View("Index", arrRBind);

        }

        public JsonResult BindPort_Edit(int Voyid)
        {
            string prtname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            int vslid = Convert.ToInt32(Session["VesselID"]);

            try
            {



                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where VoyageId=" + Voyid + "  and VesselId=" + vslid + " and  IsActive=1", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where VoyageId=" + Voyid + "  and VesselId=" + vslid + " and  IsActive=1", ConnectionBulder.con))
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

                distinctTextList = jst.Select(item => item.Text).Distinct().Select(text => jst.First(item => item.Text == text)).ToList();

                int id = Convert.ToInt32(Session["arrivalEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from ArrivalReport where id=" + id + "", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        prtname = tbl.Rows[0][0].ToString();

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, PortName = prtname,/* Data = jst*/ Data = distinctTextList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindPortEdit(int Legid)
        {
            string prtname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id=" + Legid + "", ConnectionBulder.con))
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
                int id = Convert.ToInt32(Session["arrivalEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from ArrivalReport where id=" + id + "", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        prtname = tbl.Rows[0][0].ToString();

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, PortName = prtname, Data = jst }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetAR_Cargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            ArrayList cargoLR_Id = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                //using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId, a.PortName from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
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
                  
                };
            }
            catch { }

            return Json(new { Result = true, cn = ViewBag.CName }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetARCargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                //using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.LegPortId=" + LegId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
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

                if (Session["AR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateNonRoutineEvents(0, 2, nrevntid, ChartererAccount, Hours, vslid,"ArrivalReport", "Insert");
                }
                if (Session["AR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["AR_ID"]);
               
                    CommonMethods.InsertUpdateNonRoutineEvents(noonReportId, 2, nrevntid, ChartererAccount, Hours, vslid, "ArrivalReport", "Update");
                }

                nrevntid++;
            }
            return View();
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select * from tblNonRoutineCommon where Report_Table_Id=2 and ReportType_Id=" + NoonReportId + " and VesselId=" + vslid + " and IsActive=1", ConnectionBulder.con))
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

        public ActionResult InsertARCargo(string arcargo)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<ARCargo> FrobList = new List<ARCargo>();
            var Result = arcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<ARCargo>>(dd);

            int k = 0;

            foreach (var rootObject in Json)
            {

                ARCargo cls = new ARCargo();

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
                   // if (rootObject.LR_Cargo_Id == 0)
                  //  {
                        //SqlDataAdapter adp = new SqlDataAdapter("select Id from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + "", ConnectionBulder.con);
                        SqlDataAdapter adp = new SqlDataAdapter("select MAX(Id) from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + " and VoyageId="+ VoyId + " and PortName='" + portName + "'", ConnectionBulder.con);
                         DataTable dt = new DataTable();
                         adp.Fill(dt);
                    
                         LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                         lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                  //  }
                  //  else
                  //  {
                       // lrcrgid = rootObject.LR_Cargo_Id;
                  //  }
                   // int LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                   // int lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                    cls.LR_Cargo_Id = lrcrgid;
                    cls.VesselId = vslid;
                    cls.Qty_Grade1 = rootObject.Qty_Grade1;
                    cls.Qty_Grade2 = rootObject.Qty_Grade2;

                }


                if (Session["AR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateARCargo(0, cls, "Insert", "ArrivalReport");

                }
                if (Session["AR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["AR_ID"]);

                    if (k == 0)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from AR_Cargo where ArrivalReport_Id ="+ noonReportId + "  and  vesselId="+ vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }
                    }

                    //CommonMethods.InsertUpdateARCargo(noonReportId, cls, "Update", "ArrivalReport");

                    CommonMethods.InsertUpdateARCargo(noonReportId, cls, "Insert", "ArrivalReport");

                }



            }
            return View();
        }
        public JsonResult GetARCargoEdit(int ArrivalReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList arCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.cargoname, b.PortName from AR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id  where  a.VesselId=" + vslid + " and arrivalreport_id=" + ArrivalReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    ViewBag.CountARCargo = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        arCargoName.Add(dt.Rows[i]["CargoName"] + " ( " +
                         (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ");
                        arCargoName.Add(dt.Rows[i]["Qty_Grade1"]);
                        arCargoName.Add(dt.Rows[i]["Qty_Grade2"]);
                        arCargoName.Add(dt.Rows[i]["LR_Cargo_Id"]);
                    }

                    ViewBag.ARCargoList = arCargoName;
                };




            }
            catch { }

            return Json(new { Result = true, arc = ViewBag.ARCargoList, cntarc = ViewBag.CountARCargo }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int id)
        {
            CommonMethods.CommonDelete(id, "ArrivalReport");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("arrivallist");
        }

        public JsonResult GetDToGo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
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

                using (SqlDataAdapter adpvv = new SqlDataAdapter("select AVG(Act_Speed) from ArrivalReport where IsActive=1 and SaveDraft=0 and VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
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
        //        cmd.Parameters.AddWithValue("@Action", "ArrivalReport");
        //        cmd.Parameters.AddWithValue("@id", id);

        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        da.Fill(dt);
        //    }

        //    using (XLWorkbook wb = new XLWorkbook())
        //    {
        //        var ws = wb.Worksheets.Add(dt, "ArrivalReport");
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

        //            string fileName = "ArrivalReport" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";

        //            return File(
        //                stream.ToArray(),
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                fileName
        //            );
        //        }
        //    }
        //}

        public ActionResult SyncReportDownload(int VoyageId, int id, string ReportDate)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@VoyageId", VoyageId);
                cmd.Parameters.AddWithValue("@ReportDate", ReportDate);
                cmd.Parameters.AddWithValue("@VesselId", vslid);
                cmd.Parameters.AddWithValue("@Action", "ArrivalReport");
                cmd.Parameters.AddWithValue("@id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            ArrivalReport arrRBind = null;
            DataTable dtFuelROB = new DataTable();
            DataTable dtFuelCons = new DataTable();
            DataTable dtNonRoutine = new DataTable();
            DataTable dtARCargo = new DataTable();

            try
            {
                var vd = new ArrivalReport();
                vd.GetArrivalRList = CommonMethods.editarrivalRList(id, "ArrivalReport");
                arrRBind = vd.GetArrivalRList != null ? vd.GetArrivalRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id) : null;

                if (arrRBind != null)
                {
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT b.FuelType, a.EOSP, a.FWE FROM tbl_FuelROB a INNER JOIN tblFuelType b ON a.FuelType_Id=b.Id WHERE a.TableMax_Id=" + id + " AND a.VesselId=" + vslid + " AND a.ReportType_Id=2 ORDER BY a.FuelType_Id", ConnectionBulder.con))
                        {
                            adp.Fill(dtFuelROB);
                        }
                    }
                    catch
                    {
                        try
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter("SELECT b.FuelType, a.EOSP, a.FWE FROM tbl_FuelROB a INNER JOIN tblFuelType b ON a.FuelType_Id=b.Id WHERE a.TableMax_Id=" + id + " AND a.VesselId=" + vslid + " ORDER BY a.FuelType_Id", ConnectionBulder.con))
                            {
                                adp.Fill(dtFuelROB);
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT a.Value, a.ConsTypeId, b.FuelType FROM Fuel_Cons_NR a INNER JOIN tblFuelType b ON a.FuelTypeId=b.Id WHERE a.Noon_Report_Id=" + id + " AND a.VesselId=" + vslid + " AND a.ReportType_Id=2 AND a.ConsTypeId NOT IN (1,6) ORDER BY a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        {
                            adp.Fill(dtFuelCons);
                        }
                    }
                    catch { }

                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT ChartererAccount, Hours FROM tblNonRoutineCommon WHERE Report_Table_Id=2 AND ReportType_Id=" + id + " AND VesselId=" + vslid + " AND IsActive=1 ORDER BY Id", ConnectionBulder.con))
                        {
                            adp.Fill(dtNonRoutine);
                        }
                    }
                    catch { }

                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT a.*, b.CargoName, b.PortName FROM AR_Cargo a INNER JOIN LR_Cargo b ON a.LR_Cargo_Id=b.Id AND a.VesselId=b.VesselId WHERE a.VesselId=" + vslid + " AND a.ArrivalReport_Id=" + id, ConnectionBulder.con))
                        {
                            adp.Fill(dtARCargo);
                        }
                    }
                    catch
                    {
                        try
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter("SELECT a.*, b.CargoName, b.PortName FROM AR_Cargo a INNER JOIN LR_Cargo b ON a.lr_cargo_id=b.Id WHERE a.VesselId=" + vslid + " AND a.ArrivalReport_Id=" + id, ConnectionBulder.con))
                            {
                                adp.Fill(dtARCargo);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            using (XLWorkbook wb = new XLWorkbook())
            {
                AddArrivalNavigationSheet(wb, arrRBind, dtNonRoutine, dt);
                AddArrivalEngineSheet(wb, arrRBind, dtFuelROB, dtFuelCons);
                AddArrivalCargoSheet(wb, arrRBind, dtARCargo);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = "ArrivalReport" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";

                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
        }

        private static string SafeValue(object val, bool asDecimal = false)
        {
            if (val == null || val == DBNull.Value || (val is string s && string.IsNullOrWhiteSpace(s)))
                return asDecimal ? "0.00" : "";
            if (asDecimal)
            {
                decimal d;
                if (decimal.TryParse(val.ToString(), out d)) return d.ToString("0.00");
                return "0.00";
            }
            if (val is DateTime dt) return dt.ToString(CommonMethods.ExcelDateTimeFormat);
            // Parse string dates (e.g. "10/18/2025 11:30:00 PM") and format as yyyy-MM-dd HH:mm
            DateTime parsed;
            if (DateTime.TryParse(val.ToString(), out parsed)) return parsed.ToString(CommonMethods.ExcelDateTimeFormat);
            return val.ToString();
        }

        private static void AddKeyValueSingle(IXLWorksheet ws, ref int row, string label, object value, bool asDecimal = false)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = SafeValue(value, asDecimal);
            row++;
        }

        private static void ApplyGridTitleStyle(IXLRange range)
        {
            range.Style.Fill.BackgroundColor = XLColor.Gray;
            range.Style.Font.FontColor = XLColor.White;
        }

        private void AddArrivalNavigationSheet(XLWorkbook wb, ArrivalReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber ?? (r.VoyageId > 0 ? r.VoyageId.ToString() : "");
                string legText = "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
                }

                AddKeyValueSingle(ws, ref row, "Voy No.", voyNo);
                AddKeyValueSingle(ws, ref row, "Leg", legText);
                AddKeyValueSingle(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueSingle(ws, ref row, "Place", r.Place ?? "");
                AddKeyValueSingle(ws, ref row, "Latitude", r.Latitude ?? "");
                AddKeyValueSingle(ws, ref row, "Longitude", r.Longitude ?? "");
                AddKeyValueSingle(ws, ref row, "NOR", r.NOR);
                AddKeyValueSingle(ws, ref row, "EOSP", r.EOSP);
                AddKeyValueSingle(ws, ref row, "ETB", r.ETB);
                AddKeyValueSingle(ws, ref row, "Draft Fwd(Mtrs)", r.DraftFwd, true);
                AddKeyValueSingle(ws, ref row, "Draft Mid(Mtrs)", r.DraftMid, true);
                AddKeyValueSingle(ws, ref row, "Draft Aft(Mtrs)", r.DraftAft, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Anchorage";
            var rngAnch = ws.Range(row, 1, row, 3);
            rngAnch.Merge();
            rngAnch.Style.Font.Bold = true;
            rngAnch.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngAnch);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Anchorage Name", r.Anchor_Name ?? "");
                AddKeyValueSingle(ws, ref row, "Drop Anchor Date & Time", r.Anchor_DateT);
                AddKeyValueSingle(ws, ref row, "Anchoring Position - Latitude", r.AnchorPos_Latitude ?? "");
                AddKeyValueSingle(ws, ref row, "Anchoring Position - Longitude", r.AnchorPos_Longitude ?? "");
                AddKeyValueSingle(ws, ref row, "FWE Date & Time", r.AnchorFWE_DateT);
            }
            row++;

            ws.Cell(row, 1).Value = "Speed - Distance - Time";
            var rngSpeed = ws.Range(row, 1, row, 3);
            rngSpeed.Merge();
            rngSpeed.Style.Font.Bold = true;
            rngSpeed.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngSpeed);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist, true);
                AddKeyValueSingle(ws, ref row, "Log Dist(NM)", r.LogDist, true);
                AddKeyValueSingle(ws, ref row, "Engine Dist(NM)", r.EngineDist, true);
                AddKeyValueSingle(ws, ref row, "Total Distance (Dep to Curr)(NM)", r.TotalDistance, true);
                AddKeyValueSingle(ws, ref row, "Dist to Go (DTG)(NM)", r.DistToGo_DTG, true);
                AddKeyValueSingle(ws, ref row, "Stmg Time Noon to Arrival(Hrs)", r.StmgTime, true);
                AddKeyValueSingle(ws, ref row, "Total Time (Dep to Curr)(Hrs)", r.TotalTime, true);
                AddKeyValueSingle(ws, ref row, "Actual Speed Noon to Arrival(Kts)", r.Act_Speed, true);
                AddKeyValueSingle(ws, ref row, "Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Manoeuvring";
            var rngMan = ws.Range(row, 1, row, 3);
            rngMan.Merge();
            rngMan.Style.Font.Bold = true;
            rngMan.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngMan);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Manoeuvring Hours", r.Manoeuvring_Hrs ?? "");
                AddKeyValueSingle(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            var rngWeather = ws.Range(row, 1, row, 3);
            rngWeather.Merge();
            rngWeather.Style.Font.Bold = true;
            rngWeather.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngWeather);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Sea State", r.SeaState ?? "");
                AddKeyValueSingle(ws, ref row, "Wind Direction", r.WindDirection ?? "");
                AddKeyValueSingle(ws, ref row, "Wind Force(BF Scale)", r.WindForce ?? "");
                AddKeyValueSingle(ws, ref row, "Swell Direction", r.SwellDirection ?? "");
                AddKeyValueSingle(ws, ref row, "Swell Height (mtrs)", r.SwellHeight, true);
                AddKeyValueSingle(ws, ref row, "Wave Length (mtrs)", r.WaveLength, true);
                AddKeyValueSingle(ws, ref row, "Wave Height (mtrs)", r.WaveHeight, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            var rngNRE = ws.Range(row, 1, row, 3);
            rngNRE.Merge();
            rngNRE.Style.Font.Bold = true;
            rngNRE.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
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

            ws.Cell(row, 1).Value = "Arrival Report Remarks";
            var rngRemarks = ws.Range(row, 1, row, 3);
            rngRemarks.Merge();
            rngRemarks.Style.Font.Bold = true;
            rngRemarks.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngRemarks);
            row++;
            AddKeyValueSingle(ws, ref row, "Remarks", r?.Remarks ?? "");

            ws.Columns().AdjustToContents();
        }

        private void AddArrivalEngineSheet(XLWorkbook wb, ArrivalReport r, DataTable dtFuelROB, DataTable dtFuelCons)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Engine";
            var rngEngine = ws.Range(row, 1, row, 3);
            rngEngine.Merge();
            rngEngine.Style.Font.Bold = true;
            rngEngine.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngEngine);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "SLIP%", r.Slip, true);
                AddKeyValueSingle(ws, ref row, "RPM", r.RPM, true);
                AddKeyValueSingle(ws, ref row, "BHP(hp)", r.BHP, true);
                AddKeyValueSingle(ws, ref row, "MCR%", r.MCR, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            var rngFuelROB = ws.Range(row, 1, row, 3);
            rngFuelROB.Merge();
            rngFuelROB.Style.Font.Bold = true;
            rngFuelROB.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngFuelROB);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "EOSP";
            ws.Cell(row, 3).Value = "FWE";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtFuelROB != null && dtFuelROB.Rows.Count > 0)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    ws.Cell(row, 1).Value = dr["FuelType"]?.ToString() ?? "";
                    ws.Cell(row, 2).Value = SafeValue(dr.Table.Columns.Contains("EOSP") ? dr["EOSP"] : null, true);
                    ws.Cell(row, 3).Value = SafeValue(dr.Table.Columns.Contains("FWE") ? dr["FWE"] : null, true);
                    row++;
                }
            }
            else if (r != null)
            {
                ws.Cell(row, 1).Value = "VLSFO";
                ws.Cell(row, 2).Value = SafeValue(r.EOSP_ROB, true);
                ws.Cell(row, 3).Value = SafeValue(r.FWE_ROB, true);
                row++;
                ws.Cell(row, 1).Value = "MDO";
                ws.Cell(row, 2).Value = "0.00";
                ws.Cell(row, 3).Value = "0.00";
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            var rngLOHO = ws.Range(row, 1, row, 4);
            rngLOHO.Merge();
            rngLOHO.Style.Font.Bold = true;
            rngLOHO.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngLOHO);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Consumption";
            ws.Cell(row, 3).Value = "ROB";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "MECC (Ltrs)";
                ws.Cell(row, 2).Value = SafeValue(r.LO_HO_Cons_MECC, true);
                ws.Cell(row, 3).Value = SafeValue(r.LO_HO_Cons_MECC_ROB, true);
                row++;
                ws.Cell(row, 1).Value = "MECYL (Ltrs)";
                ws.Cell(row, 2).Value = SafeValue(r.LO_HO_Cons_MECYL, true);
                ws.Cell(row, 3).Value = SafeValue(r.LO_HO_Cons_MECYL_ROB, true);
                row++;
                ws.Cell(row, 1).Value = "AECC (Ltrs)";
                ws.Cell(row, 2).Value = SafeValue(r.LO_HO_Cons_AECC, true);
                ws.Cell(row, 3).Value = SafeValue(r.LO_HO_Cons_AECC_ROB, true);
                row++;
                ws.Cell(row, 1).Value = "HYDRAULIC Oil (Ltrs)";
                ws.Cell(row, 2).Value = SafeValue(r.LO_HO_Cons_HYDR_Oil, true);
                ws.Cell(row, 3).Value = SafeValue(r.LO_HO_Cons_HYDR_Oil_ROB, true);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Other ROB";
            var rngOtherROB = ws.Range(row, 1, row, 4);
            rngOtherROB.Merge();
            rngOtherROB.Style.Font.Bold = true;
            rngOtherROB.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngOtherROB);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Full";
            ws.Cell(row, 3).Value = "In Use";
            ws.Cell(row, 4).Value = "Empty";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 2).Value = SafeValue(r.OT_ROB_OXY_Full, true);
                ws.Cell(row, 3).Value = SafeValue(r.OT_ROB_OXY_InUse, true);
                ws.Cell(row, 4).Value = SafeValue(r.OT_ROB_OXY_Empty, true);
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 2).Value = SafeValue(r.OT_ROB_ACYT_Full, true);
                ws.Cell(row, 3).Value = SafeValue(r.OT_ROB_ACYT_InUse, true);
                ws.Cell(row, 4).Value = SafeValue(r.OT_ROB_ACYT_Empty, true);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            var rngFC = ws.Range(row, 1, row, 30);
            rngFC.Merge();
            rngFC.Style.Font.Bold = true;
            rngFC.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngFC);
            row++;
            // Row 1 - Level 1 (Main Engine, Aux Engine, Boiler, Framo System, IGG, Incinerator, Events, TOTAL) - match edit page design
            ws.Cell(row, 1).Value = "";
            ws.Range(row, 2, row, 6).Merge();
            ws.Cell(row, 2).Value = "Main Engine";
            ws.Range(row, 7, row, 11).Merge();
            ws.Cell(row, 7).Value = "Aux Engine";
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
            WriteFuelConsumptionRow(ws, ref row, "VLSFO", dtFuelCons);
            WriteFuelConsumptionRow(ws, ref row, "MDO", dtFuelCons);

            ws.Columns().AdjustToContents();
            // Ensure Events (22-29) and TOTAL (30) columns show full text - set minimum width for "Stoppage at Sea", "Cargo Heating", etc.
            for (int c = 22; c <= 29; c++)
            {
                var col = ws.Column(c);
                if (col.Width < 14) col.Width = 14;
            }
            if (ws.Column(30).Width < 10) ws.Column(30).Width = 10;
        }

        private void WriteFuelConsumptionRow(IXLWorksheet ws, ref int row, string fuelType, DataTable dtFuelCons)
        {
            var vals = GetArrivalFuelConsByFuelType(dtFuelCons, fuelType);
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

        private static decimal[] GetArrivalFuelConsByFuelType(DataTable dt, string fuelType)
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

        private void AddArrivalCargoSheet(XLWorkbook wb, ArrivalReport r, DataTable dtARCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;

            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Fresh Water Noon To Report";
            var rngFW = ws.Range(row, 1, row, 3);
            rngFW.Merge();
            rngFW.Style.Font.Bold = true;
            rngFW.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngFW);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "FW Generated (MT)", r.FW_Generated, true);
                AddKeyValueSingle(ws, ref row, "Consumption (MT)", r.FW_Consumption, true);
                AddKeyValueSingle(ws, ref row, "ROB (MT)", r.FW_ROB, true);
            }
            row++;

            ws.Cell(row, 1).Value = "Cargo";
            var rngCargo = ws.Range(row, 1, row, 7);
            rngCargo.Merge();
            rngCargo.Style.Font.Bold = true;
            rngCargo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngCargo);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "Qty(MT)";
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            row++;
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cname = dr["CargoName"]?.ToString() ?? "";
                    string pname = dr["PortName"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(pname)) cname = cname + " ( " + pname + " ) ";
                    AddKeyValueSingle(ws, ref row, cname, SafeValue(dr["Qty_Grade1"], true));
                }
            }
            else if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "", SafeValue(r.Qty_Grade1, true));
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            var rngBallast = ws.Range(row, 1, row, 3);
            rngBallast.Merge();
            rngBallast.Style.Font.Bold = true;
            rngBallast.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngBallast);
            row++;
            AddKeyValueSingle(ws, ref row, "ROB", r?.Ballast_ROB ?? "");
            row++;

            ws.Cell(row, 1).Value = "Slops ROB";
            var rngSlops = ws.Range(row, 1, row, 4);
            rngSlops.Merge();
            rngSlops.Style.Font.Bold = true;
            rngSlops.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ApplyGridTitleStyle(rngSlops);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Oil(m3)";
            ws.Cell(row, 3).Value = "Water(m3)";
            ws.Cell(row, 4).Value = "Total";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = "ROB (m3)";
            ws.Cell(row, 2).Value = SafeValue(r?.SLOPS_ROB_OXY_Oil, true);
            ws.Cell(row, 3).Value = SafeValue(r?.SLOPS_ROB_OXY_Water, true);
            ws.Cell(row, 4).Value = SafeValue(r?.SLOPS_ROB_OXY_Total, true);
            row++;

            ws.Columns().AdjustToContents();
        }
    }
}                                                      