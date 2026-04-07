using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class DailyNoonReport
    {
        public int TotalCount { get; set; }
        public int Id { get; set; }
        public int VoyageId { get; set; }
        public int LegPortId { get; set; }
        public string VesselStatus { get; set; }
        public string AtSeaOrPort { get; set; }
        public int? PortStatus { get; set; }
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? Date { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? ETA { get; set; }
        public string NPOC { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftFwd { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftAft { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DraftMid { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Displacement { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? NoonToNoonDMG_Dist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LogDist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? EngineDist { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? TotalDistance { get; set; }
        public decimal? StmgTime { get; set; }
        public decimal? TotalTime { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? DistToGo_DTG { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? CP_Speed { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Act_Speed { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Gen_Avg_Speed { get; set; }
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
        public string ME_ControlLoc { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SCAV_ManiPress { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SCAV_Temp { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Max_Exhaust_Temp { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Min_Exhaust_Temp { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? SW_Temp { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ER_Temp { get; set; }
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
      //  [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_RungHrs_No1 { get; set; }
      //  [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_RungHrs_No2 { get; set; }
      //  [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BR_RungHrs_No1 { get; set; }
      // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BR_RungHrs_No2 { get; set; }
       // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_RungHrs_No3 { get; set; }
      //  [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_RungHrs_No4 { get; set; }
       // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_RungHrs_ShaftGen { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_Load_No1 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_Load_No2 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_Load_No3 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_Load_No4 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AE_Load_ShaftGen { get; set; }
        public string AE_Extra_Run_Reason { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public string BR_Extra_Run_Reason1 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public string BR_Extra_Run_Reason2 { get; set; }
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
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Ballast_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Generated { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_Consumption { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FW_ROB { get; set; }
        public decimal? F_ROB_HFO { get; set; }
        public decimal? F_ROB_MDO { get; set; }
        public decimal? F_ROB_LSHFO { get; set; }
        public decimal? F_ROB_LSMFO { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ER_Bilge_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ER_Sludge_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ER_WasteOil_ROB { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? PumpRoomMaxSounding { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ChainLocker1 { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ChainLocker2 { get; set; }

        public string Remarks { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? FuelRob { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BunkerReceipt { get; set; }

        public decimal EOSP_ROB { get; set; }
        public decimal FWE_ROB { get; set; }

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
        public string VesselName { get; set; }
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

        public decimal IGG_VLSFO { get; set; }
        public decimal IGG_MDO { get; set; }
        public decimal Incinerator_VLSFO { get; set; }
        public decimal Incinerator_MDO { get; set; }

        public decimal StopageAtSea_MDO { get; set; }
        public decimal Deviation_MDO { get; set; }
        public decimal SlowSteaming_MDO { get; set; }
        public decimal BadWeather_MDO { get; set; }
        public decimal COTPrep_MDO { get; set; }
        public decimal CargoHeating_MDO { get; set; }
        public decimal BWExchange_MDO { get; set; }
        public decimal Others_MDO { get; set; }

        public string voyagenumber { get; set; }

        public string dateF { get; set; }

        //---------------- New Column Add in V2 -- Date 25 May 2022 --------///////////////       


        //-------------------------------------------------------------------/////////

        public int CPID { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }

        public int VesselId { get; set; }

        public bool SaveDraft { get; set; }

        public string Lat1 { get; set; }
        public string Lat2 { get; set; }
        public string Lat3 { get; set; }
        public string Long1 { get; set; }
        public string Long2 { get; set; }
        public string Long3 { get; set; }


        //For NR_Cargo
        //
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BL_Qty { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal LoadPortalActual { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal TodaysActual { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Qty_Diff { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public string Reasonfor_Qty_Diff { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Cargo_Temp { get; set; }

        public string  LegPortName { get; set; }

        public string Status { get; set; }

        public int HiddenTotalSum { get; set; }

        public string CPNo  { get; set; }


    }

    public class FCons
    {
        public string FuelType { get; set; }
        public decimal ME_CP { get; set; }
        public decimal ME_ACT_SEA { get; set; }
        public decimal ME_ACT_MAN { get; set; }
        public decimal ME_ACT_WAIT { get; set; }
        public decimal ME_ACT_BERTH { get; set; }
        public decimal AE_CP { get; set; }
        public decimal AE_ACT_SEA { get; set; }
        public decimal AE_ACT_MAN { get; set; }
        public decimal AE_ACT_WAIT { get; set; }

        public decimal AE_ACT_BERTH { get; set; }
        public decimal BLR_ACT_SEA { get; set; }
        public decimal BLR_ACT_MAN { get; set; }
        public decimal BLR_ACT_WAIT { get; set; }
        public decimal BLR_ACT_BERTH { get; set; }
        public decimal FRAMO_ACT_SEA { get; set; }
        public decimal FRAMO_ACT_MAN { get; set; }
        public decimal FRAMO_ACT_WAIT { get; set; }
        public decimal FRAMO_ACT_BERTH { get; set; }

        public decimal IGG { get; set; }
        public decimal StopageAtSea { get; set; }
        public decimal Deviation { get; set; }
        public decimal SlowSteaming { get; set; }
        public decimal BadWeather { get; set; }
        public decimal BWExchange { get; set; }
        public decimal COTPrep { get; set; }
        public decimal CargoHeating { get; set; }
        public decimal Others { get; set; }
        public decimal Incinerator { get; set; }

        //public decimal AE_ACT_IDLING_VLSFO { get; set; }
        //public decimal AE_ACT_Loading_VLSFO { get; set; }
        //public decimal AE_ACT_Discharging_VLSFO { get; set; }

        //public decimal BLR_ACT_IDLING_VLSFO { get; set; }
        //public decimal BLR_ACT_Loading_VLSFO { get; set; }
        //public decimal BLR_ACT_Discharging_VLSFO { get; set; }

        //public decimal FRAMO_ACT_IDLING_VLSFO { get; set; }
        //public decimal FRAMO_ACT_Loading_VLSFO { get; set; }
        //public decimal FRAMO_ACT_Discharging_VLSFO { get; set; }


    }

    public class PortStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class SpeedStatus
    {
        public int Id { get; set; }
        public string Speed { get; set; }
    }

    public class Fuel_Cons_NR
    {
        public int Id { get; set; }
        public int Noon_Report_Id { get; set; }
        public int FuelTypeId { get; set; }
        public int ConsTypeId { get; set; }
        public decimal Value { get; set; }
    }

    public class NonRoutineEvents
    {
        public int Id { get; set; }
        public string NR_Events { get; set; }
    }


    public class DNR_Cargo_Tank
    {
        public int Id { get; set; }
        public int CargoTank_Id { get; set; }
        public int DailyNoonReport_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Ullage { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Qty_MT { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Oxygen { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal H2S { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal HC { get; set; }

        public string TankName { get; set; }
        public string Height { get; set; }
        public string Capacity { get; set; }
        public string TanksTypeId { get; set; }
        public int SortingOrder { get; set; }
    }



    public class DNR_Ballast_Tank
    {
        public int Id { get; set; }
        public int BallastTank_Id { get; set; }
        public int DailyNoonReport_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Sounding { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Qty_Vol { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal HC { get; set; }

        public string TankName { get; set; }
        public string Height { get; set; }
        public string Capacity { get; set; }
        public string TanksTypeId { get; set; }
        public int SortingOrder { get; set; }
    }

    public class DNR_Void_Space
    {
        public int Id { get; set; }
        public int Void_Space_Id { get; set; }
        public int DailyNoonReport_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal Sounding { get; set; }

        public string TankName { get; set; }
        public string Height { get; set; }
        public string Capacity { get; set; }
        public string TanksTypeId { get; set; }
        public int SortingOrder { get; set; }
    }

    public class NonRoutineEventClass
    {

        public string ChartererAccount { get; set; }
        public string Hours { get; set; }

        //public string StopageAtSea { get; set; }
        //public string Deviation { get; set; }
        //public string SlowSteaming { get; set; }
        //public string BadWeather { get; set; }      
        //public string COTPrep { get; set; }
        //public string CargoHeating { get; set; }
        //public string BWExchange { get; set; }



    }

}
