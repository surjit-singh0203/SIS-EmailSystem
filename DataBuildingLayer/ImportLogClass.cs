using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public class ImportLgClass
    {
        public string VesselName { get; set; }
        public string VoyageNo { get; set; }
        public DateTime? DataAvailabilityTillDate { get; set; }
        public DateTime? ExportedDate { get; set; }
        public string ExportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
        public string ImportedBy { get; set; }

        public List<ImportLgClass> ImportLogInfo { get; set; }

        public int TotalCount { get; set; }
    }

    public class ErrorLogClass
    {
        public int? Id { get; set; }
        public string SheetName { get; set; }     
        public DateTime? ImportDate { get; set; }
        public List<ErrorLogClass> ErrorLogInfo { get; set; }
        public int TotalCount { get; set; }
    }
}
