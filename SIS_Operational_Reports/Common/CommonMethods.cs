using DataBuildingLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;


namespace SIS_Operational_Reports.Common
{
    public class CommonMethods
    {
        /// <summary>Excel export date format - matches edit page (yyyy-MM-dd).</summary>
        public const string ExcelDateFormat = "yyyy-MM-dd";
        /// <summary>Excel export datetime format - matches edit page flatpickr (yyyy-MM-dd HH:mm).</summary>
        public const string ExcelDateTimeFormat = "yyyy-MM-dd HH:mm";

        //SqlConnection con = ConnectionBulder.con;
        const string ConnectionName = "ShipmentContaxt";

        public static int PAGESIZE
        {
            get
            {
                string pageSize = System.Configuration.ConfigurationManager.AppSettings["pageSize"];

                if (string.IsNullOrEmpty(pageSize))
                    return 3;
                else
                    return Convert.ToInt32(pageSize);
            }
        }

        public static void InsertUpdateUD(UserDetail userD, string Action)
        {
            try
            {
               
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateDeleteUD", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", userD.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@UserEmail", userD.UserName);
                    adapter.SelectCommand.Parameters.AddWithValue("@fullname", userD.UserName);
                    adapter.SelectCommand.Parameters.AddWithValue("@createddate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@UserType", userD.UserType);
                    adapter.SelectCommand.Parameters.AddWithValue("@AssignVessel", userD.VesselID.ToString());
                    adapter.SelectCommand.Parameters.AddWithValue("@rankid", userD.RankId);
                    adapter.SelectCommand.Parameters.AddWithValue("@depid", userD.DepId);
                    if (Action == "Delete")
                        adapter.SelectCommand.Parameters.AddWithValue("@userid", "");
                    else
                        //adapter.SelectCommand.Parameters.AddWithValue("@userid", GetUserByEmailID(userD.UserName + userD.VesselID));
                        adapter.SelectCommand.Parameters.AddWithValue("@userid", GetUserByEmailID(userD.UserEmail));
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);

                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void InsertUpdateVessel(VesselDetail vesselD, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateVessel", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", vesselD.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselName", vesselD.VesselName);
                    adapter.SelectCommand.Parameters.AddWithValue("@ImoNo", vesselD.ImoNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@FleetNameID", vesselD.FleetNameID);
                    adapter.SelectCommand.Parameters.AddWithValue("@FleetTypeID", vesselD.FleetTypeID);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselTradeID", vesselD.VesselTradeID);
                    adapter.SelectCommand.Parameters.AddWithValue("@Displacement",  vesselD.Displacement);                   
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);                    
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateTanksAndHolds(TanksAndHolds tankandH, string Action)
        {
            try
            {
                int maxid = GetMaxSortingID(tankandH.TanksTypeId, "TanksAndHolds");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateTanksAndHolds", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", tankandH.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@Name", tankandH.Name);
                    adapter.SelectCommand.Parameters.AddWithValue("@Height", tankandH.Height);
                    adapter.SelectCommand.Parameters.AddWithValue("@Capacity", tankandH.Capacity);
                    adapter.SelectCommand.Parameters.AddWithValue("@TanksTypeId", tankandH.TanksTypeId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Maintaining_Status", "Yes");

                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@SortingOrder", maxid);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", tankandH.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdatePumps(PumpClass pump, string Action)
        {
            try
            {
                int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdatePumps", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", pump.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@PumpTypeId", pump.PumpTypeId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Name", pump.Name);                    
                    adapter.SelectCommand.Parameters.AddWithValue("@Capacity", pump.Capacity);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@SortingOrder", maxid);

                    adapter.SelectCommand.Parameters.AddWithValue("@PumpUseId", pump.PumpUseId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", pump.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertPort(PortListClass prt)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("InsertPort", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@countrycode", prt.CountryCode);
                    adapter.SelectCommand.Parameters.AddWithValue("@countryname", prt.CountryName);
                    adapter.SelectCommand.Parameters.AddWithValue("@portname", prt.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@facilityname", prt.FacilityName);
                    adapter.SelectCommand.Parameters.AddWithValue("@imoport", prt.IMOPortFacilityNumber);
                    adapter.SelectCommand.Parameters.AddWithValue("@longi", prt.Longitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@lati", prt.Latitude);


                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void InsertUpdateFuelConsNR(int id, int fueltypeid, int constypeid,decimal value,int ReportType_Id,int VesselId, string Action, string maxidAction)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "NoonReport");

                if(Action=="Insert")
                { 

                int maxid = GetMaxSortingID(0, maxidAction);
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateFuelConsNR", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Noon_Report_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelTypeId", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@ConsTypeId", constypeid);

                        adapter.SelectCommand.Parameters.AddWithValue("@Value", value);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
                if (Action == "Update")
                {

                  
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateFuelConsNR", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Noon_Report_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelTypeId", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@ConsTypeId", constypeid);

                        adapter.SelectCommand.Parameters.AddWithValue("@Value", value);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateFuelROB(int id, int fueltypeid,  decimal eosp, decimal fwe, decimal othrob, int ReportType_Id,int VesselId, string Action,string maxidAction)
        {
            try
            {
                if (Action == "Insert")
                {
                    int maxid = GetMaxSortingID(0, maxidAction);
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateFuelRob", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@EOSP", eosp);
                        adapter.SelectCommand.Parameters.AddWithValue("@FWE", fwe);

                        adapter.SelectCommand.Parameters.AddWithValue("@OtherROB", othrob);
                        adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {
                    
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateFuelRob", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@EOSP", eosp);
                        adapter.SelectCommand.Parameters.AddWithValue("@FWE", fwe);

                        adapter.SelectCommand.Parameters.AddWithValue("@OtherROB", othrob);
                        adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateNRCargo(int id,  NRCargo cls,string Action, string maxidAction)
        {
            try
            {
                int maxid = 0;
                if (Action == "Insert")
                {
                    if (id == 0)
                    {
                        maxid = GetMaxSortingID(0, maxidAction);
                    }
                    else
                    {
                        maxid = id;
                    }
                    
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@NoonReport_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@BL_Qty", cls.BL_Qty);
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadPortalActual", cls.LoadPortalActual);

                        adapter.SelectCommand.Parameters.AddWithValue("@TodaysActual", cls.TodaysActual);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Diff", cls.Qty_Diff);

                        if (cls.Reasonfor_Qty_Diff == null)
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Reasonfor_Qty_Diff", "");
                        }
                        else
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Reasonfor_Qty_Diff", cls.Reasonfor_Qty_Diff);
                        }
                        adapter.SelectCommand.Parameters.AddWithValue("@Cargo_Temp", cls.Cargo_Temp);
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@NoonReport_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BL_Qty", cls.BL_Qty);
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadPortalActual", cls.LoadPortalActual);

                        adapter.SelectCommand.Parameters.AddWithValue("@TodaysActual", cls.TodaysActual);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Diff", cls.Qty_Diff);
                        if (cls.Reasonfor_Qty_Diff == null)
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Reasonfor_Qty_Diff", "");
                        }
                        else
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Reasonfor_Qty_Diff", cls.Reasonfor_Qty_Diff);
                        }
                        adapter.SelectCommand.Parameters.AddWithValue("@Cargo_Temp", cls.Cargo_Temp);
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateARCargo(int id, ARCargo cls, string Action, string maxidAction)
        {
            try
            {
                int maxid = 0;
                if (Action == "Insert")
                {
                    if (id == 0)
                    {
                         maxid = GetMaxSortingID(0, maxidAction);
                    }
                    else
                    {
                        maxid = id;
                    }
                    
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateARCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@ArrivalReport_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade1", cls.Qty_Grade1);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade2", cls.Qty_Grade2);

                       
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateARCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@ArrivalReport_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade1", cls.Qty_Grade1);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade2", cls.Qty_Grade2);


                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateBRCargo(int id, BRCargo cls, string Action, string maxidAction)
        {
            try
            {
                int maxid = 0;

                if (Action == "Insert")
                {
                    if (id == 0)
                    {
                        maxid = GetMaxSortingID(0, maxidAction);
                    }
                    else
                    {
                        maxid = id;
                    }
                     

                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateBRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BerthingReport_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade1", cls.Qty_Grade1);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade2", cls.Qty_Grade2);

                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateBRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BerthingReport_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade1", cls.Qty_Grade1);
                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Grade2", cls.Qty_Grade2);

                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateDRCargo(int id, DRCargo cls, string Action, string maxidAction)
        {
            try
            {
                int maxid = 0;
                if (Action == "Insert")
                {

                    if (id == 0)
                    {
                        maxid = GetMaxSortingID(0, maxidAction);
                    }
                    else
                    {
                        maxid = id;
                    }
                     
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateDRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DepReport_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@BL_Qty", cls.BL_Qty);
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadPortalActual", cls.LoadPortalActual);
                        adapter.SelectCommand.Parameters.AddWithValue("@Cargo_Temp", cls.Cargo_Temp);

                        if (cls.Completion_DateT == null)
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Completion_DateT", DateTime.Now);
                        }
                        else
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Completion_DateT", cls.Completion_DateT);
                        }
                        adapter.SelectCommand.Parameters.AddWithValue("@Rate", cls.Rate);
                     
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateDRCargo", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@LR_Cargo_Id", cls.LR_Cargo_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DepReport_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BL_Qty", cls.BL_Qty);
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadPortalActual", cls.LoadPortalActual);
                        adapter.SelectCommand.Parameters.AddWithValue("@Cargo_Temp", cls.Cargo_Temp);

                        if (cls.Completion_DateT == null)
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Completion_DateT", DateTime.Now);
                        }
                        else
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@Completion_DateT", cls.Completion_DateT);
                        }
                        adapter.SelectCommand.Parameters.AddWithValue("@Rate", cls.Rate);

                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", cls.VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void InsertUpdateBunkerReceipt(int id, int fueltypeid, decimal rec, int ReportType_Id, int VesselId, string Action, string maxidAction )
        {
            try
            {
                if (Action == "Insert")
                {
                    int maxid = 0;
                    if (id == 0)
                    {
                        maxid = GetMaxSortingID(0, maxidAction);
                    }
                    else
                    {
                        maxid = id;
                    }
                    
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateBunkerReceipt", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Receipt", rec);

                        adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
                if (Action == "Update")
                {
                  
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateBunkerReceipt", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", fueltypeid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Receipt", rec);

                        adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void InsertUpdateDNR_CargoTank(int id, int CargoTank_Id, decimal Ullage, decimal Qty_MT, decimal Oxygen, decimal H2S,decimal HC,int VesselId, string Action)
        {
            try
            {
                if (Action == "Insert")
                {

                    int maxid = GetMaxSortingID(0, "NoonReport");
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRCargo_Tank", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@CargoTank_Id", CargoTank_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Ullage", Ullage);

                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_MT", Qty_MT);
                        adapter.SelectCommand.Parameters.AddWithValue("@Oxygen", Oxygen);
                        adapter.SelectCommand.Parameters.AddWithValue("@H2S", H2S);

                        adapter.SelectCommand.Parameters.AddWithValue("@HC", HC);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                    
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRCargo_Tank", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@CargoTank_Id", CargoTank_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Ullage", Ullage);

                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_MT", Qty_MT);
                        adapter.SelectCommand.Parameters.AddWithValue("@Oxygen", Oxygen);
                        adapter.SelectCommand.Parameters.AddWithValue("@H2S", H2S);

                        adapter.SelectCommand.Parameters.AddWithValue("@HC", HC);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateDNR_BallastTank(int id, int BallastTank_Id, decimal Sounding, decimal Qty_Vol,  decimal HC,int VesselId, string Action)
        {
            try
            {
                if (Action == "Insert")
                {

                    int maxid = GetMaxSortingID(0, "NoonReport");
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRBallast_Tank", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BallastTank_Id", BallastTank_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Sounding", Sounding);

                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Vol", Qty_Vol);
                        adapter.SelectCommand.Parameters.AddWithValue("@HC", HC);

                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);

                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
                if (Action == "Update")
                {

                   
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRBallast_Tank", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@BallastTank_Id", BallastTank_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Sounding", Sounding);

                        adapter.SelectCommand.Parameters.AddWithValue("@Qty_Vol", Qty_Vol);
                        adapter.SelectCommand.Parameters.AddWithValue("@HC", HC);

                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);

                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateDNR_VoidSpace(int id, int Void_Space_Id, decimal? Sounding,int VesselId, string Action)
        {
            try
            {
                if (Action == "Insert")
                {

                    int maxid = GetMaxSortingID(0, "NoonReport");
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRVoid_Space", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Void_Space_Id", Void_Space_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@Sounding", Sounding);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {

                  
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNRVoid_Space", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Void_Space_Id", Void_Space_Id);
                        adapter.SelectCommand.Parameters.AddWithValue("@DNR_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Sounding", Sounding);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void InsertUpdateNonRoutineEvents(int id, int ReportTableId, int NREvntId, string ChartererA,string Hours, int VesselId,string maxidAction, string Action)
        {
            try
            {
                if (Action == "Insert")
                {

                    int maxid = GetMaxSortingID(0, maxidAction);
                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNonRoutineCommon", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        if(Hours=="")
                        {
                            Hours = "0";
                        }

                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Report_Table_Id", ReportTableId);
                        adapter.SelectCommand.Parameters.AddWithValue("@NREvents_Id", NREvntId);
                        adapter.SelectCommand.Parameters.AddWithValue("@ChartererAccount", ChartererA);
                        adapter.SelectCommand.Parameters.AddWithValue("@Hours", Hours);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", maxid);
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }

                if (Action == "Update")
                {


                    using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateNonRoutineCommon", ConnectionBulder.con))
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        if (Hours == "")
                        {
                            Hours = "0";
                        }
                        adapter.SelectCommand.Parameters.AddWithValue("@id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@Report_Table_Id", ReportTableId);
                        adapter.SelectCommand.Parameters.AddWithValue("@NREvents_Id", NREvntId);
                        adapter.SelectCommand.Parameters.AddWithValue("@ChartererAccount", ChartererA);
                        adapter.SelectCommand.Parameters.AddWithValue("@Hours", Hours);
                        adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", id);
                        adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                        adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                        // SqlDataAdapter adapter = new SqlDataAdapter(command);
                        using (DataTable dataTable = new DataTable())
                        {
                            adapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateVoyage(VoyageClass Vg, string Action)
        {
            try
            {
               
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateVoyage", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", Vg.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageNumber", Vg.VoyageNumber);
                    adapter.SelectCommand.Parameters.AddWithValue("@CPId", Vg.CPId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Nor_Conditions", Vg.Nor_Conditions);
                    //adapter.SelectCommand.Parameters.AddWithValue("@VoyageStartPoint", Vg.VoyageStartPoint);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ETA", Vg.ETA);
                    //adapter.SelectCommand.Parameters.AddWithValue("@DTG", Vg.DTG);
                    //adapter.SelectCommand.Parameters.AddWithValue("@CP_SOG", Vg.CP_SOG);
                    //adapter.SelectCommand.Parameters.AddWithValue("@CP_Log_Speed", Vg.CP_Log_Speed);

                    //adapter.SelectCommand.Parameters.AddWithValue("@FW", Vg.FW);
                    adapter.SelectCommand.Parameters.AddWithValue("@HeavyWeather_BSS", Vg.HeavyWeather_BSS);
                    adapter.SelectCommand.Parameters.AddWithValue("@HeavyWeather_WH", Vg.HeavyWeather_WH);
                    adapter.SelectCommand.Parameters.AddWithValue("@HeavyWeather_CV", Vg.HeavyWeather_CV);
                   
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", Vg.VesselId);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateVoyageLeg(VoyageClass Vleg, string Action)
        {
            try
            {

                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertVoyageleg", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", Vleg.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", Vleg.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPort_A", Vleg.LegPort_A);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_A", Vleg.ReasonforPortCall_A);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPort_B", Vleg.LegPort_B);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_B", Vleg.ReasonforPortCall_B);
                    
                    adapter.SelectCommand.Parameters.AddWithValue("@DTG", Vleg.DTG);
                    adapter.SelectCommand.Parameters.AddWithValue("@CP_SOG", Vleg.CP_SOG);
                    adapter.SelectCommand.Parameters.AddWithValue("@CP_Log_Speed", Vleg.CP_Log_Speed);


                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", Vleg.VesselId);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateFuel(VoyageClass Vleg, string Action)
        {
            try
            {

                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertFuelConsumption", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", Vleg.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", Vleg.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@FuelType", Vleg.FuelT);
                    adapter.SelectCommand.Parameters.AddWithValue("@CP_cons_perday_HFO", Vleg.CP_Consumption);
                   



                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", Vleg.VesselId);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateDailyNoonReport(DailyNoonReport dnR, string Action)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateDailyNoonReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", dnR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", dnR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", dnR.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselStatus", dnR.VesselStatus);
                    adapter.SelectCommand.Parameters.AddWithValue("@AtSeaOrPort", dnR.AtSeaOrPort);

                    if (dnR.AtSeaOrPort == "Sea")
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@PortStatus", 0);
                    }
                    else
                    {

                        adapter.SelectCommand.Parameters.AddWithValue("@PortStatus", dnR.PortStatus);
                    }
                    adapter.SelectCommand.Parameters.AddWithValue("@Date", dnR.Date);
                    adapter.SelectCommand.Parameters.AddWithValue("@Latitude", dnR.Latitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@Longitude", dnR.Longitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@ETA", dnR.ETA);
                    adapter.SelectCommand.Parameters.AddWithValue("@NPOC", dnR.NPOC);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", dnR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", dnR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid",dnR.DraftMid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Displacement", dnR.Displacement);
                    adapter.SelectCommand.Parameters.AddWithValue("@NoonToNoonDMG_Dist", dnR.NoonToNoonDMG_Dist);
                    adapter.SelectCommand.Parameters.AddWithValue("@LogDist", dnR.LogDist);
                    adapter.SelectCommand.Parameters.AddWithValue("@EngineDist", dnR.EngineDist);
                    adapter.SelectCommand.Parameters.AddWithValue("@TotalDistance", dnR.TotalDistance);
                    adapter.SelectCommand.Parameters.AddWithValue("@StmgTime", dnR.StmgTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@TotalTime", dnR.TotalTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@DistToGo_DTG", dnR.DistToGo_DTG);
                    adapter.SelectCommand.Parameters.AddWithValue("@CP_Speed", dnR.CP_Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@Act_Speed", dnR.Act_Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@Gen_Avg_Speed", dnR.Gen_Avg_Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@GAS", dnR.GAS);
                    adapter.SelectCommand.Parameters.AddWithValue("@SeaState", dnR.SeaState);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindDirection", dnR.WindDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindForce", dnR.WindForce);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellDirection", dnR.SwellDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellHeight", dnR.SwellHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveLength", dnR.WaveLength);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveHeight", dnR.WaveHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@Slip", dnR.Slip);
                    adapter.SelectCommand.Parameters.AddWithValue("@RPM", dnR.RPM);
                    adapter.SelectCommand.Parameters.AddWithValue("@BHP", dnR.BHP);
                    adapter.SelectCommand.Parameters.AddWithValue("@MCR", dnR.MCR);
                    adapter.SelectCommand.Parameters.AddWithValue("@ME_ControlLoc", dnR.ME_ControlLoc);
                    adapter.SelectCommand.Parameters.AddWithValue("@SCAV_ManiPress", dnR.SCAV_ManiPress);
                    adapter.SelectCommand.Parameters.AddWithValue("@SCAV_Temp", dnR.SCAV_Temp);
                    adapter.SelectCommand.Parameters.AddWithValue("@Max_Exhaust_Temp", dnR.Max_Exhaust_Temp);
                    adapter.SelectCommand.Parameters.AddWithValue("@Min_Exhaust_Temp", dnR.Min_Exhaust_Temp);
                    adapter.SelectCommand.Parameters.AddWithValue("@SW_Temp", dnR.SW_Temp);
                    adapter.SelectCommand.Parameters.AddWithValue("@ER_Temp", dnR.ER_Temp);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Full", dnR.OT_ROB_OXY_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_InUse", dnR.OT_ROB_OXY_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Empty", dnR.OT_ROB_OXY_Empty);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Full", dnR.OT_ROB_ACYT_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_InUse", dnR.OT_ROB_ACYT_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Empty", dnR.OT_ROB_ACYT_Empty);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_RungHrs_No1", dnR.AE_RungHrs_No1);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_RungHrs_No2", dnR.AE_RungHrs_No2);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_RungHrs_No3", dnR.AE_RungHrs_No3);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_RungHrs_No4", dnR.AE_RungHrs_No4);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_RungHrs_ShaftGen", dnR.AE_RungHrs_ShaftGen);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Load_No1", dnR.AE_Load_No1);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Load_No2", dnR.AE_Load_No2);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Load_No3", dnR.AE_Load_No3);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Load_No4", dnR.AE_Load_No4);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Load_ShaftGen", dnR.AE_Load_ShaftGen);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_Extra_Run_Reason", dnR.AE_Extra_Run_Reason);
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Oil", dnR.SLOPS_ROB_OXY_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Water", dnR.SLOPS_ROB_OXY_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Total", dnR.SLOPS_ROB_OXY_Total);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC", dnR.LO_HO_Cons_MECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL", dnR.LO_HO_Cons_MECYL);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC", dnR.LO_HO_Cons_AECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil", dnR.LO_HO_Cons_HYDR_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC_ROB", dnR.LO_HO_Cons_MECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL_ROB", dnR.LO_HO_Cons_MECYL_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC_ROB", dnR.LO_HO_Cons_AECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil_ROB", dnR.LO_HO_Cons_HYDR_Oil_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@Ballast_ROB", dnR.Ballast_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Generated", dnR.FW_Generated);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Consumption", dnR.FW_Consumption);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_ROB", dnR.FW_ROB);
                    //adapter.SelectCommand.Parameters.AddWithValue("@F_ROB_HFO", dnR.F_ROB_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@F_ROB_MDO", dnR.F_ROB_MDO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@F_ROB_LSHFO", dnR.F_ROB_LSHFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@F_ROB_LSMFO", dnR.F_ROB_LSMFO);

                    adapter.SelectCommand.Parameters.AddWithValue("@ER_Bilge_ROB", dnR.ER_Bilge_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@ER_Sludge_ROB", dnR.ER_Sludge_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@ER_WasteOil_ROB", dnR.ER_WasteOil_ROB);


                    adapter.SelectCommand.Parameters.AddWithValue("@PumpRoomMaxSounding", dnR.PumpRoomMaxSounding);
                    adapter.SelectCommand.Parameters.AddWithValue("@ChainLocker1", dnR.ChainLocker1);
                    adapter.SelectCommand.Parameters.AddWithValue("@ChainLocker2", dnR.ChainLocker2);
                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", dnR.Remarks);

                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_CP_VLSFO", dnR.ME_CP_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_CP_HFO", dnR.ME_CP_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_CP_MGO", dnR.ME_CP_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_SEA_VLSFO", dnR.ME_ACT_SEA_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_SEA_HFO", dnR.ME_ACT_SEA_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_SEA_MGO", dnR.ME_ACT_SEA_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_MAN_VLSFO", dnR.ME_ACT_MAN_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_MAN_HFO", dnR.ME_ACT_MAN_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_MAN_MGO", dnR.ME_ACT_MAN_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_WAIT_VLSFO", dnR.ME_ACT_WAIT_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_WAIT_HFO", dnR.ME_ACT_WAIT_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_WAIT_MGO", dnR.ME_ACT_WAIT_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_BERTH_VLSFO", dnR.ME_ACT_BERTH_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_BERTH_HFO", dnR.ME_ACT_BERTH_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@ME_ACT_BERTH_MGO", dnR.ME_ACT_BERTH_MGO);

                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_CP_VLSFO", dnR.AE_CP_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_CP_HFO", dnR.AE_CP_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_CP_MGO", dnR.AE_CP_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_SEA_VLSFO", dnR.AE_ACT_SEA_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_SEA_HFO", dnR.AE_ACT_SEA_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_SEA_MGO", dnR.AE_ACT_SEA_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_MAN_VLSFO", dnR.AE_ACT_MAN_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_MAN_HFO", dnR.AE_ACT_MAN_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_MAN_MGO", dnR.AE_ACT_MAN_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_WAIT_VLSFO", dnR.AE_ACT_WAIT_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_WAIT_HFO", dnR.AE_ACT_WAIT_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_WAIT_MGO", dnR.AE_ACT_WAIT_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_BERTH_VLSFO", dnR.AE_ACT_BERTH_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_BERTH_HFO", dnR.AE_ACT_BERTH_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@AE_ACT_BERTH_MGO", dnR.AE_ACT_BERTH_MGO);

                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_CP_VLSFO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_CP_HFO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_CP_MGO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_SEA_VLSFO", dnR.BLR_ACT_SEA_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_SEA_HFO", dnR.BLR_ACT_SEA_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_SEA_MGO", dnR.BLR_ACT_SEA_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_MAN_VLSFO", dnR.BLR_ACT_MAN_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_MAN_HFO", dnR.BLR_ACT_MAN_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_MAN_MGO", dnR.BLR_ACT_MAN_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_WAIT_VLSFO", dnR.BLR_ACT_WAIT_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_WAIT_HFO", dnR.BLR_ACT_WAIT_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_WAIT_MGO", dnR.BLR_ACT_WAIT_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_BERTH_VLSFO", dnR.BLR_ACT_BERTH_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_BERTH_HFO", dnR.BLR_ACT_BERTH_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BLR_ACT_BERTH_MGO", dnR.BLR_ACT_BERTH_MGO);

                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_CP_VLSFO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_CP_HFO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_CP_MGO", 0.00);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_SEA_VLSFO", dnR.FRAMO_ACT_SEA_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_SEA_HFO", dnR.FRAMO_ACT_SEA_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_SEA_MGO", dnR.FRAMO_ACT_SEA_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_MAN_VLSFO", dnR.FRAMO_ACT_MAN_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_MAN_HFO", dnR.FRAMO_ACT_MAN_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_MAN_MGO", dnR.FRAMO_ACT_MAN_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_WAIT_VLSFO", dnR.FRAMO_ACT_WAIT_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_WAIT_HFO", dnR.FRAMO_ACT_WAIT_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_WAIT_MGO", dnR.FRAMO_ACT_WAIT_MGO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_BERTH_VLSFO", dnR.FRAMO_ACT_BERTH_VLSFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_BERTH_HFO", dnR.FRAMO_ACT_BERTH_HFO);
                    //adapter.SelectCommand.Parameters.AddWithValue("@FRAMO_ACT_BERTH_MGO", dnR.FRAMO_ACT_BERTH_MGO);

                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", dnR.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", dnR.SaveDraft);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat1", dnR.Lat1);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat2", dnR.Lat2);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat3", dnR.Lat3);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long1", dnR.Long1);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long2", dnR.Long2);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long3", dnR.Long3);
                    adapter.SelectCommand.Parameters.AddWithValue("@BR_RungHrs_No1", dnR.BR_RungHrs_No1);
                    adapter.SelectCommand.Parameters.AddWithValue("@BR_RungHrs_No2", dnR.BR_RungHrs_No2);
                    adapter.SelectCommand.Parameters.AddWithValue("@BR_Extra_Run_Reason1", dnR.BR_Extra_Run_Reason1);
                    adapter.SelectCommand.Parameters.AddWithValue("@BR_Extra_Run_Reason2", dnR.BR_Extra_Run_Reason2);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);


                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateArrivalReport(ArrivalReport arrR, string Action)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateArrivalReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", arrR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", arrR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", arrR.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", arrR.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@Place", arrR.Place);
                    adapter.SelectCommand.Parameters.AddWithValue("@EOSP", arrR.EOSP);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", arrR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", arrR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid", arrR.DraftMid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Latitude", arrR.Latitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@Longitude", arrR.Longitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@NOR", arrR.NOR);
                    adapter.SelectCommand.Parameters.AddWithValue("@ETB", arrR.ETB);
                    adapter.SelectCommand.Parameters.AddWithValue("@Anchor_Name", arrR.Anchor_Name);                 
                    adapter.SelectCommand.Parameters.AddWithValue("@Anchor_DateT", arrR.Anchor_DateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@AnchorPos_Latitude", arrR.AnchorPos_Latitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@AnchorPos_Longitude", arrR.AnchorPos_Longitude);
                    adapter.SelectCommand.Parameters.AddWithValue("@AnchorFWE_DateT", arrR.AnchorFWE_DateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@NoonToNoonDMG_Dist", arrR.NoonToNoonDMG_Dist);
                    adapter.SelectCommand.Parameters.AddWithValue("@LogDist", arrR.LogDist);
                    adapter.SelectCommand.Parameters.AddWithValue("@EngineDist", arrR.EngineDist);
                    adapter.SelectCommand.Parameters.AddWithValue("@TotalDistance", arrR.TotalDistance);
                    adapter.SelectCommand.Parameters.AddWithValue("@StmgTime", arrR.StmgTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@DistToGo_DTG", arrR.DistToGo_DTG);
                    adapter.SelectCommand.Parameters.AddWithValue("@CP_Speed", arrR.CP_Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@Act_Speed", arrR.Act_Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@GAS", arrR.GAS);
                    adapter.SelectCommand.Parameters.AddWithValue("@SeaState", arrR.SeaState);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindDirection", arrR.WindDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindForce", arrR.WindForce);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellDirection", arrR.SwellDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellHeight", arrR.SwellHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveLength", arrR.WaveLength);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveHeight", arrR.WaveHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@Slip", arrR.Slip);
                    adapter.SelectCommand.Parameters.AddWithValue("@RPM", arrR.RPM);
                    adapter.SelectCommand.Parameters.AddWithValue("@BHP", arrR.BHP);
                    adapter.SelectCommand.Parameters.AddWithValue("@MCR", arrR.MCR);                   
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Full", arrR.OT_ROB_OXY_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_InUse", arrR.OT_ROB_OXY_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Empty", arrR.OT_ROB_OXY_Empty);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Full", arrR.OT_ROB_ACYT_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_InUse", arrR.OT_ROB_ACYT_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Empty", arrR.OT_ROB_ACYT_Empty);                  
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Oil", arrR.SLOPS_ROB_OXY_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Water", arrR.SLOPS_ROB_OXY_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@SLOPS_ROB_OXY_Total", arrR.SLOPS_ROB_OXY_Total);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC", arrR.LO_HO_Cons_MECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL", arrR.LO_HO_Cons_MECYL);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC", arrR.LO_HO_Cons_AECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil", arrR.LO_HO_Cons_HYDR_Oil);

                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC_ROB", arrR.LO_HO_Cons_MECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL_ROB", arrR.LO_HO_Cons_MECYL_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC_ROB", arrR.LO_HO_Cons_AECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil_ROB", arrR.LO_HO_Cons_HYDR_Oil_ROB);

                    adapter.SelectCommand.Parameters.AddWithValue("@Ballast_ROB", arrR.Ballast_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Hrs", arrR.Manoeuvring_Hrs);
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Distance", arrR.Manoeuvring_Distance);
                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", arrR.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", arrR.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", arrR.SaveDraft);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat1", arrR.Lat1);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat2", arrR.Lat2);
                    adapter.SelectCommand.Parameters.AddWithValue("@Lat3", arrR.Lat3);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long1", arrR.Long1);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long2", arrR.Long2);
                    adapter.SelectCommand.Parameters.AddWithValue("@Long3", arrR.Long3);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALat1", arrR.ALat1);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALat2", arrR.ALat2);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALat3", arrR.ALat3);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALong1", arrR.ALong1);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALong2", arrR.ALong2);
                    adapter.SelectCommand.Parameters.AddWithValue("@ALong3", arrR.ALong3);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Generated", arrR.FW_Generated);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Consumption", arrR.FW_Consumption);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_ROB", arrR.FW_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@TotalTime", arrR.TotalTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@Gen_Avg_Speed", arrR.Gen_Avg_Speed);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);


                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public static void InsertUpdateDepartureReport(DepartureReport deprtureR, string Action)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateDepartureReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", deprtureR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", deprtureR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@DepLegPortId", deprtureR.DepLegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@NextLegPortId", deprtureR.NextLegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@DeparturePort", deprtureR.DeparturePort);
                    adapter.SelectCommand.Parameters.AddWithValue("@NextPort", deprtureR.NextPort);
                    adapter.SelectCommand.Parameters.AddWithValue("@ETA", deprtureR.ETA);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReportDate", deprtureR.ReportDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", deprtureR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", deprtureR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid", deprtureR.DraftMid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Ballast_ROB", deprtureR.Ballast_ROB);


                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsDisposed_Oil", deprtureR.SlopsDisposed_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsDisposed_Water", deprtureR.SlopsDisposed_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsDisposed_Total", deprtureR.SlopsDisposed_Total);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Oil", deprtureR.SlopsROB_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Water", deprtureR.SlopsROB_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Total", deprtureR.SlopsROB_Total);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesDisposed_Oil", deprtureR.BilgesDisposed_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesDisposed_Water", deprtureR.BilgesDisposed_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesDisposed_Total", deprtureR.BilgesDisposed_Total);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesROB_Oil", deprtureR.BilgesROB_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesROB_Water", deprtureR.BilgesROB_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@BilgesROB_Total", deprtureR.BilgesROB_Total);


                    adapter.SelectCommand.Parameters.AddWithValue("@Sludge", deprtureR.Sludge);
                    adapter.SelectCommand.Parameters.AddWithValue("@GarbagePlastic", deprtureR.GarbagePlastic);
                    adapter.SelectCommand.Parameters.AddWithValue("@GarbageOthers", deprtureR.GarbageOthers);
                    adapter.SelectCommand.Parameters.AddWithValue("@OtherDisposal", deprtureR.OtherDisposal);
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Hrs", deprtureR.Manoeuvring_Hrs);
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Distance", deprtureR.Manoeuvring_Distance);
                    adapter.SelectCommand.Parameters.AddWithValue("@SBE_DateT", deprtureR.SBE_DateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@RFA_DateT", deprtureR.RFA_DateT);


                    adapter.SelectCommand.Parameters.AddWithValue("@SeaState", deprtureR.SeaState);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindDirection", deprtureR.WindDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindForce", deprtureR.WindForce);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellDirection", deprtureR.SwellDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellHeight", deprtureR.SwellHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveLength", deprtureR.WaveLength);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveHeight", deprtureR.WaveHeight);

                    adapter.SelectCommand.Parameters.AddWithValue("@Slip", deprtureR.Slip);
                    adapter.SelectCommand.Parameters.AddWithValue("@RPM", deprtureR.RPM);
                    adapter.SelectCommand.Parameters.AddWithValue("@BHP", deprtureR.BHP);
                    adapter.SelectCommand.Parameters.AddWithValue("@MCR", deprtureR.MCR);

                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC", deprtureR.LO_HO_Cons_MECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL", deprtureR.LO_HO_Cons_MECYL);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC", deprtureR.LO_HO_Cons_AECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil", deprtureR.LO_HO_Cons_HYDR_Oil);

                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Full", deprtureR.OT_ROB_OXY_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_InUse", deprtureR.OT_ROB_OXY_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Empty", deprtureR.OT_ROB_OXY_Empty);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Full", deprtureR.OT_ROB_ACYT_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_InUse", deprtureR.OT_ROB_ACYT_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Empty", deprtureR.OT_ROB_ACYT_Empty);

                    adapter.SelectCommand.Parameters.AddWithValue("@Bunker_LO_Rec_MECC", deprtureR.Bunker_LO_Rec_MECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@Bunker_LO_Rec_MECYL", deprtureR.Bunker_LO_Rec_MECYL);
                    adapter.SelectCommand.Parameters.AddWithValue("@Bunker_LO_Rec_AECC", deprtureR.Bunker_LO_Rec_AECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@Bunker_LO_Rec_HYDR_Oil", deprtureR.Bunker_LO_Rec_HYDR_Oil);


                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC_ROB", deprtureR.LO_HO_Cons_MECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL_ROB", deprtureR.LO_HO_Cons_MECYL_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC_ROB", deprtureR.LO_HO_Cons_AECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil_ROB", deprtureR.LO_HO_Cons_HYDR_Oil_ROB);

                    adapter.SelectCommand.Parameters.AddWithValue("@CTM", deprtureR.CTM);
                    adapter.SelectCommand.Parameters.AddWithValue("@Spares", deprtureR.Spares);
                    adapter.SelectCommand.Parameters.AddWithValue("@Stores", deprtureR.Stores);                  
                    adapter.SelectCommand.Parameters.AddWithValue("@RepairsConducted", deprtureR.RepairsConducted);
                    adapter.SelectCommand.Parameters.AddWithValue("@CrewChange", deprtureR.CrewChange);
                    adapter.SelectCommand.Parameters.AddWithValue("@ItemsLanded", deprtureR.ItemsLanded);

                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", deprtureR.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", deprtureR.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", deprtureR.SaveDraft);

                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Generated", deprtureR.FW_Generated);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Consumption", deprtureR.FW_Consumption);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_ROB", deprtureR.FW_ROB);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);


                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateBerthingReport(BerthingReport berthingR, string Action)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdateBerthingReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@id", berthingR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", berthingR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", berthingR.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", berthingR.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@FacilityName", berthingR.FacilityName);
                    adapter.SelectCommand.Parameters.AddWithValue("@BerthName", berthingR.BerthName);

                    adapter.SelectCommand.Parameters.AddWithValue("@ReportDate", berthingR.ReportDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortStatus", berthingR.PortStatus);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", berthingR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", berthingR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid", berthingR.DraftMid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Ballast_ROB", berthingR.Ballast_ROB);


                
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Oil", berthingR.SlopsROB_Oil);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Water", berthingR.SlopsROB_Water);
                    adapter.SelectCommand.Parameters.AddWithValue("@SlopsROB_Total", berthingR.SlopsROB_Total);
                    


                  
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Hrs", berthingR.Manoeuvring_Hrs);
                    adapter.SelectCommand.Parameters.AddWithValue("@Manoeuvring_Distance", berthingR.Manoeuvring_Distance);
                    adapter.SelectCommand.Parameters.AddWithValue("@SBE_DateT", berthingR.SBE_DateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@RFA_DateT", berthingR.RFA_DateT);


                    adapter.SelectCommand.Parameters.AddWithValue("@SeaState", berthingR.SeaState);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindDirection", berthingR.WindDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@WindForce", berthingR.WindForce);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellDirection", berthingR.SwellDirection);
                    adapter.SelectCommand.Parameters.AddWithValue("@SwellHeight", berthingR.SwellHeight);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveLength", berthingR.WaveLength);
                    adapter.SelectCommand.Parameters.AddWithValue("@WaveHeight", berthingR.WaveHeight);

                    adapter.SelectCommand.Parameters.AddWithValue("@Slip", berthingR.Slip);
                    adapter.SelectCommand.Parameters.AddWithValue("@RPM", berthingR.RPM);
                    adapter.SelectCommand.Parameters.AddWithValue("@BHP", berthingR.BHP);
                    adapter.SelectCommand.Parameters.AddWithValue("@MCR", berthingR.MCR);

                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC", berthingR.LO_HO_Cons_MECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL", berthingR.LO_HO_Cons_MECYL);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC", berthingR.LO_HO_Cons_AECC);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil", berthingR.LO_HO_Cons_HYDR_Oil);

                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECC_ROB", berthingR.LO_HO_Cons_MECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_MECYL_ROB", berthingR.LO_HO_Cons_MECYL_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_AECC_ROB", berthingR.LO_HO_Cons_AECC_ROB);
                    adapter.SelectCommand.Parameters.AddWithValue("@LO_HO_Cons_HYDR_Oil_ROB", berthingR.LO_HO_Cons_HYDR_Oil_ROB);

                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Full", berthingR.OT_ROB_OXY_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_InUse", berthingR.OT_ROB_OXY_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_OXY_Empty", berthingR.OT_ROB_OXY_Empty);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Full", berthingR.OT_ROB_ACYT_Full);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_InUse", berthingR.OT_ROB_ACYT_InUse);
                    adapter.SelectCommand.Parameters.AddWithValue("@OT_ROB_ACYT_Empty", berthingR.OT_ROB_ACYT_Empty);

                 

                 

                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", berthingR.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", berthingR.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", berthingR.SaveDraft);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Generated", berthingR.FW_Generated);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_Consumption", berthingR.FW_Consumption);
                    adapter.SelectCommand.Parameters.AddWithValue("@FW_ROB", berthingR.FW_ROB);

                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);


                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateFreshWaterReport(FreshWaterReport model)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("InsertUpdateFreshWaterReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", model.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", model.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", model.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@Facility_Name", model.Facility_Name);
                    adapter.SelectCommand.Parameters.AddWithValue("@VendorDetails", model.VendorDetails);
                    adapter.SelectCommand.Parameters.AddWithValue("@Intial_Meter_Reading_MT_supplied", model.Intial_Meter_Reading_MT_supplied);
                    adapter.SelectCommand.Parameters.AddWithValue("@Final_Meter_Reading_MT", model.Final_Meter_Reading_MT);
                    adapter.SelectCommand.Parameters.AddWithValue("@Difference_in_Meter_Reading_MT", model.Difference_in_Meter_Reading_MT);
                    adapter.SelectCommand.Parameters.AddWithValue("@QTY_supplied_MT", model.QTY_supplied_MT);
                    adapter.SelectCommand.Parameters.AddWithValue("@File_Name", model.File_Name);
                    adapter.SelectCommand.Parameters.AddWithValue("@Received_Date", model.Received_Date);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertUpdateBunkerReport(BunkerReport model)
        {
            try
            {
                //int maxid = GetMaxSortingID(0, "Pump");
                using (SqlDataAdapter adapter = new SqlDataAdapter("InsertUpdateBunkerReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", model.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", model.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", model.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", model.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Supplier", model.Supplier);
                    adapter.SelectCommand.Parameters.AddWithValue("@BargeAlongside", model.BargeAlongside);
                    adapter.SelectCommand.Parameters.AddWithValue("@BunkerHoseConnected", model.BunkerHoseConnected);
                    adapter.SelectCommand.Parameters.AddWithValue("@CommencedBunkering", model.CommencedBunkering);
                    adapter.SelectCommand.Parameters.AddWithValue("@BunkeringCompleted", model.BunkeringCompleted);
                    adapter.SelectCommand.Parameters.AddWithValue("@BunkerHosedisconnected", model.BunkerHosedisconnected);
                    adapter.SelectCommand.Parameters.AddWithValue("@BargeCastOff", model.BargeCastOff);
                    adapter.SelectCommand.Parameters.AddWithValue("@BargeName", model.BargeName);
                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", model.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@LabAnalysisReport_Name", model.LabAnalysisReport_Name);
                    adapter.SelectCommand.Parameters.AddWithValue("@FirstName", model.FirstName);
                    adapter.SelectCommand.Parameters.AddWithValue("@LastName", model.LastName);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public static void CommonDelete(int Id, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonDelete", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@id", Id);                  
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);                 
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertinOrder(int sortingorder,string name,int tanktypeid, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertinOrder", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@SortingOrder", sortingorder);
                    adapter.SelectCommand.Parameters.AddWithValue("@Name", name.Trim());
                    adapter.SelectCommand.Parameters.AddWithValue("@TanksTypeId", tanktypeid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int CheckVesselExistence(VesselDetail vesselD, int check,int id ,string action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCheckVesselExistence", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                    adapter.SelectCommand.Parameters.AddWithValue("@vesselName", vesselD.VesselName);
                    adapter.SelectCommand.Parameters.AddWithValue("@imono", vesselD.ImoNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", action);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if(dataTable.Rows.Count > 0)
                        {
                            check = 1;
                        }


                    }
                }
                return check;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int CheckUserExistence(UserDetail user, int check)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCheckUserExistence", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                   
                    adapter.SelectCommand.Parameters.AddWithValue("@email", user.UserEmail);
                  

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            check = 1;
                        }


                    }
                }
                return check;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int CheckTHsNameExistence(TanksAndHolds tnk, int check, int id, string action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCheckTHsNameExistence", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                    adapter.SelectCommand.Parameters.AddWithValue("@Name", tnk.Name);                 
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", action);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            check = 1;
                        }


                    }
                }
                return check;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int CheckpumpExistence(PumpClass pmp, int vesselid, int check, int id, string action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCheckpumpExistence", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                    adapter.SelectCommand.Parameters.AddWithValue("@Name", pmp.Name);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", action);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            check = 1;
                        }
                    }
                }
                return check;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int CheckvoyageExistence(VoyageClass vg, int check, int id, string action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCheckvoyageExistence", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageNumber", vg.VoyageNumber);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", action);

                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            check = 1;
                        }
                    }
                }
                return check;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetUserByEmailID(string UserEmail)
        {
            try
            {
                string UserID = null;
                using (SqlDataAdapter adapter = new SqlDataAdapter("GetAspNetUser", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Email", UserEmail);


                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            UserID = dataTable.Rows[0]["Id"].ToString();

                        }
                    }
                }

                return UserID;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<UserDetail> GetUserList(int pageNo, int totalnoofPage)
        {
            try
            {
                List<UserDetail> list = new List<UserDetail>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("GetUserList", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new UserDetail()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    UserEmail = dataTable.Tables[0].Rows[i]["UserEmail"].ToString(),
                                    FullName = dataTable.Tables[0].Rows[i]["FullName"].ToString(),
                                    UserName = dataTable.Tables[0].Rows[i]["UserName"].ToString(),
                                    UserType = dataTable.Tables[0].Rows[i]["UserType"].ToString(),
                                    AssignVessel = dataTable.Tables[0].Rows[i]["AssignVessel"].ToString(),
                                    //RankName = dataTable.Rows[i]["Ranks"].ToString(),
                                    DeptName = dataTable.Tables[0].Rows[i]["Departments"].ToString(),
                                    //RankId = Convert.ToInt32(dataTable.Rows[i]["RankId"]),
                                    DepId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["DepId"]),
                                    UserID = dataTable.Tables[0].Rows[i]["UserID"].ToString(),
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<VesselDetail> GetVesselList(int? pageNo, int totalnoofPage)
        {
            try
            {
                if (ConnectionBulder.con.State != ConnectionState.Open)
                {
                    ConnectionBulder.con.Open();
                }
                List<VesselDetail> list = new List<VesselDetail>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVesselList", ConnectionBulder.con))
                {

                     adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new VesselDetail()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    ImoNo = Convert.ToInt32(dataTable.Tables[0].Rows[i]["ImoNo"]),
                                    FleetType = dataTable.Tables[0].Rows[i]["FleetType"].ToString(),
                                    FleetName = dataTable.Tables[0].Rows[i]["FleetName"].ToString(),
                                    VesselTrade = dataTable.Tables[0].Rows[i]["VesselTrade"].ToString(),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VesselDetail> GetVesselListNew(int pageNo, int totalnoofPage)
        {
            try
            {
                if (ConnectionBulder.con.State != ConnectionState.Open)
                {
                    ConnectionBulder.con.Open();
                }
                List<VesselDetail> list = new List<VesselDetail>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVesselListNew", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new VesselDetail()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    ImoNo = Convert.ToInt32(dataTable.Tables[0].Rows[i]["ImoNo"]),
                                    FleetType = dataTable.Tables[0].Rows[i]["FleetType"].ToString(),
                                    FleetName = dataTable.Tables[0].Rows[i]["FleetName"].ToString(),
                                    VesselTrade = dataTable.Tables[0].Rows[i]["VesselTrade"].ToString(),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static List<TanksType> GetTankTypeList()
        {
            try
            {               
                List<TanksType> list = new List<TanksType>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "TanksType");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new TanksType()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                    TankType = dataTable.Rows[i]["TankType"].ToString(),
                                    MaintainingROBs =Convert.ToBoolean(dataTable.Rows[i]["MaintainingROBs"]),
                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<DailyNoonReport> GetNoonReportList(int vesselid, int pageNo,int totalnoofPage)
        {
            try
            {
                List<DailyNoonReport> list = new List<DailyNoonReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DailyNoonReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DailyNoonReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),
                                    AtSeaOrPort = dataTable.Tables[0].Rows[i]["AtSeaOrPort"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["AtSeaOrPort"].ToString(),

                                    Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Date"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["Date"]),


                                    // Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Date"]),

                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? false : dataTable.Tables[0].Rows[i]["SaveDraft"]),

                                    // SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),

                                   // CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),


                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),


                                });
                            }
                        }
                      
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<DailyNoonReport> SearchNoonReportList(int vesselid, int pageNo, int totalnoofPage,string firstS,string dateF,string dateT)
        {
            try
            {
                List<DailyNoonReport> list = new List<DailyNoonReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DailyNoonReport");
                 
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DailyNoonReport()
                                {
                                    //Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    //AtSeaOrPort = dataTable.Tables[0].Rows[i]["AtSeaOrPort"].ToString(),
                                    //Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Date"]),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ////TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),
                                    ///
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),

                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    AtSeaOrPort = dataTable.Tables[0].Rows[i]["AtSeaOrPort"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["AtSeaOrPort"].ToString(),
                                    Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Date"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["Date"]),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                  
                                   // TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[0][0]),


                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<ArrivalReport> SearchArrivalReportList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<ArrivalReport> list = new List<ArrivalReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "ArrivalReport");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new ArrivalReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),

                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),

                                    Place = dataTable.Tables[0].Rows[i]["Place"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Place"].ToString(),
                                    EOSP = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EOSP"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["EOSP"]),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                   // TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? DateTime.Now : dataTable.Tables[1].Rows[0][0]),





                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<BerthingReport> SearchBerthingReportList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<BerthingReport> list = new List<BerthingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "BerthingReport");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new BerthingReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    //PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    //FacilityName = dataTable.Tables[0].Rows[i]["FacilityName"].ToString(),
                                    //BerthName = dataTable.Tables[0].Rows[i]["BerthName"].ToString(),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),


                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    FacilityName = dataTable.Tables[0].Rows[i]["FacilityName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["FacilityName"].ToString(),
                                    BerthName = dataTable.Tables[0].Rows[i]["BerthName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["BerthName"].ToString(),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ReportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDate"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDate"]),


                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<DepartureReport> SearchDepartureReportList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<DepartureReport> list = new List<DepartureReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DepartureReport");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DepartureReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    //DeparturePort = dataTable.Tables[0].Rows[i]["DeparturePort"].ToString(),
                                    //NextPort = dataTable.Tables[0].Rows[i]["NextPort"].ToString(),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),

                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),

                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    DeparturePort = dataTable.Tables[0].Rows[i]["DeparturePort"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["DeparturePort"].ToString(),
                                    NextPort = dataTable.Tables[0].Rows[i]["NextPort"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["NextPort"].ToString(),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ReportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDate"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDate"]),

                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<DischargingReport> SearchDischargingReportList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<DischargingReport> list = new List<DischargingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DischargingReport");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DischargingReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"]),
                                    ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),

                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<LoadingReport> SearchLoadingReportList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<LoadingReport> list = new List<LoadingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchReport", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "LoadingReport");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new LoadingReport()
                                {
                                    //Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    //LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    //PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    //VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"]),
                                    //ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"]),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ////TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),



                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                  
                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["VesselId"]),
                                    ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["ReportDateTime"]),

                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),

                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),

                                   // TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),





                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<VoyageClass> SearchVoyageDList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<VoyageClass> list = new List<VoyageClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchCommon", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "VoyageDetails");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new VoyageClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    VoyageStartP = dataTable.Tables[0].Rows[i]["VoyageStartP"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageStartP"].ToString(),
                                    VoyageEndP = dataTable.Tables[0].Rows[i]["VoyageEndP"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageEndP"].ToString(),
                                   
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),

                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<CPVessel> SearchCPVoyageDList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT,string action)
        {
            try
            {
                List<CPVessel> list = new List<CPVessel>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchCommon", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", action);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {

                                int cpid = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]);
                                string vslinfo = "";
                                using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,v.VesselName,cp.CPNo from CPpart1 a join VesselDetail v on a.VesselID=v.ImoNo join CPContract cp on cp.Id=a.CPId where a.IsActive = 1 and  a.CPId = " + cpid + "", ConnectionBulder.con))
                                {
                                    DataTable dt = new DataTable();
                                    adp.Fill(dt);
                                    for (int j = 0; j < dt.Rows.Count; j++)
                                    {
                                        vslinfo += dt.Rows[j]["VesselName"].ToString() + ",";
                                    }
                                }



                                list.Add(new CPVessel()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["CPNo"].ToString(),
                                    StartDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["StartDate"]),
                                    EndDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EndDate"]),
                                    VoyageType = dataTable.Tables[0].Rows[i]["VoyageType"].ToString(),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows.Count),


                                    VesselInCP = vslinfo,
                                   
                                    CPName = dataTable.Tables[0].Rows[i]["CPName"].ToString(),
                                   



                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<UserDetail> SearchUserList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<UserDetail> list = new List<UserDetail>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchCommon", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", "");
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "UserList");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new UserDetail()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    UserEmail = dataTable.Tables[0].Rows[i]["UserEmail"].ToString(),
                                    FullName = dataTable.Tables[0].Rows[i]["FullName"].ToString(),
                                    UserName = dataTable.Tables[0].Rows[i]["UserName"].ToString(),
                                    UserType = dataTable.Tables[0].Rows[i]["UserType"].ToString(),
                                    AssignVessel = dataTable.Tables[0].Rows[i]["AssignVessel"].ToString(),
                                    //RankName = dataTable.Rows[i]["Ranks"].ToString(),
                                    DeptName = dataTable.Tables[0].Rows[i]["Departments"].ToString(),
                                    //RankId = Convert.ToInt32(dataTable.Rows[i]["RankId"]),
                                    DepId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["DepId"]),
                                    UserID = dataTable.Tables[0].Rows[i]["UserID"].ToString(),
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    

                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VesselDetail> SearchVesselPartList(int vesselid, int pageNo, int totalnoofPage, string firstS, string dateF, string dateT)
        {
            try
            {
                List<VesselDetail> list = new List<VesselDetail>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spSearchCommon", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@firstS", firstS);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateF", dateF);
                    adapter.SelectCommand.Parameters.AddWithValue("@dateT", dateT);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", "");
                    //adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    //adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "VesselPartList");

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new VesselDetail()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["VesselName"].ToString(),
                                    ImoNo = Convert.ToInt32(dataTable.Tables[0].Rows[i]["ImoNo"]),
                                    FleetType = dataTable.Tables[0].Rows[i]["FleetType"].ToString(),
                                    FleetName = dataTable.Tables[0].Rows[i]["FleetName"].ToString(),
                                    VesselTrade = dataTable.Tables[0].Rows[i]["VesselTrade"].ToString(),


                                });
                            }
                        }

                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<DailyNoonReport> GetNoonReportList1(int vesselid, int? pageno)
        {
            try
            {
                List<DailyNoonReport> list = new List<DailyNoonReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("DECLARE @PageNumber AS INT DECLARE @RowsOfPage AS INT SET @PageNumber="+pageno+" SET @RowsOfPage=10 SELECT * FROM DailyNoonReport ORDER BY Id OFFSET (@PageNumber-1)*@RowsOfPage ROWS FETCH NEXT @RowsOfPage ROWS ONLY", ConnectionBulder.con))
                {

                    //adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    //adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@Action", "DailyNoonReport");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new DailyNoonReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                   // voyagenumber = dataTable.Rows[i]["voyagenumber"].ToString(),
                                    AtSeaOrPort = dataTable.Rows[i]["AtSeaOrPort"].ToString(),
                                    Date = Convert.ToDateTime(dataTable.Rows[i]["Date"]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<ArrivalReport> GetArrivalReportList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {
                List<ArrivalReport> list = new List<ArrivalReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "ArrivalReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new ArrivalReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                   // voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                   // Place = dataTable.Tables[0].Rows[i]["Place"].ToString(),
                                   // EOSP = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EOSP"]),
                                   // SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                   // CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                   // TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),


                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    Place = dataTable.Tables[0].Rows[i]["Place"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Place"].ToString(),
                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),
                                    EOSP = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EOSP"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["EOSP"]),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),




                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<DepartureReport> GetDepartureReportList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {
                List<DepartureReport> list = new List<DepartureReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DepartureReport");

                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DepartureReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    
                                    //DeparturePort = dataTable.Tables[0].Rows[i]["DeparturePort"].ToString(),
                                    //NextPort = dataTable.Tables[0].Rows[i]["NextPort"].ToString(),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),


                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    DeparturePort = dataTable.Tables[0].Rows[i]["DeparturePort"].ToString() == "0" ? "" : dataTable.Tables[0].Rows[i]["DeparturePort"].ToString(),
                                    NextPort = dataTable.Tables[0].Rows[i]["NextPort"].ToString() == "0" ? "" : dataTable.Tables[0].Rows[i]["NextPort"].ToString(),
                                    VoyageId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VoyageId"]),
                                    // BerthName = dataTable.Tables[0].Rows[i]["BerthName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["BerthName"].ToString(),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ReportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDate"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),



                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<FreshWaterReport> GetFreshWaterRList(string VesselId, int pageNo, int totalnoofPage, string Voyage_no, string S_val = "")
        {

            if (Voyage_no == null)
            {
                Voyage_no = "";
            }
            try
            {
                List<FreshWaterReport> list = new List<FreshWaterReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "FreshWaterReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Voyage_no", Voyage_no);
                    adapter.SelectCommand.Parameters.AddWithValue("@Date", S_val);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables.Count > 1)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new FreshWaterReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    Facility_Name = dataTable.Tables[0].Rows[i]["Facility_Name"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Facility_Name"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VendorDetails = dataTable.Tables[0].Rows[i]["VendorDetails"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VendorDetails"].ToString(),
                                    Intial_Meter_Reading_MT_supplied = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Intial_Meter_Reading_MT_supplied"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Intial_Meter_Reading_MT_supplied"]),
                                    Final_Meter_Reading_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Final_Meter_Reading_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Final_Meter_Reading_MT"]),
                                    Difference_in_Meter_Reading_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Difference_in_Meter_Reading_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Difference_in_Meter_Reading_MT"]),
                                    QTY_supplied_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["QTY_supplied_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["QTY_supplied_MT"]),
                                    Received_Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Received_Date"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["Received_Date"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new FreshWaterReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    Facility_Name = dataTable.Tables[0].Rows[i]["Facility_Name"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Facility_Name"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VendorDetails = dataTable.Tables[0].Rows[i]["VendorDetails"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VendorDetails"].ToString(),
                                    Intial_Meter_Reading_MT_supplied = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Intial_Meter_Reading_MT_supplied"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Intial_Meter_Reading_MT_supplied"]),
                                    Final_Meter_Reading_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Final_Meter_Reading_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Final_Meter_Reading_MT"]),
                                    Difference_in_Meter_Reading_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["Difference_in_Meter_Reading_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["Difference_in_Meter_Reading_MT"]),
                                    QTY_supplied_MT = Convert.ToDecimal(dataTable.Tables[0].Rows[i]["QTY_supplied_MT"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["QTY_supplied_MT"]),
                                    Received_Date = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["Received_Date"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["Received_Date"]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<FreshWaterReport> editFreshWaterRList(int id, string Action)
        {

            List<FreshWaterReport> BunkerRList = new List<FreshWaterReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    BunkerRList = DataTableToList<FreshWaterReport>(dt);
                }

            }
            return BunkerRList;
        }


        public static List<BunkerReport> GetBunkerRList(string VesselId, int pageNo, int totalnoofPage, string Voyage_no)
        {

            if (Voyage_no == null)
            {
                Voyage_no = "";
            }
            try
            {
                List<BunkerReport> list = new List<BunkerReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "BunkerReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    adapter.SelectCommand.Parameters.AddWithValue("@Voyage_no", Voyage_no);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables.Count > 1)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new BunkerReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    Supplier = dataTable.Tables[0].Rows[i]["Supplier"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Supplier"].ToString(),
                                    BargeName = dataTable.Tables[0].Rows[i]["BargeName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["BargeName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["VesselId"]),
                                    // CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    // ReportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDate"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new BunkerReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    Supplier = dataTable.Tables[0].Rows[i]["Supplier"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["Supplier"].ToString(),
                                    BargeName = dataTable.Tables[0].Rows[i]["BargeName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["BargeName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["VesselId"]),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<BerthingReport> GetBerthingReportList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {
                List<BerthingReport> list = new List<BerthingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "BerthingReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new BerthingReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    //PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    //FacilityName = dataTable.Tables[0].Rows[i]["FacilityName"].ToString(),
                                    //BerthName = dataTable.Tables[0].Rows[i]["BerthName"].ToString(),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),




                                    voyagenumber = dataTable.Tables[0].Rows[i]["voyagenumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["voyagenumber"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    FacilityName = dataTable.Tables[0].Rows[i]["FacilityName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["FacilityName"].ToString(),
                                    BerthName = dataTable.Tables[0].Rows[i]["BerthName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["BerthName"].ToString(),
                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    ReportDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDate"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDate"]),

                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),



                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<PumpClass> GetPumpList(int? ImoNo)
        {
            try
            {
                List<PumpClass> list = new List<PumpClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", ImoNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "PumpList");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new PumpClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                    Name = dataTable.Rows[i]["Name"].ToString(),
                                    Capacity = Convert.ToInt32(dataTable.Rows[i]["Capacity"]),
                                    PumpType = dataTable.Rows[i]["type"].ToString(),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VoyageClass> GetVoyageList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {
                List<VoyageClass> list = new List<VoyageClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageList", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "VoyageList");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);

                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new VoyageClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    VoyageStartP = dataTable.Tables[0].Rows[i]["VoyageStartP"] == DBNull.Value ?  "" : dataTable.Tables[0].Rows[i]["VoyageStartP"].ToString(),
                                    VoyageEndP = dataTable.Tables[0].Rows[i]["VoyageEndP"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageEndP"].ToString(),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),
                                    //Nor_Conditions = Convert.ToInt32(dataTable.Rows[i]["Nor_Conditions"]),
                                    //HeavyWeather_BSS = Convert.ToInt32(dataTable.Rows[i]["HeavyWeather_BSS"]),
                                    //HeavyWeather_WH = Convert.ToDecimal(dataTable.Rows[i]["HeavyWeather_WH"]),
                                    //HeavyWeather_CV = Convert.ToDecimal(dataTable.Rows[i]["HeavyWeather_CV"]),
                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VoyageClass> GetVoyageLegList(int VesselId)
        {
            try
            {
                List<VoyageClass> list = new List<VoyageClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageListVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "VoyagelegList");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new VoyageClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Rows[i]["VoyageNumber"].ToString(),
                                    LegPort_A = dataTable.Rows[i]["LegPort_A"].ToString(),
                                    LegPort_B = dataTable.Rows[i]["LegPort_B"].ToString(),
                                    ReasonforPortCall_A = dataTable.Rows[i]["ReasonforPortCall_A"].ToString(),
                                    ReasonforPortCall_B = dataTable.Rows[i]["ReasonforPortCall_B"].ToString(),

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VoyageClass> GetFuelTList(int VesselId)
        {
            try
            {
                List<VoyageClass> list = new List<VoyageClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageListVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "FuelCList");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new VoyageClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Rows[i]["VoyageNumber"].ToString(),
                                    CP_Consumption = dataTable.Rows[i]["CP_Consumption"].ToString(),
                                    FuelT = dataTable.Rows[i]["FuelT"].ToString(),
                                   

                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<VesselDetail> edtiVesselList(int id)
        {           
            
            //con.Open();
            List<VesselDetail> vslList = new List<VesselDetail>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VesselDetail");
                DataTable dt = new DataTable();
                    adp.Fill(dt);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                    vslList.Add(new VesselDetail
                        {
                            Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                            VesselTradeID = Convert.ToInt32(dt.Rows[i]["VesselTradeID"]),
                            FleetNameID = Convert.ToInt32(dt.Rows[i]["FleetNameID"]),
                            FleetTypeID = Convert.ToInt32(dt.Rows[i]["FleetTypeID"]),
                            ImoNo = Convert.ToInt32(dt.Rows[i]["ImoNo"]),
                            VesselName = (dt.Rows[i]["VesselName"]).ToString(),
                            Displacement = Convert.ToDecimal(dt.Rows[i]["Displacement"])
                        });
                    }
                    // con.Close();
                }               
                return vslList;
        }

        public static List<VoyageClass> edtiVoyageList(int id)
        {

            //con.Open();
            List<VoyageClass> vslList = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageDetails");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    vslList.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"] == DBNull.Value ? 0 : dt.Rows[i]["Id"]),
                        VoyageNumber = dt.Rows[i]["VoyageNumber"] == DBNull.Value ? "" : dt.Rows[i]["VoyageNumber"].ToString(),
                       // Conditions = dt.Rows[i]["conditions"] == DBNull.Value ? "" : dt.Rows[i]["conditions"].ToString(),
                        Nor_Conditions = Convert.ToInt32(dt.Rows[i]["Nor_Conditions"] == DBNull.Value ? 0 : dt.Rows[i]["Nor_Conditions"]),
                        CPId = Convert.ToInt32(dt.Rows[i]["CPId"] == DBNull.Value ? 0 : dt.Rows[i]["CPId"]),
                        //VoyageStartPoint = Convert.ToInt32(dt.Rows[i]["VoyageStartPoint"]),
                        //ETA = Convert.ToDateTime(dt.Rows[i]["ETA"]),                      
                        //DTG = Convert.ToInt32(dt.Rows[i]["DTG"]),
                        //CP_SOG = Convert.ToDecimal(dt.Rows[i]["CP_SOG"]),
                        //CP_Log_Speed = Convert.ToDecimal(dt.Rows[i]["CP_Log_Speed"]),
                        //FW = Convert.ToDecimal(dt.Rows[i]["FW"]),
                        HeavyWeather_BSS = Convert.ToInt32(dt.Rows[i]["HeavyWeather_BSS"] == DBNull.Value ? 0 : dt.Rows[i]["HeavyWeather_BSS"]),
                        HeavyWeather_WH = Convert.ToDecimal(dt.Rows[i]["HeavyWeather_WH"] == DBNull.Value ? 0 : dt.Rows[i]["HeavyWeather_WH"]),
                        HeavyWeather_CV = Convert.ToDecimal(dt.Rows[i]["HeavyWeather_CV"] == DBNull.Value ? 0 : dt.Rows[i]["HeavyWeather_CV"]),
                    });
                }
               
            }
            return vslList;
        }

        public static List<VoyageClass> viewVoyageLegList(int id)
        {

            //con.Open();
            List<VoyageClass> vslList = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageLegDetails");
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageLegDetailsEdit");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    vslList.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        LegPort_A = dt.Rows[i]["LegPort_A"].ToString(),
                        LegPort_B = dt.Rows[i]["LegPort_B"].ToString(),
                        ReasonforPortCall_A = dt.Rows[i]["ReasonforPortCall_A"].ToString(),
                        ReasonforPortCall_B = dt.Rows[i]["ReasonforPortCall_B"].ToString(),
                        ReasonA = dt.Rows[i]["ReasonA"].ToString(),
                        ReasonB = dt.Rows[i]["ReasonB"].ToString(),
                        DTG = Convert.ToDecimal(dt.Rows[i]["DTG"]),
                        CP_SOG = Convert.ToDecimal(dt.Rows[i]["CP_SOG"]),
                        CP_Log_Speed = Convert.ToDecimal(dt.Rows[i]["CP_Log_Speed"]),
                    });
                }
                // con.Close();
            }
            return vslList;
        }
        public static List<VoyageClass> edtiVoyageLegList(int id)
        {

            //con.Open();
            List<VoyageClass> vslList = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageLegDetails");
                adp.SelectCommand.Parameters.AddWithValue("@Action", "VoyageLegDetailsEdit");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    vslList.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        LegPort_A = dt.Rows[i]["LegPort_A"].ToString(),
                        LegPort_B = dt.Rows[i]["LegPort_B"].ToString(),
                        ReasonforPortCall_A = dt.Rows[i]["ReasonforPortCall_A"].ToString(),
                        ReasonforPortCall_B = dt.Rows[i]["ReasonforPortCall_B"].ToString(),
                        ReasonA = dt.Rows[i]["ReasonA"].ToString(),
                        ReasonB = dt.Rows[i]["ReasonB"].ToString(),
                        DTG = Convert.ToDecimal(dt.Rows[i]["DTG"]),
                        CP_SOG = Convert.ToDecimal(dt.Rows[i]["CP_SOG"]),
                        CP_Log_Speed = Convert.ToDecimal(dt.Rows[i]["CP_Log_Speed"]),
                    });
                }
                // con.Close();
            }
            return vslList;
        }

        public static List<VoyageClass> edtiFuelCList(int id)
        {

            //con.Open();
            List<VoyageClass> vslList = new List<VoyageClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "FuelCDetails");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    vslList.Add(new VoyageClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        FuelTypeId = Convert.ToInt32(dt.Rows[i]["FuelTypeId"]),
                        CP_Consumption = dt.Rows[i]["CP_cons_perday_HFO"].ToString(),
                       FuelT = dt.Rows[i]["FuelType"].ToString(),
                    });
                }
                // con.Close();
            }
            return vslList;
        }

        public static List<FreshWaterReport> editFreshWaterList(int id, string Action)
        {

            //con.Open();
            List<FreshWaterReport> List = new List<FreshWaterReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    List = DataTableToList<FreshWaterReport>(dt);
                }

            }
            return List;
        }

        public static List<BunkerReport> editBunkerRList(int id, string Action)
        {

            //con.Open();
            List<BunkerReport> BunkerRList = new List<BunkerReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    BunkerRList = DataTableToList<BunkerReport>(dt);
                }

            }
            return BunkerRList;
        }

        public static List<BukerFuelList> editBunkerFuelList(int id, string vslid, string Action)
        {

            //con.Open();
            List<BukerFuelList> BunkerFList = new List<BukerFuelList>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@VesselId", vslid);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    BunkerFList = DataTableToList<BukerFuelList>(dt);
                }

            }
            return BunkerFList;
        }


        public static List<DailyNoonReport> editnoonRList(int id,string Action)
        {

            //con.Open();
            List<DailyNoonReport> noonRDetails = new List<DailyNoonReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count> 0)
                {                   
                    noonRDetails = DataTableToList<DailyNoonReport>(dt);
                }
               
            }
            return noonRDetails;
        }


        public static string editnoonR(int id, string Action)
        {
            string date = "";
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    date = dt.Rows[0][0].ToString();
                    
                }

            }
            return date;
        }

        public static List<DischargingReport> editdischargingRList(int id, string Action)
        {

            //con.Open();
            List<DischargingReport> disRDetails = new List<DischargingReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //disRDetails = ConvertDataTable<DischargingReport>(dt);
                    disRDetails = DataTableToList<DischargingReport>(dt);
                }
                // con.Close();
            }
            return disRDetails;
        }

        public static List<LoadingReport> editloadingRList(int id, string Action)
        {

            //con.Open();
            List<LoadingReport> noonRDetails = new List<LoadingReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    noonRDetails = DataTableToList<LoadingReport>(dt);
                }
                // con.Close();
            }
            return noonRDetails;
        }


        public static List<ArrivalReport> editarrivalRList(int id, string Action)
        {

            //con.Open();
            List<ArrivalReport> arrRDetails = new List<ArrivalReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    arrRDetails = DataTableToList<ArrivalReport>(dt);
                }
                // con.Close();
            }
            return arrRDetails;
        }

        public static List<DepartureReport> editdepartureRList(int id, string Action)
        {

            //con.Open();
            List<DepartureReport> depRDetails = new List<DepartureReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    depRDetails = DataTableToList<DepartureReport>(dt);
                }
                // con.Close();
            }
            return depRDetails;
        }


        public static List<BerthingReport> editberthingRList(int id, string Action)
        {

            //con.Open();
            List<BerthingReport> berRDetails = new List<BerthingReport>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", Action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    berRDetails = DataTableToList<BerthingReport>(dt);
                }
                // con.Close();
            }
            return berRDetails;
        }

        public static List<PumpClass> edtiPumpList(int id)
        {
          
            //con.Open();
            List<PumpClass> pmpList = new List<PumpClass>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", id);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "PumpDetail");
                DataTable dt = new DataTable();
                adp.Fill(dt);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    pmpList.Add(new PumpClass
                    {
                        Id = Convert.ToInt32(dt.Rows[i]["Id"]),
                        PumpTypeId = Convert.ToInt32(dt.Rows[i]["PumpTypeId"]),
                        Name = (dt.Rows[i]["Name"]).ToString(),
                        Capacity = Convert.ToInt32(dt.Rows[i]["Capacity"]),
                    });
                }
                // con.Close();
            }
            return pmpList;
        }


        public static string[] GetVesselName1(int? ImoNo)
        {
            string[] arr = new string[2];
           

            using (SqlDataAdapter adp = new SqlDataAdapter("GetVesselName", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@ImoNo", ImoNo);

                DataTable dt = new DataTable();
                adp.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    arr[0] = dt.Rows[0][0].ToString();
                    arr[1] = dt.Rows[0][1].ToString();
                }


                // con.Close();
            }
           // return VName;

            return arr;
        }

        public static List<CharterPartyC> CharterPList(int vesselid)
        {

            List<CharterPartyC> ftype = new List<CharterPartyC>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select distinct a.id,a.cpno from CPContract a inner join CPpart1 b on a.Id=b.CPId where VesselID=" + vesselid + "", ConnectionBulder.con))
            {
                //adp.SelectCommand.CommandType = CommandType.StoredProcedure;
               // adp.SelectCommand.Parameters.AddWithValue("@Action", "CPList");
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

        public static string GetVesselName(int? ImoNo)
        {
            string VName = "";
           
            using (SqlDataAdapter adp = new SqlDataAdapter("GetVesselName", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@ImoNo", ImoNo);
               
                DataTable dt = new DataTable();
                adp.Fill(dt);
                if(dt.Rows.Count>0)
                {
                    VName = dt.Rows[0][0].ToString();
                }

                
                // con.Close();
            }
            return VName;
        }

        

        public static List<TanksAndHolds> viewTanksandHoldsList(int typeid,int vesselid)
        {

          
            List<TanksAndHolds> tanksList = new List<TanksAndHolds>();
            using (SqlDataAdapter adp = new SqlDataAdapter("TanksNdHoldsList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@typeId", typeid);
                adp.SelectCommand.Parameters.AddWithValue("@vesselId", vesselid);
                //adp.SelectCommand.Parameters.AddWithValue("@Action", "TanksAndHolds");
                DataSet dt = new DataSet();
                adp.Fill(dt);
                for (int i = 0; i < dt.Tables[0].Rows.Count; i++)
                {
                    tanksList.Add(new TanksAndHolds
                    {
                        Id = Convert.ToInt32(dt.Tables[0].Rows[i]["Id"]),
                        Name = dt.Tables[0].Rows[i]["Name"].ToString(),
                        Height = Convert.ToDecimal(  dt.Tables[0].Rows[i]["Height"]),
                        Capacity = Convert.ToDecimal(dt.Tables[0].Rows[i]["Capacity"]),
                        TanksTypeId = Convert.ToInt32(dt.Tables[0].Rows[i]["TanksTypeId"]),
                      
                    });
                }
         

            }
            return tanksList;
        }
        public static List<TanksAndHolds> editTanksandHoldsList(int typeid)
        {

            List<TanksAndHolds> tanksList = new List<TanksAndHolds>();
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonEditList", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@Id", typeid);
                adp.SelectCommand.Parameters.AddWithValue("@Action", "TanksAndHolds");
                DataSet dt = new DataSet();
                adp.Fill(dt);
            
                for (int i = 0; i < dt.Tables[1].Rows.Count; i++)
                {
                    tanksList.Add(new TanksAndHolds
                    {
                        Id = Convert.ToInt32(dt.Tables[1].Rows[i]["Id"]),
                        Name = dt.Tables[1].Rows[i]["Name"].ToString(),
                        Height = Convert.ToDecimal(dt.Tables[1].Rows[i]["Height"]),
                        Capacity = Convert.ToDecimal(dt.Tables[1].Rows[i]["Capacity"]),
                        TanksTypeId = Convert.ToInt32(dt.Tables[1].Rows[i]["TanksTypeId"]),

                    });
                }

            }
            return tanksList;
        }

        public static int GetMaxSortingID(int tanktpid,string action)
        {
            int getmaxid = 0;
            using (SqlDataAdapter adp = new SqlDataAdapter("spGetMaxSortingOrdId", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@tankstypeid", tanktpid);
                adp.SelectCommand.Parameters.AddWithValue("@Action", action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                getmaxid = Convert.ToInt32(dt.Rows[0][0]);
                

            }
            return getmaxid;
        }

        public static int GetFuelConslist( string action)
        {
            int totalC = 0;
            using (SqlDataAdapter adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
            {
                adp.SelectCommand.CommandType = CommandType.StoredProcedure;
               
                adp.SelectCommand.Parameters.AddWithValue("@Action", action);
                DataTable dt = new DataTable();
                adp.Fill(dt);
                totalC = Convert.ToInt32(dt.Rows.Count);


            }
            return totalC;
        }

        public static void InsertCPContrect(CharterPartyClass CP, string Action, out int Exist)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("AddCPcontract", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@Id", CP.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@CPNo", CP.CPNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@StartDate", CP.StartDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@EndDate", CP.EndDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageType", CP.VoyageType);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    //adapter.SelectCommand.Parameters.AddWithValue("@VesselId", CP.VesselID);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    adapter.SelectCommand.Parameters.AddWithValue("@Exist", 0);
                    adapter.SelectCommand.Parameters["@Exist"].Direction = ParameterDirection.Output;
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        Exist = int.Parse(adapter.SelectCommand.Parameters["@Exist"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertCPpart1(CPVessel CP, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("AddCPpart1", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", CP.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@CPId", CP.CPId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselID", CP.VesselID);
                    adapter.SelectCommand.Parameters.AddWithValue("@StartDate", CP.StartDate);
                    adapter.SelectCommand.Parameters.AddWithValue("@EndDate", CP.EndDate);
                    //adapter.SelectCommand.Parameters.AddWithValue("@VoyageType", CP.VoyageType);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertCPpart2(CPMainCONSUMPTIONClass CP, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("AddCPpart2", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", CP.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@CPId", CP.CPId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Speed", CP.Speed);
                    adapter.SelectCommand.Parameters.AddWithValue("@ME_LADEN", CP.ME_LADEN);
                    adapter.SelectCommand.Parameters.AddWithValue("@ME_BALLAST", CP.ME_BALLAST);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_VLSFO", CP.AE_VLSFO);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_DO", CP.AE_DO);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void InsertCPpart3(CPOtherCONSUMPTIONClass CP, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("AddCPpart3", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", CP.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@CPId", CP.CPId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Consumption", CP.Consumption);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_VLSFO", CP.AE_VLSFO);
                    adapter.SelectCommand.Parameters.AddWithValue("@AE_MDO", CP.AE_MDO);
                    adapter.SelectCommand.Parameters.AddWithValue("@BOILER_MDO", CP.BOILER_MDO);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public static List<T> GetListOfTypeFromRunningAssembly<T>()
        //{
        //    List<object> TypeList = new List<object>();
        //    //Assembly a = GetRunningAssembly();
        //    foreach (Type t in a.GetTypes())
        //    {
        //        if (t is T)
        //        {
        //            TypeList.Add(t);
        //        }
        //    }
        //    return TypeList as List<T>;
        //}


        public static List<CPVessel> GetCharterPartyListAdmin(int pageNo, int totalnoofPage)
        {
            try
            {
               
                List<CPVessel> cPVessels = new List<CPVessel>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageList", ConnectionBulder.con))
                {
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        adapter.SelectCommand.Parameters.AddWithValue("@VesselId", "");
                        adapter.SelectCommand.Parameters.AddWithValue("@Action", "CPAdminList");
                        adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                        adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                cPVessels.Add(new CPVessel()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["CPNo"].ToString(),
                                    StartDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["StartDate"]),
                                    EndDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EndDate"]),
                                    //VoyageType = dataTable.Rows[i]["VoyageType"].ToString(),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                    }
                }
                return cPVessels;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static List<CPVessel> GetCharterPartyList(int VId,int pageNo, int totalnoofPage)
        {
            try
            {
                int VesselId = Convert.ToInt32(VId);
                List<CPVessel> cPVessels = new List<CPVessel>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageList", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "CPVoyageList");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {

                                int cpid = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]);
                                string vslinfo = "";
                                using (SqlDataAdapter adp = new SqlDataAdapter("select a.*,v.VesselName,cp.CPNo from CPpart1 a join VesselDetail v on a.VesselID=v.ImoNo join CPContract cp on cp.Id=a.CPId where a.IsActive = 1 and  a.CPId = " + cpid + "", ConnectionBulder.con))
                                {
                                    DataTable dt = new DataTable();
                                    adp.Fill(dt);
                                    for (int j = 0; j < dt.Rows.Count; j++)
                                    {
                                        vslinfo += dt.Rows[j]["VesselName"].ToString() + ",";
                                    }
                                }



                                cPVessels.Add(new CPVessel()
                                {
                                    VesselInCP = vslinfo,
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    VesselName = dataTable.Tables[0].Rows[i]["CPNo"].ToString(),
                                    CPName = dataTable.Tables[0].Rows[i]["CPName"].ToString(),
                                    StartDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["StartDate"]),
                                    EndDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["EndDate"]),
                                    VoyageType = dataTable.Tables[0].Rows[i]["VoyageType"].ToString(),
                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                });
                            }
                        }
                    }
                }
                return cPVessels;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static CharterPartyClass GetCharterPartyDetails(int Id)
        {
            try
            {
                CharterPartyClass cpdetail = new CharterPartyClass();
                using (SqlDataAdapter adapter = new SqlDataAdapter("GetCharterPartyDetail", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                    using (DataSet Dset = new DataSet())
                    {
                        adapter.Fill(Dset);
                        // Charter party detail
                        if (Dset.Tables[0].Rows.Count > 0)
                        {
                            cpdetail.Id = Convert.ToInt32(Dset.Tables[0].Rows[0]["Id"]);
                            cpdetail.CPNo = Dset.Tables[0].Rows[0]["CPNo"].ToString();
                            cpdetail.CPName = Dset.Tables[0].Rows[0]["CPName"].ToString();
                            cpdetail.StartDate = Convert.ToDateTime(Dset.Tables[0].Rows[0]["StartDate"]);

                            cpdetail.EndDate = Convert.ToDateTime(Dset.Tables[0].Rows[0]["EndDate"]);
                            cpdetail.VoyageType = Dset.Tables[0].Rows[0]["VoyageType"].ToString();
                            //cpdetail.CreatedDate = Convert.ToDateTime(Dset.Tables[1].Rows[i]["CreatedDate"]);
                        }

                        // charter party vessel detail
                        if (Dset.Tables[1].Rows.Count > 0)
                        {
                            int count = Dset.Tables[1].Rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                cpdetail.CPVesselInfo.Add(new CPVessel()
                                {
                                    Id = Convert.ToInt32(Dset.Tables[1].Rows[i]["Id"]),
                                    CPId = Convert.ToInt32(Dset.Tables[1].Rows[i]["CPId"]),
                                    VesselName = Dset.Tables[1].Rows[i]["VesselName"].ToString(),
                                    StartDate = Convert.ToDateTime(Dset.Tables[1].Rows[i]["StartDate"]),
                                    EndDate = Convert.ToDateTime(Dset.Tables[1].Rows[i]["EndDate"]),
                                    //VoyageType = Dset.Tables[1].Rows[i]["VoyageType"].ToString(),
                                    CreatedDate = Convert.ToDateTime(Dset.Tables[1].Rows[i]["CreatedDate"]),
                                    VesselID = Convert.ToInt32(Dset.Tables[1].Rows[i]["VesselID"]),

                                });
                            }
                        }

                        // CP Main consumption
                        if (Dset.Tables[2].Rows.Count > 0)
                        {
                            int count = Dset.Tables[2].Rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                cpdetail.MainCONSUMPTION.Add(new CPMainCONSUMPTIONClass()
                                {
                                    Id = Convert.ToInt32(Dset.Tables[2].Rows[i]["Id"]),
                                    CPId = Convert.ToInt32(Dset.Tables[2].Rows[i]["CPId"]),
                                    Speed = Convert.ToDecimal(Dset.Tables[2].Rows[i]["Speed"]),
                                    AE_VLSFO = Convert.ToDecimal(Dset.Tables[2].Rows[i]["AE_VLSFO"]),
                                    AE_DO = Convert.ToDecimal(Dset.Tables[2].Rows[i]["AE_DO"]),
                                    ME_LADEN = Convert.ToDecimal(Dset.Tables[2].Rows[i]["ME_LADEN"]),
                                    ME_BALLAST = Convert.ToDecimal(Dset.Tables[2].Rows[i]["ME_BALLAST"]),

                                });
                            }

                        }
                        // CP Other consumption
                        if (Dset.Tables[3].Rows.Count > 0)
                        {
                            int count = Dset.Tables[3].Rows.Count;
                            for (int i = 0; i < count; i++)
                            {
                                cpdetail.OtherCONSUMPTION.Add(new CPOtherCONSUMPTIONClass()
                                {
                                    Id = Convert.ToInt32(Dset.Tables[3].Rows[i]["Id"]),
                                    CPId = Convert.ToInt32(Dset.Tables[3].Rows[i]["CPId"]),
                                    Consumption = Dset.Tables[3].Rows[i]["Consumption"].ToString(),
                                    AE_VLSFO = Convert.ToDecimal(Dset.Tables[3].Rows[i]["AE_VLSFO"]),
                                    AE_MDO = Convert.ToDecimal(Dset.Tables[3].Rows[i]["AE_MDO"]),
                                    BOILER_MDO = Convert.ToDecimal(Dset.Tables[3].Rows[i]["BOILER_MDO"]),


                                });
                            }

                        }
                    }
                }
                return cpdetail;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static DataTable GetCharterPartyByID(int id, int CPId, string TblName)
        {
            //[DeleteCharterPartyByID]
            using (SqlDataAdapter adapter = new SqlDataAdapter("GetCharterPartyByID", ConnectionBulder.con))
            {

                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                adapter.SelectCommand.Parameters.AddWithValue("@CPId", CPId);
                adapter.SelectCommand.Parameters.AddWithValue("@TblName", TblName);

                using (DataTable dataTable = new DataTable())
                {
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }

        }

        public static void DeleteCharterPartyByID(int id, int CPId, string TblName)
        {

            using (SqlDataAdapter adapter = new SqlDataAdapter("DeleteCharterPartyByID", ConnectionBulder.con))
            {

                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("@Id", id);
                adapter.SelectCommand.Parameters.AddWithValue("@CPId", CPId);
                adapter.SelectCommand.Parameters.AddWithValue("@TblName", TblName);

                using (DataTable dataTable = new DataTable())
                {
                    adapter.Fill(dataTable);

                }
            }

        }

        public static List<VoyageClass> bindleg(int voyid)
        {
          
            List<VoyageClass> ftype = new List<VoyageClass>();
            //using (SqlDataAdapter adp = new SqlDataAdapter("select id, '(Port A - '+ LegPort_A +') - Port B - '+ legport_b as Leg from VoyageLeg where voyageid='" + voyid + "'", ConnectionBulder.con))
            using (SqlDataAdapter adp = new SqlDataAdapter("select id, LegPort_A +' to '+ legport_b as Leg from VoyageLeg  where voyageid='" + voyid + "'", ConnectionBulder.con))
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








        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new()
        {
            List<T> lstItems = new List<T>();
            if (dt != null && dt.Rows.Count > 0)
                foreach (DataRow row in dt.Rows)
                    lstItems.Add(ConvertDataRowToGenericType<T>(row));
            else
                lstItems = null;
            return lstItems;
        }

        private static T ConvertDataRowToGenericType<T>(DataRow row) where T : class, new()
        {
            Type entityType = typeof(T);
            T objEntity = new T();
            foreach (DataColumn column in row.Table.Columns)
            {
                object value = row[column.ColumnName];
                if (value == DBNull.Value) value = null;
                PropertyInfo property = entityType.GetProperty(column.ColumnName, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                try
                {
                    if (property != null && property.CanWrite)
                        property.SetValue(objEntity, value, null);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return objEntity;
        }






        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            try
            {
               

                foreach (DataColumn column in dr.Table.Columns)
                {
                    foreach (PropertyInfo pro in temp.GetProperties())
                    {
                        if (pro.Name == column.ColumnName)
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        else
                            continue;
                    }
                }
               
            }
            catch (Exception ex) { }
            return obj;
        }


        public static int GetMaxIDLoad_DischargeReport(string TableName)
        {
            // select max(Id) from LoadingReport
            int MID = 0;
            using (SqlDataAdapter adapter = new SqlDataAdapter("select max(Id) from " + TableName + "", ConnectionBulder.con))
            {
                using (DataTable dataTable = new DataTable())
                {
                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        MID = Convert.ToInt32(dataTable.Rows[0][0]);
                    }
                }
            }
            return MID;
        }

        public static DischargingReport GetSingleLoadingReport(int Id, int VesselID, string TableLoadingDischarge)
        {
            // select max(Id) from LoadingReport
            DischargingReport LRD = new DischargingReport();
            using (SqlDataAdapter adapter = new SqlDataAdapter("select * from " + TableLoadingDischarge + " where Id=" + Id + " and VesselId = " + VesselID + "", ConnectionBulder.con))
            {
                using (DataTable dataTable = new DataTable())
                {
                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        LRD.Id = Convert.ToInt32(dataTable.Rows[0]["Id"]);
                        LRD.VoyageId = Convert.ToInt32(dataTable.Rows[0]["VoyageId"]);
                        LRD.LegPortId = Convert.ToInt32(dataTable.Rows[0]["LegPortId"]);
                        LRD.PortName = dataTable.Rows[0]["PortName"].ToString();
                    }
                }
            }
            return LRD;
        }

        public static void InsertUpdateDischargeReport(DischargingReport LR, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertDischargeReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", LR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", LR.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", LR.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReportDateTime", LR.ReportDateTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@ETDDateTime", LR.ETDDateTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", LR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", LR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid", LR.DraftMid);


                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", LR.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifyDate", DateTime.Now.Date);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LR.VesselId);

                    adapter.SelectCommand.Parameters.AddWithValue("@Power_Packs_onboard", LR.Power_Packs_onboard);
                    adapter.SelectCommand.Parameters.AddWithValue("@Power_Packs_Used", LR.Power_Packs_Used);

                    adapter.SelectCommand.Parameters.AddWithValue("@Times", LR.Times);
                    adapter.SelectCommand.Parameters.AddWithValue("@Rate", LR.Rate);
                    adapter.SelectCommand.Parameters.AddWithValue("@Hose_Connection", LR.Hose_Connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@High_H2S", LR.High_H2S);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", LR.SaveDraft);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public static void InsertUpdateLoadingReport(LoadingReport LR, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertLoadingReport", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LR.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", LR.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", LR.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", LR.PortName);
                    adapter.SelectCommand.Parameters.AddWithValue("@ReportDateTime", LR.ReportDateTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@ETDDateTime", LR.ETDDateTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftFwd", LR.DraftFwd);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftAft", LR.DraftAft);
                    adapter.SelectCommand.Parameters.AddWithValue("@DraftMid", LR.DraftMid);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BallastPumps_Rate1", LR.BallastPumps_Rate1);
                    //adapter.SelectCommand.Parameters.AddWithValue("@BallastPumps_Rate2", LR.BallastPumps_Rate2);

                    adapter.SelectCommand.Parameters.AddWithValue("@Remarks", LR.Remarks);
                    adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", DateTime.Now.Date);
                    adapter.SelectCommand.Parameters.AddWithValue("@ModifyDate", DateTime.Now.Date);
                    adapter.SelectCommand.Parameters.AddWithValue("@IsActive", true);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LR.VesselId);

                    adapter.SelectCommand.Parameters.AddWithValue("@Times", LR.Times);
                    adapter.SelectCommand.Parameters.AddWithValue("@Rate", LR.Rate);
                    adapter.SelectCommand.Parameters.AddWithValue("@Hose_Connection", LR.Hose_Connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@High_H2S", LR.High_H2S);
                    adapter.SelectCommand.Parameters.AddWithValue("@SaveDraft", LR.SaveDraft);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void InsertUpdateLoadingCargo(CargoList LRC, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertLoadingCargoList", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LRC.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@LRId", LRC.LRId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LRC.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@CargoName", LRC.CargoName);

                    if (LRC.LoadingDatetime == null)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadingDatetime", DBNull.Value);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@LoadingDatetime", LRC.LoadingDatetime);
                    }
                    adapter.SelectCommand.Parameters.AddWithValue("@TerminalLoadingRate", LRC.TerminalLoadingRate);
                    adapter.SelectCommand.Parameters.AddWithValue("@LoadingRateAccepted", LRC.LoadingRateAccepted);
                    adapter.SelectCommand.Parameters.AddWithValue("@AverageAchievedLoadingRate", LRC.AverageAchievedLoadingRate);
                    adapter.SelectCommand.Parameters.AddWithValue("@No_Manifold_Hoses_by_Terminal", LRC.No_Manifold_Hoses_by_Terminal);
                    adapter.SelectCommand.Parameters.AddWithValue("@Size_of_Manifold_Hoses_by_Terminal", LRC.Size_of_Manifold_Hoses_by_Terminal);
                    adapter.SelectCommand.Parameters.AddWithValue("@No_Manifold_Hoses_by_Vessel", LRC.No_Manifold_Hoses_by_Vessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@Size_of_Manifold_Hoses_by_Vessel", LRC.Size_of_Manifold_Hoses_by_Vessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@ShoreLineDistance", LRC.ShoreLineDistance);
                    adapter.SelectCommand.Parameters.AddWithValue("@QuantityOnboard", LRC.QuantityOnboard);
                    adapter.SelectCommand.Parameters.AddWithValue("@BalanceQuantityLoaded", LRC.BalanceQuantityLoaded);

                    if (LRC.EstCompDateTime == null)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@EstCompDateTime", DBNull.Value);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@EstCompDateTime", LRC.EstCompDateTime);
                    }

                    if (LRC.ActualCompDateTime == null)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@ActualCompDateTime", DBNull.Value);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@ActualCompDateTime", LRC.ActualCompDateTime);
                    }

                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", LRC.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", LRC.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", LRC.PortName);

                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void InsertUpdateDischargingCargo(DSCargoList LRC, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertDischargeingCargoList", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LRC.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@LRId", LRC.DSId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LRC.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@CId", LRC.CId);
                    adapter.SelectCommand.Parameters.AddWithValue("@CargoName", LRC.CargoName);
                    adapter.SelectCommand.Parameters.AddWithValue("@DischargeDatetime", LRC.DischargeDatetime);
                    adapter.SelectCommand.Parameters.AddWithValue("@Terminal_Acceptable_Discharging_Rate", LRC.Terminal_Acceptable_Discharging_Rate);
                    adapter.SelectCommand.Parameters.AddWithValue("@Discharging_pressure_Requested", LRC.Discharging_pressure_Requested);
                    adapter.SelectCommand.Parameters.AddWithValue("@Average_Discharge_Rate_ByVessel", LRC.Average_Discharge_Rate_ByVessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@Average_Discharge_pressure_ByVessel", LRC.Average_Discharge_pressure_ByVessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@No_of_Pumps_Use", LRC.No_of_Pumps_Use);
                    adapter.SelectCommand.Parameters.AddWithValue("@No_Manifold_Hoses_by_Terminal", LRC.No_Manifold_Hoses_by_Terminal);
                    adapter.SelectCommand.Parameters.AddWithValue("@Size_of_Manifold_Hoses_by_Terminal", LRC.Size_of_Manifold_Hoses_by_Terminal);
                    adapter.SelectCommand.Parameters.AddWithValue("@No_Manifold_Hoses_by_Vessel", LRC.No_Manifold_Hoses_by_Vessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@Size_of_Manifold_Hoses_by_Vessel", LRC.Size_of_Manifold_Hoses_by_Vessel);
                    adapter.SelectCommand.Parameters.AddWithValue("@Total_CargoDischarged", LRC.Total_CargoDischarged);
                    adapter.SelectCommand.Parameters.AddWithValue("@Balance_Cargo_ToBe_Deischarged", LRC.Balance_Cargo_ToBe_Deischarged);
                    adapter.SelectCommand.Parameters.AddWithValue("@ActualCompDateTime", LRC.ActualCompDateTime);
                    adapter.SelectCommand.Parameters.AddWithValue("@EstCompDateTime", LRC.EstCompDateTime);

                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", LRC.VoyageId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LegPortId", LRC.LegPortId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PortName", LRC.PortName);

                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static void InsertUpdateBunkerFuelType(BukerFuelList BFL/*, string Action*/)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertBunkerFuelType", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", BFL.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@Fuel_type_Id", BFL.Fuel_type_Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@MaxR_Id", BFL.MaxR_Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", BFL.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@BDN", BFL.BDN);
                    adapter.SelectCommand.Parameters.AddWithValue("@BDN_Number", BFL.BDN_Number);
                    adapter.SelectCommand.Parameters.AddWithValue("@Fuel_Density", BFL.Fuel_Density);
                    adapter.SelectCommand.Parameters.AddWithValue("@Sulphur_content", BFL.Sulphur_content);
                   // adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command); @LoadingDischarged
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void InsertUpdateLoadingStoppage(StoppageList LRS, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertLoadingStoppage", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LRS.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@LRId", LRS.LRId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LRS.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Stoppage", LRS.Stoppage);
                    //adapter.SelectCommand.Parameters.AddWithValue("@Reason", LRS.Reason);
                    adapter.SelectCommand.Parameters.AddWithValue("@Reason", "N/A");

                    if (LRS.DateTimeFrom == null)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@DateTimeFrom", DBNull.Value);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@DateTimeFrom", LRS.DateTimeFrom);
                    }

                    if (LRS.DateTimeTo == null)
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@DateTimeTo", DBNull.Value);
                    }
                    else
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@DateTimeTo", LRS.DateTimeTo);
                    }

                    //adapter.SelectCommand.Parameters.AddWithValue("@DateTimeFrom", LRS.DateTimeFrom);
                    //adapter.SelectCommand.Parameters.AddWithValue("@DateTimeTo", LRS.DateTimeTo);
                    adapter.SelectCommand.Parameters.AddWithValue("@DCId", LRS.DCId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LoadingDischarged", LRS.LoadingDischarged);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command); @LoadingDischarged
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static void InsertUpdateLoadingBallast_PumpUse(LR_DCR_PumpsUse LRS, string Action)
        {
            try
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertLoading_DischargePumpsUsed", ConnectionBulder.con))
                {
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@Id", LRS.Id);
                    adapter.SelectCommand.Parameters.AddWithValue("@PumpUseId", LRS.PumpUseId);
                    adapter.SelectCommand.Parameters.AddWithValue("@LRId", LRS.LRId);
                    adapter.SelectCommand.Parameters.AddWithValue("@DCRId", LRS.DCRId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", LRS.VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@PumpId", LRS.PumpId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Rate", LRS.Rate);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", Action);
                    // SqlDataAdapter adapter = new SqlDataAdapter(command);
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);

                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<LoadingReport> GetCommonLoadingReportsList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {// "LoadingReport"
                List<LoadingReport> list = new List<LoadingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "LoadingReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new LoadingReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                   // VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["VesselId"]),
                                    ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDateTime"]),

                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),

                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),

                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),




                                    //LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    // PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    //VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"]),
                                    //ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"]),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),
                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static DataSet ExportReportsTables(ExportPeram peram, int vesselId)
        {
            try
            {

                using (SqlDataAdapter adapter = new SqlDataAdapter("ExportReportsTable", ConnectionBulder.con))
                {
                    string VoyageId = string.Join(",", peram.VoyageIds);

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", vesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", VoyageId);
                    // adapter.SelectCommand.Parameters.AddWithValue("@DateFrom", peram.FromDate);
                    // adapter.SelectCommand.Parameters.AddWithValue("@DateTo", peram.ToDate);
                    using (DataSet dataTables = new DataSet())
                    {
                        adapter.Fill(dataTables);
                        return dataTables;
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static List<DischargingReport> GetCommonDischargingReportsList(int VesselId, int pageNo, int totalnoofPage)
        {
            try
            {// "LoadingReport"
                List<DischargingReport> list = new List<DischargingReport>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spCommonListsVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "DischargingReport");
                    adapter.SelectCommand.Parameters.AddWithValue("@PageNumber", pageNo);
                    adapter.SelectCommand.Parameters.AddWithValue("@RowsOfPage", totalnoofPage);
                    using (DataSet dataTable = new DataSet())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Tables[0].Rows.Count; i++)
                            {
                                list.Add(new DischargingReport()
                                {
                                    Id = Convert.ToInt32(dataTable.Tables[0].Rows[i]["Id"]),
                                    //VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    //LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    //PortName = dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    //VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"]),
                                    //ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"]),
                                    //SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"]),
                                    //CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"]),
                                    //TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0]),

                                    VoyageNumber = dataTable.Tables[0].Rows[i]["VoyageNumber"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["VoyageNumber"].ToString(),
                                    LegPort_A = dataTable.Tables[0].Rows[i]["LegPort_A"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["LegPort_A"].ToString(),
                                    PortName = dataTable.Tables[0].Rows[i]["PortName"] == DBNull.Value ? "" : dataTable.Tables[0].Rows[i]["PortName"].ToString(),
                                    VesselId = Convert.ToInt32(dataTable.Tables[0].Rows[i]["VesselId"] == DBNull.Value ? 0 : dataTable.Tables[0].Rows[i]["VesselId"]),
                                    ReportDateTime = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["ReportDateTime"] == DBNull.Value ? null : dataTable.Tables[0].Rows[i]["ReportDateTime"]),

                                    SaveDraft = Convert.ToBoolean(dataTable.Tables[0].Rows[i]["SaveDraft"] == DBNull.Value ? true : dataTable.Tables[0].Rows[i]["SaveDraft"]),

                                    CreatedDate = Convert.ToDateTime(dataTable.Tables[0].Rows[i]["CreatedDate"] == DBNull.Value ? DateTime.Now : dataTable.Tables[0].Rows[i]["CreatedDate"]),

                                    TotalCount = Convert.ToInt32(dataTable.Tables[1].Rows[0][0] == DBNull.Value ? 0 : dataTable.Tables[1].Rows[0][0]),




                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public static List<VoyageClass> GetVoyageList(int VesselId)
        {
            try
            {
                List<VoyageClass> list = new List<VoyageClass>();
                using (SqlDataAdapter adapter = new SqlDataAdapter("spVoyageListVesselWise", ConnectionBulder.con))
                {

                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                    adapter.SelectCommand.Parameters.AddWithValue("@Action", "VoyageList");
                    using (DataTable dataTable = new DataTable())
                    {
                        adapter.Fill(dataTable);
                        if (dataTable.Rows.Count > 0)
                        {
                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                list.Add(new VoyageClass()
                                {
                                    Id = Convert.ToInt32(dataTable.Rows[i]["Id"]),
                                    VoyageNumber = dataTable.Rows[i]["VoyageNumber"].ToString(),
                                    //CP_Consumption = dataTable.Rows[i]["CP_Consumption"].ToString(),
                                    //FuelT = dataTable.Rows[i]["FuelT"].ToString(),


                                });
                            }
                        }
                    }

                    return list;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


    }
}

public class ListtoDataTableConverter
{
    public DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);
        try
        {
           
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            
        }
        catch { }
        return dataTable;
    }


}

