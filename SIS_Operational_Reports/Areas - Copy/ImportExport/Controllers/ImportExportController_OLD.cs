using ClosedXML.Excel;
using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            ExportPeram2 epm = new ExportPeram2();
            epm.VesselList = CommonClass.GetVesselList();
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
        public async Task<ActionResult> Index(string submit, ExportPeram2 peram, HttpPostedFileBase photo, FormCollection collection)
        {
            //ExportPeram epm = new ExportPeram();
            //epm.VoyageId = peram.VoyageId;

            if (submit == "Export")
            {

                DataSet ds = CommonMethods.ExportSettingTables(peram);
               
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

                                        
                                        var getErrorsheet = InsertImportedData(sheetName, dtx);
                                        if (!string.IsNullOrEmpty(getErrorsheet))
                                        {
                                            ErrorINimportedTables += getErrorsheet + ", ";
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(ErrorINimportedTables))
                                {
                                    TempData["Success"] = "Data has been Impored Successfully";
                                }
                                else
                                {
                                    TempData["Error"] = "Data has been impored but except following Sheets:- " + Environment.NewLine + ErrorINimportedTables;
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

                                   // InsertImportedData(sheetName, dtx);
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


        private string InsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {

                string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);



                if (sheetName == "VoyageLeg")
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
                else
                {


                    // if (!destinationTableName.Equals("VersionTable", StringComparison.OrdinalIgnoreCase))
                    //if (sheetName == "LoadingReport")
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
                return sheetName;

            }


        }
    }

}