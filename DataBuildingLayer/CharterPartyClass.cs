using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
    public class CharterPartyClass
    {
        public CharterPartyClass()
        {
            VesselList = new List<SelectListItem>();
            VoyageTypeList = new List<SelectListItem>
        {
            //new SelectListItem { Text = "--Select--", Value = null },
            new SelectListItem { Text = "TC", Value = "TC" },
            new SelectListItem { Text = "VC", Value = "VC" }
        };

        ConsumptionTypeList = new List<SelectListItem>
        {

            new SelectListItem { Text = "IDLING", Value = "IDLING" },
            new SelectListItem { Text = "LOADING", Value = "LOADING" },
             new SelectListItem { Text = "DISCHARGING", Value = "DISCHARGING" },
            new SelectListItem { Text = "GAS FREEING", Value = "GAS FREEING" },
             new SelectListItem { Text = "INERTING", Value = "INERTING" },
            new SelectListItem { Text = "TANK CLEANING", Value = "TANK CLEANING" },
             new SelectListItem { Text = "BALLAST EXCHANGE", Value = "BALLAST EXCHANGE" },
            new SelectListItem { Text = "HS/MANOUVERING PER DAY", Value = "HS/MANOUVERING PER DAY" },

             new SelectListItem { Text = "STOPPAGE AT SEA", Value = "STOPPAGE AT SEA" },
            new SelectListItem { Text = "DEVIATION", Value = "DEVIATION" },
             new SelectListItem { Text = "SLOW STEAMING", Value = "SLOW STEAMING" },
            new SelectListItem { Text = "BAD WEATHER", Value = "BAD WEATHER" },
             new SelectListItem { Text = "COT PREPARATION", Value = "COT PREPARATION" },
            new SelectListItem { Text = "CARGO HEATING", Value = "CARGO HEATING" },
             new SelectListItem { Text = "BW EXCHANGE", Value = "BW EXCHANGE" }
            
        };
            CPVesselInfo = new List<CPVessel>();
            MainCONSUMPTION = new List<CPMainCONSUMPTIONClass>();
            OtherCONSUMPTION = new List<CPOtherCONSUMPTIONClass>();
        }
        public List<SelectListItem> VesselList { get; set; }

        public List<SelectListItem> ConsumptionTypeList { get; set; }

        public List<SelectListItem> VoyageTypeList { get; set; }

        // For Create CP-ID Contract
        public int Id { get; set; }   // Main CPContrect ID
        public string CPNo { get; set; }
        public string CPName { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]

       
        public DateTime? StartDate { get; set; } 
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime? EndDate { get; set; } 
        public string VoyageType { get; set; }

        public string ConsType { get; set; }

        public string strtdt { get; set; }
        public string enddt { get; set; }


        public int VesselID { get; set; }
        public string VesselName { get; set; }
        //public DateTime StartDate { get; set; } = DateTime.Now.Date;
        //public DateTime EndDate { get; set; } = DateTime.Now.Date;
        //public string VoyageType { get; set; }

        public List<CPVessel> CPVesselInfo { get; set; }
        public List<CPMainCONSUMPTIONClass> MainCONSUMPTION { get; set; }

        public List<CPOtherCONSUMPTIONClass> OtherCONSUMPTION { get; set; }

        //public CPVessel EditCPVessels { get; set; }
        //public CPMainCONSUMPTIONClass EditMainConsumption { get; set; }
        //public CPOtherCONSUMPTIONClass EditOtherConsumption { get; set; }
    }

        public class CPVessel
        {
        public CPVessel()
        {
            VesselList1 = new List<SelectListItem>();
        }
        public int Id { get; set; }
        public int CPId { get; set; }
        public int VesselID { get; set; }
        public string VesselName { get; set; }
        public string CPNO { get; set; }

        public string VesselInCP { get; set; }
        public string CPName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now.Date;
        public DateTime EndDate { get; set; } = DateTime.Now.Date;
        public string VoyageType { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now.Date;
        public bool IsActive { get; set; }

        public List<SelectListItem> VesselList1 { get; set; }
        public int Svid { get; set; }

        public int TotalCount { get; set; }

        }

    public class CPMainCONSUMPTIONClass
    {
        public int Id { get; set; }
        public int CPId { get; set; }
        public decimal Speed { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal ME_LADEN { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal ME_BALLAST { get; set; }
        

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal AE_VLSFO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal AE_DO { get; set; }
    }

    public class CPOtherCONSUMPTIONClass
    {
        public int Id { get; set; }
        public int CPId { get; set; }
        public string Consumption { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal AE_VLSFO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal AE_MDO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:N3}")]
        public decimal BOILER_MDO { get; set; }

    }

    public class DailyNoonReportAllow
    {
        public int Id { get; set; }
        public string allow_Noondate { get; set; }
        public int VesselID { get; set; }
        public string VesselName { get; set; }
        public string userName { get; set; }
        public int TotalCount { get; set; }
        public List<SelectListItem> VesselList { get; set; }

        public List<DailyNoonReportAllow> ByPass_NDPList { get; set; }

    }
}
