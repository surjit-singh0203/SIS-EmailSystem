using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
   public class ExportPeram
    {
        public ExportPeram()
        {
            VoyageNumberList = voyageNList2(); //LoadingReport.voyageNList();
            VoyageIds = new List<int>();
        }
        public List<int> VoyageIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<VoyageClass> VoyageNumberList { get; set; }

        public  List<VoyageClass> voyageNList2()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }
    }
}
