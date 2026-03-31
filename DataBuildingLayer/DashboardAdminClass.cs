using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
    public partial class DashboardAdminClass
    {
        public DashboardAdminClass()
        {
            //VesselList = new List<SelectListItem>(); //CommonClass.GetVesselList();
            VesselList = CommonClass.GetVesselList();
        }
        public int? Id { get; set; }
        public string VesselName { get; set; }
        public string ImoNo { get; set; }
        public string first { get; set; }
        public string second { get; set; }
        public string third { get; set; }
        public string fourth { get; set; }
        public string fifth { get; set; }
        public string sixth { get; set; }
        public string seventh { get; set; }
        public string eighth { get; set; }
        public string ninth { get; set; }
        public string tenth { get; set; }
        public string eleven { get; set; }
        public string twelve { get; set; }
        public string thirteen { get; set; }
        public string fourteen { get; set; }
        public string fifteen { get; set; }
        public string sixteen { get; set; }
        public string seventeen { get; set; }
        public string eighteen { get; set; }
        public string ninteen { get; set; }
        public string twenty { get; set; }
        public string twentyone { get; set; }
        public string twentytwo { get; set; }
        public string twentythree { get; set; }
        public string twentyfour { get; set; }
        public string twentyfive { get; set; }
        public string twentysix { get; set; }
        public string twentyseven { get; set; }
        public string twentyeight { get; set; }
        public string twentynine { get; set; }
        public string thirty { get; set; }
        public string thirtyone { get; set; }

        public string searchVessel { get; set; }

        public string VesselId { get; set; }
        public List<int> VesselIDs { get; set; }
        public string month { get; set; }
        public string year { get; set; }

        public List<SelectListItem> VesselList { get; set; }

        public List<DashboardAdminClass> DashoardList { get; set; }
        public List<DashboardAdminClass1> DashoardList1 { get; set; }

    }

    public partial class ReportList
    {
        public List<string> Reports { get; set; }
        public List<DateTime> Date { get; set; }
        public string ImoNo { get; set; }
        public string _Date { get; set; }

    }

    public partial class DashboardAdminClass1
    {
        public DashboardAdminClass1()
        {
            //VesselList = new List<SelectListItem>(); //CommonClass.GetVesselList();
            VesselList = CommonClass.GetVesselList();
        }
        public int? Id { get; set; }
        public string VesselName { get; set; }
        public string ImoNo { get; set; }
        public string Date { get; set; }
        public string Report_Type { get; set; }
        public string searchVessel { get; set; }

        public string VesselId { get; set; }
        public List<int> VesselIDs { get; set; }
        public string month { get; set; }
        public string year { get; set; }


        public List<SelectListItem> VesselList { get; set; }

        public List<DashboardAdminClass1> DashoardList1 { get; set; }

    }
}
