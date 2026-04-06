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
    /// Builds HTML email body for Berthing Report. Uses BerthingReport.html template when available.
    /// Same design as Departure Report, Arrival Report and Daily Noon Report.
    /// </summary>
    public static class BerthingReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/BerthingReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <param name="reportId">When set (from export filename R{id}), matches Excel save fallback via editberthingRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            var vd = new BerthingReport();
            BerthingReport berthRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                vd.GetBerthRList = CommonMethods.editberthingRList(reportId.Value, vesselId, "BerthingReport");
                berthRBind = vd.GetBerthRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (berthRBind == null)
            {
                vd.GetBerthRList = CommonMethods.editberthingRListDashboard(reportdate, vesselId, "BerthingReport");
                berthRBind = vd.GetBerthRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (berthRBind == null)
            {
                vd.GetBerthRList = CommonMethods.editberthingRListDashboard(reportdate, vesselId, "BerthingReportR");
                berthRBind = vd.GetBerthRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (berthRBind == null) return null;

            int id = berthRBind.Id;
            DataTable dtFuelCons = new DataTable(), dtFuelROB = new DataTable(), dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable(), dtMain = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=4 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.EOSP as SBE, a.FWE as RFA from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=4", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=4", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                if (dtBunker.Rows.Count == 0)
                {
                    DataTable dtFuelTypes = new DataTable();
                    using (var adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                        adp.Fill(dtFuelTypes);
                    using (var adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=4 order by FuelType_Id", ConnectionBulder.con))
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
                using (var adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=4 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                    adp.Fill(dtNonRoutine);
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", berthRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "BerthingReport");
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
                return ApplyTemplate(template, berthRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker);
            }
            return BuildHtmlInline(berthRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker);
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

        private static string ApplyTemplate(string template, BerthingReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = "";
            string portStatusText = r.PortStatus.ToString();
            string facilityName = r.FacilityName ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                if (string.IsNullOrEmpty(facilityName) && dtMain.Columns.Contains("FacilityName"))
                    facilityName = dr["FacilityName"]?.ToString() ?? "";
            }
            if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter(
                        "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and IsActive=1", ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                catch { }
            }

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Facility", facilityName)).Append(KvRow("Berth", r.BerthName));
            sb.Append(KvRow("Port Status", portStatusText)).Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date", V(r.ReportDate)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            sb.Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("RFA Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
            sb.Append(KvRow("SBE ROB", r.SBE_ROB)).Append(KvRow("RFA ROB", r.RFA_ROB));
            string manoeuvringRows = sb.ToString();
            sb.Clear();

            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < nreLabels.Length; i++)
            {
                string acc = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "-") : "-";
                string hrs = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["Hours"]?.ToString() ?? "-") : "-";
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(nreLabels[i]).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(acc).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(hrs).Append(@"</td></tr>");
            }
            string nonRoutineRows = sb.ToString();
            sb.Clear();

            string windDir = string.IsNullOrEmpty(r.WindDirection) || r.WindDirection == "---Select---" ? null : r.WindDirection;
            string swellDir = string.IsNullOrEmpty(r.SwellDirection) || r.SwellDirection == "---Select---" ? null : r.SwellDirection;
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", windDir)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", swellDir)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight));
            sb.Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            string weatherRows = sb.ToString();
            sb.Clear();

            string remarksRow = @"<tr><td colspan=""8"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + (r.Remarks ?? "-") + @"</td></tr>";

            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR));
            string engineRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ME Crosshead Cons", r.LO_HO_Cons_MECC)).Append(KvRow("ME Cylinder Cons", r.LO_HO_Cons_MECYL));
            sb.Append(KvRow("AE Crosshead Cons", r.LO_HO_Cons_AECC)).Append(KvRow("Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil));
            sb.Append(KvRow("ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB)).Append(KvRow("ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB)).Append(KvRow("Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB));
            string lubeOilRows = sb.ToString();
            sb.Clear();

            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string sbe = dr.Table.Columns.Contains("SBE") ? dr["SBE"]?.ToString() : "";
                    string rfa = dr.Table.Columns.Contains("RFA") ? dr["RFA"]?.ToString() : "";
                    string robVal = string.IsNullOrEmpty(sbe) && string.IsNullOrEmpty(rfa) ? "-" : (sbe ?? "-") + " / " + (rfa ?? "-");
                    sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", robVal));
                }
            }
            string fuelRobRows = sb.ToString();
            sb.Clear();

            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
                    sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
            }
            string bunkerRows = sb.ToString();
            sb.Clear();

            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                decimal vlsfo = 0, mdo = 0;
                foreach (DataRow dr in dtFuelCons.Rows)
                {
                    string ft = dr["FuelType"]?.ToString() ?? "";
                    decimal val = 0;
                    if (dr["Value"] != null && dr["Value"] != DBNull.Value) val = Convert.ToDecimal(dr["Value"]);
                    if (ft.Equals("VLSFO", StringComparison.OrdinalIgnoreCase)) vlsfo += val;
                    else if (ft.Equals("MDO", StringComparison.OrdinalIgnoreCase)) mdo += val;
                }
                sb.Append(KvRow("VLSFO (Total)", vlsfo.ToString("0.000"))).Append(KvRow("MDO (Total)", mdo.ToString("0.000")));
            }
            string fuelConsRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Qty Grade 1", r.Qty_Grade1)).Append(KvRow("Qty Grade 2", r.Qty_Grade2));
            string cargoRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            string ballastRow = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            string freshWaterRows = sb.ToString();

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{MANOEUVRING_ROWS}}", manoeuvringRows)
                .Replace("{{NON_ROUTINE_ROWS}}", nonRoutineRows)
                .Replace("{{WEATHER_ROWS}}", weatherRows)
                .Replace("{{REMARKS_ROW}}", remarksRow)
                .Replace("{{ENGINE_ROWS}}", engineRows)
                .Replace("{{LUBE_OIL_ROWS}}", lubeOilRows)
                .Replace("{{FUEL_ROB_ROWS}}", fuelRobRows)
                .Replace("{{BUNKER_ROWS}}", bunkerRows)
                .Replace("{{FUEL_CONS_ROWS}}", fuelConsRows)
                .Replace("{{OT_ROB_OXY_Full}}", V(r.OT_ROB_OXY_Full))
                .Replace("{{OT_ROB_OXY_InUse}}", V(r.OT_ROB_OXY_InUse))
                .Replace("{{OT_ROB_OXY_Empty}}", V(r.OT_ROB_OXY_Empty))
                .Replace("{{OT_ROB_ACYT_Full}}", V(r.OT_ROB_ACYT_Full))
                .Replace("{{OT_ROB_ACYT_InUse}}", V(r.OT_ROB_ACYT_InUse))
                .Replace("{{OT_ROB_ACYT_Empty}}", V(r.OT_ROB_ACYT_Empty))
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{SLOPS_ROB_Oil}}", V(r.SlopsROB_Oil))
                .Replace("{{SLOPS_ROB_Water}}", V(r.SlopsROB_Water))
                .Replace("{{SLOPS_ROB_Total}}", V(r.SlopsROB_Total))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(BerthingReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = "";
            string portStatusText = r.PortStatus.ToString();
            string facilityName = r.FacilityName ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                if (string.IsNullOrEmpty(facilityName) && dtMain.Columns.Contains("FacilityName"))
                    facilityName = dr["FacilityName"]?.ToString() ?? "";
            }
            if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter(
                        "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and IsActive=1", ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                catch { }
            }

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Berthing Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Berthing Report - Navigation (").Append(V(vesselName)).Append(@")</td></tr>");

            // Navigation header
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Facility", facilityName)).Append(KvRow("Berth", r.BerthName));
            sb.Append(KvRow("Port Status", portStatusText)).Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date", V(r.ReportDate)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");

            // Manoeuvring & SBE/RFA
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Manoeuvring & SBE/RFA</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            sb.Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("RFA Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
            sb.Append(KvRow("SBE ROB", r.SBE_ROB)).Append(KvRow("RFA ROB", r.RFA_ROB));
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
            string windDir2 = string.IsNullOrEmpty(r.WindDirection) || r.WindDirection == "---Select---" ? null : r.WindDirection;
            string swellDir2 = string.IsNullOrEmpty(r.SwellDirection) || r.SwellDirection == "---Select---" ? null : r.SwellDirection;
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", windDir2)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", swellDir2)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight));
            sb.Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Berthing Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");

            // Engine
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR));
            sb.Append(@"</table></td></tr>");

            // Lube Oil & Hydraulic Oil
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Lube Oil & Hydraulic Oil</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ME Crosshead Cons", r.LO_HO_Cons_MECC)).Append(KvRow("ME Cylinder Cons", r.LO_HO_Cons_MECYL));
            sb.Append(KvRow("AE Crosshead Cons", r.LO_HO_Cons_AECC)).Append(KvRow("Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil));
            sb.Append(KvRow("ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB)).Append(KvRow("ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB)).Append(KvRow("Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB));
            sb.Append(@"</table></td></tr>");

            // Fuel ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT (SBE/RFA)</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string sbe = dr.Table.Columns.Contains("SBE") ? dr["SBE"]?.ToString() : "";
                    string rfa = dr.Table.Columns.Contains("RFA") ? dr["RFA"]?.ToString() : "";
                    sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", string.IsNullOrEmpty(sbe) && string.IsNullOrEmpty(rfa) ? "-" : (sbe ?? "-") + " / " + (rfa ?? "-")));
                }
            }
            sb.Append(@"</table></td></tr>");

            // Bunker
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows) sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
            sb.Append(@"</table></td></tr>");

            // Fuel Consumption
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Consumption in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                decimal vlsfo = 0, mdo = 0;
                foreach (DataRow dr in dtFuelCons.Rows)
                {
                    string ft = dr["FuelType"]?.ToString() ?? "";
                    decimal val = 0;
                    if (dr["Value"] != null && dr["Value"] != DBNull.Value) val = Convert.ToDecimal(dr["Value"]);
                    if (ft.Equals("VLSFO", StringComparison.OrdinalIgnoreCase)) vlsfo += val;
                    else if (ft.Equals("MDO", StringComparison.OrdinalIgnoreCase)) mdo += val;
                }
                sb.Append(KvRow("VLSFO (Total)", vlsfo.ToString("0.000"))).Append(KvRow("MDO (Total)", mdo.ToString("0.000")));
            }
            sb.Append(@"</table></td></tr>");

            // Other ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Cargo
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Qty Grade 1", r.Qty_Grade1)).Append(KvRow("Qty Grade 2", r.Qty_Grade2));
            sb.Append(@"</table></td></tr>");

            // Slops ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Total)).Append(@"</td></tr>");
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

        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }
    }
}
