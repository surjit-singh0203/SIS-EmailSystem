using DataBuildingLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace SIS_Operational_Reports.Common
{
    /// <summary>
    /// Builds HTML email body for Arrival Report. Uses ArrivalReport.html template when available.
    /// </summary>
    public static class ArrivalReportEmailTemplate
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/ArrivalReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        /// <summary>Show the value exactly as stored in the DB, preserving the column's scale/trailing
        /// zeros (9.000 -> "9.000", 9.750 -> "9.750", 25.00 -> "25.00"). ArrivalReport decimals are
        /// mostly decimal(18,3) with a couple decimal(18,2); InvariantCulture reproduces each stored scale.</summary>
        private static string V(decimal? d) => d.HasValue ? d.Value.ToString(CultureInfo.InvariantCulture) : "-";

        /// <summary>Strip trailing zeros from a numeric value/string. Use this in tables where the
        /// web view shows the number as-entered (e.g. Fuel ROB EOSP/FWE: 745.200 → 745.2).
        /// Returns "-" for null/empty, raw trimmed string when the value isn't parseable as decimal.</summary>
        private static string Num(object o)
        {
            if (o == null || o == DBNull.Value) return "-";
            string s = o.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return "-";
            if (decimal.TryParse(s, out decimal d)) return d.ToString(CultureInfo.InvariantCulture);
            return s;
        }

        /// <summary>True when the Bunker Received DataTable has at least one row with a non-empty
        /// Receipt value. Used to hide the "Bunker Received in MT" section entirely when no
        /// bunker data was entered for this arrival in the web view.</summary>
        private static bool BunkerHasData(DataTable dtBunker)
        {
            if (dtBunker == null || dtBunker.Rows.Count == 0) return false;
            if (!dtBunker.Columns.Contains("Receipt")) return false;
            foreach (DataRow dr in dtBunker.Rows)
            {
                object v = dr["Receipt"];
                if (v == null || v == DBNull.Value) continue;
                string s = v.ToString().Trim();
                if (string.IsNullOrEmpty(s)) continue;
                // Treat unparseable text as data (rare). A parsed numeric 0 also counts as
                // entered data — the user typed something, even if it's zero.
                return true;
            }
            return false;
        }
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <summary>Format nautical Latitude/Longitude: "21,58.29 S" → "21° 58.29' S".
        /// Returns "-" if empty, the original trimmed string if it can't be parsed.</summary>
        private static string FormatLatLon(object o)
        {
            if (o == null || o == DBNull.Value) return "-";
            string s = o.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return "-";
            var m = System.Text.RegularExpressions.Regex.Match(s, @"^\s*(\d+)\s*,\s*([\d.]+)\s*([NSEWnsew])?\s*$");
            if (!m.Success) return s;
            string dir = m.Groups[3].Success ? (" " + m.Groups[3].Value.ToUpperInvariant()) : "";
            return m.Groups[1].Value + "° " + m.Groups[2].Value + "'" + dir;
        }

        /// <summary>Resolve display VoyageNumber (e.g. "61") from internal VoyageId FK.</summary>
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

        /// <summary>Resolve "LegPort_A to LegPort_B" scoped by VoyageId + VesselId (VoyageLeg.Id is not unique).</summary>
        private static string LookupLeg(int legPortId, int voyageId, int vesselId)
        {
            try
            {
                if (legPortId > 0)
                {
                    using (var adp = new SqlDataAdapter(
                        "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + legPortId + " and VoyageId=" + voyageId + " and VesselId=" + vesselId, ConnectionBulder.con))
                    {
                        var dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0) return dt.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
                if (voyageId > 0)
                {
                    using (var adp = new SqlDataAdapter(
                        "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + voyageId + " and VesselId=" + vesselId + " and IsActive=1", ConnectionBulder.con))
                    {
                        var dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0) return dt.Rows[0]["Leg"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Builds full HTML email body for Arrival Report. Loads template from Templates/ArrivalReport.html and replaces placeholders with data.
        /// </summary>
        /// <param name="reportId">When set (from export filename R{id}), matches Excel save fallback via editarrivalRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            var vd = new ArrivalReport();
            ArrivalReport arrRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                vd.GetArrivalRList = CommonMethods.editarrivalRList(reportId.Value, vesselId, "ArrivalReport");
                arrRBind = vd.GetArrivalRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (arrRBind == null)
            {
                vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselId, "ArrivalReport");
                arrRBind = vd.GetArrivalRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (arrRBind == null)
            {
                vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselId, "ArrivalReportR");
                arrRBind = vd.GetArrivalRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (arrRBind == null) return null;

            int id = arrRBind.Id;
            DataTable dtFuelCons = new DataTable(), dtFuelROB = new DataTable(), dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable(), dtMain = new DataTable(), dtARCargo = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.EOSP, a.FWE from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                if (dtBunker.Rows.Count == 0)
                {
                    DataTable dtFuelTypes = new DataTable();
                    using (var adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                        adp.Fill(dtFuelTypes);
                    using (var adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=2 order by FuelType_Id", ConnectionBulder.con))
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
                using (var adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=2 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                    adp.Fill(dtNonRoutine);
                try
                {
                    // Cargo names: try the direct lr_cargo_id FK first; if that's a dead reference
                    // (LR_Cargo row was deleted/renumbered by a Loading Report re-save), fall back
                    // to leg-scoped lookup. Cargoes belong to a leg — same convention as
                    // ArrivalController line 990: LR_Cargo WHERE VoyageId + LegPortId + VesselId.
                    // Positional pairing uses reverse Id (newest LR_Cargo entry pairs with oldest
                    // AR_Cargo row) because the original save's MAX(Id) lookup gave newest-first.
                    string cargoQuery = @"
WITH ar_rows AS (
    SELECT *, ROW_NUMBER() OVER (ORDER BY Id ASC) AS _pos
    FROM AR_Cargo
    WHERE VesselId = " + vesselId + @" AND arrivalreport_id = " + id + @"
),
report_ctx AS (
    SELECT LegPortId, VoyageId
    FROM ArrivalReport
    WHERE Id = " + id + @" AND VesselId = " + vesselId + @"
),
leg_lr AS (
    -- Match by LegPortId + VesselId only. VoyageId intentionally NOT in the join because
    -- data drift has been observed where cargoes from the same Loading Report (same LRId,
    -- same leg) ended up tagged to different VoyageIds — losing them on a strict join.
    SELECT b.CargoName, b.PortName,
           ROW_NUMBER() OVER (ORDER BY b.Id DESC) AS _pos
    FROM LR_Cargo b
    INNER JOIN report_ctx rc ON b.LegPortId = rc.LegPortId
    WHERE b.VesselId = " + vesselId + @"
)
SELECT a.*,
       COALESCE(direct.CargoName, leg.CargoName) AS CargoName,
       COALESCE(direct.PortName,  leg.PortName)  AS PortName
FROM ar_rows a
LEFT JOIN LR_Cargo direct ON a.lr_cargo_id = direct.Id AND a.VesselId = direct.VesselId
LEFT JOIN leg_lr leg      ON leg._pos = a._pos
ORDER BY a.Id";
                    using (var adp = new SqlDataAdapter(cargoQuery, ConnectionBulder.con))
                        adp.Fill(dtARCargo);
                }
                catch { }
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", arrRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "ArrivalReport");
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var da = new SqlDataAdapter(cmd)) da.Fill(dtMain);
                }
            }
            catch { /* continue with empty DataTables */ }

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
                return ApplyTemplate(template, arrRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtARCargo);
            }
            return BuildHtmlInline(arrRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtARCargo);
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

        private static string ApplyTemplate(string template, ArrivalReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtARCargo)
        {
            string voyNo = r.voyagenumber;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();

            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg")) legText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(legText)) legText = LookupLeg(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Latitude", FormatLatLon(r.Latitude))).Append(KvRow("Longitude", FormatLatLon(r.Longitude)));
            sb.Append(KvRow("Place", r.Place)).Append(KvRow("Leg", legText)).Append(KvRow("NOR", r.NOR != null ? Vdt(r.NOR) : "-")).Append(KvRow("Port", r.PortName));
            sb.Append(KvRow("EOSP", r.EOSP != null ? Vdt(r.EOSP) : "-")).Append(KvRow("ETB", r.ETB != null ? Vdt(r.ETB) : "-"));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Anchorage Name", r.Anchor_Name)).Append(KvRow("Drop Anchor Date & Time", r.Anchor_DateT != null ? Vdt(r.Anchor_DateT) : "-"));
            sb.Append(KvRow("Anchoring Position - Latitude", FormatLatLon(r.AnchorPos_Latitude))).Append(KvRow("Anchoring Position - Longitude", FormatLatLon(r.AnchorPos_Longitude)));
            sb.Append(KvRow("FWE Date & Time", r.AnchorFWE_DateT != null ? Vdt(r.AnchorFWE_DateT) : "-"));
            string anchorageRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG));
            sb.Append(KvRow("Stmg Time Noon to Arrival(Hrs)", r.StmgTime));
            sb.Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime));
            sb.Append(KvRow("Actual Speed Noon to Arrival(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            string speedRows = sb.ToString();
            sb.Clear();

            // Manoeuvring — its own dedicated section between Speed-Distance-Time and Non-Routine Events.
            sb.Append(KvRow("Manoeuvring Hours", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
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

            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight));
            sb.Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            string weatherRows = sb.ToString();
            sb.Clear();

            string remarksRow = @"<tr><td colspan=""8"" style=""padding:8px;border:1px solid #ccc;vertical-align:middle;"">" + (r.Remarks ?? "-") + @"</td></tr>";

            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR));
            string engineRows = sb.ToString();
            sb.Clear();

            // Fuel ROB in MT — rows for the 3-col table (Fuel | EOSP | FWE) consumed by the template.
            // Num() strips trailing zeros so 745.200 → 745.2, matching the web view.
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string ft = (dr["FuelType"]?.ToString() ?? "").Trim();
                    string eosp = dr.Table.Columns.Contains("EOSP") ? Num(dr["EOSP"]) : "-";
                    string fwe = dr.Table.Columns.Contains("FWE") ? Num(dr["FWE"]) : "-";
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">").Append(ft).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(eosp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(fwe).Append(@"</td></tr>");
                }
            }
            string fuelRobRows = sb.ToString();
            sb.Clear();

            // Bunker Received section — only build the HTML when data exists. When the web view
            // has no bunker entries, bunkerSection is empty and the {{BUNKER_SECTION}} placeholder
            // resolves to nothing so the section header and empty table never render.
            string bunkerSection = "";
            if (BunkerHasData(dtBunker))
            {
                var bsb = new StringBuilder();
                bsb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-size:14px;font-weight:bold;text-align:center;border:1px solid #ccc;white-space:nowrap"">Bunker Received in MT</td></tr>");
                bsb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table class=""data-table"" style=""width:100%;border-collapse:collapse;border:none;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
                foreach (DataRow dr in dtBunker.Rows)
                    bsb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
                bsb.Append(@"</table></td></tr>");
                bunkerSection = bsb.ToString();
            }
            sb.Clear();

            // Fuel Consumption in MT — full 7-block layout matching Daily Noon.
            decimal vlsfoTotal = GetFuelConsByType(dtFuelCons, "VLSFO");
            decimal mdoTotal = GetFuelConsByType(dtFuelCons, "MDO");
            string fuelConsFullTable = BuildFuelConsFullTable(vlsfoTotal, mdoTotal, dtFuelCons);

            // Cargo rows — single Qty(MT) column, no decimal padding (40476.00 → 40476).
            // When CargoName is empty (LR_Cargo FK no longer exists), show "Cargo #<lr_cargo_id>"
            // so the broken FK is visible in the email instead of a misleading "Cargo" placeholder.
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel;
                    if (!string.IsNullOrEmpty(cName))
                        cargoLabel = cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    else
                    {
                        // Try both casings explicitly because some SqlDataAdapter loads preserve column case.
                        string lrId = "";
                        if (dr.Table.Columns.Contains("LR_Cargo_Id") && dr["LR_Cargo_Id"] != DBNull.Value)
                            lrId = dr["LR_Cargo_Id"].ToString();
                        else if (dr.Table.Columns.Contains("lr_cargo_id") && dr["lr_cargo_id"] != DBNull.Value)
                            lrId = dr["lr_cargo_id"].ToString();
                        cargoLabel = string.IsNullOrEmpty(lrId) || lrId == "0" ? "Cargo" : "Cargo #" + lrId;
                    }
                    string q1 = FormatCargoQty(dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null);
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(q1).Append(@"</td></tr>");
                }
            }
            string cargoRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ROB", r.Ballast_ROB));
            string ballastRow = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            string freshWaterRows = sb.ToString();

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", V(r.NOR ?? r.EOSP))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{ANCHORAGE_ROWS}}", anchorageRows)
                .Replace("{{SPEED_DISTANCE_ROWS}}", speedRows)
                .Replace("{{MANOEUVRING_ROWS}}", manoeuvringRows)
                .Replace("{{NON_ROUTINE_ROWS}}", nonRoutineRows)
                .Replace("{{WEATHER_ROWS}}", weatherRows)
                .Replace("{{REMARKS_ROW}}", remarksRow)
                .Replace("{{ENGINE_ROWS}}", engineRows)
                .Replace("{{LO_MECC_CONS}}", V(r.LO_HO_Cons_MECC))
                .Replace("{{LO_MECC_ROB}}", V(r.LO_HO_Cons_MECC_ROB))
                .Replace("{{LO_MECYL_CONS}}", V(r.LO_HO_Cons_MECYL))
                .Replace("{{LO_MECYL_ROB}}", V(r.LO_HO_Cons_MECYL_ROB))
                .Replace("{{LO_AECC_CONS}}", V(r.LO_HO_Cons_AECC))
                .Replace("{{LO_AECC_ROB}}", V(r.LO_HO_Cons_AECC_ROB))
                .Replace("{{LO_HYDR_CONS}}", V(r.LO_HO_Cons_HYDR_Oil))
                .Replace("{{LO_HYDR_ROB}}", V(r.LO_HO_Cons_HYDR_Oil_ROB))
                .Replace("{{FUEL_ROB_ROWS}}", fuelRobRows)
                .Replace("{{BUNKER_SECTION}}", bunkerSection)
                .Replace("{{OT_ROB_OXY_Full}}", V(r.OT_ROB_OXY_Full))
                .Replace("{{OT_ROB_OXY_InUse}}", V(r.OT_ROB_OXY_InUse))
                .Replace("{{OT_ROB_OXY_Empty}}", V(r.OT_ROB_OXY_Empty))
                .Replace("{{OT_ROB_ACYT_Full}}", V(r.OT_ROB_ACYT_Full))
                .Replace("{{OT_ROB_ACYT_InUse}}", V(r.OT_ROB_ACYT_InUse))
                .Replace("{{OT_ROB_ACYT_Empty}}", V(r.OT_ROB_ACYT_Empty))
                .Replace("{{FUEL_CONS_FULL_TABLE}}", fuelConsFullTable)
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{SLOPS_ROB_OXY_Oil}}", V(r.SLOPS_ROB_OXY_Oil))
                .Replace("{{SLOPS_ROB_OXY_Water}}", V(r.SLOPS_ROB_OXY_Water))
                .Replace("{{SLOPS_ROB_OXY_Total}}", V(r.SLOPS_ROB_OXY_Total))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(ArrivalReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtARCargo)
        {
            string voyNo = r.voyagenumber;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
            }
            if (string.IsNullOrWhiteSpace(voyNo)) voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();

            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg")) legText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(legText)) legText = LookupLeg(r.LegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Arrival Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.NOR ?? r.EOSP)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Arrival Report (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(CompactRow("Voy No.", voyNo, "Latitude", FormatLatLon(r.Latitude), "Longitude", FormatLatLon(r.Longitude)));
            sb.Append(CompactRow("Place", r.Place, "Leg", legText));
            sb.Append(CompactRow("NOR", r.NOR != null ? Vdt(r.NOR) : "-", "Port", r.PortName));
            sb.Append(CompactRow("EOSP", r.EOSP != null ? Vdt(r.EOSP) : "-", "ETB", r.ETB != null ? Vdt(r.ETB) : "-"));
            sb.Append(CompactRow("Draft Fwd (Mtrs)", r.DraftFwd, "Draft Mid (Mtrs)", r.DraftMid, "Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Anchorage</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Anchorage Name", r.Anchor_Name)).Append(KvRow("Drop Anchor Date & Time", r.Anchor_DateT != null ? Vdt(r.Anchor_DateT) : "-"));
            sb.Append(KvRow("Anchoring Position - Latitude", FormatLatLon(r.AnchorPos_Latitude))).Append(KvRow("Anchoring Position - Longitude", FormatLatLon(r.AnchorPos_Longitude))).Append(KvRow("FWE Date & Time", r.AnchorFWE_DateT != null ? Vdt(r.AnchorFWE_DateT) : "-"));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Speed - Distance - Time</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Arrival(Hrs)", r.StmgTime)).Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime)).Append(KvRow("Actual Speed Noon to Arrival(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            sb.Append(@"</table></td></tr>");

            // Manoeuvring — separate section after Speed-Distance-Time.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Manoeuvring</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Manoeuvring Hours", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            sb.Append(@"</table></td></tr>");
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
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Weather</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight)).Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Arrival Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR));
            sb.Append(@"</table></td></tr>");

            // 1. LO & HO Consumptions — 3-col layout (Item | Consumption | ROB), matching Daily Noon.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">LO &amp; HO Consumptions</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:30%;min-width:150px""><col style=""width:30%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Consumption</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">ROB</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECC (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECYL (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">AECC (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">HYDRAULIC Oil (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil_ROB)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // 2. Fuel ROB in MT — 3-col layout (Fuel | EOSP | FWE) sourced from dtFuelROB (joined tbl_FuelROB).
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:30%;min-width:150px""><col style=""width:30%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">EOSP</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">FWE</td></tr>");
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string ft = (dr["FuelType"]?.ToString() ?? "").Trim();
                    string eosp = dr.Table.Columns.Contains("EOSP") ? Num(dr["EOSP"]) : "-";
                    string fwe = dr.Table.Columns.Contains("FWE") ? Num(dr["FWE"]) : "-";
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">").Append(ft).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(eosp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(fwe).Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Bunker Received in MT — only render the section when at least one row has a
            // non-empty Receipt value. If no bunker data is entered in the web view, hide the
            // header and table completely (no empty section).
            if (BunkerHasData(dtBunker))
            {
                sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
                foreach (DataRow dr in dtBunker.Rows) sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
                sb.Append(@"</table></td></tr>");
            }

            // 3. Other ROB — 4-col (label | Full | In Use | Empty), unchanged from prior layout.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // 4. Fuel Consumption in MT — full 7-block layout matching Daily Noon.
            {
                decimal vlsfoTot = GetFuelConsByType(dtFuelCons, "VLSFO");
                decimal mdoTot = GetFuelConsByType(dtFuelCons, "MDO");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;"">").Append(BuildFuelConsFullTable(vlsfoTot, mdoTot, dtFuelCons)).Append(@"</td></tr>");
            }
            // Cargo tab section order matches the web view:
            //   1. Fresh Water Noon To Report   2. Slops ROB   3. Cargo   4. Ballast

            // 1. Fresh Water Noon To Report (renamed from "Fresh Water").
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fresh Water Noon To Report</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            sb.Append(@"</table></td></tr>");

            // 2. Slops ROB — column labels now include the (m3) unit.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Total)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // 3. Cargo — single Qty(MT) column, no decimals.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:60%;min-width:280px""><col style=""width:40%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Qty(MT)</td></tr>");
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel;
                    if (!string.IsNullOrEmpty(cName))
                        cargoLabel = cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    else
                    {
                        // Try both casings explicitly because some SqlDataAdapter loads preserve column case.
                        string lrId = "";
                        if (dr.Table.Columns.Contains("LR_Cargo_Id") && dr["LR_Cargo_Id"] != DBNull.Value)
                            lrId = dr["LR_Cargo_Id"].ToString();
                        else if (dr.Table.Columns.Contains("lr_cargo_id") && dr["lr_cargo_id"] != DBNull.Value)
                            lrId = dr["lr_cargo_id"].ToString();
                        cargoLabel = string.IsNullOrEmpty(lrId) || lrId == "0" ? "Cargo" : "Cargo #" + lrId;
                    }
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoQty(dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null)).Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // 4. Ballast — label simplified to "ROB".
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ROB", r.Ballast_ROB));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"</table>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>");
            sb.Append(@"<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Thank you,</p><p style=""margin:4px 0 0 0;"">Team SIS</p></body></html>");
            return sb.ToString();
        }

        /// <summary>Single label-value row for 2-column layout. Label 45% min 280px, value 55%.</summary>
        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }

        private static string CompactRow(params object[] pairs)
        {
            if (pairs == null || pairs.Length < 2) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < pairs.Length - 1; i += 2)
                sb.Append(KvRow(pairs[i]?.ToString() ?? "", pairs[i + 1]));
            return sb.ToString();
        }

        private static string FormatCargoVal(object v)
        {
            if (v == null || v == DBNull.Value) return "-";
            decimal d;
            return decimal.TryParse(v.ToString(), out d) ? d.ToString(CultureInfo.InvariantCulture) : V(v);
        }

        /// <summary>Cargo Qty(MT) formatter — shows the value exactly as stored in the DB, preserving
        /// trailing zeros (40476.000 stays "40476.000"). Returns "-" for null/empty.</summary>
        private static string FormatCargoQty(object v)
        {
            if (v == null || v == DBNull.Value) return "-";
            decimal d;
            return decimal.TryParse(v.ToString(), out d) ? d.ToString(CultureInfo.InvariantCulture) : V(v);
        }

        private static decimal GetFuelConsByType(DataTable dt, string fuelType)
        {
            if (dt == null) return 0;
            decimal sum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase))
                {
                    var v = dr["Value"];
                    if (v != null && v != DBNull.Value) sum += Convert.ToDecimal(v);
                }
            }
            return sum;
        }

        // Shared inline-style fragments for the Fuel Consumption section (email-safe; CSS is
        // inlined per cell). Copied from DailyNoonReportEmailTemplate to keep the two reports
        // visually identical for this section.
        private const string FC_FONT  = "font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TBL   = "width:100%;border-collapse:collapse;margin-bottom:14px;font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TD    = "border:1px solid #bbb;padding:5px 9px;text-align:center;";
        private const string FC_TH    = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#f0f0f0;font-weight:600;font-size:11px;";
        private const string FC_LBL   = "border:1px solid #bbb;padding:5px 9px;text-align:left;font-weight:600;background:#f7f7f7;";
        private const string FC_GRP   = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#e8e8e8;font-weight:700;font-size:11px;letter-spacing:0.03em;";
        private const string FC_TOTAL = "border:1px solid #bbb;padding:5px 9px;text-align:center;font-weight:700;";

        // Fuel consumption columns are decimal(18,3); render fixed 3 decimals so stored values and
        // computed 0 defaults show uniformly (e.g. 5.000, 0.000) — same as the Daily Noon template.
        private static string FcFmt(decimal v) => v.ToString("0.000", CultureInfo.InvariantCulture);

        /// <summary>Engine-style table: group header + Fuel/At Sea/Manoeuv./Anchor-Wait/Berth columns,
        /// optionally a Sub Total column. Used for Main Engine, Aux Engine, Boiler, FRAMO System.</summary>
        private static string FuelConsEngineTable(string groupName, bool showSubTotal,
            decimal vSea, decimal vMan, decimal vWait, decimal vBerth,
            decimal dSea, decimal dMan, decimal dWait, decimal dBerth)
        {
            int cols = showSubTotal ? 6 : 5;
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"").Append(cols).Append("\" style=\"").Append(FC_GRP).Append("\">").Append(groupName).Append("</td></tr>");
            s.Append("<tr>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Fuel</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">AT SEA</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">MANOEUV</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">ANCHOR/WAIT</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">BERTH</th>");
            if (showSubTotal) s.Append("<th style=\"").Append(FC_TH).Append("\">SUB TOTAL</th>");
            s.Append("</tr>");
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vSea)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vMan)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vWait)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vBerth)).Append("</td>");
            if (showSubTotal) s.Append("<td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(FcFmt(vSea + vMan + vWait + vBerth)).Append("</td>");
            s.Append("</tr>");
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dSea)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dMan)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dWait)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dBerth)).Append("</td>");
            if (showSubTotal) s.Append("<td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(FcFmt(dSea + dMan + dWait + dBerth)).Append("</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>IGG & Incinerator table: group header + Fuel/IGG/Incinerator columns.</summary>
        private static string FuelConsIggIncTable(decimal vIgg, decimal vInc, decimal dIgg, decimal dInc)
        {
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"3\" style=\"").Append(FC_GRP).Append("\">IGG &amp; Incinerator</td></tr>");
            s.Append("<tr>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Fuel</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">IGG</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Incinerator</th>")
              .Append("</tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vIgg)).Append("</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vInc)).Append("</td></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dIgg)).Append("</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dInc)).Append("</td></tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Events table: group header + Fuel + 8 event columns (Stoppage..Others).</summary>
        private static string FuelConsEventsTable(DataTable dtFuelCons)
        {
            int[] eventConsTypeIds = { 20, 21, 22, 23, 24, 25, 26, 27 };
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"9\" style=\"").Append(FC_GRP).Append("\">Events</td></tr>");
            s.Append("<tr>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Fuel</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Stoppage</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Deviation</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Slow Steaming</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Bad Weather</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">COT Prep</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Cargo Heating</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">BW Exchange</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Others</th>")
              .Append("</tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td>");
            foreach (int ct in eventConsTypeIds)
                s.Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(GetFuelConsByConsType(dtFuelCons, "VLSFO", ct))).Append("</td>");
            s.Append("</tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td>");
            foreach (int ct in eventConsTypeIds)
                s.Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(GetFuelConsByConsType(dtFuelCons, "MDO", ct))).Append("</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Total summary table: VLSFO Total / MDO Total rows with " MT" suffix.</summary>
        private static string FuelConsTotalTable(decimal vlsfoTotal, decimal mdoTotal)
        {
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"2\" style=\"").Append(FC_GRP).Append("\">Total</td></tr>");
            // Total values are pinned to 3 decimal places (no "MT" suffix) to match the web view.
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO TOTAL</td><td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(vlsfoTotal.ToString("0.000")).Append("</td></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO TOTAL</td><td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(mdoTotal.ToString("0.000")).Append("</td></tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Single (fuelType, consTypeId) lookup against dtFuelCons.</summary>
        private static decimal GetFuelConsByConsType(DataTable dt, string fuelType, int consTypeId)
        {
            if (dt == null) return 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (!(dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase)) continue;
                if (!int.TryParse(dr["ConsTypeId"]?.ToString() ?? "", out int rowConsType)) continue;
                if (rowConsType != consTypeId) continue;
                var v = dr["Value"];
                if (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out decimal d)) return d;
                return 0;
            }
            return 0;
        }

        /// <summary>Full Fuel Consumption in MT block: dark-grey banner + 7 sub-tables
        /// (Main Engine, Aux Engine, Boiler, FRAMO, IGG &amp; Incinerator, Events, Total).
        /// Uses the same ConsTypeId mapping as Daily Noon (ME 2-5, AE 7-10, BLR 11-14,
        /// FRAMO 15-18, IGG 19, Events 20-27, Incinerator 28).</summary>
        private static string BuildFuelConsFullTable(decimal vlsfoTotal, decimal mdoTotal, DataTable dtFuelCons)
        {
            decimal FV(string ft, int consTypeId) => GetFuelConsByConsType(dtFuelCons, ft, consTypeId);
            var sb = new StringBuilder();
            sb.Append("<div style=\"").Append(FC_FONT).Append("\">");
            sb.Append("<div style=\"padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;font-size:14px;margin-bottom:16px;\">Fuel Consumption in MT</div>");
            sb.Append(FuelConsEngineTable("Main Engine", true,
                FV("VLSFO", 2), FV("VLSFO", 3), FV("VLSFO", 4), FV("VLSFO", 5),
                FV("MDO",   2), FV("MDO",   3), FV("MDO",   4), FV("MDO",   5)));
            sb.Append(FuelConsEngineTable("Aux Engine", true,
                FV("VLSFO", 7), FV("VLSFO", 8), FV("VLSFO", 9), FV("VLSFO", 10),
                FV("MDO",   7), FV("MDO",   8), FV("MDO",   9), FV("MDO",   10)));
            sb.Append(FuelConsEngineTable("Boiler", true,
                FV("VLSFO", 11), FV("VLSFO", 12), FV("VLSFO", 13), FV("VLSFO", 14),
                FV("MDO",   11), FV("MDO",   12), FV("MDO",   13), FV("MDO",   14)));
            sb.Append(FuelConsEngineTable("Framo System", false,
                FV("VLSFO", 15), FV("VLSFO", 16), FV("VLSFO", 17), FV("VLSFO", 18),
                FV("MDO",   15), FV("MDO",   16), FV("MDO",   17), FV("MDO",   18)));
            sb.Append(FuelConsIggIncTable(FV("VLSFO", 19), FV("VLSFO", 28), FV("MDO", 19), FV("MDO", 28)));
            sb.Append(FuelConsEventsTable(dtFuelCons));
            sb.Append(FuelConsTotalTable(vlsfoTotal, mdoTotal));
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}
