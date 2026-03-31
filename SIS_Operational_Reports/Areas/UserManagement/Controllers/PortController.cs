using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataBuildingLayer;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using SIS_Operational_Reports.Common;
using SIS_Operational_Reports.Models;
using Microsoft.AspNet.Identity;
using System.Configuration;
using ClosedXML.Excel;
using System.IO;
using System.Threading;

namespace SIS_Operational_Reports.Areas.UserManagement.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class PortController : BaseController
    {
       
      
        public ActionResult Index(int? pageNo, string firstVal)
        {
            PortListClass mode = new PortListClass();


            mode. PortList = CommonClass.portList();
            //mode.GetPortList = CommonMethods.GetPortList();


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            //int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            int pageSize = 20;

            TempData["CurrentPage"] = currPage;



            if (firstVal == null)
            {
                mode.GetPortList = CommonMethods.GetPortList(currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "")
            {
                mode.GetPortList = CommonMethods.GetPortList(currPage, pageSize);
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetPortList = CommonMethods.SearchPortList(firstVal);
                return PartialView("_searchPort", mode);
            }



            return View(mode);
           
           
          

            
        }
        public class City
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        //public class CargoGradeClass
        //{
        //    public int Id { get; set; }
        //    public string CargoGrade { get; set; }

        //    public List<CargoGradeClass> GetCargoGradeList { get; set; }
        //}

        public ActionResult cargoGradeList(int? pageNo, string firstVal)
        {
            CargoGradeClass mode = new CargoGradeClass();


            //mode.PortList = CommonClass.portList();
            //mode.GetPortList = CommonMethods.GetPortList();


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            //int pageSize = 20;

            TempData["CurrentPage"] = currPage;


            mode.GetCargoGradeList = CommonMethods.GetCargoGradeList(currPage, pageSize);
             return View(mode);


            //if (firstVal == null)
            //{
            //    mode.GetPortList = CommonMethods.GetPortList(currPage, pageSize);
            //    return View(mode);
            //}
            //else if (firstVal == "")
            //{
            //    mode.GetPortList = CommonMethods.GetPortList(currPage, pageSize);
            //    return View(mode);
            //}
            //else if (firstVal != null)
            //{
            //    mode.GetPortList = CommonMethods.SearchPortList(firstVal);
            //    return PartialView("_searchPort", mode);
            //}



            return View(mode);





        }

        [HttpPost]
        public JsonResult AutoPort(string Prefix)
        {
            List<City> ObjList = new List<City>()
            {

                new City { Id = 1, Name = "Latur" },  
                new City { Id = 2, Name = "Mumbai" },  
                new City { Id = 3, Name = "Pune" },  
                new City { Id = 4, Name = "Delhi" },  
                new City { Id = 5, Name = "Dehradun" },  
                new City { Id = 6, Name = "Noida" },  
                new City { Id = 7, Name = "New Delhi" }


        };
            //Searching records from list using LINQ query  
            var Name = (from N in ObjList
                        where N.Name.StartsWith(Prefix)
                        select new { N.Name });
            return Json(Name, JsonRequestBehavior.AllowGet);



            //DataTable dt = new DataTable();
            //using (SqlDataAdapter adp=new SqlDataAdapter ("select top (20) portname from PortList", ConnectionBulder.con))
            //{               
            //    adp.Fill(dt);
            //}
            //List<PortListClass> prtlist = new List<PortListClass>();
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    PortListClass prt = new PortListClass();               
            //    prt.PortName = dt.Rows[i]["PortName"].ToString();
            //    prtlist.Add(prt);
            //}
            ////Searching records from list using LINQ query  
            //var PortName = (from N in prtlist
            //            where N.PortName.StartsWith(Prefix)
            //            select new { N.PortName });
            //return Json(PortName, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddCargoGrade()
        {
            CargoGradeClass ud = new CargoGradeClass();
            return View(ud);
        }

        [HttpPost]
        public ActionResult AddCargoGrade(CargoGradeClass user)
        {
            if (user.Id == 0)
            {
                int countchk = 0;
                using (SqlDataAdapter adp = new SqlDataAdapter("SELECT *   FROM tblCargoGrades   WHERE Cargo = '" + user.Cargo + "' ", ConnectionBulder.con))
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
                    CommonMethods.InsertCargoGrade(user, "Insert");
                    TempData["Success"] = "Record saved successfully";
                    return RedirectToAction("cargoGradeList");
                }
                else
                {
                    TempData["Error"] = "Cargo name already exists !";
                    return View("AddCargoGrade", user);
                }
            }
            else
            {
                CommonMethods.InsertCargoGrade(user, "Update");
                TempData["Success"] = "Record update successfully";
                return RedirectToAction("cargoGradeList");
            }
        }

        public ActionResult AddPort()
        {
            PortListClass ud = new PortListClass();
            return View(ud);
        }
        [HttpPost]
        public ActionResult AddPort(PortListClass user)
        {
            if (user.Id == 0)
            {
                int countchk = 0;
                using (SqlDataAdapter adp = new SqlDataAdapter("SELECT *   FROM PortList   WHERE PortName = '" + user.PortName + "' and FacilityName = '" + user.FacilityName + "'", ConnectionBulder.con))
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
                    CommonMethods.InsertPort(user,"Insert");
                    TempData["Success"] = "Record saved successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Port and facility name already exists !";
                    return View("AddPort", user);
                }
            }
            else
            {
                CommonMethods.InsertPort(user,"Update");
                TempData["Success"] = "Record update successfully";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Edit(int id)
        {
            PortListClass vd = new PortListClass();
            vd.GetPortList = CommonMethods.edtiPortList(id);
            vd.Id = id;
            vd.PortName = vd.GetPortList.Select(x => x.PortName).SingleOrDefault();
            vd.FacilityName = vd.GetPortList.Select(x => x.FacilityName).SingleOrDefault();
            vd.CountryCode = vd.GetPortList.Select(x => x.CountryCode).SingleOrDefault();
            vd.CountryName = vd.GetPortList.Select(x => x.CountryName).SingleOrDefault();
            vd.IMOPortFacilityNumber = vd.GetPortList.Select(x => x.IMOPortFacilityNumber).SingleOrDefault();
            vd.Longitude = vd.GetPortList.Select(x => x.Longitude).SingleOrDefault();
            vd.Latitude = vd.GetPortList.Select(x => x.Latitude).SingleOrDefault();
            return View("AddPort", vd);

        }
        public ActionResult EditCargo(int id)
        {
            CargoGradeClass vd = new CargoGradeClass();
            vd.GetCargoGradeList = CommonMethods.edtiCargoList(id);
            vd.Id = id;
            vd.Cargo = vd.GetCargoGradeList.Select(x => x.Cargo).SingleOrDefault();
            
            return View("AddCargoGrade", vd);

        }


        public ActionResult DeleteCargo(int id)
        {

            CommonMethods.CommonDelete(id, "CargoList");
            TempData["Success"] = "Record deleted successfully";


            return RedirectToAction("cargoGradeList");
        }
        public ActionResult Delete(int id)
        {

            CommonMethods.CommonDelete(id, "PortList");
            TempData["Success"] = "Record deleted successfully";


            return RedirectToAction("Index");
        }
        public ActionResult ExportPort(PortListClass user)
        {
            user.PortIDs = user.PortIDs.Distinct().ToList();
            if (user.PortIDs.Count > 0)
            {
                foreach (string id in user.PortIDs)
                    user.ExportedPortName = string.Format("{0},{1}", id, user.ExportedPortName);
            }
            user.ExportedPortName = user.ExportedPortName.Trim(',');
            DataSet ds = CommonMethods.ExportPortList(user.ExportedPortName);
            using (XLWorkbook wb = new XLWorkbook())
            {              
                int i = 0;
                foreach (DataTable table in ds.Tables)
                {                  
                    if (i == 0)
                        table.TableName = "PortList";                   
                   
                    if (table.Rows.Count > 0)
                    {
                        var protectedsheet = wb.Worksheets.Add(table);

                        var projection = protectedsheet.Protect("49WEB$TREET#");
                        projection.InsertColumns = true;
                        projection.InsertRows = true;
                    }
                    i++;
                }

           

                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;

                DateTime today = DateTime.Today;
                //string vsname = searchTerm.Replace(" ", "");
                //string HeaderName = "Work-Ship_Export_" + vsname + "_" + today.ToString("dd-MMM-yyyy");

                Response.Clear();
                Response.BufferOutput = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=Sis_Nova_PortList" + DateTime.Now.ToString("ddMMyyyy") + "_OfficeExport.xlsx");

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



            return RedirectToAction("Index");
        }




    }


    
}