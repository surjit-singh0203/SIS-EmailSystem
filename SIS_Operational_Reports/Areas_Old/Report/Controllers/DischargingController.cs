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
    public class DischargingController : Controller
    {
        // GET: Report/Discharging
        public ActionResult Index1()
        {
            return View();
        }
        public ActionResult Index()
        {
            TempData["dischargingReportId"] = null;
            DischargingReport lp = new DischargingReport();

            lp.DSCargoClone = new DSCargoList()
            {
                Id = 0,
                CargoName = "",
                Terminal_Acceptable_Discharging_Rate = 0.00m,
                Discharging_pressure_Requested = 0.00m,
                Average_Discharge_Rate_ByVessel = 0.00m,
                Average_Discharge_pressure_ByVessel = 0.00m,
                No_of_Pumps_Use = 0,
                No_Manifold_Hoses_by_Terminal = 0,
                Size_of_Manifold_Hoses_by_Terminal = 0,
                No_Manifold_Hoses_by_Vessel = 0,
                Size_of_Manifold_Hoses_by_Vessel = 0,

                Total_CargoDischarged = 0,
                Balance_Cargo_ToBe_Deischarged = 0,

            };
            return View(lp);
        }



        [HttpPost]
        public ActionResult insertDischareR(DischargingReport _lodingR)
        {

            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            _lodingR.VesselId = vslid;
            // CommonMethods.InsertUpdateDischargeReport(_lodingR, "Insert");
            //TempData["Success"] = "Record saved successfully";



            if (_lodingR.Id == 0)
            {
                Session["DIS_ID"] = "";
                CommonMethods.InsertUpdateDischargeReport(_lodingR, "Insert");
                TempData["Success"] = "Record saved successfully";
            }
            if (_lodingR.Id != 0)
            {
                //_lodingR.Id = Convert.ToInt32(Session["DisCId"]);
                Session["DIS_ID"] = _lodingR.Id;
                CommonMethods.InsertUpdateDischargeReport(_lodingR, "Update");
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
            int k = 0; int DsID = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<DSCargoList>>(CargoListing);
            if (Session["DIS_ID"].ToString() != "")
            {
                DsID = Convert.ToInt32(Session["DIS_ID"]);
            }
            else
            {
                DsID = CommonMethods.GetMaxIDLoad_DischargeReport("DischargingReport");
            }
            var loadRep = CommonMethods.GetSingleLoadingReport(DsID, vslid, "LoadingReport");
            foreach (var rootObject in Json)
            {
                rootObject.DSId = DsID;
                rootObject.VesselId = vslid;
                rootObject.VoyageId = loadRep.VoyageId;
                rootObject.LegPortId = loadRep.LegPortId;
                rootObject.PortName = loadRep.PortName;


                if (k == 0)
                {
                    if (Session["DIS_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from DS_Cargo where LRId=" + DsID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }


                if (Session["DIS_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateDischargingCargo(rootObject, "Insert");
                }
                if (Session["DIS_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateDischargingCargo(rootObject, "Update");


                }


                //CommonMethods.InsertUpdateDischargingCargo(rootObject, "Insert");
            }
            return View();
        }

        public ActionResult InsertStoppage(string StoppageListing)
        {
            int k = 0; int DCID = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<StoppageList>>(StoppageListing);
            if (Session["DIS_ID"].ToString() != "")
            {
                DCID = Convert.ToInt32(Session["DIS_ID"]);
            }
            else
            {
                DCID = CommonMethods.GetMaxIDLoad_DischargeReport("DischargingReport");
            }
            foreach (var rootObject in Json)
            {
                rootObject.LRId = 0;
                rootObject.VesselId = vslid;
                rootObject.DCId = DCID;
                rootObject.LoadingDischarged = true;
               // CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");


                if (k == 0)
                {
                    if (Session["DIS_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_Stoppage where DCId=" + DCID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }

                if (Session["DIS_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");
                }
                if (Session["DIS_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Update");
                }

            }
            return View();
        }

        public ActionResult InsertBllastPumpUse(string ballasttank)
        {
            int k = 0; int DSID = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<LR_DCR_PumpsUse>>(ballasttank);
            if (Session["DIS_ID"].ToString() != "")
            {
                DSID = Convert.ToInt32(Session["DIS_ID"]);
            }
            else
            {
                DSID = CommonMethods.GetMaxIDLoad_DischargeReport("DischargingReport");
            }
            foreach (var rootObject in Json)
            {
                rootObject.LRId = 0;
                rootObject.PumpUseId = 2;
                rootObject.DCRId = DSID;
                rootObject.VesselId = vslid;
                //CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");



                if (k == 0)
                {
                    if (Session["DIS_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }

                if (Session["DIS_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");
                }
                if (Session["DIS_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Update");
                }

            }
            return View();
        }

        public ActionResult InsertCargoPumpUse(string CargoPumpsUse)
        {
            int k = 0;int DSID = 0;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            var Json = JsonConvert.DeserializeObject<List<LR_DCR_PumpsUse>>(CargoPumpsUse);
            if (Session["DIS_ID"].ToString() != "")
            {
                DSID = Convert.ToInt32(Session["DIS_ID"]);
            }
            else
            {
                DSID = CommonMethods.GetMaxIDLoad_DischargeReport("DischargingReport");
            }
            foreach (var rootObject in Json)
            {
                rootObject.LRId = 0;
                rootObject.PumpUseId = 1;
                rootObject.DCRId = DSID;
                rootObject.VesselId = vslid;
                //CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");


                if (k == 0)
                {
                    if (Session["DIS_ID"].ToString() != "")
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            k = 1;
                        }


                    }
                }

                if (Session["DIS_ID"].ToString() == "")
                {
                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");
                }
                if (Session["DIS_ID"].ToString() != "")
                {
                    //int noonReportId = Convert.ToInt32(Session["NR_ID"]);

                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Update");
                }

            }
            return View();
        }


        //public List<DSCargoList> GetDischargeCargo(int VNo, int VLeg)
        //{
        //    List<DSCargoList> ftype = new List<DSCargoList>();

        //    using (SqlDataAdapter adp = new SqlDataAdapter(" select * from LR_Cargo where VoyageId="+ VNo + " and LegPortId="+ VLeg + " ", ConnectionBulder.con))
        //    {
        //        // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            ftype.Add(new DSCargoList
        //            {
        //                Id = Convert.ToInt32(dt.Rows[i]["Id"]),
        //                CargoName = dt.Rows[i]["CargoName"].ToString(),
        //                Terminal_Acceptable_Discharging_Rate = 0.00m,
        //                Discharging_pressure_Requested = 0.00m,
        //                Average_Discharge_Rate_ByVessel = 0.00m,
        //                Average_Discharge_pressure_ByVessel = 0.00m,
        //                No_of_Pumps_Use = 0,
        //                No_Manifold_Hoses_by_Terminal = 0,
        //                Size_of_Manifold_Hoses_by_Terminal = 0,
        //                No_Manifold_Hoses_by_Vessel = 0,
        //                Size_of_Manifold_Hoses_by_Vessel = 0,

        //                Total_CargoDischarged = 0,
        //                Balance_Cargo_ToBe_Deischarged = 0,

        //            }); 
        //        }
        //        // con.Close();
        //    }

        //    return ftype;
        //}

        public JsonResult GetDischargeCargos(int VoyId, int VLeg)
        {

            List<DSCargoList> ftype = new List<DSCargoList>();

            using (SqlDataAdapter adp = new SqlDataAdapter(" select * from LR_Cargo where VoyageId=" + VoyId + " and LegPortId=" + VLeg + " ", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DSCargoList
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        CargoName = dt.Rows[i]["CargoName"].ToString(),
                        Terminal_Acceptable_Discharging_Rate = 0.00m,
                        Discharging_pressure_Requested = 0.00m,
                        Average_Discharge_Rate_ByVessel = 0.00m,
                        Average_Discharge_pressure_ByVessel = 0.00m,
                        No_of_Pumps_Use = 0,
                        No_Manifold_Hoses_by_Terminal = 0,
                        Size_of_Manifold_Hoses_by_Terminal = 0,
                        No_Manifold_Hoses_by_Vessel = 0,
                        Size_of_Manifold_Hoses_by_Vessel = 0,

                        Total_CargoDischarged = 0,
                        Balance_Cargo_ToBe_Deischarged = 0,

                    });
                }
                // con.Close();
            }

            return Json(new { Result = true, ftype, }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDischargeCargosEdit(int VoyId, int VLeg)
        {
            int dcid = Convert.ToInt32(TempData["dischargingReportId"]);
            Session["DisCId"] = dcid;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<DSCargoList> ftype = new List<DSCargoList>();

            //using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT,CONVERT(VARCHAR(20),ActualCompDateTime,127)  ADT from DS_Cargo a where a.VoyageId=" + VoyId + " and a.LegPortId=" + VLeg + " and a.LRId="+dcid+" and a.vesselid="+vslid+" ", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT,CONVERT(VARCHAR(20),ActualCompDateTime,127)  ADT from DS_Cargo a where a.VoyageId=" + VoyId + " and  a.LRId=" + dcid + " and a.vesselid=" + vslid + " ", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DSCargoList
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        CargoName = dt.Rows[i]["CargoName"].ToString(),
                        DDT = dt.Rows[i]["DDT"].ToString(),
                        Terminal_Acceptable_Discharging_Rate = Convert.ToDecimal(dt.Rows[i]["Terminal_Acceptable_Discharging_Rate"]),
                        Discharging_pressure_Requested = Convert.ToDecimal(dt.Rows[i]["Discharging_pressure_Requested"]),
                        Average_Discharge_Rate_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_Rate_ByVessel"]),
                        Average_Discharge_pressure_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_pressure_ByVessel"]),
                        No_of_Pumps_Use = Convert.ToInt32(dt.Rows[i]["No_of_Pumps_Use"]),
                        No_Manifold_Hoses_by_Terminal = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Terminal"]),
                        Size_of_Manifold_Hoses_by_Terminal = Convert.ToInt32(dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"]),
                        No_Manifold_Hoses_by_Vessel = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Vessel"]),
                        Size_of_Manifold_Hoses_by_Vessel = Convert.ToInt32(dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"]),

                        Total_CargoDischarged = Convert.ToInt32(dt.Rows[i]["Total_CargoDischarged"]),
                        Balance_Cargo_ToBe_Deischarged = Convert.ToInt32(dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"]),
                        ADT = dt.Rows[i]["ADT"].ToString(),

                    });
                }
                // con.Close();
            }

            return Json(new { Result = true, ftype, }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DischargeRList(int? pageNo, string firstVal, string dateF, string dateT)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            DischargingReport Dis = new DischargingReport();


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            //if (firstVal == null)
            //{
            //    Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
            //    return View(Dis);
            //}
            //else if (firstVal == "")
            //{
            //    Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
            //    return View(Dis);
            //}
            //else if (firstVal != null)
            //{
            //    Dis.DischargingReportList = CommonMethods.SearchDischargingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
            //    return PartialView("_searchdischargingR", Dis);
            //}

            if (string.IsNullOrEmpty(firstVal))
            {

                Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList("", currPage, pageSize);
                return View(Dis);
            }
            else //if (firstVal != null)
            {
                Dis.DischargingReportList = CommonMethods.SearchDischargingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchdischargingR", Dis);
            }


            //Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();


            //Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid);
            return View(Dis);
        }

        public ActionResult Delete(int id,int  VId)
        {
          return  RedirectToAction("DischargeRList");
        }

        public ActionResult Edit(int id)
        {
            DischargingReport vd = new DischargingReport();
            vd.DischargingReportList = CommonMethods.editdischargingRList(id, "DischargingReport");
            var loadingRBind = vd.DischargingReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["dischargingReportId"] = id;

            int vslid = Convert.ToInt32(Session["VesselID"]);

            loadingRBind.LR_Ballast_PumpUseList = GetPumpsINUse(id, vslid);

            loadingRBind.DC_Cargo_PumpUseList = GetPumpsINUse1(id, vslid);

            
            //noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            //noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = loadingRBind.LegPortId;
            string portname = loadingRBind.PortName;
           
            ViewBag.EditStopR = string.Format("GetStopR('{0}');", id);
            ViewBag.GetLegEdit = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("bindportEdit('{0}');", legportid);
            ViewBag.EditDisRCargo = string.Format("GetCargoListEdit('{0}');", legportid);
            return View("Index", loadingRBind);

        }
        public static List<LR_DCR_PumpsUse> GetPumpsINUse1(int id,int vslid)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId=" + id + " and a.PumpUseId=1", ConnectionBulder.con))
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
        public static List<LR_DCR_PumpsUse> GetPumpsINUse(int id, int vslid)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId="+id+" and a.PumpUseId=2", ConnectionBulder.con))
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

        public JsonResult GetStopR(int DischargingReportId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            ArrayList stpr = new ArrayList();

            IList<string> nrc = new List<string>();
            try
            {
                using (SqlDataAdapter objCMD = new SqlDataAdapter("select a.*, CONVERT(VARCHAR(20),DateTimeFrom,120)  fromdt,CONVERT(VARCHAR(20),DateTimeTo,120)  todate from LR_Stoppage a  where  a.VesselId=" + vslid + " and a.LRId=0 and dcid="+DischargingReportId+"", ConnectionBulder.con))
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