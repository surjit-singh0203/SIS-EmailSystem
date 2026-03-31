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
    // [Authorize]
    public class LoadingController : Controller
    {
        // GET: Report/Loading
        public ActionResult Index()
        {
            TempData["loadingReportId"] = null;
            LoadingReport lp = new LoadingReport();
            return View(lp);
        }

        [HttpPost]
        public ActionResult insertLodingR(LoadingReport _lodingR)
        {

            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _lodingR.VesselId = vslid;
            //CommonMethods.InsertUpdateLoadingReport(_lodingR, "Insert");
            //TempData["Success"] = "Record saved successfully";



            if (_lodingR.Id == 0)
            {
                Session["LR_ID"] = "";
                CommonMethods.InsertUpdateLoadingReport(_lodingR, "Insert");
                TempData["Success"] = "Record saved successfully";
            }
            if (_lodingR.Id != 0)
            {
                //_lodingR.Id = Convert.ToInt32(TempData["loadingReportId"]);
                Session["LR_ID"] = _lodingR.Id;
                CommonMethods.InsertUpdateLoadingReport(_lodingR, "Update");
                TempData["Success"] = "Record updated successfully";
            }



            return Json(_lodingR);
        }

        public JsonResult BindLeg(int Voyid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();

            try
            {



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

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
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

        public ActionResult InsertCargoList(string CargoListing)
        {
            int k = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<CargoList>>(CargoListing);
            int LRID = CommonMethods.GetMaxIDLoad_DischargeReport("LoadingReport");
            var loadRep = CommonMethods.GetSingleLoadingReport(LRID, vslid, "LoadingReport");
            foreach (var rootObject in Json)
            {               
                rootObject.LRId = LRID;
                rootObject.VesselId = vslid;
                rootObject.VoyageId = loadRep.VoyageId;
                rootObject.LegPortId = loadRep.LegPortId;
                rootObject.PortName = loadRep.PortName;

                if(k==0)
                {
                    if (Session["LR_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp=new SqlDataAdapter ("delete from LR_Cargo where LRId="+ LRID + " and VesselId="+vslid+"", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }


                if (Session["LR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingCargo(rootObject, "Insert");
                }
                if (Session["LR_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingCargo(rootObject, "Update");

                   
                }

              
               
            }
            return View();
        }

        public ActionResult InsertStoppage(string StoppageListing)
        {
            int k = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<StoppageList>>(StoppageListing);
            int LRID = CommonMethods.GetMaxIDLoad_DischargeReport("LoadingReport");
            foreach (var rootObject in Json)
            {
                rootObject.LRId = LRID;
                rootObject.VesselId = vslid;
                rootObject.DCId = 0;
                rootObject.LoadingDischarged = false;

                if (k == 0)
                {
                    if (Session["LR_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_Stoppage where LRId=" + LRID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }


                if (Session["LR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");
                }
                if (Session["LR_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Update");
                }

            }
            return View();
        }

        public ActionResult InsertBllastPumpUse(string ballasttank)
        {
            int k = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            //var Json = JsonConvert.DeserializeObject<List<LR_Ballast_PumpUse>>(ballasttank);
            var Json = JsonConvert.DeserializeObject<List<LR_DCR_PumpsUse>>(ballasttank);
            
            int LRID = CommonMethods.GetMaxIDLoad_DischargeReport("LoadingReport");
            foreach (var rootObject in Json)
            {
                rootObject.LRId = LRID;
                rootObject.PumpUseId = 2;
                rootObject.VesselId = vslid;
                rootObject.DCRId = 0;
                //CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");


                if (k == 0)
                {
                    if (Session["LR_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where LRId=" + LRID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }

                if (Session["LR_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");
                }
                if (Session["LR_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Update");
                }
            }
            return View();
        }

        public ActionResult LoadingRList(int? pageNo, string firstVal, string dateF, string dateT)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            LoadingReport Loading = new LoadingReport();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            //if (firstVal == null)
            //{
            //    Loading.LoadingReportList = CommonMethods.GetCommonLoadingReportsList(vslid, currPage, pageSize);
            //    return View(Loading);
            //}
            //else if (firstVal == "")
            //{
            //    Loading.LoadingReportList = CommonMethods.GetCommonLoadingReportsList(vslid, currPage, pageSize);
            //    return View(Loading);
            //}
            //else if (firstVal != null)
            //{
            //    Loading.LoadingReportList = CommonMethods.SearchLoadingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
            //    return PartialView("_searchloadingR", Loading);
            //}

            if (string.IsNullOrEmpty(firstVal))
            {

                Loading.LoadingReportList = CommonMethods.GetCommonLoadingReportsList("", currPage, pageSize);
                return View(Loading);
            }
            else //if (firstVal != null)
            {
                Loading.LoadingReportList = CommonMethods.SearchLoadingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchdischargingR", Loading);
            }


            //Loading.LoadingReportList = CommonMethods.GetCommonLoadingReportsList(vslid, currPage, pageSize);          


            //Loading.LoadingReportList = CommonMethods.GetCommonLoadingReportsList(vslid);
            return View(Loading);
        }

        public ActionResult Edit(int id)
        {
            LoadingReport vd = new LoadingReport();
            vd.LoadingReportList = CommonMethods.editloadingRList(id, "LoadingReport");
            var loadingRBind = vd.LoadingReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["loadingReportId"] = id;

            int vslid = Convert.ToInt32(Session["VesselID"]);

            loadingRBind.LR_Ballast_PumpUseList = GetPumpsINUse(id, vslid);
            //noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            //noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = loadingRBind.LegPortId;
            string portname = loadingRBind.PortName;
            ViewBag.EditLRCargo = string.Format("GetLRCargoEdit('{0}');", id);
            ViewBag.EditStopR = string.Format("GetStopR('{0}');", id);
            ViewBag.GetLegEdit = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("bindportEdit('{0}');", legportid);
            return View("Index", loadingRBind);

        }


        public static List<LR_DCR_PumpsUse> GetPumpsINUse(int id,int vslid)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId="+ id + " and  a.VesselId="+vslid+" and DCRId=0 and a.PumpUseId=2", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new LR_DCR_PumpsUse
                    {
                        PumpId = Convert.ToInt32(dt.Rows[i]["PumpId"]),
                        PumpUseId = Convert.ToInt32(dt.Rows[i]["PumpUseId"]),
                        Name = dt.Rows[i]["Name"].ToString(),
                        Rate = Convert.ToDecimal(dt.Rows[i]["Rate"]),


                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public JsonResult GetLRCargoEdit(int LoadingReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList lrCargo = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("SELECT a.*, CONVERT(VARCHAR(20),LoadingDatetime,120)  LDT,CONVERT(VARCHAR(20),ActualCompDateTime,120)  ADT from LR_Cargo a  where  a.VesselId=" + vslid + " and a.LRId=" + LoadingReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    ViewBag.CountLRCargo = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        lrCargo.Add(dt.Rows[i]["CargoName"]);
                        lrCargo.Add(dt.Rows[i]["LDT"]);
                        lrCargo.Add(dt.Rows[i]["TerminalLoadingRate"]);
                        lrCargo.Add(dt.Rows[i]["LoadingRateAccepted"]);
                        lrCargo.Add(dt.Rows[i]["AverageAchievedLoadingRate"]);
                        lrCargo.Add(dt.Rows[i]["No_Manifold_Hoses_by_Terminal"]);
                        lrCargo.Add(dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"]);

                        lrCargo.Add(dt.Rows[i]["No_Manifold_Hoses_by_Vessel"]);
                        lrCargo.Add(dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"]);
                        lrCargo.Add(dt.Rows[i]["ShoreLineDistance"]);
                        lrCargo.Add(dt.Rows[i]["QuantityOnboard"]);
                        lrCargo.Add(dt.Rows[i]["BalanceQuantityLoaded"]);
                        lrCargo.Add(dt.Rows[i]["ADT"]);
                       
                    }

                    ViewBag.LRCargoList = lrCargo;
                };




            }
            catch { }

            return Json(new { Result = true, lrc = ViewBag.LRCargoList, cntlrc = ViewBag.CountLRCargo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetStopR(int LoadingReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList stpr = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*, CONVERT(VARCHAR(20),DateTimeFrom,120)  fromdt,CONVERT(VARCHAR(20),DateTimeTo,120)  todate from LR_Stoppage a  where LoadingDischarged=0 and  a.VesselId=" + vslid + " and a.LRId=" + LoadingReportId + "", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    objCMD.Fill(dt);

                    ViewBag.CountStopR = dt.Rows.Count;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        stpr.Add(dt.Rows[i]["Stoppage"]);
                        stpr.Add(dt.Rows[i]["Reason"]);
                        stpr.Add(dt.Rows[i]["fromdt"]);
                        stpr.Add(dt.Rows[i]["todate"]);                       

                    }

                    ViewBag.StopRList = stpr;
                };




            }
            catch { }

            return Json(new { Result = true, stpr = ViewBag.StopRList, cntstopr = ViewBag.CountStopR }, JsonRequestBehavior.AllowGet);
        }
    }
}