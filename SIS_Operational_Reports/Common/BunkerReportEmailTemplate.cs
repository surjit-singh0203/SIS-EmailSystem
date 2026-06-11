using DataBuildingLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/BunkerReport.html";
        // Public portal URL used to build clickable BDN-Report download links in emails.
        // Resolves to the host that is actually generating the email (so the link points
        // at the same server where SaveAttachmentFilesFromSheet / ExtractAttachmentSheetDirect
        // just wrote the file). Falls back to the hardcoded production URL when no current
        // HTTP context is available (e.g. background scheduled run with no incoming request).
        private static string SiteBaseUrl
        {
            get
            {
                try
                {
                    var ctx = System.Web.HttpContext.Current;
                    if (ctx != null && ctx.Request != null && ctx.Request.Url != null)
                        return ctx.Request.Url.GetLeftPart(UriPartial.Authority); // e.g. "https://sisnovastaging.mooringplan.com"
                }
                catch { }
                return "https://sisv.mooringplan.com";
            }
        }

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? d.Value.ToString("0.000") : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime dt) => dt != DateTime.MinValue ? dt.ToString(DateTimeFormat) : "-";

        /// <summary>Renders the BDN-Report file name as an HTML anchor that downloads the file
        /// via the Bunker controller's OpenPDF action when clicked. Falls back to "-" when
        /// no file is present.</summary>
        private static string FileLink(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "-";
            // Direct link to the static file path (~/Bunker_LabAnalysisReport/<fileName>)
            // so the browser downloads the actual file instead of routing through the
            // OpenPDF controller action. The `download` attribute hints the browser to
            // save the file rather than render inline.
            string url = SiteBaseUrl + "/Bunker_LabAnalysisReport/" + System.Web.HttpUtility.UrlPathEncode(fileName);
            string safeName = System.Web.HttpUtility.HtmlEncode(fileName);
            return @"<a href=""" + url + @""" download=""" + safeName + @""" style=""color:#1a73e8;text-decoration:underline;"" rel=""noopener"">" + safeName + @"</a>";
        }

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
                    "BunkerHosedisconnected, BargeCastOff, FirstName, LastName, LabAnalysisReport_Name, " +
                    "Created_Date, Modified_Date " +
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
                        if (br["Created_Date"] != DBNull.Value) bunkerRBind.CreatedDate = Convert.ToDateTime(br["Created_Date"]);
                        if (br["Modified_Date"] != DBNull.Value) bunkerRBind.ModifiedDate = Convert.ToDateTime(br["Modified_Date"]);
                    }
                }
            }
            catch { }

            // Fetch fuel details using the same SP the Edit view calls, but with a FRESH
            // dedicated SqlConnection rather than the shared static ConnectionBulder.con.
            // The shared connection is a process-wide singleton; if a prior operation on
            // another thread left it in a bad state, subsequent calls (like this one in
            // the email path) silently return truncated result sets. A dedicated connection
            // makes this call independent of what any other thread is doing.
            var fuelList = LoadBunkerFuelListIsolated(id, vesselId);
            foreach (var item in fuelList)
            {
                if (item.Fuel_type_Id == 5) item.Fuel_type = "VLSFO";
                if (item.Fuel_type_Id == 2) item.Fuel_type = "MDO";
            }

            // Fetch main details
            DataTable dtMain = new DataTable();
            try
            {
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    // Recipient-lookup date: prefer ModifiedDate (last edit) → fall back to
                    // CreatedDate when the report has never been edited → fall back to
                    // BargeAlongside (original domain date) so legacy reports without audit
                    // timestamps still resolve recipients. Email body display is unchanged.
                    DateTime triggerDate = bunkerRBind.ModifiedDate ?? bunkerRBind.CreatedDate ?? bunkerRBind.BargeAlongside;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", bunkerRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", triggerDate.ToString("yyyy-MM-dd"));
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

        /// <summary>
        /// Calls spCommonEditList exactly the way BunkerController.Edit does, but on a fresh
        /// dedicated SqlConnection so it isn't affected by state on the shared static
        /// ConnectionBulder.con. Falls back to the CommonMethods helper if the isolated
        /// call fails for any reason.
        /// </summary>
        private static List<BukerFuelList> LoadBunkerFuelListIsolated(int reportId, int vesselId)
        {
            var list = new List<BukerFuelList>();
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("spCommonEditList", con) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@Id", reportId);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId.ToString());
                    cmd.Parameters.AddWithValue("@Action", "BunkerFReport");
                    con.Open();
                    using (var dt = new DataTable())
                    using (var adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(dt);
                        foreach (DataRow fr in dt.Rows)
                        {
                            list.Add(MapFuelRow(fr));
                        }
                    }
                }
            }
            catch
            {
                list.Clear();
            }
            // Fallback to the shared helper if the isolated call yielded nothing.
            if (list.Count == 0)
            {
                list = CommonMethods.editBunkerFuelList(reportId, vesselId.ToString(), "BunkerFReport") ?? new List<BukerFuelList>();
            }
            return list;
        }

        private static BukerFuelList MapFuelRow(DataRow fr)
        {
            var f = new BukerFuelList();
            // Map by column name when present, defensively (the SP's output column names
            // are the source of truth — we look up each property's matching column).
            foreach (PropertyInfo p in typeof(BukerFuelList).GetProperties())
            {
                if (!fr.Table.Columns.Contains(p.Name)) continue;
                object v = fr[p.Name];
                if (v == null || v == DBNull.Value) continue;
                try
                {
                    Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    p.SetValue(f, Convert.ChangeType(v, t), null);
                }
                catch { /* skip on type mismatch */ }
            }
            return f;
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
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port Name", portDisplay));
            sb.Append(KvRow("Supplier", r.Supplier)).Append(KvRow("Barge Name", r.BargeName));
            sb.Append(KvRow("Master Name", r.FirstName));
            sb.Append(KvRow("Chief Engineer", r.LastName));
            string headerRows = sb.ToString();
            sb.Clear();

            // --- Bunker Timing ---
            sb.Append(KvRow("Barge Alongside", Vdt(r.BargeAlongside)));
            sb.Append(KvRow("Bunker Hose Connected", Vdt(r.BunkerHoseConnected)));
            sb.Append(KvRow("Commenced Bunkering", Vdt(r.CommencedBunkering)));
            sb.Append(KvRow("Bunkering Completed", Vdt(r.BunkeringCompleted)));
            sb.Append(KvRow("Bunker Hose disconnected", Vdt(r.BunkerHosedisconnected)));
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
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(V(fuel.BDN_Number));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V(fuel.Fuel_Density));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V(fuel.Sulphur_content));
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
                .Replace("{{LAB_ANALYSIS_FILE}}", FileLink(r.LabAnalysisReport_Name));
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

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Bunker Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.BargeAlongside)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");

            // Bunker Details header
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Bunker Report - Details (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            string portDisplayInline = ResolvePortDisplay(r);
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port Name", portDisplayInline));
            sb.Append(KvRow("Supplier", r.Supplier)).Append(KvRow("Barge Name", r.BargeName));
            sb.Append(KvRow("Master Name", r.FirstName));
            sb.Append(KvRow("Chief Engineer", r.LastName));
            sb.Append(@"</table></td></tr>");

            // Bunker Timing
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Timing</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Barge Alongside", Vdt(r.BargeAlongside)));
            sb.Append(KvRow("Bunker Hose Connected", Vdt(r.BunkerHoseConnected)));
            sb.Append(KvRow("Commenced Bunkering", Vdt(r.CommencedBunkering)));
            sb.Append(KvRow("Bunkering Completed", Vdt(r.BunkeringCompleted)));
            sb.Append(KvRow("Bunker Hose disconnected", Vdt(r.BunkerHosedisconnected)));
            sb.Append(KvRow("Barge Cast Off", Vdt(r.BargeCastOff)));
            sb.Append(@"</table></td></tr>");

            // Fuel Details
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Details</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Fuel Type</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Quantity Received(MT)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">BDN Number</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Fuel Density(Kg/m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Sulphur content(%)</td></tr>");
            if (fuelList != null && fuelList.Count > 0)
            {
                foreach (var fuel in fuelList)
                {
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(V(fuel.Fuel_type));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.BDN));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(V(fuel.BDN_Number));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.Fuel_Density));
                    sb.Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(fuel.Sulphur_content));
                    sb.Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(V(r.Remarks)).Append(@"</td></tr>");

            // BDN Report — file name rendered as a clickable hyperlink to the portal's OpenPDF
            // endpoint. The actual file is also attached to the outgoing email so users who
            // cannot reach the portal can still download it directly from the message.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">BDN Report</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(@"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">BDN Report</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FileLink(r.LabAnalysisReport_Name)).Append(@"</td></tr>");
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
