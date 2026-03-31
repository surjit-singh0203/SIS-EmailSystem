using DataBuildingLayer;
using Newtonsoft.Json;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.VesselParticulars.Controllers
{
    [Authorize]
    public class PumpController : Controller
    {
        PumpClass pmp = new PumpClass();
        // GET: VesselParticulars/Pump
        public ActionResult Index(int? ImoNo)
        {
            PumpClass pumps = new PumpClass();

           // ViewBag.ImoNo = ImoNo;

            Session["ImoNo"]= ImoNo;
            ViewBag.VesselName = CommonMethods.GetVesselName(ImoNo);
            pumps.GetPumpList = CommonMethods.GetPumpList(ImoNo);
            return View(pumps);
        }

        public ActionResult addpump()
        {
            //PumpClass pmp = new PumpClass();
            return View(pmp);
        }

        public ActionResult pumpandtank(int? pageNo, string firstVal, string dateF, string dateT)
        {
            VesselDetail mode = new VesselDetail();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["pageSize"]);


            TempData["CurrentPage"] = currPage;


            if (firstVal == null)
            {
                mode.GetVesselList = CommonMethods.GetVesselList(currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "")
            {
                mode.GetVesselList = CommonMethods.GetVesselList(currPage, pageSize);
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetVesselList = CommonMethods.SearchVesselPartList(0, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_pandtank", mode);
            }



            // mode.GetVesselList = CommonMethods.GetVesselList( currPage, pageSize);
            return View(mode);
        }
        [HttpPost]
        public ActionResult addpump(PumpClass cls)
        {
            if (cls.Id == 0)
            {
                cls.VesselId =Convert.ToInt32( Session["ImoNo"]);
                int checkExistense = CommonMethods.CheckpumpExistence(cls, 0, 0, "Insert");
                if (checkExistense == 0)
                {
                    
                    CommonMethods.InsertUpdatePumps(cls, "Insert");
                    TempData["Success"] = "Record saved successfully";
                }
                else
                {
                    TempData["Error"] = "Pump Name already exists ! ";
                    return View(pmp);
                }
            }
            else
            {
                cls.VesselId = Convert.ToInt32(Session["ImoNo"]);
                int checkExistense = CommonMethods.CheckpumpExistence(cls, 0, cls.Id, "Update");
                if (checkExistense == 0)
                {
                    CommonMethods.InsertUpdatePumps(cls, "Update");
                    TempData["Success"] = "Record update successfully ";
                }
                else
                {
                    TempData["Error"] = "Pump Name already exists ! ";
                    return View(pmp);

                }

            }
            return RedirectToAction("Index",new {ImoNo= cls.VesselId});
        }

        public ActionResult Edit(int id)
        {
            PumpClass pmp = new PumpClass();
            pmp.GetPumpList = CommonMethods.edtiPumpList(id);
            pmp.Id = id;
            pmp.Name = pmp.GetPumpList.Select(x => x.Name).SingleOrDefault();
            pmp.Capacity = pmp.GetPumpList.Select(x => x.Capacity).SingleOrDefault();          
            pmp.PumpTypeId = pmp.GetPumpList.Select(x => x.PumpTypeId).SingleOrDefault();
            pmp.PumpUseId = pmp.GetPumpList.Select(x => x.PumpUseId).SingleOrDefault();
            return View("addpump", pmp);
        }

        public ActionResult Delete(int id)
        {
            CommonMethods.CommonDelete(id, "Pump");
            TempData["Success"] = "Record deleted successfully";
            int imono = Convert.ToInt32(Session["ImoNo"]);
            return RedirectToAction("Index", new { ImoNo = imono});
        }

        [HttpPost]

        public ActionResult InsertInOrder(string pumplist)
        {
            try
            {
                int vesselid = Convert.ToInt32(Session["ImoNo"]);
                var Result = pumplist;
                string json = Result.ToString();
                string data = json;
                int count = 0;
                var Json = JsonConvert.DeserializeObject<List<PumpClass>>(data);
                foreach (var rootObject in Json)
                {
                    count++;
                    CommonMethods.InsertinOrder(count, rootObject.Name, 0, vesselid, "Pump");
                }
            }
            catch { }
            int imo = Convert.ToInt32(Session["ImoNo"]);
            TempData["Success"] = "Record successfully saved !";
            return Json(Url.Action("Index", "Pump",new {ImoNo= imo }));

        }
    }
}