using System;
using System.Collections.Generic;
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

            new SelectListItem { Text = "AT SEA", Value = "AT SEA" },
            new SelectListItem { Text = "MANOEUV", Value = "MANOEUV" },
             new SelectListItem { Text = "ANCHOR/WAIT", Value = "ANCHOR/WAIT" },
            new SelectListItem { Text = "BERTH", Value = "BERTH" },
             new SelectListItem { Text = "LOADING", Value = "LOADING" },
            new SelectListItem { Text = "DISCHARGING", Value = "DISCHARGING" }
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
        public DateTime StartDate { get; set; } = DateTime.Now.Date;
        public DateTime EndDate { get; set; } = DateTime.Now.Date;
        public string VoyageType { get; set; }

        public string ConsType { get; set; }

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
        public decimal ME_LADEN { get; set; }
        public decimal ME_BALLAST { get; set; }
        public decimal AE_VLSFO { get; set; }
        public decimal AE_DO { get; set; }
    }

    public class CPOtherCONSUMPTIONClass
    {
        public int Id { get; set; }
        public int CPId { get; set; }
        public string Consumption { get; set; }
        public decimal AE_VLSFO { get; set; }
        public decimal AE_MDO { get; set; }
        public decimal BOILER_MDO { get; set; }

    }
}
