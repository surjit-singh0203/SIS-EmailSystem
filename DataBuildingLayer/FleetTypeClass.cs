using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{
    [Table("tblFleetType")]
    [MetadataType(typeof(FleetTypeMetadata))]
    public class FleetTypeClass
    {
        [Key]
        public int Tid { get; set; }

        [Display(Name = "Fleet Type")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fleet Type is Required")]
        public string FleetType { get; set; }
        public string AddedBy { get; set; }
    }
    public class FleetTypeMetadata
    {

        [Remote("checkftype", "Vesseldetails", "setting", ErrorMessage = "Fleet Type Already in use", AdditionalFields = "initialProductT")]
        public string FleetType { get; set; }
    }
}
