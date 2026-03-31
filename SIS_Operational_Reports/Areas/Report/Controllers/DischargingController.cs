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

            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
            
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
                int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);


                //using (SqlDataAdapter sda = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where VoyageId=" + Voyid + "", ConnectionBulder.con))
                using (SqlDataAdapter sda = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + Voyid + "' and VesselId='"+vslid+"'", ConnectionBulder.con))
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

                int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_a  as port from VoyageLeg where id=" + Legid + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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
            }
            catch { }
            // return jst;

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertCargoList(string CargoListing)
        {
            int k = 0; int DsID = 0;
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
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
                rootObject.Id = rootObject.Id;
                rootObject.DSId = DsID;
                rootObject.VesselId = vslid;
                rootObject.VoyageId = loadRep.VoyageId;
                rootObject.LegPortId = loadRep.LegPortId;
                rootObject.PortName = loadRep.PortName;
                var Cname = rootObject.CargoName;
                Cname = Regex.Replace(Cname, @"\s+\(.*\)", "");
                rootObject.CargoName = Cname;

                //if (k == 0)
                //{
                //    if (Session["DIS_ID"].ToString() != "")
                //    {
                //        using (SqlDataAdapter adp = new SqlDataAdapter("delete from DS_Cargo where LRId=" + DsID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                //        {
                //            DataTable dt = new DataTable();
                //            adp.Fill(dt);
                //            k = 1;
                //        }


                //    }
                //}

                //using (SqlDataAdapter adp = new SqlDataAdapter("select id  from DS_Cargo where LRId ="+ DsID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                //{
                //    DataTable dt = new DataTable();
                //    adp.Fill(dt);

                //}

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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
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
                if (rootObject.DCId == 0)
                {
                    rootObject.DCId = DCID;
                }
                
                rootObject.LoadingDischarged = true;
               // CommonMethods.InsertUpdateLoadingStoppage(rootObject, "Insert");


                //if (k == 0)
                //{
                //    if (Session["DIS_ID"].ToString() != "")
                //    {
                //        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_Stoppage where DCId=" + DCID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                //        {
                //            DataTable dt = new DataTable();
                //            adp.Fill(dt);
                //            k = 1;
                //        }


                //    }
                //}

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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
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



                //if (k == 0)
                //{
                //    if (Session["DIS_ID"].ToString() != "")
                //    {
                //        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                //        {
                //            DataTable dt = new DataTable();
                //            adp.Fill(dt);
                //            k = 1;
                //        }


                //    }
                //}

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
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
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


                //if (k == 0)
                //{
                //    if (Session["DIS_ID"].ToString() != "")
                //    {
                //        using (SqlDataAdapter adp = new SqlDataAdapter("delete from LR_DCR_PumpsUse where DCRId=" + DSID + " and VesselId=" + vslid + "", ConnectionBulder.con))
                //        {
                //            DataTable dt = new DataTable();
                //            adp.Fill(dt);
                //            k = 1;
                //        }


                //    }
                //}

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

        public JsonResult BindPort_Edit(int Voyid)
        {
            string prtname = "";
            List<SelectListItem> jst = new List<SelectListItem>();
            List<SelectListItem> distinctTextList = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);

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

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from DischargingReport where id=" + id + " and VesselId=" + vslid + "", ConnectionBulder.con))
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

                int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);

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

                using (SqlDataAdapter sda = new SqlDataAdapter("select id, legport_b  as port from VoyageLeg where id=" + Legid + " and VesselId='"+vslid+"'", ConnectionBulder.con))
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

                using (SqlDataAdapter sda = new SqlDataAdapter("select PortName  from DischargingReport where id=" + id + " and VesselId='" + vslid + "'", ConnectionBulder.con))
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
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
            List<DSCargoList> ftype = new List<DSCargoList>();

            //using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT,CONVERT(VARCHAR(20),ActualCompDateTime,127)  ADT from DS_Cargo a where a.VoyageId=" + VoyId + " and a.LegPortId=" + VLeg + " and a.LRId="+dcid+" and a.vesselid="+vslid+" ", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter(" SELECT a.*, CONVERT(VARCHAR(20),DischargeDatetime,127)  DDT, CONVERT(VARCHAR(20),ActualCompDateTime,127) ADT, CONVERT(VARCHAR(20),EstCompDateTime,127) EDT, PortName from DS_Cargo a where a.VoyageId=" + VoyId + " and  a.LRId=" + dcid + " and a.vesselid=" + vslid + " ", ConnectionBulder.con))
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
                        CargoName = dt.Rows[i]["CargoName"].ToString() + " ( " +
                       (dt.Rows[i]["PortName"] == DBNull.Value ? "" : dt.Rows[i]["PortName"].ToString()) + " ) ",
                        DDT = dt.Rows[i]["DDT"].ToString(),
                        Terminal_Acceptable_Discharging_Rate = Convert.ToDecimal(dt.Rows[i]["Terminal_Acceptable_Discharging_Rate"]),
                        Discharging_pressure_Requested = Convert.ToDecimal(dt.Rows[i]["Discharging_pressure_Requested"]),
                        Average_Discharge_Rate_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_Rate_ByVessel"]),
                        Average_Discharge_pressure_ByVessel = Convert.ToDecimal(dt.Rows[i]["Average_Discharge_pressure_ByVessel"]),
                        No_of_Pumps_Use = Convert.ToInt32(dt.Rows[i]["No_of_Pumps_Use"]),
                        No_Manifold_Hoses_by_Terminal = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Terminal"]),
                        Size_of_Manifold_Hoses_by_Terminal = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Terminal"]),
                        No_Manifold_Hoses_by_Vessel = Convert.ToInt32(dt.Rows[i]["No_Manifold_Hoses_by_Vessel"]),
                        Size_of_Manifold_Hoses_by_Vessel = Convert.ToDecimal(dt.Rows[i]["Size_of_Manifold_Hoses_by_Vessel"]),

                        Total_CargoDischarged = Convert.ToDecimal(dt.Rows[i]["Total_CargoDischarged"]),
                        Balance_Cargo_ToBe_Deischarged = Convert.ToDecimal(dt.Rows[i]["Balance_Cargo_ToBe_Deischarged"]),
                        EDT = dt.Rows[i]["EDT"].ToString(),
                        ADT = dt.Rows[i]["ADT"].ToString(),
                        
                    });
                }
                // con.Close();
            }

            return Json(new { Result = true, ftype, }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DischargeRList(int? pageNo, string firstVal, string dateF, string dateT, string Vessel)
        {
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }
            //ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            DischargingReport Dis = new DischargingReport();
            Dis.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
                Dis.VesselId = Convert.ToInt32(Vessel);
                return View(Dis);
            }
            else if (firstVal == "" && dateF == "" && Vessel == "")
            {
                Dis.DischargingReportList = CommonMethods.GetCommonDischargingReportsList(vslid, currPage, pageSize);
                Dis.VesselId = Convert.ToInt32(Vessel);
                return View(Dis);
            }
            else if (firstVal != null || dateF != "" || Vessel != "")
            {
                Dis.DischargingReportList = CommonMethods.SearchDischargingReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                Dis.VesselId = Convert.ToInt32(Vessel);
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

        public ActionResult Edit(int id,int vesselid)
        {
            DischargingReport vd = new DischargingReport();
            vd.DischargingReportList = CommonMethods.editdischargingRList(id, vesselid, "DischargingReport");
            var loadingRBind = vd.DischargingReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["dischargingReportId"] = id;

            loadingRBind.Id = id;
            Session["DisCId"] = id;
            int vslid = vesselid;
            //Session["VesselID"] = vesselid;
            Session["EditVesselIDDischarging"] = vesselid;

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
            ViewBag.Id = id;
            return View("Index", loadingRBind);

        }
        public static List<LR_DCR_PumpsUse> GetPumpsINUse1(int id,int vslid)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            // using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId=" + id + " and a.PumpUseId=1", ConnectionBulder.con))
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
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
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

            //using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and  b.VesselId=" + vslid + " and DCRId=" + id+" and a.PumpUseId=2", ConnectionBulder.con))
            //using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id  where IsActive=1 and LRId=0 and  a.VesselId=" + vslid + " and DCRId=" + id + " and a.PumpUseId=2", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select  a.*, a.Id as PumpId,b.Name,b.Capacity,b.PumpUseId  from lr_dcr_pumpsuse a inner join tblPump b on a.PumpId=b.Id and a.VesselId=b.VesselId and a.PumpUseId=b.PumpUseId and b.IsActive=1 where a.LRId=0 and  a.VesselId=" + vslid + " and a.DCRId=" + id + " and a.PumpUseId=2", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new LR_DCR_PumpsUse
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
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
            int vslid = Convert.ToInt32(Session["EditVesselIDDischarging"]);
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
                        //  stpr.Add(dt.Rows[i]["Stoppage"]);
                        //   stpr.Add(dt.Rows[i]["Reason"]);
                        stpr.Add(dt.Rows[i]["Id"]);
                        stpr.Add(dt.Rows[i]["Stoppage"]);
                        stpr.Add(dt.Rows[i]["fromdt"]);
                        stpr.Add(dt.Rows[i]["todate"]);

                    }

                    ViewBag.StopRList = stpr;
                };




            }
            catch { }

            return Json(new { Result = true, stpr = ViewBag.StopRList, cntstopr = ViewBag.CountStopR }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadExcel(int id, int vesselid)
        {
            DailyNoonReport vd = new DailyNoonReport();
            int i = 0; int check = 0;
            XLWorkbook wb = new XLWorkbook();

            DataTable newTable = new DataTable();

            DataSet ds = CommonMethods.ExportReportList(id, vesselid, "DischargingReport");
            using (wb = new XLWorkbook())
            {
                foreach (DataTable table in ds.Tables)
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Rate,b.Name from LR_DCR_PumpsUse a left join tblPump b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId =" + vesselid + "", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        int chk = 0;
                        for (int m = 0; m < dt.Rows.Count; m++)
                        {
                            chk++;
                            table.Columns.Add(new DataColumn("Ballast_Pump_Use_Name" + chk + "", typeof(string)));
                            table.Columns.Add(new DataColumn("Ballast_Pump_Use_Rate" + chk + "", typeof(decimal)));
                        }
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.Rate,b.Name from LR_DCR_PumpsUse a left join tblPump b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId =" + vesselid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            int chk = 0;

                            for (int m = 0; m < dt.Rows.Count; m++)
                            {
                                chk++;
                                row["Ballast_Pump_Use_Name" + chk + ""] = dt.Rows[m]["Name"];
                                row["Ballast_Pump_Use_Rate" + chk + ""] = dt.Rows[m]["Rate"];
                            }
                        }
                    }

                    /////   for stoppage reason
                    ///
                    using (SqlDataAdapter adp = new SqlDataAdapter("select Stoppage,DateTimeFrom,DateTimeTo from LR_Stoppage where DCId=" + id + " and VesselId =" + vesselid + "", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        int chk = 0;
                        for (int m = 0; m < dt.Rows.Count; m++)
                        {
                            chk++;
                            table.Columns.Add(new DataColumn("StoppageReason" + chk + "", typeof(string)));
                            table.Columns.Add(new DataColumn("DateTimeFrom" + chk + "", typeof(DateTime)));
                            table.Columns.Add(new DataColumn("DateTimeTo" + chk + "", typeof(DateTime)));
                        }
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Stoppage,DateTimeFrom,DateTimeTo from LR_Stoppage where DCId=" + id + " and VesselId =" + vesselid + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            int chk = 0;

                            for (int m = 0; m < dt.Rows.Count; m++)
                            {
                                chk++;
                                row["StoppageReason" + chk + ""] = dt.Rows[m]["Stoppage"];
                                row["DateTimeFrom" + chk + ""] = dt.Rows[m]["DateTimeFrom"];
                                row["DateTimeTo" + chk + ""] = dt.Rows[m]["DateTimeTo"];
                            }
                        }
                    }

                    ////////////////


                    if (i == 0)
                        table.TableName = "DischargingReport";
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
                Response.AddHeader("content-disposition", "attachment;filename=DischargingReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");

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

        public ActionResult EditFromDashboard(string reportdate, int vesselid, string Exactdate = "")
        {
            DischargingReport vd = new DischargingReport();
            if (Exactdate == "")
            {
                vd.DischargingReportList = CommonMethods.editdischargingRListDashbord(reportdate, vesselid, "DischargingReport");
            }
            if (Exactdate != "")
            {
                vd.DischargingReportList = CommonMethods.editdischargingRListDashbord(reportdate, vesselid, "DischargingReportR");
            }
            
            var ID = vd.DischargingReportList.Select(x => x.Id).FirstOrDefault();
            var loadingRBind = vd.DischargingReportList.Where(x => x.Id == ID).FirstOrDefault(e => e.Id == ID);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            TempData["dischargingReportId"] = ID;

            Session["DisCId"] = ID;
            int vslid = vesselid;
            //Session["VesselID"] = vesselid;
            Session["EditVesselIDDischarging"] = vesselid;

            loadingRBind.LR_Ballast_PumpUseList = GetPumpsINUse(ID, vslid);

            loadingRBind.DC_Cargo_PumpUseList = GetPumpsINUse1(ID, vslid);

            loadingRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            //noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            //noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = loadingRBind.LegPortId;
            string portname = loadingRBind.PortName;

            ViewBag.EditStopR = string.Format("GetStopR('{0}');", ID);
            ViewBag.GetLegEdit = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.GetPortEdit = string.Format("BindPrtEdit('{0}');", legportid);
            ViewBag.EditDisRCargo = string.Format("GetCargoListEdit('{0}');", legportid);
            return View("Index", loadingRBind);

        }

    }
}