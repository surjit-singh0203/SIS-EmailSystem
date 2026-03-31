using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
   public class ExportPeram2
    {
        public ExportPeram2()
        {
            VesselList = new List<SelectListItem>();
            VesselIDs = new List<int>();
            SyncEmailList = new List<SyncEmailVesselsReport>();
        }
        public int VesselID { get; set; }
        public List<int> VesselIDs { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SelectListItem> VesselList { get; set; }
        public List<SyncEmailVesselsReport> SyncEmailList { get; set; }
    }
}
