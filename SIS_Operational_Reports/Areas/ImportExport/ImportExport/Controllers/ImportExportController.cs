using ClosedXML.Excel;
using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace SIS_Operational_Reports.Areas.ImportExport.Controllers
{
    public class ImportExportController : Controller
    {
        // GET: ImportExport/ImportExport
        public ActionResult Index()
        {
            ExportPeram epm = new ExportPeram();
            return View(epm);
        }

        //public static List<VoyageClass> voyageNList()
        //{
        //    List<VoyageClass> ftype = new List<VoyageClass>();
        //    using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
        //    {
        //        adp.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            ftype.Add(new VoyageClass
        //            {
        //                Id = Convert.ToInt32(dt.Rows[i]["Id"]),
        //                VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
        //            });
        //        }
        //        // con.Close();
        //    }

        //    return ftype;
        //}

        [HttpPost]
        public async Task<ActionResult> Index(string submit, ExportPeram peram, HttpPostedFileBase photo, FormCollection collection)
        {
            //ExportPeram epm = new ExportPeram();
            //epm.VoyageId = peram.VoyageId;

            if (submit == "Export")
            {
                int vslid = Convert.ToInt32(Session["VesselID"]);
                ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
                // _lodingR.VesselId = vslid;
                DataSet ds = CommonMethods.ExportReportsTables(peram, vslid);
                //ds.Tables[0].TableName = "LoadingReport";
                //ds.Tables[1].TableName = "BerthingReport";
                //ds.Tables[2].TableName = "ArrivalReport";
                //ds.Tables[3].TableName = "DepartureReport";
                //ds.Tables[4].TableName = "DailyNoonReport";
                //ds.Tables[5].TableName = "DischargingReport";
                //DataTable DailyNoonReport = ds.Tables[0];
                //DailyNoonReport.TableName = "DailyNoonReport";

                //DataTable Fuel_Cons_NR = ds.Tables[1];
                //Fuel_Cons_NR.TableName = "Fuel_Cons_NR";

                //DataTable NR_Ballast_Tank = ds.Tables[2];
                //NR_Ballast_Tank.TableName = "NR_Ballast_Tank";

                //DataTable NR_Cargo = ds.Tables[3];
                //NR_Cargo.TableName = "NR_Cargo";

                //DataTable NR_Void_Space = ds.Tables[4];
                //NR_Void_Space.TableName = "NR_Void_Space";

                //DataTable NR_Cargo_Tank = ds.Tables[5];
                //NR_Cargo_Tank.TableName = "NR_Cargo_Tank";

                //DataTable ropeInspectionSettings = new DataTable();
                //DataTable ropeTailInspectionSettings = new DataTable();
                //DataTable notificationCommentSettings = new DataTable();
                //DataTable WinchRotationSettings = new DataTable();
                using (XLWorkbook wb = new XLWorkbook())
                {
                    // var protectedsheet = wb.Worksheets.Add(ds.Tables[0]);
                   // IXLWorksheet protectedsheet;
                    //for (int i = 0; i < ds.Tables.Count; i++)
                    int i = 0;
                    foreach (DataTable table in ds.Tables)
                    {
                      //  IXLWorksheet protectedsheet;
                        if (i == 0)
                            table.TableName = "LoadingReport";
                        else if (i == 1)
                            table.TableName = "BerthingReport";
                        else if (i == 2)
                            table.TableName = "ArrivalReport";
                        else if (i == 3)
                            table.TableName = "DepartureReport";
                        else if (i == 4)
                            table.TableName = "DailyNoonReport";
                        else if (i == 5)
                            table.TableName = "DischargingReport";
                        else if (i == 6)
                            table.TableName = "Fuel_Cons_NR";
                        else if (i == 7)
                            table.TableName = "NR_Ballast_Tank";
                        else if (i == 8)
                            table.TableName = "NR_Cargo";
                        else if (i == 9)
                            table.TableName = "NR_Void_Space";
                        else if (i == 10)
                            table.TableName = "NR_Cargo_Tank";
                        else if (i == 11)
                            table.TableName = "BR_Cargo";
                        else if (i == 12)
                            table.TableName = "DR_Cargo";
                        else if (i == 13)
                            table.TableName = "LR_Cargo";
                        else if (i == 14)
                            table.TableName = "LR_Ballast_PumpUse";
                        else if (i == 15)
                            table.TableName = "LR_DCR_PumpsUse";
                        else if (i == 16)
                            table.TableName = "LR_Stoppage";
                        else if (i == 17)
                            table.TableName = "DS_Cargo";

                        if (table.Rows.Count > 0)
                        {
                            var protectedsheet = wb.Worksheets.Add(table);

                            var projection = protectedsheet.Protect("49WEB$TREET#");
                            projection.InsertColumns = true;
                            projection.InsertRows = true;
                        }
                        i++;
                    }
                    
                    //var projection = protectedsheet.Protect("49WEB$TREET#");
                    //projection.InsertColumns = true;
                    //projection.InsertRows = true;

                    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    wb.Style.Font.Bold = true;

                    DateTime today = DateTime.Today;
                    //string vsname = searchTerm.Replace(" ", "");
                    //string HeaderName = "Work-Ship_Export_" + vsname + "_" + today.ToString("dd-MMM-yyyy");

                    Response.Clear();
                    Response.BufferOutput = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=Sis_Nova_"+DateTime.Now.ToString("ddMMyyyy")+"_Export_" + vslid + ".xlsx");

                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        wb.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.End();
                    }

                    Response.Clear();

                    Thread.Sleep(300);
                }
                
            }
            else
            {
                string query = string.Empty;
                try
                {
                    /*
                    if (photo != null)
                    {
                        int numberfiles = 0;

                        if (Request.Files["photo"].ContentLength > 0)
                        {
                            string fileExtension = Path.GetExtension(Request.Files["photo"].FileName);
                            string filenames = Path.GetFileName(Request.Files["photo"].FileName).Replace(".xls", "").Replace(".xlsx", "").Replace(".xlsm", "");
                            if (fileExtension == ".xls" || fileExtension == ".xlsx" || fileExtension == ".xlsm")
                            {
                                string location = Server.MapPath("~/Files/") + string.Format("{0}{1}", DateTime.Now.Ticks, fileExtension);
                                if (System.IO.File.Exists(location))
                                    System.IO.File.Delete(location);
                                Request.Files["photo"].SaveAs(location);
                                string filePath = filenames;
                                // SqlDatabase objdb = new SqlDatabase(OSMC.constring_Property);
                                DataSet importdataset = new DataSet();
                                //Open the Excel file using ClosedXML.
                                // XLWorkbook theWorkBook = new XLWorkbook(fullfilename);
                                int count = 1;
                                using (XLWorkbook workBook = new XLWorkbook(location))
                                {
                                    //Read the first Sheet from Excel file.
                                    //int allshheets = workBook.Worksheet.
                                    //  string sheetname = workBook.Table
                                    int worksheetcount = workBook.Worksheets.Count;
                                    for (int s = 1; s <= worksheetcount; s++)
                                    {

                                        IXLWorksheet workSheet = workBook.Worksheet(s);
                                        var sheetName = workBook.Worksheet(s).Name;
                                        //Create a new DataTable.
                                        DataTable dtx = new DataTable();

                                        //Loop through the Worksheet rows.
                                        bool firstRow = true;
                                        foreach (IXLRow row in workSheet.Rows())
                                        {

                                            //Use the first row to add columns to DataTable.
                                            if (firstRow)
                                            {
                                                foreach (IXLCell cell in row.Cells())
                                                {
                                                    dtx.Columns.Add(cell.Value.ToString());
                                                }
                                                firstRow = false;
                                            }
                                            else
                                            {
                                                //Add rows to DataTable.
                                                dtx.Rows.Add();
                                                int i = 0;

                                                //var col = row.Cells(row.FirstCellUsed().Address.ColumnNumber.ToString());
                                                //var row121 = row.LastCellUsed().Address.ColumnNumber;

                                                foreach (IXLCell cell in row.Cells())
                                                {
                                                    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                    i++;
                                                }
                                                //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                                //{
                                                //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                //    i++;
                                                //}
                                            }
                                        }

                                        InsertImportedData(sheetName, dtx);
                                    }
                                }
                                string importedTables = string.Empty;
                            }
                            else
                            {
                                //bmsuploaddats.bmsname = "No file(s) selected";
                            }
                        }
                    }
                    else
                    {
                       // bmsuploaddats.bmsname = "No file(s) selected";
                    }
                    */
                }
                catch (Exception ex)
                {

                }

            }
            return View(peram);
        }


        public ActionResult Import()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Import(string submit, string download, HttpPostedFileBase photo, FormCollection collection)
        {
            string query = string.Empty;
            try
            {

                if (photo != null)
                {
                    int numberfiles = 0;

                    if (Request.Files["photo"].ContentLength > 0)
                    {
                        string fileExtension = Path.GetExtension(Request.Files["photo"].FileName);
                        string filenames = Path.GetFileName(Request.Files["photo"].FileName).Replace(".xls", "").Replace(".xlsx", "").Replace(".xlsm", "");
                        if (fileExtension == ".xls" || fileExtension == ".xlsx" || fileExtension == ".xlsm")
                        {
                            string location = Server.MapPath("~/Files/") + string.Format("{0}{1}", DateTime.Now.Ticks, fileExtension);
                            if (System.IO.File.Exists(location))
                                System.IO.File.Delete(location);
                            Request.Files["photo"].SaveAs(location);
                            string filePath = filenames;
                            // SqlDatabase objdb = new SqlDatabase(OSMC.constring_Property);
                            DataSet importdataset = new DataSet();
                            //Open the Excel file using ClosedXML.
                            // XLWorkbook theWorkBook = new XLWorkbook(fullfilename);
                            int count = 1;
                            using (XLWorkbook workBook = new XLWorkbook(location))
                            {
                                //Read the first Sheet from Excel file.
                                //int allshheets = workBook.Worksheet.
                                //  string sheetname = workBook.Table
                                int worksheetcount = workBook.Worksheets.Count;
                                for (int s = 1; s <= worksheetcount; s++)
                                {

                                    IXLWorksheet workSheet = workBook.Worksheet(s);
                                    var sheetName = workBook.Worksheet(s).Name;
                                    //Create a new DataTable.
                                    DataTable dtx = new DataTable();

                                    //Loop through the Worksheet rows.
                                    bool firstRow = true;
                                    foreach (IXLRow row in workSheet.Rows())
                                    {

                                        //Use the first row to add columns to DataTable.
                                        if (firstRow)
                                        {
                                            foreach (IXLCell cell in row.Cells())
                                            {
                                                dtx.Columns.Add(cell.Value.ToString());
                                            }
                                            firstRow = false;
                                        }
                                        else
                                        {
                                            //Add rows to DataTable.
                                            dtx.Rows.Add();
                                            int i = 0;

                                            //var col = row.Cells(row.FirstCellUsed().Address.ColumnNumber.ToString());
                                            //var row121 = row.LastCellUsed().Address.ColumnNumber;

                                            foreach (IXLCell cell in row.Cells())
                                            {
                                                dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                i++;
                                            }
                                            //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                            //{
                                            //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                            //    i++;
                                            //}
                                        }
                                    }

                                    InsertImportedData(sheetName, dtx);
                                }
                            }
                            string importedTables = string.Empty;
                        }
                        else
                        {
                            //bmsuploaddats.bmsname = "No file(s) selected";
                        }
                    }
                }
                else
                {
                    // bmsuploaddats.bmsname = "No file(s) selected";
                }

            }
            catch (Exception ex)
            {

            }
            return View();
        }


        private void InsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {
                System.Data.DataTable dtt = tbls;

                if (sheetName == "Vessel Particulars")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            VesselDetail vd = new VesselDetail();
                            int ftid = 0; int fnameid = 0; int Vtid = 0;
                            //string vsname= dtt.Rows[j]["Vessel Name"].ToString();
                            vd.VesselName = dtt.Rows[j]["Vessel Name"].ToString();
                            if (vd.VesselName == "")
                            {
                                break;
                            }
                            vd.ImoNo = Convert.ToInt32(dtt.Rows[j]["IMO Number"]);
                            string FlType = dtt.Rows[j]["Fleet Type"].ToString();

                            using (SqlDataAdapter adp = new SqlDataAdapter("select Tid from tblFleetType where FleetType='" + FlType.Trim() + "'", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    ftid = Convert.ToInt32(dt.Rows[0][0]);
                                    vd.FleetTypeID = ftid;
                                }

                            }
                            string FlName = dtt.Rows[j]["Fleet Name"].ToString();
                            using (SqlDataAdapter adp = new SqlDataAdapter("select Fid from tblFleetName where FleetName='" + FlName.Trim() + "'", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    fnameid = Convert.ToInt32(dt.Rows[0][0]);
                                    vd.FleetNameID = fnameid;
                                }

                            }
                            string VTrade = dtt.Rows[j]["Vessel Trade"].ToString();

                            using (SqlDataAdapter adp = new SqlDataAdapter("select id from tblVesselTrade where VesselTrade='" + VTrade.Trim() + "'", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    Vtid = Convert.ToInt32(dt.Rows[0][0]);
                                    vd.VesselTradeID = Vtid;
                                }

                            }
                            vd.Displacement = Convert.ToDecimal(dtt.Rows[j]["Displacement"]);

                            CommonMethods.InsertUpdateVessel(vd, "Insert");
                        }
                    }
                }

                if (sheetName == "Cargo Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 1;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Cargo Holds")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 2;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Ballast Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 3;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Void Space")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 4;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Bunker Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 5;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Lube Oil Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 6;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Fresh Water Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 7;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Other Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 8;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Other Spaces")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            TanksAndHolds tanksH = new TanksAndHolds();
                            tanksH.Name = dtt.Rows[j][0].ToString();
                            if (tanksH.Name == "")
                            {
                                break;
                            }
                            tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
                            tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
                            tanksH.TanksTypeId = 9;
                            tanksH.VesselId = vslid;
                            CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                        }
                    }
                }

                if (sheetName == "Pumps")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselID"]);
                        for (int j = 0; j < dtt.Rows.Count; j++)
                        {
                            int ptid = 0; int puseid = 0;
                            PumpClass pcls = new PumpClass();
                            pcls.Name = dtt.Rows[j][2].ToString();
                            if (pcls.Name == "")
                            {
                                break;
                            }

                            string PType = dtt.Rows[j][0].ToString();

                            using (SqlDataAdapter adp = new SqlDataAdapter("select id from PumpType where Type='" + PType.Trim() + "'", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    ptid = Convert.ToInt32(dt.Rows[0][0]);
                                    pcls.PumpTypeId = ptid;
                                }
                            }

                            string PUse = dtt.Rows[j][1].ToString();

                            using (SqlDataAdapter adp = new SqlDataAdapter("select id from tblpumpuse where PumpName='" + PUse.Trim() + "'", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                if (dt.Rows.Count > 0)
                                {
                                    puseid = Convert.ToInt32(dt.Rows[0][0]);
                                    pcls.PumpUseId = puseid;
                                }

                            }
                            pcls.Capacity = Convert.ToDecimal(dtt.Rows[j][3]);
                            CommonMethods.InsertUpdatePumps(pcls, "Insert");
                        }
                    }
                }



            }
            catch (Exception ex)
            {


            }


        }
    }

}