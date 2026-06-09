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
    /// Builds HTML email body for Fresh Water Report. Uses FreshWaterReport.html template when available.
    /// Uses FreshWaterReport data model with port, meter reading, and quantity details.
    /// </summary>
    public static class FreshWaterReportEmailTemplate
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/FreshWaterReport.html";
        // Public portal URL used to build clickable attachment-download links in emails.
        // Update if the live portal hostname changes.
        private const string SiteBaseUrl = "https://sisv.mooringplan.com";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? d.Value.ToString("0.000") : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime dt) => dt != DateTime.MinValue ? dt.ToString(DateTimeFormat) : "-";

        /// <summary>Renders the attachment file name as an HTML anchor that downloads the file
        /// via the FreshWater controller's OpenPDF action when clicked. Falls back to plain text
        /// (or "-" if empty) when no file is present.</summary>
        private static string FileLink(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "-";
            string ext = System.IO.Path.GetExtension(fileName) ?? "";
            string url = SiteBaseUrl + "/Report/FreshWater/OpenPDF"
                       + "?fileName=" + System.Web.HttpUtility.UrlEncode(fileName)
                       + "&fileExtension=" + System.Web.HttpUtility.UrlEncode(ext);
            string safeName = System.Web.HttpUtility.HtmlEncode(fileName);
            return @"<a href=""" + url + @""" style=""color:#1a73e8;text-decoration:underline;"" target=""_blank"" rel=""noopener"">" + safeName + @"</a>";
        }

        /// <param name="reportId">When set (from export filename R{id}), used to fetch the fresh water report by ID.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            if (!reportId.HasValue || reportId.Value <= 0) return null;

            int id = reportId.Value;
            var fwRList = CommonMethods.editFreshWaterRList(id, vesselId, "FreshWaterReport");
            var fwRBind = fwRList?.Where(x => x.Id == id).FirstOrDefault();
            if (fwRBind == null) return null;

            // Backfill directly from FreshWaterReport table in case spCommonEditList aliases columns
            // to names that don't map to the long-named model properties (same family of bug as Departure).
            try
            {
                using (var adp = new SqlDataAdapter(
                    "select PortName, PortName_others, Facility_Name, VendorDetails, Received_Date, " +
                    "Intial_Meter_Reading_MT_supplied, Final_Meter_Reading_MT, " +
                    "Difference_in_Meter_Reading_MT, QTY_supplied_MT, File_Name " +
                    "from FreshWaterReport where Id=" + id, ConnectionBulder.con))
                {
                    var dtBackfill = new DataTable();
                    adp.Fill(dtBackfill);
                    if (dtBackfill.Rows.Count > 0)
                    {
                        var br = dtBackfill.Rows[0];
                        if (br["PortName"] != DBNull.Value) fwRBind.PortName = br["PortName"].ToString();
                        if (br["PortName_others"] != DBNull.Value) fwRBind.PortName_others = br["PortName_others"].ToString();
                        if (br["Facility_Name"] != DBNull.Value) fwRBind.Facility_Name = br["Facility_Name"].ToString();
                        if (br["VendorDetails"] != DBNull.Value) fwRBind.VendorDetails = br["VendorDetails"].ToString();
                        if (br["Received_Date"] != DBNull.Value) fwRBind.Received_Date = Convert.ToDateTime(br["Received_Date"]);
                        if (br["Intial_Meter_Reading_MT_supplied"] != DBNull.Value) fwRBind.Intial_Meter_Reading_MT_supplied = Convert.ToDecimal(br["Intial_Meter_Reading_MT_supplied"]);
                        if (br["Final_Meter_Reading_MT"] != DBNull.Value) fwRBind.Final_Meter_Reading_MT = Convert.ToDecimal(br["Final_Meter_Reading_MT"]);
                        if (br["Difference_in_Meter_Reading_MT"] != DBNull.Value) fwRBind.Difference_in_Meter_Reading_MT = Convert.ToDecimal(br["Difference_in_Meter_Reading_MT"]);
                        if (br["QTY_supplied_MT"] != DBNull.Value) fwRBind.QTY_supplied_MT = Convert.ToDecimal(br["QTY_supplied_MT"]);
                        if (br["File_Name"] != DBNull.Value) fwRBind.File_Name = br["File_Name"].ToString();
                    }
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
                return ApplyTemplate(template, fwRBind, vesselName);
            }
            return BuildHtmlInline(fwRBind, vesselName);
        }

        private static string ApplyTemplate(string template, FreshWaterReport r, string vesselName)
        {
            string portDisplay = ResolvePortDisplay(r);

            var sb = new StringBuilder();
            sb.Append(KvRow("Port Name", portDisplay)).Append(KvRow("Facility Name", r.Facility_Name));
            sb.Append(KvRow("Received Date", Vdt(r.Received_Date)));
            sb.Append(KvRow("Vendor Details", r.VendorDetails));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Initial Meter Reading Supply(MT)", r.Intial_Meter_Reading_MT_supplied));
            sb.Append(KvRow("Final Meter Reading(MT)", r.Final_Meter_Reading_MT));
            sb.Append(KvRow("Difference Meter Reading(MT)", r.Difference_in_Meter_Reading_MT));
            sb.Append(KvRow("QTY Received(MT)", r.QTY_supplied_MT));
            string meterRows = sb.ToString();

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", Vdt(r.Received_Date))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{METER_ROWS}}", meterRows)
                .Replace("{{FILE_NAME}}", FileLink(r.File_Name));
        }

        private static string BuildHtmlInline(FreshWaterReport r, string vesselName)
        {
            string portDisplay = ResolvePortDisplay(r);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Fresh Water Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.Received_Date)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");

            // Fresh Water Details header
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Fresh Water Report - Details (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Port Name", portDisplay)).Append(KvRow("Facility Name", r.Facility_Name));
            sb.Append(KvRow("Received Date", Vdt(r.Received_Date)));
            sb.Append(KvRow("Vendor Details", r.VendorDetails));
            sb.Append(@"</table></td></tr>");

            // Meter Reading & Quantity
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Meter Reading & Quantity</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Initial Meter Reading Supply(MT)", r.Intial_Meter_Reading_MT_supplied));
            sb.Append(KvRow("Final Meter Reading(MT)", r.Final_Meter_Reading_MT));
            sb.Append(KvRow("Difference Meter Reading(MT)", r.Difference_in_Meter_Reading_MT));
            sb.Append(KvRow("QTY Received(MT)", r.QTY_supplied_MT));
            sb.Append(@"</table></td></tr>");

            // Attached Document — file name rendered as a clickable hyperlink to the portal's
            // OpenPDF endpoint. The actual file is also attached to the outgoing email so users
            // who cannot reach the portal can still download it from the message itself.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Attached Document</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(@"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">Attachment</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FileLink(r.File_Name)).Append(@"</td></tr>");
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

        /// <summary>When PortName is "Others" (or blank), use the custom PortName_others entered by the user.</summary>
        private static string ResolvePortDisplay(FreshWaterReport r)
        {
            string port = r.PortName?.Trim();
            string others = r.PortName_others?.Trim();
            bool isOthers = !string.IsNullOrEmpty(port) && port.Equals("Others", StringComparison.OrdinalIgnoreCase);
            if ((isOthers || string.IsNullOrEmpty(port)) && !string.IsNullOrEmpty(others)) return others;
            return port;
        }
    }
}
