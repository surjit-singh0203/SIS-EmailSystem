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
    /// Builds HTML email body for Departure Report. Uses DepartureReport.html template when available.
    /// Same design as Arrival Report and Daily Noon Report.
    /// </summary>
    public static class DepartureReportEmailTemplate
    {
        private const string DateFormat = "dd-MMM-yyyy";
        private const string DateTimeFormat = "dd-MMM-yyyy HH:mm";
        private const string TemplatePath = "~/Templates/DepartureReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

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

        /// <summary>Resolve "LegPort_A to LegPort_B" scoped by VoyageId + VesselId. Tries DepLegPortId first, then NextLegPortId, then the first active leg for the voyage.</summary>
        private static string LookupLeg(int depLegPortId, int nextLegPortId, int voyageId, int vesselId)
        {
            try
            {
                foreach (int legId in new[] { depLegPortId, nextLegPortId })
                {
                    if (legId <= 0) continue;
                    using (var adp = new SqlDataAdapter(
                        "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + legId + " and VoyageId=" + voyageId + " and VesselId=" + vesselId, ConnectionBulder.con))
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

        /// <param name="reportId">When set (from export filename R{id}), matches Excel save fallback via editdepartureRList when dashboard date lookup returns no row.</param>
        public static string BuildHtml(int vesselId, string datePart, int? reportId = null)
        {
            string reportdate = DatePartToReportDate(datePart);
            if (string.IsNullOrEmpty(reportdate)) return null;

            var vd = new DepartureReport();
            DepartureReport depRBind = null;
            if (reportId.HasValue && reportId.Value > 0)
            {
                vd.GetDepRList = CommonMethods.editdepartureRList(reportId.Value, vesselId, "DepartureReport");
                depRBind = vd.GetDepRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (depRBind == null)
            {
                vd.GetDepRList = CommonMethods.editdepartureRListDashboard(reportdate, vesselId, "DepartureReport");
                depRBind = vd.GetDepRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (depRBind == null)
            {
                vd.GetDepRList = CommonMethods.editdepartureRListDashboard(reportdate, vesselId, "DepartureReportR");
                depRBind = vd.GetDepRList?.Where(x => x.Id > 0).FirstOrDefault();
            }
            if (depRBind == null) return null;

            int id = depRBind.Id;

            // Backfill LO/HO + Cargo_Temp from DepartureReport table directly because
            // spCommonEditListReport aliases these columns to short names (AECC_ROB, MECC_ROB, ...)
            // that DataTableToList cannot map to the long-named model properties (LO_HO_Cons_AECC_ROB, ...).
            try
            {
                using (var adp = new SqlDataAdapter(
                    "select LO_HO_Cons_MECC, LO_HO_Cons_MECC_ROB, LO_HO_Cons_MECYL, LO_HO_Cons_MECYL_ROB, " +
                    "LO_HO_Cons_AECC, LO_HO_Cons_AECC_ROB, LO_HO_Cons_HYDR_Oil, LO_HO_Cons_HYDR_Oil_ROB, " +
                    "Cargo_Temp from DepartureReport where Id=" + id, ConnectionBulder.con))
                {
                    var dtBackfill = new DataTable();
                    adp.Fill(dtBackfill);
                    if (dtBackfill.Rows.Count > 0)
                    {
                        var br = dtBackfill.Rows[0];
                        if (br["LO_HO_Cons_MECC"] != DBNull.Value) depRBind.LO_HO_Cons_MECC = Convert.ToDecimal(br["LO_HO_Cons_MECC"]);
                        if (br["LO_HO_Cons_MECC_ROB"] != DBNull.Value) depRBind.LO_HO_Cons_MECC_ROB = Convert.ToDecimal(br["LO_HO_Cons_MECC_ROB"]);
                        if (br["LO_HO_Cons_MECYL"] != DBNull.Value) depRBind.LO_HO_Cons_MECYL = Convert.ToDecimal(br["LO_HO_Cons_MECYL"]);
                        if (br["LO_HO_Cons_MECYL_ROB"] != DBNull.Value) depRBind.LO_HO_Cons_MECYL_ROB = Convert.ToDecimal(br["LO_HO_Cons_MECYL_ROB"]);
                        if (br["LO_HO_Cons_AECC"] != DBNull.Value) depRBind.LO_HO_Cons_AECC = Convert.ToDecimal(br["LO_HO_Cons_AECC"]);
                        if (br["LO_HO_Cons_AECC_ROB"] != DBNull.Value) depRBind.LO_HO_Cons_AECC_ROB = Convert.ToDecimal(br["LO_HO_Cons_AECC_ROB"]);
                        if (br["LO_HO_Cons_HYDR_Oil"] != DBNull.Value) depRBind.LO_HO_Cons_HYDR_Oil = Convert.ToDecimal(br["LO_HO_Cons_HYDR_Oil"]);
                        if (br["LO_HO_Cons_HYDR_Oil_ROB"] != DBNull.Value) depRBind.LO_HO_Cons_HYDR_Oil_ROB = Convert.ToDecimal(br["LO_HO_Cons_HYDR_Oil_ROB"]);
                        if (br["Cargo_Temp"] != DBNull.Value) depRBind.Cargo_Temp = Convert.ToDecimal(br["Cargo_Temp"]);
                    }
                }
            }
            catch { }

            DataTable dtFuelCons = new DataTable(), dtFuelROB = new DataTable(), dtBunker = new DataTable();
            DataTable dtNonRoutine = new DataTable(), dtMain = new DataTable(), dtDRCargo = new DataTable();

            try
            {
                using (var adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                    adp.Fill(dtFuelCons);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.EOSP as SBE, a.FWE as RFA from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3", ConnectionBulder.con))
                    adp.Fill(dtFuelROB);
                using (var adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3", ConnectionBulder.con))
                    adp.Fill(dtBunker);
                using (var adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=3 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                    adp.Fill(dtNonRoutine);
                try
                {
                    using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from DR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.depreport_id=" + id, ConnectionBulder.con))
                        adp.Fill(dtDRCargo);
                }
                catch
                {
                    try
                    {
                        using (var adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from DR_Cargo a inner join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.depreport_id=" + id, ConnectionBulder.con))
                            adp.Fill(dtDRCargo);
                    }
                    catch { }
                }
                using (var cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoyageId", depRBind.VoyageId);
                    cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId);
                    cmd.Parameters.AddWithValue("@Action", "DepartureReport");
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
                return ApplyTemplate(template, depRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtDRCargo);
            }
            return BuildHtmlInline(depRBind, vesselName, dtNonRoutine, dtMain, dtFuelCons, dtFuelROB, dtBunker, dtDRCargo);
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

        private static string ApplyTemplate(string template, DepartureReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtDRCargo)
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
            if (string.IsNullOrWhiteSpace(legText)) legText = LookupLeg(r.DepLegPortId, r.NextLegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Departure Port", r.DeparturePort)).Append(KvRow("Next Port", r.NextPort));
            sb.Append(KvRow("Report Date", V(r.ReportDate))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            sb.Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("RFA Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
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
            sb.Append(KvRow("MECC Consumption (Ltrs)", r.LO_HO_Cons_MECC)).Append(KvRow("MECC ROB (Ltrs)", r.LO_HO_Cons_MECC_ROB));
            sb.Append(KvRow("MECYL Consumption (Ltrs)", r.LO_HO_Cons_MECYL)).Append(KvRow("MECYL ROB (Ltrs)", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AECC ROB (Ltrs)", r.LO_HO_Cons_AECC_ROB));
            sb.Append(KvRow("Hydraulic Oil ROB (Ltrs)", r.LO_HO_Cons_HYDR_Oil_ROB));
            string engineRows = sb.ToString();
            sb.Clear();

            decimal vlsfoTotal = 0m, mdoTotal = 0m;
            if (dtFuelCons != null)
            {
                foreach (DataRow drc in dtFuelCons.Rows)
                {
                    string ft = drc["FuelType"]?.ToString() ?? "";
                    decimal val; decimal.TryParse(drc["Value"]?.ToString(), out val);
                    if (ft.Equals("VLSFO", StringComparison.OrdinalIgnoreCase)) vlsfoTotal += val;
                    else if (ft.Equals("MDO", StringComparison.OrdinalIgnoreCase)) mdoTotal += val;
                }
            }

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
            sb.Append(KvRow("VLSFO Total Consumption (MT)", (decimal?)vlsfoTotal));
            sb.Append(KvRow("MDO Total Consumption (MT)", (decimal?)mdoTotal));
            string fuelRobRows = sb.ToString();
            sb.Clear();

            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
                    sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
            }
            string bunkerRows = sb.ToString();
            sb.Clear();

            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;"">Cargo Temp.</td><td colspan=""3"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(V((decimal?)r.Cargo_Temp)).Append(@"</td></tr>");
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    string bl = FormatCargoVal(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    string lpa = FormatCargoVal(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(bl).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(lpa).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null)).Append(@"</td></tr>");
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
                .Replace("{{REPORT_DATE}}", V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{MANOEUVRING_ROWS}}", manoeuvringRows)
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
                .Replace("{{CARGO_ROWS}}", cargoRows)
                .Replace("{{SLOPS_DISPOSED_Oil}}", V(r.SlopsDisposed_Oil))
                .Replace("{{SLOPS_DISPOSED_Water}}", V(r.SlopsDisposed_Water))
                .Replace("{{SLOPS_DISPOSED_Total}}", V(r.SlopsDisposed_Total))
                .Replace("{{SLOPS_ROB_Oil}}", V(r.SlopsROB_Oil))
                .Replace("{{SLOPS_ROB_Water}}", V(r.SlopsROB_Water))
                .Replace("{{SLOPS_ROB_Total}}", V(r.SlopsROB_Total))
                .Replace("{{BALLAST_ROW}}", ballastRow)
                .Replace("{{FRESH_WATER_ROWS}}", freshWaterRows);
        }

        private static string BuildHtmlInline(DepartureReport r, string vesselName, DataTable dtNonRoutine, DataTable dtMain, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker, DataTable dtDRCargo)
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
            if (string.IsNullOrWhiteSpace(legText)) legText = LookupLeg(r.DepLegPortId, r.NextLegPortId, r.VoyageId, r.VesselId);

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Departure Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Departure Report (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Voy No.", voyNo)).Append(KvRow("Departure Port", r.DeparturePort)).Append(KvRow("Next Port", r.NextPort)).Append(KvRow("Report Date", V(r.ReportDate))).Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd)).Append(KvRow("Draft Mid (Mtrs)", r.DraftMid)).Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Manoeuvring & SBE/RFA</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Manoeuvring Hrs", r.Manoeuvring_Hrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance)).Append(KvRow("SBE Date & Time", r.SBE_DateT != null ? Vdt(r.SBE_DateT) : "-")).Append(KvRow("RFA Date & Time", r.RFA_DateT != null ? Vdt(r.RFA_DateT) : "-"));
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
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce)).Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight)).Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Departure Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:4px;border:1px solid #ccc;"">").Append(r.Remarks ?? "-").Append(@"</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", r.Slip)).Append(KvRow("RPM", r.RPM)).Append(KvRow("BHP(hp)", r.BHP)).Append(KvRow("MCR%", r.MCR));
            sb.Append(KvRow("MECC Consumption (Ltrs)", r.LO_HO_Cons_MECC)).Append(KvRow("MECC ROB (Ltrs)", r.LO_HO_Cons_MECC_ROB));
            sb.Append(KvRow("MECYL Consumption (Ltrs)", r.LO_HO_Cons_MECYL)).Append(KvRow("MECYL ROB (Ltrs)", r.LO_HO_Cons_MECYL_ROB));
            sb.Append(KvRow("AECC ROB (Ltrs)", r.LO_HO_Cons_AECC_ROB));
            sb.Append(KvRow("Hydraulic Oil ROB (Ltrs)", r.LO_HO_Cons_HYDR_Oil_ROB));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT (SBE/RFA)</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            decimal vlsfoTotalInline = 0m, mdoTotalInline = 0m;
            if (dtFuelCons != null)
            {
                foreach (DataRow drc in dtFuelCons.Rows)
                {
                    string ft = drc["FuelType"]?.ToString() ?? "";
                    decimal val; decimal.TryParse(drc["Value"]?.ToString(), out val);
                    if (ft.Equals("VLSFO", StringComparison.OrdinalIgnoreCase)) vlsfoTotalInline += val;
                    else if (ft.Equals("MDO", StringComparison.OrdinalIgnoreCase)) mdoTotalInline += val;
                }
            }
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string sbe = dr.Table.Columns.Contains("SBE") ? dr["SBE"]?.ToString() : "";
                    string rfa = dr.Table.Columns.Contains("RFA") ? dr["RFA"]?.ToString() : "";
                    sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", string.IsNullOrEmpty(sbe) && string.IsNullOrEmpty(rfa) ? "-" : (sbe ?? "-") + " / " + (rfa ?? "-")));
                }
            }
            sb.Append(KvRow("VLSFO Total Consumption (MT)", (decimal?)vlsfoTotalInline));
            sb.Append(KvRow("MDO Total Consumption (MT)", (decimal?)mdoTotalInline));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Bunker Received in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            if (dtBunker != null) foreach (DataRow dr in dtBunker.Rows) sb.Append(KvRow(dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "-"));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">B/L QTY</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Load Portal Actual</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Cargo Temp.</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo Temp.</td><td colspan=""3"" style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V((decimal?)r.Cargo_Temp)).Append(@"</td></tr>");
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(FormatCargoVal(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null)).Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops Disposed / ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Disposed (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsDisposed_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsDisposed_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsDisposed_Total)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">ROB (m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V(r.SlopsROB_Total)).Append(@"</td></tr>");
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

        private static string KvRow(string label, object value)
        {
            string v = (value is decimal || value is decimal?) ? V((decimal?)value) : (value is DateTime || value is DateTime?) ? V((DateTime?)value) : V(value);
            return @"<tr><td class=""col-label"" style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;vertical-align:middle;white-space:nowrap;"">" + (label ?? "") + @"</td><td class=""col-value"" style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">" + v + @"</td></tr>";
        }

        private static string FormatCargoVal(object v)
        {
            if (v == null || v == DBNull.Value) return "-";
            decimal d;
            return decimal.TryParse(v.ToString(), out d) ? d.ToString("0.00") : V(v);
        }
    }
}
