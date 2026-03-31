using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
    public partial class VoyageClass
    {
       


        public int Id { get; set; }  
        public string VoyageNumber { get; set; }
        public int Nor_Conditions { get; set; }
        public int CPId { get; set; }
        //public int VoyageStartPoint { get; set; }
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        //public DateTime? ETA { get; set; }
        //public int DTG { get; set; }
        //public decimal CP_SOG { get; set; }
        //public decimal CP_Log_Speed { get; set; }
        //public decimal FW { get; set; }
        public int HeavyWeather_BSS { get; set; }
        public decimal HeavyWeather_WH { get; set; }
        public decimal HeavyWeather_CV { get; set; }
        public DateTime? CreatedDate { get; set; }       
        public bool? IsActive { get; set; }
        public int VesselId { get; set; }

        public string VoyageStartP { get; set; }
        public string VoyageEndP { get; set; }

        public int TotalCount { get; set; }




    }

    public class VoyageLeg
    {
        //public VoyageLeg()
        //{

        //    PortList = portList();
            

        //}
       // public List<PortClass> PortList { get; set; }
        public int Id { get; set; }     
        public int VoyageId { get; set; }
        public string LegPort_A { get; set; }
        public int ReasonforPortCall_A { get; set; }
        public string LegPort_B { get; set; }
        public int ReasonforPortCall_B { get; set; }
        public int DTG { get; set; }
        public decimal CP_SOG { get; set; }
        public decimal CP_Log_Speed { get; set; }
        public DateTime? CreatedDate { get; set; }       
        public bool? IsActive { get; set; }
       
        //public static List<PortClass> portList()
        //{
        //    List<PortClass> ftype = new List<PortClass>();
        //    using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
        //    {
        //        adp.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        adp.SelectCommand.Parameters.AddWithValue("@Action", "PortList");
        //        DataTable dt = new DataTable();
        //        adp.Fill(dt);
        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            ftype.Add(new PortClass
        //            {
        //                // Id = Convert.ToInt32(dt.Rows[i]["Id"]),
        //                PortName = dt.Rows[i]["PortName"].ToString()
        //            });
        //        }
        //        // con.Close();
        //    }

        //    return ftype;
        //}


    }

    public class FuelConsumption
    {
        public int Id { get; set; }
        public int VoyageId { get; set; }
        public int FuelTypeId { get; set; }
        public decimal CP_cons_perday_HFO { get; set; }
        public DateTime? CreatedDate { get; set; }      
        public bool? IsActive { get; set; }
    }
    public class NorCondition
    {
        public int Id { get; set; }
        public string Conditions { get; set; }
    }
    public class CharterPartyC
    {
        public int Id { get; set; }
        public string CPNo { get; set; }
    }
    public class VoyageStartP
    {
        public int Id { get; set; }
        public string VoyageStarPoint { get; set; }
    }
    public class PortClass
    {
        public int Id { get; set; }
        public string PortName { get; set; }
        public string FacilityName { get; set; }

       
    }
    public class CargoNameClass
    {
        public int Id { get; set; }
        public string Cargo { get; set; }

    }
    public class FuelClass
    {
        public int Id { get; set; }
        public string FuelType { get; set; }
    }
}
