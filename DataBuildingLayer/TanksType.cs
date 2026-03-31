using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class TanksType
    {
        public int Id { get; set; }
        public string TankType { get; set; }
        public bool MaintainingROBs { get; set; }
    }
}
