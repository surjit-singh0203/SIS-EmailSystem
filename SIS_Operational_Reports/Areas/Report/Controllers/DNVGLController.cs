using ClosedXML.Excel;
using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    public class DNVGLController : Controller
    {
        // GET: Report/DNVGL
        public ActionResult Index()
        {
            DNVGLReport DNVGL = new DNVGLReport();
            DNVGL.VesselList = CommonClass.GetVesselList();

          //  DNVGL.VesselList = DNVGL.VesselList.Where(x => vslIds.Contains(x.)).ToList();
            return View(DNVGL);
        }


        public ActionResult _DNVGLReport(int? year, string Ship_CD)
        {
            DNVGLReport DNVGL = new DNVGLReport();
            if (!string.IsNullOrEmpty(Ship_CD))
            {
                string[] vslIds = Ship_CD.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim()).ToArray();
                DNVGL.VesselList = CommonClass.GetVesselList();

                DNVGL.VesselList = DNVGL.VesselList.Where(x => vslIds.Contains(x.Value)).ToList();
            }

            return View(DNVGL);
        }

        public ActionResult DNVGLReport(int? year, string Ship_CD)
        {

            DataSet ds = CommonMethods.ExportDNVGL(Ship_CD);
            DataTable myTable = ds.Tables[0];
            
            if (myTable.Columns.Count > 0)
            {
                myTable.Columns.RemoveAt(0);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                
                int i = 0;
              //  foreach (DataTable table in ds.Tables)
              //  {

                 if (i == 0)
                 myTable.TableName = "DNVGLReport";             

                 if (myTable.Rows.Count > 0)
                 {
                     var protectedsheet = wb.Worksheets.Add(myTable);

                     var projection = protectedsheet.Protect("49WEB$TREET#");
                     projection.InsertColumns = true;
                     projection.InsertRows = true;
                 }

                 i++;

               // }


                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;

                DateTime today = DateTime.Today;

                Response.Clear();
                Response.BufferOutput = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=DNVGLReport" + DateTime.Now.ToString("ddMMyyyy")+ ".xlsx");

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
    }
}