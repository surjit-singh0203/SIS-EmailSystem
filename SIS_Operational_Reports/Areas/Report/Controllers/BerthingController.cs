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
    public class BerthingController : BaseController
    {
        // GET: Report/Berthing
        public ActionResult Index()
        {
            BerthingReport berR = new BerthingReport();
            TempData["berthingReportId"] = null;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            berR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            ViewBag.FuelConsCount = CommonMethods.GetFuelConslist("FCList");
            return View(berR);
        }

        [HttpPost]
        public ActionResult insertBerthingR(BerthingReport _berthing)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _berthing.VesselId = vslid;
           
            //TempData["Success"] = "Record save successfully ";


           


            if (_berthing.Id == 0)
            {
                Session["BR_ID"] = "";
                CommonMethods.InsertUpdateBerthingReport(_berthing, "Insert");
                TempData["Success"] = "Record saved successfully";
            }
            if (_berthing.Id != 0)
            {
               // _berthing.Id = Convert.ToInt32(TempData["berthingReportId"]);
                Session["BR_ID"] = _berthing.Id;
                CommonMethods.InsertUpdateBerthingReport(_berthing, "Update");
                TempData["Success"] = "Record updated successfully";
            }





            return Json(_berthing);
        }

        public JsonResult BindLeg(int Voyid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



                // using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
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

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BindFacility(string portname)
        {
            string facname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {

                using (SqlDataAdapter sda = new SqlDataAdapter("select * from portlist where portname='" + portname + "'", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["FacilityName"].ToString(), Value = tbl.Rows[i]["FacilityName"].ToString() });

                        }

                    }
                }


               // int id = Convert.ToInt32(Session["berthingEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select FacilityName  from BerthingReport where portname='" + portname + "'", ConnectionBulder.con))
                {
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        facname = tbl.Rows[0][0].ToString();

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, FacName = facname, Data = jst }, JsonRequestBehavior.AllowGet);
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where VoyageId=" + Voyid + " and VesselId=" + vslid + " and  IsActive=1 ", ConnectionBulder.con))
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=5  and ConsTypeId not in (1,6)", ConnectionBulder.con))
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select eosp from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=5", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FRobValue.Add(dt.Rows[i]["eosp"]);
                    }
                    ViewBag.FrobValue = FRobValue;
                };

                using (SqlDataAdapter objCMD = new SqlDataAdapter("select FWE from tbl_FuelROB where TableMax_Id=" + NoonReportId + " and VesselId=" + vslid + "  and ReportType_Id=5", ConnectionBulder.con))
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
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, AE_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (AE_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (AE_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (AE_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (AE_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (AE_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (ME_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (ME_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (ME_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (ME_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (ME_CP.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (BLR_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (BLR_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (BLR_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (BLR_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
        //        }



        //        if (FRAMO_ACT_SEA.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (FRAMO_ACT_MAN.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
        //        }

        //        if (FRAMO_ACT_WAIT.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (FRAMO_ACT_BERTH.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (IGG.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 5, vslid, "Insert", "BerthingReport");
        //        }




        //        if (StopageAtSea.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (Deviation.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (SlowSteaming.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (BadWeather.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (COTPrep.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (CargoHeating.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 5, vslid, "Insert", "BerthingReport");
        //        }

        //        if (BWExchange.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 5, vslid, "Insert", "BerthingReport");
        //        }
        //        if (Others.ToString() != "0.00")
        //        {
        //            CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 5, vslid, "Insert", "BerthingReport");
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


                if (Session["BR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 1, ME_CP, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 2, ME_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 3, ME_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 4, ME_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 5, ME_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 6, AE_CP, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 7, AE_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 8, AE_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 9, AE_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 10, AE_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 11, BLR_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 12, BLR_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 13, BLR_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 14, BLR_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 15, FRAMO_ACT_SEA, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 16, FRAMO_ACT_MAN, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 17, FRAMO_ACT_WAIT, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 18, FRAMO_ACT_BERTH, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 19, IGG, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 20, StopageAtSea, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 21, Deviation, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 22, SlowSteaming, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 23, BadWeather, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 24, COTPrep, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 25, CargoHeating, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 26, BWExchange, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 27, Others, 5, vslid, "Insert", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(0, fueltypeid, 28, Incinerator, 5, vslid, "Insert", "BerthingReport");
                }
                if (Session["BR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["BR_ID"]);
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 1, ME_CP, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 2, ME_ACT_SEA, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 3, ME_ACT_MAN, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 4, ME_ACT_WAIT, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 5, ME_ACT_BERTH, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 6, AE_CP, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 7, AE_ACT_SEA, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 8, AE_ACT_MAN, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 9, AE_ACT_WAIT, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 10, AE_ACT_BERTH, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 11, BLR_ACT_SEA, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 12, BLR_ACT_MAN, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 13, BLR_ACT_WAIT, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 14, BLR_ACT_BERTH, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 15, FRAMO_ACT_SEA, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 16, FRAMO_ACT_MAN, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 17, FRAMO_ACT_WAIT, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 18, FRAMO_ACT_BERTH, 2, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 19, IGG, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 20, StopageAtSea, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 21, Deviation, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 22, SlowSteaming, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 23, BadWeather, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 24, COTPrep, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 25, CargoHeating, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 26, BWExchange, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 27, Others, 5, vslid, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 28, Incinerator, 5, vslid, "Update", "BerthingReport");
                }
            }
            return View();
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

                    var SBE_ROB = rootObject.SBE_ROB;
                    var RFA_ROB = rootObject.RFA_ROB;




                    if (Session["BR_ID"].ToString() == "")
                    {

                        CommonMethods.InsertUpdateFuelROB(0, fueltypeid, SBE_ROB, RFA_ROB, 0, 5, vslid, "Insert", "BerthingReport");


                    }
                    if (Session["BR_ID"].ToString() != "")
                    {
                        int noonReportId = Convert.ToInt32(Session["BR_ID"]);

                        CommonMethods.InsertUpdateFuelROB(noonReportId, fueltypeid, SBE_ROB, RFA_ROB, 0, 5, vslid, "Update", "BerthingReport");



                    }



                }
            }
            catch { }
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

                if (Session["BR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateNonRoutineEvents(0, 5, nrevntid, ChartererAccount, Hours, vslid, "BerthingReport", "Insert");
                }
                if (Session["BR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["BR_ID"]);
                    CommonMethods.InsertUpdateNonRoutineEvents(noonReportId, 5, nrevntid, ChartererAccount, Hours, vslid, "BerthingReport", "Update");
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select * from tblNonRoutineCommon where Report_Table_Id=5 and ReportType_Id=" + NoonReportId + " and VesselId=" + vslid + " and IsActive=1", ConnectionBulder.con))
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


                CommonMethods.InsertUpdateBunkerReceipt(0, fueltypeid, Rec, 5, vslid, "Insert", "BerthingReport");


            }
            return View();
        }

        public ActionResult berthinglist(int? pageNo, string firstVal, string dateF, string dateT)
        {
            BerthingReport berR = new BerthingReport();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["berthingReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid, currPage, pageSize);
                return View(berR);
            }
            else if (firstVal == "" && dateF == "")
            {
                berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid, currPage, pageSize);
                return View(berR);
            }
            else if (firstVal != null)
            {
                berR.GetBerthRList = CommonMethods.SearchBerthingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchberthingR", berR);
            }

           // berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid, currPage, pageSize);
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();
       

          
           // berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid);
            return View(berR);
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

                int id = Convert.ToInt32(Session["berthingEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from BerthingReport where id=" + id + "", ConnectionBulder.con))
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
                int id = Convert.ToInt32(Session["berthingEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from BerthingReport where id=" + id + "", ConnectionBulder.con))
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

        public ActionResult Edit(int id)
        {
            BerthingReport vd = new BerthingReport();
            vd.GetBerthRList = CommonMethods.editberthingRList(id, "BerthingReport");
            var berRBind = vd.GetBerthRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["berthingReportId"] = id;

            Session["berthingEditId"] = id;

            int vslid = Convert.ToInt32(Session["VesselID"]);
            berRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            //arrRBind.CargoTanks = GetCargoTankList(id, vslid);
            //arrRBind.BallastTanks = GetBallastTankList(id, vslid);
            //arrRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = berRBind.LegPortId;
            string facname= berRBind.FacilityName;
            string portname = berRBind.PortName;
            int Voyid = berRBind.VoyageId;


            ViewBag.EditBerthingFC = string.Format("Getfuelcons11('{0}');", id);
            ViewBag.EditLegId = string.Format("GetLegEdit('{0}');", legportid);
            //ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", Voyid);

            ViewBag.EditFacName = string.Format("GetFacNameEdit('{0}');", portname);

            ViewBag.EditBRCargoEdit = string.Format("GetBRCargoEdit('{0}');", id);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

            return View("Index", berRBind);

        }

        public JsonResult GetBR_Cargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                // using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId, a.PortName  from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        //  cargoName.Add(dt.Rows[i]["CargoName"]);
                        cargoName.Add(dt.Rows[i]["CargoName"] == DBNull.Value ? "" : dt.Rows[i]["CargoName"].ToString() + " ( " +
                        (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ");
                    }
                    ViewBag.CName = cargoName;
                };
            }
            catch { }

            return Json(new { Result = true, cn = ViewBag.CName }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBRCargo(int LegId, int VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList cargoName = new ArrayList();
            IList<string> cn = new List<string>();
            try
            {
                // using (SqlDataAdapter objCMD = new SqlDataAdapter("select CargoName from LR_Cargo  where VoyageId=" + VoyageId + " and LegPortId=" + LegId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId  from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.SaveDraft=0 and b.IsActive=1 and a.VoyageId=" + VoyageId + " and a.LegPortId=" + LegId + " and a.VesselId=" + vslid + "", ConnectionBulder.con))
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
        public ActionResult InsertBRCargo(string brcargo)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<BRCargo> FrobList = new List<BRCargo>();
            var Result = brcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<BRCargo>>(dd);
            int k = 0;

            foreach (var rootObject in Json)
            {

                BRCargo cls = new BRCargo();

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
                    //if (rootObject.LR_Cargo_Id == 0)
                    //{

                        //SqlDataAdapter adp = new SqlDataAdapter("select Id from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + "", ConnectionBulder.con);
                        SqlDataAdapter adp = new SqlDataAdapter("select MAX(Id) from LR_Cargo  where CargoName='" + Cname + "' and VesselId=" + vslid + " and VoyageId=" + VoyId + " and PortName='" + portName + "'", ConnectionBulder.con);
                        DataTable dt = new DataTable();
                        adp.Fill(dt);

                         LR_CargoID = Convert.ToInt32(dt.Rows[0][0]);

                         lrcrgid = Convert.ToInt32(dt.Rows[0][0]);
                    //}
                    //else
                    //{
                    //    lrcrgid = rootObject.LR_Cargo_Id;
                    //}
                    cls.LR_Cargo_Id = lrcrgid;
                    cls.VesselId = vslid;
                    cls.Qty_Grade1 = rootObject.Qty_Grade1;
                    cls.Qty_Grade2 = rootObject.Qty_Grade2;

                }


                if (Session["BR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateBRCargo(0, cls, "Insert", "BerthingReport");

                }

                if (Session["BR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["BR_ID"]);

                    if (k == 0)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from BR_Cargo where BerthingReport_Id=" + noonReportId + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }
                    }

                    // CommonMethods.InsertUpdateBRCargo(noonReportId, cls, "Update", "BerthingReport");
                    CommonMethods.InsertUpdateBRCargo(noonReportId, cls, "Insert", "BerthingReport");

                }



            }
            return View();
        }
        public JsonResult GetBRCargoEdit(int BerthingReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList arCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*,b.cargoname, b.PortName from BR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id  where  a.VesselId=" + vslid + " and berthingreport_id=" + BerthingReportId + "", ConnectionBulder.con))
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
            CommonMethods.CommonDelete(id, "BerthingReport");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("berthinglist");
        }

        public ActionResult SyncReportDownload(int VoyageId, int id, string ReportDate)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            BerthingReport report = null;
            DataTable dtNonRoutine = new DataTable();
            DataTable dtCargo = new DataTable();

            try
            {
                var vd = new BerthingReport();
                vd.GetBerthRList = CommonMethods.editberthingRList(id, "BerthingReport");
                report = vd.GetBerthRList?.Where(x => x.Id == id).FirstOrDefault();
                if (report != null)
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("SELECT ChartererAccount, Hours FROM tblNonRoutineCommon WHERE Report_Table_Id=5 AND ReportType_Id=" + id + " AND VesselId=" + vslid + " AND IsActive=1 ORDER BY NREvents_Id", ConnectionBulder.con))
                    {
                        adp.Fill(dtNonRoutine);
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("SELECT b.CargoName, b.PortName, a.Qty_Grade1, a.Qty_Grade2 FROM BR_Cargo a INNER JOIN LR_Cargo b ON a.lr_cargo_id=b.Id WHERE a.VesselId=" + vslid + " AND a.berthingreport_id=" + id, ConnectionBulder.con))
                    {
                        adp.Fill(dtCargo);
                    }
                }
            }
            catch { }

            using (XLWorkbook wb = new XLWorkbook())
            {
                AddBerthingNavigationSheet(wb, report, dtNonRoutine);
                AddBerthingEngineSheet(wb, report);
                AddBerthingCargoSheet(wb, report, dtCargo);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = "BerthingReport_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        private static void AddKeyValueSingle(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = value != null ? value.ToString() : "";
            row++;
        }

        private static void ApplyGridTitleStyle(IXLRange range)
        {
            range.Style.Fill.BackgroundColor = XLColor.Gray;
            range.Style.Font.FontColor = XLColor.White;
        }

        private void AddBerthingNavigationSheet(XLWorkbook wb, BerthingReport r, DataTable dtNonRoutine)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;

            ws.Cell(row, 1).Value = "Berthing Report - Navigation";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
                string legText = "";
                string portStatusText = "";
                if (r.LegPortId > 0)
                {
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg FROM VoyageLeg WHERE Id=" + r.LegPortId, ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            if (dt.Rows.Count > 0) legText = dt.Rows[0]["Leg"]?.ToString() ?? "";
                        }
                    }
                    catch { }
                }
                if (string.IsNullOrEmpty(voyNo) && r.VoyageId > 0)
                {
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT VoyageNumber FROM Voyage WHERE Id=" + r.VoyageId, ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            if (dt.Rows.Count > 0) voyNo = dt.Rows[0]["VoyageNumber"]?.ToString() ?? voyNo;
                        }
                    }
                    catch { }
                }
                if (r.PortStatus > 0)
                {
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("SELECT Status FROM tblPortStatus WHERE Id=" + r.PortStatus, ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            if (dt.Rows.Count > 0) portStatusText = dt.Rows[0]["Status"]?.ToString() ?? "";
                        }
                    }
                    catch { }
                }

                AddKeyValueSingle(ws, ref row, "Voy No.", voyNo);
                AddKeyValueSingle(ws, ref row, "Port", r.PortName);
                AddKeyValueSingle(ws, ref row, "Berth Name", r.BerthName);
                AddKeyValueSingle(ws, ref row, "Leg", legText);
                AddKeyValueSingle(ws, ref row, "Facility Name", r.FacilityName);
                AddKeyValueSingle(ws, ref row, "Report Date", r.ReportDate != null ? Convert.ToDateTime(r.ReportDate).ToString("yyyy-MM-dd") : "");
                AddKeyValueSingle(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueSingle(ws, ref row, "Draft Fwd(Mtrs)", r.DraftFwd);
                AddKeyValueSingle(ws, ref row, "Draft Mid(Mtrs)", r.DraftMid);
                AddKeyValueSingle(ws, ref row, "Draft Aft(Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            var rngWeather = ws.Range(row, 1, row, 2);
            rngWeather.Merge();
            rngWeather.Style.Font.Bold = true;
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

            ws.Cell(row, 1).Value = "Manoeuvring";
            var rngManoeuv = ws.Range(row, 1, row, 2);
            rngManoeuv.Merge();
            rngManoeuv.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngManoeuv);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Manoeuvring Hours", r.Manoeuvring_Hrs);
                AddKeyValueSingle(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            var rngNRE = ws.Range(row, 1, row, 3);
            rngNRE.Merge();
            rngNRE.Style.Font.Bold = true;
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

            ws.Cell(row, 1).Value = "Berthing Report Remarks";
            var rngRem = ws.Range(row, 1, row, 2);
            rngRem.Merge();
            rngRem.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngRem);
            row++;
            AddKeyValueSingle(ws, ref row, "Remarks", r?.Remarks);
            ws.Columns().AdjustToContents();
        }

        private void AddBerthingEngineSheet(XLWorkbook wb, BerthingReport r)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;

            ws.Cell(row, 1).Value = "Berthing Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Engine";
            var rngEngine = ws.Range(row, 1, row, 2);
            rngEngine.Merge();
            rngEngine.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngEngine);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueSingle(ws, ref row, "RPM", r.RPM);
                AddKeyValueSingle(ws, ref row, "BHP(hp)", r.BHP);
                AddKeyValueSingle(ws, ref row, "MCR%", r.MCR);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            var rngFuel = ws.Range(row, 1, row, 2);
            rngFuel.Merge();
            rngFuel.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngFuel);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "SBE", r.SBE_ROB);
                AddKeyValueSingle(ws, ref row, "FWE", r.RFA_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            var rngLO = ws.Range(row, 1, row, 2);
            rngLO.Merge();
            rngLO.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngLO);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "MECC", r.LO_HO_Cons_MECC);
                AddKeyValueSingle(ws, ref row, "MECYL", r.LO_HO_Cons_MECYL);
                AddKeyValueSingle(ws, ref row, "AECC", r.LO_HO_Cons_AECC);
                AddKeyValueSingle(ws, ref row, "HYDRAULIC Oil", r.LO_HO_Cons_HYDR_Oil);
            }
            row++;

            ws.Cell(row, 1).Value = "Date & Time";
            var rngDT = ws.Range(row, 1, row, 2);
            rngDT.Merge();
            rngDT.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngDT);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "SBE", r.SBE_DateT != null ? Convert.ToDateTime(r.SBE_DateT).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueSingle(ws, ref row, "FWE", r.RFA_DateT != null ? Convert.ToDateTime(r.RFA_DateT).ToString("yyyy-MM-dd HH:mm") : "");
            }
            ws.Columns().AdjustToContents();
        }

        private void AddBerthingCargoSheet(XLWorkbook wb, BerthingReport r, DataTable dtCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;

            ws.Cell(row, 1).Value = "Berthing Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Cargo";
            var rngCargo = ws.Range(row, 1, row, 2);
            rngCargo.Merge();
            rngCargo.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngCargo);
            row++;
            ws.Cell(row, 1).Value = "Cargo Name";
            ws.Cell(row, 2).Value = "Qty(MT)";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            if (dtCargo != null)
            {
                foreach (DataRow dr in dtCargo.Rows)
                {
                    string cargoName = dr["CargoName"]?.ToString() ?? "";
                    if (dr["PortName"] != DBNull.Value && !string.IsNullOrEmpty(dr["PortName"].ToString()))
                        cargoName += " (" + dr["PortName"].ToString() + ")";
                    ws.Cell(row, 1).Value = cargoName;
                    ws.Cell(row, 2).Value = dr["Qty_Grade1"]?.ToString() ?? "";
                    row++;
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Fresh Water";
            var rngFW = ws.Range(row, 1, row, 2);
            rngFW.Merge();
            rngFW.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngFW);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueSingle(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueSingle(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            var rngBallast = ws.Range(row, 1, row, 2);
            rngBallast.Merge();
            rngBallast.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngBallast);
            row++;
            AddKeyValueSingle(ws, ref row, "ROB (MT)", r?.Ballast_ROB);
            row++;

            ws.Cell(row, 1).Value = "Slops ROB";
            var rngSlops = ws.Range(row, 1, row, 2);
            rngSlops.Merge();
            rngSlops.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngSlops);
            row++;
            if (r != null)
            {
                AddKeyValueSingle(ws, ref row, "Oil", r.SlopsROB_Oil);
                AddKeyValueSingle(ws, ref row, "Water", r.SlopsROB_Water);
                AddKeyValueSingle(ws, ref row, "Total", r.SlopsROB_Total);
            }
            ws.Columns().AdjustToContents();
        }
    }
}