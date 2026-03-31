using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
    public partial class  CargoGradeClass
    {
        public int Id { get; set; }

        public int TotalCount { get; set; }
        public string Cargo { get; set; }

        public List<CargoGradeClass> GetCargoGradeList { get; set; }
    }
}
