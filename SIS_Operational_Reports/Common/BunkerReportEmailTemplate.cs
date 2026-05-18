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
    /// Builds HTML email body for Bunker Report. Uses BunkerReport.html template when available.
    /// Uses BunkerReport data model with Bunker Details and Fuel Details sections.
    /// </summary>
    public static class BunkerReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/BunkerReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime dt) => dt != DateTime.MinValue ? dt.ToString(DateTimeFormat) : "-";

        /// <param name="reportId">When set (from export filename R{id}), used to fetch the bunker report by ID.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            if (!reportId.HasValue || reportId.Value <= 0) return null;

            int id = reportId.Value;
            var bunkerRList = CommonMethods.editBunkerRList(id, vesselId, "BunkerReport");
            var bunkerRBind = bunkerRList?.Where(x => x.Id == id).FirstOrDefault();
            if (bunkerRBind == null) return null;

            // Backfill from BunkerReport table directly in case spCommonEditList aliases columns
            // to names that don't map to the model's long property names (same family of bug as Departure / FreshWater).
            try
            {
                using (var adp = new SqlDataAdapter(
                    "select PortName, PortName_others, Supplier, BargeName, Remarks, " +
                    "BargeAlongside, BunkerHoseConnected, CommencedBunkering, BunkeringCompleted, " +
                    "BunkerHosedisconnected, BargeCastOff, FirstName, LastName, LabAnalysisReport_Name " +
                    "from BunkerReport where Id=" + id, ConnectionBulder.con))
                {
                    var dtBackfill = new DataTable();
                    adp.Fill(dtBackfill);
                    if (dtBackfill.Rows.Count > 0)
                    {
                        var br = dtBackfill.Rows[0];
                        if (br["PortName"] != DBNull.Value) bunkerRBind.PortName = br["PortName"].ToString();
                        if (br["PortName_others"] != DBNull.Value) bunkerRBind.PortName_others = br["PortName_others"].ToString();
                        if (br["Supplier"] != DBNull.Value) bunkerRBind.Supplier = br["Supplier"].ToString();
                        if (br["BargeName"] != DBNull.Value) bunkerRBind.BargeName = br["BargeName"].ToString();
                        if (br["Remarks"] != DBNull.Value) bunkerRBind.Remarks = br["Remarks"].ToString();
                        if (br["BargeAlongside"] != DBNull.Value) bunkerRBind.BargeAlongside = Convert.ToDateTime(br["BargeAlongside"]);
                        if (br["BunkerHoseConnected"] != DBNull.Value) bunkerRBind.BunkerHoseConnected = Convert.ToDateTime(br["BunkerHoseConnected"]);
                        if (br["CommencedBunkering"] != DBNull.Value) bunkerRBind.CommencedBunkering = Convert.ToDateTime(br["CommencedBunkering"]);
                        if (br["BunkeringCompleted"] != DBNull.Value) bunkerRBind.BunkeringCompleted = Convert.ToDateTime(br["BunkeringCompleted"]);
                        if (br["BunkerHosedisconnected"] != DBNull.Value) bunkerRBind.BunkerHosedisconnected = Convert.ToDateTime(br["BunkerHosedisconnected"]);
                        if (br["BargeCastOff"] != DBNull.Value) bunkerRBind.BargeCastOff = Convert.ToDateTime(br["BargeCastOff"]);
                        if (br["FirstName"] != DBNull.Value) bunkerRBind.FirstName = br["FirstName"].ToString();
                        if (br["LastName"] != DBNull.Value) bunkerRBind.LastName = br["LastName"].ToString();
                        if (br["LabAnalysisReport_Name"] != DBNull.Value) bunkerRBind.LabAnalysisReport_Name = br["LabAnalysisReport_Name"].ToString();
                    }
                }
            }
            catch { }

            // Fetch fuel details
            var fuelList = CommonMethods.editBunkerFuelList(id, vesselId.ToString(), "BunkerFReport");
            if (fuelList != null)
            {
                foreach (var item in fuelList)
                {
                    if (item.Fuel_type_Id == 5) item.Fuel_type = "VLSFO";
                    if (item.Fuel_type_Id == 2) item.Fuel_type = "MDO";
                }
            }

            // Fetch main details
            DataTable dtMain = new DataTable();
            try
            {
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", bunkerRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", bunkerRBind.BargeAlongside.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "BunkerReport");
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dtMain);
                }
            }
            catch { }

            string vesselName = CommonClass.GetVesselNamesByImoNo(vesselId.ToString());
            if (string.IsNullOrEmpty(vesselName)) vesselName = "Vessel " + vesselId;

            // Always use the inline HTML builder. The external Templates/BunkerReport.html
            // shipped with older deployments still contains stale placeholders, so the inline
            // builder (mirrors the Berthing / Daily Noon fix) guarantees the email renders.
            return BuildHtmlInline(bunkerRBind, vesselName, fuelList, dtMain);
        }

        /// <summary>
        /// Resolves the display VoyageNumber (e.g. "61") from the Voyage table PK
        /// using the same GetVoyageList method that the web form uses.
        /// </summary>
        private static string LookupVoyageNumber(int voyageId, int vesselId)
        {
            if (voyageId <= 0) return null;
            try
            {
                var voyages = CommonMethods.GetVoyageList(vesselId);
                var match = voyages?.FirstOrDefault(v => v.Id == voyageId);
                if (match != null && !string.IsNullOrWhiteSpace(match.VoyageNumber))
                    return match.VoyageNumber.Trim();
            }
            catch { }
            return null;
        }

        /// <summary>When PortName is "Others" (or blank), use the custom PortName_others entered by the user.</summary>
        private static string ResolvePortDisplay(BunkerReport r)
        {
            string port = r.PortName?.Trim();
            string others = r.PortName_others?.Trim();
            bool isOthers = !string.IsNullOrEmpty(port) && port.Equals("Others", StringComparison.OrdinalIgnoreCase);
            if ((isOthers || string.IsNullOrEmpty(port)) && !string.IsNullOrEmpty(others)) return others;
            return port;
        }

        private static string ApplyTemplate(string template, BunkerReport r, string vesselName, List<BukerFuelList> fuelList, DataTable dtMain)
        {
            string voyNo = r.voyagenumber;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            string reportedBy = ((r.FirstName ?? "") + " " + (r.LastName ?? "")).Trim();

            // --- Header ---
            string portDisplay = ResolvePortDisplay(r);
            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", portDisplay));
            sb.Append(KvRow("Supplier", r.Supplier)).Append(KvRow("Barge Name", r.BargeName));
            string headerRows = sb.ToString();
            sb.Clear();

            // --- Bunker Timing ---
            sb.Append(KvRow("Barge Alongside", Vdt(r.BargeAlongside)));
            sb.Append(KvRow("Bunker Hose Connected", Vdt(r.BunkerHoseConnected)));
            sb.Append(KvRow("Commenced Bunkering", Vdt(r.CommencedBunkering)));
            sb.Append(KvRow("Bunkering Completed", Vdt(r.BunkeringCompleted)));
            sb.Append(KvRow("Bunker Hose Disconnected", Vdt(r.BunkerHosedisconnected)));
            sb.Append(KvRow("Barge Cast Off", Vdt(r.BargeCastOff)));
            string timingRows = sb.ToString();
            sb.Clear();

            // --- Fuel Details ---
            if (fuelList != null && fuelList.Count > 0)
            {
                foreach (var fuel in fuelList)
                {
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(V(fuel.Fuel_type));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V(fuel.BDN));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V(fuel.Fuel_Density));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V(fuel.Sulphur_content));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(V(fuel.BDN_Number));
                    sb.Append(@"</td></tr>");
                }
            }
            string fuelRows = sb.ToString();
            sb.Clear();

            // --- Remarks ---
            string remarksRow = @"<tr><td colspan=""8"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + V(r.Remarks) + @"</td></tr>";

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", Vdt(r.BargeAlongside))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{TIMING_ROWS}}", timingRows)
                .Replace("{{FUEL_ROWS}}", fuelRows)
                .Replace("{{REMARKS_ROW}}", remarksRow)
                .Replace("{{REPORTED_BY}}", V(reportedBy))
                .Replace("{{LAB_ANALYSIS_FILE}}", V(r.LabAnalysisReport_Name));
        }

        private static string BuildHtmlInline(BunkerReport r, string vesselName, List<BukerFuelList> fuelList, DataTable dtMain)
        {
            string voyNo = r.voyagenumber;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            string reportedBy = ((r.FirstName ?? "") + " " + (r.LastName ?? "")).Trim();

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Bunker Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.BargeAlongside)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");

            // Bunker Details header
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Bunker Report - Details (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            string portDisplayInline = ResolvePortDisplay(r);
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", portDisplayInline));
            sb.Append(KvRow("Supplier", r.Supplier)).Append(KvRow("Barge Name", r.BargeName));
            sb.Append(@"</table></td></tr>");

            // Bunker Timing
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Timing</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Barge Alongside", Vdt(r.BargeAlongside)));
            sb.Append(KvRow("Bunker Hose Connected", Vdt(r.BunkerHoseConnected)));
            sb.Append(KvRow("Commenced Bunkering", Vdt(r.CommencedBunkering)));
            sb.Append(KvRow("Bunkering Completed", Vdt(r.BunkeringCompleted)));
            sb.Append(KvRow("Bunker Hose Disconnected", Vdt(r.BunkerHosedisconnected)));
            sb.Append(KvRow("Barge Cast Off", Vdt(r.BargeCastOff)));
            sb.Append(@"</table></td></tr>");

            // Fuel Details
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Details</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Fuel Type</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">BDN (MT)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Fuel Density</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Sulphur Content</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">BDN Number</td></tr>");
            if (fuelList != null && fuelList.Count > 0)
            {
                foreach (var fuel in fuelList)
                {
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(V(fuel.Fuel_type));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.BDN));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.Fuel_Density));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.Sulphur_content));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(V(fuel.BDN_Number));
                    sb.Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(V(r.Remarks)).Append(@"</td></tr>");

            // Reported By
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Reported By</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Name", reportedBy));
            sb.Append(@"</table></td></tr>");

            // Lab Analysis Report
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Lab Analysis Report</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("File Name", r.LabAnalysisReport_Name));
            sb.Append(@"</table></td></tr>");

            sb.Append(@"</table>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>");
            sb.Append(@"<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Thank you,</p><p style=""margin:4px 0 0 0;"">Team SIS</p></body></html>");
            return sb.ToString();
        }

        /// <summary>Single label-value row for 2-column layout.</summary>
        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }
    }
}
