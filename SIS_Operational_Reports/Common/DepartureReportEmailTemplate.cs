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
        private const string DateFormat = "yyyy-MM-dd";
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        private const string TemplatePath = "~/Templates/DepartureReport.html";

        private static string V(object o) => o == null || o == DBNull.Value || string.IsNullOrWhiteSpace(o.ToString()) ? "-" : o.ToString().Trim();
        private static string V(decimal? d) => d.HasValue ? (d.Value == Math.Truncate(d.Value) ? d.Value.ToString("0") : d.Value.ToString("0.000")) : "-";
        private static string V(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateFormat) : "-";
        private static string Vdt(DateTime? dt) => dt.HasValue ? dt.Value.ToString(DateTimeFormat) : "-";

        /// <summary>Strip trailing zeros from a numeric value/string. Used for fields where the
        /// web view shows the number with no decimal padding (e.g. NRE Hrs: 0 not 0.000).
        /// Returns "-" for null/empty, raw trimmed string for unparseable input.</summary>
        private static string Num(object o)
        {
            if (o == null || o == DBNull.Value) return "-";
            string s = o.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return "-";
            if (decimal.TryParse(s, out decimal d)) return d.ToString("0.##########");
            return s;
        }

        /// <summary>Always-3-decimal formatter for Engine/LO&amp;HO/Slops/Bilge/Fuel ROB cells,
        /// matching the web view layout where 0 renders as "0.000" and 757 as "757.000".</summary>
        private static string V3(decimal? d) => d.HasValue ? d.Value.ToString("0.000") : "-";

        /// <summary>Same as V3 but accepts boxed DataRow values (decimal/int/string).</summary>
        private static string V3(object o)
        {
            if (o == null || o == DBNull.Value) return "-";
            if (decimal.TryParse(o.ToString(), out decimal d)) return d.ToString("0.000");
            return "-";
        }

        // Shared inline-style fragments for the Fuel Consumption section (email-safe; CSS is
        // inlined per cell). Mirrors the Daily Noon / Arrival templates.
        private const string FC_FONT  = "font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TBL   = "width:100%;border-collapse:collapse;margin-bottom:14px;font-family:Arial,sans-serif;font-size:12px;color:#222;";
        private const string FC_TD    = "border:1px solid #bbb;padding:5px 9px;text-align:center;";
        private const string FC_TH    = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#f0f0f0;font-weight:600;font-size:11px;";
        private const string FC_LBL   = "border:1px solid #bbb;padding:5px 9px;text-align:left;font-weight:600;background:#f7f7f7;";
        private const string FC_GRP   = "border:1px solid #bbb;padding:5px 9px;text-align:center;background:#e8e8e8;font-weight:700;font-size:11px;letter-spacing:0.03em;";
        private const string FC_TOTAL = "border:1px solid #bbb;padding:5px 9px;text-align:center;font-weight:700;";

        private static string FcFmt(decimal v) => v.ToString("0.000");

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

        private static string FuelConsIggIncTable(decimal vIgg, decimal vInc, decimal dIgg, decimal dInc)
        {
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"3\" style=\"").Append(FC_GRP).Append("\">IGG &amp; Incinerator</td></tr>");
            s.Append("<tr><th style=\"").Append(FC_TH).Append("\">Fuel</th><th style=\"").Append(FC_TH).Append("\">IGG</th><th style=\"").Append(FC_TH).Append("\">Incinerator</th></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vIgg)).Append("</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(vInc)).Append("</td></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dIgg)).Append("</td><td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(dInc)).Append("</td></tr>");
            s.Append("</table>");
            return s.ToString();
        }

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
            foreach (int ct in eventConsTypeIds) s.Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(GetFuelConsByConsType(dtFuelCons, "VLSFO", ct))).Append("</td>");
            s.Append("</tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO</td>");
            foreach (int ct in eventConsTypeIds) s.Append("<td style=\"").Append(FC_TD).Append("\">").Append(FcFmt(GetFuelConsByConsType(dtFuelCons, "MDO", ct))).Append("</td>");
            s.Append("</tr>");
            s.Append("</table>");
            return s.ToString();
        }

        private static string FuelConsTotalTable(decimal vlsfoTotal, decimal mdoTotal)
        {
            var s = new StringBuilder();
            s.Append("<table style=\"").Append(FC_TBL).Append("\">");
            s.Append("<tr class=\"grp\"><td colspan=\"2\" style=\"").Append(FC_GRP).Append("\">Total</td></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">VLSFO TOTAL</td><td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(vlsfoTotal.ToString("0.000")).Append("</td></tr>");
            s.Append("<tr><td class=\"lbl\" style=\"").Append(FC_LBL).Append("\">MDO TOTAL</td><td class=\"total\" style=\"").Append(FC_TOTAL).Append("\">").Append(mdoTotal.ToString("0.000")).Append("</td></tr>");
            s.Append("</table>");
            return s.ToString();
        }

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

        private static decimal GetFuelConsTotal(DataTable dt, string fuelType)
        {
            if (dt == null) return 0;
            decimal sum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (!(dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase)) continue;
                var v = dr["Value"];
                if (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out decimal d)) sum += d;
            }
            return sum;
        }

        /// <summary>Full Fuel Consumption in MT block — same 7 sub-tables as Daily Noon / Arrival.
        /// ConsTypeId mapping: ME 2-5, AE 7-10, BLR 11-14, FRAMO 15-18, IGG 19, Events 20-27, Inc 28.</summary>
        private static string BuildFuelConsFullTable(DataTable dtFuelCons)
        {
            decimal FV(string ft, int consTypeId) => GetFuelConsByConsType(dtFuelCons, ft, consTypeId);
            decimal vlsfoTotal = GetFuelConsTotal(dtFuelCons, "VLSFO");
            decimal mdoTotal = GetFuelConsTotal(dtFuelCons, "MDO");
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

        /// <summary>Resolve "LegPort_A to LegPort_B" for a single VoyageLeg row scoped by
        /// Id + VoyageId + VesselId. Returns "" when nothing matches. Used to render the
        /// Dep leg and Next leg as separate header fields in the Departure email.</summary>
        private static string LookupSingleLeg(int legId, int voyageId, int vesselId)
        {
            if (legId <= 0) return "";
            try
            {
                using (var adp = new SqlDataAdapter(
                    "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + legId + " and VoyageId=" + voyageId + " and VesselId=" + vesselId, ConnectionBulder.con))
                {
                    var dt = new DataTable();
                    adp.Fill(dt);
                    if (dt.Rows.Count > 0) return dt.Rows[0]["Leg"]?.ToString() ?? "";
                }
            }
            catch { }
            return "";
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
            // Always use the inline HTML builder. The external Templates/DepartureReport.html
            // shipped with older deployments still contains stale placeholders for the prior
            // Engine layout, so the inline builder (which carries the 8-table structure)
            // guarantees the email renders correctly.
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

            // Resolve Dep and Next leg separately so the email shows them as two distinct
            // fields (Leg + Next Leg) matching the web view.
            string depLegText = LookupSingleLeg(r.DepLegPortId, r.VoyageId, r.VesselId);
            string nextLegText = LookupSingleLeg(r.NextLegPortId, r.VoyageId, r.VesselId);
            if (string.IsNullOrWhiteSpace(depLegText) && dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg"))
                depLegText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";

            // Header field order matches the web view exactly.
            var sb = new StringBuilder();
            sb.Append(KvRow("Voy No.", voyNo));
            sb.Append(KvRow("Leg", depLegText));
            sb.Append(KvRow("Dep. Port", r.DeparturePort));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd));
            sb.Append(KvRow("Next Leg", nextLegText));
            sb.Append(KvRow("Next Port", r.NextPort));
            sb.Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Mid (Mtrs)", r.DraftMid));
            sb.Append(KvRow("Report Date", V(r.ReportDate)));
            sb.Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            string headerRows = sb.ToString();
            sb.Clear();

            // Manoeuvring section — only Hours (2 decimals) and Distance, matching the web view.
            // SBE/RFA date fields are intentionally NOT rendered here.
            string mhrs = r.Manoeuvring_Hrs.HasValue ? r.Manoeuvring_Hrs.Value.ToString("0.00") : "-";
            sb.Append(KvRow("Manoeuvring Hours", mhrs)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
            string manoeuvringRows = sb.ToString();
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

            // Other Receipts, Repairs, Crew Change & Landed — 6 key-value rows in web view order.
            sb.Append(KvRow("CTM (Mention Currency & Amount)", r.CTM));
            sb.Append(KvRow("SPARES (Mention Revision Numbers)", r.Spares));
            sb.Append(KvRow("STORES (Mention Revision Numbers)", r.Stores));
            sb.Append(KvRow("REPAIRS CONDUCTED (Mention details)", r.RepairsConducted));
            sb.Append(KvRow("CREW CHANGE (No of Crew)", r.CrewChange));
            sb.Append(KvRow("ITEMS LANDED (Mention Details)", r.ItemsLanded));
            string otherReceiptsRows = sb.ToString();
            sb.Clear();

            // Departure Report Remarks rendered as a single labeled key-value row.
            string remarksRow = KvRow("Departure Report Remarks", r.Remarks);

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

            // Cargo rows for the template builder — 6-col matching the inline builder layout.
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    string bl = V3(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    string lpa = V3(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    string ctmp = Num(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
                    string comp = "-";
                    if (dr.Table.Columns.Contains("Completion_DateT") && dr["Completion_DateT"] != null && dr["Completion_DateT"] != DBNull.Value)
                    {
                        if (DateTime.TryParse(dr["Completion_DateT"].ToString(), out DateTime cd)) comp = cd.ToString(DateTimeFormat);
                    }
                    string rate = Num(dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null);
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(bl).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(lpa).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(ctmp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(comp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;vertical-align:middle;text-align:right;"">").Append(rate).Append(@"</td></tr>");
                }
            }
            string cargoRows = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("ROB", V3(r.Ballast_ROB)));
            string ballastRow = sb.ToString();
            sb.Clear();

            sb.Append(KvRow("FW Generated (MT)", V3(r.FW_Generated))).Append(KvRow("Consumption (MT)", V3(r.FW_Consumption))).Append(KvRow("ROB (MT)", V3(r.FW_ROB)));
            string freshWaterRows = sb.ToString();

            return template
                .Replace("{{VESSEL_NAME}}", V(vesselName))
                .Replace("{{REPORT_DATE}}", V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT))
                .Replace("{{HEADER_ROWS}}", headerRows)
                .Replace("{{MANOEUVRING_ROWS}}", manoeuvringRows)
                .Replace("{{NON_ROUTINE_ROWS}}", nonRoutineRows)
                .Replace("{{OTHER_RECEIPTS_ROWS}}", otherReceiptsRows)
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

            // Resolve Dep and Next leg separately for the two header fields.
            string depLegText = LookupSingleLeg(r.DepLegPortId, r.VoyageId, r.VesselId);
            string nextLegText = LookupSingleLeg(r.NextLegPortId, r.VoyageId, r.VesselId);
            if (string.IsNullOrWhiteSpace(depLegText) && dtMain != null && dtMain.Rows.Count > 0 && dtMain.Columns.Contains("Leg"))
                depLegText = dtMain.Rows[0]["Leg"]?.ToString() ?? "";

            var sb = new StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">Hello,</p><p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>");
            sb.Append(@"<p style=""margin:0 0 12px 0;"">The Departure Report for vessel ").Append(System.Web.HttpUtility.HtmlEncode(vesselName ?? "")).Append(@" dated ").Append(V(r.ReportDate ?? r.SBE_DateT ?? r.RFA_DateT)).Append(@" has been successfully generated and is attached to this email in Excel format for your reference.</p>");
            sb.Append(@"<table style=""width:100%;min-width:1200px;max-width:1200px;border-collapse:collapse;border:1px solid #ccc;"">");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:12px;background:#555;color:#fff;font-size:16px;font-weight:bold;text-align:center;"">Departure Report (").Append(V(vesselName)).Append(@")</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            // Header field order matches the web view: Voy No., Leg, Dep. Port, Draft Fwd,
            // Next Leg, Next Port, ETA, Draft Mid, Report Date, Draft Aft.
            sb.Append(KvRow("Voy No.", voyNo));
            sb.Append(KvRow("Leg", depLegText));
            sb.Append(KvRow("Dep. Port", r.DeparturePort));
            sb.Append(KvRow("Draft Fwd (Mtrs)", r.DraftFwd));
            sb.Append(KvRow("Next Leg", nextLegText));
            sb.Append(KvRow("Next Port", r.NextPort));
            sb.Append(KvRow("ETA", Vdt(r.ETA)));
            sb.Append(KvRow("Draft Mid (Mtrs)", r.DraftMid));
            sb.Append(KvRow("Report Date", V(r.ReportDate)));
            sb.Append(KvRow("Draft Aft (Mtrs)", r.DraftAft));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Manoeuvring</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            // Manoeuvring Hours: 2 decimal places per spec. Distance keeps the default V() format.
            string mhrsInline = r.Manoeuvring_Hrs.HasValue ? r.Manoeuvring_Hrs.Value.ToString("0.00") : "-";
            sb.Append(KvRow("Manoeuvring Hours", mhrsInline)).Append(KvRow("Manoeuvring Distance", r.Manoeuvring_Distance));
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

            // Other Receipts, Repairs, Crew Change & Landed — placed after Non-Routine Events
            // and before Weather, matching the web view's section order.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other Receipts, Repairs, Crew Change & Landed</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("CTM (Mention Currency & Amount)", r.CTM));
            sb.Append(KvRow("SPARES (Mention Revision Numbers)", r.Spares));
            sb.Append(KvRow("STORES (Mention Revision Numbers)", r.Stores));
            sb.Append(KvRow("REPAIRS CONDUCTED (Mention details)", r.RepairsConducted));
            sb.Append(KvRow("CREW CHANGE (No of Crew)", r.CrewChange));
            sb.Append(KvRow("ITEMS LANDED (Mention Details)", r.ItemsLanded));
            sb.Append(@"</table></td></tr>");

            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Weather</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Sea State", r.SeaState)).Append(KvRow("Wind Direction", r.WindDirection)).Append(KvRow("Wind Force(BF Scale)", r.WindForce)).Append(KvRow("Swell Direction", r.SwellDirection)).Append(KvRow("Swell Height (mtrs)", r.SwellHeight)).Append(KvRow("Wave Length (mtrs)", r.WaveLength)).Append(KvRow("Wave Height (mtrs)", r.WaveHeight));
            sb.Append(@"</table></td></tr>");

            // Departure Report Remarks — labeled key-value row matching the web view layout.
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Departure Report Remarks</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Departure Report Remarks", r.Remarks));
            sb.Append(@"</table></td></tr>");
            // === Engine section — 8 tables matching the web view ===

            // Table 1: Slops / Bilge Disposed & ROB (label | Oil | Water | Total)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Slops / Bilge Disposed &amp; ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Oil</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Water</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Total</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Slops Disposed(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsDisposed_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsDisposed_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsDisposed_Total)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Slops ROB(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsROB_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsROB_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.SlopsROB_Total)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Bilge Disposed(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesDisposed_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesDisposed_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesDisposed_Total)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Bilge ROB(m3)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesROB_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesROB_Water)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.BilgesROB_Total)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Table 2: Other Disposal
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other Disposal</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("Sludge(m3)", V3(r.Sludge)));
            sb.Append(KvRow("Garbage - Plastic(m3)", V3(r.GarbagePlastic)));
            sb.Append(KvRow("Garbage - Others(m3)", V3(r.GarbageOthers)));
            sb.Append(KvRow("Other Disposal(m3)", V3(r.OtherDisposal)));
            sb.Append(@"</table></td></tr>");

            // Table 3: Date & Time (SBE / RFA)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Date &amp; Time</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SBE", Vdt(r.SBE_DateT)));
            sb.Append(KvRow("RFA", Vdt(r.RFA_DateT)));
            sb.Append(@"</table></td></tr>");

            // Table 4: Engine (force 3 decimals on all values)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Engine</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("SLIP%", V3(r.Slip)));
            sb.Append(KvRow("RPM", V3(r.RPM)));
            sb.Append(KvRow("BHP(hp)", V3(r.BHP)));
            sb.Append(KvRow("MCR%", V3(r.MCR)));
            sb.Append(@"</table></td></tr>");

            // Table 5: LO & HO Consumptions — 3-col (Consumption | Received | ROB)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">LO &amp; HO Consumptions</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:35%;min-width:180px""><col style=""width:22%;min-width:120px""><col style=""width:21%;min-width:120px""><col style=""width:22%;min-width:120px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Consumptions</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Received</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">ROB</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECC(Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_MECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.Bunker_LO_Rec_MECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_MECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">MECYL(Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_MECYL)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.Bunker_LO_Rec_MECYL)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_MECYL_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">AECC(Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_AECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.Bunker_LO_Rec_AECC)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_AECC_ROB)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">HYDRAULIC Oil(Ltrs)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_HYDR_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.Bunker_LO_Rec_HYDR_Oil)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.LO_HO_Cons_HYDR_Oil_ROB)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Table 6: Other ROB — 3-col (Full | In Use | Empty), values formatted with 3 decimals
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Other ROB</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px""><col style=""width:20%;min-width:100px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Full</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">In Use</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">Empty</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Oxygen (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_OXY_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_OXY_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_OXY_Empty)).Append(@"</td></tr>");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Acetylene (Bottles)</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_ACYT_Full)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_ACYT_InUse)).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(V3(r.OT_ROB_ACYT_Empty)).Append(@"</td></tr>");
            sb.Append(@"</table></td></tr>");

            // Table 7: Fuel ROB in MT — 2-col (SBE | RFA) per fuel type
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fuel ROB in MT</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:40%;min-width:200px""><col style=""width:30%;min-width:150px""><col style=""width:30%;min-width:150px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;""></td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">SBE</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:center;"">RFA</td></tr>");
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string ft = (dr["FuelType"]?.ToString() ?? "").Trim();
                    string sbe = dr.Table.Columns.Contains("SBE") ? V3(dr["SBE"]) : "-";
                    string rfa = dr.Table.Columns.Contains("RFA") ? V3(dr["RFA"]) : "-";
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">").Append(ft).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(sbe).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(rfa).Append(@"</td></tr>");
                }
            }
            sb.Append(@"</table></td></tr>");

            // Table 8: Fuel Consumption in MT — full 7-block layout
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;"">").Append(BuildFuelConsFullTable(dtFuelCons)).Append(@"</td></tr>");
            // Cargo — 6-col table matching the web view:
            //   Cargo | B/L QTY(MT) | Load Portal Actual(MT) | Cargo Temp.(Deg centigrade)
            //         | Completion Date & Time | Rate(m3/hr)
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Cargo</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:22%;min-width:140px""><col style=""width:14%;min-width:90px""><col style=""width:16%;min-width:100px""><col style=""width:18%;min-width:110px""><col style=""width:18%;min-width:120px""><col style=""width:12%;min-width:80px"">");
            sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;"">Cargo</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">B/L QTY(MT)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Load Portal Actual(MT)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Cargo Temp.(Deg centigrade)</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Completion Date &amp; Time</td><td style=""padding:6px 8px;border:1px solid #ccc;font-weight:bold;background:#f5f5f5;text-align:right;"">Rate(m3/hr)</td></tr>");
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    string cargoLabel = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    string bl = V3(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    string lpa = V3(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    string ctmp = Num(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
                    string comp = "-";
                    if (dr.Table.Columns.Contains("Completion_DateT") && dr["Completion_DateT"] != null && dr["Completion_DateT"] != DBNull.Value)
                    {
                        if (DateTime.TryParse(dr["Completion_DateT"].ToString(), out DateTime cd)) comp = cd.ToString(DateTimeFormat);
                    }
                    string rate = Num(dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null);
                    sb.Append(@"<tr><td style=""padding:6px 8px;border:1px solid #ccc;"">").Append(cargoLabel).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(bl).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(lpa).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(ctmp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(comp).Append(@"</td><td style=""padding:6px 8px;border:1px solid #ccc;text-align:right;"">").Append(rate).Append(@"</td></tr>");
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
            sb.Append(KvRow("ROB", V3(r.Ballast_ROB)));
            sb.Append(@"</table></td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:10px 8px;background:#555;color:#fff;font-weight:bold;text-align:center;"">Fresh Water</td></tr>");
            sb.Append(@"<tr><td colspan=""8"" style=""padding:0;""><table style=""width:100%;border-collapse:collapse;table-layout:fixed;""><col style=""width:45%;min-width:280px""><col style=""width:55%"">");
            sb.Append(KvRow("FW Generated (MT)", V3(r.FW_Generated))).Append(KvRow("Consumption (MT)", V3(r.FW_Consumption))).Append(KvRow("ROB (MT)", V3(r.FW_ROB)));
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
