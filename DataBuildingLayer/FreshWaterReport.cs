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
    public partial class FreshWaterReport
    {
        public int TotalCount { get; set; }
        public int Id { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime? Received_Date { get; set; }

        public string PortName { get; set; }
        public string PortName_others { get; set; }
        public int VesselId { get; set; }
        public string Facility_Name { get; set; }
        public string VendorDetails { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Intial_Meter_Reading_MT_supplied { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Final_Meter_Reading_MT { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Difference_in_Meter_Reading_MT { get; set; }

        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? QTY_supplied_MT { get; set; }
        public HttpPostedFileBase FreshWater_File { get; set; }
        public string File_Name { get; set; }
        public string fileExtension { get; set; }
        public string File_Path { get; set; }
        public List<FreshWaterReport> FWReportList { get; set; }
        // public List<VoyageClass> VoyageNumberList { get; set; }

    }


}
