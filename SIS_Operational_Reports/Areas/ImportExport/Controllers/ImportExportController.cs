using ClosedXML.Excel;
using DataBuildingLayer;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.Identity;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace SIS_Operational_Reports.Areas.ImportExport.Controllers
{

    [Authorize]
    [UserAuthenticationFilter]
    public class ImportExportController : BaseController
    {
        // GET: ImportExport/ImportExport
        public ActionResult Index()
        {
            
            ExportPeram2 epm = new ExportPeram2();
            epm.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
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

       
        public static List<ImportLgClass> ImportLogList(int cpage)
        {
            try
            {

               


                List<ImportLgClass> importLogs = new List<ImportLgClass>();
                //using (SqlDataAdapter adapter = new SqlDataAdapter("select a.*,b.vesselname from ImportLog a inner join VesselDetail b on a.VesselId=b.ImoNo  order by b.VesselName asc OFFSET ("+cpage+"-1) * 10 ROWS FETCH NEXT 10 ROWS ONLY ", ConnectionBulder.con))
                using (SqlDataAdapter adapter = new SqlDataAdapter("GetimportlogList", ConnectionBulder.con))
                {
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                       // adapter.SelectCommand.Parameters.AddWithValue("@VesselId", "");
                        //adapter.SelectCommand.Parameters.AddWithValue("@Action", "CPAdminList");
                        adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", cpage);
                        adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", 10);
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {

                           

                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                string voygeno = "";
                                var stringToSplit = dataTable.Tables[0].Rows[i]["VoyageId"].ToString();
                                var vslid = dataTable.Tables[0].Rows[i]["VesselId"].ToString();
                                List<string[]> arrays = new List<string[]>();
                                var primeArray = stringToSplit.Split(',');
                                for (int j = 0; j < primeArray.Length; j++)
                                {
                                    var first = primeArray[j];


                                    using (SqlDataAdapter adp = new SqlDataAdapter("select VoyageNumber from VoyageDetails where id="+ first + " and VesselId='"+ vslid + "' and IsActive=1", ConnectionBulder.con))
                                    {
                                        DataTable dt = new DataTable();
                                        adp.Fill(dt);
                                        for (int k = 0; k < dt.Rows.Count; k++)
                                        {
                                            voygeno += dt.Rows[k]["VoyageNumber"].ToString() + ",";
                                        }
                                    }

                                }

                                importLogs.Add(new ImportLgClass()
                                {   
                                   
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    VoyageNo = voygeno,
                                    ExportedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ExportedDate"]),
                                    ImportedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ImportedDate"]),
                                  
                                    ExportedBy = dataTable.Tables[0].Rows[i]["ExportedBy"].ToString(),
                                    ImportedBy = dataTable.Tables[0].Rows[i]["ImportedBy"].ToString(),
                                    DataAvailabilityTillDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["DataAvailabilityTillDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),
                                });
                            }
                        }
                    }
                }
                return importLogs;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<ErrorLogClass> ErrorLogList(int cpage)
        {
            try
            {
                List<ErrorLogClass> errorLogs = new List<ErrorLogClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("select * from errorlog", ConnectionBulder.con))
                {
                    using (DataSet dataTable = new DataSet())
                    {                       
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                errorLogs.Add(new ErrorLogClass()
                                {
                                    SheetName = dataTable.Tables[0].Rows[i]["SheetName"].ToString(),                                   
                                    ImportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ImportDate"]),                                   
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),
                                });
                            }
                        }
                    }
                }
                return errorLogs;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public ActionResult ImportLog(int? pageNo)
        {
            ImportLgClass lg = new ImportLgClass();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);


            TempData["CurrentPage"] = currPage;

            lg.ImportLogInfo = ImportLogList(currPage);
            return View(lg);
        }

        public ActionResult ErrorLog(int? pageNo)
        {
            ErrorLogClass lg = new ErrorLogClass();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);


            TempData["CurrentPage"] = currPage;

            lg.ErrorLogInfo = ErrorLogList(currPage);
            return View(lg);
        }


        //public static string NoonReport_dateEncrypt(string date)
        //{
        //    using (var sha256 = SHA256.Create())
        //    {
        //        // Convert the password string to a byte array
        //        byte[] bytes = Encoding.UTF8.GetBytes(date);

        //        // Compute the hash
        //        byte[] hashBytes = sha256.ComputeHash(bytes);

        //        // Convert the hash bytes back to a string representation
        //        string encryptdate = Convert.ToBase64String(hashBytes);
        //        return encryptdate;
        //    }
        //}


        public static string NoonReport_dateEncrypt(string date)
        {
            try
            {
                byte[] encData_byte = new byte[date.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(date);
                string encryptdate = Convert.ToBase64String(encData_byte);
                return encryptdate;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Index(string submit, ExportPeram2 peram, HttpPostedFileBase photo, FormCollection collection)
        {
            //ExportPeram epm = new ExportPeram();
            //epm.VoyageId = peram.VoyageId;

            if (submit == "Export")
            {

                DataSet ds = CommonMethods.ExportSettingTables(peram);
                DataTable myTable =  ds.Tables[13];
                for (int i = 0; i < myTable.Rows.Count; i++)
                {
                    string data = NoonReport_dateEncrypt(myTable.Rows[i]["Noon_date"].ToString());
                    myTable.Rows[i]["Noon_date"] = data;
                }


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
                            table.TableName = "CPContract";
                        else if (i == 1)
                            table.TableName = "CPpart1";
                        else if (i == 2)
                            table.TableName = "CPpart2";
                        else if (i == 3)
                            table.TableName = "CPpart3";
                        else if (i == 4)
                            table.TableName = "TanksAndHolds";
                        else if (i == 5)
                            table.TableName = "tblPump";
                        else if (i == 6)
                            table.TableName = "PumpType";
                        else if (i == 7)
                            table.TableName = "tblPumpUse";
                        else if (i == 8)
                            table.TableName = "tblFuelType";
                        else if (i == 9)
                            table.TableName = "UserDetail";
                        else if (i == 10)
                            table.TableName = "AspNetUsers";                       
                        else if (i == 11)
                            table.TableName = "VesselDetail";
                        else if (i == 12)
                            table.TableName = "tblCargoGrades";
                        else if (i == 13)
                            table.TableName = "NoonReport_allow";
                        else if (i == 14)
                            table.TableName = "SyncEmailVesselsReport";
                        //else if (i == 12)
                        //    table.TableName = "tblCargoGrades";
                        //else if (i == 11)
                        //    table.TableName = "BR_Cargo";
                        //else if (i == 12)
                        //    table.TableName = "DR_Cargo";
                        //else if (i == 13)
                        //    table.TableName = "LR_Cargo";
                        //else if (i == 14)
                        //    table.TableName = "LR_Ballast_PumpUse";
                        //else if (i == 15)
                        //    table.TableName = "LR_DCR_PumpsUse";
                        //else if (i == 16)
                        //    table.TableName = "LR_Stoppage";
                        //else if (i == 17)
                        //    table.TableName = "DS_Cargo";

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
                    Response.AddHeader("content-disposition", "attachment;filename=Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_OfficeExport_" + peram.VesselID + ".xlsx");

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
                //try
                //{

                    if (photo != null)
                    {
                        int numberfiles = 0;

                        if (Request.Files["photo"].ContentLength > 0)
                        {
                            string ErrorINimportedTables = string.Empty;
                            string fileExtension = Path.GetExtension(Request.Files["photo"].FileName);
                            string filenames = Path.GetFileName(Request.Files["photo"].FileName).Replace(".xls", "").Replace(".xlsx", "").Replace(".xlsm", "");
                            if (fileExtension.ToLower() == ".xls" || fileExtension.ToLower() == ".xlsx" || fileExtension.ToLower() == ".xlsm")
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
                                int amit = 0;
                                int rana = 0;
                                try
                                {
                                  
                                    for (int s = 1; s <= worksheetcount; s++)
                                    {

                                        amit++;

                                        IXLWorksheet workSheet = workBook.Worksheet(s);
                                        var sheetName = workBook.Worksheet(s).Name;
                                        //Create a new DataTable.
                                        DataTable dtx = new DataTable();


                                       // if (sheetName == "Fuel_Cons_NR")
                                        if (sheetName == "Fuel_Cons_NR")
                                        {
                                            //continue;

                                            Task<DataTable> dtxx = generateTable(location, sheetName);

                                            var getErrorsheet = InsertImportedData(sheetName, await dtxx);
                                            if (!string.IsNullOrEmpty(getErrorsheet))
                                            {
                                                ErrorINimportedTables += getErrorsheet + ", ";
                                            }
                                        }
                                        else
                                        {
                                            //Loop through the Worksheet rows.
                                            bool firstRow = true;
                                            foreach (IXLRow row in workSheet.Rows())
                                            {
                                                rana++;
                                                int lastrow = workSheet.LastRowUsed().RowNumber();
                                                var rows = workSheet.Rows(1, lastrow);
                                                foreach (IXLRow row1 in rows)
                                                {
                                                    if (row1.IsEmpty())
                                                        row1.Delete();
                                                }



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

                                                    //foreach (IXLCell cell in row.Cells())
                                                    //{
                                                    //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                    //    i++;
                                                    //}
                                                    //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                                    //{
                                                    //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                    //    i++;
                                                    //}
                                                }
                                            }


                                            var getErrorsheet = InsertImportedData(sheetName, dtx);
                                            if (!string.IsNullOrEmpty(getErrorsheet))
                                            {
                                                ErrorINimportedTables += getErrorsheet + ", ";
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { int cc= amit;int kk=rana; }
                            }
                                if (string.IsNullOrEmpty(ErrorINimportedTables))
                                {
                                    TempData["Success"] = "Data has been Imported Successfully";
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
                        TempData["Error"] = "No file selected";
                    }

                //}
                //catch (Exception ex)
                //{

                //}

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


                                maxdt = new List<DateTime>();

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

                                   

                                    ImportActionInsertImportedData(sheetName, dtx);
                                }
                            }
                            //string importedTables = string.Empty;
                            if (string.IsNullOrEmpty(ErrorINimportedTables))
                            {
                                if (seatcheck == 1)
                                {
                                    TempData["Success"] = "Data has been Imported Successfully";
                                    FormsAuthentication.SignOut();
                                    return RedirectToAction("Login", "Account", new { Area = "" });
                                }else
                                {
                                    TempData["Error"] = "Invalid File !";

                                }
                            }
                            else
                            {
                                TempData["Error"] = "Data has been imported but except following Sheets:- " + Environment.NewLine + ErrorINimportedTables;
                            }
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



        static int seatcheck=0;

        private void ImportActionInsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {
                System.Data.DataTable dtt = tbls;

                if (sheetName == "Vessel Particulars")
                {
                    seatcheck = 1;
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
                            if(vd.VesselTradeID == 0)
                            {
                                vd.VesselTradeID = 3;
                            }
                            vd.Displacement = Convert.ToDecimal(dtt.Rows[j]["Displacement"]);

                            int checkExistense = CommonMethods.CheckVesselExistence(vd, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {
                                CommonMethods.InsertUpdateVessel(vd, "Insert");


                                if(StaticHelper.UserRole == "Administrator")
                                {
                                    var pv = StaticHelper.PermittedVessel + "," + vd.ImoNo;

                                    using (SqlDataAdapter adp = new SqlDataAdapter("update UserDetail set AssignVessel ='" + pv + "' where UserID='" + StaticHelper.UserId + "'", ConnectionBulder.con))
                                    {
                                        DataTable dt = new DataTable();
                                        adp.Fill(dt);

                                    }
                                }

                                

                            }
                        }
                    }
                }

                if (sheetName == "Cargo Tanks")
                {
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                    seatcheck = 1;
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
                            int checkExistense = CommonMethods.CheckpumpExistence(pcls, 0, 0, "Insert");
                            if (checkExistense == 0)
                            {                              
                                CommonMethods.InsertUpdatePumps(pcls, "Insert");
                            }
                        }
                    }
                }



            }
            catch (Exception ex)
            {


            }


        }

        List<DateTime> maxdt = new List<DateTime>();
        private string InsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {
                tbls = tbls.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(f => f is DBNull ||
                                  string.IsNullOrEmpty(f as string ?? f.ToString())))
                 .CopyToDataTable();


                string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
                if (sheetName == "DailyNoonReport" || sheetName == "ArrivalReport" || sheetName == "DepartureReport" || sheetName == "BerthingReport")
                {
                    object maxDate = tbls.Compute("MAX(ModifiedDate)", null);
                    maxdt.Add(Convert.ToDateTime(maxDate));

                }
                if (sheetName == "DischargingReport" || sheetName == "LoadingReport")
                {
                    object maxDate = tbls.Compute("MAX(ModifyDate)", null);

                    maxdt.Add(Convert.ToDateTime(maxDate));
                }

                if (sheetName == "ExportLog")
                {
                    DateTime biggestDate = maxdt.Max(p => p);

                    for ( int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        string VoyageId = tbls.Rows[i]["VoyageId"].ToString();
                      
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);


                        string importedby = User.Identity.GetUserName();

                        string exportedby = tbls.Rows[i]["ExportedBy"].ToString();
                        DateTime exporteddate = Convert.ToDateTime(tbls.Rows[i]["ExportedDate"]);

                        DateTime crntdate = DateTime.UtcNow;

                        using (SqlDataAdapter adp = new SqlDataAdapter("insert into ImportLog values( '" + VesselId + "','" + VoyageId + "','" + exporteddate + "', '" + crntdate + "','" + exportedby + "', '" + importedby + "','" + biggestDate + "' )", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                        }
                    }
                }

                //else if (sheetName == "Fuel_Cons_NR")
                //{

                //    for (int i = 0; i < tbls.Rows.Count; i++)
                //    {
                //        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                //        int Noon_Report_Id = Convert.ToInt32(tbls.Rows[i]["Noon_Report_Id"]);
                //        int FuelTypeId = Convert.ToInt32(tbls.Rows[i]["FuelTypeId"]);
                //        int ConsTypeId = Convert.ToInt32(tbls.Rows[i]["ConsTypeId"]);
                //        decimal Value = Convert.ToDecimal(tbls.Rows[i]["Value"]);
                //        int ReportType_Id = Convert.ToInt32(tbls.Rows[i]["ReportType_Id"]);
                //        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                //        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdate_FuelConsNR", ConnectionBulder.con))
                //        {
                //            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                //            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@Noon_Report_Id", Noon_Report_Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@FuelTypeId", FuelTypeId);
                //            adapter.SelectCommand.Parameters.AddWithValue("@ConsTypeId", ConsTypeId);
                //            adapter.SelectCommand.Parameters.AddWithValue("@Value", Value);
                //            adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                //            using (DataTable dataTable = new DataTable())
                //            {
                //                adapter.Fill(dataTable);

                //            }
                //        }
                //    }
                //}

                else if (sheetName == "VoyageLeg")
                {
                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int VoyageId = Convert.ToInt32(tbls.Rows[i]["VoyageId"]);
                        string LegPort_A = tbls.Rows[i]["LegPort_A"].ToString();
                        int ReasonforPortCall_A = Convert.ToInt32(tbls.Rows[i]["ReasonforPortCall_A"]);
                        string LegPort_B = tbls.Rows[i]["LegPort_B"].ToString();
                        int ReasonforPortCall_B = Convert.ToInt32(tbls.Rows[i]["ReasonforPortCall_B"]);
                        decimal DTG = Convert.ToDecimal(tbls.Rows[i]["DTG"]);
                        decimal CP_SOG = Convert.ToDecimal(tbls.Rows[i]["CP_SOG"]);
                        decimal CP_Log_Speed = Convert.ToDecimal(tbls.Rows[i]["CP_Log_Speed"]);
                        DateTime CreatedDate = Convert.ToDateTime(tbls.Rows[i]["CreatedDate"]);
                        bool IsActive = Convert.ToBoolean(tbls.Rows[i]["IsActive"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertVoyagelegImport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", VoyageId);
                            adapter.SelectCommand.Parameters.AddWithValue("@LegPort_A", LegPort_A);
                            adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_A", ReasonforPortCall_A);
                            adapter.SelectCommand.Parameters.AddWithValue("@LegPort_B", LegPort_B);
                            adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_B", ReasonforPortCall_B);

                            adapter.SelectCommand.Parameters.AddWithValue("@DTG", DTG);
                            adapter.SelectCommand.Parameters.AddWithValue("@CP_SOG", CP_SOG);
                            adapter.SelectCommand.Parameters.AddWithValue("@CP_Log_Speed", CP_Log_Speed);


                            adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", CreatedDate);
                            adapter.SelectCommand.Parameters.AddWithValue("@IsActive", IsActive);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);

                            //adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                            // SqlDataAdapter adapter = new SqlDataAdapter(command);
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }
                else if (sheetName == "tbl_BunkerLReceipt_NoonReport")
                {
                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int FuelType_Id = Convert.ToInt32(tbls.Rows[i]["FuelType_Id"]);
                        //string EOSP = tbls.Rows[i]["EOSP"].ToString();
                       // int FWE = Convert.ToInt32(tbls.Rows[i]["FWE"]);
                        decimal Receipt = Convert.ToDecimal(tbls.Rows[i]["Receipt"]);
                        int TableMax_Id = Convert.ToInt32(tbls.Rows[i]["TableMax_Id"]);
                        int ReportType_Id = Convert.ToInt32(tbls.Rows[i]["ReportType_Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                        using (SqlDataAdapter adapter = new SqlDataAdapter("tbl_BunkerLReceipt_NoonReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", FuelType_Id);
                           // adapter.SelectCommand.Parameters.AddWithValue("@EOSP", EOSP);
                           // adapter.SelectCommand.Parameters.AddWithValue("@FWE", FWE);
                            adapter.SelectCommand.Parameters.AddWithValue("@Receipt", Receipt);
                            adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", TableMax_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }
                else if (sheetName == "BunkerReport")
                {
                    //string zipFilePath = @"C:\Users\Developer\Desktop\files.zip";
                    ////string folderPath = @"E:\Neeraj\SiS_Nova\SIS Office Latest\SIS_Office_Reports\SIS_Operational_Reports\Bunker_LabAnalysisReport";
                    //string folderPath = Server.MapPath(string.Format("~/Bunker_LabAnalysisReport"));

                    //if (System.IO.File.Exists(zipFilePath))
                    //{
                    //    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                    //    {
                    //        foreach (ZipArchiveEntry entry in archive.Entries)
                    //        {
                    //            if (Path.GetExtension(entry.Name).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    //            {
                    //                string fileToExtract = Path.Combine(folderPath, entry.Name);
                    //                entry.ExtractToFile(fileToExtract, true);
                    //            }
                    //        }
                    //    }
                    //}

                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        string PortName = (tbls.Rows[i]["PortName"]).ToString();
                        int VoyageId = Convert.ToInt32(tbls.Rows[i]["VoyageId"]);
                        string Supplier = (tbls.Rows[i]["Supplier"]).ToString();
                        string BargeAlongside = (tbls.Rows[i]["BargeAlongside"]).ToString();
                        string BunkerHoseConnected = (tbls.Rows[i]["BunkerHoseConnected"]).ToString();
                        string CommencedBunkering = (tbls.Rows[i]["CommencedBunkering"]).ToString();
                        string BunkeringCompleted = (tbls.Rows[i]["BunkeringCompleted"]).ToString();
                        string BunkerHosedisconnected = (tbls.Rows[i]["BunkerHosedisconnected"]).ToString();
                        string BargeCastOff = (tbls.Rows[i]["BargeCastOff"]).ToString();
                        string BargeName = (tbls.Rows[i]["BargeName"]).ToString();
                        string Remarks = (tbls.Rows[i]["Remarks"]).ToString();
                        string LabAnalysisReport_Name = (tbls.Rows[i]["LabAnalysisReport_Name"]).ToString();
                        string FirstName = (tbls.Rows[i]["FirstName"]).ToString();
                        string LastName = (tbls.Rows[i]["LastName"]).ToString();
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);
                        string Created_Date = (tbls.Rows[i]["Created_Date"]).ToString();
                        string Modified_Date = "";
                        if (tbls.Rows[i]["Modified_Date"] != null)
                        {
                           Modified_Date = (tbls.Rows[i]["Modified_Date"]).ToString();
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter("InsertUpdateBunkerReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            adapter.SelectCommand.Parameters.AddWithValue("@PortName", PortName);
                            adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", VoyageId);
                            adapter.SelectCommand.Parameters.AddWithValue("@Supplier", Supplier);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeAlongside", BargeAlongside);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkerHoseConnected", BunkerHoseConnected);
                            adapter.SelectCommand.Parameters.AddWithValue("@CommencedBunkering", CommencedBunkering);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkeringCompleted", BunkeringCompleted);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkerHosedisconnected", BunkerHosedisconnected);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeCastOff", BargeCastOff);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeName", BargeName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Remarks", Remarks);
                            adapter.SelectCommand.Parameters.AddWithValue("@LabAnalysisReport_Name", LabAnalysisReport_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@FirstName", FirstName);
                            adapter.SelectCommand.Parameters.AddWithValue("@LastName", LastName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Created_Date", Created_Date);
                            adapter.SelectCommand.Parameters.AddWithValue("@Modified_Date", Modified_Date);

                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }

                else if (sheetName == "FreshWaterReport")
                {
                    
                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        string PortName = tbls.Rows[i]["PortName"].ToString();
                        string Facility_Name = tbls.Rows[i]["Facility_Name"].ToString();
                        string VendorDetails = tbls.Rows[i]["VendorDetails"].ToString();
                        decimal Intial_Meter_Reading_MT_supplied = Convert.ToDecimal(tbls.Rows[i]["Intial_Meter_Reading_MT_supplied"]);
                        decimal Final_Meter_Reading_MT = Convert.ToDecimal(tbls.Rows[i]["Final_Meter_Reading_MT"]);
                        decimal Difference_in_Meter_Reading_MT = Convert.ToDecimal(tbls.Rows[i]["Difference_in_Meter_Reading_MT"]);
                        decimal QTY_supplied_MT = Convert.ToDecimal(tbls.Rows[i]["QTY_supplied_MT"]);
                        string File_Name = (tbls.Rows[i]["File_Name"]).ToString();
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        string Received_Date = tbls.Rows[i]["Received_Date"].ToString();
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);
                        string Created_Date = (tbls.Rows[i]["Created_Date"]).ToString();
                       // string Modified_Date = (tbls.Rows[i]["Modified_Date"]).ToString();

                        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertFreshWaterReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@PortName", PortName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Facility_Name", Facility_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@VendorDetails", VendorDetails);
                            adapter.SelectCommand.Parameters.AddWithValue("@Intial_Meter_Reading_MT_supplied", Intial_Meter_Reading_MT_supplied);
                            adapter.SelectCommand.Parameters.AddWithValue("@Final_Meter_Reading_MT", Final_Meter_Reading_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@Difference_in_Meter_Reading_MT", Difference_in_Meter_Reading_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@QTY_supplied_MT", QTY_supplied_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@File_Name", File_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            adapter.SelectCommand.Parameters.AddWithValue("@Received_Date", Received_Date);
                            adapter.SelectCommand.Parameters.AddWithValue("@Created_Date", Created_Date);
                           // adapter.SelectCommand.Parameters.AddWithValue("@Modified_Date", Modified_Date);
                            
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }

                else if (sheetName == "tbl_BunkerFuelType")
                {

                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {

                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int Fuel_type_Id = Convert.ToInt32(tbls.Rows[i]["Fuel_type_Id"]);
                        decimal BDN = Convert.ToDecimal(tbls.Rows[i]["BDN"]);
                        decimal Fuel_Density = Convert.ToDecimal(tbls.Rows[i]["Fuel_Density"]);
                        decimal Sulphur_content = Convert.ToDecimal(tbls.Rows[i]["Sulphur_content"]);
                        string BDN_Number = (tbls.Rows[i]["BDN_Number"]).ToString();
                        int MaxR_Id = Convert.ToInt32(tbls.Rows[i]["MaxR_Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);

                        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertBunkerFuelType", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@Fuel_type_Id", Fuel_type_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@BDN", BDN);
                            adapter.SelectCommand.Parameters.AddWithValue("@Fuel_Density", Fuel_Density);
                            adapter.SelectCommand.Parameters.AddWithValue("@Sulphur_content", Sulphur_content);
                            adapter.SelectCommand.Parameters.AddWithValue("@BDN_Number", BDN_Number);
                            adapter.SelectCommand.Parameters.AddWithValue("@MaxR_Id", MaxR_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }

                else
                {

                    if (sheetName == "DailyNoonReport")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 92)
                        {
                            tbls.Columns.Add("BR_Extra_Run_Reason1", typeof(string)).DefaultValue = "N/A";
                            tbls.Columns.Add("BR_Extra_Run_Reason2", typeof(string)).DefaultValue = "N/A";

                            tbls.Columns.Add("BR_RungHrs_No1", typeof(decimal)).DefaultValue = 0;
                            tbls.Columns.Add("BR_RungHrs_No2", typeof(decimal)).DefaultValue = 0;
                            tbls.Columns.Add("AE_RungHrs_No4", typeof(string)).DefaultValue = 0;
                            tbls.Columns.Add("AE_Load_No4", typeof(string)).DefaultValue = 0;
                        }

                    }

                    if (sheetName == "LR_Cargo")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 19)
                        {
                            tbls.Columns.Add("EstCompDateTime", typeof(DateTime)).DefaultValue = DBNull.Value;
                         
                        }

                    }

                    if (sheetName == "DS_Cargo")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 21)
                        {
                            tbls.Columns.Add("EstCompDateTime", typeof(DateTime)).DefaultValue = DBNull.Value;

                        }

                    }

                    // Attachment-file sheets carry the chunked base64 for Bunker LabAnalysis and
                    // FreshWater attachments. They have no Update_{sheetName} stored proc, so the
                    // fallback below would throw a SQL exception and the catch would return the
                    // sheet name as an "error". Handle them inline (reconstruct the file from
                    // chunks and write to the static folder the email link points at) and return
                    // "" so the import succeeds silently.
                    if (sheetName.Equals("BunkerReport_Files", StringComparison.OrdinalIgnoreCase))
                    {
                        SaveImportAttachmentSheet(tbls, Server.MapPath("~/Bunker_LabAnalysisReport/"));
                        return "";
                    }
                    if (sheetName.Equals("FreshWaterReport_Files", StringComparison.OrdinalIgnoreCase))
                    {
                        SaveImportAttachmentSheet(tbls, Server.MapPath("~/FreshWaterReport/"));
                        return "";
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        using (SqlCommand cmd = new SqlCommand(string.Format("Update_{0}", sheetName)))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = connection;
                            cmd.Parameters.AddWithValue(string.Format("{0}TableType", sheetName), tbls);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                //return ex.Message.ToString();
                return sheetName;
            }
        }

        /// <summary>
        /// Reconstructs chunked attachment files from a BunkerReport_Files or
        /// FreshWaterReport_Files import sheet and writes them to <paramref name="targetFolderPath"/>.
        /// Groups rows by FileName, sorts chunks by PartIndex, concatenates base64, decodes,
        /// writes to disk. Same logic as getAttachment.aspx.cs uses for the email-driven import.
        /// </summary>
        private void SaveImportAttachmentSheet(System.Data.DataTable tbls, string targetFolderPath)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;

            string fileNameCol = null, dataCol = null, partCol = null;
            foreach (string c in new[] { "FileName", "File_Name", "Name" })
                if (tbls.Columns.Contains(c)) { fileNameCol = c; break; }
            foreach (string c in new[] { "FileData", "File_Data", "Data", "Base64" })
                if (tbls.Columns.Contains(c)) { dataCol = c; break; }
            foreach (string c in new[] { "PartIndex", "Part_Index", "Part", "ChunkIndex", "Index" })
                if (tbls.Columns.Contains(c)) { partCol = c; break; }

            if (fileNameCol == null || dataCol == null) return;

            if (!System.IO.Directory.Exists(targetFolderPath))
            {
                try { System.IO.Directory.CreateDirectory(targetFolderPath); }
                catch { return; }
            }

            var groups = new System.Collections.Generic.Dictionary<string,
                System.Collections.Generic.SortedDictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in tbls.Rows)
            {
                string fileName = row[fileNameCol]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(fileName)) continue;
                if (row[dataCol] == DBNull.Value || row[dataCol] == null) continue;
                string chunk = row[dataCol].ToString();
                if (string.IsNullOrEmpty(chunk)) continue;

                int partIndex = 0;
                if (partCol != null && row[partCol] != DBNull.Value && row[partCol] != null)
                    int.TryParse(row[partCol].ToString(), out partIndex);

                if (!groups.ContainsKey(fileName))
                    groups[fileName] = new System.Collections.Generic.SortedDictionary<int, string>();
                groups[fileName][partIndex] = chunk;
            }

            foreach (var kvp in groups)
            {
                try
                {
                    // Strip whitespace/line breaks that some exports insert every N chars,
                    // which would otherwise break Convert.FromBase64String.
                    var sb = new System.Text.StringBuilder();
                    foreach (var part in kvp.Value.Values)
                        sb.Append(part.Replace("\r", "").Replace("\n", "").Replace(" ", ""));

                    byte[] fileBytes = Convert.FromBase64String(sb.ToString());
                    string fullPath = System.IO.Path.Combine(targetFolderPath, kvp.Key);
                    System.IO.File.WriteAllBytes(fullPath, fileBytes);
                }
                catch { /* skip this file; continue with others */ }
            }
        }

        private async Task<DataTable> generateTable(string location,string sheetname)
        {
            //string filepath = @"D:\Sis_Nova_26102022_Export_9242156 (1).xlsx";
            //string sqlquery = "Select * From [Fuel_Cons_NR$]";
            string filepath = location;
            string sqlquery = "Select * From ["+ sheetname + "$]";

            string constring = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filepath + ";Extended Properties=\"Excel 12.0;HDR=YES;\"";
            using (OleDbConnection con = new OleDbConnection(constring + ""))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(sqlquery, con))
                {
                    using (DataSet ds = new DataSet())
                    {
                        da.Fill(ds);
                        return ds.Tables[0];
                    }
                }
            }
        }


        [HttpPost]
        public ActionResult _importAttachment(string path)
        {

            bool sucess = false;
            string msg = "";

            try
            {

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(new { Sucess = sucess, msg = msg });
        }

        [HttpPost]
        public ActionResult UploadZip(HttpPostedFileBase files)
        {

            bool success = false;
            string message = "";
            string zipFilePath = "";

            try
            {

               if (files != null && files.ContentLength > 0)
               {
                    if (Path.GetExtension(files.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        string F_path = Server.MapPath("~/Bunker_LabAnalysisReport");
                        if (!Directory.Exists(F_path))
                        {
                            Directory.CreateDirectory(F_path);
                        }

                        var path = Path.Combine(F_path, Path.GetFileName(files.FileName));

                        files.SaveAs(path);

                        zipFilePath = path;

                    }

                    string folderPath = Server.MapPath("~/Bunker_LabAnalysisReport");

                    if (System.IO.File.Exists(zipFilePath))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (IsSupportedExtension(entry.Name))
                                {
                                    string fileToExtract = Path.Combine(folderPath, entry.Name);
                                    entry.ExtractToFile(fileToExtract, true);
                                }
                            }
                        }
                    }

                    System.IO.File.Delete(zipFilePath);
                    success = true;
                    message = "Files Uploaded Sucessfully";
               }

            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message ;
            }

            return Json(new { success = success, message = message });
        }

        [HttpPost]
        public ActionResult UploadFreshWZip(HttpPostedFileBase files)
        {

            bool success = false;
            string message = "";
            string zipFilePath = "";

            try
            {

                if (files != null && files.ContentLength > 0)
                {
                    if (Path.GetExtension(files.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        string F_path = Server.MapPath("~/FreshWaterReport");
                        if (!Directory.Exists(F_path))
                        {
                            Directory.CreateDirectory(F_path);
                        }

                        var path = Path.Combine(F_path, Path.GetFileName(files.FileName));

                        files.SaveAs(path);

                        zipFilePath = path;

                    }

                    string folderPath = Server.MapPath("~/FreshWaterReport");

                    if (System.IO.File.Exists(zipFilePath))
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (IsSupportedExtension(entry.Name))
                                {
                                    string fileToExtract = Path.Combine(folderPath, entry.Name);
                                    entry.ExtractToFile(fileToExtract, true);
                                }
                            }
                        }
                    }

                    System.IO.File.Delete(zipFilePath);
                    success = true;
                    message = "Files Uploaded Sucessfully";
                }

            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return Json(new { success = success, message = message });
        }

        private bool IsSupportedExtension(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".doc", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".csv", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   ext.Equals(".png", StringComparison.OrdinalIgnoreCase);
        }


    }

}