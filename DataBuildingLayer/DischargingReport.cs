using System;
using System.ComponentModel.DataAnnotations;

namespace DataBuildingLayer
{
    public partial class DischargingReport: LoadingReport
    {
        
        public int? Power_Packs_onboard { get; set; }

        public int? Power_Packs_Used { get; set; }

        
    }


    public class DSCargoList
    {
        public int Id { get; set; }
        public int DSId { get; set; }
        public int VesselId { get; set; }
        public int CId { get; set; }
        public string CargoName { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? DischargeDatetime { get; set; }

        public string DDT { get; set; }
        public string EDT { get; set; }
        public string ADT { get; set; }
        public decimal? Terminal_Acceptable_Discharging_Rate { get; set; }
        public decimal? Discharging_pressure_Requested { get; set; }
        public decimal? Average_Discharge_Rate_ByVessel { get; set; }
        public decimal? Average_Discharge_pressure_ByVessel { get; set; }

        public int? No_of_Pumps_Use { get; set; }
        public int? No_Manifold_Hoses_by_Terminal { get; set; }

        public decimal? Size_of_Manifold_Hoses_by_Terminal { get; set; }
        //public int? Size_of_Manifold_Hoses_by_Terminal { get; set; }
        public int? No_Manifold_Hoses_by_Vessel { get; set; }

        public decimal? Size_of_Manifold_Hoses_by_Vessel { get; set; }
        // public int? Size_of_Manifold_Hoses_by_Vessel { get; set; }

        public decimal? Total_CargoDischarged { get; set; }
        //public int? Total_CargoDischarged { get; set; }

        public decimal? Balance_Cargo_ToBe_Deischarged { get; set; }
        // public int? Balance_Cargo_ToBe_Deischarged { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? EstCompDateTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ActualCompDateTime { get; set; }

        //---------------------------------------
        public int VoyageId { get; set; }
        public int LegPortId { get; set; }
        public string PortName { get; set; }
        //---------------------------------------
    }

    //public class StoppageList
    //{
    //    public int Id { get; set; }
    //    public int LRId { get; set; }
    //    public int VesselId { get; set; }
    //    public string Stoppage { get; set; }
    //    public string Reason { get; set; }
    //    public DateTime DateTimeFrom { get; set; }
    //    public DateTime DateTimeTo { get; set; }


    //}

    //public class Discharged_PumpUse : LR_Ballast_PumpUse
    //{
    //    public int DCId { get; set; }
    //    public int PumpUseId { get; set; }
    //    public bool LoadingDischarged { get; set; }

    //}
}
