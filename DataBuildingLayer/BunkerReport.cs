using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DataBuildingLayer
{
    public partial class BunkerReport
    {
        public int TotalCount { get; set; }
        public string Supplier { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? BargeAlongside { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? BunkerHoseConnected { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? CommencedBunkering { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? BunkeringCompleted { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? BunkerHosedisconnected { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTime? BargeCastOff { get; set; }
        public string BargeName { get; set; }
        public string Remarks { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Id { get; set; }
        public string PortName { get; set; }
        public string PortName_others { get; set; }
        public int VoyageId { get; set; }
        public int VesselId { get; set; }
        public string voyagenumber { get; set; }
        public string File_Path { get; set; }
        public int? Fuel_type_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BDN { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Fuel_Density { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Sulphur_content { get; set; }
        public string BDN_Number { get; set; }
        public HttpPostedFileBase LabAnalysisReport { get; set; }
        public string LabAnalysisReport_Name { get; set; }
        public string fileExtension { get; set; }
        public List<BunkerReport> BunkerReportList { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
       
    }

    public class BukerFuelList
    {
        public int Id { get; set; }
        public int VesselId { get; set; }
        public string Fuel_type { get; set; }
        public int MaxR_Id { get; set; }
        public int? Fuel_type_Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BDN { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Fuel_Density { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Sulphur_content { get; set; }
        public string BDN_Number { get; set; }

    }
}
