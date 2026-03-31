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
    public partial class DNVGLReport
    {
        public List<int> VesselIDs { get; set; }
        public int TotalCount { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
       // public List<VesselClass> VesselList { get; set; }
        public List<SelectListItem> VesselList { get; set; }

    }

    //public class VesselClass
    //{
    //    public int Ship_CD { get; set; }
    //    public string ship_name { get; set; }
    //}

}
