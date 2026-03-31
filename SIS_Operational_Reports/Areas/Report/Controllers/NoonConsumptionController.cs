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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    public class NoonConsumptionController : Controller
    {
        // GET: Report/Consumption

        [HttpGet]
        public ActionResult Index(int? pageNo, string firstVal, string dateF,string datefilter, string dateT, string VesselId, string Vessel,string CPID, string search)
        {
          
            DailyNoonReport dnR = new DailyNoonReport();
            dnR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //int vslid = Convert.ToInt32(Session["VesselID"]);
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);

            //string vslname = dnR.VesselList.Where(x => x.Value == Vessel).Select(x => x.Text).SingleOrDefault();
            //ViewBag.VesselName = vslname;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            string dtfil = datefilter;

            if (dtfil != "" && dtfil !=null)
            {
                string[] dt = dtfil.Split('-');
                Session["dateF"] = dt[0];
                Session["dateT"] = dt[1];
                
            }

            TempData["CurrentPage"] = currPage;

            if (search == null)
            {


                //dnR.GetNoonRList = CommonMethods.GetNoonConsumptionReportList(vslid, firstVal, dateF, dateT);
                dnR.GetNoonRList = null;
                return View(dnR);

                //return PartialView("_noonConsumptionR");
            }
            if (search != null)
            {
                Session["Vsl"] = VesselId;
                Session["NoonCPID"] = CPID;
                ViewBag.vname = "jhj";
                //dnR.GetNoonRList = CommonMethods.GetNoonConsumptionReportList(vslid, firstVal, dateF, dateT);
                dnR.GetNoonRList = null;
                return View(dnR);

               
            }
            if (firstVal != null || dateF != "" || Vessel != "")
            {
                Session["DateFrom"] = dateF;
                Session["DateTo"] = dateT;
                Session["Vsl"] = Vessel;

               // dnR.GetNoonRList = CommonMethods.GetNoonConsumptionReportList(vslid,  firstVal, dateF, dateT);
               dnR.GetNoonRList = null;
                dnR.VesselId = Convert.ToInt32(Vessel);

                ViewBag.VesselName = Vessel;
                ViewBag.vname = "jhj";
                //dnR.VesselId = Vessel;
                return View(dnR); 

                //return PartialView("_noonConsumptionR", dnR);
               //return PartialView("_searchNoonCons", dnR);
            }


            return View(dnR);
        }

        public JsonResult BindCP(int VesselId)
        {
            List<SelectListItem> jst = new List<SelectListItem>();            
            try
            {                
                using (SqlDataAdapter sda = new SqlDataAdapter("spVesselWiseCP_NoonC", ConnectionBulder.con))
                {
                    sda.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["CPName"].ToString(), Value = tbl.Rows[i]["CPID"].ToString() });

                        }

                    }
                }
            }
            catch { }
            // return jst;

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BindCPDate(int Cpid)
        {
            string strtDt="" ; string endDt="" ;
            List<SelectListItem> jst = new List<SelectListItem>();
            try
            {
               
                using (SqlDataAdapter sda = new SqlDataAdapter("select * from CPContract where id=" + Cpid + "", ConnectionBulder.con))
                {
                    //sda.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    //sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        strtDt =  tbl.Rows[0]["StartDate"].ToString();
                        endDt = tbl.Rows[0]["EndDate"].ToString();

                        DateTime ss = DateTime.Parse(strtDt);
                        DateTime ss1 = DateTime.Parse(endDt);
                        // DateTime dt = DateTime.ParseExact(strtDt, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        var date_string = ss.ToString("yyyy-MM-dd");
                        var date_string1 = ss1.ToString("yyyy-MM-dd");
                        strtDt = date_string;
                        endDt = date_string1;
                    }
                }
            }
            catch (Exception ex) { }
            // return jst;

            return Json(new { Result = true, StartDate = strtDt,EndDate=endDt }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public void DownloadExcel()
        {
            try
            {
                DailyNoonReport vd = new DailyNoonReport();
                int i = 0; int check = 0;
                XLWorkbook wb = new XLWorkbook();

                string dfrm = Session["DateFrom"].ToString();
                string dto = Session["DateTo"].ToString();
                int vsl = Convert.ToInt32(Session["Vsl"]);


                DataTable newTable = new DataTable();

                DataSet ds = CommonMethods.ExportNoonConsumtionReport(dfrm, dto, vsl);
                using (wb = new XLWorkbook())
                {
                    foreach (DataTable table in ds.Tables)
                    {
                        if (i == 0)
                            table.TableName = "NoonConsumtionReport";

                        if (check == 0)
                        {
                            //Decimal N2N = Convert.ToDecimal(ds.Tables[0].Compute("SUM(noontonoondmg_dist)", string.Empty));
                            //Decimal ConsNM_VLSFO = Convert.ToDecimal(ds.Tables[0].Compute("SUM(cons_nm_VLSFO)", string.Empty));
                            //Decimal ConsNM_MDO = Convert.ToDecimal(ds.Tables[0].Compute("SUM(cons_nm_MDO)", string.Empty));
                            //Decimal ActSpd = Convert.ToDecimal(ds.Tables[0].Compute("SUM(act_Speed)", string.Empty));

                            try
                            {
                                Decimal N2N = Convert.ToDecimal(ds.Tables[0].Compute("AVG(noontonoondmg_dist)", string.Empty));
                                Decimal ConsNM_VLSFO = Convert.ToDecimal(ds.Tables[0].Compute("AVG(cons_nm_VLSFO1)", string.Empty));
                                Decimal ConsNM_MDO = Convert.ToDecimal(ds.Tables[0].Compute("AVG(cons_nm_MDO1)", string.Empty));
                                Decimal ActSpd = Convert.ToDecimal(ds.Tables[0].Compute("AVG(act_Speed)", string.Empty));

                                var cons_vlsfo = decimal.Round(ConsNM_VLSFO, 2, MidpointRounding.AwayFromZero);
                                var cons_mdo = decimal.Round(ConsNM_MDO, 2, MidpointRounding.AwayFromZero);


                                ds.Tables[0].Columns["VesselStatus"].ColumnName = "Laden/Ballast";
                                ds.Tables[0].Columns["AtSeaOrPort"].ColumnName = "Sea/Port";
                                ds.Tables[0].Columns["noontonoondmg_dist"].ColumnName = "Dist(N2N)";
                                ds.Tables[0].Columns["act_Speed"].ColumnName = "Act Spd";
                                ds.Tables[0].Columns["windforce"].ColumnName = "BF Scale";

                                ds.Tables[0].Columns["swelldirection"].ColumnName = "Swell Dir";
                                ds.Tables[0].Columns["swellheight"].ColumnName = "Swell Hght";

                                ds.Tables[0].Columns["wavelength"].ColumnName = "Wave Lngth";
                                ds.Tables[0].Columns["waveheight"].ColumnName = "Wave Hght";

                                ds.Tables[0].Columns["cons_nm_VLSFO"].ColumnName = "cons/nm_VLSFO";
                                ds.Tables[0].Columns["cons_nm_MDO"].ColumnName = "cons/nm_MDO";

                                ds.Tables[0].Columns.Remove("id");
                                ds.Tables[0].Columns.Remove("vesselname");
                                ds.Tables[0].Columns.Remove("voyagenumber");
                                ds.Tables[0].Columns.Remove("cons_nm_VLSFO1");
                                ds.Tables[0].Columns.Remove("cons_nm_MDO1");

                                var cnt = ds.Tables[0].Rows.Count;

                                cnt = cnt + 1;

                                table.Rows.Add();
                                ds.Tables[0].Rows.Add("Total");
                                //Decimal TotalPrice = Convert.ToDecimal(ds.Tables[0].Compute("SUM(Cons_AE_VLSFO)", string.Empty));

                                ds.AcceptChanges();
                                ds.Tables[0].Rows[cnt].SetField("Dist(N2N)", N2N);
                                ds.Tables[0].Rows[cnt].SetField("cons/nm_VLSFO", cons_vlsfo);
                                ds.Tables[0].Rows[cnt].SetField("cons/nm_MDO", cons_mdo);
                                ds.Tables[0].Rows[cnt].SetField("Act Spd", ActSpd);

                                newTable = ds.Tables[0];

                            }
                            catch { }

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
                }


                //var ws = wb.Worksheets.Add("Style Rows and Columns");
                if (newTable.Rows.Count > 0)
                {
                    //ws.Rows(2, 3).Style.Fill.BackgroundColor = XLColor.LightYellow;

                    var cnt = newTable.Rows.Count;
                    cnt = cnt + 1;

                    var protectedsheet = wb.Worksheets.Add(newTable);
                    protectedsheet.Rows(2, newTable.Rows.Count - 1).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    protectedsheet.Cell(cnt, 1).Style.Font.Bold = true;
                    protectedsheet.Cell(cnt, 1).Style.Font.FontSize = 13;

                    protectedsheet.Cell(cnt, 1).Style.Fill.BackgroundColor = XLColor.Yellow;

                    protectedsheet.Cell(cnt, 12).Style.Font.Bold = true;
                    protectedsheet.Cell(cnt, 13).Style.Font.Bold = true;
                    protectedsheet.Cell(cnt, 14).Style.Font.Bold = true;
                    protectedsheet.Cell(cnt, 15).Style.Font.Bold = true;

                    protectedsheet.Cell(cnt, 12).Style.Font.FontSize = 13;
                    protectedsheet.Cell(cnt, 13).Style.Font.FontSize = 13;
                    protectedsheet.Cell(cnt, 14).Style.Font.FontSize = 13;
                    protectedsheet.Cell(cnt, 15).Style.Font.FontSize = 13;

                    protectedsheet.Cell(cnt, 12).Style.Fill.BackgroundColor = XLColor.Yellow;
                    protectedsheet.Cell(cnt, 13).Style.Fill.BackgroundColor = XLColor.Yellow;
                    protectedsheet.Cell(cnt, 14).Style.Fill.BackgroundColor = XLColor.Yellow;
                    protectedsheet.Cell(cnt, 15).Style.Fill.BackgroundColor = XLColor.Yellow;

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
                Response.AddHeader("content-disposition", "attachment;filename=NoonConsumtionReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");


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
            catch {
          
            }
            

        }


    }
}

