using ClosedXML.Excel;

using DataBuildingLayer;
using Microsoft.AspNet.Identity;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;

namespace SIS_Operational_Reports.Areas.ImportExport.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class ImportExportController : BaseController
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


        public void DownloadZipFile(string VoyageId)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);

            string destinationFolder = Server.MapPath(string.Format("~/Bunker_LabAnalysisReport/Labfiles"));
            string startPath = Server.MapPath(string.Format("~/Bunker_LabAnalysisReport/files"));
            string zipFileName = string.Format("files.zip");
            string zipPath = Server.MapPath("~/Bunker_LabAnalysisReport/" + zipFileName);

            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
                Directory.CreateDirectory(destinationFolder);
            }
            else
            {
                Directory.CreateDirectory(destinationFolder);
            }
            // Delete the existing zip file if it exists
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            List<string> fileNames = new List<string>();
            string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select LabAnalysisReport_Name from BunkerReport where Is_Active =1 and VesselId=" + vslid + " and VoyageId in (" + VoyageId + ")";
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fileName = reader["LabAnalysisReport_Name"].ToString();
                        fileNames.Add(fileName);
                        if (Directory.Exists(startPath))
                        {
                            string sourceFilePath = Path.Combine(startPath, fileName);
                            string destinationFilePath = Path.Combine(destinationFolder, fileName);
                           System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                        }
                    }
                }
            }

            ZipFile.CreateFromDirectory(destinationFolder, zipPath, CompressionLevel.Fastest, true);

            foreach (var item in fileNames)
            {
                string path = Server.MapPath("~/Bunker_LabAnalysisReport/Labfiles/" + item);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            // Send the zip file path to the client
            Response.ContentType = "application/zip";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + zipFileName);
            Response.TransmitFile(zipPath);
            Response.Flush();
            Response.End();

            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
                //Directory.CreateDirectory(destinationFolder);
            }

        }




        [HttpPost]
        public async Task<ActionResult> Index(string submit, ExportPeram peram, HttpPostedFileBase photo, FormCollection collection)
        {
            //ExportPeram epm = new ExportPeram();
            //epm.VoyageId = peram.VoyageId;

            if (submit == "Export")
            {
                int vslid = Convert.ToInt32(Session["VesselID"]);

                string exportedby = User.Identity.GetUserName();


                string VoyageId = string.Join(",", peram.VoyageIds);

                DateTime crntdate= DateTime.Now;

                using (SqlDataAdapter adp = new SqlDataAdapter("insert into ExportLog values( '" + VoyageId + "','" + vslid + "','" + exportedby + "', getdate() )", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);
                }


                ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
                // _lodingR.VesselId = vslid;
                //if(peram.VoyageIds.Count>0)
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

                        else if (i == 18)
                            table.TableName = "AR_Cargo";

                        else if (i == 19)
                            table.TableName = "tbl_FuelROB";

                        else if (i == 20)
                            table.TableName = "tbl_BunkerLReceipt";
                        else if (i == 21)
                            table.TableName = "VoyageDetails";
                        else if (i == 22)
                            table.TableName = "VoyageLeg";
                        else if (i == 23)
                            table.TableName = "FuelConsumption";
                        else if (i == 24)
                            table.TableName = "tblNonRoutineCommon";
                        else if (i == 25)
                            table.TableName = "ExportLog";
                        else if (i == 26)
                            table.TableName = "tbl_BunkerLReceipt_NoonReport";
                        else if (i == 27)
                            table.TableName = "BunkerReport";
                        else if (i == 28)
                            table.TableName = "tbl_BunkerFuelType";
                        else if (i == 29)
                            table.TableName = "FreshWaterReport";

                        if (table.Rows.Count > 0)
                        {
                            var protectedsheet = wb.Worksheets.Add(table);

                            var projection = protectedsheet.Protect("49WEB$TREET#");
                            projection.InsertColumns = true;
                            projection.InsertRows = true;
                        }
                        i++;
                    }

                    //string destinationFolder = @"C:\Users\Developer\Desktop\Bunker_LabAnalysisReport";
                    //string zipFileName = @"C:\Users\Developer\Desktop\PDFFiles.zip";
                    //string folderPath = @"E:\Neeraj\SiS_Nova\Vessel\SIS_Operational_Reports\SIS_Operational_Reports\Bunker_LabAnalysisReport";

                    //if (System.IO.File.Exists(zipFileName))
                    //{
                    //    System.IO.File.Delete(zipFileName);
                    //}

                    //if (Directory.Exists(destinationFolder))
                    //{
                    //    Directory.Delete(destinationFolder, true);
                    //    Directory.CreateDirectory(destinationFolder);
                    //}

                    //string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
                    //using (SqlConnection connection = new SqlConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    string query = "select LabAnalysisReport_Name from BunkerReport where Is_Active =1 and VesselId="+ vslid + " and VoyageId="+ VoyageId + "";
                    //    using (SqlCommand command = new SqlCommand(query, connection))
                    //    using (SqlDataReader reader = command.ExecuteReader())
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            string fileName = reader["LabAnalysisReport_Name"].ToString();
                    //            string sourceFilePath = Path.Combine(folderPath, fileName);
                    //            string destinationFilePath = Path.Combine(destinationFolder, fileName);

                    //           System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                    //        }
                    //    }
                    //}

                    //ZipFile.CreateFromDirectory(destinationFolder, zipFileName);


                    
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
                    Response.AddHeader("content-disposition", "attachment;filename=Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_Export_" + vslid + ".xlsx");

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

            }
            else
            {
                string query = string.Empty;
                try
                {

                    if (photo != null)
                    {
                        int numberfiles = 0;

                        if (Request.Files["photo"].ContentLength > 0)
                        {
                            string ErrorINimportedTables = string.Empty;
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
                                    DataSet ExDataSet = new DataSet();
                                    int exportid = 0;
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

                                                //foreach (IXLCell cell in row.Cells())
                                                //{
                                                //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                //    i++;
                                                //}

                                                foreach (IXLCell cell in row.Cells(1, dtx.Columns.Count))
                                                {
                                                    if (cell.Value.ToString() != "")
                                                    {
                                                        dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                        i++;
                                                    }
                                                    else
                                                    {
                                                        dtx.Rows[dtx.Rows.Count - 1][i] = DBNull.Value;
                                                        i++;
                                                    }
                                                }

                                                //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                                //{
                                                //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                //    i++;
                                                //}
                                            }
                                        }
                                        dtx.TableName = sheetName;
                                        if (sheetName == "CPpart1")
                                        {
                                            var vi = dtx.Rows[0]["VesselID"].ToString();
                                            exportid = Convert.ToInt32(vi);
                                        }
                                        if (sheetName == "ByPass_Noondate")
                                        { 

                                        }
                                        ExDataSet.Tables.Add(dtx);
                                        //var getErrorsheet = InsertImportedData(sheetName, dtx);
                                        //if (!string.IsNullOrEmpty(getErrorsheet))
                                        //{
                                        //    ErrorINimportedTables += getErrorsheet + ", ";
                                        //}
                                    }

                                    var getErrorsheet = InsertImportedData(exportid ,ExDataSet);
                                    if (!string.IsNullOrEmpty(getErrorsheet))
                                    {
                                        ErrorINimportedTables += getErrorsheet + ", ";
                                    }
                                }
                                //
                                if (string.IsNullOrEmpty(ErrorINimportedTables))
                                {
                                    TempData["Success"] = "Data has been Imported Successfully";
                                }
                                else if ("Vessel Not found!" == ErrorINimportedTables)
                                {
                                    TempData["Error"] = "Vessel Not found!";
                                }
                                else 
                                {
                                    TempData["Error"] = "Data has been imported but except following Sheets:- " + Environment.NewLine + ErrorINimportedTables;
                                }
                            }
                            else
                            {
                                TempData["Error"] = "No file selected";
                            }
                        }
                    }
                    else
                    {
                        // bmsuploaddats.bmsname = "No file(s) selected";
                        TempData["Error"] = "No file selected";
                    }

                }
                catch (Exception ex)
                {

                }

            }
            return View(peram);
        }

        [HttpGet]
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

                                            //foreach (IXLCell cell in row.Cells())
                                            //{
                                            //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                            //    i++;
                                            //}

                                            foreach (IXLCell cell in row.Cells(1, dtx.Columns.Count))
                                            {
                                                if (cell.Value.ToString() != "")
                                                {
                                                    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                    i++;
                                                }
                                                else
                                                {
                                                    dtx.Rows[dtx.Rows.Count - 1][i] = DBNull.Value;
                                                    i++;
                                                }
                                            }


                                            //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                            //{
                                            //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                            //    i++;
                                            //}
                                        }
                                    }

                                    ImportActionInsertImportedData(sheetName, dtx);
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


        private void ImportActionInsertImportedData(string sheetName, System.Data.DataTable tbls)
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

                            Session["VesselIDImport"] = vd.ImoNo;
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

                            int checkExistense = CommonMethods.CheckVesselExistence(vd, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateVessel(vd, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Cargo Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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

                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Cargo Holds")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Ballast Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Void Space")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Bunker Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Lube Oil Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Fresh Water Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Other Tanks")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Other Spaces")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Pumps")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            pcls.VesselId = vslid;
                            int checkExistense = CommonMethods.CheckpumpExistence(pcls,vslid, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdatePumps(pcls, "Insert");
                            }
                        }
                    }
                }

                if (sheetName == "Other Spaces")
                {
                    if (dtt.Rows.Count > 0)
                    {
                        int vslid = Convert.ToInt32(Session["VesselIDImport"]);
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
                            int checkExistense = CommonMethods.CheckTHsNameExistence(tanksH, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {


            }


        }

        //private void ImportActionInsertImportedData(string sheetName, System.Data.DataTable tbls)
        //{
        //    try
        //    {
        //        System.Data.DataTable dtt = tbls;

        //        if (sheetName == "Vessel Particulars")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    VesselDetail vd = new VesselDetail();
        //                    int ftid = 0; int fnameid = 0; int Vtid = 0;
        //                    //string vsname= dtt.Rows[j]["Vessel Name"].ToString();
        //                    vd.VesselName = dtt.Rows[j]["Vessel Name"].ToString();
        //                    if (vd.VesselName == "")
        //                    {
        //                        break;
        //                    }
        //                    vd.ImoNo = Convert.ToInt32(dtt.Rows[j]["IMO Number"]);

        //                    Session["VesselIDImport"] = vd.ImoNo;
        //                    string FlType = dtt.Rows[j]["Fleet Type"].ToString();

        //                    using (SqlDataAdapter adp = new SqlDataAdapter("select Tid from tblFleetType where FleetType='" + FlType.Trim() + "'", ConnectionBulder.con))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        adp.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            ftid = Convert.ToInt32(dt.Rows[0][0]);
        //                            vd.FleetTypeID = ftid;
        //                        }

        //                    }
        //                    string FlName = dtt.Rows[j]["Fleet Name"].ToString();
        //                    using (SqlDataAdapter adp = new SqlDataAdapter("select Fid from tblFleetName where FleetName='" + FlName.Trim() + "'", ConnectionBulder.con))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        adp.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            fnameid = Convert.ToInt32(dt.Rows[0][0]);
        //                            vd.FleetNameID = fnameid;
        //                        }

        //                    }
        //                    string VTrade = dtt.Rows[j]["Vessel Trade"].ToString();

        //                    using (SqlDataAdapter adp = new SqlDataAdapter("select id from tblVesselTrade where VesselTrade='" + VTrade.Trim() + "'", ConnectionBulder.con))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        adp.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            Vtid = Convert.ToInt32(dt.Rows[0][0]);
        //                            vd.VesselTradeID = Vtid;
        //                        }

        //                    }
        //                    vd.Displacement = Convert.ToDecimal(dtt.Rows[j]["Displacement"]);

        //                    CommonMethods.InsertUpdateVessel(vd, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Cargo Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 1;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Cargo Holds")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 2;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Ballast Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 3;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Void Space")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 4;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Bunker Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 5;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Lube Oil Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 6;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Fresh Water Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 7;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Other Tanks")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 8;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Other Spaces")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    TanksAndHolds tanksH = new TanksAndHolds();
        //                    tanksH.Name = dtt.Rows[j][0].ToString();
        //                    if (tanksH.Name == "")
        //                    {
        //                        break;
        //                    }
        //                    tanksH.Height = Convert.ToDecimal(dtt.Rows[j][1]);
        //                    tanksH.Capacity = Convert.ToDecimal(dtt.Rows[j][2]);
        //                    tanksH.TanksTypeId = 9;
        //                    tanksH.VesselId = vslid;
        //                    CommonMethods.InsertUpdateTanksAndHolds(tanksH, "Insert");
        //                }
        //            }
        //        }

        //        if (sheetName == "Pumps")
        //        {
        //            if (dtt.Rows.Count > 0)
        //            {
        //                int vslid = Convert.ToInt32(Session["VesselIDImport"]);
        //                for (int j = 0; j < dtt.Rows.Count; j++)
        //                {
        //                    int ptid = 0; int puseid = 0;
        //                    PumpClass pcls = new PumpClass();
        //                    pcls.Name = dtt.Rows[j][2].ToString();
        //                    if (pcls.Name == "")
        //                    {
        //                        break;
        //                    }

        //                    string PType = dtt.Rows[j][0].ToString();

        //                    using (SqlDataAdapter adp = new SqlDataAdapter("select id from PumpType where Type='" + PType.Trim() + "'", ConnectionBulder.con))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        adp.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            ptid = Convert.ToInt32(dt.Rows[0][0]);
        //                            pcls.PumpTypeId = ptid;
        //                        }
        //                    }

        //                    string PUse = dtt.Rows[j][1].ToString();

        //                    using (SqlDataAdapter adp = new SqlDataAdapter("select id from tblpumpuse where PumpName='" + PUse.Trim() + "'", ConnectionBulder.con))
        //                    {
        //                        DataTable dt = new DataTable();
        //                        adp.Fill(dt);

        //                        if (dt.Rows.Count > 0)
        //                        {
        //                            puseid = Convert.ToInt32(dt.Rows[0][0]);
        //                            pcls.PumpUseId = puseid;
        //                        }

        //                    }
        //                    pcls.Capacity = Convert.ToDecimal(dtt.Rows[j][3]);
        //                    CommonMethods.InsertUpdatePumps(pcls, "Insert");
        //                }
        //            }
        //        }



        //    }
        //    catch (Exception ex)
        //    {


        //    }


        //}


        //public string NoonReport_dateDecode(string date)
        //{
        //    System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
        //    System.Text.Decoder utf8Decode = encoder.GetDecoder();
        //    byte[] todecode_byte = Convert.FromBase64String(date);
        //    int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
        //    char[] decoded_char = new char[charCount];
        //    utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
        //    string decryptdate = new String(decoded_char);
        //    return decryptdate;
        //}

        private string InsertImportedData(int Vesselid, DataSet dts)
        {
            string sheetName = "";
            try
            {
                 int imo = 0;
                string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlDataAdapter sda = new SqlDataAdapter("select ImoNo from VesselDetail",con);
                    DataTable tbl = new DataTable();
                    sda.Fill(tbl);
                    if(tbl.Rows.Count>0)
                    {
                         imo = Convert.ToInt32(tbl.Rows[0]["ImoNo"]);

                    }
                    
                }
                if (imo == Vesselid)
                {
                    for (int t = 0; t < dts.Tables.Count; t++)
                    {
                        System.Data.DataTable tbls = dts.Tables[t];
                        sheetName = dts.Tables[t].TableName;


                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            if (sheetName == "AspNetUsers")
                            {
                                for (int i = 0; i < tbls.Rows.Count; i++)
                                {

                                    string Id = tbls.Rows[i]["Id"].ToString();
                                    string Email = tbls.Rows[i]["Email"].ToString();
                                    //bool EmailConfirmed = Convert.ToBoolean(tbls.Rows[i]["EmailConfirmed"]);
                                    bool EmailConfirmed = false;
                                    string PasswordHash = tbls.Rows[i]["PasswordHash"].ToString();
                                    string SecurityStamp = tbls.Rows[i]["SecurityStamp"].ToString();
                                    string PhoneNumber = tbls.Rows[i]["PhoneNumber"].ToString();
                                    bool PhoneNumberConfirmed = Convert.ToBoolean(tbls.Rows[i]["PhoneNumberConfirmed"]);
                                    bool TwoFactorEnabled = Convert.ToBoolean(tbls.Rows[i]["TwoFactorEnabled"]);
                                    var LockoutEndDateUtc = tbls.Rows[i]["LockoutEndDateUtc"].ToString();
                                    // bool LockoutEnabled = Convert.ToBoolean(tbls.Rows[i]["LockoutEnabled"]);
                                    int AccessFailedCount = 0;
                                    var AccessFailedCountSrt = tbls.Rows[i]["AccessFailedCount"].ToString();

                                    string UserName = tbls.Rows[i]["UserName"].ToString();

                                    using (SqlDataAdapter adp1 = new SqlDataAdapter("InsertTOAspNetUsers", con))
                                    {

                                        adp1.SelectCommand.CommandType = CommandType.StoredProcedure;
                                        adp1.SelectCommand.Parameters.AddWithValue("@Id", Id);
                                        adp1.SelectCommand.Parameters.AddWithValue("@Email", Email.Trim());
                                        adp1.SelectCommand.Parameters.AddWithValue("@EmailConfirmed", EmailConfirmed);
                                        adp1.SelectCommand.Parameters.AddWithValue("@PasswordHash", PasswordHash.Trim());
                                        adp1.SelectCommand.Parameters.AddWithValue("@SecurityStamp", SecurityStamp.Trim());
                                        adp1.SelectCommand.Parameters.AddWithValue("@PhoneNumber", PhoneNumber.Trim());
                                        adp1.SelectCommand.Parameters.AddWithValue("@PhoneNumberConfirmed", PhoneNumberConfirmed);
                                        adp1.SelectCommand.Parameters.AddWithValue("@TwoFactorEnabled", TwoFactorEnabled);
                                        if (string.IsNullOrEmpty(LockoutEndDateUtc))
                                            adp1.SelectCommand.Parameters.AddWithValue("@LockoutEndDateUtc", DBNull.Value);
                                        else
                                            adp1.SelectCommand.Parameters.AddWithValue("@LockoutEndDateUtc", DBNull.Value);
                                        adp1.SelectCommand.Parameters.AddWithValue("@LockoutEnabled", true);

                                        adp1.SelectCommand.Parameters.AddWithValue("@AccessFailedCount", AccessFailedCount);
                                        //adp1.SelectCommand.Parameters.AddWithValue("@UserName", Email);
                                        adp1.SelectCommand.Parameters.AddWithValue("@UserName", UserName);


                                        System.Data.DataTable dtt1 = new System.Data.DataTable();
                                        adp1.Fill(dtt1);
                                    }
                                }

                            }
                            else if (sheetName == "PortList")
                            {
                                PortListClass prtlist = new PortListClass();

                                for (int i = 0; i < tbls.Rows.Count; i++)
                                {

                                    prtlist.CountryCode = tbls.Rows[i]["CountryCode"].ToString();
                                    prtlist.CountryName = tbls.Rows[i]["CountryName"].ToString();

                                    prtlist.PortName = tbls.Rows[i]["PortName"].ToString();
                                    prtlist.FacilityName = tbls.Rows[i]["FacilityName"].ToString();
                                    prtlist.IMOPortFacilityNumber = tbls.Rows[i]["IMOPortFacilityNumber"].ToString();

                                    prtlist.Longitude = tbls.Rows[i]["Longitude"].ToString();
                                    prtlist.Latitude = tbls.Rows[i]["Latitude"].ToString();

                                    int countchk = 0;
                                    using (SqlDataAdapter adp = new SqlDataAdapter("SELECT *   FROM PortList   WHERE PortName = '" + prtlist.PortName + "' and FacilityName = '" + prtlist.FacilityName + "'", ConnectionBulder.con))
                                    {
                                        DataTable dt = new DataTable();
                                        adp.Fill(dt);
                                        if (dt.Rows.Count > 0)
                                        {
                                            countchk = 1;
                                        }
                                    }
                                    if (countchk == 0)
                                    {
                                        CommonMethods.InsertPort(prtlist);
                                    }
                                }

                            }
                            else if (sheetName == "ByPass_Noondate")
                            {
                                for (int i = 0; i < tbls.Rows.Count; i++)
                                {

                                   // int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                                    int Vessel_Id = Convert.ToInt32(tbls.Rows[i]["Vessel_Id"]);
                                    string Noon_date = tbls.Rows[i]["Noon_date"].ToString();
                                   // string Noon_date = data;
                                   // bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);
                                   // string CreatedDate = tbls.Rows[i]["CreatedDate"].ToString();

                                    using (SqlDataAdapter adp1 = new SqlDataAdapter("InsertTOByPass_Noondate", con))
                                    {

                                        adp1.SelectCommand.CommandType = CommandType.StoredProcedure;
                                       // adp1.SelectCommand.Parameters.AddWithValue("@Id", Id);
                                        adp1.SelectCommand.Parameters.AddWithValue("@Vessel_Id", Vessel_Id);
                                        adp1.SelectCommand.Parameters.AddWithValue("@Noon_date", Noon_date);
                                        //adp1.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                                        //adp1.SelectCommand.Parameters.AddWithValue("@CreatedDate", CreatedDate.Trim());
                                        System.Data.DataTable dtt1 = new System.Data.DataTable();
                                        adp1.Fill(dtt1);
                                    }
                                }
                            }
                            else
                            {
                                using (SqlDataAdapter adp1 = new SqlDataAdapter("Truncate Table " + sheetName + "", con))
                                {
                                    System.Data.DataTable dtt1 = new System.Data.DataTable();
                                    adp1.Fill(dtt1);
                                }

                                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                                {
                                    //Set the database table name
                                    sqlBulkCopy.DestinationTableName = sheetName;

                                    //[OPTIONAL]: Map the Excel columns with that of the database table

                                    con.Open();
                                    sqlBulkCopy.WriteToServer(tbls);
                                    con.Close();
                                }
                            }
                        }
                    }
                    return "";
                }
                else
                    return "Vessel Not found!";
                

            }
            catch (Exception ex)
            {
                return sheetName;

            }


        }
    }


    //public class AspNetUsers
    //{

    //}
}