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
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class ArrivalController : BaseController
    {
        // GET: Report/Arrival
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

            try
            {



                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where  VoyageId=" + Voyid + " and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where i VoyageId=" + Voyid + " and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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

            return Json(new { Result = true, /*Data = jst*/ Data = distinctTextList }, JsonRequestBehavior.AllowGet);
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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
                //int vslid = Convert.ToInt32(Session["VesselID"]);
                int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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
        public ActionResult arrivallist(int? pageNo, string firstVal, string dateF, string dateT,string Vessel)
        {
            ArrivalReport arrR = new ArrivalReport();

            arrR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["arrivalReportId"] = null;


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
                arrR.VesselId = Convert.ToInt32(Vessel);
                return View(arrR);
            }
            else if (firstVal == "" && dateF == "" && Vessel == "")
            {
                arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
                arrR.VesselId = Convert.ToInt32(Vessel);
                return View(arrR);
            }
            else if (firstVal != null || dateF != "" || Vessel != "")
            {
                arrR.GetArrivalRList = CommonMethods.SearchArrivalReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                arrR.VesselId = Convert.ToInt32(Vessel);
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
                int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

                using (SqlDataAdapter adp = new SqlDataAdapter("select cpid from VoyageDetails where id=" + Voyid + " and VesselId='"+vslid+"'", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        cpid = Convert.ToInt32(dt.Rows[0][0]);
                    }
                }


                //using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "' and VesselId='" + vslid + "'", ConnectionBulder.con))
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
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=2  and ConsTypeId not in (1,6) order by id asc", ConnectionBulder.con))
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

        public ActionResult Edit(int id, int vesselid)
        {
            ArrivalReport vd = new ArrivalReport();
            vd.GetArrivalRList = CommonMethods.editarrivalRList(id, vesselid, "ArrivalReport");
            var arrRBind = vd.GetArrivalRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["arrivalReportId"] = id;

            Session["arrivalEditId"] = id;

            int vslid = vesselid;

            //Session["VesselID"] = vesselid;       

            Session["EditVesselIDArrival"] = vesselid;
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
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

            try
            {
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where VoyageId=" + Voyid + "  and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where VoyageId=" + Voyid + "  and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from ArrivalReport where id=" + id + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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

            return Json(new { Result = true, PortName = prtname, /*Data = jst*/ Data = distinctTextList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindPortEdit(int Legid)
        {
            string prtname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {
                //int vesselid = Convert.ToInt32(Session["VesselID"]);
                int vesselid = Convert.ToInt32(Session["EditVesselIDArrival"]);
              

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id=" + Legid + " and VesselId='"+vesselid+"'", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where id=" + Legid + " and VesselId='" + vesselid + "'", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from ArrivalReport where id=" + id + " and VesselId='" + vesselid + "'", ConnectionBulder.con))
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
            // int vslid = Convert.ToInt32(Session["VesselID"]);

            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

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

        public JsonResult GetARCargo(int LegId, int VoyageId)
        {
           // int vslid = Convert.ToInt32(Session["VesselID"]);

            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

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

        public ActionResult InsertNREvents(string nonroutine)
        {
            int nrevntid = 1;
           // int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);

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
                    CommonMethods.InsertUpdateNonRoutineEvents(0, 2, nrevntid, ChartererAccount, Hours, vslid, "ArrivalReport", "Insert");
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
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
            List<ARCargo> FrobList = new List<ARCargo>();
            var Result = arcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<ARCargo>>(dd);
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

                    CommonMethods.InsertUpdateARCargo(noonReportId, cls, "Update", "ArrivalReport");

                }



            }
            return View();
        }
        public JsonResult GetARCargoEdit(int ArrivalReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
            ArrayList arCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                 using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.cargoname, b.PortName from AR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id  where  a.VesselId=" + vslid + " and b.VesselId=" + vslid + " and arrivalreport_id=" + ArrivalReportId + "", ConnectionBulder.con))
               // using (SqlDataAdapter objCMD = new SqlDataAdapter("select distinct aa.Qty_Grade1, aa.Qty_Grade2, bb.CargoName from AR_Cargo aa join LR_Cargo bb on aa.VesselId = bb.VesselId where aa.VesselId = " + vslid + " and aa.arrivalreport_id =" + ArrivalReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    ViewBag.CountARCargo = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        arCargoName.Add(dt.Rows[i]["CargoName"] + " ( " +
                         (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) " );
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
           // int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDArrival"]);
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

        public ActionResult DownloadExcel(int id, int vesselid)
        {
            DailyNoonReport vd = new DailyNoonReport();
            int i = 0; int check = 0;
            XLWorkbook wb = new XLWorkbook();

            DataTable newTable = new DataTable();
            // DataTable table ;
            // DataSet ds =new DataSet ();

            //using (SqlDataAdapter adp2 = new SqlDataAdapter("select * from bulknoonReportTemp", ConnectionBulder.con))
            //{
            //    DataTable ddt2 = new DataTable();
            //    adp2.Fill(ddt2);

            //    for (int j = 0; j < ddt2.Rows.Count; j++)
            //    {
            //        int nrid = Convert.ToInt32(ddt2.Rows[j]["NoonReportId"]);
            //        int vslid = Convert.ToInt32(ddt2.Rows[j]["VesselId"]);


                    //DataSet ds = CommonMethods.ExportBulkNoonRList(id, vesselid);
                    DataSet ds = CommonMethods.ExportReportList(id, vesselid,"ArrivalReport");
                    using (wb = new XLWorkbook())
                    {
                        foreach (DataTable table in ds.Tables)
                        {
                            //table.Columns.AddRange(new DataColumn[3] { new DataColumn("Id"), new DataColumn("Name"), new DataColumn("Country") });

                            table.Columns.Add(new DataColumn("FUELROB_VLSFO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FUELROB_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_SEA_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_MAN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_AN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_BE_VLS", typeof(decimal)));
                            // table.Columns.Add(new DataColumn("ME_ACT_SUBT_VLS", typeof(decimal)));                

                            table.Columns.Add(new DataColumn("AE_ACT_SEA_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_MAN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_AN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_BE_VLS", typeof(decimal)));

                            // table.Columns.Add(new DataColumn("AE_ACT_SUBT_VLS", typeof(decimal)));

                            table.Columns.Add(new DataColumn("BOIL_ACT_SEA_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_MAN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_AN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_BE_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_SEA_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_MAN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_AN_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_BE_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("IGG_VLS", typeof(decimal)));
                             table.Columns.Add(new DataColumn("INCINERATOR_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_SEASTOP_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_DEV_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_SLOW_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_BADWE_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_COT_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_CHE_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_BWEX_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_OTH_VLS", typeof(decimal)));
                            //table.Columns.Add(new DataColumn("TOT_VLS", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_SEA_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_MAN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_AN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("ME_ACT_BE_MDO", typeof(decimal)));
                            //  table.Columns.Add(new DataColumn("ME_ACT_SUBT_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_SEA_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_MAN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_AN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("AE_ACT_BE_MDO", typeof(decimal)));

                            //  table.Columns.Add(new DataColumn("AE_ACT_SUBT_MDO", typeof(decimal)));

                            table.Columns.Add(new DataColumn("BOIL_ACT_SEA_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_MAN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_AN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("BOIL_ACT_BE_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_SEA_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_MAN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_AN_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("FRAMO_ACT_BE_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("IGG_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("INCINERATOR_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_SEASTOP_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_DEV_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_SLOW_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_BADWE_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_COT_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_CHE_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_BWEX_MDO", typeof(decimal)));
                            table.Columns.Add(new DataColumn("EV_OTH_MDO", typeof(decimal)));
                            //table.Columns.Add(new DataColumn("TOT_MDO", typeof(decimal)));


                            foreach (DataRow row in table.Rows)
                            {

                                //using (SqlDataAdapter adp = new SqlDataAdapter("select * from tbl_FuelROB where ReportType_Id=1 and TableMax_Id in(3,7) and VesselId in(9293143,9293143)", ConnectionBulder.con))
                                using (SqlDataAdapter adp = new SqlDataAdapter("select * from tbl_FuelROB where ReportType_Id=2 and TableMax_Id =" + id + " and VesselId =" + vesselid + "", ConnectionBulder.con))
                                {
                                    DataTable dt = new DataTable();
                                    adp.Fill(dt);

                                    if (dt.Rows.Count > 0)
                                    {
                                        row["FUELROB_VLSFO"] = dt.Rows[0]["OtherROB"];
                                        row["FUELROB_MDO"] = dt.Rows[1]["OtherROB"];
                                    }
                                }



                                // using (SqlDataAdapter adp = new SqlDataAdapter("select * from Fuel_Cons_NR  where ReportType_Id=1 and VesselId in(9293143,9293143) and Noon_Report_Id in(3,7)", ConnectionBulder.con))
                                using (SqlDataAdapter adp = new SqlDataAdapter("select * from Fuel_Cons_NR  where ReportType_Id=2 and VesselId =" + vesselid + " and Noon_Report_Id =" + id + "", ConnectionBulder.con))
                                {
                                    DataTable ddt = new DataTable();
                                    adp.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
                                    {
                                      if (ddt.Rows.Count > 54)
                                      {
                                           row["ME_ACT_SEA_VLS"] = ddt.Rows[1][4];
                                           row["ME_ACT_MAN_VLS"] = ddt.Rows[2][4];
                                           row["ME_ACT_AN_VLS"] = ddt.Rows[3][4];
                                           row["ME_ACT_BE_VLS"] = ddt.Rows[4][4];
                                           row["AE_ACT_SEA_VLS"] = ddt.Rows[6][4];
                                           row["AE_ACT_MAN_VLS"] = ddt.Rows[7][4];
                                           row["AE_ACT_AN_VLS"] = ddt.Rows[8][4];
                                           row["AE_ACT_BE_VLS"] = ddt.Rows[9][4];
                                           row["BOIL_ACT_SEA_VLS"] = ddt.Rows[10][4];
                                           row["BOIL_ACT_MAN_VLS"] = ddt.Rows[11][4];
                                           row["BOIL_ACT_AN_VLS"] = ddt.Rows[12][4];
                                           row["BOIL_ACT_BE_VLS"] = ddt.Rows[13][4];
                                           row["FRAMO_ACT_SEA_VLS"] = ddt.Rows[14][4];
                                           row["FRAMO_ACT_MAN_VLS"] = ddt.Rows[15][4];
                                           row["FRAMO_ACT_AN_VLS"] = ddt.Rows[16][4];
                                           row["FRAMO_ACT_BE_VLS"] = ddt.Rows[17][4];
                                           row["IGG_VLS"] = ddt.Rows[18][4];
                                           row["INCINERATOR_VLS"] = ddt.Rows[27][4];

                                           row["EV_SEASTOP_VLS"] = ddt.Rows[19][4];
                                           row["EV_DEV_VLS"] = ddt.Rows[20][4];
                                           row["EV_SLOW_VLS"] = ddt.Rows[21][4];
                                           row["EV_BADWE_VLS"] = ddt.Rows[22][4];
                                           row["EV_COT_VLS"] = ddt.Rows[23][4];
                                           row["EV_CHE_VLS"] = ddt.Rows[24][4];
                                           row["EV_BWEX_VLS"] = ddt.Rows[25][4];
                                           row["EV_OTH_VLS"] = ddt.Rows[26][4];
                                           
                                           row["ME_ACT_SEA_MDO"] = ddt.Rows[29][4];
                                           row["ME_ACT_MAN_MDO"] = ddt.Rows[30][4];
                                           row["ME_ACT_AN_MDO"] = ddt.Rows[31][4];
                                           row["ME_ACT_BE_MDO"] = ddt.Rows[32][4];
                                           row["AE_ACT_SEA_MDO"] = ddt.Rows[34][4];
                                           row["AE_ACT_MAN_MDO"] = ddt.Rows[35][4];
                                           row["AE_ACT_AN_MDO"] = ddt.Rows[36][4];
                                           row["AE_ACT_BE_MDO"] = ddt.Rows[37][4];
                                           row["BOIL_ACT_SEA_MDO"] = ddt.Rows[38][4];
                                           row["BOIL_ACT_MAN_MDO"] = ddt.Rows[39][4];
                                           row["BOIL_ACT_AN_MDO"] = ddt.Rows[40][4];
                                           row["BOIL_ACT_BE_MDO"] = ddt.Rows[41][4];
                                           row["FRAMO_ACT_SEA_MDO"] = ddt.Rows[42][4];
                                           row["FRAMO_ACT_MAN_MDO"] = ddt.Rows[43][4];
                                           row["FRAMO_ACT_AN_MDO"] = ddt.Rows[44][4];
                                           row["FRAMO_ACT_BE_MDO"] = ddt.Rows[45][4];
                                           row["IGG_MDO"] = ddt.Rows[46][4];
                                           row["INCINERATOR_MDO"] = ddt.Rows[55][4];

                                           row["EV_SEASTOP_MDO"] = ddt.Rows[47][4];
                                           row["EV_DEV_MDO"] = ddt.Rows[48][4];
                                           row["EV_SLOW_MDO"] = ddt.Rows[49][4];
                                           row["EV_BADWE_MDO"] = ddt.Rows[50][4];
                                           row["EV_COT_MDO"] = ddt.Rows[51][4];
                                           row["EV_CHE_MDO"] = ddt.Rows[52][4];
                                           row["EV_BWEX_MDO"] = ddt.Rows[53][4];
                                           row["EV_OTH_MDO"] = ddt.Rows[54][4];
                                           
                                }
                                      else
                                      {
                                    
                                           row["ME_ACT_SEA_VLS"] = ddt.Rows[1][4];
                                           row["ME_ACT_MAN_VLS"] = ddt.Rows[2][4];
                                           row["ME_ACT_AN_VLS"] = ddt.Rows[3][4];
                                           row["ME_ACT_BE_VLS"] = ddt.Rows[4][4];
                                           //-----------
                                           // row["ME_ACT_SUBT_VLS"] = 555;
                                           //  row["ME_ACT_SUBT_MDO"] = 666;
                                           //----------
                                           row["AE_ACT_SEA_VLS"] = ddt.Rows[6][4];
                                           row["AE_ACT_MAN_VLS"] = ddt.Rows[7][4];
                                           row["AE_ACT_AN_VLS"] = ddt.Rows[8][4];
                                           row["AE_ACT_BE_VLS"] = ddt.Rows[9][4];
                                           row["BOIL_ACT_SEA_VLS"] = ddt.Rows[10][4];
                                           row["BOIL_ACT_MAN_VLS"] = ddt.Rows[11][4];
                                           row["BOIL_ACT_AN_VLS"] = ddt.Rows[12][4];
                                           row["BOIL_ACT_BE_VLS"] = ddt.Rows[13][4];
                                           row["FRAMO_ACT_SEA_VLS"] = ddt.Rows[14][4];
                                           row["FRAMO_ACT_MAN_VLS"] = ddt.Rows[15][4];
                                           row["FRAMO_ACT_AN_VLS"] = ddt.Rows[16][4];
                                           row["FRAMO_ACT_BE_VLS"] = ddt.Rows[17][4];
                                           row["IGG_VLS"] = ddt.Rows[18][4];
                                          
                                           //row["Incinerator_VLS"] = ddt.Rows[28][4];
                                           row["EV_SEASTOP_VLS"] = ddt.Rows[19][4];
                                           row["EV_DEV_VLS"] = ddt.Rows[20][4];
                                           row["EV_SLOW_VLS"] = ddt.Rows[21][4];
                                           row["EV_BADWE_VLS"] = ddt.Rows[22][4];
                                           row["EV_COT_VLS"] = ddt.Rows[23][4];
                                           row["EV_CHE_VLS"] = ddt.Rows[24][4];
                                           row["EV_BWEX_VLS"] = ddt.Rows[25][4];
                                           row["EV_OTH_VLS"] = ddt.Rows[26][4];
                                           //----------
                                           //  row["AE_ACT_SUBT_VLS"] = 777;                               
                                           // row["AE_ACT_SUBT_MDO"] = 888;
                                           //----------
                                          
                                           row["ME_ACT_SEA_MDO"] = ddt.Rows[28][4];
                                           row["ME_ACT_MAN_MDO"] = ddt.Rows[29][4];
                                           row["ME_ACT_AN_MDO"] = ddt.Rows[30][4];
                                           row["ME_ACT_BE_MDO"] = ddt.Rows[31][4];
                                          
                                           row["AE_ACT_SEA_MDO"] = ddt.Rows[33][4];
                                           row["AE_ACT_MAN_MDO"] = ddt.Rows[34][4];
                                           row["AE_ACT_AN_MDO"] = ddt.Rows[35][4];
                                           row["AE_ACT_BE_MDO"] = ddt.Rows[36][4];
                                          
                                           row["BOIL_ACT_SEA_MDO"] = ddt.Rows[37][4];
                                           row["BOIL_ACT_MAN_MDO"] = ddt.Rows[38][4];
                                           row["BOIL_ACT_AN_MDO"] = ddt.Rows[39][4];
                                           row["BOIL_ACT_BE_MDO"] = ddt.Rows[40][4];
                                           row["FRAMO_ACT_SEA_MDO"] = ddt.Rows[41][4];
                                           row["FRAMO_ACT_MAN_MDO"] = ddt.Rows[42][4];
                                           row["FRAMO_ACT_AN_MDO"] = ddt.Rows[43][4];
                                           row["FRAMO_ACT_BE_MDO"] = ddt.Rows[44][4];
                                           row["IGG_MDO"] = ddt.Rows[45][4];
                                           //row["Incinerator_MDO"] = ddt.Rows[56][4];
                                           row["EV_SEASTOP_MDO"] = ddt.Rows[46][4];
                                           row["EV_DEV_MDO"] = ddt.Rows[47][4];
                                           row["EV_SLOW_MDO"] = ddt.Rows[48][4];
                                           row["EV_BADWE_MDO"] = ddt.Rows[49][4];
                                           row["EV_COT_MDO"] = ddt.Rows[50][4];
                                           row["EV_CHE_MDO"] = ddt.Rows[51][4];
                                           row["EV_BWEX_MDO"] = ddt.Rows[52][4];
                                           row["EV_OTH_MDO"] = ddt.Rows[53][4];
                                      
                                      }

                                    }

                                }
                            }



                            if (i == 0)
                                table.TableName = "ArrivalReport";

                            if (check == 0)
                            {

                                newTable = ds.Tables[0];

                            }
                            else
                            {
                                var row1 = ds.Tables[0].Rows[i];
                                newTable.ImportRow(row1);
                            }

                            check++;
                          
                            break;
                            i++;
                        }
                //    }
                //}


                if (newTable.Rows.Count > 0)
                {
                    var protectedsheet = wb.Worksheets.Add(newTable);

                    var projection = protectedsheet.Protect("49WEB$TREET#");
                    projection.InsertColumns = true;
                    projection.InsertRows = true;
                }

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                DateTime today = DateTime.Today;
                Response.Clear();
                Response.BufferOutput = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=ArrivalReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.End();
                }

                Response.Clear();

                Thread.Sleep(300);
                TempData["Success"] = "Data has been Export Successfully";
            }


            return View();

        }

        public ActionResult EditFromDashboard(string reportdate, int vesselid, string Exactdate ="")
        {
            ArrivalReport vd = new ArrivalReport();
            if (Exactdate == "")
            {
                vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselid, "ArrivalReport");
            }
            if (Exactdate != "")
            {
                vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselid, "ArrivalReportR");
            }

            var ID = vd.GetArrivalRList.Select(x => x.Id).FirstOrDefault();
            var arrRBind = vd.GetArrivalRList.Where(x => x.Id == ID).FirstOrDefault(e => e.Id == ID);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["arrivalReportId"] = ID;

            Session["arrivalEditId"] = ID;

            int vslid = vesselid;

            //Session["VesselID"] = vesselid;       

            Session["EditVesselIDArrival"] = vesselid;

            arrRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            int legportid = arrRBind.LegPortId;
            ViewBag.EditArrivalFC = string.Format("Getfuelcons11('{0}');", ID);
            ViewBag.EditLeg = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);
            ViewBag.EditARCargoEdit = string.Format("GetARCargoEdit('{0}');", ID);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", ID);

            return View("Index", arrRBind);

        }

    }
}                                                      