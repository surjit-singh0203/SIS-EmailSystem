using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class DepartureReport
    {
        public int TotalCount { get; set; }
        public int Id { get; set; }
        public int VoyageId { get; set; }
        public int DepLegPortId { get; set; }
        public int NextLegPortId { get; set; }
        public string DeparturePort { get; set; }
        public string NextPort { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ETA { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Date only")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ReportDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftFwd { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftAft { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftMid { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Ballast_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsDisposed_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsDisposed_Water { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsDisposed_Total { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsROB_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsROB_Water { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SlopsROB_Total { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesDisposed_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesDisposed_Water { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesDisposed_Total { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesROB_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesROB_Water { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BilgesROB_Total { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Sludge { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? GarbagePlastic { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? GarbageOthers { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OtherDisposal { get; set; }

        public decimal? Manoeuvring_Hrs { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Manoeuvring_Distance { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? SBE_DateT { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? RFA_DateT { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SBE_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? RFA_ROB { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Generated { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Consumption { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_ROB { get; set; }


        public string SeaState { get; set; }
        public string WindDirection { get; set; }
        public string WindForce { get; set; }
        public string SwellDirection { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SwellHeight { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? WaveLength { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? WaveHeight { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Slip { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? RPM { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BHP { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? MCR { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_MECC { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_MECYL { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]  
        public decimal? LO_HO_Cons_AECC { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_HYDR_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_MECC_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_MECYL_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_AECC_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LO_HO_Cons_HYDR_Oil_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Bunker_LO_Rec_MECC { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Bunker_LO_Rec_MECYL { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Bunker_LO_Rec_AECC { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Bunker_LO_Rec_HYDR_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_OXY_Full { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_OXY_InUse { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_OXY_Empty { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_ACYT_Full { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_ACYT_InUse { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? OT_ROB_ACYT_Empty { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BunkerReceipt { get; set; }

        public string CTM { get; set; }
        public string Spares { get; set; }
        public string Stores { get; set; }
        public string RepairsConducted { get; set; }
        public int? CrewChange { get; set; }
        public string ItemsLanded { get; set; }
        public string Remarks { get; set; }

        public decimal ME_CP_VLSFO { get; set; }
        public decimal ME_CP_HFO { get; set; }
        public decimal ME_CP_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal ME_ACT_SEA_VLSFO { get; set; }
        public decimal ME_ACT_SEA_HFO { get; set; }
        public decimal ME_ACT_SEA_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal ME_ACT_MAN_VLSFO { get; set; }
        public decimal ME_ACT_MAN_HFO { get; set; }
        public decimal ME_ACT_MAN_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal ME_ACT_WAIT_VLSFO { get; set; }
        public decimal ME_ACT_WAIT_HFO { get; set; }
        public decimal ME_ACT_WAIT_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal ME_ACT_BERTH_VLSFO { get; set; }
        public decimal ME_ACT_BERTH_HFO { get; set; }
        public decimal ME_ACT_BERTH_MGO { get; set; }

        public decimal AE_CP_VLSFO { get; set; }
        public decimal AE_CP_HFO { get; set; }
        public decimal AE_CP_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal AE_ACT_SEA_VLSFO { get; set; }
        public decimal AE_ACT_SEA_HFO { get; set; }
        public decimal AE_ACT_SEA_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal AE_ACT_MAN_VLSFO { get; set; }
        public decimal AE_ACT_MAN_HFO { get; set; }
        public decimal AE_ACT_MAN_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal AE_ACT_WAIT_VLSFO { get; set; }
        public decimal AE_ACT_WAIT_HFO { get; set; }
        public decimal AE_ACT_WAIT_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal AE_ACT_BERTH_VLSFO { get; set; }
        public decimal AE_ACT_BERTH_HFO { get; set; }
        public decimal AE_ACT_BERTH_MGO { get; set; }

        public decimal? BLR_CP_VLSFO { get; set; }
        public decimal? BLR_CP_HFO { get; set; }
        public decimal? BLR_CP_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BLR_ACT_SEA_VLSFO { get; set; }
        public decimal BLR_ACT_SEA_HFO { get; set; }
        public decimal BLR_ACT_SEA_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BLR_ACT_MAN_VLSFO { get; set; }
        public decimal BLR_ACT_MAN_HFO { get; set; }
        public decimal BLR_ACT_MAN_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BLR_ACT_WAIT_VLSFO { get; set; }
        public decimal BLR_ACT_WAIT_HFO { get; set; }
        public decimal BLR_ACT_WAIT_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BLR_ACT_BERTH_VLSFO { get; set; }
        public decimal BLR_ACT_BERTH_HFO { get; set; }
        public decimal BLR_ACT_BERTH_MGO { get; set; }

        public decimal? FRAMO_CP_VLSFO { get; set; }
        public decimal? FRAMO_CP_HFO { get; set; }
        public decimal? FRAMO_CP_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal FRAMO_ACT_SEA_VLSFO { get; set; }
        public decimal FRAMO_ACT_SEA_HFO { get; set; }
        public decimal FRAMO_ACT_SEA_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal FRAMO_ACT_MAN_VLSFO { get; set; }
        public decimal FRAMO_ACT_MAN_HFO { get; set; }
        public decimal FRAMO_ACT_MAN_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal FRAMO_ACT_WAIT_VLSFO { get; set; }
        public decimal FRAMO_ACT_WAIT_HFO { get; set; }
        public decimal FRAMO_ACT_WAIT_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal FRAMO_ACT_BERTH_VLSFO { get; set; }
        public decimal FRAMO_ACT_BERTH_HFO { get; set; }
        public decimal FRAMO_ACT_BERTH_MGO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal IGG { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Incinerator { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal StopageAtSea { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Deviation { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal SlowSteaming { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BWExchange { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal COTPrep { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal CargoHeating { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Others { get; set; }





        public string voyagenumber { get; set; }


        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }

        public int VesselId { get; set; }


        //For NR_Cargo
        //
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BL_Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal LoadPortalActual { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Cargo_Temp { get; set; }
      

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? Completion_DateT { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Rate { get; set; }

        public bool SaveDraft { get; set; }

    }
}
