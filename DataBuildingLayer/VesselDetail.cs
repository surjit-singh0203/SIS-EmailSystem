using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class VesselDetail
    {
        public int Id { get; set; }

        [Display(Name = "Vessel Id")]
        [NotMapped]
        public int VesselID { get; set; }

        [Display(Name = "Vessel Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Vessel name is required")]
        public string VesselName { get; set; }

        [Display(Name = "IMO Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "IMO number is required")]
        [RegularExpression(@"[0-9]*\.?[0-9]+", ErrorMessage = "IMO number must be numeric")]
        public int? ImoNo { get; set; }       

        [Display(Name = "Fleet Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fleet name is required")]
        public int FleetNameID { get; set; }

        [Display(Name = "Fleet Type")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Fleet Type is required")]
        public int FleetTypeID { get; set; }

        [Display(Name = "Trade Area")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Trade Area is required")]
        public int VesselTradeID { get; set; }

       

        public DateTime CreatedDate { get; set; }

        public decimal Displacement { get; set; }

        public bool IsActive { get; set; }

        public int TotalCount { get; set; }

    }
}
