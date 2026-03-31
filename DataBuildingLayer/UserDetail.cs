using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBuildingLayer
{
   public partial class UserDetail
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string FullName { get; set; }

        public DateTime CreatedDate { get; set; }
        public string UserType { get; set; }
        public string AssignVessel { get; set; }

        public int RankId { get; set; }
        public int DepId { get; set; }
        public string UserID { get; set; }
    }
}
