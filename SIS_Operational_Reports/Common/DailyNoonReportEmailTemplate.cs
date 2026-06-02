using DataBuildingLayer;
using SIS_Operational_Reports.Areas.Report.Controllers;
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
    /// Builds HTML email body for Daily Noon Report. Uses DailyNoonReport.html template when available.
    /// Same design, width (1200px) and column layout as Arrival Report.
    /// </summary>
    public static class DailyNoonReportEmailTemplate
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/DailyNoonReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        /// <summary>Format decimal: show value exactly as entered with no trailing-zero padding.
        /// 10 → "10", 20.7 → "20.7", 9.750 → "9.75", 0 → "0", null → "-".</summary>
        private static string V(decimal? d) => d.HasValue ? d.Value.ToString("0.##########") : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <summary>Normalize a numeric value coming from a DataRow column (where the type may be
        /// string, decimal, or null) so the output strips trailing zeros. Returns "-" for null/empty,
        /// the raw trimmed string when the value can't be parsed as a number.</summary>
        private static string Num(object o)
        {
            if (o == null || o == DBNull.Value) return "-";
            string s = o.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return "-";
            if (decimal.TryParse(s, out decimal d)) return d.ToString("0.##########");
            return s;
        }

        /// <summary>Format nautical Latitude/Longitude: "21,58.29 S" → "21° 58.29' S". Returns "-" if empty, original string if unparseable.</summary>
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

        /// <summary>
        /// Builds full HTML email body for Daily Noon Report. Loads template from Templates/DailyNoonReport.html and replaces placeholders with data.
        /// </summary>
        public static string BuildHtml(int vesselId, string datePart)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            var vd = new DailyNoonReport();
            vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselId, "DailyNoonReport");
            var noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
            if (noonRBind == null) return null;

            int id = noonRBind.Id;
            var cargoTanks = DailyNoonController.GetCargoTankList(id, vesselId);
            var ballastTanks = DailyNoonController.GetBallastTankList(id, vesselId);
            var voidSpaces = DailyNoonController.GetVoid_SpaceList(id, vesselId);

            DataTable dtFuelCons = new DataTable(), dtFuelROB = new DataTable(), dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable(), dtNRCargo = new DataTable(), dtMain = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 order by a.Id", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                using (var adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                    adp.Fill(dtNonRoutine);
                try
                {
                    using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                        adp.Fill(dtNRCargo);
                }
                catch
                {
                    using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.lr_cargo_id=b.Id where a.VesselId=" + vesselId + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                        adp.Fill(dtNRCargo);
                }
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
            catch { /* continue with empty DataTables - still build full HTML */ }

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
                return ApplyTemplate(template, noonRBind, vesselName, cargoTanks, ballastTanks, voidSpaces, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtNRCargo);
            }
            return BuildHtmlInline(noonRBind, vesselName, cargoTanks, ballastTanks, voidSpaces, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtNRCargo);
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

        /// <summary>Resolves the human-readable Port Status label (e.g. "Others") from the
        /// PortStatus FK id, using the same spCommonBinding@PortStatus list that the web form uses.
        /// Returns null when the id is missing or the lookup fails.</summary>
        private static string LookupPortStatusName(int? portStatusId)
        {
            if (!portStatusId.HasValue || portStatusId.Value <= 0) return null;
            try
            {
                var list = DailyNoonReport.portSList();
                var match = list?.FirstOrDefault(p => p.Id == portStatusId.Value);
                if (match != null && !string.IsNullOrWhiteSpace(match.Status))
                    return match.Status.Trim();
            }
            catch { }
            return null;
        }

        /// <summary>Resolves the Leg display text (e.g. "AT SEA to Khor al Fakkan") from the
        /// VoyageLeg table by LegPortId+VoyageId+VesselId. Falls back to a top-1 lookup by VoyageId
        /// when the LegPortId-scoped row is missing. Returns null when nothing matches.</summary>
        private static string LookupLegText(int legPortId, int voyageId, int vesselId)
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
                        if (dt.Rows.Count > 0)
                        {
                            string s = dt.Rows[0]["Leg"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                        }
                    }
                }
                if (voyageId > 0)
                {
                    using (var adp = new SqlDataAdapter(
                        "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + voyageId + " and VesselId=" + vesselId + " and IsActive=1", ConnectionBulder.con))
                    {
                        var dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            string s = dt.Rows[0]["Leg"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string ApplyTemplate(string template, DailyNoonReport r, string vesselName, List<DNR_Cargo_Tank> cargoTanks, List<DNR_Ballast_Tank> ballastTanks, List<DNR_Void_Space> voidSpaces, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtNRCargo)
        {
            string voyNo = r.voyagenumber;
            string legText = r.LegPortName ?? "";
            string portStatusText = r.PortStatus?.ToString() ?? "";
            decimal? cpSpeed = r.CP_Speed;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                else if (dtMain.Columns.Contains("VoyNo")) voyNo = dr["VoyNo"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                if (dtMain.Columns.Contains("CP_Speed") && decimal.TryParse(dr["CP_Speed"]?.ToString(), out decimal cpVal)) cpSpeed = cpVal;
                else if (dtMain.Columns.Contains("CPSpeed") && decimal.TryParse(dr["CPSpeed"]?.ToString(), out decimal cpVal2)) cpSpeed = cpVal2;
            }
            // Fallback: look up the display VoyageNumber from the Voyage table
            if (string.IsNullOrWhiteSpace(voyNo))
                voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            // Fallback: when portStatusText is still the numeric FK (or empty), resolve the label
            if (string.IsNullOrWhiteSpace(portStatusText) || int.TryParse(portStatusText, out _))
                portStatusText = LookupPortStatusName(r.PortStatus) ?? portStatusText;
            // Fallback: when legText is empty, resolve from VoyageLeg by LegPortId+VoyageId+VesselId
            if (string.IsNullOrWhiteSpace(legText))
                legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId) ?? legText;

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Status", r.VesselStatus)).Append(KvRow("Latitude", FormatLatLon(r.Latitude))).Append(KvRow("Longitude", FormatLatLon(r.Longitude)));
            sb.Append(KvRow("At Sea/In Port", r.AtSeaOrPort)).Append(KvRow("In Port Status", portStatusText)).Append(KvRow("Displacement(MT)", r.Displacement)).Append(KvRow("CP Speed(Kts)", cpSpeed));
            sb.Append(KvRow("Leg", legText)).Append(KvRow("Report Date", V(r.Date))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime));
            sb.Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime)).Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            string speedRows = sb.ToString();
            sb.Clear();

            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < nreLabels.Length; i++)
            {
                string acc = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "-") : "-";
                string hrs = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? Num(dtNonRoutine.Rows[i]["Hours"]) : "-";
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

            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR)).Append(KvRow("M/E Control Location", r.ME_ControlLoc));
            sb.Append(KvRow("SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress)).Append(KvRow("SCAV. Temp (Deg Centigrade)", r.SCAV_Temp));
            sb.Append(KvRow("Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp)).Append(KvRow("Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp)).Append(KvRow("SW Temp (Deg Centigrade)", r.SW_Temp)).Append(KvRow("ER Temp (Deg Centigrade)", r.ER_Temp));
            string engineRows = sb.ToString();
            sb.Clear();

            if (dtFuelROB != null) foreach (DataRow dr in dtFuelROB.Rows)
                sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"] : null));
            string fuelRobRows = sb.ToString();
            sb.Clear();

            // Build bunker rows – fuel types and values come from DB join with tblFuelType; dedupe by FuelType (keep first occurrence by Id)
            {
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;text-align:center;"">Received</td></tr>");
                var seenBunkerFuels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows)
                {
                    string ft = (dr["FuelType"]?.ToString() ?? "").Trim();
                    if (string.IsNullOrEmpty(ft) || !seenBunkerFuels.Add(ft)) continue;
                    string rcpt = Num(dr["Receipt"]);
                    if (rcpt == "-") rcpt = "0";
                    sb.Append(KvRow(ft, rcpt));
                }
            }
            string bunkerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Running Hrs No.1", r.AE_RungHrs_No1)).Append(KvRow("Running Hrs No.2", r.AE_RungHrs_No2)).Append(KvRow("Running Hrs No.3", r.AE_RungHrs_No3)).Append(KvRow("Running Hrs No.4", r.AE_RungHrs_No4)).Append(KvRow("Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen));
            sb.Append(KvRow("Load No.1 (KW)", r.AE_Load_No1)).Append(KvRow("Load No.2 (KW)", r.AE_Load_No2)).Append(KvRow("Load No.3 (KW)", r.AE_Load_No3)).Append(KvRow("Load No.4 (KW)", r.AE_Load_No4)).Append(KvRow("Load Shaft Gen (KW)", r.AE_Load_ShaftGen)).Append(KvRow("Extra Run Reason", r.AE_Extra_Run_Reason));
            string auxEngineRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Boiler No.1 Running Hrs", r.BR_RungHrs_No1)).Append(KvRow("Boiler No.2 Running Hrs", r.BR_RungHrs_No2)).Append(KvRow("Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1)).Append(KvRow("Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2));
            string boilerRows = sb.ToString();
            sb.Clear();

            // Build full fuel consumption table
            decimal vlsfoTotal = GetFuelConsByType(dtFuelCons, "VLSFO"), mdoTotal = GetFuelConsByType(dtFuelCons, "MDO");
            string fuelConsFullTable = BuildFuelConsFullTable(r, vlsfoTotal, mdoTotal, dtFuelCons);
            sb.Clear();

            if (dtNRCargo != null) foreach (DataRow dr in dtNRCargo.Rows)
            {
                string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                string lbl = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(V(lbl)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargo(dr, "BL_Qty")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargo(dr, "LoadPortalActual")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargo(dr, "TodaysActual")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargo(dr, "Qty_Diff")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(V(dr.Table.Columns.Contains("Reasonfor_Qty_Diff") ? dr["Reasonfor_Qty_Diff"] : null)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargo(dr, "Cargo_Temp")).Append(@"</td></tr>");
            }
            string cargoRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            string ballastRow = sb.ToString();
            sb.Clear();

            foreach (var vs in voidSpaces ?? new List<DNR_Void_Space>())
                sb.Append(KvRow(vs.TankName ?? "", vs.Sounding));
            string voidSpacesRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            string freshWaterRows = sb.ToString();
            sb.Clear();

            // Pump Room & Chain Lockers
            sb.Append(KvRow("Pump Room bilge max sounding", r.PumpRoomMaxSounding)).Append(KvRow("Chain Locker 1", r.ChainLocker1)).Append(KvRow("Chain Locker 2", r.ChainLocker2));
            string pumpChainRows = sb.ToString();

            var cTanks = cargoTanks ?? new List<DNR_Cargo_Tank>();
            string cargoTanksSection = "";
            if (cTanks.Count > 0)
            {
                var ctSb = new StringBuilder();
                ctSb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-size:14px;font-weight:bold;text-align:center;border:1px solid #ccc;white-space:nowrap"">Cargo Tanks</td></tr>");
                ctSb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table class=""data-table"" style=""width:100%;border-collapse:collapse;border:none;table-layout:fixed;font-size:11px;"">");
                ctSb.Append(@"<tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Tank</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:center;"">").Append(V(t.TankName)).Append(@"</td>");
                ctSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Ullage (mtrs)</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Ullage)).Append(@"</td>");
                ctSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">MT Qty</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Qty_MT)).Append(@"</td>");
                ctSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Oxygen (% Volume)</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Oxygen)).Append(@"</td>");
                ctSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">H2S (PPM)</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.H2S)).Append(@"</td>");
                ctSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">HC (% Volume)</td>");
                foreach (var t in cTanks) ctSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.HC)).Append(@"</td>");
                ctSb.Append(@"</tr></table></td></tr>");
                cargoTanksSection = ctSb.ToString();
            }

            var bTanks = ballastTanks ?? new List<DNR_Ballast_Tank>();
            string ballastTanksSection = "";
            if (bTanks.Count > 0)
            {
                var btSb = new StringBuilder();
                btSb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-size:14px;font-weight:bold;text-align:center;border:1px solid #ccc;white-space:nowrap"">Ballast Tanks</td></tr>");
                btSb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table class=""data-table"" style=""width:100%;border-collapse:collapse;border:none;table-layout:fixed;font-size:11px;"">");
                btSb.Append(@"<tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Tank</td>");
                foreach (var t in bTanks) btSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:center;"">").Append(V(t.TankName)).Append(@"</td>");
                btSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Sounding (mtrs)</td>");
                foreach (var t in bTanks) btSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Sounding)).Append(@"</td>");
                btSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Cubic Vol</td>");
                foreach (var t in bTanks) btSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Qty_Vol)).Append(@"</td>");
                btSb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">HC (% Volume)</td>");
                foreach (var t in bTanks) btSb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.HC)).Append(@"</td>");
                btSb.Append(@"</tr></table></td></tr>");
                ballastTanksSection = btSb.ToString();
            }

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", V(r.Date))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{SPEED_DISTANCE_ROWS}}", speedRows)
                .Replace("{{NON_ROUTINE_ROWS}}", nonRoutineRows)
                .Replace("{{WEATHER_ROWS}}", weatherRows)
                .Replace("{{REMARKS_ROW}}", remarksRow)
                .Replace("{{ENGINE_ROWS}}", engineRows)
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
                .Replace("{{FUEL_CONS_FULL_TABLE}}", fuelConsFullTable)
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{SLOPS_ROB_OXY_Oil}}", V(r.SLOPS_ROB_OXY_Oil))
                .Replace("{{SLOPS_ROB_OXY_Water}}", V(r.SLOPS_ROB_OXY_Water))
                .Replace("{{SLOPS_ROB_OXY_Total}}", V(r.SLOPS_ROB_OXY_Total))
                .Replace("{{LO_MECC_CONS}}", V(r.LO_HO_Cons_MECC))
                .Replace("{{LO_MECC_ROB}}", V(r.LO_HO_Cons_MECC_ROB))
                .Replace("{{LO_MECYL_CONS}}", V(r.LO_HO_Cons_MECYL))
                .Replace("{{LO_MECYL_ROB}}", V(r.LO_HO_Cons_MECYL_ROB))
                .Replace("{{LO_AECC_CONS}}", V(r.LO_HO_Cons_AECC))
                .Replace("{{LO_AECC_ROB}}", V(r.LO_HO_Cons_AECC_ROB))
                .Replace("{{LO_HYDR_CONS}}", V(r.LO_HO_Cons_HYDR_Oil))
                .Replace("{{LO_HYDR_ROB}}", V(r.LO_HO_Cons_HYDR_Oil_ROB))
                .Replace("{{ER_BILGE_ROB}}", V(r.ER_Bilge_ROB))
                .Replace("{{ER_SLUDGE_ROB}}", V(r.ER_Sludge_ROB))
                .Replace("{{ER_WASTEOIL_ROB}}", V(r.ER_WasteOil_ROB))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{VOID_SPACES_ROWS}}", voidSpacesRows)
                .Replace("{{PUMP_CHAIN_ROWS}}", pumpChainRows)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows)
                .Replace("{{CARGO_TANKS_SECTION}}", cargoTanksSection)
                .Replace("{{BALLAST_TANKS_SECTION}}", ballastTanksSection);
        }

        private static string BuildHtmlInline(DailyNoonReport r, string vesselName, List<DNR_Cargo_Tank> cargoTanks, List<DNR_Ballast_Tank> ballastTanks, List<DNR_Void_Space> voidSpaces, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtNRCargo)
        {
            string voyNo = r.voyagenumber;
            string legText = r.LegPortName ?? "";
            string portStatusText = r.PortStatus?.ToString() ?? "";
            decimal? cpSpeed = r.CP_Speed;
            if (dtMain != null && dtMain.Rows.Count > 0)
            {
                var dr = dtMain.Rows[0];
                if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? voyNo;
                else if (dtMain.Columns.Contains("VoyNo")) voyNo = dr["VoyNo"]?.ToString() ?? voyNo;
                if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                if (dtMain.Columns.Contains("CP_Speed") && decimal.TryParse(dr["CP_Speed"]?.ToString(), out decimal cpVal)) cpSpeed = cpVal;
                else if (dtMain.Columns.Contains("CPSpeed") && decimal.TryParse(dr["CPSpeed"]?.ToString(), out decimal cpVal2)) cpSpeed = cpVal2;
            }
            // Fallback: look up the display VoyageNumber from the Voyage table
            if (string.IsNullOrWhiteSpace(voyNo))
                voyNo = LookupVoyageNumber(r.VoyageId, r.VesselId) ?? r.VoyageId.ToString();
            // Fallback: when portStatusText is still the numeric FK (or empty), resolve the label
            if (string.IsNullOrWhiteSpace(portStatusText) || int.TryParse(portStatusText, out _))
                portStatusText = LookupPortStatusName(r.PortStatus) ?? portStatusText;
            // Fallback: when legText is empty, resolve from VoyageLeg by LegPortId+VoyageId+VesselId
            if (string.IsNullOrWhiteSpace(legText))
                legText = LookupLegText(r.LegPortId, r.VoyageId, r.VesselId) ?? legText;

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Daily Noon Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.Date)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Daily Noon Report (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Status", r.VesselStatus)).Append(KvRow("Latitude", FormatLatLon(r.Latitude))).Append(KvRow("Longitude", FormatLatLon(r.Longitude)));
            sb.Append(KvRow("At Sea/In Port", r.AtSeaOrPort)).Append(KvRow("In Port Status", portStatusText)).Append(KvRow("Displacement(MT)", r.Displacement)).Append(KvRow("CP Speed(Kts)", cpSpeed));
            sb.Append(KvRow("Leg", legText)).Append(KvRow("Report Date", V(r.Date))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Speed - Distance - Time</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime)).Append(KvRow("Total Time (Dep to Curr)(Hrs)", r.TotalTime)).Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Non-Routine Events</td></tr>");
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:38%;min-width:200px""><col style=""width:45%;min-width:220px""><col style=""width:17%;min-width:80px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Event Name</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap"">Owners/Charterers Account</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:nowrap;text-align:right"">Hrs.</td></tr>");
            for (int i = 0; i < nreLabels.Length; i++)
            {
                string acc = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "-") : "-";
                string hrs = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? Num(dtNonRoutine.Rows[i]["Hours"]) : "-";
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(nreLabels[i]).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(acc).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(hrs).Append(@"</td></tr>");
            }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Weather</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce));
            sb.Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight)).Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Noon Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR)).Append(KvRow("M/E Control Location", r.ME_ControlLoc));
            sb.Append(KvRow("SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress)).Append(KvRow("SCAV. Temp (Deg Centigrade)", r.SCAV_Temp)).Append(KvRow("Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp)).Append(KvRow("Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp)).Append(KvRow("SW Temp (Deg Centigrade)", r.SW_Temp)).Append(KvRow("ER Temp (Deg Centigrade)", r.ER_Temp));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtFuelROB != null) foreach (DataRow dr in dtFuelROB.Rows)
                sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"] : null));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;text-align:center;"">Received</td></tr>");
            {
                var seenBunkerFuels2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows)
                {
                    string ft = (dr["FuelType"]?.ToString() ?? "").Trim();
                    if (string.IsNullOrEmpty(ft) || !seenBunkerFuels2.Add(ft)) continue;
                    string rcpt = Num(dr["Receipt"]);
                    if (rcpt == "-") rcpt = "0";
                    sb.Append(KvRow(ft, rcpt));
                }
            }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Aux. Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Running Hrs No.1", r.AE_RungHrs_No1)).Append(KvRow("Running Hrs No.2", r.AE_RungHrs_No2)).Append(KvRow("Running Hrs No.3", r.AE_RungHrs_No3)).Append(KvRow("Running Hrs No.4", r.AE_RungHrs_No4)).Append(KvRow("Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen));
            sb.Append(KvRow("Load No.1 (KW)", r.AE_Load_No1)).Append(KvRow("Load No.2 (KW)", r.AE_Load_No2)).Append(KvRow("Load No.3 (KW)", r.AE_Load_No3)).Append(KvRow("Load No.4 (KW)", r.AE_Load_No4)).Append(KvRow("Load Shaft Gen (KW)", r.AE_Load_ShaftGen)).Append(KvRow("Extra Run Reason", r.AE_Extra_Run_Reason));
            sb.Append(@"</table></td></tr>");
            // LO & HO Consumptions (after Aux. Engine, before Boiler's)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">LO &amp; HO Consumptions</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:30%;min-width:150px""><col style=""width:30%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Consumption</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">ROB</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECC (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECYL (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_MECYL_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">AECC (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_AECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">HYDRAULIC Oil (Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.LO_HO_Cons_HYDR_Oil_ROB)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Boiler's</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Boiler No.1 Running Hrs", r.BR_RungHrs_No1)).Append(KvRow("Boiler No.2 Running Hrs", r.BR_RungHrs_No2)).Append(KvRow("Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1)).Append(KvRow("Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2));
            sb.Append(@"</table></td></tr>");
            // Fuel Consumption full table (title rendered inside generated table per spec)
            {
                decimal vlsfoTot = GetFuelConsByType(dtFuelCons, "VLSFO"), mdoTot = GetFuelConsByType(dtFuelCons, "MDO");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;"">").Append(BuildFuelConsFullTable(r, vlsfoTot, mdoTot, dtFuelCons)).Append(@"</td></tr>");
            }
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:25%;min-width:140px""><col style=""width:12%;min-width:80px""><col style=""width:13%;min-width:90px""><col style=""width:13%;min-width:90px""><col style=""width:12%;min-width:80px""><col style=""width:17%;min-width:100px""><col style=""width:8%;min-width:60px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">B/L QTY</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Load Portal Actual</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Today's Actual</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">QTY diff</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Reason for QTY diff</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Cargo Temp.</td></tr>");
            if (dtNRCargo != null) foreach (DataRow dr in dtNRCargo.Rows)
            {
                string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                string lbl = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(lbl).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargo(dr, "BL_Qty")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargo(dr, "LoadPortalActual")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargo(dr, "TodaysActual")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargo(dr, "Qty_Diff")).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(V(dr.Table.Columns.Contains("Reasonfor_Qty_Diff") ? dr["Reasonfor_Qty_Diff"] : null)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargo(dr, "Cargo_Temp")).Append(@"</td></tr>");
            }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Total)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            // E/R Tanks
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">E/R Tanks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Bilge</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Sludge</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Waste Oil</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.ER_Bilge_ROB)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.ER_Sludge_ROB)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.ER_WasteOil_ROB)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Void Spaces Soundings in mtrs</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            foreach (var vs in voidSpaces ?? new List<DNR_Void_Space>())
                sb.Append(KvRow(vs.TankName ?? "", vs.Sounding));
            sb.Append(@"</table></td></tr>");
            // Other Soundings in mtrs
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other Soundings in mtrs</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Pump Room bilge max sounding", r.PumpRoomMaxSounding)).Append(KvRow("Chain Locker 1", r.ChainLocker1)).Append(KvRow("Chain Locker 2", r.ChainLocker2));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fresh Water</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("FW Generated (MT)", r.FW_Generated)).Append(KvRow("Consumption (MT)", r.FW_Consumption)).Append(KvRow("ROB (MT)", r.FW_ROB));
            sb.Append(@"</table></td></tr>");
            var cTanks = cargoTanks ?? new List<DNR_Cargo_Tank>();
            if (cTanks.Count > 0)
            {
                sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo Tanks</td></tr>");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;font-size:11px;"">");
                sb.Append(@"<tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Tank</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:center;"">").Append(V(t.TankName)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Ullage (mtrs)</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Ullage)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">MT Qty</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Qty_MT)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Oxygen (% Volume)</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Oxygen)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">H2S (PPM)</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.H2S)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">HC (% Volume)</td>");
                foreach (var t in cTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.HC)).Append(@"</td>");
                sb.Append(@"</tr></table></td></tr>");
            }
            var bTanks = ballastTanks ?? new List<DNR_Ballast_Tank>();
            if (bTanks.Count > 0)
            {
                sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast Tanks</td></tr>");
                sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;font-size:11px;"">");
                sb.Append(@"<tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Tank</td>");
                foreach (var t in bTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:center;"">").Append(V(t.TankName)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Sounding (mtrs)</td>");
                foreach (var t in bTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Sounding)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">Cubic Vol</td>");
                foreach (var t in bTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.Qty_Vol)).Append(@"</td>");
                sb.Append(@"</tr><tr><td style=""padding:4px 6px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;"">HC (% Volume)</td>");
                foreach (var t in bTanks) sb.Append(@"<td style=""padding:4px 6px;border:1px solid #ccc;white-space:normal;word-wrap:break-word;overflow-wrap:break-word;text-align:right;"">").Append(V(t.HC)).Append(@"</td>");
                sb.Append(@"</tr></table></td></tr>");
            }
            sb.Append(@"</table>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>");
            sb.Append(@"<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>");
            sb.Append(@"<p style=""margin:12px 0 0 0;"">Thank you,</p><p style=""margin:4px 0 0 0;"">Team SIS</p></body></html>");
            return sb.ToString();
        }

        /// <summary>Single label-value row for 2-column layout. Label 45% min 280px, value 55%. Same as Arrival Report.</summary>
        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }

        // Shared inline-style fragments for the Fuel Consumption section (email-safe; CSS is inlined per cell).
        // Spec: font Arial 12px #222; tables 100% width, border-collapse, margin-bottom 14px;
        // th/td border 1px solid #bbb, padding 5px 9px, text-align center;
        // th bg #f0f0f0, font-weight 600, font-size 11px;
        // td.lbl: text-align left, font-weight 600, bg #f7f7f7;
        // tr.grp: bg #e8e8e8, font-weight 700, font-size 11px, letter-spacing 0.03em;
        // td.total: font-weight 700.
        private const string FC_FONT  = "font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TBL   = "width:100%;border-collapse:collapse;margin-bottom:14px;font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TD    = "border:1px solid #bbb;padding:5px 9px;text-align:center;";
        private const string FC_TH    = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#f0f0f0;font-weight:600;font-size:11px;";
        private const string FC_LBL   = "border:1px solid #bbb;padding:5px 9px;text-align:left;font-weight:600;background:#f7f7f7;";
        private const string FC_GRP   = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#e8e8e8;font-weight:700;font-size:11px;letter-spacing:0.03em;";
        private const string FC_TOTAL = "border:1px solid #bbb;padding:5px 9px;text-align:center;font-weight:700;";

        private static string FcFmt(decimal v) => v.ToString("0.##########");

        /// <summary>Renders an engine-style table: group header row + Fuel/At Sea/Manoeuv./Anchor-Wait/Berth columns,
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
              .Append("<th style=\"").Append(FC_TH).Append("\">Manoeuv.</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">ANCHOR/WAIT</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">BERTH</th>");
            if (showSubTotal) s.Append("<th style=\"").Append(FC_TH).Append("\">SUB TOTAL</th>");
            s.Append("</tr>");
            // VLSFO
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vSea)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vMan)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vWait)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vBerth)).Append("</td>");
            if (showSubTotal) s.Append("<td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(FcFmt(vSea + vMan + vWait + vBerth)).Append("</td>");
            s.Append("</tr>");
            // MDO
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

        /// <summary>Renders the IGG &amp; Incinerator table: group header + Fuel/IGG/Incinerator columns.</summary>
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
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vIgg)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vInc)).Append("</td>")
              .Append("</tr>");
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dIgg)).Append("</td>")
              .Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dInc)).Append("</td>")
              .Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Renders the Events table: group header + Fuel + 8 event columns.</summary>
        private static string FuelConsEventsTable(DataTable dtFuelCons)
        {
            // Events ConsTypeIds: 20=Stoppage, 21=Deviation, 22=SlowSteaming, 23=BadWeather,
            // 24=COTPrep, 25=CargoHeating, 26=BWExchange, 27=Others
            int[] eventConsTypeIds = { 20, 21, 22, 23, 24, 25, 26, 27 };
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"9\" style=\"").Append(FC_GRP).Append("\">Events</td></tr>");
            s.Append("<tr>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Fuel</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Stoppage</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Deviation</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Slow Stm.</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Bad Wthr.</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">COT Prep</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">Cargo Htg.</th>")
              .Append("<th style=\"").Append(FC_TH).Append("\">BW Exch.</th>")
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

        /// <summary>Renders the Total summary table: group header + two rows (VLSFO Total, MDO Total) each with " MT" suffix.</summary>
        private static string FuelConsTotalTable(decimal vlsfoTotal, decimal mdoTotal)
        {
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"2\" style=\"").Append(FC_GRP).Append("\">Total</td></tr>");
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO TOTAL</td>")
              .Append("<td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(FcFmt(vlsfoTotal)).Append(" MT</td>")
              .Append("</tr>");
            s.Append("<tr>")
              .Append("<td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO TOTAL</td>")
              .Append("<td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(FcFmt(mdoTotal)).Append(" MT</td>")
              .Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        /// <summary>Returns the decimal value for the row matching (fuelType, consTypeId) in dtFuelCons, or 0 if not found.</summary>
        private static decimal GetFuelConsByConsType(DataTable dt, string fuelType, int consTypeId)
        {
            if (dt == null) return 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (!(dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase)) continue;
                int rowConsType;
                if (!int.TryParse(dr["ConsTypeId"]?.ToString() ?? "", out rowConsType)) continue;
                if (rowConsType != consTypeId) continue;
                var v = dr["Value"];
                if (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out decimal d)) return d;
                return 0;
            }
            return 0;
        }

        /// <summary>Builds the Fuel Consumption in MT HTML block: centered section title followed by 7 tables
        /// (Main Engine, Aux Engine, Boiler, FRAMO System, IGG &amp; Incinerator, Events, Total) per spec.
        /// Reads breakdown values from dtFuelCons keyed by ConsTypeId (matches save mapping in DailyNoonController):
        /// 1=ME_CP, 2=ME_SEA, 3=ME_MAN, 4=ME_WAIT, 5=ME_BERTH,
        /// 6=AE_CP, 7=AE_SEA, 8=AE_MAN, 9=AE_WAIT, 10=AE_BERTH,
        /// 11-14=BLR(SEA/MAN/WAIT/BERTH), 15-18=FRAMO(SEA/MAN/WAIT/BERTH),
        /// 19=IGG, 20=Stoppage, 21=Deviation, 22=SlowSteaming, 23=BadWeather,
        /// 24=COTPrep, 25=CargoHeating, 26=BWExchange, 27=Others, 28=Incinerator
        /// </summary>
        private static string BuildFuelConsFullTable(DailyNoonReport r, decimal vlsfoTotal, decimal mdoTotal, DataTable dtFuelCons)
        {
            decimal FV(string ft, int consTypeId) => GetFuelConsByConsType(dtFuelCons, ft, consTypeId);

            var sb = new StringBuilder();
            sb.Append("<div style=\"").Append(FC_FONT).Append("\">");

            // Section title: dark-grey banner matching the other section headers (e.g. "Boiler's").
            sb.Append("<div style=\"padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;font-size:14px;margin-bottom:16px;\">Fuel Consumption in MT</div>");

            // 1. Main Engine — ConsTypeId 2..5 (Sea/Man/Wait/Berth), with Sub Total
            sb.Append(FuelConsEngineTable("Main Engine", true,
                FV("VLSFO", 2), FV("VLSFO", 3), FV("VLSFO", 4), FV("VLSFO", 5),
                FV("MDO",   2), FV("MDO",   3), FV("MDO",   4), FV("MDO",   5)));

            // 2. Aux Engine — ConsTypeId 7..10, with Sub Total
            sb.Append(FuelConsEngineTable("Aux Engine", true,
                FV("VLSFO", 7), FV("VLSFO", 8), FV("VLSFO", 9), FV("VLSFO", 10),
                FV("MDO",   7), FV("MDO",   8), FV("MDO",   9), FV("MDO",   10)));

            // 3. Boiler — ConsTypeId 11..14, no Sub Total
            sb.Append(FuelConsEngineTable("Boiler", false,
                FV("VLSFO", 11), FV("VLSFO", 12), FV("VLSFO", 13), FV("VLSFO", 14),
                FV("MDO",   11), FV("MDO",   12), FV("MDO",   13), FV("MDO",   14)));

            // 4. FRAMO System — ConsTypeId 15..18, no Sub Total
            sb.Append(FuelConsEngineTable("Framo System", false,
                FV("VLSFO", 15), FV("VLSFO", 16), FV("VLSFO", 17), FV("VLSFO", 18),
                FV("MDO",   15), FV("MDO",   16), FV("MDO",   17), FV("MDO",   18)));

            // 5. IGG & Incinerator — IGG=ConsTypeId 19, Incinerator=ConsTypeId 28
            sb.Append(FuelConsIggIncTable(FV("VLSFO", 19), FV("VLSFO", 28), FV("MDO", 19), FV("MDO", 28)));

            // 6. Events — ConsTypeId 20..27
            sb.Append(FuelConsEventsTable(dtFuelCons));

            // 7. Total summary
            sb.Append(FuelConsTotalTable(vlsfoTotal, mdoTotal));

            sb.Append("</div>");
            return sb.ToString();
        }

        private static string FormatCargo(DataRow dr, string col)
        {
            if (!dr.Table.Columns.Contains(col)) return "-";
            var v = dr[col];
            if (v == null || v == DBNull.Value) return "-";
            decimal d;
            return decimal.TryParse(v.ToString(), out d) ? d.ToString("0.##########") : V(v);
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
    }
}
