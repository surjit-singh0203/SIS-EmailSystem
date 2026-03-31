using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class LoadingReport
    {
        public int TotalCount { get; set; }
        public int Id { get; set; }
        public int VoyageId { get; set; }
        public int LegPortId { get; set; }       
        public string PortName { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ReportDateTime { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ETDDateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftFwd { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftAft { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftMid { get; set; }
        // public decimal Ballast_ROB { get; set; }
        //public decimal BallastPumps_Rate1 { get; set; }
        //public decimal BallastPumps_Rate2 { get; set; }
        public string Remarks { get; set; }
        public int VesselId { get; set; }

      
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
        public bool IsActive { get; set; }

        //============ LOP Fields ==========
        public string Times { get; set; }
        public string Rate { get; set; }
        public string Hose_Connection { get; set; }
        public string High_H2S { get; set; }
        public bool SaveDraft { get; set; }
        //==============
    }

    public class CargoList
    {
        public int Id { get; set; }
        public int LRId { get; set; }
        public int VesselId { get; set; }
        public string CargoName { get; set; }
        public DateTime? LoadingDatetime { get; set; }
        public decimal TerminalLoadingRate { get; set; }
        public decimal LoadingRateAccepted { get; set; }
        public decimal AverageAchievedLoadingRate  { get; set; }
        public int No_Manifold_Hoses_by_Terminal { get; set; }
        //  public decimal No_Manifold_Hoses_by_Terminal { get; set; }
        public decimal Size_of_Manifold_Hoses_by_Terminal { get; set; }
        public int No_Manifold_Hoses_by_Vessel { get; set; }
       // public decimal No_Manifold_Hoses_by_Vessel { get; set; }
        public decimal Size_of_Manifold_Hoses_by_Vessel { get; set; }
        public decimal ShoreLineDistance { get; set; }
        public decimal QuantityOnboard { get; set; }
        public decimal BalanceQuantityLoaded { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? EstCompDateTime { get; set; }
        public DateTime? ActualCompDateTime { get; set; }
        //---------------------------------------
        public int VoyageId { get; set; }
        public int LegPortId { get; set; }
        public string PortName { get; set; }
        //---------------------------------------
    }

    public class StoppageList
    {
        public int Id { get; set; }
        public int LRId { get; set; }
        public int VesselId { get; set; }
        public string Stoppage { get; set; }
        public string Reason { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }
        public int DCId { get; set; }
        public bool LoadingDischarged { get; set; }
    }

    public class LR_DCR_PumpsUse
    {
        public int Id { get; set; }
        public int PumpId { get; set; }
        public int PumpUseId { get; set; }
        public int LRId { get; set; }
        public int DCRId { get; set; }
        public int VesselId { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }
        public string Name { get; set; }

    }
}
