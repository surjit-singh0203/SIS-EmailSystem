using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
   public class VesselTradeClass
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Vessel Trade")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Vessel Trade is Required")]
        public string VesselTrade { get; set; }
        public string AddedBy { get; set; }
    }
}
