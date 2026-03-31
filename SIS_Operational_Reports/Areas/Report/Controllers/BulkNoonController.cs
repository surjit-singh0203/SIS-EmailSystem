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
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class BulkNoonController : BaseController
    {
        // GET: Report/Arrival
        public ActionResult Index(int? pageNo, string firstVal, string dateF, string dateT, string Vessel)
        {
            DailyNoonReport dnR = new DailyNoonReport();
            dnR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            //if (vslid.ToString() == "")
            //{
            //    vslid = Convert.ToString(Session["VesselID"]);
            //}

        
          //  TempData["noonReportId"] = null;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                dnR.GetNoonRList = CommonMethods.GetBulkNoonReportList(vslid, currPage, pageSize);
                return View(dnR);
            }
            else if (firstVal == "" && dateF == "" && Vessel == "")
            {
                dnR.GetNoonRList = CommonMethods.GetBulkNoonReportList(vslid, currPage, pageSize);
                return View(dnR);
            }
            else if (firstVal != null || dateF != "" || Vessel != "")
            {
                dnR.GetNoonRList = CommonMethods.SearchBulkNoonReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchBulk", dnR);
            }
           

            return View(dnR);

        }

        public ActionResult Edit(int id, int vesselid,string actionName)
        {
            DailyNoonReport vd = new DailyNoonReport();
            vd.GetNoonRList = CommonMethods.editnoonRList(id, vesselid, "DailyNoonReport");
            var noonRBind = vd.GetNoonRList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            //ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", noonRBind.VoyageId);
            //TempData["noonReportId"] = id;

            int vslid = vesselid;
            Session["EditVesselIDBulk"] = vesselid;
           
            ViewBag.ActionN=actionName;

            //noonRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
            //noonRBind.CargoTanks = GetCargoTankList(id, vslid);
            //noonRBind.BallastTanks = GetBallastTankList(id, vslid);
            //noonRBind.Void_SpaceTanks = GetVoid_SpaceList(id, vslid);

            int legportid = noonRBind.LegPortId;
            ViewBag.JavaScriptFunction = string.Format("Getfuelcons11('{0}');", id);
            ViewBag.JavaScriptFunction1 = string.Format("GetLegEdit('{0}');", legportid);
            ViewBag.JavaScriptFunction2 = string.Format("GetNRCargoEdit('{0}');", id);
            ViewBag.JavaScriptFunction3 = string.Format("GetNonRoutineEvents('{0}');", id);

            return View("editBulk", noonRBind);

        }


        //public ActionResult DownloadExcel(int id, int vesselid, string actionName)
        public ActionResult DownloadExcel()
        {
            DailyNoonReport vd = new DailyNoonReport();
            int i = 0; int check = 0;
            XLWorkbook wb = new XLWorkbook();

            DataTable newTable = new DataTable();
            // DataTable table ;
            // DataSet ds =new DataSet ();

            using (SqlDataAdapter adp2 = new SqlDataAdapter("select * from bulknoonReportTemp", ConnectionBulder.con))
            {
                DataTable ddt2 = new DataTable();
                adp2.Fill(ddt2);

                for (int j = 0; j < ddt2.Rows.Count; j++)
                {
                    int nrid = Convert.ToInt32(ddt2.Rows[j]["NoonReportId"]);
                    int vslid = Convert.ToInt32(ddt2.Rows[j]["VesselId"]);


                    //DataSet ds = CommonMethods.ExportBulkNoonRList(id, vesselid);
                    DataSet ds = CommonMethods.ExportBulkNoonRListNew(nrid.ToString(), vslid.ToString());
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
                                using (SqlDataAdapter adp = new SqlDataAdapter("select * from tbl_FuelROB where ReportType_Id=1 and TableMax_Id =" + nrid + " and VesselId =" + vslid + "", ConnectionBulder.con))
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
                                using (SqlDataAdapter adp = new SqlDataAdapter("select * from Fuel_Cons_NR  where ReportType_Id=1 and VesselId =" + vslid + " and Noon_Report_Id =" + nrid + "", ConnectionBulder.con))
                                {
                                    DataTable ddt = new DataTable();
                                    adp.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
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



                            if (i == 0)
                                table.TableName = "BulkNoonReport";

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
                            //newTable = ds.Tables[0].Clone();

                            //newTable.Tables[i].ImportRow(ds.Tables[0].Rows[i]);

                            //for (int k = 0; k < table.Rows.Count; k++)
                            //{
                            //    DataRow drNew = newTable.NewRow();
                            //    drNew.ItemArray = table.Rows[k].ItemArray;
                            //    newTable.Rows.Add(drNew);
                            //}

                            //if (table.Rows.Count > 0)
                            //{
                            //    var protectedsheet = wb.Worksheets.Add(table);

                            //    var projection = protectedsheet.Protect("49WEB$TREET#");
                            //    projection.InsertColumns = true;
                            //    projection.InsertRows = true;
                            //}
                            break;
                            i++;
                        }
                    }
                }


                if (newTable.Rows.Count > 0)
                {
                    var protectedsheet = wb.Worksheets.Add(newTable);

                    //var projection = protectedsheet.Protect("49WEB$TREET#");
                    //projection.InsertColumns = true;
                    //projection.InsertRows = true;
                }

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                DateTime today = DateTime.Today;
                Response.Clear();
                Response.BufferOutput = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=BulkNoonReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");

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


        public JsonResult Getfuelcons(int NoonReportId)
        {
            int vslid = Convert.ToInt32(Session["EditVesselIDBulk"]);
            ArrayList arrName = new ArrayList();
            ArrayList CpValue = new ArrayList();
            ArrayList FCValue = new ArrayList();
            ArrayList FRobValue = new ArrayList();

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
                    //DataTable dt = new DataTable();
                    //objCMD.Fill(dt);
                    //for (int i = 0; i < dt.Rows.Count; i++)
                    //{
                    //    FCValue.Add(dt.Rows[i]["value"]);
                    //}
                    //ViewBag.FcValue = FCValue;

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


            }
            catch { }

            return Json(new { Result = true, ft = ViewBag.Ftype, cp = ViewBag.CPValue, fc = ViewBag.FcValue, frob = ViewBag.FrobValue }, JsonRequestBehavior.AllowGet);
        }


    }
}