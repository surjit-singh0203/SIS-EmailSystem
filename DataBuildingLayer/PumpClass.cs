using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class  PumpClass
    {
        public int Id { get; set; }
        public int PumpTypeId { get; set; }
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Capacity { get; set; }       
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public int PumpUseId { get; set; }
        public int VesselId { get; set; }
        public string PumpType { get; set; }
    }

    public class PumpType
    {
        public int Id { get; set; }
        public string Type { get; set; }
    }
}
