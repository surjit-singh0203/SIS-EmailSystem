using DataBuildingLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace SIS_Operational_Reports.Common
{
    /// <summary>
    /// Builds HTML email body for Bulk Noon Report. Uses BulkNoonReport.html template when available.
    /// Uses DailyNoonReport data model, structured with Navigation / Engine / Cargo sections.
    /// </summary>
    public static class BulkNoonReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/BulkNoonReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <param name="reportId">When set (from export filename R{id}), matches via editnoonRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            var vd = new DailyNoonReport();
            DailyNoonReport noonRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                vd.GetNoonRList = CommonMethods.editnoonRList(reportId.Value, vesselId, "DailyNoonReport");
                noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (noonRBind == null)
            {
                vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselId, "DailyNoonReport");
                noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (noonRBind == null) return null;

            int id = noonRBind.Id;
            DataTable dtFuelCons = new DataTable(), dtFuelROB = new DataTable(), dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable(), dtMain = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                if (dtBunker.Rows.Count == 0)
                {
                    DataTable dtFuelTypes = new DataTable();
                    using (var adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                        adp.Fill(dtFuelTypes);
                    using (var adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                    {
                        DataTable dtBunk = new DataTable();
                        adp.Fill(dtBunk);
                        foreach (DataRow r in dtBunk.Rows)
                        {
                            int ftId = Convert.ToInt32(r["FuelType_Id"]);
                            var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                            string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                            dtBunker.Rows.Add(fuelType, r["Receipt"]?.ToString());
                        }
                    }
                }
                using (var adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                    adp.Fill(dtNonRoutine);
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", noonRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dtMain);
                }
            }
            catch { }

            string vesselName = CommonClass.GetVesselNamesByImoNo(vesselId.ToString());
            if (string.IsNullOrEmpty(vesselName)) vesselName = "Vessel " + vesselId;

            string templatePath = null;
            try { templatePath = HostingEnvironment.MapPath(TemplatePath); } catch { }
            if (string.IsNullOrEmpty(templatePath))
            {
                try { templatePath = HttpContext.Current?.Server?.MapPath(TemplatePath); } catch { }
            }
            if (!string.IsNullOrEmpty(templatePath) && File.Exists(templatePath))
            {
                string template = File.ReadAllText(templatePath);
                return ApplyTemplate(template, noonRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker);
            }
            return BuildHtmlInline(noonRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker);
        }

        private static string DatePartToReportDate(string datePart)
        {
            if (string.IsNullOrEmpty(datePart)) return null;
            var p = datePart.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length >= 3 && int.TryParse(p[0], out int d) && int.TryParse(p[1], out int m) && int.TryParse(p[2], out int y))
            {
                try
                {
                    var dt = new DateTime(y, m, d);
                    return dt.ToString("yyyy-MM-dd");
                }
                catch { }
            }
            return null;
        }

        private static string ApplyTemplate(string template, DailyNoonReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = r.LegPortName ?? "";
            string portStatusText = r.PortStatus?.ToString() ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
            }

            // --- Navigation ---
            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Status", r.VesselStatus)).Append(KvRow("Latitude", r.Latitude)).Append(KvRow("Longitude", r.Longitude));
            sb.Append(KvRow("At Sea/In Port", r.AtSeaOrPort)).Append(KvRow("In Port Status", portStatusText)).Append(KvRow("Displacement(MT)", r.Displacement)).Append(KvRow("CP Speed(Kts)", r.CP_Speed));
            sb.Append(KvRow("Leg", legText)).Append(KvRow("Report Date", V(r.Date))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            // --- Speed - Distance - Time ---
            sb.Append(KvRow("Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime));
            sb.Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime)).Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            string speedDistRows = sb.ToString();
            sb.Clear();

            // --- Non-Routine Events ---
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < nreLabels.Length; i++)
            {
                string acc = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "-") : "-";
                string hrs = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["Hours"]?.ToString() ?? "-") : "-";
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(nreLabels[i]).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(acc).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(hrs).Append(@"</td></tr>");
            }
            string nonRoutineRows = sb.ToString();
            sb.Clear();

            // --- Weather ---
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight));
            sb.Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            string weatherRows = sb.ToString();
            sb.Clear();

            // --- Remarks ---
            string remarksRow = @"<tr><td colspan=""8"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + (r.Remarks ?? "-") + @"</td></tr>";

            // --- Engine ---
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR)).Append(KvRow("M/E Control Location", r.ME_ControlLoc));
            sb.Append(KvRow("SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress)).Append(KvRow("SCAV. Temp (Deg Centigrade)", r.SCAV_Temp));
            sb.Append(KvRow("Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp)).Append(KvRow("Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp)).Append(KvRow("SW Temp (Deg Centigrade)", r.SW_Temp)).Append(KvRow("ER Temp (Deg Centigrade)", r.ER_Temp));
            string engineRows = sb.ToString();
            sb.Clear();

            // --- Lube Oil & Hydraulic Oil ---
            sb.Append(KvRow("ME Crosshead Cons", r.LO_HO_Cons_MECC)).Append(KvRow("ME Cylinder Cons", r.LO_HO_Cons_MECYL));
            sb.Append(KvRow("AE Crosshead Cons", r.LO_HO_Cons_AECC)).Append(KvRow("Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil));
            sb.Append(KvRow("ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB)).Append(KvRow("ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB)).Append(KvRow("Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB));
            string lubeOilRows = sb.ToString();
            sb.Clear();

            // --- Fuel ROB in MT (OtherROB) ---
            if (dtFuelROB != null) foreach (DataRow dr in dtFuelROB.Rows)
                sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"] : null));
            string fuelRobRows = sb.ToString();
            sb.Clear();

            // --- Bunker Received ---
            if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows)
                sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]));
            string bunkerRows = sb.ToString();
            sb.Clear();

            // --- Aux. Engine ---
            sb.Append(KvRow("Running Hrs No.1", r.AE_RungHrs_No1)).Append(KvRow("Running Hrs No.2", r.AE_RungHrs_No2)).Append(KvRow("Running Hrs No.3", r.AE_RungHrs_No3)).Append(KvRow("Running Hrs No.4", r.AE_RungHrs_No4)).Append(KvRow("Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen));
            sb.Append(KvRow("Load No.1 (KW)", r.AE_Load_No1)).Append(KvRow("Load No.2 (KW)", r.AE_Load_No2)).Append(KvRow("Load No.3 (KW)", r.AE_Load_No3)).Append(KvRow("Load No.4 (KW)", r.AE_Load_No4)).Append(KvRow("Load Shaft Gen (KW)", r.AE_Load_ShaftGen)).Append(KvRow("Extra Run Reason", r.AE_Extra_Run_Reason));
            string auxEngineRows = sb.ToString();
            sb.Clear();

            // --- Boiler's ---
            sb.Append(KvRow("Boiler No.1 Running Hrs", r.BR_RungHrs_No1)).Append(KvRow("Boiler No.2 Running Hrs", r.BR_RungHrs_No2)).Append(KvRow("Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1)).Append(KvRow("Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2));
            string boilerRows = sb.ToString();
            sb.Clear();

            // --- Fuel Consumption in MT ---
            string[] fuelTypes = { "VLSFO", "MDO" };
            foreach (string ft in fuelTypes)
            {
                string main = GetFuelConsByConsType(dtFuelCons, ft, 2);
                string aux = GetFuelConsByConsType(dtFuelCons, ft, 3);
                string boiler = GetFuelConsByConsType(dtFuelCons, ft, 4);
                string total = GetFuelConsByConsType(dtFuelCons, ft, 5);
                sb.Append(KvRow(ft + " (Main/Aux/Boiler/Total)", main + " / " + aux + " / " + boiler + " / " + total));
            }
            string fuelConsRows = sb.ToString();
            sb.Clear();

            // --- Ballast ---
            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            string ballastRow = sb.ToString();
            sb.Clear();

            // --- Fresh Water ---
            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            string freshWaterRows = sb.ToString();

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", V(r.Date))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{SPEED_DIST_ROWS}}", speedDistRows)
                .Replace("{{NON_ROUTINE_ROWS}}", nonRoutineRows)
                .Replace("{{WEATHER_ROWS}}", weatherRows)
                .Replace("{{REMARKS_ROW}}", remarksRow)
                .Replace("{{ENGINE_ROWS}}", engineRows)
                .Replace("{{LUBE_OIL_ROWS}}", lubeOilRows)
                .Replace("{{FUEL_ROB_ROWS}}", fuelRobRows)
                .Replace("{{BUNKER_ROWS}}", bunkerRows)
                .Replace("{{OT_ROB_OXY_Full}}", V(r.OT_ROB_OXY_Full))
                .Replace("{{OT_ROB_OXY_InUse}}", V(r.OT_ROB_OXY_InUse))
                .Replace("{{OT_ROB_OXY_Empty}}", V(r.OT_ROB_OXY_Empty))
                .Replace("{{OT_ROB_ACYT_Full}}", V(r.OT_ROB_ACYT_Full))
                .Replace("{{OT_ROB_ACYT_InUse}}", V(r.OT_ROB_ACYT_InUse))
                .Replace("{{OT_ROB_ACYT_Empty}}", V(r.OT_ROB_ACYT_Empty))
                .Replace("{{AUX_ENGINE_ROWS}}", auxEngineRows)
                .Replace("{{BOILER_ROWS}}", boilerRows)
                .Replace("{{FUEL_CONS_ROWS}}", fuelConsRows)
                .Replace("{{SLOPS_ROB_Oil}}", V(r.SLOPS_ROB_OXY_Oil))
                .Replace("{{SLOPS_ROB_Water}}", V(r.SLOPS_ROB_OXY_Water))
                .Replace("{{SLOPS_ROB_Total}}", V(r.SLOPS_ROB_OXY_Total))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(DailyNoonReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = r.LegPortName ?? "";
            string portStatusText = r.PortStatus?.ToString() ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
            }

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Bulk Noon Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.Date)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");

            // Navigation header
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Bulk Noon Report - Navigation (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Status", r.VesselStatus)).Append(KvRow("Latitude", r.Latitude)).Append(KvRow("Longitude", r.Longitude));
            sb.Append(KvRow("At Sea/In Port", r.AtSeaOrPort)).Append(KvRow("In Port Status", portStatusText)).Append(KvRow("Displacement(MT)", r.Displacement)).Append(KvRow("CP Speed(Kts)", r.CP_Speed));
            sb.Append(KvRow("Leg", legText)).Append(KvRow("Report Date", V(r.Date))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");

            // Speed - Distance - Time
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Speed - Distance - Time</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime));
            sb.Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime)).Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            sb.Append(@"</table></td></tr>");

            // Non-Routine Events
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Non-Routine Events</td></tr>");
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:38%;min-width:200px""><col style=""width:45%;min-width:220px""><col style=""width:17%;min-width:80px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Event Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Owners/Charterers Account</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Hrs.</td></tr>");
            for (int i = 0; i < nreLabels.Length; i++)
            {
                string acc = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "-") : "-";
                string hrs = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["Hours"]?.ToString() ?? "-") : "-";
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(nreLabels[i]).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(acc).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(hrs).Append(@"</td></tr>");
            }
            sb.Append(@"</table></td></tr>");

            // Weather
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Weather</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight));
            sb.Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bulk Noon Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");

            // Engine
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR)).Append(KvRow("M/E Control Location", r.ME_ControlLoc));
            sb.Append(KvRow("SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress)).Append(KvRow("SCAV. Temp (Deg Centigrade)", r.SCAV_Temp));
            sb.Append(KvRow("Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp)).Append(KvRow("Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp)).Append(KvRow("SW Temp (Deg Centigrade)", r.SW_Temp)).Append(KvRow("ER Temp (Deg Centigrade)", r.ER_Temp));
            sb.Append(@"</table></td></tr>");

            // Lube Oil & Hydraulic Oil
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Lube Oil & Hydraulic Oil</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ME Crosshead Cons", r.LO_HO_Cons_MECC)).Append(KvRow("ME Cylinder Cons", r.LO_HO_Cons_MECYL));
            sb.Append(KvRow("AE Crosshead Cons", r.LO_HO_Cons_AECC)).Append(KvRow("Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil));
            sb.Append(KvRow("ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB)).Append(KvRow("ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB)).Append(KvRow("Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB));
            sb.Append(@"</table></td></tr>");

            // Fuel ROB in MT
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtFuelROB != null) foreach (DataRow dr in dtFuelROB.Rows)
                sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"] : null));
            sb.Append(@"</table></td></tr>");

            // Bunker Received
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows) sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
            sb.Append(@"</table></td></tr>");

            // Other ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Aux. Engine
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Aux. Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Running Hrs No.1", r.AE_RungHrs_No1)).Append(KvRow("Running Hrs No.2", r.AE_RungHrs_No2)).Append(KvRow("Running Hrs No.3", r.AE_RungHrs_No3)).Append(KvRow("Running Hrs No.4", r.AE_RungHrs_No4)).Append(KvRow("Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen));
            sb.Append(KvRow("Load No.1 (KW)", r.AE_Load_No1)).Append(KvRow("Load No.2 (KW)", r.AE_Load_No2)).Append(KvRow("Load No.3 (KW)", r.AE_Load_No3)).Append(KvRow("Load No.4 (KW)", r.AE_Load_No4)).Append(KvRow("Load Shaft Gen (KW)", r.AE_Load_ShaftGen)).Append(KvRow("Extra Run Reason", r.AE_Extra_Run_Reason));
            sb.Append(@"</table></td></tr>");

            // Boiler's
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Boiler's</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Boiler No.1 Running Hrs", r.BR_RungHrs_No1)).Append(KvRow("Boiler No.2 Running Hrs", r.BR_RungHrs_No2)).Append(KvRow("Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1)).Append(KvRow("Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2));
            sb.Append(@"</table></td></tr>");

            // Fuel Consumption in MT
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Consumption in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            string[] fuelTypes = { "VLSFO", "MDO" };
            foreach (string ft in fuelTypes)
            {
                string main = GetFuelConsByConsType(dtFuelCons, ft, 2);
                string aux = GetFuelConsByConsType(dtFuelCons, ft, 3);
                string boiler = GetFuelConsByConsType(dtFuelCons, ft, 4);
                string total = GetFuelConsByConsType(dtFuelCons, ft, 5);
                sb.Append(KvRow(ft + " (Main/Aux/Boiler/Total)", main + " / " + aux + " / " + boiler + " / " + total));
            }
            sb.Append(@"</table></td></tr>");

            // Slops ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Total)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Ballast
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            sb.Append(@"</table></td></tr>");

            // Fresh Water
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fresh Water</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            sb.Append(@"</table></td></tr>");

            sb.Append(@"</table>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>");
            sb.Append(@"<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Thank you,</p><p style=""margin:4px 0 0 0;"">Team SIS</p></body></html>");
            return sb.ToString();
        }

        /// <summary>Single label-value row for 2-column layout. Label 45% min 280px, value 55%. Same as Berthing Report.</summary>
        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }

        /// <summary>Gets fuel consumption value for a specific fuel type and consumption type (2=Main, 3=Aux, 4=Boiler, 5=Total).</summary>
        private static string GetFuelConsByConsType(DataTable dt, string fuelType, int consTypeId)
        {
            if (dt == null) return "0.000";
            foreach (DataRow dr in dt.Rows)
            {
                if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase)
                    && Convert.ToInt32(dr["ConsTypeId"]) == consTypeId)
                {
                    var v = dr["Value"];
                    if (v != null && v != DBNull.Value)
                    {
                        decimal d;
                        return decimal.TryParse(v.ToString(), out d) ? d.ToString("0.000") : v.ToString();
                    }
                }
            }
            return "0.000";
        }
    }
}
