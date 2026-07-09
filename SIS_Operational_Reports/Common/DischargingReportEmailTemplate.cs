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
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/DischargingReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(int? i) => i.HasValue ? i.Value.ToString() : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <summary>Always-3-decimal formatter for Draft cells (e.g. 6 → "6.000") per webpage.</summary>
        private static string V3(decimal? d) => d.HasValue ? d.Value.ToString("0.000") : "-";

        // VoyageLeg.Id is not unique across vessels/voyages — must scope by VoyageId + VesselId.
        // Same lookup pattern as LoadingReportEmailTemplate; ensures Leg value resolves to
        // "LegPort_A to LegPort_B" (e.g. "Ruwais to Jebel Ali") when the sync SP doesn't carry it.
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
            // New per webpage: Ballast Pumps in Use (PumpUseId=2) and Cargo Pumps in Use (PumpUseId=1)
            // sourced from LR_DCR_PumpsUse joined to tblPump, matching DischargingController.GetPumpsINUse/GetPumpsINUse1.
            DataTable dtBallastPumps = new DataTable(), dtCargoPumpsInUse = new DataTable();

            try
            {
                // DS_Cargo FK to DischargingReport is `LRId` (per spInsertDischargeingCargoList SP
                // and DischargingController.GetDischargeCargo query). The previous filter `DSId=`
                // didn't match any rows so dtCargo was empty and every cargo cell rendered as "-".
                //
                // VoyageId filter added to match the web form's controller (GetDischargeCargosEdit
                // at DischargingController line 627). Without it, the email was picking up stale
                // DS_Cargo rows that shared LRId+VesselId but belonged to a different voyage —
                // observed: 5 rows in email vs 3 on the form, because 2 leftover OFF KARWAR rows
                // from an earlier voyage were being included.
                //
                // Dedupe by CargoName + PortName (taking MAX(Id)) eliminates same-voyage double-
                // submit duplicates — same pattern as SaveLoadingReportExcelToFiles in
                // getAttachment.aspx.cs which handles LR_Cargo duplicates the same way.
                string cargoQuery = @"
SELECT a.*
FROM DS_Cargo a
INNER JOIN (
    SELECT CargoName, PortName, MAX(Id) AS MaxId
    FROM DS_Cargo
    WHERE LRId = " + id + @" AND VesselId = " + vesselId + @" AND VoyageId = " + disRBind.VoyageId + @"
    GROUP BY CargoName, PortName
) g ON a.Id = g.MaxId
WHERE a.LRId = " + id + @" AND a.VesselId = " + vesselId + @"
ORDER BY a.Id";
                using (var adp = new SqlDataAdapter(cargoQuery, ConnectionBulder.con))
                    adp.Fill(dtCargo);
                using (var adp = new SqlDataAdapter("select * from LR_Stoppage where DCId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=1", ConnectionBulder.con))
                    adp.Fill(dtStoppage);
                // tblPump is singular — earlier code used "tblPumps" (typo). On SQL Servers where
                // `tblPumps` doesn't exist the query throws and shorts the try block, so the
                // ballast/cargo pump fetches below were silently skipped and the email rendered
                // empty rows. Use a LEFT JOIN so pumps without a matching tblPump row still appear.
                using (var adp = new SqlDataAdapter("select a.*, b.Name as PumpName from LR_DCR_PumpsUse a left join tblPump b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                    adp.Fill(dtPumpsUse);
                // Ballast pumps in use (PumpUseId=2) — INNER JOIN tblPump on PumpId AND PumpUseId
                // so a pump can only appear in Ballast if tblPump itself classifies it as ballast.
                // Mirrors DischargingController.GetPumpsINUse exactly; prevents COP entries that
                // were mistakenly saved with PumpUseId=2 from leaking into this section. Inner
                // dedup subquery keeps one row per pump Name, preferring the row with a NON-ZERO Rate:
                // LR_DCR_PumpsUse accumulates duplicate rows on re-save (a real-rate row plus a later
                // 0.000 row at a higher Id), so the old max(Id) picked the 0.000 duplicate and the
                // email showed Rate 0.000 for pumps that actually have a rate. ROW_NUMBER prefers a
                // non-zero Rate, then the latest Id.
                using (var adp = new SqlDataAdapter(
                    "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                    "inner join (select aa.Id, ROW_NUMBER() over (partition by bb.Name order by (case when aa.Rate <> 0 then 1 else 0 end) desc, aa.Id desc) as rn from LR_DCR_PumpsUse aa inner join tblPump bb on aa.PumpId=bb.Id and aa.PumpUseId=bb.PumpUseId where aa.DCRId=" + id + " and aa.VesselId=" + vesselId + " and aa.PumpUseId=2) g on a.Id=g.Id and g.rn=1 " +
                    "inner join tblPump b on a.PumpId=b.Id and a.PumpUseId=b.PumpUseId " +
                    "where a.DCRId=" + id + " and a.VesselId=" + vesselId + " and a.PumpUseId=2", ConnectionBulder.con))
                    adp.Fill(dtBallastPumps);
                // Cargo pumps in use (PumpUseId=1) — same strict-join pattern so a ballast pump
                // (e.g. B/P# 1) mis-saved with PumpUseId=1 won't appear in the Cargo Pumps pivot.
                using (var adp = new SqlDataAdapter(
                    "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                    "inner join (select aa.Id, ROW_NUMBER() over (partition by bb.Name order by (case when aa.Rate <> 0 then 1 else 0 end) desc, aa.Id desc) as rn from LR_DCR_PumpsUse aa inner join tblPump bb on aa.PumpId=bb.Id and aa.PumpUseId=bb.PumpUseId where aa.DCRId=" + id + " and aa.VesselId=" + vesselId + " and aa.PumpUseId=1) g on a.Id=g.Id and g.rn=1 " +
                    "inner join tblPump b on a.PumpId=b.Id and a.PumpUseId=b.PumpUseId " +
                    "where a.DCRId=" + id + " and a.VesselId=" + vesselId + " and a.PumpUseId=1", ConnectionBulder.con))
                    adp.Fill(dtCargoPumpsInUse);
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

            // Always use the inline HTML builder. Mirrors the Berthing fix: avoids stale
            // Templates/DischargingReport.html copies leaving literal {{...}} placeholders
            // in the rendered email when the deployed template file is out of sync.
            return BuildHtmlInline(disRBind, vesselName, dtCargo, dtStoppage, dtPumpsUse, dtBallastPumps, dtCargoPumpsInUse, dtMain);
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

            if (string.IsNullOrEmpty(legText)) legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd", V3(r.DraftFwd))).Append(KvRow("Draft Mid", V3(r.DraftMid))).Append(KvRow("Draft Aft", V3(r.DraftAft)));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Time", r.Times)).Append(KvRow("Rate", r.Rate));
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

        private static string BuildHtmlInline(DischargingReport r, string vesselName, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtBallastPumps, DataTable dtCargoPumpsInUse, DataTable dtMain)
        {
            string voyNo = null;
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString();
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
            }
            // Fallback: resolve display VoyageNumber from the Voyage table by VoyageId so the
            // "Voy No." cell shows the user-facing number (e.g. 61) rather than the FK id (e.g. 14).
            if (string.IsNullOrWhiteSpace(voyNo) && r.VoyageId > 0)
            {
                try
                {
                    var voyages = CommonMethods.GetVoyageList(r.VesselId);
                    var match = voyages?.FirstOrDefault(v => v.Id == r.VoyageId);
                    if (match != null && !string.IsNullOrWhiteSpace(match.VoyageNumber))
                        voyNo = match.VoyageNumber.Trim();
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = r.VoyageId.ToString();
            if (string.IsNullOrEmpty(legText)) legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Discharging Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(Vdt(r.ReportDateTime)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            // Outer table mirrors the Loading Report email — fixed 1200px width keeps the 15-column
            // Cargo Details legible (~80px/col) and leaves the table left-aligned with the body text
            // (no margin:0 auto so Gmail anchors it to the left like the Loading email).
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Discharging Report (").Append(V(vesselName)).Append(@")</td></tr>");

            // Section banner + cargo header styles copied from LoadingReportEmailTemplate so both emails render identically.
            const string CARGO_TH = "padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;vertical-align:middle;";

            // General Info
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Port", r.PortName));
            sb.Append(KvRow("Leg", legText));
            sb.Append(KvRow("Report Date & Time", Vdt(r.ReportDateTime))).Append(KvRow("ETD Date & Time", Vdt(r.ETDDateTime)));
            sb.Append(KvRow("Draft Fwd", V3(r.DraftFwd))).Append(KvRow("Draft Mid", V3(r.DraftMid))).Append(KvRow("Draft Aft", V3(r.DraftAft)));
            sb.Append(@"</table></td></tr>");

            // Cargo Details — single 15-column table; table-layout:fixed gives every column an equal
            // share (~80px in the 1200px container) so wrapping is natural and never truncates.
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Details</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;"">");
            sb.Append(@"<tr>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Cargo Grades</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Commence Discharge Date &amp; Time</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Terminal Acceptable Discharging Rate</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Discharging Pressure Requested</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Average Discharge Rate By Vessel</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Average Discharge pressure By Vessel</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">No of Pumps in use</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">No of Manifold / Hoses by Terminal</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Size of Manifold / Hoses by Terminal (Inches)</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">No of Manifold / Hoses by Vessel</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Size of Manifold / Hoses by Vessel (Inches)</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Total Cargo Discharged</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Balance Cargo to be Discharged</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">ETC Comp Date &amp; Time</td>")
              .Append(@"<td style=""").Append(CARGO_TH).Append(@""">Actual Comp Date &amp; Time</td>")
              .Append(@"</tr>");
            if (dtCargo != null)
            {
                foreach (DataRow dr in dtCargo.Rows)
                {
                    sb.Append(@"<tr>");
                    // Cargo Grades — webpage shows "CargoName ( PortName )" (e.g. "ATF ( Jebel Ali )").
                    // DS_Cargo has both CargoName and PortName columns directly (per
                    // spInsertDischargeingCargoList parameters), so no JOIN needed.
                    string cName = dr.Table.Columns.Contains("CargoName") && dr["CargoName"] != DBNull.Value ? dr["CargoName"].ToString().Trim() : "";
                    string pName = dr.Table.Columns.Contains("PortName") && dr["PortName"] != DBNull.Value ? dr["PortName"].ToString().Trim() : "";
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "-" : cName + (string.IsNullOrEmpty(pName) ? "" : " ( " + pName + " )");
                    sb.Append(@"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(cargoLabel).Append(@"</td>");
                    sb.Append(TdDtSec(dr, "DischargeDatetime"));
                    sb.Append(TdDec(dr, "Terminal_Acceptable_Discharging_Rate")).Append(TdDec(dr, "Discharging_pressure_Requested"));
                    sb.Append(TdDec(dr, "Average_Discharge_Rate_ByVessel")).Append(TdDec(dr, "Average_Discharge_pressure_ByVessel"));
                    sb.Append(Td(dr, "No_of_Pumps_Use"));
                    sb.Append(Td(dr, "No_Manifold_Hoses_by_Terminal")).Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Terminal"));
                    sb.Append(Td(dr, "No_Manifold_Hoses_by_Vessel")).Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Vessel"));
                    // Total Cargo Discharged uses TdDecTrim to strip trailing zeros (40582.400 → "40582.4").
                    sb.Append(TdDecTrim(dr, "Total_CargoDischarged")).Append(TdDec(dr, "Balance_Cargo_ToBe_Deischarged"));
                    sb.Append(TdDtSec(dr, "EstCompDateTime")).Append(TdDtSec(dr, "ActualCompDateTime"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Letter of Protests
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Letter of Protests</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Time", r.Times)).Append(KvRow("Rate", r.Rate));
            sb.Append(KvRow("Hose Connection", r.Hose_Connection)).Append(KvRow("High H2S", r.High_H2S));
            sb.Append(@"</table></td></tr>");

            // Stoppage Reason
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Stoppage Reason</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Stoppage Reason</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Date Time From</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Date Time To</td></tr>");
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

            // Ballast Pumps in Use — column-header layout (Name | Rate) with one row per pump.
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast Pumps in Use</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Rate</td></tr>");
            if (dtBallastPumps != null)
            {
                // Dedupe by PumpName so duplicates in lr_dcr_pumpsuse or tblPump don't render the same pump multiple times.
                // Identical pattern to LoadingReportEmailTemplate's Ballast Pump Use loop.
                var seenBallastPumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow dr in dtBallastPumps.Rows)
                {
                    string pumpName = (dr.Table.Columns.Contains("PumpName") ? dr["PumpName"]?.ToString() : "")?.Trim() ?? "";
                    if (!seenBallastPumps.Add(pumpName)) continue;
                    sb.Append(@"<tr>");
                    sb.Append(Td(dr, "PumpName")).Append(TdDec3(dr, "Rate"));
                    sb.Append(@"</tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Cargo Pumps in Use — pivoted: pump names (COP#1, COP#2, ...) span column headers,
            // then a "Name" row repeats the names and a "Rate" row shows each pump's rate.
            // Dedupe rows once (same pattern as Ballast Pump loop above) so the pivot doesn't
            // explode into 15 corrupted-looking columns when LR_DCR_PumpsUse has repeated rows.
            var cargoPumps = new List<DataRow>();
            if (dtCargoPumpsInUse != null)
            {
                var seenCargoPumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow dr in dtCargoPumpsInUse.Rows)
                {
                    string pumpName = (dr.Table.Columns.Contains("PumpName") ? dr["PumpName"]?.ToString() : "")?.Trim() ?? "";
                    if (!seenCargoPumps.Add(pumpName)) continue;
                    cargoPumps.Add(dr);
                }
            }
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Pumps in Use</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;"">");
            // Column-header row — pump names as column headers
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td>");
            foreach (DataRow dr in cargoPumps)
                sb.Append(@"<td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">").Append(dr["PumpName"]?.ToString() ?? "-").Append(@"</td>");
            sb.Append(@"</tr>");
            // Name row — pump names rendered as data cells
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Name</td>");
            foreach (DataRow dr in cargoPumps)
                sb.Append(@"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:center;"">").Append(dr["PumpName"]?.ToString() ?? "-").Append(@"</td>");
            sb.Append(@"</tr>");
            // Rate row — TdDec3 forces 3 decimals (e.g. 750 → "750.000") per webpage format.
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Rate</td>");
            foreach (DataRow dr in cargoPumps)
                sb.Append(TdDec3(dr, "Rate"));
            sb.Append(@"</tr>");
            sb.Append(@"</table></td></tr>");

            // Power Packs — two key-value rows sourced from DischargingReport model.
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Power Packs</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("No of Power Packs onboard", V(r.Power_Packs_onboard)));
            sb.Append(KvRow("No of Power Packs used", V(r.Power_Packs_Used)));
            sb.Append(@"</table></td></tr>");

            // Discharge Report Remarks — single "Remarks" key-value row inside the section.
            sb.Append(@"<tr><td colspan=""15"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Discharge Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""15"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Remarks", r.Remarks ?? "-"));
            sb.Append(@"</table></td></tr>");

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
                sb.Append(Td(dr, "No_Manifold_Hoses_by_Terminal")).Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Terminal"));
                sb.Append(Td(dr, "No_Manifold_Hoses_by_Vessel")).Append(TdDec(dr, "Size_of_Manifold_Hoses_by_Vessel"));
                sb.Append(TdDec(dr, "Total_CargoDischarged")).Append(TdDec(dr, "Balance_Cargo_ToBe_Deischarged"));
                sb.Append(TdDt(dr, "EstCompDateTime")).Append(TdDt(dr, "ActualCompDateTime"));
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
                sb.Append(@"<td style=""padding:3px 2px;border:1px solid #ccc;vertical-align:middle;font-size:10px;word-wrap:break-word;overflow-wrap:break-word;"">").Append(reason ?? "-").Append(@"</td>");
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

        // Cell styles match Loading Report email — same outer table width (1200px) and cell padding
        // so the 15-column Cargo Details has room to render without horizontal scroll in Gmail's
        // preview pane while staying left-aligned with the rest of the body text.

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

        // TdDtSec — variant of TdDt that renders datetimes with seconds (yyyy-MM-dd HH:mm:ss).
        // Used only for the Cargo Details datetime columns to match the webpage precision.
        private static string TdDtSec(DataRow dr, string col)
        {
            string val = "-";
            if (dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value)
            {
                DateTime d;
                if (DateTime.TryParse(dr[col].ToString(), out d))
                    val = d.ToString("yyyy-MM-dd HH:mm:ss");
                else
                    val = dr[col].ToString();
            }
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">" + val + @"</td>";
        }

        // TdDec3 — always renders 3 decimal places (e.g. 1500 → "1500.000") for pump rates,
        // matching the Excel "0.000" format and the webpage's pump rate display.
        private static string TdDec3(DataRow dr, string col)
        {
            string val = "-";
            if (dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value)
            {
                decimal d;
                if (decimal.TryParse(dr[col].ToString(), out d))
                    val = d.ToString("0.000");
                else
                    val = dr[col].ToString();
            }
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + val + @"</td>";
        }

        // TdDecTrim — strips trailing zeros (e.g. 40582.400 → "40582.4", 40582.000 → "40582")
        // to match the webpage's significant-digits display for Total Cargo Discharged.
        private static string TdDecTrim(DataRow dr, string col)
        {
            string val = "-";
            if (dr.Table.Columns.Contains(col) && dr[col] != DBNull.Value)
            {
                decimal d;
                if (decimal.TryParse(dr[col].ToString(), out d))
                    val = d.ToString("0.###############");
                else
                    val = dr[col].ToString();
            }
            return @"<td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + val + @"</td>";
        }

        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }
    }
}
