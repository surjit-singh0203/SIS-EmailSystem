using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.VesselParticulars.Controllers
{
    [Authorize]
    public class VesselPartController : Controller
    {
        VesselDetail vD = new VesselDetail();
        // GET: VesselParticulars/VesselPart
        public ActionResult Index(int? pageNo, string firstVal, string dateF, string dateT)
        {
            VesselDetail mode = new VesselDetail();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            //mode.GetVesselList = CommonMethods.GetVesselList(currPage, pageSize);

            TempData["CurrentPage"] = currPage;


            if (firstVal == null)
            {
                mode.GetVesselList = CommonMethods.GetVesselListNew( currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetVesselList = CommonMethods.GetVesselListNew( currPage, pageSize);
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetVesselList = CommonMethods.SearchVesselPartList(0, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchVesselPart", mode);
            }


            //mode.GetVesselList = CommonMethods.GetVesselListNew(currPage, pageSize);
            return View(mode);
        }

        public ActionResult addvessel()
        {
            VesselDetail vD = new VesselDetail();
            return View(vD);
        }
        [HttpPost]
        public ActionResult addvessel(VesselDetail cls)
        {
            if (cls.Id == 0)
            {
                int checkExistense = CommonMethods.CheckVesselExistence(cls,0,0,"Insert");
                if (checkExistense == 0)
                {
                    CommonMethods.InsertUpdateVessel(cls, "Insert");
                    TempData["Success"] = "Record saved successfully";
                }
                else
                {
                    TempData["Error"] = "Vessel detail already exists ! ";
                    return RedirectToAction("addvessel");
                }
            }
            else
            {
                //int checkExistense = CommonMethods.CheckVesselExistence(cls, 0,cls.Id,"Update");
                //if (checkExistense == 0)
                //{
                    CommonMethods.InsertUpdateVessel(cls, "Update");
                    TempData["Success"] = "Record update successfully ";
               // }
                //else
                //{
                //    TempData["Error"] = "Vessel detail already exists ! ";
                //    return RedirectToAction("addvessel");
                //}

            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            VesselDetail vd = new VesselDetail();
            vd.GetVesselList= CommonMethods.edtiVesselList(id);
            vd.Id = id;
            vd.VesselName = vd.GetVesselList.Select(x => x.VesselName).SingleOrDefault();
            vd.ImoNo = vd.GetVesselList.Select(x => x.ImoNo).SingleOrDefault();
            vd.Displacement = vd.GetVesselList.Select(x => x.Displacement).SingleOrDefault();
            vd.FleetNameID= vd.GetVesselList.Select(x => x.FleetNameID).SingleOrDefault();
            vd.FleetTypeID = vd.GetVesselList.Select(x => x.FleetTypeID).SingleOrDefault();
            vd.VesselTradeID = vd.GetVesselList.Select(x => x.VesselTradeID).SingleOrDefault();
            return View("addvessel", vd);
           
        }

        public ActionResult Delete(int id)
        {
            CommonMethods.CommonDelete(id, "VesselDetail");
            TempData["Success"] = "Record deleted successfully";
            return RedirectToAction("Index");
        }
    }
}