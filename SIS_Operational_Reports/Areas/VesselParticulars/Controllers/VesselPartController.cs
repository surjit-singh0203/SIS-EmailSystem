using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.VesselParticulars.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
    public class VesselPartController : BaseController
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
                mode.GetVesselList = CommonMethods.GetVesselListNew(StaticHelper.PermittedVessel, currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetVesselList = CommonMethods.GetVesselListNew(StaticHelper.PermittedVessel, currPage, pageSize);
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


                    if (StaticHelper.UserRole == "Administrator")
                    {
                        //var pv = StaticHelper.PermittedVessel + "," + cls.ImoNo;

                        using (SqlDataAdapter adp = new SqlDataAdapter("select * from Userdetail where UserID='" + StaticHelper.UserId + "'", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);

                            string pv = dt.Rows[0]["AssignVessel"].ToString() + "," + cls.ImoNo;


                            using (SqlDataAdapter adp1 = new SqlDataAdapter("update UserDetail set AssignVessel ='" + pv + "' where DepId=101", ConnectionBulder.con))
                            {
                                DataTable dt1 = new DataTable();
                                adp1.Fill(dt1);

                            }
                        }
                    }


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

                if (StaticHelper.UserRole == "Administrator")
                {
                    //var pv = StaticHelper.PermittedVessel + "," + cls.ImoNo;

                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from Userdetail where UserID='" + StaticHelper.UserId + "'", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);

                        string pv = dt.Rows[0]["AssignVessel"].ToString() + "," + cls.ImoNo;


                        using (SqlDataAdapter adp1 = new SqlDataAdapter("update UserDetail set AssignVessel ='" + pv + "' where DepId=101", ConnectionBulder.con))
                        {
                            DataTable dt1 = new DataTable();
                            adp1.Fill(dt1);

                        }
                    }
                }





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

        public ActionResult Delete(int id,int imo)
        {
            int deletechk = 0;
            using (SqlDataAdapter adp=new SqlDataAdapter ("select VesselID from CPpart1 where IsActive = 1 and VesselID='" + imo+"'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if(dt.Rows.Count >0)
                {
                    deletechk = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select * from TanksAndHolds where IsActive = 1 and VesselID='" + imo + "'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    deletechk = 1;
                }
            }
            using (SqlDataAdapter adp = new SqlDataAdapter("select * from tblPump where IsActive = 1 and VesselID='" + imo + "'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    deletechk = 1;
                }
            }
            if (deletechk == 0)
            {
                CommonMethods.CommonDelete(id, "VesselDetail");
                TempData["Success"] = "Record deleted successfully";
            }
            if (deletechk == 1)
            {              
                TempData["Error"] = "You can't delete this vessel because this vessel is used in another forms !";
            }
            return RedirectToAction("Index");
        }
    }
}