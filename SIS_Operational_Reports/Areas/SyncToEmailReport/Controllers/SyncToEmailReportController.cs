using DataBuildingLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace SIS_Operational_Reports.Areas.SyncToEmailReport.Controllers
{
    public class SyncToEmailReportController : Controller
    {
        private List<SyncEmailVesselsReport> GetSyncEmailList()
        {
            var list = new List<SyncEmailVesselsReport>();
            try
            {
                using (var cmd = new SqlCommand("USP_GetSyncEmailVesselsReport", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ConnectionBulder.con.State == ConnectionState.Closed)
                        ConnectionBulder.con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SyncEmailVesselsReport
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                VesselId = dr["VesselId"]?.ToString() ?? "",
                                EmailTo = dr["EmailTo"]?.ToString() ?? "",
                                EmailCC = dr["EmailCC"]?.ToString() ?? "",
                                CreatedBy = dr["CreatedBy"]?.ToString() ?? "",
                                ModifiedBy = dr["ModifiedBy"]?.ToString() ?? "",
                                CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : DateTime.MinValue,
                                ModifiedDate = dr["ModifiedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["ModifiedDate"]) : null,
                                IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                            });
                        }
                    }
                    ConnectionBulder.con.Close();
                }
            }
            catch { }
            return list;
        }

        private SyncEmailVesselsReport GetSyncEmailById(int id)
        {
            SyncEmailVesselsReport item = null;
            try
            {
                using (var cmd = new SqlCommand("USP_GetSyncEmailVesselsReportById", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    if (ConnectionBulder.con.State == ConnectionState.Closed)
                        ConnectionBulder.con.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            item = new SyncEmailVesselsReport
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                VesselId = dr["VesselId"]?.ToString() ?? "",
                                EmailTo = dr["EmailTo"]?.ToString() ?? "",
                                EmailCC = dr["EmailCC"]?.ToString() ?? "",
                                CreatedBy = dr["CreatedBy"]?.ToString() ?? "",
                                ModifiedBy = dr["ModifiedBy"]?.ToString() ?? "",
                                CreatedDate = dr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CreatedDate"]) : DateTime.MinValue,
                                ModifiedDate = dr["ModifiedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["ModifiedDate"]) : null,
                                IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                            };
                        }
                    }
                    ConnectionBulder.con.Close();
                }
            }
            catch { }
            return item;
        }

       
        public ActionResult Index()
        {
            var epm = new ExportPeram2();
            epm.SyncEmailList = GetSyncEmailList();
            epm.VesselList = string.IsNullOrEmpty(StaticHelper.PermittedVessel) ? CommonClass.GetVesselList() : CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            return View(epm);
        }

       
        public ActionResult Add(int? id)
        {
            var epm = new ExportPeram2();
            epm.VesselList = string.IsNullOrEmpty(StaticHelper.PermittedVessel) ? CommonClass.GetVesselList() : CommonClass.GetVesselList(StaticHelper.PermittedVessel);
            if (id.HasValue && id.Value > 0)
            {
                var item = GetSyncEmailById(id.Value);
                if (item != null)
                {
                    ViewBag.EditId = item.Id;
                    ViewBag.VesselId = string.IsNullOrEmpty(item.VesselId) ? "" : item.VesselId.Split(',')[0].Trim();
                    ViewBag.EmailTo = item.EmailTo ?? "";
                    ViewBag.EmailCC = item.EmailCC ?? "";
                }
            }
            return View(epm);
        }

        [HttpPost]
        public ActionResult Add(SyncEmailVesselsReport sync)
        {
            try
            {
                using (var cmd = new SqlCommand("USP_insertSyncEmailVesselsReport", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", sync.Id);
                    cmd.Parameters.AddWithValue("@VesselId", sync.VesselId ?? "");
                    cmd.Parameters.AddWithValue("@EmailTo", sync.EmailTo ?? "");
                    cmd.Parameters.AddWithValue("@EmailCC", sync.EmailCC ?? "");
                    cmd.Parameters.AddWithValue("@CreatedBy", sync.CreatedBy ?? "Admin");
                    cmd.Parameters.AddWithValue("@ModifiedBy", sync.ModifiedBy ?? "Admin");
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                    if (ConnectionBulder.con.State == ConnectionState.Closed)
                        ConnectionBulder.con.Open();
                    cmd.ExecuteNonQuery();
                    ConnectionBulder.con.Close();
                }
                TempData["success"] = "Data Saved Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error while saving data: " + ex.Message;
                var epm = new ExportPeram2();
                epm.VesselList = string.IsNullOrEmpty(StaticHelper.PermittedVessel) ? CommonClass.GetVesselList() : CommonClass.GetVesselList(StaticHelper.PermittedVessel);
                return View(epm);
            }
        }

        [HttpPost]
        public ActionResult GetById(int id)
        {
            var item = GetSyncEmailById(id);
            if (item == null)
                return Json(new { success = false });
            return Json(new
            {
                success = true,
                id = item.Id,
                vesselId = item.VesselId,
                emailTo = item.EmailTo,
                emailCC = item.EmailCC
            });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var cmd = new SqlCommand("USP_DeleteSyncEmailVesselsReport", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ModifiedBy", "Admin");
                    if (ConnectionBulder.con.State == ConnectionState.Closed)
                        ConnectionBulder.con.Open();
                    cmd.ExecuteNonQuery();
                    ConnectionBulder.con.Close();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
