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
    public class DischargingController : BaseController
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

            int vslid = Convert.ToInt32(Session["VesselID"]);
            lp.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
           

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

                if (_lodingR.SaveDraft == false)
                {
                    TempData["Success"] = "Record saved successfully";
                }

                
            }
            if (_lodingR.Id != 0)
            {
                //_lodingR.Id = Convert.ToInt32(Session["DisCId"]);
                Session["DIS_ID"] = _lodingR.Id;
                CommonMethods.InsertUpdateDischargeReport(_lodingR, "Update");
                if (_lodingR.SaveDraft == false)
                {
                    TempData["Success"] = "Record updated successfully";
                }
              
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
            //var loadRep = CommonMethods.GetSingleLoadingReport(DsID, vslid, "LoadingReport");
            var loadRep = CommonMethods.GetSingleLoadingReport(DsID, vslid, "DischargingReport");
            foreach (var rootObject in Json)
            {
                rootObject.DSId = DsID;
                rootObject.VesselId = vslid;
                rootObject.VoyageId = loadRep.VoyageId;
                rootObject.LegPortId = loadRep.LegPortId;
                rootObject.PortName = loadRep.PortName;
                var Cname = rootObject.CargoName;
                Cname = Regex.Replace(Cname, @"\s+\(.*\)", "");
                rootObject.CargoName = Cname;

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

                    // CommonMethods.InsertUpdateDischargingCargo(rootObject, "Update");
                    CommonMethods.InsertUpdateDischargingCargo(rootObject, "Insert");

                }


                //CommonMethods.InsertUpdateDischargingCargo(rootObject, "Insert");
            }
            return View();
        }

        public ActionResult InsertStoppage(string StoppageListing)
        {
            try
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
                    if (rootObject.DCId == 0)
                    {
                        rootObject.DCId = DCID;
                    }
                    rootObject.LRId = 0;
                    rootObject.VesselId = vslid;
                    //rootObject.DCId = DCID;
                    rootObject.LoadingDischarged = true;
                    // CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");


                    if (k == 0)
                    {
                        if (Session["DIS_ID"].ToString() != "")
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_Stoppage where DCId=" + rootObject.DCId + " and VesselId=" + vslid + "", ConnectionBulder.con))
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
                        CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");
                        // CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Update");
                    }

                }
            }
            catch { }
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
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and pumpuseid=2 and VesselId=" + vslid + "", ConnectionBulder.con))
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

                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");
                    //CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Update");
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
                        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and pumpuseid=1 and VesselId=" + vslid + "", ConnectionBulder.con))
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

                    CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Insert");
                    //CommonMethods.InsertUpdateLoadingBallast_PumpUse(rootObject, "Update");
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

        public JsonResult GetLDischargeCargos(int VoyId)
        {

            List<DSCargoList> ftype = new List<DSCargoList>();

            //using (SqlDataAdapter adp = new SqlDataAdapter(" select * from LR_Cargo where VoyageId=" + VoyId + " and LegPortId=" + VLeg + " ", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId, a.PortName  from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.IsActive=1 and b.SaveDraft=0 and a.VoyageId=" + VoyId + "", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DSCargoList
                    {
                        //Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        // CargoName = dt.Rows[i]["CargoName"] == DBNull.Value ? "" : dt.Rows[i]["CargoName"].ToString(),
                        CargoName = (dt.Rows[i]["CargoName"] == DBNull.Value ? "" : dt.Rows[i]["CargoName"].ToString()) + " ( " +
                        (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ",
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

        public JsonResult GetDischargeCargos(int VoyId, int VLeg)
        {

            List<DSCargoList> ftype = new List<DSCargoList>();

            //using (SqlDataAdapter adp = new SqlDataAdapter(" select * from LR_Cargo where VoyageId=" + VoyId + " and LegPortId=" + VLeg + " ", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select Distinct a.CargoName,a.VoyageId,a.LegPortId  from LR_Cargo a inner join LoadingReport b on a.LRId=b.Id where b.IsActive=1 and b.SaveDraft=0 and a.VoyageId=" + VoyId + " and a.LegPortId=" + VLeg + " ", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DSCargoList
                    {
                        //Id = Convert.ToInt32(dt.Rows[i]["Id"]),
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

        public JsonResult BindPort_Edit(int Voyid)
        {
            string prtname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
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

                int id = Convert.ToInt32(Session["DisCId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from DischargingReport where id=" + id + "", ConnectionBulder.con))
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
                int id = Convert.ToInt32(Session["DisCId"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from DischargingReport where id=" + id + "", ConnectionBulder.con))
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

        public JsonResult GetDischargeCargosEdit(int VoyId, int VLeg)
        {
            //int dcid = Convert.ToInt32(TempData["dischargingReportId"]);
            //Session["DisCId"] = dcid;
            int dcid = Convert.ToInt32(Session["DisCId"]);
            int vslid = Convert.ToInt32(Session["VesselID"]);
            List<DSCargoList> ftype = new List<DSCargoList>();

            //using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT,CONVERT(VARCHAR(20),ActualCompDateTime,127)  ADT from DS_Cargo a where a.VoyageId=" + VoyId + " and a.LegPortId=" + VLeg + " and a.LRId="+dcid+" and a.vesselid="+vslid+" ", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT,CONVERT(VARCHAR(20),EstCompDateTime,127)  EDT, CONVERT(VARCHAR(20),ActualCompDateTime,127)  ADT, PortName from DS_Cargo a where a.VoyageId=" + VoyId + " and  a.LRId=" + dcid + " and a.vesselid=" + vslid + " ", ConnectionBulder.con))
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
                        CargoName = dt.Rows[i]["CargoName"] == DBNull.Value ? "" : dt.Rows[i]["CargoName"].ToString() + " ( " +
                       (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ",
                      
                        DDT = dt.Rows[i]["DDT"] == DBNull.Value ? "" : dt.Rows[i]["DDT"].ToString(),
                        Terminal_Acceptable_Discharging_Rate = Convert.ToDecimal(dt.Rows[i]["Terminal_Acceptable_Discharging_Rate"] == DBNull.Value ? 0 : dt.Rows[i]["Terminal_Acceptable_Discharging_Rate"]),
                        Discharging_pressure_Requested = Convert.ToDecimal(dt.Rows[i]["Discharging_pressure_Requested"] == DBNull.Value ? 0 : dt.Rows[i]["Discharging_pressure_Requested"]),
                        Average_Discharge_Rate_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_Rate_ByVessel"] == DBNull.Value ? 0 : dt.Rows[i]["Average_Discharge_Rate_ByVessel"]),
                        Average_Discharge_pressure_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_pressure_ByVessel"] == DBNull.Value ? 0 : dt.Rows[i]["Average_Discharge_pressure_ByVessel"]),
                        //No_of_Pumps_Use = Convert.ToDecimal(dt.Rows[i]["No_of_Pumps_Use"] == DBNull.Value ? 0 : dt.Rows[i]["No_of_Pumps_Use"]),
                        //No_Manifold_Hoses_by_Terminal = Convert.ToDecimal(dt.Rows[i]["No_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : dt.Rows[i]["No_Manifold_Hoses_by_Terminal"]),
                        //Size_of_Manifold_Hoses_by_Terminal = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"]),
                        //No_Manifold_Hoses_by_Vessel = Convert.ToDecimal(dt.Rows[i]["No_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : dt.Rows[i]["No_Manifold_Hoses_by_Vessel"]),
                        //Size_of_Manifold_Hoses_by_Vessel = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"]),

                        //Total_CargoDischarged = Convert.ToDecimal(dt.Rows[i]["Total_CargoDischarged"] == DBNull.Value ? 0 : dt.Rows[i]["Total_CargoDischarged"]),
                        //Balance_Cargo_ToBe_Deischarged = Convert.ToDecimal(dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"] == DBNull.Value ? 0 : dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"]),

                        No_of_Pumps_Use = Convert.ToInt32(dt.Rows[i]["No_of_Pumps_Use"] == DBNull.Value ? 0 : dt.Rows[i]["No_of_Pumps_Use"]),
                        No_Manifold_Hoses_by_Terminal = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : dt.Rows[i]["No_Manifold_Hoses_by_Terminal"]),
                        Size_of_Manifold_Hoses_by_Terminal = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"]),
                        No_Manifold_Hoses_by_Vessel = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : dt.Rows[i]["No_Manifold_Hoses_by_Vessel"]),
                        Size_of_Manifold_Hoses_by_Vessel = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"]),

                        Total_CargoDischarged = Convert.ToDecimal(dt.Rows[i]["Total_CargoDischarged"] == DBNull.Value ? 0 : dt.Rows[i]["Total_CargoDischarged"]),
                        Balance_Cargo_ToBe_Deischarged = Convert.ToDecimal(dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"] == DBNull.Value ? 0 : dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"]),
                        ADT = dt.Rows[i]["ADT"] == DBNull.Value ? "" : dt.Rows[i]["ADT"].ToString(),
                        EDT = dt.Rows[i]["EDT"] == DBNull.Value ? "" : dt.Rows[i]["EDT"].ToString(),

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

            if (firstVal == null)
            {
                Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
                return View(Dis);
            }
            else if (firstVal == "" && dateF == "")
            {
                Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
                return View(Dis);
            }
            else if (firstVal != null)
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
            CommonMethods.CommonDelete(id, "DischargingReport");
            TempData["Success"] = "Record deleted successfully";
            return  RedirectToAction("DischargeRList");
        }

        public ActionResult Edit(int id)
        {
            DischargingReport vd = new DischargingReport();
            vd.DischargingReportList = CommonMethods.editdischargingRList(id, "DischargingReport");
            var loadingRBind = vd.DischargingReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["dischargingReportId"] = id;

            Session["DisCId"] = id;
            int vslid = Convert.ToInt32(Session["VesselID"]);

            loadingRBind.LR_Ballast_PumpUseList = GetPumpsINUse(id, vslid);

            loadingRBind.DC_Cargo_PumpUseList = GetPumpsINUse1(id, vslid);

            loadingRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            //noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            //noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = loadingRBind.LegPortId;
            string portname = loadingRBind.PortName;
            int Voyid = loadingRBind.VoyageId;

            ViewBag.EditStopR = string.Format("GetStopR('{0}');", id);
            ViewBag.GetLegEdit = string.Format("GetLegEdit('{0}');", legportid);
            //ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", Voyid);
            ViewBag.EditDisRCargo = string.Format("GetCargoListEdit('{0}');", legportid);
            return View("Index", loadingRBind);

        }
        public static List<LR_DCR_PumpsUse> GetPumpsINUse1(int id,int vslid)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            //using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId=" + id + " and a.PumpUseId=1", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id and a.VesselId=b.VesselId and a.PumpUseId=b.PumpUseId and b.IsActive=1 where a.LRId=0 and  a.VesselId=" + vslid + " and a.DCRId=" + id + " and a.PumpUseId=1", ConnectionBulder.con))
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

            //using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId="+id+" and a.PumpUseId=2", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id and a.VesselId=b.VesselId and a.PumpUseId=b.PumpUseId and b.IsActive=1 where a.LRId=0 and a.VesselId=" + vslid + " and a.DCRId=" + id + " and a.PumpUseId=2", ConnectionBulder.con))
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
                       // stpr.Add(dt.Rows[i]["Reason"]);
                        stpr.Add(dt.Rows[i]["fromdt"]);
                        stpr.Add(dt.Rows[i]["todate"]);

                    }

                    ViewBag.StopRList = stpr;
                };




            }
            catch { }

            return Json(new { Result = true, stpr = ViewBag.StopRList, cntstopr = ViewBag.CountStopR }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SyncReportDownload(int VoyageId, int id, string ReportDate)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            DischargingReport report = null;
            List<DSCargoList> cargoList = new List<DSCargoList>();
            List<LR_DCR_PumpsUse> ballastPumps = new List<LR_DCR_PumpsUse>();
            List<LR_DCR_PumpsUse> cargoPumps = new List<LR_DCR_PumpsUse>();
            DataTable dtStoppage = new DataTable();

            try
            {
                var vd = new DischargingReport();
                vd.DischargingReportList = CommonMethods.editdischargingRList(id, "DischargingReport");
                report = vd.DischargingReportList?.Where(x => x.Id == id).FirstOrDefault();
                if (report != null)
                {
                    ballastPumps = GetPumpsINUse(id, vslid);
                    cargoPumps = GetPumpsINUse1(id, vslid);

                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,120) DDT, CONVERT(VARCHAR(20),EstCompDateTime,120) EDT, CONVERT(VARCHAR(20),ActualCompDateTime,120) ADT, PortName " +
                        "FROM DS_Cargo a WHERE a.LRId=" + id + " AND a.VesselId=" + vslid + " ORDER BY a.Id", ConnectionBulder.con))
                    {
                        DataTable dtCargo = new DataTable();
                        adp.Fill(dtCargo);
                        for (int i = 0; i < dtCargo.Rows.Count; i++)
                        {
                            var r = dtCargo.Rows[i];
                            string cargoName = (r["CargoName"] == DBNull.Value ? "" : r["CargoName"].ToString());
                            if (r["PortName"] != DBNull.Value && !string.IsNullOrEmpty(r["PortName"].ToString()))
                                cargoName += " (" + r["PortName"].ToString() + ")";
                            cargoList.Add(new DSCargoList
                            {
                                CId = r["CId"] != DBNull.Value ? Convert.ToInt32(r["CId"]) : 0,
                                CargoName = cargoName,
                                DDT = r["DDT"]?.ToString() ?? "",
                                EDT = r["EDT"]?.ToString() ?? "",
                                ADT = r["ADT"]?.ToString() ?? "",
                                Terminal_Acceptable_Discharging_Rate = r["Terminal_Acceptable_Discharging_Rate"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Terminal_Acceptable_Discharging_Rate"]),
                                Discharging_pressure_Requested = r["Discharging_pressure_Requested"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Discharging_pressure_Requested"]),
                                Average_Discharge_Rate_ByVessel = r["Average_Discharge_Rate_ByVessel"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Average_Discharge_Rate_ByVessel"]),
                                Average_Discharge_pressure_ByVessel = r["Average_Discharge_pressure_ByVessel"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Average_Discharge_pressure_ByVessel"]),
                                No_of_Pumps_Use = r["No_of_Pumps_Use"] == DBNull.Value ? 0 : Convert.ToInt32(r["No_of_Pumps_Use"]),
                                No_Manifold_Hoses_by_Terminal = r["No_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : Convert.ToInt32(r["No_Manifold_Hoses_by_Terminal"]),
                                Size_of_Manifold_Hoses_by_Terminal = r["Size_of_Manifold_Hoses_by_Terminal"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Size_of_Manifold_Hoses_by_Terminal"]),
                                No_Manifold_Hoses_by_Vessel = r["No_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : Convert.ToInt32(r["No_Manifold_Hoses_by_Vessel"]),
                                Size_of_Manifold_Hoses_by_Vessel = r["Size_of_Manifold_Hoses_by_Vessel"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Size_of_Manifold_Hoses_by_Vessel"]),
                                Total_CargoDischarged = r["Total_CargoDischarged"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Total_CargoDischarged"]),
                                Balance_Cargo_ToBe_Deischarged = r["Balance_Cargo_ToBe_Deischarged"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Balance_Cargo_ToBe_Deischarged"])
                            });
                        }
                    }

                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "SELECT Stoppage, CONVERT(VARCHAR(20),DateTimeFrom,120) DateTimeFrom, CONVERT(VARCHAR(20),DateTimeTo,120) DateTimeTo " +
                        "FROM LR_Stoppage WHERE VesselId=" + vslid + " AND LRId=0 AND dcid=" + id, ConnectionBulder.con))
                    {
                        adp.Fill(dtStoppage);
                    }
                }
            }
            catch { }

            using (XLWorkbook wb = new XLWorkbook())
            {
                AddDischargingCargo1Sheet(wb, report, cargoList);
                AddDischargingCargo2Sheet(wb, report, ballastPumps, cargoPumps, dtStoppage);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = "DischargingReport_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
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

        /// <summary>Cargo-1 sheet: Header + Cargo table matching edit page layout.</summary>
        private void AddDischargingCargo1Sheet(XLWorkbook wb, DischargingReport r, List<DSCargoList> cargoList)
        {
            var ws = wb.Worksheets.Add("Cargo-1");
            ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            int row = 1;

            ws.Cell(row, 1).Value = "Discharging Report - Cargo-1";
            var rngHdr = ws.Range(row, 1, row, 16);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.VoyageNumber ?? r.VoyageId.ToString();
                string legText = "";
                if (string.IsNullOrEmpty(legText) && r.LegPortId > 0)
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

                AddKeyValueSingle(ws, ref row, "Voy No.", voyNo);
                AddKeyValueSingle(ws, ref row, "ETD Date & Time", r.ETDDateTime != null ? Convert.ToDateTime(r.ETDDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueSingle(ws, ref row, "Leg", legText);
                AddKeyValueSingle(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueSingle(ws, ref row, "Report Date & Time", r.ReportDateTime != null ? Convert.ToDateTime(r.ReportDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueSingle(ws, ref row, "Draft Fwd", r.DraftFwd);
                AddKeyValueSingle(ws, ref row, "Draft Mid", r.DraftMid);
                AddKeyValueSingle(ws, ref row, "Draft Aft", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Cargo";
            var rngCargo = ws.Range(row, 1, row, 2);
            rngCargo.Merge();
            rngCargo.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngCargo);
            row++;

            int cargoIndex = 1;
            foreach (var c in cargoList ?? new List<DSCargoList>())
            {
                ws.Cell(row, 1).Value = "Cargo " + cargoIndex;
                ws.Range(row, 1, row, 2).Merge().Style.Font.Bold = true;
                row++;

                AddKeyValueSingle(ws, ref row, "Cargo Grades", c.CargoName);
                AddKeyValueSingle(ws, ref row, "Commence Discharge Date & Time", c.DDT);
                AddKeyValueSingle(ws, ref row, "Terminal Acceptable Discharging Rate", c.Terminal_Acceptable_Discharging_Rate);
                AddKeyValueSingle(ws, ref row, "Discharging Pressure Requested", c.Discharging_pressure_Requested);
                AddKeyValueSingle(ws, ref row, "Average Discharge Rate By Vessel", c.Average_Discharge_Rate_ByVessel);
                AddKeyValueSingle(ws, ref row, "Average Discharge pressure By Vessel", c.Average_Discharge_pressure_ByVessel);
                AddKeyValueSingle(ws, ref row, "No of Pumps in use", c.No_of_Pumps_Use);
                AddKeyValueSingle(ws, ref row, "No of Manifold / Hoses by Terminal", c.No_Manifold_Hoses_by_Terminal);
                AddKeyValueSingle(ws, ref row, "Size of Manifold / Hoses by Terminal (Inches)", c.Size_of_Manifold_Hoses_by_Terminal);
                AddKeyValueSingle(ws, ref row, "No of Manifold / Hoses by Vessel", c.No_Manifold_Hoses_by_Vessel);
                AddKeyValueSingle(ws, ref row, "Size of Manifold / Hoses by Vessel (Inches)", c.Size_of_Manifold_Hoses_by_Vessel);
                AddKeyValueSingle(ws, ref row, "Total Cargo Discharged", c.Total_CargoDischarged);
                AddKeyValueSingle(ws, ref row, "Balance Cargo to be Discharged", c.Balance_Cargo_ToBe_Deischarged);
                AddKeyValueSingle(ws, ref row, "ETC Comp Date & Time", c.EDT);
                AddKeyValueSingle(ws, ref row, "Actual Comp Date & Time", c.ADT);

                row++;
                cargoIndex++;
            }
            ws.Columns().AdjustToContents();
        }

        /// <summary>Cargo-2 sheet: Ballast Pumps, Cargo Pumps, Power Packs, Letter of Protests, Remarks, Stoppage table.</summary>
        private void AddDischargingCargo2Sheet(XLWorkbook wb, DischargingReport r, List<LR_DCR_PumpsUse> ballastPumps, List<LR_DCR_PumpsUse> cargoPumps, DataTable dtStoppage)
        {
            var ws = wb.Worksheets.Add("Cargo-2");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;

            ws.Cell(row, 1).Value = "Discharging Report - Cargo-2";
            var rngHdr = ws.Range(row, 1, row, 5);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Ballast Pumps in Use";
            var rngBP = ws.Range(row, 1, row, 3);
            rngBP.Merge();
            rngBP.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngBP);
            row++;
            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = "Rate";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            foreach (var p in ballastPumps ?? new List<LR_DCR_PumpsUse>())
            {
                ws.Cell(row, 1).Value = p.Name ?? "";
                ws.Cell(row, 2).Value = p.Rate;
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Cargo Pumps in Use";
            var rngCP = ws.Range(row, 1, row, 3);
            rngCP.Merge();
            rngCP.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngCP);
            row++;
            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = "Rate";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            foreach (var p in cargoPumps ?? new List<LR_DCR_PumpsUse>())
            {
                ws.Cell(row, 1).Value = p.Name ?? "";
                ws.Cell(row, 2).Value = p.Rate;
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Power Packs";
            var rngPP = ws.Range(row, 1, row, 3);
            rngPP.Merge();
            rngPP.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngPP);
            row++;
            AddKeyValueSingle(ws, ref row, "No of Power Packs onboard", r?.Power_Packs_onboard);
            AddKeyValueSingle(ws, ref row, "No of Power Packs used", r?.Power_Packs_Used);
            row++;

            ws.Cell(row, 1).Value = "Letter of Protests";
            var rngLOP = ws.Range(row, 1, row, 3);
            rngLOP.Merge();
            rngLOP.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngLOP);
            row++;
            AddKeyValueSingle(ws, ref row, "Time", r?.Times);
            AddKeyValueSingle(ws, ref row, "Rate", r?.Rate);
            AddKeyValueSingle(ws, ref row, "Hose Connection", r?.Hose_Connection);
            AddKeyValueSingle(ws, ref row, "High H2S", r?.High_H2S);
            row++;

            ws.Cell(row, 1).Value = "Discharge Report Remarks";
            var rngRem = ws.Range(row, 1, row, 3);
            rngRem.Merge();
            rngRem.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngRem);
            row++;
            AddKeyValueSingle(ws, ref row, "Remarks", r?.Remarks);
            row++;

            ws.Cell(row, 1).Value = "Stoppage";
            var rngStop = ws.Range(row, 1, row, 3);
            rngStop.Merge();
            rngStop.Style.Font.Bold = true;
            ApplyGridTitleStyle(rngStop);
            row++;
            ws.Cell(row, 1).Value = "Stoppage Reason";
            ws.Cell(row, 2).Value = "Date Time From";
            ws.Cell(row, 3).Value = "Date Time To";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtStoppage != null)
            {
                foreach (DataRow dr in dtStoppage.Rows)
                {
                    ws.Cell(row, 1).Value = dr["Stoppage"]?.ToString() ?? "";
                    ws.Cell(row, 2).Value = dr["DateTimeFrom"]?.ToString() ?? "";
                    ws.Cell(row, 3).Value = dr["DateTimeTo"]?.ToString() ?? "";
                    row++;
                }
            }
            ws.Columns().AdjustToContents();
        }
    }
}