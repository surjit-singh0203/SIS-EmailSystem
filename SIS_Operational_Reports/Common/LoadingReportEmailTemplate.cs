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
    /// Builds HTML email body for Loading Report. Uses LoadingReport.html template when available.
    /// Same design as Berthing Report, Departure Report, Arrival Report and Daily Noon Report.
    /// </summary>
    public static class LoadingReportEmailTemplate
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/LoadingReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <param name="reportId">When set (from export filename R{id}), matches Excel save fallback via editloadingRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            LoadingReport loadingRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                var list = CommonMethods.editloadingRList(reportId.Value, vesselId, "LoadingReport");
                loadingRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (loadingRBind == null)
            {
                var list = CommonMethods.editloadingRListDashboard(reportdate, vesselId, "LoadingReport");
                loadingRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (loadingRBind == null)
            {
                var list = CommonMethods.editloadingRListDashboard(reportdate, vesselId, "LoadingReportR");
                loadingRBind = list?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (loadingRBind == null) return null;

            int id = loadingRBind.Id;
            DataTable dtCargo = new DataTable(), dtStoppage = new DataTable(), dtPumpsUse = new DataTable(), dtMain = new DataTable();

            try
            {
                // Cargo: filter by LRId AND dedupe by CargoName + LoadingDatetime, taking the row
                // with the highest Id (the latest save). LR_Cargo has been observed accumulating
                // duplicate rows for the same report from double-submits; without the dedupe an
                // older row would show stale values (e.g. Size_of_Manifold_Hoses_by_Vessel = 1
                // instead of the updated 12).
                using (var adp = new SqlDataAdapter(
                    "select a.* from LR_Cargo a " +
                    "inner join (select CargoName, LoadingDatetime, max(Id) as MaxId " +
                    "            from LR_Cargo where VesselId=" + vesselId + " and LRId=" + id +
                    "            group by CargoName, LoadingDatetime) g on a.Id = g.MaxId " +
                    "where a.VesselId=" + vesselId + " and a.LRId=" + id + " order by a.Id",
                    ConnectionBulder.con))
                    adp.Fill(dtCargo);
                // Stoppage: column is `Stoppage` not `Reason` (renamed in the schema). Scope to
                // loading stoppages (LoadingDischarged=0) to mirror the web form. Dedupe by
                // (Stoppage + DateTimeFrom) taking the row with the highest Id — LR_Stoppage
                // has been observed accumulating duplicates from double-submits.
                using (var adp = new SqlDataAdapter(
                    "select a.Stoppage as Reason, a.DateTimeFrom, a.DateTimeTo from LR_Stoppage a " +
                    "inner join (select Stoppage, DateTimeFrom, max(Id) as MaxId " +
                    "            from LR_Stoppage where LRId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=0 " +
                    "            group by Stoppage, DateTimeFrom) g on a.Id = g.MaxId " +
                    "where a.LRId=" + id + " and a.VesselId=" + vesselId + " and a.LoadingDischarged=0 order by a.Id",
                    ConnectionBulder.con))
                    adp.Fill(dtStoppage);
                // Pumps: the table is `tblPump` (singular) per the Loading controller; `tblPumps` returns no rows.
                // Dedupe by pump Name (the value the user sees) — picks the latest row (MAX Id) per
                // unique name so previous saves that duplicated tblPump or LR_DCR_PumpsUse rows
                // don't render the same pump multiple times in the email.
                using (var adp = new SqlDataAdapter(
                    "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                    "inner join (select bb.Name as Name, max(aa.Id) as Id from LR_DCR_PumpsUse aa left join tblPump bb on aa.PumpId=bb.Id where aa.LRId=" + id + " and aa.VesselId=" + vesselId + " group by bb.Name) g on a.Id=g.Id " +
                    "left join tblPump b on a.PumpId=b.Id " +
                    "where a.LRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                    adp.Fill(dtPumpsUse);
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", loadingRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "LoadingReport");
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dtMain);
                }
            }
            catch { }

            string vesselName = CommonClass.GetVesselNamesByImoNo(vesselId.ToString());
            if (string.IsNullOrEmpty(vesselName)) vesselName = "Vessel " + vesselId;

            // Always use inline HTML builder. The external Templates/LoadingReport.html shipped
            // with older deployments still contains stale placeholders, so the inline builder
            // (which mirrors the Berthing / Daily Noon fix) guarantees the email renders.
            return BuildHtmlInline(loadingRBind, vesselName, dtCargo, dtStoppage, dtPumpsUse, dtMain);
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

        // VoyageLeg.Id is not unique across vessels/voyages — must scope by VoyageId + VesselId.
        private static string LookupLegText(int legPortId, int voyageId, int vesselId)
        {
            try
            {
                if (legPortId > 0)
                {
                    using (var adp = new SqlDataAdapter(
                        "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + legPortId + " and VoyageId=" + voyageId + " and VesselId=" + vesselId, ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) return dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                if (voyageId > 0)
                {
                    using (var adp = new SqlDataAdapter(
                        "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + voyageId + " and VesselId=" + vesselId + " and IsActive=1", ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) return dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        private static string ApplyTemplate(string template, LoadingReport r, string vesselName, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
        {
            string voyNo = "";
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            if (string.IsNullOrEmpty(legText)) legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Vessel", r.VesselName ?? vesselName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Time", r.Times)).Append(KvRow("Rate", r.Rate));
            sb.Append(KvRow("Hose Connection", r.Hose_Connection)).Append(KvRow("High H2S", r.High_H2S));
            string lopRows = sb.ToString();
            sb.Clear();

            string cargoRows = BuildCargoRowsHtml(dtCargo);
            string stoppageRows = BuildStoppageRowsHtml(dtStoppage);
            string pumpsRows = BuildPumpsRowsHtml(dtPumpsUse);
            string remarksRow = @"<tr><td colspan=""8"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + (r.Remarks ?? "-") + @"</td></tr>";

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

        private static string BuildHtmlInline(LoadingReport r, string vesselName, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
        {
            string voyNo = "";
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            if (string.IsNullOrEmpty(legText)) legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Loading Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.ReportDateTime)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Loading Report (").Append(V(vesselName)).Append(@")</td></tr>");

            // General Info
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName)).Append(KvRow("Vessel", r.VesselName ?? vesselName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");

            // Cargo Details — column names match the web edit form exactly. Order: 14 columns.
            // Rendered BEFORE Letter of Protests per stakeholder request.
            const string cargoTh = "padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;vertical-align:middle;";
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Details</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;"">");
            sb.Append(@"<tr>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Cargo Grades</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Commence Loading Date &amp; Time</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Terminal Loading Rate (m3/hr)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Loading Rate Accepted by Vessel (m3/hr)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Average Achieved Loading Rate (m3/hr)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">No of Manifold / Hoses by Terminal</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Size of Manifold / Hoses by Terminal (Inches)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">No of Manifold / Hoses by Vessel</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Size of Manifold / Hoses by Vessel (Inches)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Shore Line Distance (mtrs)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Quantity onboard (MT)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Balance Quantity to be Loaded (MT)</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">ETC Comp Date &amp; Time</td>")
              .Append(@"<td style=""").Append(cargoTh).Append(@""">Actual Comp Date &amp; Time</td>")
              .Append(@"</tr>");
            if (dtCargo != null)
            {
                foreach (DataRow dr in dtCargo.Rows)
                {
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "CargoName"))
                      .Append(TdDt(dr, "LoadingDatetime"))
                      .Append(TdDec(dr, "TerminalLoadingRate"))
                      .Append(TdDec(dr, "LoadingRateAccepted"))
                      .Append(TdDec(dr, "AverageAchievedLoadingRate"))
                      .Append(TdDec(dr, "No_Manifold_Hoses_by_Terminal"))
                      .Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Terminal"))
                      .Append(TdDec(dr, "No_Manifold_Hoses_by_Vessel"))
                      .Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Vessel"))
                      .Append(TdDec(dr, "ShoreLineDistance"))
                      .Append(TdDec(dr, "QuantityOnboard"))
                      .Append(TdDec(dr, "BalanceQuantityLoaded"))
                      .Append(TdDt(dr, "EstCompDateTime"))
                      .Append(TdDt(dr, "ActualCompDateTime"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // LOP — rendered AFTER Cargo Details.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Letter of Protests</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Time", r.Times)).Append(KvRow("Rate", r.Rate));
            sb.Append(KvRow("Hose Connection", r.Hose_Connection)).Append(KvRow("High H2S", r.High_H2S));
            sb.Append(@"</table></td></tr>");

            // Stoppage Details — headers match the web edit form exactly.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Stoppage Details</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Stoppage Reason</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Date Time From</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Date Time To</td></tr>");
            if (dtStoppage != null)
            {
                foreach (DataRow dr in dtStoppage.Rows)
                {
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "Reason")).Append(TdDt(dr, "DateTimeFrom")).Append(TdDt(dr, "DateTimeTo"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Ballast Pump Use
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast Pump Use</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Pump Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Rate</td></tr>");
            if (dtPumpsUse != null)
            {
                // Dedupe by PumpName so duplicates in lr_dcr_pumpsuse or tblPump don't render the same pump multiple times.
                var seenPumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow dr in dtPumpsUse.Rows)
                {
                    string pumpName = (dr.Table.Columns.Contains("PumpName") ? dr["PumpName"]?.ToString() : "")?.Trim() ?? "";
                    if (!seenPumps.Add(pumpName)) continue;
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "PumpName")).Append(TdDec(dr, "Rate"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Remarks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");

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
                sb.Append(Td(dr, "CargoName"))
                  .Append(TdDt(dr, "LoadingDatetime"))
                  .Append(TdDec(dr, "TerminalLoadingRate"))
                  .Append(TdDec(dr, "LoadingRateAccepted"))
                  .Append(TdDec(dr, "AverageAchievedLoadingRate"))
                  .Append(TdDec(dr, "No_Manifold_Hoses_by_Terminal"))
                  .Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Terminal"))
                  .Append(TdDec(dr, "No_Manifold_Hoses_by_Vessel"))
                  .Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Vessel"))
                  .Append(TdDec(dr, "ShoreLineDistance"))
                  .Append(TdDec(dr, "QuantityOnboard"))
                  .Append(TdDec(dr, "BalanceQuantityLoaded"))
                  .Append(TdDt(dr, "EstCompDateTime"))
                  .Append(TdDt(dr, "ActualCompDateTime"));
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
                sb.Append(Td(dr, "Reason")).Append(TdDt(dr, "DateTimeFrom")).Append(TdDt(dr, "DateTimeTo"));
                sb.Append(@"</tr>");
            }
            return sb.ToString();
        }

        private static string BuildPumpsRowsHtml(DataTable dtPumpsUse)
        {
            if (dtPumpsUse == null || dtPumpsUse.Rows.Count == 0) return "";
            var sb = new StringBuilder();
            // Dedupe by PumpName so duplicates in lr_dcr_pumpsuse or tblPump don't render multiple rows.
            var seenPumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow dr in dtPumpsUse.Rows)
            {
                string pumpName = (dr.Table.Columns.Contains("PumpName") ? dr["PumpName"]?.ToString() : "")?.Trim() ?? "";
                if (!seenPumps.Add(pumpName)) continue;
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
