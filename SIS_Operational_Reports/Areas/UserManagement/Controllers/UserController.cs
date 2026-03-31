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
using System.Threading.Tasks;
using System.IO;

namespace SIS_Operational_Reports.Areas.UserManagement.Controllers
{
    [Authorize]
    [UserAuthenticationFilter]
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
                mode.GetUserList = CommonMethods.GetUserList( currPage, pageSize, "Office");
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetUserList = CommonMethods.GetUserList(currPage, pageSize, "Office");
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetUserList = CommonMethods.SearchUserList(0, "Office", currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchUser", mode);
            }

            //mode.GetUserList = CommonMethods.GetUserList( currPage, pageSize);
                
            

            return View(mode);
        }

        public ActionResult VesselUsers(int? pageNo, string firstVal, string dateF, string dateT)
        {
            UserDetail mode = new UserDetail();
            //mode.GetUserList = CommonMethods.GetUserList();

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);

            TempData["CurrentPage"] = currPage;



            if (firstVal == null)
            {
                mode.GetUserList = CommonMethods.GetUserList1(currPage, pageSize, "Vessel");
                return View(mode);
            }
            else if (firstVal == "" && dateF == "")
            {
                mode.GetUserList = CommonMethods.GetUserList1(currPage, pageSize,"Vessel");
                return View(mode);
            }
            else if (firstVal != null)
            {
                mode.GetUserList = CommonMethods.SearchUserList(0,"Vessel", currPage, pageSize, firstVal, dateF, dateT);
                return PartialView("_searchVesselUser", mode);
            }

            //mode.GetUserList = CommonMethods.GetUserList( currPage, pageSize);



            return View(mode);
        }
        public ActionResult AddUserVessel()
        {
            UserDetail ud = new UserDetail();
            ud.UserType = "Vessel";
            //ud.DepartmentWiseUserRole = GetDepartmentVessel(ud.UserType);

            ud.DepartmentWiseUserRole = GetDepartment(ud.UserType);

            return View(ud);
        }
        [HttpPost]
        public ActionResult AddUserVessel(UserDetail user)
        {

            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password does not match.");
                return View("AddUserVessel", user);
            }
          


            if ((user.VesselIDs != null) || (user.DepId == 101))
            {
                if (user.DepId == 101)
                {
                    List<int> list1 = new List<int>();
                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from VesselDetail where IsActive=1", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                list1.Add(Convert.ToInt32(dt.Rows[i]["IMoNo"]));
                            }

                            if (list1.Count > 0)
                            {
                                foreach (int id in list1)
                                    user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                            }
                        }
                    }
                }
                else
                {
                    user.VesselIDs = user.VesselIDs.Distinct().ToList();
                    if (user.VesselIDs.Count > 0)
                    {
                        foreach (int id in user.VesselIDs)
                            user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "Please choose assign vessel.");
                return View("AddUserVessel", user);
            }

            user.AssignVessel = user.AssignVessel.Trim(',');

            //Guid num = Guid.NewGuid();
            

           

            user.UserType = "Vessel";

            string UName = Convert.ToString(user.DepId) == "1" ? "master" : Convert.ToString(user.DepId) == "2" ? "choff" : Convert.ToString(user.DepId) == "3" ? "2off" : Convert.ToString(user.DepId) == "4" ? "cheng" : Convert.ToString(user.DepId).ToString();

            string Uname1 =  UName;

            user.UserName =  UName +"_"+ user.AssignVessel;

            user.UserEmail = Uname1 + "_" + user.AssignVessel + "@gmail.com";
            //user.UserEmail = UName;

            //user.UserName = user.UserEmail;
            //user.UserEmail = user.UserName + "@gm.com";
            var appuser = new ApplicationUser { UserName = user.UserName, Email = user.UserEmail, EmailConfirmed = false };
            var result = UserManager.CreateAsync(appuser, user.Password);
            if (result.Result.Succeeded)
            {
                //int checkExistense = CommonMethods.CheckUserExistence(user, 0);
                //if (checkExistense == 0)
                //{

                //using (SqlDataAdapter adp = new SqlDataAdapter("Update aspnetusers set email='" + UName + "' where email='" + user.UserEmail + "'", ConnectionBulder.con))
                //{
                //    DataTable dt = new DataTable();
                //    adp.Fill(dt);
                //}



               

                CommonMethods.InsertUpdateUD(user, "Insert");
                TempData["Success"] = "Record saved successfully";
                return RedirectToAction("VesselUsers");
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
                return View("AddUserVessel", user);
            }
            // return View("AddUser", user);
        }
        public ActionResult AddUser()
        {
            UserDetail ud = new UserDetail();
            ud.UserType = "Office";
            
            ud.DepartmentWiseUserRole = GetDepartment(ud.UserType);
           
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
            user.UserType = "Office";
            user.UserName = user.UserEmail;
            //user.UserEmail = user.UserName + "@gm.com";
            var appuser = new ApplicationUser { UserName = user.UserName, Email = user.UserEmail, EmailConfirmed = false };
            var result = UserManager.CreateAsync(appuser, user.Password);
            if (result.Result.Succeeded)
            {
                //int checkExistense = CommonMethods.CheckUserExistence(user, 0);
                //if (checkExistense == 0)
                //{

                if ((user.VesselIDs != null) || (user.DepId == 101))
                {
                    if (user.DepId == 101)
                    {
                        List<int> list1 = new List<int>();
                        using (SqlDataAdapter adp=new SqlDataAdapter ("select * from VesselDetail where IsActive=1", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                            if(dt.Rows.Count >0)
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    list1.Add(Convert.ToInt32(dt.Rows[i]["IMoNo"]));
                                }

                                if (list1.Count > 0)
                                {
                                    foreach (int id in list1)
                                        user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                                }
                            }
                        }
                    }
                    else
                    {
                        user.VesselIDs = user.VesselIDs.Distinct().ToList();
                        if (user.VesselIDs.Count > 0)
                        {
                            foreach (int id in user.VesselIDs)
                                user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please choose assign vessel.");
                    return View("AddUser", user);
                }

                user.AssignVessel = user.AssignVessel.Trim(',');

                //using (SqlDataAdapter adp=new SqlDataAdapter ("",ConnectionBulder.con))
                //{

                //}
                CommonMethods.InsertUpdateUD(user, "Insert");
                TempData["Success"] = "Record saved successfully";
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
        public ActionResult DeleteVessel(int id)
        {          
            CommonMethods.CommonDelete(id, "User");
            return RedirectToAction("VesselUsers");
        }
        public ActionResult Edit(int id, int pageno)
        {
            UserDetail user =  CommonMethods.GetUserList(pageno, 10, "Office").Where(x=> x.Id == id).FirstOrDefault();
            user.EditUser = user;
            user.VesselID = user.UserType == "Vessel" ? Convert.ToInt32(user.AssignVessel) : 0;
            user.RankList = user.GetRanksvalues(user.DepId);

            



            //user.VesselList = user.VesselList();
            return View("Edit", user);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(UserDetail user)
        {
            //user.AssignVessel = Convert.ToString( user.VesselID);
            //CommonMethods.InsertUpdateUD(user, "Update");

            if (user.VesselIDs == null)
            {
                ModelState.AddModelError("", "Please choose assign vessel.");
                return View("Edit", user);
            }
            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password does not match.");
                return View("Edit", user);
            }

            else
            {
                if (user.Password != user.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Password and Confirm Password does not match.");
                    return View("Edit", user);
                }



                string dd = Convert.ToString(user.UserID);

            

                var token = await UserManager.GeneratePasswordResetTokenAsync(dd);
                var result = await UserManager.ResetPasswordAsync(dd, token, user.Password);

                user.VesselIDs = user.VesselIDs.Distinct().ToList();
                if (user.VesselIDs.Count > 0)
                {
                    foreach (int id in user.VesselIDs)
                        user.AssignVessel = string.Format("{0},{1}", id, user.AssignVessel);
                }

                user.AssignVessel = user.AssignVessel.Trim(',');

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("update AspNetUsers set Email='" + user.UserName + "', UserName='" + user.UserName + "' where Id='" + user.UserID + "'", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                    }

                    using (SqlDataAdapter adp = new SqlDataAdapter("update UserDetail set UserEmail='" + user.UserName + "', AssignVessel ='" + user.AssignVessel + "' where UserID='" + user.UserID + "'", ConnectionBulder.con))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                    }
                }
                catch {

                    ModelState.AddModelError("", "User email already exist.");
                    return View("Edit", user);
                }

               
            }

            TempData["Success"] = "Record update successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult EditVessel(int id)
        {
            UserDetail user = CommonMethods.GetUserList1(1, 30, "Vessel").Where(x => x.Id == id).FirstOrDefault();
            user.UserType = "Vessel";


            user.DepartmentWiseUserRole = GetDepartment(user.UserType);

            user.VesselID = user.VesselID;
            user.EditUser = user;
            //user.VesselID = user.UserType == "Vessel" ? Convert.ToInt32(user.AssignVessel) : 0;
            user.RankList = user.GetRanksvalues(user.DepId);




            //user.VesselList = user.VesselList();
            return View("EditVessel", user);
        }
        [HttpPost]
        public async Task<ActionResult> EditVessel(UserDetail user)
        {
            user.AssignVessel = Convert.ToString(user.VesselID);

            string LoginUser = Convert.ToString(user.DepId) == "1" ? "master" : Convert.ToString(user.DepId) == "2" ? "choff" : Convert.ToString(user.DepId) == "3" ? "2off" : Convert.ToString(user.DepId) == "4" ? "cheng" : Convert.ToString(user.DepId).ToString();

            //user.UserName = LoginUser;


            string Uname1 = LoginUser;

            user.UserName = LoginUser + "_" + user.AssignVessel;

            user.UserEmail = Uname1 + "_" + user.AssignVessel + "@gmail.com";

            if (user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and Confirm Password does not match.");
                return View("Edit", user);
            }
            else
            {
                //int check = 0;
                //using (SqlDataAdapter adp=new SqlDataAdapter ("select * from UserDetail where UserEmail='"+ user.UserName + "' and AssignVessel='"+ user.AssignVessel + "' and UserID not in ('"+ user.UserID + "')", ConnectionBulder.con))
                //{
                //    DataTable dt = new DataTable();
                //    adp.Fill(dt);
                //    if(dt.Rows.Count>0)
                //    {
                        
                //    }
                //}


                string dd = Convert.ToString(user.UserID);



                var token = await UserManager.GeneratePasswordResetTokenAsync(dd);
                var result = await UserManager.ResetPasswordAsync(dd, token, user.Password);


                CommonMethods.InsertUpdateUD(user, "Update");

                //using (SqlDataAdapter adp = new SqlDataAdapter("update AspNetUsers set Email='" + user.UserName + "', UserName='" + user.UserName + "' where Id='" + user.UserID + "'", ConnectionBulder.con))
                //{
                //    DataTable dt = new DataTable();
                //    adp.Fill(dt);
                //}

                TempData["Success"] = "Record update successfully.";
            }
            return RedirectToAction("VesselUsers");
        }
        //public JsonResult GetRanks(string PortName)
        //{

        //    // var facilityList = context.PortLists.Where(p => p.PortName.Equals(PortName)).Select(u => u.FacilityName).ToList();
        //    var facilityList = CommonClass.GetFacilityNameList(PortName);
        //    //if (facilityList.Count == 0)
        //    //    facilityList.Add("Other");

        //    return Json(new { Result = true, Data = facilityList }, JsonRequestBehavior.AllowGet);
        //}

        public List<SelectListItem> GetDepartment(string Uts)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            string qry = "";
            if (Uts == "Vessel")
            {
                qry = "Select * from DepartmentWiseRole where id < 100";
            }
            else
            {
                qry = "Select * from DepartmentWiseRole where id > 100";
            }
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });

            using (SqlDataAdapter sda = new SqlDataAdapter(qry, ConnectionBulder.con))
            {
                using (DataTable tbl = new DataTable())
                {
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["Departments"].ToString(), Value = tbl.Rows[i]["Id"].ToString() });

                        }

                    }
                }
            }
            return jst;
        }

        public List<SelectListItem> GetDepartmentVessel(string Uts)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            string qry = "";
            if (Uts == "Vessel")
            {
                qry = "Select * from DepartmentWiseRole where id < 100";
            }
          
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });

            using (SqlDataAdapter sda = new SqlDataAdapter(qry, ConnectionBulder.con))
            {
                using (DataTable tbl = new DataTable())
                {
                    sda.Fill(tbl);
                    if (tbl.Rows.Count > 0)
                    {
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {

                            jst.Add(new SelectListItem() { Text = tbl.Rows[i]["Departments"].ToString(), Value = tbl.Rows[i]["Id"].ToString() });

                        }

                    }
                }
            }
            return jst;
        }
        public JsonResult GetUTypes(string Uts)
        {
            List<SelectListItem> jst = new List<SelectListItem>();

            jst =  GetDepartment(Uts);
            

            return Json(new { Result = true, Data = jst }, JsonRequestBehavior.AllowGet);
        }

        
        SqlCommand sqlcmd = new SqlCommand();
        SqlConnection conVessel = new SqlConnection();
        DataTable dt = new DataTable();
        SqlDataAdapter adp = new SqlDataAdapter();



        private void updateVesselDB(string vesselname,string vesselid,decimal? displacement,string password)
        {
            try
            {
                conVessel.ConnectionString = ConfigurationManager.ConnectionStrings["SISVesselDBContext"].ConnectionString;

                using (adp = new SqlDataAdapter("update VesselDetail set VesselName='"+ vesselname + "', ImoNo='"+ vesselid + "', Displacement='"+ displacement + "'", conVessel))
                {
                    dt = new DataTable();
                    adp.Fill(dt);
                }

                string username = "master_" + vesselid;
                string useremail = username+"@gmail.com";

                using (adp = new SqlDataAdapter("update UserDetail set UserEmail='"+ username + "', FullName='"+ username + "',AssignVessel='"+ vesselid + "'", conVessel))
                {
                    dt = new DataTable();
                    adp.Fill(dt);
                }

                using (adp = new SqlDataAdapter("update AspNetUsers set Email='" + useremail + "', UserName='" + username + "', PasswordHash='" + password + "'", conVessel))
                {
                    dt = new DataTable();
                    adp.Fill(dt);
                }
            }
            catch
            {

            }
        }
        public ActionResult DownloadDatabase(int id)
        {
            string imono = ""; string vesselname = ""; decimal displacement = 0;string pwd = "";
            UserDetail user = CommonMethods.GetUserList1(1, 30, "Vessel").Where(x => x.Id == id).FirstOrDefault();


            using (SqlDataAdapter adp = new SqlDataAdapter("select PasswordHash from AspNetUsers where id='" + user.UserID + "'", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    pwd = dt.Rows[0][0].ToString();
                }
            }


            using (SqlDataAdapter adp = new SqlDataAdapter("select * from VesselDetail where ImoNo=" + user.VesselID + " and IsActive=1", ConnectionBulder.con))
            {
                DataTable dt = new DataTable();
                adp.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    vesselname = dt.Rows[0]["VesselName"].ToString();
                    imono = dt.Rows[0]["ImoNo"].ToString();
                    displacement = Convert.ToDecimal(dt.Rows[0]["Displacement"]);
                }
            }


            updateVesselDB(vesselname, imono, displacement, pwd);


            //string backupDIR = "C:\\SISNova_Vessel_DB_Backup";
            //if (!System.IO.Directory.Exists(backupDIR))
            //{
            //    System.IO.Directory.CreateDirectory(backupDIR);
            //}
            //try
            //{               
            //    adp = new SqlDataAdapter("backup database SISOperationalDB to disk='" + backupDIR + "\\SISOperationalDB.bak'", ConnectionBulder.con);
            //    adp.Fill(dt);
            //}
            //catch (Exception ex)
            //{
            //    TempData["Error"] = ex.Message.ToString();               
            //}


            ////=============================

            string dbNAme = "SISOperationalDB";
            string backupDestination = Server.MapPath("~/Files");
            if (!Directory.Exists(backupDestination))
            {
                Directory.CreateDirectory(backupDestination);
            }
            string fileName = dbNAme + " of " + DateTime.Now.ToString("yyyy-MM-dd@HH_mm") + ".bak";
           // string conString = ConnectionBulder.con;
            string query = "BACKUP database " + dbNAme + " to disk='" + backupDestination + "\\" + fileName + "'";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = query;
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteScalar();
                    con.Close();
                }
            }

            byte[] bytes =System.IO.File.ReadAllBytes(Path.Combine(backupDestination, fileName));
            // Delete .bak file from server folder.
            //if (Directory.Exists(backupDestination))
            //{
            //    Directory.Delete(backupDestination, true);
            //}

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();


            ////===============




            //DailyNoonReport vd = new DailyNoonReport();



            //DataSet ds = CommonMethods.ExportBulkNoonRList(id, vesselid);
            //using (XLWorkbook wb = new XLWorkbook())
            //{

            //    int i = 0;
            //    foreach (DataTable table in ds.Tables)
            //    {

            //        foreach (DataRow row in table.Rows)
            //        {

            //        }

            //        if (i == 0)
            //            table.TableName = "BulkNoonReport";


            //        if (table.Rows.Count > 0)
            //        {
            //            var protectedsheet = wb.Worksheets.Add(table);

            //            var projection = protectedsheet.Protect("49WEB$TREET#");
            //            projection.InsertColumns = true;
            //            projection.InsertRows = true;
            //        }
            //        i++;
            //    }
            //    wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //    wb.Style.Font.Bold = true;
            //    DateTime today = DateTime.Today;
            //    Response.Clear();
            //    Response.BufferOutput = true;
            //    Response.Charset = "";
            //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //    Response.AddHeader("content-disposition", "attachment;filename=BulkNoonReport_Sis_Nova_" + DateTime.Now.ToString("ddMMyyyy") + "_.xlsx");

            //    using (MemoryStream MyMemoryStream = new MemoryStream())
            //    {
            //        wb.SaveAs(MyMemoryStream);
            //        MyMemoryStream.WriteTo(Response.OutputStream);
            //        Response.End();
            //    }

            //    Response.Clear();

            //    Thread.Sleep(300);
            //    TempData["Success"] = "Data has been Export Successfully";
            //}

            TempData["Success"] = "Database created successfully";
            return RedirectToAction("VesselUsers");

        }

    }





    class RankData
    {
        public int Id { get; set; }
        public String Rank { get; set; }
    }
}