using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
   
    public partial class TanksAndHolds
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Height { get; set; }
        public decimal Capacity { get; set; }
        public int TanksTypeId { get; set; }
        public string Maintaining_Status { get; set; }
        public DateTime CreatedDate { get; set; }     
        public bool IsActive { get; set; }
        public int VesselId { get; set; }

    }
}
