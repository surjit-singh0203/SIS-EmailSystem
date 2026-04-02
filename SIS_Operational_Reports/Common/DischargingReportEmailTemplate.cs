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
    /// Builds HTML email body for Discharging Report. Uses DischargingReport.html template when available.
    /// Same design as Berthing Report, Loading Report, Departure Report, Arrival Report and Daily Noon Report.
    /// </summary>
    public static class DischargingReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/DischargingReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(int? i) => i.HasValue ? i.Value.ToString() : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <param name="reportId">When set (from export filename R{id}), matches Excel save fallback via editdischargingRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            DischargingReport disRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                var list = CommonMethods.editdischargingRList(reportId.Value, vesselId, "DischargingReport");
                disRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (disRBind == null)
            {
                var list = CommonMethods.editdischargingRListDashbord(reportdate, vesselId, "DischargingReport");
                disRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (disRBind == null)
            {
                var list = CommonMethods.editdischargingRListDashbord(reportdate, vesselId, "DischargingReportR");
                disRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (disRBind == null) return null;

            int id = disRBind.Id;
            DataTable dtCargo = new DataTable(), dtStoppage = new DataTable(), dtPumpsUse = new DataTable(), dtMain = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select * from DS_Cargo where DSId=" + id + " and VesselId=" + vesselId, ConnectionBulder.con))
                    adp.Fill(dtCargo);
                using (var adp = new SqlDataAdapter("select * from LR_Stoppage where DCId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=1", ConnectionBulder.con))
                    adp.Fill(dtStoppage);
                using (var adp = new SqlDataAdapter("select a.*, b.Name as PumpName from LR_DCR_PumpsUse a left join tblPumps b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                    adp.Fill(dtPumpsUse);
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", disRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "DischargingReport");
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
                return ApplyTemplate(template, disRBind, vesselName, dtCargo, dtStoppage, dtPumpsUse, dtMain);
            }
            return BuildHtmlInline(disRBind, vesselName, dtCargo, dtStoppage, dtPumpsUse, dtMain);
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

        private static string ApplyTemplate(string template, DischargingReport r, string vesselName, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
        {
            string voyNo = r.VoyageId.ToString();
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
            }

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Vessel", r.VesselName ?? vesselName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(KvRow("Power Packs Onboard", V(r.Power_Packs_onboard))).Append(KvRow("Power Packs Used", V(r.Power_Packs_Used)));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Times", r.Times)).Append(KvRow("Rate", r.Rate));
            sb.Append(KvRow("Hose Connection", r.Hose_Connection)).Append(KvRow("High H2S", r.High_H2S));
            string lopRows = sb.ToString();
            sb.Clear();

            string cargoRows = BuildCargoRowsHtml(dtCargo);
            string stoppageRows = BuildStoppageRowsHtml(dtStoppage);
            string pumpsRows = BuildPumpsRowsHtml(dtPumpsUse);
            string remarksRow = @"<tr><td colspan=""10"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + (r.Remarks ?? "-") + @"</td></tr>";

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", Vdt(r.ReportDateTime))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{LOP_ROWS}}", lopRows)
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{STOPPAGE_ROWS}}", stoppageRows)
                .Replace("{{PUMPS_ROWS}}", pumpsRows)
                .Replace("{{REMARKS_ROW}}", remarksRow);
        }

        private static string BuildHtmlInline(DischargingReport r, string vesselName, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
        {
            string voyNo = r.VoyageId.ToString();
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
            }

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Discharging Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.ReportDateTime)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Discharging Report (").Append(V(vesselName)).Append(@")</td></tr>");

            // General Info
            sb.Append(@"<tr><td colspan=""10"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Vessel", r.VesselName ?? vesselName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(KvRow("Power Packs Onboard", V(r.Power_Packs_onboard))).Append(KvRow("Power Packs Used", V(r.Power_Packs_Used)));
            sb.Append(@"</table></td></tr>");

            // LOP
            sb.Append(@"<tr><td colspan=""10"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Letter of Protest (LOP)</td></tr>");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Times", r.Times)).Append(KvRow("Rate", r.Rate));
            sb.Append(KvRow("Hose Connection", r.Hose_Connection)).Append(KvRow("High H2S", r.High_H2S));
            sb.Append(@"</table></td></tr>");

            // Cargo Details
            sb.Append(@"<tr><td colspan=""10"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Details</td></tr>");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Cargo Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Discharge Date/Time</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Terminal Rate</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Pressure Req.</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Avg Rate</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Avg Pressure</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Pumps</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Total Discharged</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Balance</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Actual Comp</td></tr>");
            if (dtCargo != null)
            {
                foreach (DataRow dr in dtCargo.Rows)
                {
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "CargoName")).Append(TdDt(dr, "DischargeDatetime"));
                    sb.Append(TdDec(dr, "Terminal_Acceptable_Discharging_Rate")).Append(TdDec(dr, "Discharging_pressure_Requested"));
                    sb.Append(TdDec(dr, "Average_Discharge_Rate_ByVessel")).Append(TdDec(dr, "Average_Discharge_pressure_ByVessel"));
                    sb.Append(Td(dr, "No_of_Pumps_Use"));
                    sb.Append(TdDec(dr, "Total_CargoDischarged")).Append(TdDec(dr, "Balance_Cargo_ToBe_Deischarged"));
                    sb.Append(TdDt(dr, "ActualCompDateTime"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Stoppage Details
            sb.Append(@"<tr><td colspan=""10"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Stoppage Details</td></tr>");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Reason</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">From</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">To</td></tr>");
            if (dtStoppage != null)
            {
                foreach (DataRow dr in dtStoppage.Rows)
                {
                    sb.Append(@"<tr>");
                    string reason = dr.Table.Columns.Contains("Reason") && dr["Reason"] != DBNull.Value ? dr["Reason"]?.ToString() : "";
                    if (string.IsNullOrEmpty(reason) && dr.Table.Columns.Contains("Stoppage")) reason = dr["Stoppage"]?.ToString() ?? "-";
                    sb.Append(@"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(reason ?? "-").Append(@"</td>");
                    sb.Append(TdDt(dr, "DateTimeFrom")).Append(TdDt(dr, "DateTimeTo"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Cargo Pump Use
            sb.Append(@"<tr><td colspan=""10"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Pump Use</td></tr>");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Pump Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Rate</td></tr>");
            if (dtPumpsUse != null)
            {
                foreach (DataRow dr in dtPumpsUse.Rows)
                {
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "PumpName")).Append(TdDec(dr, "Rate"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""10"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""10"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");

            sb.Append(@"</table>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>");
            sb.Append(@"<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Thank you,</p><p style=""margin:4px 0 0 0;"">Team SIS</p></body></html>");
            return sb.ToString();
        }

        private static string BuildCargoRowsHtml(DataTable dtCargo)
        {
            if (dtCargo == null || dtCargo.Rows.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (DataRow dr in dtCargo.Rows)
            {
                sb.Append(@"<tr>");
                sb.Append(Td(dr, "CargoName")).Append(TdDt(dr, "DischargeDatetime"));
                sb.Append(TdDec(dr, "Terminal_Acceptable_Discharging_Rate")).Append(TdDec(dr, "Discharging_pressure_Requested"));
                sb.Append(TdDec(dr, "Average_Discharge_Rate_ByVessel")).Append(TdDec(dr, "Average_Discharge_pressure_ByVessel"));
                sb.Append(Td(dr, "No_of_Pumps_Use"));
                sb.Append(TdDec(dr, "Total_CargoDischarged")).Append(TdDec(dr, "Balance_Cargo_ToBe_Deischarged"));
                sb.Append(TdDt(dr, "ActualCompDateTime"));
                sb.Append(@"</tr>");
            }
            return sb.ToString();
        }

        private static string BuildStoppageRowsHtml(DataTable dtStoppage)
        {
            if (dtStoppage == null || dtStoppage.Rows.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (DataRow dr in dtStoppage.Rows)
            {
                sb.Append(@"<tr>");
                string reason = dr.Table.Columns.Contains("Reason") && dr["Reason"] != DBNull.Value ? dr["Reason"]?.ToString() : "";
                if (string.IsNullOrEmpty(reason) && dr.Table.Columns.Contains("Stoppage")) reason = dr["Stoppage"]?.ToString() ?? "-";
                sb.Append(@"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(reason ?? "-").Append(@"</td>");
                sb.Append(TdDt(dr, "DateTimeFrom")).Append(TdDt(dr, "DateTimeTo"));
                sb.Append(@"</tr>");
            }
            return sb.ToString();
        }

        private static string BuildPumpsRowsHtml(DataTable dtPumpsUse)
        {
            if (dtPumpsUse == null || dtPumpsUse.Rows.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (DataRow dr in dtPumpsUse.Rows)
            {
                sb.Append(@"<tr>");
                sb.Append(Td(dr, "PumpName")).Append(TdDec(dr, "Rate"));
                sb.Append(@"</tr>");
            }
            return sb.ToString();
        }

        private static string Td(DataRow dr, string col)
        {
            string val = dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value ? dr[col]?.ToString() ?? "-" : "-";
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">" + val + @"</td>";
        }

        private static string TdDec(DataRow dr, string col)
        {
            string val = "-";
            if (dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value)
            {
                decimal d;
                if (decimal.TryParse(dr[col].ToString(), out d))
                    val = d == Math.Truncate(d) ? d.ToString("0") : d.ToString("0.000");
                else
                    val = dr[col].ToString();
            }
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + val + @"</td>";
        }

        private static string TdDt(DataRow dr, string col)
        {
            string val = "-";
            if (dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value)
            {
                DateTime d;
                if (DateTime.TryParse(dr[col].ToString(), out d))
                    val = d.ToString(DateTimeFormat);
                else
                    val = dr[col].ToString();
            }
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">" + val + @"</td>";
        }

        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }
    }
}
