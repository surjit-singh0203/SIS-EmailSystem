using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{

    [Authorize]
    [UserAuthenticationFilter]
    public class FreshWaterController : BaseController
    {
        // GET: Report/FreshWater
        public ActionResult Index()
        {
            FreshWaterReport FR = new FreshWaterReport();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            FR.Received_Date = DateTime.Now;
            // FR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            return View(FR);
        }

        public ActionResult FreshWaterReportList(int? pageNo, string firstVal)
        {
            FreshWaterReport FR = new FreshWaterReport();

            string vslid = string.Empty;
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            FR.FWReportList = CommonMethods.GetFreshWaterRList(vslid, currPage, pageSize, firstVal);
            return View(FR);

        }

        public ActionResult _FreshWaterReport(int? pageNo, string firstVal, string S_val, string Vessel)
        {
            FreshWaterReport FR = new FreshWaterReport();
            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);

            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            FR.FWReportList = CommonMethods.GetFreshWaterRList(vslid, currPage, pageSize, firstVal, S_val);
            ViewBag.search1 = firstVal;
            ViewBag.search2 = vslid;
            ViewBag.pageNo = pageNo;
            return PartialView("_FreshWaterReport", FR);

        }

        public ActionResult Edit(int id, int vslid)
        {
            FreshWaterReport FR = new FreshWaterReport();
            FR.FWReportList = CommonMethods.editFreshWaterRList(id, vslid, "FreshWaterReport");
            var FreshWaterRBind = FR.FWReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            var path = Path.Combine(Server.MapPath("~/FreshWaterReport"), FreshWaterRBind.File_Name);
            FreshWaterRBind.fileExtension = Path.GetExtension(FreshWaterRBind.File_Name);
            FreshWaterRBind.File_Path = path;

          //  int vslid = Convert.ToInt32(Session["VesselID"]);

            return View("Index", FreshWaterRBind);

        }

        [HttpPost]
        public ActionResult InsertFreshWaterReport(FreshWaterReport model)
        {
            bool success = false;
            string msg = string.Empty;

            try
            {
                //model.VesselId = Convert.ToInt32(Session["VesselID"]);
                if (model.Id != 0)
                {
                    model.FWReportList = CommonMethods.editFreshWaterRList(model.Id, model.VesselId,"FreshWaterReport");
                    var FreshWRBind = model.FWReportList.Where(x => x.Id == model.Id).FirstOrDefault(e => e.Id == model.Id);

                    if (model.FreshWater_File != null && model.FreshWater_File.ContentLength > 0)
                    {
                        if (model.FreshWater_File.ContentLength <= 200 * 1024)
                        {
                            string path = Server.MapPath("~/FreshWaterReport/" + FreshWRBind.File_Name);
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            string fileName = Path.GetFileNameWithoutExtension(model.FreshWater_File.FileName);
                            string extension = Path.GetExtension(model.FreshWater_File.FileName);
                            fileName = fileName + "_" + DateTime.Now.Ticks + extension;
                            string filePath = Path.Combine(Server.MapPath("~/FreshWaterReport"), fileName);
                            model.FreshWater_File.SaveAs(filePath);
                            model.File_Name = fileName;
                            CommonMethods.InsertUpdateFreshWaterReport(model);
                            TempData["success"] = "Record Update successfully";
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds 200KB. Please select a smaller file.";
                        }

                    }
                    else
                    {
                        model.File_Name = FreshWRBind.File_Name;
                        CommonMethods.InsertUpdateFreshWaterReport(model);
                        TempData["success"] = "Record Update successfully";
                    }
                }
                else
                {
                    if (model.FreshWater_File != null && model.FreshWater_File.ContentLength > 0)
                    {
                        if (model.FreshWater_File.ContentLength <= 200 * 1024)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(model.FreshWater_File.FileName);
                            string extension = Path.GetExtension(model.FreshWater_File.FileName);
                            fileName = fileName + "_" + DateTime.Now.Ticks + extension;
                            string filePath = Path.Combine(Server.MapPath("~/FreshWaterReport"), fileName);
                            model.FreshWater_File.SaveAs(filePath);
                            model.File_Name = fileName;
                            CommonMethods.InsertUpdateFreshWaterReport(model);
                            TempData["success"] = "Record saved successfully";
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds 200KB. Please select a smaller file.";
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction("FreshWaterReportList");
        }

        public ActionResult OpenPDF(string fileName, string fileExtension)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string path = Server.MapPath("~/FreshWaterReport/" + fileName + fileExtension);
            byte[] filedata = System.IO.File.ReadAllBytes(path);
            string contentType;

            switch (fileExtension.ToLower())
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                //case ".xls":
                //    contentType = "application/vnd.ms-excel";
                //    break;
                //case ".xlsx":
                //    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                //    break;
                //case ".csv":
                //    contentType = "text/csv";
                //    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                default:

                    contentType = "application/octet-stream";
                    break;
            }

            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName + fileExtension,
                Inline = true,
            };

            Response.Headers.Set("Content-Disposition", contentDisposition.ToString());

            return File(filedata, contentType);
        }

    }
}