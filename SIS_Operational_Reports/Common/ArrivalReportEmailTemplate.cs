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
    /// Builds HTML email body for Arrival Report. Uses ArrivalReport.html template when available.
    /// </summary>
    public static class ArrivalReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/ArrivalReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        /// <summary>Format decimal: whole numbers (e.g. IDs, bottle counts) stay as-is; fractional values use 0.000.</summary>
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

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
                    using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from AR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.arrivalreport_id=" + id, ConnectionBulder.con))
                        adp.Fill(dtARCargo);
                }
                catch
                {
                    try
                    {
                        using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from AR_Cargo a inner join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.arrivalreport_id=" + id, ConnectionBulder.con))
                            adp.Fill(dtARCargo);
                    }
                    catch { }
                }
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
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg")) legText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Latitude", r.Latitude)).Append(KvRow("Longitude", r.Longitude));
            sb.Append(KvRow("Place", r.Place)).Append(KvRow("Leg", legText)).Append(KvRow("NOR", r.NOR != null ? Vdt(r.NOR) : "-")).Append(KvRow("Port", r.PortName));
            sb.Append(KvRow("EOSP", r.EOSP != null ? Vdt(r.EOSP) : "-")).Append(KvRow("ETB", r.ETB != null ? Vdt(r.ETB) : "-"));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Anchorage Name", r.Anchor_Name)).Append(KvRow("Drop Anchor Date & Time", r.Anchor_DateT != null ? Vdt(r.Anchor_DateT) : "-"));
            sb.Append(KvRow("Anchoring Position - Latitude", r.AnchorPos_Latitude)).Append(KvRow("Anchoring Position - Longitude", r.AnchorPos_Longitude));
            sb.Append(KvRow("FWE Date & Time", r.AnchorFWE_DateT != null ? Vdt(r.AnchorFWE_DateT) : "-"));
            string anchorageRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG));
            sb.Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime)).Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            sb.Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
            string speedRows = sb.ToString();
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

            sb.Append(KvRow("EOSP ROB", r.EOSP_ROB)).Append(KvRow("FWE ROB", r.FWE_ROB));
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string eosp = dr.Table.Columns.Contains("EOSP") ? dr["EOSP"]?.ToString() : "";
                    string fwe = dr.Table.Columns.Contains("FWE") ? dr["FWE"]?.ToString() : "";
                    string robVal = string.IsNullOrEmpty(eosp) && string.IsNullOrEmpty(fwe) ? "-" : (eosp ?? "-") + " / " + (fwe ?? "-");
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

            decimal vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
            decimal mdo = GetFuelConsByType(dtFuelCons, "MDO");
            sb.Append(KvRow("VLSFO (Total)", vlsfo > 0 ? vlsfo.ToString("0.000") : "-")).Append(KvRow("MDO (Total)", mdo > 0 ? mdo.ToString("0.000") : "-"));
            string fuelConsRows = sb.ToString();
            sb.Clear();

            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    string q1 = FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null);
                    string q2 = FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade2") ? dr["Qty_Grade2"] : null);
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(q1).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(q2).Append(@"</td></tr>");
                }
            }
            string cargoRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
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
                .Replace("{{FUEL_CONS_ROWS}}", fuelConsRows)
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{SLOPS_ROB_OXY_Oil}}", V(r.SLOPS_ROB_OXY_Oil))
                .Replace("{{SLOPS_ROB_OXY_Water}}", V(r.SLOPS_ROB_OXY_Water))
                .Replace("{{SLOPS_ROB_OXY_Total}}", V(r.SLOPS_ROB_OXY_Total))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(ArrivalReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtARCargo)
        {
            string voyNo = r.voyagenumber ?? r.VoyageId.ToString();
            string legText = "";
            if (dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg")) legText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Arrival Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.NOR ?? r.EOSP)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Arrival Report (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(CompactRow("Voy No.", voyNo, "Latitude", r.Latitude, "Longitude", r.Longitude));
            sb.Append(CompactRow("Place", r.Place, "Leg", legText));
            sb.Append(CompactRow("NOR", r.NOR != null ? Vdt(r.NOR) : "-", "Port", r.PortName));
            sb.Append(CompactRow("EOSP", r.EOSP != null ? Vdt(r.EOSP) : "-", "ETB", r.ETB != null ? Vdt(r.ETB) : "-"));
            sb.Append(CompactRow("Draft Fwd (Mtrs)", r.DraftFwd, "Draft Mid (Mtrs)", r.DraftMid, "Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Anchorage</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Anchorage Name", r.Anchor_Name)).Append(KvRow("Drop Anchor Date & Time", r.Anchor_DateT != null ? Vdt(r.Anchor_DateT) : "-"));
            sb.Append(KvRow("Anchoring Position - Latitude", r.AnchorPos_Latitude)).Append(KvRow("Anchoring Position - Longitude", r.AnchorPos_Longitude)).Append(KvRow("FWE Date & Time", r.AnchorFWE_DateT != null ? Vdt(r.AnchorFWE_DateT) : "-"));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Speed - Distance - Time</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist)).Append(KvRow("Log Dist(NM)", r.LogDist)).Append(KvRow("Engine Dist(NM)", r.EngineDist));
            sb.Append(KvRow("Total Distance (Dep to Curr)(NM)", r.TotalDistance)).Append(KvRow("Dist to Go (DTG)(NM)", r.DistToGo_DTG)).Append(KvRow("Stmg Time Noon to Noon(Hrs)", r.StmgTime)).Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance)).Append(KvRow("Actual Speed Noon to Noon(Kts)", r.Act_Speed)).Append(KvRow("Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed));
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
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB (EOSP/FWE)</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("EOSP ROB", r.EOSP_ROB)).Append(KvRow("FWE ROB", r.FWE_ROB));
            if (dtFuelROB != null) { foreach (DataRow dr in dtFuelROB.Rows) { string eosp = dr.Table.Columns.Contains("EOSP") ? dr["EOSP"]?.ToString() : ""; string fwe = dr.Table.Columns.Contains("FWE") ? dr["FWE"]?.ToString() : ""; sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", string.IsNullOrEmpty(eosp) && string.IsNullOrEmpty(fwe) ? "-" : (eosp ?? "-") + " / " + (fwe ?? "-"))); } }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtBunker != null) { foreach (DataRow dr in dtBunker.Rows) sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-")); }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel Consumption</td></tr>");
            decimal vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
            decimal mdo = GetFuelConsByType(dtFuelCons, "MDO");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("VLSFO (Total)", vlsfo > 0 ? vlsfo.ToString("0.000") : "-")).Append(KvRow("MDO (Total)", mdo > 0 ? mdo.ToString("0.000") : "-"));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:27%;min-width:150px""><col style=""width:28%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Qty Grade 1</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Qty Grade 2</td></tr>");
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade2") ? dr["Qty_Grade2"] : null)).Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SLOPS_ROB_OXY_Total)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Ballast</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("ROB (MT)", r.Ballast_ROB));
            sb.Append(@"</table></td></tr>");
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
            return decimal.TryParse(v.ToString(), out d) ? d.ToString("0.00") : V(v);
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
