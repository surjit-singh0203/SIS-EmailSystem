using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class ArrivalReport
    {
        public int TotalCount { get; set; }
        public int Id { get; set; }
        public int VoyageId { get; set; }

        public string PortName { get; set; }
        public int LegPortId { get; set; }
        public string Place { get; set; }
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? EOSP { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftFwd { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftMid { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftAft { get; set; }      
        public string Latitude { get; set; }
        public string Longitude { get; set; }
       // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? NOR { get; set; }
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ETB { get; set; }
        public string Anchor_Name { get; set; }
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? Anchor_DateT { get; set; }
        public string AnchorPos_Latitude { get; set; }
        public string AnchorPos_Longitude { get; set; }
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? AnchorFWE_DateT { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? NoonToNoonDMG_Dist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LogDist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? EngineDist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? TotalDistance { get; set; }
       // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? StmgTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DistToGo_DTG { get; set; }
        public decimal CP_Speed { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Act_Speed { get; set; }
        public decimal GAS { get; set; }
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
        public decimal? SLOPS_ROB_OXY_Oil { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SLOPS_ROB_OXY_Water { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SLOPS_ROB_OXY_Total { get; set; }
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
        public string Ballast_ROB { get; set; }



        public string Manoeuvring_Hrs { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Manoeuvring_Distance { get; set; }
        

        public string Remarks { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? EOSP_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FWE_ROB { get; set; }

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

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Qty_Grade1 { get; set; }
        public decimal Qty_Grade2 { get; set; }
        public bool SaveDraft { get; set; }

        public string Lat1 { get; set; }
        public string Lat2 { get; set; }
        public string Lat3 { get; set; }
        public string Long1 { get; set; }
        public string Long2 { get; set; }
        public string Long3 { get; set; }

        public string ALat1 { get; set; }
        public string ALat2 { get; set; }
        public string ALat3 { get; set; }
        public string ALong1 { get; set; }
        public string ALong2 { get; set; }
        public string ALong3 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Generated { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Consumption { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Gen_Avg_Speed { get; set; }
       // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? TotalTime { get; set; }
        public int HiddenTotalSum { get; set; }
    }

    public class FuelROB
    {
        public string FuelType { get; set; }
        public decimal EOSP_ROB { get; set; }
        public decimal FWE_ROB { get; set; }
        public decimal SBE_ROB { get; set; }
        public decimal RFA_ROB { get; set; }

        public decimal Rec { get; set; }
    }
    public class NRCargo
    {
        public int VesselId { get; set; }
        public int LR_Cargo_Id { get; set; }
        public string CargoName { get; set; }
        public decimal BL_Qty { get; set; }
        public decimal LoadPortalActual { get; set; }
        public decimal TodaysActual { get; set; }
        public decimal Qty_Diff { get; set; }
        public string Reasonfor_Qty_Diff { get; set; }
        public decimal Cargo_Temp { get; set; }
        public int VoyId { get; set; }
        public int Id { get; set; }


    }

    public class ARCargo
    {
        public int VesselId { get; set; }
        public int LR_Cargo_Id { get; set; }
        public string CargoName { get; set; }
        public decimal Qty_Grade1 { get; set; }
        public decimal Qty_Grade2 { get; set; }
        public int VoyId { get; set; }

    }
    public class BRCargo
    {
        public int VesselId { get; set; }
        public int LR_Cargo_Id { get; set; }
        public string CargoName { get; set; }
        public decimal Qty_Grade1 { get; set; }
        public decimal Qty_Grade2 { get; set; }
        public int BerthingReport_Id { get; set; }
        public int VoyId { get; set; }

    }
    public class DRCargo
    {
        public int VesselId { get; set; }
        public int LR_Cargo_Id { get; set; }
        public string CargoName { get; set; }
        public decimal BL_Qty { get; set; }
        public decimal LoadPortalActual { get; set; }
        public DateTime? Completion_DateT { get; set; }
        public decimal Rate { get; set; }
        public int VoyId { get; set; }

        public decimal Cargo_Temp { get; set; }
    }
}
