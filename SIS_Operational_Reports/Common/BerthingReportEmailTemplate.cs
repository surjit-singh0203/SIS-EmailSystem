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
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.EOSP as SBE, a.FWE as RFA from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                if (dtBunker.Rows.Count == 0)
                {
                    DataTable dtFuelTypes = new DataTable();
                    using (var adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                        adp.Fill(dtFuelTypes);
                    using (var adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=5 order by FuelType_Id", ConnectionBulder.con))
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

            // Always use the inline HTML builder. The external Templates/BerthingReport.html
            // file shipped with older deployments still contains the legacy {{FUEL_CONS_ROWS}}
            // placeholder, which produces a literal placeholder in the rendered email when the
            // file path resolves to a stale copy. The inline builder embeds the Fuel Consumption
            // MT table directly (sourced from BerthingReport model fields), removing all template
            // ambiguity and guaranteeing the section renders.
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

        /// <summary>
        /// Resolves the display VoyageNumber (e.g. "61") from the Voyage table PK
        /// using the same GetVoyageList method that the web form uses.
        /// Returns null when lookup fails so callers can keep their existing fallback.
        /// </summary>
        private static string LookupVoyageNumber(int voyageId, int vesselId)
        {
            if (voyageId <= 0) return null;
            try
            {
                var voyages = CommonMethods.GetVoyageList(vesselId);
                if (voyages != null)
                {
                    var match = voyages.FirstOrDefault(v => v.Id == voyageId);
                    if (match != null && !string.IsNullOrWhiteSpace(match.VoyageNumber))
                        return match.VoyageNumber.Trim();
                }
            }
            catch { }
            return null;
        }

        private static string ApplyTemplate(string template, BerthingReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber;
            string legText = "";
            string portStatusText = r.PortStatus.ToString();
            string facilityName = r.FacilityName ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrEmpty(r.voyagenumber) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("PortStatusName"))
                {
                    string psName = dr["PortStatusName"]?.ToString();
                    if (!string.IsNullOrEmpty(psName)) portStatusText = psName;
                }
                if (string.IsNullOrEmpty(facilityName) && dtMain.Columns.Contains("FacilityName"))
                    facilityName = dr["FacilityName"]?.ToString() ?? "";
            }
            // Fallback: look up the display VoyageNumber from the Voyage table
            if (string.IsNullOrWhiteSpace(voyNo))
                voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();

            // Look up port status text from DB if still numeric
            if (int.TryParse(portStatusText, out int psId) && psId > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter("select Status from PortStatusMaster where Id=" + psId, ConnectionBulder.con))
                    {
                        DataTable dtPs = new DataTable();
                        adp.Fill(dtPs);
                        if (dtPs.Rows.Count > 0) portStatusText = dtPs.Rows[0]["Status"]?.ToString() ?? portStatusText;
                    }
                }
                catch
                {
                    try
                    {
                        DataTable dtPsBind = new DataTable();
                        using (var adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
                        {
                            adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                            adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                            adp.Fill(dtPsBind);
                        }
                        foreach (DataRow psr in dtPsBind.Rows)
                        {
                            if (Convert.ToInt32(psr["Id"]) == psId)
                            {
                                portStatusText = psr["Status"]?.ToString() ?? portStatusText;
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            // Look up leg by LegPortId (specific leg linked to this report)
            if (r.LegPortId > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter(
                        "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId, ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                catch { }
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
            sb.Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("FWE Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
            sb.Append(KvRow("SBE ROB", r.SBE_ROB)).Append(KvRow("FWE ROB", r.RFA_ROB));
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

            // Lub Oil rows kept for template backward compat
            sb.Append(KvRow("ME Crankcase (Ltrs)", V(r.LO_HO_Cons_MECC) + " / " + V(r.LO_HO_Cons_MECC_ROB)));
            sb.Append(KvRow("ME Cylinder (Ltrs)", V(r.LO_HO_Cons_MECYL) + " / " + V(r.LO_HO_Cons_MECYL_ROB)));
            sb.Append(KvRow("AE Crankcase (Ltrs)", V(r.LO_HO_Cons_AECC) + " / " + V(r.LO_HO_Cons_AECC_ROB)));
            sb.Append(KvRow("Hydraulic Oil (Ltrs)", V(r.LO_HO_Cons_HYDR_Oil) + " / " + V(r.LO_HO_Cons_HYDR_Oil_ROB)));
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

            // Build bunker rows – always show VLSFO and MDO even when DB returns no rows
            {
                var bunkerDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows)
                {
                    string ft = dr["FuelType"]?.ToString() ?? "";
                    string rcpt = dr["Receipt"]?.ToString() ?? "0";
                    if (!string.IsNullOrEmpty(ft)) bunkerDict[ft] = rcpt;
                }
                string[] requiredFuels = { "VLSFO", "MDO" };
                foreach (var ft in requiredFuels)
                {
                    string val = bunkerDict.ContainsKey(ft) ? bunkerDict[ft] : "0.000";
                    sb.Append(KvRow(ft, val));
                }
                foreach (var kvp in bunkerDict)
                {
                    if (!kvp.Key.Equals("VLSFO", StringComparison.OrdinalIgnoreCase) && !kvp.Key.Equals("MDO", StringComparison.OrdinalIgnoreCase))
                        sb.Append(KvRow(kvp.Key, kvp.Value));
                }
            }
            string bunkerRows = sb.ToString();
            sb.Clear();

            // Build full fuel consumption table
            decimal vlsfoTotal = GetFuelConsByType(dtFuelCons, "VLSFO"), mdoTotal = GetFuelConsByType(dtFuelCons, "MDO");
            string fuelConsFullTable = BuildFuelConsFullTable(r, vlsfoTotal, mdoTotal, dtFuelCons);
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
                .Replace("{{FUEL_CONS_FULL_TABLE}}", fuelConsFullTable)
                // Legacy placeholder name from older deployed BerthingReport.html files —
                // replace with the same fuel consumption table so old templates also render correctly.
                .Replace("{{FUEL_CONS_ROWS}}", fuelConsFullTable)
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
                .Replace("{{LO_MECC_CONS}}", V(r.LO_HO_Cons_MECC))
                .Replace("{{LO_MECC_ROB}}", V(r.LO_HO_Cons_MECC_ROB))
                .Replace("{{LO_MECYL_CONS}}", V(r.LO_HO_Cons_MECYL))
                .Replace("{{LO_MECYL_ROB}}", V(r.LO_HO_Cons_MECYL_ROB))
                .Replace("{{LO_AECC_CONS}}", V(r.LO_HO_Cons_AECC))
                .Replace("{{LO_AECC_ROB}}", V(r.LO_HO_Cons_AECC_ROB))
                .Replace("{{LO_HYDR_CONS}}", V(r.LO_HO_Cons_HYDR_Oil))
                .Replace("{{LO_HYDR_ROB}}", V(r.LO_HO_Cons_HYDR_Oil_ROB))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(BerthingReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            string voyNo = r.voyagenumber;
            string legText = "";
            string portStatusText = r.PortStatus.ToString();
            string facilityName = r.FacilityName ?? "";
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrEmpty(r.voyagenumber) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("PortStatusName"))
                {
                    string psName = dr["PortStatusName"]?.ToString();
                    if (!string.IsNullOrEmpty(psName)) portStatusText = psName;
                }
                if (string.IsNullOrEmpty(facilityName) && dtMain.Columns.Contains("FacilityName"))
                    facilityName = dr["FacilityName"]?.ToString() ?? "";
            }
            // Fallback: look up the display VoyageNumber from the Voyage table
            if (string.IsNullOrWhiteSpace(voyNo))
                voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();

            // Look up port status text from DB if still numeric
            if (int.TryParse(portStatusText, out int psId2) && psId2 > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter("select Status from PortStatusMaster where Id=" + psId2, ConnectionBulder.con))
                    {
                        DataTable dtPs = new DataTable();
                        adp.Fill(dtPs);
                        if (dtPs.Rows.Count > 0) portStatusText = dtPs.Rows[0]["Status"]?.ToString() ?? portStatusText;
                    }
                }
                catch
                {
                    try
                    {
                        DataTable dtPsBind = new DataTable();
                        using (var adp = new SqlDataAdapter("spCommonBinding", ConnectionBulder.con))
                        {
                            adp.SelectCommand.CommandType = CommandType.StoredProcedure;
                            adp.SelectCommand.Parameters.AddWithValue("@Action", "PortStatus");
                            adp.Fill(dtPsBind);
                        }
                        foreach (DataRow psr in dtPsBind.Rows)
                        {
                            if (Convert.ToInt32(psr["Id"]) == psId2)
                            {
                                portStatusText = psr["Status"]?.ToString() ?? portStatusText;
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            // Look up leg by LegPortId (specific leg linked to this report)
            if (r.LegPortId > 0)
            {
                try
                {
                    using (var adp = new SqlDataAdapter(
                        "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId, ConnectionBulder.con))
                    {
                        DataTable dtLeg = new DataTable();
                        adp.Fill(dtLeg);
                        if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                catch { }
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
            sb.Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("FWE Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
            sb.Append(KvRow("SBE ROB", r.SBE_ROB)).Append(KvRow("FWE ROB", r.RFA_ROB));
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

            // Lub Oil / Hydraulic Oil
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Lub Oil / Hydraulic Oil</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:30%;min-width:150px""><col style=""width:30%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Consumption</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">ROB</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ME Crankcase (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ME Cylinder (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">AE Crankcase (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Hydraulic Oil (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil_ROB)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Fuel ROB
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT (SBE/FWE)</td></tr>");
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
            {
                var bunkerDict2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows)
                {
                    string ft = dr["FuelType"]?.ToString() ?? "";
                    string rcpt = dr["Receipt"]?.ToString() ?? "0";
                    if (!string.IsNullOrEmpty(ft)) bunkerDict2[ft] = rcpt;
                }
                string[] requiredFuels2 = { "VLSFO", "MDO" };
                foreach (var ft in requiredFuels2)
                {
                    string val = bunkerDict2.ContainsKey(ft) ? bunkerDict2[ft] : "0.000";
                    sb.Append(KvRow(ft, val));
                }
                foreach (var kvp in bunkerDict2)
                {
                    if (!kvp.Key.Equals("VLSFO", StringComparison.OrdinalIgnoreCase) && !kvp.Key.Equals("MDO", StringComparison.OrdinalIgnoreCase))
                        sb.Append(KvRow(kvp.Key, kvp.Value));
                }
            }
            sb.Append(@"</table></td></tr>");

            // Fuel Consumption full table
            {
                decimal vlsfoTot = GetFuelConsByType(dtFuelCons, "VLSFO"), mdoTot = GetFuelConsByType(dtFuelCons, "MDO");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Consumption in MT</td></tr>");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;"">").Append(BuildFuelConsFullTable(r, vlsfoTot, mdoTot, dtFuelCons)).Append(@"</td></tr>");
            }

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

        private static decimal GetFuelConsByType(DataTable dt, string fuelType)
        {
            if (dt == null) return 0;
            decimal total = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase))
                {
                    if (dr["Value"] != null && dr["Value"] != DBNull.Value && decimal.TryParse(dr["Value"].ToString(), out decimal d))
                        total += d;
                }
            }
            return total;
        }

        /// <summary>Returns the decimal value at a given index within a fuel type's rows from dtFuelCons, or 0 if not found.</summary>
        private static decimal GetFuelConsAtIndex(DataTable dt, string fuelType, int index)
        {
            if (dt == null) return 0;
            int count = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase))
                {
                    if (count == index)
                    {
                        var v = dr["Value"];
                        if (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out decimal d)) return d;
                        return 0;
                    }
                    count++;
                }
            }
            return 0;
        }

        private static string FuelConsSection(string sectionName, (string fuelType, decimal sea, decimal man, decimal wait, decimal berth, decimal subTotal)[] rows, bool showSubTotal = true)
        {
            string B = "border:1px solid #999;";
            string P = "padding:5px 6px;";
            string FS = "font-size:10px;";
            string cell = B + P + FS + "text-align:center;";
            string label = B + P + FS + "font-weight:bold;background:#f5f5f5;white-space:nowrap;";
            string hdr = B + P + FS + "font-weight:bold;background:#f5f5f5;text-align:center;white-space:nowrap;";
            int totalCols = showSubTotal ? 6 : 5;
            var s = new StringBuilder();
            s.Append("<tr><td colspan=\"").Append(totalCols).Append("\" style=\"").Append(B).Append(P).Append("text-align:center;font-size:11px;font-weight:normal;\">").Append(sectionName).Append("</td></tr>");
            s.Append("<tr><td style=\"").Append(cell).Append("\"></td><td colspan=\"").Append(totalCols - 1).Append("\" style=\"").Append(cell).Append("\">ACT</td></tr>");
            s.Append("<tr><td style=\"").Append(hdr).Append("\"></td>");
            s.Append("<td style=\"").Append(hdr).Append("\">AT SEA</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">MANOEUV</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">ANCHOR/<br>WAIT</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">BERTH</td>");
            if (showSubTotal) s.Append("<td style=\"").Append(hdr).Append("\">SUB<br>TOTAL</td>");
            s.Append("</tr>");
            foreach (var row in rows)
            {
                s.Append("<tr><td style=\"").Append(label).Append("\">").Append(row.fuelType).Append("</td>");
                s.Append("<td style=\"").Append(cell).Append("\">").Append(row.sea.ToString("0.000")).Append("</td>");
                s.Append("<td style=\"").Append(cell).Append("\">").Append(row.man.ToString("0.000")).Append("</td>");
                s.Append("<td style=\"").Append(cell).Append("\">").Append(row.wait.ToString("0.000")).Append("</td>");
                s.Append("<td style=\"").Append(cell).Append("\">").Append(row.berth.ToString("0.000")).Append("</td>");
                if (showSubTotal) s.Append("<td style=\"").Append(cell).Append("\">").Append(row.subTotal.ToString("0.000")).Append("</td>");
                s.Append("</tr>");
            }
            return s.ToString();
        }

        private static string FuelConsIGGRow(decimal? iggVlsfo, decimal? iggMdo, decimal? incVlsfo, decimal? incMdo)
        {
            string B = "border:1px solid #999;";
            string P = "padding:5px 6px;";
            string F = "font-size:10px;";
            string lbl = B + P + F + "font-weight:bold;background:#f5f5f5;white-space:nowrap;";
            string hdr = B + P + F + "font-weight:bold;background:#f5f5f5;text-align:center;white-space:nowrap;";
            string val = B + P + F + "text-align:center;";
            var s = new StringBuilder();
            s.Append("<table style=\"width:100%;border-collapse:collapse;table-layout:fixed;font-size:10px;font-family:Arial,sans-serif;\">");
            s.Append("<col style=\"width:18%\"><col style=\"width:41%\"><col style=\"width:41%\">");
            s.Append("<tr>");
            s.Append("<td style=\"" + lbl + "\"></td>");
            s.Append("<td style=\"" + hdr + "\">IGG</td>");
            s.Append("<td style=\"" + hdr + "\">Incinerator</td>");
            s.Append("</tr>");
            s.Append("<tr>");
            s.Append("<td style=\"" + lbl + "\">VLSFO</td>");
            s.Append("<td style=\"" + val + "\">").Append(iggVlsfo.HasValue ? iggVlsfo.Value.ToString("0.000") : "0.000").Append("</td>");
            s.Append("<td style=\"" + val + "\">").Append(incVlsfo.HasValue ? incVlsfo.Value.ToString("0.000") : "0.000").Append("</td>");
            s.Append("</tr>");
            s.Append("<tr>");
            s.Append("<td style=\"" + lbl + "\">MDO</td>");
            s.Append("<td style=\"" + val + "\">").Append(iggMdo.HasValue ? iggMdo.Value.ToString("0.000") : "0.000").Append("</td>");
            s.Append("<td style=\"" + val + "\">").Append(incMdo.HasValue ? incMdo.Value.ToString("0.000") : "0.000").Append("</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        private static string FuelConsEventsSectionFromModel(BerthingReport r)
        {
            string B = "border:1px solid #999;";
            string P = "padding:5px 6px;";
            string FS = "font-size:10px;";
            string cell = B + P + FS + "text-align:center;";
            string label = B + P + FS + "font-weight:bold;background:#f5f5f5;white-space:nowrap;";
            string hdr = B + P + FS + "font-weight:bold;background:#f5f5f5;text-align:center;white-space:normal;word-wrap:break-word;";
            var s = new StringBuilder();
            s.Append("<table style=\"width:100%;border-collapse:collapse;table-layout:fixed;font-size:10px;font-family:Arial,sans-serif;\">");
            s.Append("<col style=\"width:12%\"><col style=\"width:11%\"><col style=\"width:10%\"><col style=\"width:11%\"><col style=\"width:10%\"><col style=\"width:9%\"><col style=\"width:12%\"><col style=\"width:13%\"><col style=\"width:12%\">");
            s.Append("<tr><td colspan=\"9\" style=\"" + B + P + "text-align:center;font-size:11px;font-weight:normal;\">Events</td></tr>");
            s.Append("<tr>");
            s.Append("<td style=\"").Append(hdr).Append("\"></td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Stoppage<br>at Sea</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Deviation</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Slow<br>Steaming</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Bad<br>Weather</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">COT<br>Prep</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Cargo<br>Heating</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">BW<br>Exchange</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Others</td>");
            s.Append("</tr>");
            // The Berthing model exposes single (non fuel-typed) event totals — render under VLSFO row, MDO row as zeros
            s.Append("<tr><td style=\"").Append(label).Append("\">VLSFO</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.StopageAtSea.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.Deviation.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.SlowSteaming.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">0.000</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.COTPrep.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.CargoHeating.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">").Append(r.BWExchange.ToString("0.000")).Append("</td>");
            s.Append("<td style=\"").Append(cell).Append("\">0.000</td>");
            s.Append("</tr>");
            s.Append("<tr><td style=\"").Append(label).Append("\">MDO</td>");
            for (int i = 0; i < 8; i++) s.Append("<td style=\"").Append(cell).Append("\">0.000</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        private static string FuelConsEventsSection(DataTable dtFuelCons)
        {
            string B = "border:1px solid #999;";
            string P = "padding:5px 6px;";
            string FS = "font-size:10px;";
            string cell = B + P + FS + "text-align:center;";
            string label = B + P + FS + "font-weight:bold;background:#f5f5f5;white-space:nowrap;";
            string hdr = B + P + FS + "font-weight:bold;background:#f5f5f5;text-align:center;white-space:normal;word-wrap:break-word;";
            // Event indices within each fuel type: 18-25
            // 18=StopageAtSea, 19=Deviation, 20=SlowSteaming, 21=BadWeather,
            // 22=COTPrep, 23=CargoHeating, 24=BWExchange, 25=Others
            var s = new StringBuilder();
            s.Append("<table style=\"width:100%;border-collapse:collapse;table-layout:fixed;font-size:10px;font-family:Arial,sans-serif;\">");
            s.Append("<col style=\"width:12%\"><col style=\"width:11%\"><col style=\"width:10%\"><col style=\"width:11%\"><col style=\"width:10%\"><col style=\"width:9%\"><col style=\"width:12%\"><col style=\"width:13%\"><col style=\"width:12%\">");
            s.Append("<tr><td colspan=\"9\" style=\"" + B + P + "text-align:center;font-size:11px;font-weight:normal;\">Events</td></tr>");
            s.Append("<tr>");
            s.Append("<td style=\"").Append(hdr).Append("\"></td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Stoppage<br>at Sea</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Deviation</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Slow<br>Steaming</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Bad<br>Weather</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">COT<br>Prep</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Cargo<br>Heating</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">BW<br>Exchange</td>");
            s.Append("<td style=\"").Append(hdr).Append("\">Others</td>");
            s.Append("</tr>");
            // VLSFO row — indices 18-25
            s.Append("<tr><td style=\"").Append(label).Append("\">VLSFO</td>");
            for (int i = 18; i <= 25; i++)
                s.Append("<td style=\"").Append(cell).Append("\">").Append(GetFuelConsAtIndex(dtFuelCons, "VLSFO", i).ToString("0.000")).Append("</td>");
            s.Append("</tr>");
            // MDO row — indices 18-25
            s.Append("<tr><td style=\"").Append(label).Append("\">MDO</td>");
            for (int i = 18; i <= 25; i++)
                s.Append("<td style=\"").Append(cell).Append("\">").Append(GetFuelConsAtIndex(dtFuelCons, "MDO", i).ToString("0.000")).Append("</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Builds the entire fuel consumption HTML table with all 6 sections.
        /// Reads breakdown values from dtFuelCons (ordered by FuelTypeId, ConsTypeId).
        /// Index mapping within each fuel type: 0=ME_SEA, 1=ME_MAN, 2=ME_WAIT, 3=ME_BERTH,
        /// 4=AE_SEA, 5=AE_MAN, 6=AE_WAIT, 7=AE_BERTH,
        /// 8=BLR_SEA, 9=BLR_MAN, 10=BLR_WAIT, 11=BLR_BERTH,
        /// 12=FRAMO_SEA, 13=FRAMO_MAN, 14=FRAMO_WAIT, 15=FRAMO_BERTH,
        /// 16=IGG, 17=Incinerator,
        /// 18=StopageAtSea, 19=Deviation, 20=SlowSteaming, 21=BadWeather,
        /// 22=COTPrep, 23=CargoHeating, 24=BWExchange, 25=Others
        /// </summary>
        private static string BuildFuelConsFullTable(BerthingReport r, decimal vlsfoTotal, decimal mdoTotal, DataTable dtFuelCons)
        {
            // The Berthing Edit/View page (Areas/Report/Views/Berthing/Index.cshtml) binds the
            // Fuel Consumption inputs directly to BerthingReport model fields
            // (ME_ACT_*_VLSFO/MGO, AE_ACT_*_*, BLR_ACT_*_*, FRAMO_ACT_*_*, IGG, Incinerator,
            // StopageAtSea/Deviation/SlowSteaming/COTPrep/CargoHeating/BWExchange).
            // Read from the same model fields so the email mirrors what users see on the form.
            // (MGO on the model corresponds to MDO in the email layout.)

            var fc = new StringBuilder();
            fc.Append("<table style=\"width:100%;border-collapse:collapse;table-layout:fixed;font-size:10px;font-family:Arial,sans-serif;\"><col style=\"width:18%\"><col style=\"width:15%\"><col style=\"width:15%\"><col style=\"width:15%\"><col style=\"width:15%\"><col style=\"width:15%\">");

            // 1. Main Engine (with SUB TOTAL)
            decimal meVS = r.ME_ACT_SEA_VLSFO, meVM = r.ME_ACT_MAN_VLSFO, meVW = r.ME_ACT_WAIT_VLSFO, meVB = r.ME_ACT_BERTH_VLSFO;
            decimal meDS = r.ME_ACT_SEA_MGO,   meDM = r.ME_ACT_MAN_MGO,   meDW = r.ME_ACT_WAIT_MGO,   meDB = r.ME_ACT_BERTH_MGO;
            fc.Append(FuelConsSection("Main Engine", new[] {
                ("VLSFO", meVS, meVM, meVW, meVB, meVS + meVM + meVW + meVB),
                ("MDO",   meDS, meDM, meDW, meDB, meDS + meDM + meDW + meDB)
            }, true));

            // 2. Aux Engine (with SUB TOTAL)
            decimal aeVS = r.AE_ACT_SEA_VLSFO, aeVM = r.AE_ACT_MAN_VLSFO, aeVW = r.AE_ACT_WAIT_VLSFO, aeVB = r.AE_ACT_BERTH_VLSFO;
            decimal aeDS = r.AE_ACT_SEA_MGO,   aeDM = r.AE_ACT_MAN_MGO,   aeDW = r.AE_ACT_WAIT_MGO,   aeDB = r.AE_ACT_BERTH_MGO;
            fc.Append(FuelConsSection("Aux Eng", new[] {
                ("VLSFO", aeVS, aeVM, aeVW, aeVB, aeVS + aeVM + aeVW + aeVB),
                ("MDO",   aeDS, aeDM, aeDW, aeDB, aeDS + aeDM + aeDW + aeDB)
            }, true));

            // 3. Boiler (no SUB TOTAL)
            fc.Append(FuelConsSection("Boiler", new[] {
                ("VLSFO", r.BLR_ACT_SEA_VLSFO, r.BLR_ACT_MAN_VLSFO, r.BLR_ACT_WAIT_VLSFO, r.BLR_ACT_BERTH_VLSFO, 0m),
                ("MDO",   r.BLR_ACT_SEA_MGO,   r.BLR_ACT_MAN_MGO,   r.BLR_ACT_WAIT_MGO,   r.BLR_ACT_BERTH_MGO,   0m)
            }, false));

            // 4. FRAMO System (no SUB TOTAL)
            fc.Append(FuelConsSection("FRAMO System", new[] {
                ("VLSFO", r.FRAMO_ACT_SEA_VLSFO, r.FRAMO_ACT_MAN_VLSFO, r.FRAMO_ACT_WAIT_VLSFO, r.FRAMO_ACT_BERTH_VLSFO, 0m),
                ("MDO",   r.FRAMO_ACT_SEA_MGO,   r.FRAMO_ACT_MAN_MGO,   r.FRAMO_ACT_WAIT_MGO,   r.FRAMO_ACT_BERTH_MGO,   0m)
            }, false));
            fc.Append("</table>");

            // IGG & Incinerator
            fc.Append(FuelConsIGGRow(r.IGG, 0m, r.Incinerator, 0m));

            // 5. Events — bound from model fields (same fields used on the Berthing edit form)
            fc.Append(FuelConsEventsSectionFromModel(r));

            // Recompute totals from model values so the TOTAL row reflects rendered cells
            vlsfoTotal = meVS + meVM + meVW + meVB
                       + aeVS + aeVM + aeVW + aeVB
                       + r.BLR_ACT_SEA_VLSFO + r.BLR_ACT_MAN_VLSFO + r.BLR_ACT_WAIT_VLSFO + r.BLR_ACT_BERTH_VLSFO
                       + r.FRAMO_ACT_SEA_VLSFO + r.FRAMO_ACT_MAN_VLSFO + r.FRAMO_ACT_WAIT_VLSFO + r.FRAMO_ACT_BERTH_VLSFO
                       + r.IGG + r.Incinerator
                       + r.StopageAtSea + r.Deviation + r.SlowSteaming + r.BWExchange + r.COTPrep + r.CargoHeating;
            mdoTotal = meDS + meDM + meDW + meDB
                     + aeDS + aeDM + aeDW + aeDB
                     + r.BLR_ACT_SEA_MGO + r.BLR_ACT_MAN_MGO + r.BLR_ACT_WAIT_MGO + r.BLR_ACT_BERTH_MGO
                     + r.FRAMO_ACT_SEA_MGO + r.FRAMO_ACT_MAN_MGO + r.FRAMO_ACT_WAIT_MGO + r.FRAMO_ACT_BERTH_MGO;

            // 6. TOTAL
            string tLabel = "border:1px solid #999;padding:5px 6px;font-size:10px;font-weight:bold;background:#f5f5f5;white-space:nowrap;";
            string tCell = "border:1px solid #999;padding:5px 6px;font-size:10px;text-align:center;";
            fc.Append("<table style=\"width:100%;border-collapse:collapse;table-layout:fixed;font-size:10px;font-family:Arial,sans-serif;\">");
            fc.Append("<col style=\"width:18%\"><col style=\"width:15%\"><col style=\"width:67%\">");
            fc.Append("<tr><td colspan=\"3\" style=\"border:1px solid #999;padding:5px 6px;font-size:11px;font-weight:normal;text-align:center;\">TOTAL</td></tr>");
            fc.Append("<tr><td style=\"").Append(tLabel).Append("\">VLSFO</td><td style=\"").Append(tCell).Append("\">").Append(vlsfoTotal.ToString("0.000")).Append("</td><td style=\"").Append(tCell).Append("\"></td></tr>");
            fc.Append("<tr><td style=\"").Append(tLabel).Append("\">MDO</td><td style=\"").Append(tCell).Append("\">").Append(mdoTotal.ToString("0.000")).Append("</td><td style=\"").Append(tCell).Append("\"></td></tr>");
            fc.Append("</table>");

            return fc.ToString();
        }

        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }
    }
}
