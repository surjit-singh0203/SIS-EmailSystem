using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.CharterParty.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class VoyageController : BaseController
    {
        VoyageClass VC = new VoyageClass();
        // GET: CharterParty/Voyage
        public async Task<ActionResult> Index(int? pageNo, string firstVal, string dateF, string dateT)
        {
            VoyageClass Vc = new VoyageClass();
            int vslid = Convert.ToInt32(Session["VesselID"]);
           // Vc.GetVoyageList = CommonMethods.GetVoyageList(vslid);
            Vc.GetVoyageLegList = CommonMethods.GetVoyageLegList(vslid);
            Vc.GetFuelCList = CommonMethods.GetFuelTList(vslid);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);

            StaticHelper.VesselName = CommonMethods.GetVesselName(vslid);

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                Vc.GetVoyageList = CommonMethods.GetVoyageList(vslid, currPage, pageSize);
                return View(Vc);
            }
            else if (firstVal == "" && dateF == "")
            {
                Vc.GetVoyageList = CommonMethods.GetVoyageList(vslid, currPage, pageSize);
                return View(Vc);
            }
            else if (firstVal != null)
            {
                Vc.GetVoyageList = CommonMethods.SearchVoyageDList(vslid, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchVoyage", Vc);
            }


            //arrR.GetArrivalRList = CommonMethods.GetArrivalReportList(vslid, currPage, pageSize);
            //var ss = dnR.GetNoonRList.Where(x => x.TotalCount == dnR.TotalCount).SingleOrDefault();



            return View(Vc);
        }
        public ActionResult ViewVoyageDetail(int id)
        {
            VoyageClass Vc = new VoyageClass();
            Vc.VoyageId = id;

            Session["VovgID"]=id;
           //vd.GetVoyageList = CommonMethods.edtiVoyageList(id);
            Vc.GetVoyageList = CommonMethods.edtiVoyageList(id);
            Vc.GetVoyageLegList = CommonMethods.viewVoyageLegList(id); 
            Vc.GetFuelCList = CommonMethods.edtiFuelCList(id);
            return View(Vc);
        }
        public ActionResult addvoyagedetail()
        {
           
            int vslid = Convert.ToInt32(Session["VesselID"]);
           VC.CPList = CommonMethods. CharterPList(vslid);
            ViewBag.VesselName = CommonMethods.GetVesselName(vslid);
            return View(VC);
        }

        [HttpPost]
        public ActionResult addvoyagedetail(VoyageClass cls)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            if (cls.Id == 0)
            {
               
                cls.VesselId = vslid;

                int checkExistense = CommonMethods.CheckvoyageExistence(cls, 0, cls.Id, "Update");
                if (checkExistense == 0)
                {
                    CommonMethods.InsertUpdateVoyage(cls, "Insert");
                    TempData["Success"] = "Record saved successfully";
                    int maxid = CommonMethods.GetMaxSortingID(0, "Voyage");
                    //return RedirectToAction("index");
                    return RedirectToAction("ViewVoyageDetail/"+ maxid + "");
                    
                }
                else
                {
                    TempData["Error"] = "Voyage number already exists ! ";
                    VC.CPList = CommonMethods.CharterPList(vslid);
                    return RedirectToAction("addvoyagedetail");
               
                }
            }
            else
            {
                int checkExistense = CommonMethods.CheckvoyageExistence(cls, 0, cls.Id, "Update");
                if (checkExistense == 0)
                {
                    CommonMethods.InsertUpdateVoyage(cls, "Update");
                    TempData["Success"] = "Record updated successfully ";

                    return RedirectToAction("index");
                }
                else
                {
                    TempData["Error"] = "Voyage number already exists ! ";
                    VC.CPList = CommonMethods.CharterPList(vslid);
                    return RedirectToAction("addvoyagedetail");
                }

            }
            //int maxid = CommonMethods.GetMaxSortingID(0, "Voyage");
            //return RedirectToAction("insertleg", new { id = maxid });
        }
        public ActionResult insertleg(int vid)
        {
            int vslid = Convert.ToInt32(Session["VesselID"]);
            VoyageClass VC = new VoyageClass();
            VC.VoyageId = vid;
            VC.SpeedStatusList = speedSList(vslid);
            TempData["VoyageId"] = vid;
            return View(VC);
        }
        public static List<SpeedStatus> speedSList(int VslId)
        {

            List<SpeedStatus> ftype = new List<SpeedStatus>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct a.id, a.speed from CPpart2 a inner join CPpart1 b on a.cpid=b.CPId where b.VesselID='"+VslId+"'", ConnectionBulder.con))
            {
               // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
               // adp.SelectCommand.Parameters.AddWithValue("@Action", "SpeedStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new SpeedStatus
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Speed = dt.Rows[i]["Speed"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public JsonResult insertlegD(List<VoyageClass> leg)
        {
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(leg);


            var vid = (int)TempData["VoyageId"];
            VoyageClass cls = new VoyageClass();
            int vslid = Convert.ToInt32(Session["VesselID"]);
           
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cls.LegPort_A = dt.Rows[i]["LegPort_A"].ToString();
                cls.ReasonforPortCall_A = dt.Rows[i]["ReasonforPortCall_A"].ToString();
                cls.LegPort_B = dt.Rows[i]["LegPort_B"].ToString();
                cls.ReasonforPortCall_B = dt.Rows[i]["ReasonforPortCall_B"].ToString();

                cls.DTG = Convert.ToDecimal(dt.Rows[i]["DTG"]);
                cls.CP_SOG = Convert.ToDecimal(dt.Rows[i]["CP_SOG"]);
                cls.CP_Log_Speed = Convert.ToDecimal(dt.Rows[i]["CP_Log_Speed"]);
                cls.VoyageId = vid;
                cls.VesselId = vslid;

                CommonMethods.InsertUpdateVoyageLeg(cls, "Insert");

                TempData["Success"] = "Record saved successfully";
            }
            //int insertedRecords = dt.Rows.Count;
            //int insertedRecords = vid;

            return Json(new { Success = true, Data = "ViewVoyageDetail/" + vid });
        }
        public ActionResult addfuel(int vid)
        {
            VoyageClass VC = new VoyageClass();
            VC.VoyageId = vid;
            TempData["VoyageId"] = vid;
            return View(VC);
        }

        public JsonResult insertfuel(List<VoyageClass> fuel)
        {
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(fuel);


            var vid = (int)TempData["VoyageId"];
            VoyageClass cls = new VoyageClass();
            int vslid = Convert.ToInt32(Session["VesselID"]);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cls.FuelT = dt.Rows[i]["FuelT"].ToString();
                cls.CP_Consumption = dt.Rows[i]["CP_Consumption"].ToString();
                cls.VoyageId = Convert.ToInt32(dt.Rows[i]["Id"]);
                cls.VesselId = vslid;

                CommonMethods.InsertUpdateFuel(cls, "Insert");

                TempData["Success"] = "Record saved successfully";
            }
            //int insertedRecords = dt.Rows.Count;
          //  int insertedRecords = vid;

            return Json(new { Success = true, Data = "ViewVoyageDetail/" + vid });
        }

        public ActionResult editvoyage(int id)
        {
            VoyageClass vd = new VoyageClass();
            vd.GetVoyageList = CommonMethods.edtiVoyageList(id);
            vd.Id = id;
            vd.VoyageNumber = vd.GetVoyageList.Select(x => x.VoyageNumber).SingleOrDefault();
            vd.Nor_Conditions = vd.GetVoyageList.Select(x => x.Nor_Conditions).SingleOrDefault();

            int vslid = Convert.ToInt32(Session["VesselID"]);
            vd.CPList = CommonMethods.CharterPList(vslid);
            //vd.CPList = vd.GetVoyageList.Select(x => x.CPList).SingleOrDefault();

            vd.CPId= vd.GetVoyageList.Select(x => x.CPId).SingleOrDefault();
            vd.HeavyWeather_BSS = vd.GetVoyageList.Select(x => x.HeavyWeather_BSS).SingleOrDefault();
            vd.HeavyWeather_WH = vd.GetVoyageList.Select(x => x.HeavyWeather_WH).SingleOrDefault();
            vd.HeavyWeather_CV = vd.GetVoyageList.Select(x => x.HeavyWeather_CV).SingleOrDefault();

            //vd.CPList = vd.CPList.Select(x => x.CPNo).SingleOrDefault();

            //vd.VoyageStartPoint = vd.GetVoyageList.Select(x => x.VoyageStartPoint).SingleOrDefault();
            //vd.ETA = vd.GetVoyageList.Select(x => x.ETA).SingleOrDefault();
            //vd.DTG = vd.GetVoyageList.Select(x => x.DTG).SingleOrDefault();
            //vd.CP_SOG = vd.GetVoyageList.Select(x => x.CP_SOG).SingleOrDefault();
            //vd.CP_Log_Speed = vd.GetVoyageList.Select(x => x.CP_Log_Speed).SingleOrDefault();
            //vd.FW = vd.GetVoyageList.Select(x => x.FW).SingleOrDefault();

            // vd.HeavyWeather_BSS = vd.GetVoyageList.Select(x => x.HeavyWeather_BSS).SingleOrDefault();
            // vd.HeavyWeather_WH = vd.GetVoyageList.Select(x => x.FW).SingleOrDefault();
            // vd.HeavyWeather_CV = vd.GetVoyageList.Select(x => x.HeavyWeather_CV).SingleOrDefault();
            return View("addvoyagedetail", vd);

        }

        public ActionResult editvoyageleg(int id)
        {
            VoyageClass vd = new VoyageClass();
            vd.GetVoyageList = CommonMethods.edtiVoyageLegList(id);
            vd.Id = id;
            vd.LegPort_A = vd.GetVoyageList.Select(x => x.LegPort_A).SingleOrDefault();
            vd.LegPort_B = vd.GetVoyageList.Select(x => x.LegPort_B).SingleOrDefault();
            vd.ReasonforPortCall_A = vd.GetVoyageList.Select(x => x.ReasonforPortCall_A).SingleOrDefault();
            vd.ReasonforPortCall_B = vd.GetVoyageList.Select(x => x.ReasonforPortCall_B).SingleOrDefault();
            vd.DTG = vd.GetVoyageList.Select(x => x.DTG).SingleOrDefault();
            vd.CP_SOG = vd.GetVoyageList.Select(x => x.CP_SOG).SingleOrDefault();
            vd.CP_Log_Speed = vd.GetVoyageList.Select(x => x.CP_Log_Speed).SingleOrDefault();
            return View("editvoyageleg", vd);

        }
        [HttpPost]
        public ActionResult editvoyageleg(VoyageClass cls)
            {
            cls.LegPort_A = cls.LegPort_A;
            cls.LegPort_B = cls.LegPort_B;
            cls.ReasonforPortCall_A = cls.ReasonforPortCall_A;
            cls.ReasonforPortCall_B = cls.ReasonforPortCall_B;

            CommonMethods.InsertUpdateVoyageLeg(cls, "Update");

            int vgid = Convert.ToInt32(Session["VovgID"]);
            return RedirectToAction("ViewVoyageDetail/" + vgid + "");
            //return RedirectToAction("index");
        }

        public ActionResult editfuelC(int id)
        {
            VoyageClass vd = new VoyageClass();
            vd.GetVoyageList = CommonMethods.edtiFuelCList(id);
            vd.Id = id;
            vd.FuelTypeId = vd.GetVoyageList.Select(x => x.FuelTypeId).SingleOrDefault();
            vd.CP_Consumption = vd.GetVoyageList.Select(x => x.CP_Consumption).SingleOrDefault();
           
            return View("editfuelC", vd);

        }
        [HttpPost]
        public ActionResult editfuelC(VoyageClass cls)
        {
            cls.FuelTypeId = cls.FuelTypeId;
            cls.CP_Consumption = cls.CP_Consumption;

            string ss = cls.FuelTypeId.ToString();
            cls.FuelT = ss;
            
            CommonMethods.InsertUpdateFuel(cls, "Update");

            return RedirectToAction("index");
        }

        public ActionResult deletevoyage(int id)
        {
            int delete = 0;
            using (SqlDataAdapter adp=new SqlDataAdapter ("select count(*) from DailyNoonReport where VoyageId="+ id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if(count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from ArrivalReport where VoyageId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from BerthingReport where VoyageId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from DepartureReport where VoyageId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from DischargingReport where VoyageId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1; 
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from LoadingReport where VoyageId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1; 
                }
            }

            if (delete == 0)
            {
                CommonMethods.CommonDelete(id, "VoyageDetail");
                TempData["Success"] = "Record deleted successfully";
            }else
            {
                TempData["Error"] = "Voyage used in reports.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult deletevoyageleg(int id)
        {
            int delete = 0;
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from DailyNoonReport where LegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from ArrivalReport where LegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from BerthingReport where LegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from DepartureReport where DepLegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from DischargingReport where LegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select count(*) from LoadingReport where LegPortId=" + id + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                int count = Convert.ToInt32(dt.Rows[0][0]);
                if (count != 0)
                {
                    delete = 1;
                }
            }

            if (delete == 0)
            {

                CommonMethods.CommonDelete(id, "Voyageleg");
                TempData["Success"] = "Record deleted successfully";
            }
            else
            {
                TempData["Error"] = "Leg used in reports.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult deletefuelC(int id)
        {
            CommonMethods.CommonDelete(id, "FuelC");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("Index");
        }


    }
}