using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
  public  class CommonClass
    {
        public static List<SelectListItem> GetVesselList()
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });
            //using (SqlDataAdapter sda = new SqlDataAdapter("Select VesselName,ImoNo from VesselDetail", ConnectionBulder.con))
            using (SqlDataAdapter sda = new SqlDataAdapter("Select VesselName,ImoNo from VesselDetail", ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {

                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["VesselName"].ToString(), Value = tbl.Rows[i]["ImoNo"].ToString() });

                    }

                }
            }
            return jst;
        }

        public static string GetVesselNamesByImoNo(string imoNoList)
        {
            if (string.IsNullOrEmpty(imoNoList)) return "";
            var list = GetVesselList(imoNoList);
            return string.Join(", ", list.Select(x => x.Text));
        }

        public static List<SelectListItem> GetVesselList(string Vesselid)
        {
            List<SelectListItem> jst = new List<SelectListItem>();
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });
            //using (SqlDataAdapter sda = new SqlDataAdapter("Select VesselName,ImoNo from VesselDetail", ConnectionBulder.con))
            using (SqlDataAdapter sda = new SqlDataAdapter("Select VesselName,ImoNo from VesselDetail where ImoNo in( SELECT value FROM SplitString('" + Vesselid + "', ','))", ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {

                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["VesselName"].ToString(), Value = tbl.Rows[i]["ImoNo"].ToString() });

                    }

                }
            }
            return jst;
        }
        public static int GetMaxId(string TableName)
        {
            int id = 0;
            using (SqlDataAdapter sda = new SqlDataAdapter("Select Max(Id) as Id from " + TableName, ConnectionBulder.con))
            {
                DataTable tbl = new DataTable();
                sda.Fill(tbl);
                if (tbl.Rows.Count > 0)
                {
                    string ids = tbl.Rows[0]["Id"].ToString();
                    id = string.IsNullOrEmpty(ids)== true? 0: Convert.ToInt32(tbl.Rows[0]["Id"]);
                }
            }
            return id;
        }

        public static List<SelectListItem> portList()
        {
            List<SelectListItem> ftype = new List<SelectListItem>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select  distinct(portname)  from portlist", ConnectionBulder.con))
            {
                //adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    ftype.Add(new SelectListItem() { Text = dt.Rows[i]["PortName"].ToString(), Value = dt.Rows[i]["PortName"].ToString() });
                   
                }
                // con.Close();
            }

            return ftype;
        }

        /// <summary>
        /// Gets email To and CC recipients by vessel ID using USP_GetEmailBYvessel.
        /// </summary>
        /// <param name="vesselId">Vessel ID (ImoNo or VesselId)</param>
        /// <returns>List of email info with Id, EmailTo, EmailCC, VesselId</returns>
        public static List<EmailByVesselInfo> GetEmailByVessel(int vesselId)
        {
            var list = new List<EmailByVesselInfo>();
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_GetEmailBYvessel", ConnectionBulder.con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@VesselId", vesselId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new EmailByVesselInfo
                {
                    Id = row["Id"] != DBNull.Value && row["Id"] != null ? Convert.ToInt32(row["Id"]) : 0,
                    EmailTo = row["EmailTo"]?.ToString() ?? "",
                    EmailCC = row["EmailCC"]?.ToString() ?? "",
                    VesselId = row["VesselId"] != DBNull.Value && row["VesselId"] != null ? Convert.ToInt32(row["VesselId"]) : 0
                });
            }
            return list;
        }

        /// <summary>
        /// Gets active sync email vessel configs using USP_GetSyncEmailVesselsReport.
        /// </summary>
        /// <param name="vesselIds">Optional. Filter by vessel ID(s). Pass null to get all.</param>
        /// <returns>List of email info with Id, VesselId, EmailTo, EmailCC</returns>
        public static List<EmailByVesselInfo> GetSyncEmailVesselsReport(params int[] vesselIds)
        {
            var list = new List<EmailByVesselInfo>();
            DataTable dt = new DataTable();
            using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailVesselsReport", ConnectionBulder.con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            var vesselSet = vesselIds != null && vesselIds.Length > 0 ? new HashSet<int>(vesselIds) : null;
            foreach (DataRow row in dt.Rows)
            {
                int vId = row["VesselId"] != DBNull.Value && row["VesselId"] != null ? Convert.ToInt32(row["VesselId"]) : 0;
                if (vesselSet != null && !vesselSet.Contains(vId)) continue;
                list.Add(new EmailByVesselInfo
                {
                    Id = row["Id"] != DBNull.Value && row["Id"] != null ? Convert.ToInt32(row["Id"]) : 0,
                    EmailTo = row["EmailTo"]?.ToString() ?? "",
                    EmailCC = row["EmailCC"]?.ToString() ?? "",
                    VesselId = vId,
                    LastSent = row["LastSent"] != DBNull.Value ? Convert.ToDateTime(row["LastSent"]) : DateTime.MinValue
                });
            }
            return list;
        }
    }

    public class EmailByVesselInfo
    {
        public int Id { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public int VesselId { get; set; }
        public DateTime LastSent { get; set; }
    }
}
