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
    }
}
