using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataBuildingLayer
{

    public partial class UserDetail    {
        // BerthTypeList = new List<SelectListItem>();
        SqlConnection cons = ConnectionBulder.con;        public UserDetail()        {            DepartmentWiseUserRole = GetDepartment();
            // RankList = GetRanksvalues(1);//new List<SelectListItem>();
            RankList = new List<SelectListItem>();            GetUserList = new List<UserDetail>();            VesselList = CommonClass.GetVesselList();
            //FleetNameList = GetFleetNameType("FleetName");
            //FleetTypeList = GetFleetNameType("FleetType");
            //UserTypeList = new List<SelectListItem>
            //{
            //    new SelectListItem { Text = "--Select--", Value = null },
            //   // new SelectListItem { Text = "Office", Value = "Office" },
            //    new SelectListItem { Text = "Vessel", Value = "Vessel" }
            //};
        }        public UserDetail EditUser { get; set; }        public List<UserDetail> GetUserList { get; set; }        public List<SelectListItem> DepartmentWiseUserRole { get; set; }        public List<SelectListItem> RankList { get; set; }        public List<SelectListItem> FixUserList { get; set; }        public List<SelectListItem> VesselList { get; set; }
        // public List<SelectListItem> UserTypeList { get; set; }
        public string ConfirmPassword { get; set; }        public string VesselName { get; set; }        public string Password { get; set; }        public int TotalCount { get; set; }
        public string RankName { get; set; }        public string UserName { get; set; }        public string DeptName { get; set; }        public int VesselID { get; set; }        public List<int> VesselIDs { get; set; }        public List<int> FTypeIDs { get; set; }        public List<int> FNameIDs { get; set; }        public List<SelectListItem> FleetTypeList { get; set; }        public List<SelectListItem> FleetNameList { get; set; }        public List<SelectListItem> GetDepartment()        {            List<SelectListItem> jst = new List<SelectListItem>();
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });
            using (SqlDataAdapter sda = new SqlDataAdapter("Select * from DepartmentWiseRole", cons))            {                DataTable tbl = new DataTable();                sda.Fill(tbl);                if (tbl.Rows.Count > 0)                {                    for (int i = 0; i < tbl.Rows.Count; i++)                    {                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["Departments"].ToString(), Value = tbl.Rows[i]["Id"].ToString() });                    }                }            }            return jst;        }

        public List<SelectListItem> GetFleetNameType(string Action)        {            List<SelectListItem> jst = new List<SelectListItem>();
            //jst.Add(new SelectListItem() { Text = "None Selected", Value = null });
            using (SqlDataAdapter sda = new SqlDataAdapter("spCommonBinding", cons))            {                sda.SelectCommand.CommandType = CommandType.StoredProcedure;                sda.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable tbl = new DataTable();                sda.Fill(tbl);                if (tbl.Rows.Count > 0)                {                    for (int i = 0; i < tbl.Rows.Count; i++)                    {                        jst.Add(new SelectListItem() { Text = tbl.Rows[i][1].ToString(), Value = tbl.Rows[i][0].ToString() });                    }                }            }            return jst;        }        public List<SelectListItem> GetRanksvalues(int Did)        {            List<SelectListItem> jst = new List<SelectListItem>();            using (SqlDataAdapter sda = new SqlDataAdapter("select * from Rank where Dep_Id=" + Did + "", cons))            {                DataTable tbl = new DataTable();                sda.Fill(tbl);                if (tbl.Rows.Count > 0)                {                    for (int i = 0; i < tbl.Rows.Count; i++)                    {

                        //jst.Add(new SelectListItem() { Text = tbl.Rows[i]["FixUser"].ToString(), Value = tbl.Rows[i]["Id"].ToString() });
                        jst.Add(new SelectListItem() { Text = tbl.Rows[i]["FixUser"].ToString(), Value = tbl.Rows[i]["FixUser"].ToString() });                    }                }            }            return jst;        }    }

    public partial class VesselDetail
    {

        SqlConnection cons = ConnectionBulder.con;
        public VesselDetail()
        {
            // DepartmentWiseUserRole = GetDepartment();

            //RankList = new List<SelectListItem>();
            GetVesselList = new List<VesselDetail>();

            FleetTypeList = fleetTypeList();
            FleetNameList = fleetNameList();
            VesselTradeList = vesselTradeList();
        }

        public string FleetName { get; set; }
        public string FleetType { get; set; }
        public string VesselTrade { get; set; }

        public List<VesselDetail> GetVesselList { get; set; }
        public List<FleetNameClass> FleetNameList { get; set; }
        public List<FleetTypeClass> FleetTypeList { get; set; }
        public List<VesselTradeClass> VesselTradeList { get; set; }


        public static List<FleetTypeClass> fleetTypeList()
        {
            List<FleetTypeClass> ftype = new List<FleetTypeClass>();
            //if (ConnectionBulder.con.State != ConnectionState.Open)
            //{
            //    ConnectionBulder.con.Open();
            //}
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "FleetType");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new FleetTypeClass
                    {
                        Tid = Convert.ToInt32(dt.Rows[i]["Tid"]),
                        FleetType = dt.Rows[i]["FleetType"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<FleetNameClass> fleetNameList()
        {
            List<FleetNameClass> fname = new List<FleetNameClass>();
            //if (ConnectionBulder.con.State != ConnectionState.Open)
            //{
            //    ConnectionBulder.con.Open();
            //}
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "FleetName");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    fname.Add(new FleetNameClass
                    {
                        Fid = Convert.ToInt32(dt.Rows[i]["Fid"]),
                        FleetName = dt.Rows[i]["FleetName"].ToString()
                    });
                }
                // con.Close();
            }

            return fname;
        }
        public static List<VesselTradeClass> vesselTradeList()
        {
            List<VesselTradeClass> vT = new List<VesselTradeClass>();
            //if (ConnectionBulder.con.State != ConnectionState.Open)
            //{
            //    ConnectionBulder.con.Open();
            //}
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VesselTrade");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    vT.Add(new VesselTradeClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["id"]),
                        VesselTrade = dt.Rows[i]["VesselTrade"].ToString()
                    });
                }
                // con.Close();
            }

            return vT;
        }

    }

    public partial class TanksType
    {
        SqlConnection cons = ConnectionBulder.con;
        public TanksType()
        {
            GetTanksTypeList = new List<TanksType>();
        }
        public List<TanksType> GetTanksTypeList { get; set; }
    }

    public partial class TanksAndHolds
    {
        SqlConnection cons = ConnectionBulder.con;
        public TanksAndHolds()
        {
            GetTanksandHoldList = new List<TanksAndHolds>();
        }
        public List<TanksAndHolds> GetTanksandHoldList { get; set; }
    }
    public partial class PumpClass
    {
        SqlConnection cons = ConnectionBulder.con;
        public PumpClass()
        {
            GetPumpList = new List<PumpClass>();
            PumpTypeList = pumpTypeList();
        }
        public List<PumpClass> GetPumpList { get; set; }
        public List<PumpType> PumpTypeList { get; set; }


        public static List<PumpType> pumpTypeList()
        {
            List<PumpType> ftype = new List<PumpType>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PumpType");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PumpType
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Type = dt.Rows[i]["Type"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }
    }

    public partial class DailyNoonReport
    {
        SqlConnection cons = ConnectionBulder.con;
        public DailyNoonReport()
        {
            GetNoonRList = new List<DailyNoonReport>();
            PortStatusList = portSList();
            //VoyageNumberList = voyageNList();
            //LegPortList = bindleg();
            LegPortList = new List<SelectListItem>();
            NREventsList = nonREventsList();
            CargoTanks = GetCargoTankList();
            BallastTanks = GetBallastTankList();
            Void_SpaceTanks = GetVoid_SpaceList();

            WindForceList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "0", Value = "0" },            new SelectListItem { Text = "1", Value = "1" },            new SelectListItem { Text = "2", Value = "2" },            new SelectListItem { Text = "3", Value = "3" },            new SelectListItem { Text = "4", Value = "4" },            new SelectListItem { Text = "5", Value = "5" },            new SelectListItem { Text = "6", Value = "6" },            new SelectListItem { Text = "7", Value = "7" },            new SelectListItem { Text = "8", Value = "8" },             new SelectListItem { Text = "9", Value = "9" },
     new SelectListItem { Text = "10", Value = "10" },
      new SelectListItem { Text = "11", Value = "11" },
       new SelectListItem { Text = "12", Value = "12" },        };            SeaStateList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Smooth", Value = "Smooth" },            new SelectListItem { Text = "Slight", Value = "Slight" },             new SelectListItem { Text = "Moderate", Value = "Moderate" },            new SelectListItem { Text = "Rough", Value = "Rough" },             new SelectListItem { Text = "Very Rough", Value = "Very Rough" },            new SelectListItem { Text = "High", Value = "High" },               new SelectListItem { Text = "Very High", Value = "Very High" }        };            OwChList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Owners", Value = "Owners" },            new SelectListItem { Text = "Charterers Acc", Value = "Charterers Acc" }        };
            NS = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "S", Value = "S" }        };
            EW = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "W", Value = "W" }        };

            MEControlLocList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Bridge", Value = "Bridge" },            new SelectListItem { Text = "ECR ", Value = "ECR " },            new SelectListItem { Text = "Man. Stn ", Value = "Man. Stn " }        };            WindDirectionList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "NNE", Value = "NNE" },             new SelectListItem { Text = "NE", Value = "NE" },            new SelectListItem { Text = "ENE", Value = "ENE" },             new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "ESE", Value = "ESE" },             new SelectListItem { Text = "SE", Value = "SE" },             new SelectListItem { Text = "SSE", Value = "SSE" },            new SelectListItem { Text = "S", Value = "S" },             new SelectListItem { Text = "SSW", Value = "SSW" },            new SelectListItem { Text = "SW", Value = "SW" },             new SelectListItem { Text = "WSW", Value = "WSW" },            new SelectListItem { Text = "W", Value = "W" },             new SelectListItem { Text = "WNW", Value = "WNW" },            new SelectListItem { Text = "NW", Value = "NW" },             new SelectListItem { Text = "NNW", Value = "NNW" },


        };
        }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BadWeather { get; set; }
        public List<SelectListItem> SeaStateList { get; set; }
        public List<SelectListItem> WindDirectionList { get; set; }

        public List<SelectListItem> MEControlLocList { get; set; }
        public List<SelectListItem> WindForceList { get; set; }

        public List<DNR_Void_Space> Void_SpaceTanks { get; set; }
        public List<FuelROB> Fuel_ROB { get; set; }
        public List<DNR_Cargo_Tank> CargoTanks { get; set; }
        public List<DNR_Ballast_Tank> BallastTanks { get; set; }
        public List<DailyNoonReport> GetNoonRList { get; set; }
        public List<PortStatus> PortStatusList { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
        //public List<VoyageClass> LegPortList { get; set; }
        public List<SelectListItem> LegPortList { get; set; }
        public List<SelectListItem> OwChList { get; set; }

        public List<SelectListItem> NS { get; set; }
        public List<SelectListItem> EW { get; set; }
        public List<NonRoutineEvents> NREventsList { get; set; }

        public int NREventsId { get; set; }
        public string OwChaterer { get; set; }

        public static List<VoyageClass> bindleg()
        {

            List<VoyageClass> ftype = new List<VoyageClass>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where voyageid='" + voyid + "'", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                // adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Leg = dt.Rows[i]["Leg"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Cargo_Tank> GetCargoTankList()
        {
            List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Cargo_Tank
                    {
                        CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Ullage = 0.00m,
                        Qty_MT = 0.00m,
                        Oxygen = 0.00m,
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Void_Space> GetVoid_SpaceList()
        {
            List<DNR_Void_Space> ftype = new List<DNR_Void_Space>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as Void_Space_Id, b.Name ,b.TanksTypeId from NR_Void_Space a right outer join TanksAndHolds b on  a.Void_Space_Id = b.Id  where b.TanksTypeId=4 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Void_Space
                    {
                        Void_Space_Id = Convert.ToInt32(dt.Rows[i]["Void_Space_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        //Sounding = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }




        public static List<DNR_Ballast_Tank> GetBallastTankList()
        {
            List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Ballast_Tank
                    {
                        BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,
                        Qty_Vol = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<PortStatus> portSList()
        {
            List<PortStatus> ftype = new List<PortStatus>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortStatus
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Status = dt.Rows[i]["Status"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<NonRoutineEvents> nonREventsList()
        {
            List<NonRoutineEvents> ftype = new List<NonRoutineEvents>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "NREvents");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new NonRoutineEvents
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        NR_Events = dt.Rows[i]["NR_Events"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<VoyageClass> voyageNList()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

    }

    public partial class ArrivalReport
    {
        SqlConnection cons = ConnectionBulder.con;
        public ArrivalReport()
        {
            GetArrivalRList = new List<ArrivalReport>();
            //PortStatusList = portSList();
            //VoyageNumberList = voyageNList();
            PortList = new List<SelectListItem>();
            //LegPortList = bindleg();
            LegPortList = new List<SelectListItem>();
            NREventsList = nonREventsList();
            //CargoTanks = GetCargoTankList();
            //BallastTanks = GetBallastTankList();
            //Void_SpaceTanks = GetVoid_SpaceList();
            WindForceList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "0", Value = "0" },            new SelectListItem { Text = "1", Value = "1" },            new SelectListItem { Text = "2", Value = "2" },            new SelectListItem { Text = "3", Value = "3" },            new SelectListItem { Text = "4", Value = "4" },            new SelectListItem { Text = "5", Value = "5" },            new SelectListItem { Text = "6", Value = "6" },            new SelectListItem { Text = "7", Value = "7" },            new SelectListItem { Text = "8", Value = "8" },             new SelectListItem { Text = "9", Value = "9" },            new SelectListItem { Text = "10", Value = "10" },            new SelectListItem { Text = "11", Value = "11" },            new SelectListItem { Text = "12", Value = "12" },        };            SeaStateList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Smooth", Value = "Smooth" },            new SelectListItem { Text = "Slight", Value = "Slight" },             new SelectListItem { Text = "Moderate", Value = "Moderate" },            new SelectListItem { Text = "Rough", Value = "Rough" },             new SelectListItem { Text = "Very Rough", Value = "Very Rough" },            new SelectListItem { Text = "High", Value = "High" },               new SelectListItem { Text = "Very High", Value = "Very High" }        };
            NS = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "S", Value = "S" }        };
            EW = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "W", Value = "W" }        };            OwChList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Owners", Value = "Owners" },            new SelectListItem { Text = "Charterers Acc", Value = "Charterers Acc" }        };            WindDirectionList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "NNE", Value = "NNE" },             new SelectListItem { Text = "NE", Value = "NE" },            new SelectListItem { Text = "ENE", Value = "ENE" },             new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "ESE", Value = "ESE" },             new SelectListItem { Text = "SE", Value = "SE" },               new SelectListItem { Text = "SSE", Value = "SSE" },            new SelectListItem { Text = "S", Value = "S" },             new SelectListItem { Text = "SSW", Value = "SSW" },            new SelectListItem { Text = "SW", Value = "SW" },             new SelectListItem { Text = "WSW", Value = "WSW" },            new SelectListItem { Text = "W", Value = "W" },             new SelectListItem { Text = "WNW", Value = "WNW" },            new SelectListItem { Text = "NW", Value = "NW" },             new SelectListItem { Text = "NNW", Value = "NNW" },


        };
        }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BadWeather { get; set; }
        public List<SelectListItem> SeaStateList { get; set; }
        public List<SelectListItem> WindDirectionList { get; set; }
        public List<SelectListItem> WindForceList { get; set; }

        public List<SelectListItem> PortList { get; set; }
        public List<ArrivalReport> GetArrivalRList { get; set; }

        public List<VoyageClass> VoyageNumberList { get; set; }
        //public List<VoyageClass> LegPortList { get; set; }
        public List<SelectListItem> LegPortList { get; set; }
        public List<SelectListItem> OwChList { get; set; }
        public List<NonRoutineEvents> NREventsList { get; set; }

        public List<SelectListItem> NS { get; set; }
        public List<SelectListItem> EW { get; set; }
        public int NREventsId { get; set; }
        public string OwChaterer { get; set; }

        public static List<VoyageClass> bindleg()
        {

            List<VoyageClass> ftype = new List<VoyageClass>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where voyageid='" + voyid + "'", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                // adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Leg = dt.Rows[i]["Leg"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }


        public static List<NonRoutineEvents> nonREventsList()
        {
            List<NonRoutineEvents> ftype = new List<NonRoutineEvents>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "NREvents");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new NonRoutineEvents
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        NR_Events = dt.Rows[i]["NR_Events"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<VoyageClass> voyageNList()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

    }


    public partial class DepartureReport
    {
        SqlConnection cons = ConnectionBulder.con;
        public DepartureReport()
        {
            GetDepRList = new List<DepartureReport>();
            PortList = new List<SelectListItem>();
            //VoyageNumberList = voyageNList();
            //LegPortList = bindleg();
            LegPortList = new List<SelectListItem>();
            NREventsList = nonREventsList();
            CargoTanks = GetCargoTankList();
            BallastTanks = GetBallastTankList();
            Void_SpaceTanks = GetVoid_SpaceList();
            WindForceList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "0", Value = "0" },            new SelectListItem { Text = "1", Value = "1" },            new SelectListItem { Text = "2", Value = "2" },            new SelectListItem { Text = "3", Value = "3" },            new SelectListItem { Text = "4", Value = "4" },            new SelectListItem { Text = "5", Value = "5" },            new SelectListItem { Text = "6", Value = "6" },            new SelectListItem { Text = "7", Value = "7" },            new SelectListItem { Text = "8", Value = "8" },             new SelectListItem { Text = "9", Value = "9" },            new SelectListItem { Text = "10", Value = "10" },            new SelectListItem { Text = "11", Value = "11" },            new SelectListItem { Text = "12", Value = "12" },        };            SeaStateList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Smooth", Value = "Smooth" },            new SelectListItem { Text = "Slight", Value = "Slight" },             new SelectListItem { Text = "Moderate", Value = "Moderate" },            new SelectListItem { Text = "Rough", Value = "Rough" },             new SelectListItem { Text = "Very Rough", Value = "Very Rough" },            new SelectListItem { Text = "High", Value = "High" },               new SelectListItem { Text = "Very High", Value = "Very High" }        };            OwChList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Owners", Value = "Owners" },            new SelectListItem { Text = "Charterers Acc", Value = "Charterers Acc" }        };            WindDirectionList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "NNE", Value = "NNE" },             new SelectListItem { Text = "NE", Value = "NE" },            new SelectListItem { Text = "ENE", Value = "ENE" },             new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "ESE", Value = "ESE" },             new SelectListItem { Text = "SE", Value = "SE" },              new SelectListItem { Text = "SSE", Value = "SSE" },            new SelectListItem { Text = "S", Value = "S" },             new SelectListItem { Text = "SSW", Value = "SSW" },            new SelectListItem { Text = "SW", Value = "SW" },             new SelectListItem { Text = "WSW", Value = "WSW" },            new SelectListItem { Text = "W", Value = "W" },             new SelectListItem { Text = "WNW", Value = "WNW" },            new SelectListItem { Text = "NW", Value = "NW" },             new SelectListItem { Text = "NNW", Value = "NNW" },


        };
        }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BadWeather { get; set; }
        public List<SelectListItem> SeaStateList { get; set; }
        public List<SelectListItem> WindDirectionList { get; set; }
        public List<SelectListItem> WindForceList { get; set; }

        public List<DNR_Void_Space> Void_SpaceTanks { get; set; }
        public List<DNR_Cargo_Tank> CargoTanks { get; set; }
        public List<DNR_Ballast_Tank> BallastTanks { get; set; }
        public List<DepartureReport> GetDepRList { get; set; }
        public List<SelectListItem> PortList { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
        //public List<VoyageClass> LegPortList { get; set; }
       // public List<VoyageClass1> VoyageClass1List { get; set; }
        
        public List<SelectListItem> LegPortList { get; set; }
        public List<SelectListItem> OwChList { get; set; }
        public List<NonRoutineEvents> NREventsList { get; set; }

        public int NREventsId { get; set; }
        public string OwChaterer { get; set; }

        public static List<VoyageClass> bindleg()
        {

            List<VoyageClass> ftype = new List<VoyageClass>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where voyageid='" + voyid + "'", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                // adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Leg = dt.Rows[i]["Leg"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Cargo_Tank> GetCargoTankList()
        {
            List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Cargo_Tank
                    {
                        CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Ullage = 0.00m,
                        Qty_MT = 0.00m,
                        Oxygen = 0.00m,
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Void_Space> GetVoid_SpaceList()
        {
            List<DNR_Void_Space> ftype = new List<DNR_Void_Space>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as Void_Space_Id, b.Name ,b.TanksTypeId from NR_Void_Space a right outer join TanksAndHolds b on  a.Void_Space_Id = b.Id  where b.TanksTypeId=4 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Void_Space
                    {
                        Void_Space_Id = Convert.ToInt32(dt.Rows[i]["Void_Space_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<DNR_Ballast_Tank> GetBallastTankList()
        {
            List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Ballast_Tank
                    {
                        BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,
                        Qty_Vol = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<PortStatus> portSList()
        {
            List<PortStatus> ftype = new List<PortStatus>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortStatus
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Status = dt.Rows[i]["Status"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<NonRoutineEvents> nonREventsList()
        {
            List<NonRoutineEvents> ftype = new List<NonRoutineEvents>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "NREvents");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new NonRoutineEvents
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        NR_Events = dt.Rows[i]["NR_Events"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<VoyageClass> voyageNList()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

    }

    public partial class BerthingReport
    {
        SqlConnection cons = ConnectionBulder.con;
        public BerthingReport()
        {
            GetBerthRList = new List<BerthingReport>();
            // PortList = portList();

            PortList = new List<SelectListItem>();
            //VoyageNumberList = voyageNList();
            PortStatusList = portSList();
            LegPortList = new List<SelectListItem>();
            FacilityList = new List<SelectListItem>();
            NREventsList = nonREventsList();
            CargoTanks = GetCargoTankList();
            BallastTanks = GetBallastTankList();
            Void_SpaceTanks = GetVoid_SpaceList();
            WindForceList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "0", Value = "0" },            new SelectListItem { Text = "1", Value = "1" },            new SelectListItem { Text = "2", Value = "2" },            new SelectListItem { Text = "3", Value = "3" },            new SelectListItem { Text = "4", Value = "4" },            new SelectListItem { Text = "5", Value = "5" },            new SelectListItem { Text = "6", Value = "6" },            new SelectListItem { Text = "7", Value = "7" },            new SelectListItem { Text = "8", Value = "8" },              new SelectListItem { Text = "9", Value = "9" },
     new SelectListItem { Text = "10", Value = "10" },
      new SelectListItem { Text = "11", Value = "11" },
       new SelectListItem { Text = "12", Value = "12" },        };            SeaStateList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Smooth", Value = "Smooth" },            new SelectListItem { Text = "Slight", Value = "Slight" },             new SelectListItem { Text = "Moderate", Value = "Moderate" },            new SelectListItem { Text = "Rough", Value = "Rough" },             new SelectListItem { Text = "Very Rough", Value = "Very Rough" },            new SelectListItem { Text = "High", Value = "High" },               new SelectListItem { Text = "Very High", Value = "Very High" }        };            OwChList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "Owners", Value = "Owners" },            new SelectListItem { Text = "Charterers Acc", Value = "Charterers Acc" }        };            WindDirectionList = new List<SelectListItem>        {            //new SelectListItem { Text = "--Select--", Value = null },            new SelectListItem { Text = "N", Value = "N" },            new SelectListItem { Text = "NNE", Value = "NNE" },             new SelectListItem { Text = "NE", Value = "NE" },            new SelectListItem { Text = "ENE", Value = "ENE" },             new SelectListItem { Text = "E", Value = "E" },            new SelectListItem { Text = "ESE", Value = "ESE" },             new SelectListItem { Text = "SE", Value = "SE" },             new SelectListItem { Text = "SSE", Value = "SSE" },            new SelectListItem { Text = "S", Value = "S" },             new SelectListItem { Text = "SSW", Value = "SSW" },            new SelectListItem { Text = "SW", Value = "SW" },             new SelectListItem { Text = "WSW", Value = "WSW" },            new SelectListItem { Text = "W", Value = "W" },             new SelectListItem { Text = "WNW", Value = "WNW" },            new SelectListItem { Text = "NW", Value = "NW" },             new SelectListItem { Text = "NNW", Value = "NNW" },


        };
        }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal BadWeather { get; set; }
        public List<SelectListItem> SeaStateList { get; set; }
        public List<SelectListItem> WindDirectionList { get; set; }
        public List<SelectListItem> WindForceList { get; set; }

        public List<DNR_Void_Space> Void_SpaceTanks { get; set; }
        public List<DNR_Cargo_Tank> CargoTanks { get; set; }
        public List<DNR_Ballast_Tank> BallastTanks { get; set; }
        public List<BerthingReport> GetBerthRList { get; set; }
        // public List<SelectListItem> PortList { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
        public List<PortStatus> PortStatusList { get; set; }
        public List<SelectListItem> LegPortList { get; set; }
        public List<SelectListItem> OwChList { get; set; }
        public List<NonRoutineEvents> NREventsList { get; set; }

        //public List<PortClass> PortList { get; set; }

        public List<SelectListItem> PortList { get; set; }
        public List<SelectListItem> FacilityList { get; set; }

        public int NREventsId { get; set; }
        public string OwChaterer { get; set; }

        public static List<PortClass> portList()
        {
            List<PortClass> ftype = new List<PortClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortClass
                    {
                        // Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        PortName = dt.Rows[i]["PortName"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<VoyageClass> bindleg()
        {

            List<VoyageClass> ftype = new List<VoyageClass>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where voyageid='" + voyid + "'", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                // adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Leg = dt.Rows[i]["Leg"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Cargo_Tank> GetCargoTankList()
        {
            List<DNR_Cargo_Tank> ftype = new List<DNR_Cargo_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as CargoTank_Id, b.Name ,b.TanksTypeId from NR_Cargo_Tank a right join TanksAndHolds b on  a.CargoTank_Id = b.Id  where b.TanksTypeId=1 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Cargo_Tank
                    {
                        CargoTank_Id = Convert.ToInt32(dt.Rows[i]["CargoTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Ullage = 0.00m,
                        Qty_MT = 0.00m,
                        Oxygen = 0.00m,
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<DNR_Void_Space> GetVoid_SpaceList()
        {
            List<DNR_Void_Space> ftype = new List<DNR_Void_Space>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as Void_Space_Id, b.Name ,b.TanksTypeId from NR_Void_Space a right outer join TanksAndHolds b on  a.Void_Space_Id = b.Id  where b.TanksTypeId=4 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Void_Space
                    {
                        Void_Space_Id = Convert.ToInt32(dt.Rows[i]["Void_Space_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<DNR_Ballast_Tank> GetBallastTankList()
        {
            List<DNR_Ballast_Tank> ftype = new List<DNR_Ballast_Tank>();
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct b.Id as BallastTank_Id, b.Name,b.TanksTypeId from NR_Ballast_Tank a right join TanksAndHolds b on  a.BallastTank_Id = b.Id  where b.TanksTypeId=3 and b.IsActive=1", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new DNR_Ballast_Tank
                    {
                        BallastTank_Id = Convert.ToInt32(dt.Rows[i]["BallastTank_Id"]),
                        TankName = dt.Rows[i]["Name"].ToString(),
                        Sounding = 0.00m,
                        Qty_Vol = 0.00m,

                    });
                }
                // con.Close();
            }

            return ftype;
        }
        public static List<PortStatus> portSList()
        {
            List<PortStatus> ftype = new List<PortStatus>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortStatus
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Status = dt.Rows[i]["Status"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<NonRoutineEvents> nonREventsList()
        {
            List<NonRoutineEvents> ftype = new List<NonRoutineEvents>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "NREvents");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new NonRoutineEvents
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        NR_Events = dt.Rows[i]["NR_Events"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<VoyageClass> voyageNList()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

    }


    public partial class DischargingReport
    {
        public DischargingReport()
        {
            DC_Cargo_PumpUseList = GetPumpsINUse(1);
        }

        public DSCargoList DSCargoClone { get; set; }

        public List<LR_DCR_PumpsUse> DC_Cargo_PumpUseList { get; set; }
        public List<DischargingReport> DischargingReportList { get; set; }
    }
    public partial class LoadingReport
    {

        public LoadingReport()
        {
            PortList = new List<SelectListItem>();
            //VoyageNumberList = voyageNList();

            CargoNameList = cargonameList();
            LegPortList = new List<SelectListItem>();
            CargoLists = new List<CargoList>();
            LR_Ballast_PumpUseList = GetPumpsINUse(2);
            StoppageLists = new List<StoppageList>();
            LOPList = new List<SelectListItem>
        {
            //new SelectListItem { Text = "--Select--", Value = null },
             new SelectListItem { Text = "Not Applicable", Value = "Not Applicable" },
            new SelectListItem { Text = "Issued", Value = "Issued" },
            new SelectListItem { Text = "Received", Value = "Received" },


        };
        }
        public string VoyageNumber { get; set; }
        public string LegPort_A { get; set; }


        public static List<CargoNameClass> cargonameList()
        {
            List<CargoNameClass> ftype = new List<CargoNameClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "CargoNameList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new CargoNameClass
                    {
                        // Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Cargo = dt.Rows[i]["Cargo"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public List<SelectListItem> LOPList { get; set; }
        public List<StoppageList> StoppageLists { get; set; }
        public List<CargoList> CargoLists { get; set; }
        public List<CargoNameClass> CargoNameList { get; set; }
        public List<LR_DCR_PumpsUse> LR_Ballast_PumpUseList { get; set; }
        public List<SelectListItem> PortList { get; set; }
        public List<VoyageClass> VoyageNumberList { get; set; }
        public List<SelectListItem> LegPortList { get; set; }       
        public static List<VoyageClass> voyageNList()
        {
            List<VoyageClass> ftype = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageNList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<LR_DCR_PumpsUse> GetPumpsINUse(int PumpUseId)
        {
            List<LR_DCR_PumpsUse> ftype = new List<LR_DCR_PumpsUse>();

            using (SqlDataAdapter adp = new SqlDataAdapter("select Id as PumpId,Name,Capacity,PumpUseId from tblpump where IsActive=1 and PumpUseId=" + PumpUseId + " ", ConnectionBulder.con))
            {
                // adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new LR_DCR_PumpsUse
                    {
                        PumpId = Convert.ToInt32(dt.Rows[i]["PumpId"]),
                        PumpUseId = Convert.ToInt32(dt.Rows[i]["PumpUseId"]),
                        Name = dt.Rows[i]["Name"].ToString(),
                        Rate = 0.00m,


                    });
                }
                // con.Close();
            }

            return ftype;
        }

        //------------- Use for Dynamic Cargo Text ------------
        public string CargoName { get; set; }
        public DateTime? LoadingDatetime { get; set; }
        // public int? TerminalLoadingRate { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? TerminalLoadingRate { get; set; }
        // public int? LoadingRateAccepted { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? LoadingRateAccepted { get; set; }
        //public int? AverageAchievedLoadingRate { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? AverageAchievedLoadingRate { get; set; }
        //public int? No_Manifold_Hoses_by_Terminal { get; set; }
        // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        // public decimal No_Manifold_Hoses_by_Terminal { get; set; }
        public int? No_Manifold_Hoses_by_Terminal { get; set; }
        //public int? Size_of_Manifold_Hoses_by_Terminal { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Size_of_Manifold_Hoses_by_Terminal { get; set; }
        //public int? No_Manifold_Hoses_by_Vessel { get; set; }
        // [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        // public decimal No_Manifold_Hoses_by_Vessel { get; set; }
        public int? No_Manifold_Hoses_by_Vessel { get; set; }
        //public int? Size_of_Manifold_Hoses_by_Vessel { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? Size_of_Manifold_Hoses_by_Vessel { get; set; }
        //public int? ShoreLineDistance { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? ShoreLineDistance { get; set; }
        //public int? QuantityOnboard { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? QuantityOnboard { get; set; }
        // public int? BalanceQuantityLoaded { get; set; }
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal? BalanceQuantityLoaded { get; set; }
        public DateTime? EstCompDateTime { get; set; }
        public DateTime? ActualCompDateTime { get; set; }

        //--------------Use for Stoppage List----------
        public string Stoppage { get; set; }
        public string Reason { get; set; }
        public DateTime? DateTimeFrom { get; set; }
        public DateTime? DateTimeTo { get; set; }

        //---------------------------------------------------

        public List<LoadingReport> LoadingReportList { get; set; }
    }

    public partial class VoyageClass
    {

        SqlConnection cons = ConnectionBulder.con;
        public VoyageClass()
        {

            GetVoyageList = new List<VoyageClass>();
            NorCList = norCList();
            // CPList = CharterPList();
            VoyagStartList = voyaSList();
            PortList = portList();
            FuelTypeList = fuelTList();
            VoyageLegList = new List<VoyageLeg>();
            FuelCList = new List<FuelConsumption>();
            PortStatusList = portSList();

        }
        public string CP_Consumption { get; set; }

        public string Leg { get; set; }
        public string FuelT { get; set; }
        public int VoyageId { get; set; }
        public int FuelTypeId { get; set; }

        public string LegPort_A { get; set; }

        public string LegPort_othersA { get; set; }
        public string ReasonforPortCall_A { get; set; }
        public string LegPort_B { get; set; }

        public string LegPort_othersB { get; set; }
        public string ReasonforPortCall_B { get; set; }

        public string ReasonA { get; set; }
        public string ReasonB { get; set; }
        //===========================================
        public decimal DTG { get; set; }
        public decimal CP_SOG { get; set; }
        public decimal CP_Log_Speed { get; set; }
        //===========================================
        public List<VoyageClass> GetVoyageList { get; set; }
        public List<VoyageClass> GetVoyageLegList { get; set; }
        public List<VoyageClass> GetFuelCList { get; set; }
        public List<NorCondition> NorCList { get; set; }
        public List<VoyageStartP> VoyagStartList { get; set; }
        public List<VoyageLeg> VoyageLegList { get; set; }
        public List<FuelConsumption> FuelCList { get; set; }
        public List<PortClass> PortList { get; set; }
        public List<FuelClass> FuelTypeList { get; set; }

        public List<CharterPartyC> CPList { get; set; }

        public static List<NorCondition> norCList()
        {
            List<NorCondition> ftype = new List<NorCondition>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "NorConditions");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new NorCondition
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Conditions = dt.Rows[i]["Conditions"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<CharterPartyC> CharterPList()
        {

            List<CharterPartyC> ftype = new List<CharterPartyC>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select a.id,a.cpno from CPContract a inner join CPpart1 b on a.Id=b.CPId where VesselID=123456", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "CPList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new CharterPartyC
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        CPNo = dt.Rows[i]["CPNo"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<VoyageStartP> voyaSList()
        {
            List<VoyageStartP> ftype = new List<VoyageStartP>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageStartPoint");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new VoyageStartP
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        VoyageStarPoint = dt.Rows[i]["VoyageStarPoint"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<FuelClass> fuelTList()
        {
            List<FuelClass> ftype = new List<FuelClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "Fuel");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new FuelClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        FuelType = dt.Rows[i]["FuelType"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public static List<PortClass> portList()
        {

            List<PortClass> ftype = new List<PortClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortClass
                    {
                        // Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        PortName = dt.Rows[i]["PortName"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public List<PortStatus> PortStatusList { get; set; }


        public static List<PortStatus> portSList()
        {
            List<PortStatus> ftype = new List<PortStatus>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortStatus
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        Status = dt.Rows[i]["Status"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

        public List<SpeedStatus> SpeedStatusList { get; set; }

    }

    public partial class BunkerReport
    {
        SqlConnection cons = ConnectionBulder.con;
        public BunkerReport()
        {
            PortList = portList();
            BunkerFuelTypeList = new List<SelectListItem>
        {
            new SelectListItem { Text = "VLSFO", Value = "5" },
            new SelectListItem { Text = "MDO", Value = "2" },
        };

        }

        public List<PortClass> PortList { get; set; }
        public List<BukerFuelList> B_FuelLists { get; set; }

        public List<SelectListItem> BunkerFuelTypeList { get; set; }

        public static List<PortClass> portList()
        {

            List<PortClass> ftype = new List<PortClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PortList");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ftype.Add(new PortClass
                    {
                        // Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        PortName = dt.Rows[i]["PortName"].ToString()
                    });
                }
                // con.Close();
            }

            return ftype;
        }

    }
}

