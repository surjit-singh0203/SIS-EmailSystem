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

namespace SIS_Operational_Reports.Areas.UserManagement.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        // GET: UserManagement/User
        // ApplicationDbContext context = new ApplicationDbContext();
      
        public ActionResult Index(int? pageNo, string firstVal, string dateF, string dateT)
        {
            UserDetail mode = new UserDetail();
            //mode.GetUserList = CommonMethods.GetUserList();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;



            if (firstVal == null)
            {
                mode.GetUserList = CommonMethods.GetUserList( currPage, pageSize);
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetUserList = CommonMethods.GetUserList(currPage, pageSize);
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetUserList = CommonMethods.SearchUserList(0, currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchUser", mode);
            }

            //mode.GetUserList = CommonMethods.GetUserList( currPage, pageSize);
                
            

            return View(mode);
        }

        public ActionResult AddUser()
        {
            UserDetail ud = new UserDetail();
            return View(ud);
        }
        [HttpPost]
        public ActionResult AddUser(UserDetail user)
        {
            
            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password does not match.");
                return View("AddUser", user);
            }
            //user.UserEmail = user.UserName + user.VesselID + "@gm.com";      
            user.UserType = user.UserType;
            user.UserName = user.UserEmail;
            //user.UserEmail = user.UserName + "@gm.com";
            var appuser = new ApplicationUser { UserName = user.UserName, Email = user.UserEmail, EmailConfirmed = true };
            var result = UserManager.CreateAsync(appuser, user.Password);
            if (result.Result.Succeeded)
            {
                //int checkExistense = CommonMethods.CheckUserExistence(user, 0);
                //if (checkExistense == 0)
                //{

                if (user.VesselIDs != null)
                {
                    user.VesselIDs = user.VesselIDs.Distinct().ToList();
                    if (user.VesselIDs.Count > 0)
                    {
                        foreach (int id in user.VesselIDs)
                            user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please choose assign vessel.");
                    return View("AddUser", user);
                }

                user.AssignVessel = user.AssignVessel.Trim(',');

                CommonMethods.InsertUpdateUD(user, "Insert");
                    return RedirectToAction("Index");
                //}
                //else
                //{
                //    string errormessage = "";

                //    errormessage = "This account already exists !";


                //    ModelState.AddModelError("", errormessage);
                //    return View("AddUser", user);
                //}
            }
            //else
            //{

            //    string errormessage = "";
            //    if (result.Result.Errors.Count() > 0)
            //    {

            //        errormessage = "This account already exists !";
            //    }

            //    ModelState.AddModelError("", errormessage);
            //    return View("AddUser", user);
            //}

            //    user.UserEmail = user.UserName + "@gm.com";
            //var appuser = new ApplicationUser { UserName = user.UserName, Email = user.UserName + user.VesselID + "@gm.com", EmailConfirmed = true };
            //var result = UserManager.CreateAsync(appuser, user.Password);




            //if (result.Result.Succeeded)
            //{
            //    //using (SqlDataAdapter adp = new SqlDataAdapter("update aspnetusers set username='" + user.UserName + user.VesselID + "' where id='" + appuser.Id + "'", ConnectionBulder.con))
            //    //{
            //    //    DataTable dt = new DataTable();
            //    //    adp.Fill(dt);
            //    //}
            //    // user.UserID = result.Id;

            //    CommonMethods.InsertUpdateUD(user, "Insert");

            //    return RedirectToAction("Index");
            //}
            else
            {

                //string errormessage = "";
                //if (result.Result.Errors.Count() > 0)
                //{
                //    errormessage = "This account already exists !";
                //}
                //ModelState.AddModelError("", errormessage);
                //return View("AddUser", user);


                string errormessage = "";
                if (result.Result.Errors.Count() > 0)
                {
                    foreach (string err in result.Result.Errors)
                        errormessage = err + ", " + errormessage;

                    if (!string.IsNullOrEmpty(errormessage))
                        errormessage = errormessage.TrimEnd(',');
                }

                ModelState.AddModelError("", errormessage);
                return View("AddUser", user);
            }
            // return View("AddUser", user);
        }

        public ActionResult Delete(int id)
        {
            //UserDetail ud = new UserDetail() { Id = id, UserEmail = "", FullName = "", CreatedDate = DateTime.Now.Date, UserType = "", AssignVessel = "0", RankId = 0, DepId = 0, UserID = "" };

            //using (SqlDataAdapter adp=new SqlDataAdapter ("",ConnectionBulder.con))
            //{
            //    DataTable dt = new DataTable();
            //    adp.Fill(dt);
            //}
            
            CommonMethods.CommonDelete(id, "User");

            return RedirectToAction("Index");
        }
        public ActionResult Edit(int id)
        {
            UserDetail user =  CommonMethods.GetUserList(1,30).Where(x=> x.Id ==id).FirstOrDefault();
            user.EditUser = user;
            user.VesselID = user.UserType == "Vessel" ? Convert.ToInt32(user.AssignVessel) : 0;
            user.RankList = user.GetRanksvalues(user.DepId);
            //user.VesselList = user.VesselList();
            return View("Edit", user);
        }
        [HttpPost]
        public ActionResult Edit(UserDetail user)
        {
            user.AssignVessel = Convert.ToString( user.VesselID);
            CommonMethods.InsertUpdateUD(user, "Update");

            using (SqlDataAdapter adp=new SqlDataAdapter ("update AspNetUsers set Email='"+user.UserName+ "', UserName='" + user.UserName + "' where Id='"+user.UserID+"'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
            }

            return RedirectToAction("Index");
        }
        //public JsonResult GetRanks(string PortName)
        //{

        //    // var facilityList = context.PortLists.Where(p => p.PortName.Equals(PortName)).Select(u => u.FacilityName).ToList();
        //    var facilityList = CommonClass.GetFacilityNameList(PortName);
        //    //if (facilityList.Count == 0)
        //    //    facilityList.Add("Other");

        //    return Json(new { Result = true, Data = facilityList }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetRanks(int Did)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            // List<RankData> jstd = new List<RankData>();
            using (SqlDataAdapter sda = new SqlDataAdapter("select * from Rank where Dep_Id=" + Did + "", ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {

                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["FixUser"].ToString(), Value = tbl.Rows[i]["FixUser"].ToString() });

                    }

                }
            }
            // return jst;

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }
    }

    class RankData
    {
        public int Id { get; set; }
        public String Rank { get; set; }
    }
}