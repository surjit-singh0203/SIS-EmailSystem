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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);

            
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
                int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);


                // using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "' and VesselId ='"+vslid+"'", ConnectionBulder.con))
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


        public JsonResult BindFacility(string portname, string Berth_Name)
        {
            string facname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {

                int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);


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

                using (SqlDataAdapter sda = new SqlDataAdapter("select FacilityName  from BerthingReport where portname='" + portname + "' and VesselId="+vslid+" and BerthName="+ Berth_Name + "", ConnectionBulder.con))
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
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);

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

        public JsonResult Getfuelcons(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select value, ConsTypeId from Fuel_Cons_NR where Noon_Report_Id=" + NoonReportId + " and VesselId=" + vslid + " and ReportType_Id=5  and ConsTypeId not in (1,6) order by id asc", ConnectionBulder.con))
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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
                    CommonMethods.InsertUpdateFuelConsNR(noonReportId, fueltypeid, 18, FRAMO_ACT_BERTH, 5, vslid, "Update", "BerthingReport");
                    
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
                //int vslid = Convert.ToInt32(Session["VesselID"]);
                int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);

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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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

        public ActionResult berthinglist(int? pageNo, string firstVal, string dateF, string dateT, string Vessel)
        {
            BerthingReport berR = new BerthingReport();
            berR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            TempData["berthingReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid, currPage, pageSize);
                berR.VesselId = Convert.ToInt32(Vessel);
                return View(berR);
            }
            else if (firstVal == "" && dateF == "" && Vessel == "")
            {
                berR.GetBerthRList = CommonMethods.GetBerthingReportList(vslid, currPage, pageSize);
                berR.VesselId = Convert.ToInt32(Vessel);
                return View(berR);
            }
            else if (firstVal != null || dateF != "" || Vessel != "")
            {
                berR.GetBerthRList = CommonMethods.SearchBerthingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                berR.VesselId = Convert.ToInt32(Vessel);
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
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);

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

                int id = Convert.ToInt32(Session["berthingEditId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from BerthingReport where id=" + id + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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
                int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);


                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id=" + Legid + " and VesselId='"+vslid+"'", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where id=" + Legid + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from BerthingReport where id=" + id + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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

        public ActionResult Edit(int id, int vesselid)
        {
            BerthingReport vd = new BerthingReport();
            vd.GetBerthRList = CommonMethods.editberthingRList(id, vesselid, "BerthingReport");
            var berRBind = vd.GetBerthRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["berthingReportId"] = id;

            Session["berthingEditId"] = id;

            int vslid = vesselid;
           // Session["VesselID"] = vesselid;

            Session["EditVesselIDBerthing"] = vesselid;
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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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

        public JsonResult GetBRCargo(int LegId, int VoyageId)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
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
        public ActionResult InsertBRCargo(string brcargo)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
            List<BRCargo> FrobList = new List<BRCargo>();
            var Result = brcargo;
            string json = Result.ToString();
            string dd = json;
            var Json = JsonConvert.DeserializeObject<List<BRCargo>>(dd);
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
                    cls.Id = rootObject.Id;

                }


                if (Session["BR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateBRCargo(0, cls, "Insert", "BerthingReport");

                }
                if (Session["BR_ID"].ToString() != "")
                {
                    int noonReportId = Convert.ToInt32(Session["BR_ID"]);

                    CommonMethods.InsertUpdateBRCargo(noonReportId, cls, "Update", "BerthingReport");

                }



            }
            return View();
        }
        public JsonResult GetBRCargoEdit(int BerthingReportId, int? vesselId = null)
        {
            // Resolve vessel id with three fallbacks. Session was the original source but it
            // can be 0 / stale when the user navigates to the Edit page from a deep link, with
            // a new session, or from a different vessel context — which makes the cargo SQL
            // filter `VesselId=0`, return zero rows, and the form falls back to its empty
            // template (one row with 0.000 qty and no cargo name).
            //  1. URL query parameter ?vesselId=... (most explicit)
            //  2. Look up BerthingReport.VesselId by primary key (always correct if report exists)
            //  3. Session["EditVesselIDBerthing"] (legacy behavior)
            int vslid = vesselId.GetValueOrDefault();
            if (vslid <= 0)
            {
                try
                {
                    using (var cmd = new SqlCommand(
                        "SELECT TOP 1 VesselId FROM BerthingReport WHERE Id=" + BerthingReportId,
                        ConnectionBulder.con))
                    {
                        if (ConnectionBulder.con.State != ConnectionState.Open) ConnectionBulder.con.Open();
                        var o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value) vslid = Convert.ToInt32(o);
                    }
                }
                catch { }
            }
            if (vslid <= 0) vslid = Convert.ToInt32(Session["EditVesselIDBerthing"]);
            ArrayList arCargoName = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                // Cargo names: try the direct lr_cargo_id FK first; if that's a dead reference
                // (LR_Cargo row was deleted/renumbered by a Loading Report re-save), fall back to
                // leg-scoped lookup (match by LegPortId + VesselId). Positional pairing uses
                // reverse Id — the order LR_Cargo gets re-inserted in matches the reverse of
                // BR_Cargo save order. Same pattern as the email template's CTE-based query.
                string cargoQuery = @"
WITH br_rows AS (
    SELECT *, ROW_NUMBER() OVER (ORDER BY Id ASC) AS _pos
    FROM BR_Cargo
    WHERE VesselId = " + vslid + @" AND berthingreport_id = " + BerthingReportId + @"
),
report_ctx AS (
    SELECT LegPortId, VoyageId
    FROM BerthingReport
    WHERE Id = " + BerthingReportId + @" AND VesselId = " + vslid + @"
),
leg_lr AS (
    SELECT b.CargoName, b.PortName,
           ROW_NUMBER() OVER (ORDER BY b.Id DESC) AS _pos
    FROM LR_Cargo b
    INNER JOIN report_ctx rc ON b.LegPortId = rc.LegPortId
    WHERE b.VesselId = " + vslid + @"
),
voyage_lr AS (
    -- Secondary fallback: match by VoyageId. Discharging-only legs (e.g. Sharjah to
    -- Sharjah) won't have LR_Cargo for the LegPortId, but the voyage's loading-leg
    -- cargoes are still associated with the same VoyageId.
    SELECT b.CargoName, b.PortName,
           ROW_NUMBER() OVER (ORDER BY b.Id DESC) AS _pos
    FROM LR_Cargo b
    INNER JOIN report_ctx rc ON b.VoyageId = rc.VoyageId
    WHERE b.VesselId = " + vslid + @"
),
vessel_lr AS (
    -- Final fallback: vessel-only. Pick the most recent LRId for this vessel that has
    -- at least as many cargo rows as the report, and pair positionally (newest LR_Cargo
    -- with oldest BR_Cargo, matching the original MAX(Id) save order).
    SELECT b.CargoName, b.PortName,
           ROW_NUMBER() OVER (ORDER BY b.Id DESC) AS _pos
    FROM LR_Cargo b
    INNER JOIN (
        SELECT TOP 1 LRId
        FROM LR_Cargo
        WHERE VesselId = " + vslid + @"
        GROUP BY LRId
        HAVING COUNT(*) >= (SELECT COUNT(*) FROM br_rows)
        ORDER BY MAX(Id) DESC
    ) recent ON b.LRId = recent.LRId
    WHERE b.VesselId = " + vslid + @"
)
SELECT a.*,
       COALESCE(direct.cargoname, leg.CargoName, voyage.CargoName, vessel.CargoName) AS CargoName,
       COALESCE(direct.PortName,  leg.PortName,  voyage.PortName,  vessel.PortName)  AS PortName
FROM br_rows a
LEFT JOIN LR_Cargo direct ON a.lr_cargo_id = direct.Id AND a.VesselId = direct.VesselId
LEFT JOIN leg_lr leg      ON leg._pos = a._pos
LEFT JOIN voyage_lr voyage ON voyage._pos = a._pos
LEFT JOIN vessel_lr vessel ON vessel._pos = a._pos
ORDER BY a.Id";
                using (SqlDataAdapter objCMD = new SqlDataAdapter(cargoQuery, ConnectionBulder.con))
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
            CommonMethods.CommonDelete(id, "BerthingReport");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("berthinglist");
        }

        public ActionResult DownloadExcel(int id, int vesselid)
        {
            DailyNoonReport vd = new DailyNoonReport();
            int i = 0; int check = 0;
            XLWorkbook wb = new XLWorkbook();

            DataTable newTable = new DataTable();
            
            DataSet ds = CommonMethods.ExportReportList(id, vesselid, "BerthingReport");
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
                        using (SqlDataAdapter adp = new SqlDataAdapter("select * from tbl_FuelROB where ReportType_Id=5 and TableMax_Id =" + id + " and VesselId =" + vesselid + "", ConnectionBulder.con))
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
                        using (SqlDataAdapter adp = new SqlDataAdapter("select * from Fuel_Cons_NR  where ReportType_Id=5 and VesselId =" + vesselid + " and Noon_Report_Id =" + id + "", ConnectionBulder.con))
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
                        table.TableName = "BerthingReport";

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
                Response.AddHeader("content-disposition", "attachment;filename=BerthingReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");

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

        public ActionResult EditFromDashboard(string reportdate, int vesselid)
        {
            BerthingReport vd = new BerthingReport();
            vd.GetBerthRList = CommonMethods.editberthingRListDashboard(reportdate, vesselid, "BerthingReport");

            var ID = vd.GetBerthRList.Select(x => x.Id).FirstOrDefault();
            var berRBind = vd.GetBerthRList.Where(x => x.Id == ID).FirstOrDefault(e => e.Id == ID);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["berthingReportId"] = ID;

            Session["berthingEditId"] = ID;

            int vslid = vesselid;
            // Session["VesselID"] = vesselid;

            Session["EditVesselIDBerthing"] = vesselid;
            berRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            //arrRBind.CargoTanks = GetCargoTankList(id, vslid);
            //arrRBind.BallastTanks = GetBallastTankList(id, vslid);
            //arrRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = berRBind.LegPortId;
            string facname = berRBind.FacilityName;
            string portname = berRBind.PortName;



            ViewBag.EditBerthingFC = string.Format("Getfuelcons11('{0}');", ID);
            ViewBag.EditLegId = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);

            ViewBag.EditFacName = string.Format("GetFacNameEdit('{0}');", portname);

            ViewBag.EditBRCargoEdit = string.Format("GetBRCargoEdit('{0}');", ID);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", ID);

            return View("Index", berRBind);

        }
    }
}