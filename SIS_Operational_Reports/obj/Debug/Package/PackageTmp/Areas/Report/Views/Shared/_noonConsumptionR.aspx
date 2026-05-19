<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="_noonConsumptionR.aspx" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>



<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

     <script src="../Scripts/jquery-1.7.1.js"></script>
    <script runat="server">

        void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {

                //List<co> reports = new List<CrewReportLayer.CrewDetailClass>();
                //using (MenuLayer.ShipmentContaxt sc = new MenuLayer.ShipmentContaxt())
                //{
                //    sc.Configuration.ProxyCreationEnabled = false;

                //    String companyname = sc.Companys.Select(x => x.CompanyName).FirstOrDefault();

                //    reports = sc.CrewWorkSchdules(CrewReportLayer.bmsuploaddat.vesselname);

                //    if (reports.Count == 0)
                //    {
                //        ReportViewer1.LocalReport.DataSources.Clear();
                //        ReportViewer1.LocalReport.Refresh();
                //        ReportViewer1.Visible = false;
                //        TempData["amit"] = "Data may not be available for Selected Criteria!  Please Select The Vessel to Check Report.";
                //    }
                //    else
                //    {


                ReportViewer1.LocalReport.DataSources.Clear();
                ReportViewer1.LocalReport.Refresh();
                //ReportViewer1.Visible = false;

                ReportViewer1.Visible = true;
                // string exeDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly("~//RDLCReport//CrewDetailReport.rdlc").Location);

                string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "~//Rdlc//NoonConsumption.rdlc";

                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Rdlc/NoonConsumption.rdlc");
                //ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Rdlc/NoonConsR.rdlc");
                ReportViewer1.LocalReport.DataSources.Clear();

                System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString);


                string vsl = Session["Vsl"].ToString();
                string cpid = Session["NoonCPID"].ToString();

                //System.Data.SqlClient.SqlDataAdapter adp = new System.Data.SqlClient.SqlDataAdapter("select id,VesselStatus,AtSeaOrPort,Date as reportdate from DailyNoonReport where VesselId='"+vsl+"'",con);

                System.Data.SqlClient.SqlDataAdapter adp = new System.Data.SqlClient.SqlDataAdapter("NoonConsumptionReport_New",con);
                System.Data.DataTable dt = new System.Data.DataTable();

                string dtf = "";string dtt = ""; string dtbetween = ""; string consNM = "0";double Total = 0;decimal cofnsNM = 0;
                if(Session["dateF"] != null)
                {
                    dtf = Session["dateF"].ToString();
                    dtt = Session["dateT"].ToString();

                    DateTime FDate = DateTime.Parse(dtf);
                    DateTime TDate = DateTime.Parse(dtt);
                    string dFDate = FDate.ToString("MMMM dd");
                    string dTDate = TDate.ToString("MMMM dd");
                    dtbetween = dFDate + " - " + dTDate;
                }


                adp.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;
                adp.SelectCommand.Parameters.AddWithValue("@dateF",dtf);
                adp.SelectCommand.Parameters.AddWithValue("@dateT",dtt);
                adp.SelectCommand.Parameters.AddWithValue("@vesselid",vsl);
                adp.SelectCommand.Parameters.AddWithValue("@cpid",cpid);
                adp.Fill(dt);

                dt.Columns.Add("ME_Color", typeof(string));
                dt.Columns.Add("AE_Color", typeof(string));
                dt.Columns.Add("ROB_Color", typeof(string));

                dt.Columns.Add("ME_ColorMDO", typeof(string));
                dt.Columns.Add("AE_ColorMDO", typeof(string));
                dt.Columns.Add("ROB_ColorMDO", typeof(string));

                try
                {
                    decimal SOG = 0, me_laden = 0, stmgTime = 0,ROB_VLSFO=0,ROB_MDO=0, cons_ME_vlsfo = 0, total_ME_Cons = 0,
                        total_AE_Cons = 0 , total_ME_ConsMDO = 0,total_AE_ConsMDO = 0;
                    int CPID = 0, noonRptId = 0;string vesselS = "";decimal AE_ConsCal = 0;decimal AE_ConsCalMDO = 0;decimal total_cons = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int voyageid = Convert.ToInt32(dt.Rows[i]["voyageid"]);
                        int legportid = Convert.ToInt32(dt.Rows[i]["legportid"]);
                        int vesselid = Convert.ToInt32(dt.Rows[i]["vesselid"]);
                        string  rptdt = Convert.ToString(dt.Rows[i]["Date"]);
                        stmgTime = Convert.ToDecimal(dt.Rows[i]["StmgTime"]);
                        cons_ME_vlsfo = Convert.ToDecimal(dt.Rows[i]["Cons_ME_VLSFO"] == DBNull.Value ? 0 : dt.Rows[i]["Cons_ME_VLSFO"]);
                        noonRptId = Convert.ToInt32(dt.Rows[i]["id"]);
                        vesselS = Convert.ToString(dt.Rows[i]["VesselStatus"]);
                        ROB_VLSFO = Convert.ToDecimal(dt.Rows[i]["ROB_VLSFO"] == DBNull.Value ? 0 : dt.Rows[i]["ROB_VLSFO"]);
                        ROB_MDO = Convert.ToDecimal(dt.Rows[i]["ROB_MDO"] == DBNull.Value ? 0 : dt.Rows[i]["ROB_MDO"]);

                        if (stmgTime > 0)
                        {

                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select * from VoyageLeg where VesselId=" + vesselid + " and VoyageId = " + voyageid + " and id=" + legportid + " and IsActive=1", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);

                                if (ddt.Rows.Count > 0)
                                {
                                    SOG = Convert.ToDecimal(ddt.Rows[0]["CP_SOG"]);
                                }
                            }
                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("SELECT * FROM CPpart1 WHERE '" + rptdt + "' between startDate and endDate and VesselId=" + vesselid + " and IsActive=1", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);
                                if (ddt.Rows.Count > 0)
                                {
                                    CPID = Convert.ToInt32(ddt.Rows[0]["CPId"]);
                                }
                            }
                            if (vesselS == "L")
                            {
                                using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select * from CPpart2 where Cpid=" + CPID + " and Speed = " + SOG + " and IsActive=1", con))
                                {
                                    System.Data.DataTable ddt = new System.Data.DataTable();
                                    adp1.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
                                    {
                                        me_laden = Convert.ToDecimal(ddt.Rows[0]["ME_Laden"]);
                                        AE_ConsCal  = Convert.ToDecimal(ddt.Rows[0]["AE_Vlsfo"]);

                                        AE_ConsCalMDO = Convert.ToDecimal(ddt.Rows[0]["AE_DO"]);
                                    }
                                }
                            }
                            if (vesselS == "B")
                            {
                                using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select * from CPpart2 where Cpid=" + CPID + " and Speed = " + SOG + " and IsActive=1", con))
                                {
                                    System.Data.DataTable ddt = new System.Data.DataTable();
                                    adp1.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
                                    {
                                        me_laden = Convert.ToDecimal(ddt.Rows[0]["ME_Ballast"]);
                                        AE_ConsCal  = Convert.ToDecimal(ddt.Rows[0]["AE_Vlsfo"]);
                                        AE_ConsCalMDO = Convert.ToDecimal(ddt.Rows[0]["AE_DO"]);
                                    }
                                }
                            }



                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_ME_Cons from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=5 and ConsTypeId in (2,3,4,5) ", con))
                            // Getting only ME At sea Value >> discuss 9 jan 2023
                            //using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_ME_Cons from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=5 and ConsTypeId in (2) ", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);

                                if (ddt.Rows.Count > 0)
                                {
                                    total_ME_Cons = Convert.ToDecimal(ddt.Rows[0]["Total_ME_Cons"]);
                                }
                            }

                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_AE_Cons from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=5 and ConsTypeId in (7,8,9,10) ", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);

                                if (ddt.Rows.Count > 0)
                                {
                                    total_AE_Cons = Convert.ToDecimal(ddt.Rows[0]["Total_AE_Cons"]);
                                }
                            }

                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_ME_ConsMDO from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=2 and ConsTypeId in (2,3,4,5) ", con))
                            // Getting only ME At sea Value >> discuss 9 jan 2023
                            //using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_ME_Cons from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=5 and ConsTypeId in (2) ", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);

                                if (ddt.Rows.Count > 0)
                                {
                                    total_ME_ConsMDO = Convert.ToDecimal(ddt.Rows[0]["Total_ME_ConsMDO"]);
                                }
                            }

                            using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter(" select SUM(value) as Total_AE_ConsMDO from Fuel_Cons_NR where  VesselId=" + vesselid + " and ReportType_Id=1 and Noon_Report_Id=" + noonRptId + " and FuelTypeId=2 and ConsTypeId in (7,8,9,10) ", con))
                            {
                                System.Data.DataTable ddt = new System.Data.DataTable();
                                adp1.Fill(ddt);

                                if (ddt.Rows.Count > 0)
                                {
                                    total_AE_ConsMDO = Convert.ToDecimal(ddt.Rows[0]["Total_AE_ConsMDO"]);
                                }
                            }

                            #region  Old Code


                            //try
                            //{

                            //    DateTime previousdt = Convert.ToDateTime(rptdt).AddDays(-1);
                            //    int prvsNoonId = 0;

                            //    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select id as noonrptId from dailynoonreport where date ='" + previousdt + "' and VesselId=" + vesselid + " and isactive=1 ", con))
                            //    {
                            //        System.Data.DataTable ddt = new System.Data.DataTable();
                            //        adp1.Fill(ddt);

                            //        if (ddt.Rows.Count > 0)
                            //        {
                            //            prvsNoonId = Convert.ToInt32(ddt.Rows[0]["noonrptId"]);
                            //        }
                            //    }
                            //    decimal prevsROB = 0;
                            //    if (prvsNoonId != 0)
                            //    {
                            //        using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select OtherROB from tbl_FuelROB where TableMax_Id = " + prvsNoonId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelType_Id=5 ", con))
                            //        {
                            //            System.Data.DataTable ddt = new System.Data.DataTable();
                            //            adp1.Fill(ddt);

                            //            if (ddt.Rows.Count > 0)
                            //            {
                            //                prevsROB = Convert.ToDecimal(ddt.Rows[0]["OtherROB"]);
                            //            }
                            //        }
                            //    }
                            //    decimal robBalance = 0;
                            //    if (prevsROB != 0)
                            //    {
                            //        robBalance = ROB_VLSFO - prevsROB;
                            //        //robBalance = prevsROB - ROB_VLSFO;

                            //        //robBalance = (ROB_VLSFO - prevsROB)+Departure Report Bunker Received in MT;

                            //        using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select Id from DepartureReport where VesselId="+vesselid+" and ReportDate='"+rptdt+"'", con))
                            //        {
                            //            System.Data.DataTable ddt = new System.Data.DataTable();
                            //            adp1.Fill(ddt);
                            //            if (ddt.Rows.Count > 0)
                            //            {
                            //                int id = Convert.ToInt32(ddt.Rows[0]["id"]);

                            //                System.Data.SqlClient.SqlDataAdapter adp11 = new System.Data.SqlClient.SqlDataAdapter("select * from tbl_BunkerLReceipt where VesselId=" + vesselid + " and ReportType_Id=3 and FuelType_Id=5 and TableMax_Id=" + id + "", con);

                            //                System.Data.DataTable ddt1 = new System.Data.DataTable();
                            //                adp11.Fill(ddt1);
                            //                if (ddt1.Rows.Count > 0)
                            //                {
                            //                    decimal receipt = Convert.ToDecimal(ddt1.Rows[0]["Receipt"]);

                            //                    robBalance = robBalance + receipt;
                            //                }


                            //            }
                            //        }
                            //    }


                            //    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select SUM(value) as totalcons from Fuel_Cons_NR where Noon_Report_Id =" + noonRptId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelTypeId=5 and ConsTypeId in (2,3,4,5,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27)", con))
                            //    {
                            //        System.Data.DataTable ddt = new System.Data.DataTable();
                            //        adp1.Fill(ddt);
                            //        if (ddt.Rows.Count > 0)
                            //        {
                            //            total_cons = Convert.ToDecimal(ddt.Rows[0]["totalcons"]);
                            //        }
                            //    }
                            //    if (robBalance != 0)
                            //    {
                            //        decimal calculatedROB = total_cons / robBalance * 100;
                            //        //calculatedROB +-10% 
                            //        //calculatedROB +-10% Up and down should be highlighted 
                            //        decimal getPercentageROB = calculatedROB * 10 / 100;

                            //        decimal plusValueROB = calculatedROB + getPercentageROB;
                            //        decimal minusValueROB = calculatedROB - getPercentageROB;

                            //        if (ROB_VLSFO > plusValueROB)
                            //        {
                            //            System.Data.DataRow dr = dt.Rows[i];
                            //            dr["ROB_Color"] = "Red";
                            //        }
                            //        if (ROB_VLSFO < minusValueROB)
                            //        {
                            //            System.Data.DataRow dr = dt.Rows[i];
                            //            dr["ROB_Color"] = "Green";
                            //        }

                            //    }
                            //}
                            //catch { }

                            #endregion

                            try
                            {

                                DateTime previousdt = Convert.ToDateTime(rptdt).AddDays(-1);
                                int prvsNoonId = 0;

                                using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select id as noonrptId from dailynoonreport where date ='" + previousdt + "' and VesselId=" + vesselid + " and isactive=1 ", con))
                                {
                                    System.Data.DataTable ddt = new System.Data.DataTable();
                                    adp1.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
                                    {
                                        prvsNoonId = Convert.ToInt32(ddt.Rows[0]["noonrptId"]);
                                    }
                                }
                                decimal prevsROB = 0, prevsROBMDO = 0;
                                if (prvsNoonId != 0)
                                {
                                    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select OtherROB from tbl_FuelROB where TableMax_Id = " + prvsNoonId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelType_Id=5 ", con))
                                    {
                                        System.Data.DataTable ddt = new System.Data.DataTable();
                                        adp1.Fill(ddt);

                                        if (ddt.Rows.Count > 0)
                                        {
                                            prevsROB = Convert.ToDecimal(ddt.Rows[0]["OtherROB"]);
                                        }
                                    }

                                    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select OtherROB from tbl_FuelROB where TableMax_Id = " + prvsNoonId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelType_Id=2 ", con))
                                    {
                                        System.Data.DataTable ddt = new System.Data.DataTable();
                                        adp1.Fill(ddt);

                                        if (ddt.Rows.Count > 0)
                                        {
                                            prevsROBMDO = Convert.ToDecimal(ddt.Rows[0]["OtherROB"]);
                                        }
                                    }
                                }
                                decimal robBalance = 0;
                                if (prevsROB != 0)
                                {
                                    //robBalance = ROB_VLSFO - prevsROB;
                                    //robBalance = prevsROB - ROB_VLSFO;

                                    //prevsROB = 50;
                                    //ROB_VLSFO = 40;



                                    //robBalance = (ROB_VLSFO - prevsROB)+Departure Report Bunker Received in MT;

                                    ////using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select Id from DepartureReport where VesselId="+vesselid+" and ReportDate='"+rptdt+"'", con))
                                    ////{
                                    ////    System.Data.DataTable ddt = new System.Data.DataTable();
                                    ////    adp1.Fill(ddt);
                                    ////    if (ddt.Rows.Count > 0)
                                    ////    {
                                    ////        int id = Convert.ToInt32(ddt.Rows[0]["id"]);

                                    ////        System.Data.SqlClient.SqlDataAdapter adp11 = new System.Data.SqlClient.SqlDataAdapter("select * from tbl_BunkerLReceipt where VesselId=" + vesselid + " and ReportType_Id=3 and FuelType_Id=5 and TableMax_Id=" + id + "", con);

                                    ////        System.Data.DataTable ddt1 = new System.Data.DataTable();
                                    ////        adp11.Fill(ddt1);
                                    ////        if (ddt1.Rows.Count > 0)
                                    ////        {
                                    ////            decimal receipt = Convert.ToDecimal(ddt1.Rows[0]["Receipt"]);

                                    ////            //robBalance = robBalance + receipt;

                                    ////            ROB_VLSFO = ROB_VLSFO + receipt;
                                    ////        }


                                    ////    }
                                    ////}
                                    ///


                                    decimal RobF = prevsROB * 10 / 100;
                                    decimal todaysROB = ROB_VLSFO;
                                    decimal todaysROBMDO = ROB_MDO;
                                    decimal plusValueROB = prevsROB + RobF;
                                    decimal minusValueROB = prevsROB - RobF;
                                    if (todaysROB > plusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_Color"] = "Red";
                                    }
                                    if (todaysROB < minusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_Color"] = "Red";
                                    }

                                    if (todaysROBMDO > plusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_ColorMDO"] = "Red";
                                    }
                                    if (todaysROBMDO < minusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_ColorMDO"] = "Red";
                                    }

                                    //decimal percentcalROB = ((prevsROB - ROB_VLSFO) / prevsROB) * 100;

                                    //if(percentcalROB < 0 && percentcalROB < 10)
                                    //{
                                    //    System.Data.DataRow dr = dt.Rows[i];
                                    //    dr["ROB_Color"] = "Red";
                                    //}
                                }


                                //using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select SUM(value) as totalcons from Fuel_Cons_NR where Noon_Report_Id =" + noonRptId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelTypeId=5 and ConsTypeId in (2,3,4,5,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27)", con))
                                //{
                                //    System.Data.DataTable ddt = new System.Data.DataTable();
                                //    adp1.Fill(ddt);
                                //    if (ddt.Rows.Count > 0)
                                //    {
                                //        total_cons = Convert.ToDecimal(ddt.Rows[0]["totalcons"]);
                                //    }
                                //}
                                //if (robBalance != 0)
                                //{
                                //    //decimal calculatedROB = total_cons / robBalance * 100;

                                //    decimal calculatedROB = (1 - total_cons / robBalance) * 100;


                                //    //calculatedROB +-10% 
                                //    //calculatedROB +-10% Up and down should be highlighted 
                                //    decimal getPercentageROB = calculatedROB * 10 / 100;

                                //    decimal plusValueROB = calculatedROB + getPercentageROB;
                                //    decimal minusValueROB = calculatedROB - getPercentageROB;

                                //    if (ROB_VLSFO > plusValueROB)
                                //    {
                                //        System.Data.DataRow dr = dt.Rows[i];
                                //        dr["ROB_Color"] = "Red";
                                //    }
                                //    if (ROB_VLSFO < minusValueROB)
                                //    {
                                //        System.Data.DataRow dr = dt.Rows[i];
                                //        dr["ROB_Color"] = "Green";
                                //    }

                                //}
                            }
                            catch { }

                            // Calculation on the basis of 10 percent plus minus >> 28 Dec 2022
                            decimal Stml_Calculated = me_laden * stmgTime / 24;
                            //decimal checkv = (total_ME_Cons - Stml_Calculated) / Stml_Calculated;
                            //checkv = checkv * 100;
                            decimal getPercentageV = Stml_Calculated * 10 / 100;
                            decimal totalMECons = total_ME_Cons;
                            decimal totalMEConsMDO = total_ME_ConsMDO;
                            decimal plusValue = Stml_Calculated + getPercentageV;
                            decimal minusValue = Stml_Calculated - getPercentageV;
                            //decimal plusValue = total_ME_Cons + getPercentageV;
                            //decimal minusValue = total_ME_Cons - getPercentageV;
                            //Stml_Calculated = 11;
                            if (totalMECons > plusValue)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["ME_Color"] = "Red";
                            }
                            if (totalMECons < minusValue)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["ME_Color"] = "Red";
                            }

                            if (totalMEConsMDO > plusValue)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["ME_ColorMDO"] = "Red";
                            }
                            if (totalMEConsMDO < minusValue)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["ME_ColorMDO"] = "Red";
                            }

                            //if (checkv > 10)
                            //{
                            //    System.Data.DataRow dr = dt.Rows[i];
                            //    dr["ME_Color"] = "Green";
                            //}
                            //if (checkv < 10)
                            //{
                            //    System.Data.DataRow dr = dt.Rows[i];
                            //    dr["ME_Color"] = "Red";
                            //}



                            decimal Stml_Calculated1 = AE_ConsCal * stmgTime / 24;
                            decimal getPercentageV1 = Stml_Calculated1 * 10 / 100;
                            decimal totalAECons = total_AE_Cons;
                            // decimal totalAEConsMDO = total_AE_ConsMDO;
                            decimal plusValueA = Stml_Calculated1 + getPercentageV1;
                            decimal minusValueA = Stml_Calculated1 - getPercentageV1;
                            //decimal plusValue = total_ME_Cons + getPercentageV;
                            //decimal minusValue = total_ME_Cons - getPercentageV;
                            //Stml_Calculated = 11;
                            if (totalAECons > plusValueA)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["AE_Color"] = "Red";
                            }
                            if (totalAECons < minusValueA)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["AE_Color"] = "Red";
                            }

                            decimal Stml_Calculated2 = AE_ConsCalMDO * stmgTime / 24;
                            decimal getPercentageV2 = Stml_Calculated2 * 10 / 100;
                            //decimal totalAECons = total_AE_Cons;
                            decimal totalAEConsMDO = total_AE_ConsMDO;
                            decimal plusValueAMDO = Stml_Calculated2 + getPercentageV2;
                            decimal minusValueAMDO = Stml_Calculated2 - getPercentageV2;

                            if (totalAEConsMDO > plusValueAMDO)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["AE_ColorMDO"] = "Red";
                            }
                            if (totalAEConsMDO < minusValueAMDO)
                            {
                                System.Data.DataRow dr = dt.Rows[i];
                                dr["AE_ColorMDO"] = "Red";
                            }


                            //decimal Stml_Calculated1 = AE_ConsCal * stmgTime / 24;
                            //decimal getPercentageV1 = total_AE_Cons * 10 / 100;
                            //decimal plusValue1 = total_AE_Cons + getPercentageV1;
                            //decimal minusValue1 = total_AE_Cons - getPercentageV1;
                            ////Stml_Calculated = 11;
                            //if (Stml_Calculated1 > plusValue1)
                            //{
                            //    System.Data.DataRow dr = dt.Rows[i];
                            //    dr["AE_Color"] = "Red";
                            //}
                            //if (Stml_Calculated1 < minusValue1)
                            //{
                            //    System.Data.DataRow dr = dt.Rows[i];
                            //    dr["AE_Color"] = "Green";
                            //}

                        }
                        else
                        {
                            try
                            {

                                DateTime previousdt = Convert.ToDateTime(rptdt).AddDays(-1);
                                int prvsNoonId = 0;

                                using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select id as noonrptId from dailynoonreport where date ='" + previousdt + "' and VesselId=" + vesselid + " and isactive=1 ", con))
                                {
                                    System.Data.DataTable ddt = new System.Data.DataTable();
                                    adp1.Fill(ddt);

                                    if (ddt.Rows.Count > 0)
                                    {
                                        prvsNoonId = Convert.ToInt32(ddt.Rows[0]["noonrptId"]);
                                    }
                                }
                                decimal prevsROB = 0, prevsROBMDO = 0;
                                if (prvsNoonId != 0)
                                {
                                    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select OtherROB from tbl_FuelROB where TableMax_Id = " + prvsNoonId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelType_Id=5 ", con))
                                    {
                                        System.Data.DataTable ddt = new System.Data.DataTable();
                                        adp1.Fill(ddt);

                                        if (ddt.Rows.Count > 0)
                                        {
                                            prevsROB = Convert.ToDecimal(ddt.Rows[0]["OtherROB"]);
                                        }
                                    }

                                    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select OtherROB from tbl_FuelROB where TableMax_Id = " + prvsNoonId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelType_Id=2 ", con))
                                    {
                                        System.Data.DataTable ddt = new System.Data.DataTable();
                                        adp1.Fill(ddt);

                                        if (ddt.Rows.Count > 0)
                                        {
                                            prevsROBMDO = Convert.ToDecimal(ddt.Rows[0]["OtherROB"]);
                                        }
                                    }
                                }
                                decimal robBalance = 0;
                                if (prevsROB != 0)
                                {
                                    //robBalance = ROB_VLSFO - prevsROB;
                                    //robBalance = prevsROB - ROB_VLSFO;

                                    //prevsROB = 50;
                                    //ROB_VLSFO = 40;



                                    //robBalance = (ROB_VLSFO - prevsROB)+Departure Report Bunker Received in MT;

                                    //using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select Id from DepartureReport where VesselId="+vesselid+" and ReportDate='"+rptdt+"'", con))
                                    //{
                                    //    System.Data.DataTable ddt = new System.Data.DataTable();
                                    //    adp1.Fill(ddt);
                                    //    if (ddt.Rows.Count > 0)
                                    //    {
                                    //        int id = Convert.ToInt32(ddt.Rows[0]["id"]);

                                    //        System.Data.SqlClient.SqlDataAdapter adp11 = new System.Data.SqlClient.SqlDataAdapter("select * from tbl_BunkerLReceipt where VesselId=" + vesselid + " and ReportType_Id=3 and FuelType_Id=5 and TableMax_Id=" + id + "", con);

                                    //        System.Data.DataTable ddt1 = new System.Data.DataTable();
                                    //        adp11.Fill(ddt1);
                                    //        if (ddt1.Rows.Count > 0)
                                    //        {
                                    //            decimal receipt = Convert.ToDecimal(ddt1.Rows[0]["Receipt"]);

                                    //            //robBalance = robBalance + receipt;

                                    //            ROB_VLSFO = ROB_VLSFO + receipt;
                                    //        }


                                    //    }
                                    //}

                                    decimal RobF = prevsROB * 10 / 100;
                                    decimal todaysROB = ROB_VLSFO;
                                    decimal todaysROBMDO = ROB_MDO;
                                    decimal plusValueROB = prevsROB + RobF;
                                    decimal minusValueROB = prevsROB - RobF;
                                    if (todaysROB > plusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_Color"] = "Red";
                                    }
                                    if (todaysROB < minusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_Color"] = "Red";
                                    }

                                    if (todaysROBMDO > plusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_ColorMDO"] = "Red";
                                    }
                                    if (todaysROBMDO < minusValueROB)
                                    {
                                        System.Data.DataRow dr = dt.Rows[i];
                                        dr["ROB_ColorMDO"] = "Red";
                                    }
                                    //decimal percentcalROB = ((prevsROB - ROB_VLSFO) / prevsROB) * 100;
                                    //if(percentcalROB < 0 && percentcalROB < 10)
                                    //{
                                    //    System.Data.DataRow dr = dt.Rows[i];
                                    //    dr["ROB_Color"] = "Red";
                                    //}
                                }


                                //using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select SUM(value) as totalcons from Fuel_Cons_NR where Noon_Report_Id =" + noonRptId + " and VesselId=" + vesselid + " and ReportType_Id=1 and FuelTypeId=5 and ConsTypeId in (2,3,4,5,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27)", con))
                                //{
                                //    System.Data.DataTable ddt = new System.Data.DataTable();
                                //    adp1.Fill(ddt);
                                //    if (ddt.Rows.Count > 0)
                                //    {
                                //        total_cons = Convert.ToDecimal(ddt.Rows[0]["totalcons"]);
                                //    }
                                //}
                                //if (robBalance != 0)
                                //{
                                //    //decimal calculatedROB = total_cons / robBalance * 100;

                                //    decimal calculatedROB = (1 - total_cons / robBalance) * 100;


                                //    //calculatedROB +-10% 
                                //    //calculatedROB +-10% Up and down should be highlighted 
                                //    decimal getPercentageROB = calculatedROB * 10 / 100;

                                //    decimal plusValueROB = calculatedROB + getPercentageROB;
                                //    decimal minusValueROB = calculatedROB - getPercentageROB;

                                //    if (ROB_VLSFO > plusValueROB)
                                //    {
                                //        System.Data.DataRow dr = dt.Rows[i];
                                //        dr["ROB_Color"] = "Red";
                                //    }
                                //    if (ROB_VLSFO < minusValueROB)
                                //    {
                                //        System.Data.DataRow dr = dt.Rows[i];
                                //        dr["ROB_Color"] = "Green";
                                //    }

                                //}
                            }
                            catch { }
                        }
                    }
                }
                catch { }



                Session["dateF"] = null;
                Session["dateT"] = null;
                Session["Vsl"] = null;
                Session["NoonCPID"] = null;

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    for (int j = 0; j < dt.Columns.Count; j++)
                //    {
                //        if (string.IsNullOrEmpty(dt.Rows[i][j].ToString()))
                //        {

                //            dt.Rows[i][j] = "0";
                //        }
                //    }
                //}



                if (dt.Rows.Count > 0)
                {

                    string speed = "", me_laden = "", me_ballast = "", ae_vlsfo = "", ae_do = "",
                        idling="",loading="",discharging="",manovering="" ;
                    using (System.Data.SqlClient.SqlDataAdapter adp1 = new System.Data.SqlClient.SqlDataAdapter("select speed,me_laden, me_ballast, ae_vlsfo, ae_do from CPpart2  where CPId in (select  CPId from CPpart1 where VesselID=" + vsl + " and cpid=" + cpid + ")", con))
                    {
                        System.Data.DataTable ddt = new System.Data.DataTable();
                        adp1.Fill(ddt);
                        // if (ddt.Rows.Count > 0)
                        for (int i = 0; i < ddt.Rows.Count; i++)
                        {

                            speed += Convert.ToDecimal( ddt.Rows[i]["Speed"]).ToString("#.00") +",";
                            me_laden += Convert.ToDecimal(ddt.Rows[i]["me_laden"]).ToString("#.00") +",";
                            me_ballast += Convert.ToDecimal(ddt.Rows[i]["me_ballast"]).ToString("#.00") +",";
                            ae_vlsfo += Convert.ToDecimal(ddt.Rows[i]["ae_vlsfo"]).ToString("#.00") +",";
                            ae_do += Convert.ToDecimal(ddt.Rows[i]["ae_do"]).ToString("#.00") +",";
                        }

                        for (int i = ddt.Rows.Count; i <= 7; i++)
                        {
                            speed += "-" + ",";
                            me_laden += "-" + ",";
                            me_ballast += "-" + ",";
                            ae_vlsfo += "-" + ",";
                            ae_do += "-" + ",";
                        }

                        //speed = Convert.ToDecimal(speed).ToString("#.00");

                        System.Data.SqlClient.SqlDataAdapter adp11 = new System.Data.SqlClient.SqlDataAdapter("spForNoonCons", con);
                        System.Data.DataSet ddt1 = new System.Data.DataSet();


                        adp11.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;

                        adp11.SelectCommand.Parameters.AddWithValue("@vesselid",vsl);
                        adp11.SelectCommand.Parameters.AddWithValue("@cpid",cpid);

                        adp11.Fill(ddt1);
                        for (int i = 0; i < ddt1.Tables[0].Rows.Count; i++)
                        {
                            idling += Convert.ToDecimal(ddt1.Tables[0].Rows[i][0]).ToString("#.00") +",";
                        }
                        for (int i = 0; i < ddt1.Tables[1].Rows.Count; i++)
                        {
                            loading += Convert.ToDecimal(ddt1.Tables[1].Rows[i][0]).ToString("#.00") +",";
                        }
                        for (int i = 0; i < ddt1.Tables[2].Rows.Count; i++)
                        {
                            discharging += Convert.ToDecimal(ddt1.Tables[2].Rows[i][0]).ToString("#.00") +",";
                        }
                        for (int i = 0; i < ddt1.Tables[3].Rows.Count; i++)
                        {
                            manovering += Convert.ToDecimal(ddt1.Tables[3].Rows[i][0]).ToString("#.00") +",";
                        }
                    }


                    string vesselname = dt.Rows[0]["VesselName"].ToString();
                    string voyno = dt.Rows[0]["voyagenumber"].ToString();
                    //List<string> ids = new List<string>(dt.Rows.Count);
                    //foreach (System.Data.DataRow row in dt.Rows)
                    //{
                    //    ids.Add((string)row["voyagenumber"]);
                    //}

                    //string sdfsf = ids.ToString();

                    List<String> stringArr = new List<String>();

                    // Classic version :-)
                    for (int a = 0; a < dt.Rows.Count; a++)
                    {
                        stringArr.Add(dt.Rows[a]["voyagenumber"].ToString());
                    }

                    List<string> myList = stringArr.Distinct().ToList();

                    // List<string> column1List = (dt. AsEnumerable().Select(x => x["Column1"].ToString()).ToList()).Distinct();

                    string concat = string.Join(",", myList);

                    decimal n2n = 0;decimal consVlsfo = 0; decimal n2n1 = 0; int ttlcnt = 0;
                    try
                    {

                        for (int b = 0; b < dt.Rows.Count; b++)
                        {
                            try
                            {
                                //n2n += Convert.ToDecimal(dt.Rows[b]["noontonoondmg_dist"]);
                                //consVlsfo += Convert.ToDecimal(dt.Rows[b]["Cons_VLSFO"]== DBNull.Value ? 0:dt.Rows[b]["Cons_VLSFO"]) ;

                                n2n = Convert.ToDecimal(dt.Rows[b]["noontonoondmg_dist"]);
                                consVlsfo = Convert.ToDecimal(dt.Rows[b]["Cons_VLSFO"]== DBNull.Value ? 0:dt.Rows[b]["Cons_VLSFO"]) ;

                                if(consVlsfo ==0)
                                {
                                    n2n1 = Convert.ToDecimal(dt.Rows[b]["noontonoondmg_dist"]);
                                    n2n = n2n - n2n1;
                                }
                                else
                                {
                                    ttlcnt++;
                                }

                                cofnsNM += Convert.ToDecimal( n2n / consVlsfo);

                            }
                            catch { }
                        }


                        int cnt = ttlcnt;
                        //cofnsNM = Convert.ToDecimal( n2n / consVlsfo);

                        cofnsNM = cofnsNM / cnt;

                        consNM = cofnsNM.ToString();

                        double ss = Convert.ToDouble( consNM);

                        Total = Convert.ToDouble(String.Format("{00:0.000}", ss));

                        //Total = String.Format("{0:0.00}", ss);
                    }
                    catch { }

                    if(dtbetween =="")
                    {
                        dtbetween = "N/A";
                    }

                    ReportDataSource rdc = new ReportDataSource("NoonConsD", dt);
                    //ReportDataSource rdc = new ReportDataSource("DataSet1", dt);

                    ReportParameter vname = new ReportParameter("VesselName", vesselname);
                    ReportParameter vno = new ReportParameter("VoyageNumber", concat);
                    ReportParameter date = new ReportParameter("Date", dtbetween);
                    ReportParameter cnsnm = new ReportParameter("consNM",Convert.ToString( Total));

                    ReportParameter spd = new ReportParameter("Speed", speed.TrimEnd(','));
                    ReportParameter me_lad = new ReportParameter("ME_Laden", me_laden.TrimEnd(','));
                    ReportParameter me_bal = new ReportParameter("ME_Ballast", me_ballast.TrimEnd(','));
                    ReportParameter ae_vls = new ReportParameter("AE_Vlsfo", ae_vlsfo.TrimEnd(','));
                    ReportParameter ae_d = new ReportParameter("AE_DO", ae_do.TrimEnd(','));

                    ReportParameter idlng = new ReportParameter("IDLING", idling.TrimEnd(','));
                    ReportParameter ldng = new ReportParameter("LOADING", loading.TrimEnd(','));
                    ReportParameter disch = new ReportParameter("DISCHARGING", discharging.TrimEnd(','));
                    ReportParameter manov = new ReportParameter("MANOURING", manovering.TrimEnd(','));

                    ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { vname, vno, date,cnsnm,spd,me_lad,me_bal,ae_vls,ae_d,idlng,ldng,disch,manov });

                    object repotname = "Consumption_Pattern_based_Noon_Report " + DateTime.Now.Date.ToString("MMM yy");

                    ReportViewer1.LocalReport.DisplayName = repotname.ToString();
                    // ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                    ReportViewer1.LocalReport.Refresh();



                    ReportViewer1.ShowPageNavigationControls = false;
                    ReportViewer1.AsyncRendering = false;
                    ReportViewer1.KeepSessionAlive = false;
                }
                else
                {
                    ReportDataSource rdc = new ReportDataSource("NoonConsD", dt);

                    if (dt.Rows.Count > 0)
                    {
                        ReportParameter vname = new ReportParameter("NoRecord", "No Record Found !");

                        ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { vname });
                    }

                    ReportViewer1.LocalReport.DataSources.Clear();
                    ReportViewer1.LocalReport.DataSources.Add(rdc);
                    ReportViewer1.LocalReport.Refresh();
                }

                //}


                // }
            }
        }



    </script>
</head>
<body>
    <form id="form1" runat="server">
          <div>
            <asp:ScriptManager ID="ScriptManager1"  runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" AsyncRendering="false" SizeToReportContent="false" Height="1000px" Width="1500px" BorderColor="#CCCCCC" BorderStyle="Solid" ShowBackButton="False" ShowFindControls="False" ShowZoomControl="False" PageCountMode="Actual"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
