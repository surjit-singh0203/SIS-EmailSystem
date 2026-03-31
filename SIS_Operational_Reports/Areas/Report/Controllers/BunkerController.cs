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
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.Report.Controllers
{
    public class BunkerController : Controller
    {
        // GET: Report/Bunker
        //public ActionResult Index()
        //{
        //    BunkerReport BR = new BunkerReport();
        //    int vslid = Convert.ToInt32(Session["VesselID"]);
        //    BR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);
        //    return View(BR);
        //}


        public ActionResult Add1() 
        {
            BunkerReport BR = new BunkerReport();
            //BR.BunkerHoseConnected = DateTime.Now;
            //BR.CommencedBunkering = DateTime.Now;
            //BR.BunkeringCompleted = DateTime.Now;
            //BR.BunkerHosedisconnected = DateTime.Now;
            //BR.BargeCastOff = DateTime.Now;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            BR.VoyageNumberList = CommonMethods.GetVoyageList(vslid);


            return View(BR);
        }

        public ActionResult BunkerReportList(int? pageNo, string firstVal)
        {
            BunkerReport BR = new BunkerReport();
            // BR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            // string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            string vslid = string.Empty;
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            BR.BunkerReportList = CommonMethods.GetBunkerRList(vslid, currPage, pageSize, firstVal);
            return View(BR);

        }

        public ActionResult _BunkerReport(int? pageNo, string firstVal)
        {
            BunkerReport BR = new BunkerReport();
            // BR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            //  string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            string vslid = string.Empty;
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            BR.BunkerReportList = CommonMethods.GetBunkerRList(vslid, currPage, pageSize, firstVal);
            ViewBag.search1 = firstVal;
            ViewBag.search2 = vslid;
            ViewBag.pageNo = pageNo;
            return PartialView("_BunkerReport", BR);

        }
     

        [HttpPost]
        public ActionResult InsertBunkerReport(BunkerReport model )
        {
            bool success = false;
            string msg = string.Empty;

            try
            {
                model.VesselId = Convert.ToInt32(Session["VesselID"]);
                if (model.Id != 0)
                {
                    model.BunkerReportList = CommonMethods.editBunkerRList(model.Id, "BunkerReport");
                    var BunkerRBind = model.BunkerReportList.Where(x => x.Id == model.Id).FirstOrDefault(e => e.Id == model.Id);

                    if (model.LabAnalysisReport != null && model.LabAnalysisReport.ContentLength > 0)
                    {
                        if (model.LabAnalysisReport.ContentLength <= 200 * 1024) // 200KB
                        {
                            string path = Server.MapPath("~/Bunker_LabAnalysisReport/files/" + BunkerRBind.LabAnalysisReport_Name);
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            string fileName = Path.GetFileNameWithoutExtension(model.LabAnalysisReport.FileName);
                            string extension = Path.GetExtension(model.LabAnalysisReport.FileName);
                            fileName = fileName + "_" + DateTime.Now.Ticks + extension;
                            // fileName = Path.GetFileName(model.LabAnalysisReport.FileName + "_" + DateTime.Now.Ticks);
                            string filePath = Path.Combine(Server.MapPath("~/Bunker_LabAnalysisReport/files"), fileName);
                            model.LabAnalysisReport.SaveAs(filePath);
                            model.LabAnalysisReport_Name = fileName ;
                            CommonMethods.InsertUpdateBunkerReport(model);
                            TempData["success"] = "Record Update successfully";
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds 200KB. Please select a smaller file.";
                        }
                        
                    }
                    else
                    {
                        model.LabAnalysisReport_Name = BunkerRBind.LabAnalysisReport_Name;
                        CommonMethods.InsertUpdateBunkerReport(model);
                        TempData["success"] = "Record Update successfully";
                    }
                }
                else
                {
                    if (model.LabAnalysisReport != null && model.LabAnalysisReport.ContentLength > 0)
                    {
                        if (model.LabAnalysisReport.ContentLength <= 200 * 1024) // 200KB
                        {
                            string fileName = Path.GetFileNameWithoutExtension(model.LabAnalysisReport.FileName);
                            string extension = Path.GetExtension(model.LabAnalysisReport.FileName);
                            fileName = fileName + "_" + DateTime.Now.Ticks + extension;
                            // string fileName = Path.GetFileName(model.LabAnalysisReport.FileName + "_" + DateTime.Now.Ticks);
                            string filePath = Path.Combine(Server.MapPath("~/Bunker_LabAnalysisReport/files"), fileName );
                            model.LabAnalysisReport.SaveAs(filePath);
                            model.LabAnalysisReport_Name = fileName ;
                            CommonMethods.InsertUpdateBunkerReport(model);
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

            return RedirectToAction("BunkerReportList");
        }

        public ActionResult Edit(int id)
        {
            BunkerReport BR = new BunkerReport();
            BR.BunkerReportList = CommonMethods.editBunkerRList(id, "BunkerReport");
            var BunkerRBind = BR.BunkerReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
            var path = Path.Combine(Server.MapPath("~/Bunker_LabAnalysisReport/files"), BunkerRBind.LabAnalysisReport_Name);
            BunkerRBind.fileExtension = Path.GetExtension(BunkerRBind.LabAnalysisReport_Name);
            BunkerRBind.File_Path = path;
            int vslid = Convert.ToInt32(Session["VesselID"]);
            BunkerRBind.B_FuelLists = CommonMethods.editBunkerFuelList(id, vslid.ToString(), "BunkerFReport");
            foreach (var item in BunkerRBind.B_FuelLists)
            {
                if (item.Fuel_type_Id == 5)
                {
                    item.Fuel_type = "VLSFO";
                }
                if (item.Fuel_type_Id == 2)
                {
                    item.Fuel_type = "MDO";
                }
            }

            //Session["BR_ID"] = id;

            BunkerRBind.VoyageNumberList = CommonMethods.GetVoyageList(vslid);

            return View("Add1", BunkerRBind);

        }

        public ActionResult Delete(int id)
        {
            bool success = false;
            try
            {
               BunkerReport BR = new BunkerReport();
               BR.BunkerReportList = CommonMethods.editBunkerRList(id, "BunkerReport");
               var BunkerRBind = BR.BunkerReportList.Where(x => x.Id == id).FirstOrDefault(e => e.Id == id);
               string path = Server.MapPath("~/Bunker_LabAnalysisReport/files/" + BunkerRBind.LabAnalysisReport_Name + "");
               if (System.IO.File.Exists(path))
               {
                   System.IO.File.Delete(path);
                   success = true;
                   if (success == true)
                   {
                       CommonMethods.CommonDelete(id, "BunkerReport");
                       TempData["Success"] = "Record deleted successfully";
                   }
                   
               }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }


            return RedirectToAction("BunkerReportList");
        }

        [HttpPost]
        public ActionResult InsertBunkerFuelType(string BunkerFuelTypeList)
        {
           // string BR_ID = "";
            int vslid = Convert.ToInt32(Session["VesselID"]);
            //if (Session["BR_ID"] == null)
            //{
            //    BR_ID = "";
            //}
            //else
            //{
            //    BR_ID = Session["BR_ID"].ToString();
            //}

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var Json = JsonConvert.DeserializeObject<List<BukerFuelList>>(BunkerFuelTypeList, settings);
            
            foreach (var rootObject in Json)
            {
              
                rootObject.VesselId = vslid;
                if (rootObject.Fuel_type == "VLSFO" || rootObject.Fuel_type == "5")
                {
                    rootObject.Fuel_type_Id = 5;
                }
                if (rootObject.Fuel_type == "MDO" || rootObject.Fuel_type == "2")
                {
                    rootObject.Fuel_type_Id = 2;
                }

                if (rootObject.MaxR_Id == 0)
                {

                    using (SqlDataAdapter adp = new SqlDataAdapter("select MAX(Id) from BunkerReport", ConnectionBulder.con))
                    {
                        DataTable dtm = new DataTable();
                        adp.Fill(dtm);
                        if (dtm.Rows.Count > 0)
                        {
                            rootObject.MaxR_Id = Convert.ToInt32(dtm.Rows[0][0]);
                        }
                    }
                }
                //else
                //{
                //    rootObject.MaxR_Id = rootObject.MaxR_Id;
                //}

                //if (BR_ID == "")
                //{
                if (rootObject.Fuel_type_Id != null)
                {
                    CommonMethods.InsertUpdateBunkerFuelType(rootObject/*, "Insert"*/);
                }
                   
                //}
                //if (BR_ID != "")
                //{
                //    CommonMethods.InsertUpdateBunkerFuelType(rootObject, "Update");
                //}

            }

            return View();
        }


        [HttpPost]
        public ActionResult DeleteBunkerFuelType(string Id)
        {

            bool result = false;
            string msg = "";
            try
            {
                CommonMethods.CommonDelete(Convert.ToInt32(Id), "DBunkerFReport");
                result = true;

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                throw;
            }
            return Json(new {sucess = result, msge = msg });
        }

        
        public ActionResult OpenPDF(string fileName, string fileExtension)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string path = Server.MapPath("~/Bunker_LabAnalysisReport/files/" + fileName + fileExtension);
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
                case ".xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".csv":
                    contentType = "text/csv";
                    break;
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