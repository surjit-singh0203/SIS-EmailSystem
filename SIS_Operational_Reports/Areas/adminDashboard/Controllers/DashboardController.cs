using DataBuildingLayer;
using SIS_Operational_Reports.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;


using MailKit.Net.Imap;
using MailKit.Security;
using MailKit;
using MailKit.Search;
using MimeKit;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNet.Identity;
using ClosedXML.Excel;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace SIS_Operational_Reports.Areas.adminDashboard.Controllers
{

    [Authorize]
    [UserAuthenticationFilter]
    public class DashboardController : Controller
    {
        // GET: adminDashboard/Dashboard
        //public ActionResult Index()
        //{
        //    return View();
        //}

        private static HttpClient _httpClient = new HttpClient();

        List<SelectListItem> ddlMonths = new List<SelectListItem>();
        List<SelectListItem> ddlYears = new List<SelectListItem>();

        public ActionResult Index(int? pageNo, string firstVal, string Year, string Month, string Vessel)
        {
            DashboardAdminClass arrR = new DashboardAdminClass();

            int? Years = null;
            if (Years == null)
            {
                Years = DateTime.Now.Year;
            }
            ViewBag.linktoYearId = GetYears(Years);
            ViewBag.linktoMonthId = GetMonths(Years);

            //arrR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);

            string vslid = Convert.ToString(Vessel == null ? "" : Vessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }


            string currentYear = DateTime.Now.Year.ToString();
            string currentMonth = DateTime.Now.Month.ToString();

            //string currentMonth = "04";

            Session["Year"] = currentYear;
            Session["Month"] = currentMonth;

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32( currentMonth));

            //ViewBag.YearMonth = "Vessel Report >> Year-" + currentYear + " >> Month-" + monthName + "";

            ViewBag.YearMonth = monthName + "," + currentYear;

            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                // arrR.DashoardList = CommonMethods.GetAdminDashboardList(vslid, currentYear, currentMonth, currPage, pageSize);
                arrR.DashoardList = CommonMethods.GetAdminDashboardList2(vslid, currentYear, currentMonth, currPage, pageSize);
                //arrR.DashoardList1 = CommonMethods.GetAdminDashboardList1(vslid, currentYear, currentMonth, currPage, pageSize);
                //arrR.VesselId = Convert.ToInt32(Vessel);
                return View(arrR);
            }
            else if (firstVal == "" && Year == "" && Vessel == "")
            {
                //  arrR.DashoardList = CommonMethods.GetAdminDashboardList(vslid, currentYear, currentMonth, currPage, pageSize);
                arrR.DashoardList = CommonMethods.GetAdminDashboardList2(vslid, currentYear, currentMonth, currPage, pageSize);
                //arrR.DashoardList1 = CommonMethods.GetAdminDashboardList1(vslid, currentYear, currentMonth, currPage, pageSize);
                //arrR.VesselId = Convert.ToInt32(Vessel);
                return View(arrR);
            }
            //else if (firstVal != null || dateF != "" || Vessel != "")
            //{
            //    arrR.GetArrivalRList = CommonMethods.SearchArrivalReportList(vslid, currPage, pageSize, firstVal, dateF, dateT);
            //    arrR.VesselId = Convert.ToInt32(Vessel);
            //    return PartialView("_searcharrivalR", arrR);
            //}
            return View(arrR);
        }

        [HttpPost]
        public ActionResult Index(int? pageNo, string firstVal,DashboardAdminClass cls)
        {
            DashboardAdminClass arrR = new DashboardAdminClass();

            if (cls.VesselIDs != null) 
                {
                cls.VesselIDs = cls.VesselIDs.Distinct().ToList();
                if (cls.VesselIDs.Count > 0)
                {
                    foreach (int id in cls.VesselIDs)
                        cls.searchVessel = string.Format("{0},{1}", id, cls.searchVessel);

                    cls.searchVessel = cls.searchVessel.Trim(',');
                }
            }

            //cls.searchVessel = "MT CLASSIC, test vessel, MT Century";
            //if (cls.VesselIDs != null)
            //{
            //    foreach (int id in cls.VesselIDs)
            //    {
            //        cls.VesselId = string.Format("{0},{1}", id, cls.VesselId);
            //    }
            //    cls.VesselId = cls.VesselId.Trim(',');
            //}


            int? Years = null;
            if (Years == null)
            {
                Years = DateTime.Now.Year;
            }
            ViewBag.linktoYearId = GetYears1(Years,cls.year);
            ViewBag.linktoMonthId = GetMonths1(Years,cls.month);

            //ViewBag.NewsCategoriesID = new SelectList(GetYears(Years), "id", "countryName", "1");

            //ViewBag.NewsCategoriesID = "1";

            //arrR.VesselList = CommonClass.GetVesselList(StaticHelper.PermittedVessel);

            string vslid = Convert.ToString(cls.VesselIDs == null ? "" : cls.searchVessel);
            if (vslid.ToString() == "")
            {
                vslid = Convert.ToString(Session["VesselID"]);
            }

            if (cls.year == "")
            {

                string currentYear = DateTime.Now.Year.ToString();
                string currentMonth = DateTime.Now.Month.ToString();

               // string currentMonth = "04";

                cls.year = currentYear;
                cls.month = currentMonth;

                Session["Year"] = currentYear;
                Session["Month"] = currentMonth;
            }
            else
            {
                Session["Year"] = cls.year;
                Session["Month"] = cls.month;
            }

            if (cls.year == null)
            {
                cls.year = DateTime.Now.Year.ToString();
                Session["Year"] = cls.year;
            }
            if (cls.month == null)
            {
                cls.month = DateTime.Now.Month.ToString();
                Session["Month"] = cls.month;
            }

            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(cls.month));

            //ViewBag.YearMonth = "Vessel Report >> Year-" + cls.year + " >> Month-" + monthName + "";

            ViewBag.YearMonth = monthName + "," + cls.year;


            int currPage = pageNo == null ? 1 : Convert.ToInt32(pageNo);
            int pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["pageSize"]);
            TempData["CurrentPage"] = currPage;

            if (firstVal == null)
            {
                //arrR.DashoardList = CommonMethods.GetAdminDashboardList(vslid, cls.year, cls.month, currPage, pageSize);
                arrR.DashoardList = CommonMethods.GetAdminDashboardList2(vslid, cls.year, cls.month, currPage, pageSize);
                // arrR.DashoardList1 = CommonMethods.GetAdminDashboardList1(vslid, cls.year, cls.month, currPage, pageSize);
                //arrR.VesselId = Convert.ToInt32(Vessel);
                return View("Index", arrR);
            }
            else if (firstVal == "" && cls.year == "" && cls.month == "")
            {
               // arrR.DashoardList = CommonMethods.GetAdminDashboardList(vslid, cls.year, cls.month, currPage, pageSize);
                arrR.DashoardList = CommonMethods.GetAdminDashboardList2(vslid, cls.year, cls.month, currPage, pageSize);
                //arrR.DashoardList1 = CommonMethods.GetAdminDashboardList1(vslid, cls.year, cls.month, currPage, pageSize);
                //arrR.VesselId = Convert.ToInt32(Vessel);
                return View("Index", arrR);
            }
            
            return View("Index", arrR);
        }
        private SelectList GetYears(int? iSelectedYear)
        {
            int CurrentYear = DateTime.Now.Year;
            for (int i = 2021; i <= CurrentYear; i++)
            {
                ddlYears.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            //Default It will Select Current Year  
            return new SelectList(ddlYears, "Value", "Text", iSelectedYear);
        }
        private SelectList GetMonths(int? iSelectedYear)
        {
            var months = Enumerable.Range(1, 12).Select(i => new
            {
                A = i,
                B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
            });

            int CurrentMonth1 = DateTime.Now.Month;
            int CurrentMonth = 1; //January  
            if (iSelectedYear == DateTime.Now.Year)
            {
                CurrentMonth = 12;
                months = Enumerable.Range(1, CurrentMonth).Select(i => new
                {
                    A = i,
                    B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
                });
            }
            foreach (var item in months)
            {
                ddlMonths.Add(new SelectListItem { Text = item.B.ToString(), Value = item.A.ToString() });
            }

            //Default It will Select Current Month  
            return new SelectList(ddlMonths, "Value", "Text", CurrentMonth1);

        }


        private SelectList GetYears1(int? iSelectedYear,string Syear)
        {
            iSelectedYear = Convert.ToInt32(Syear);
            int CurrentYear = DateTime.Now.Year;
            for (int i = 2021; i <= CurrentYear; i++)
            {
                ddlYears.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            //Default It will Select Current Year  
            return new SelectList(ddlYears, "Value", "Text", iSelectedYear);
        }
        private SelectList GetMonths1(int? iSelectedYear,string Smonth)
        {
            var months = Enumerable.Range(1, 12).Select(i => new
            {
                A = i,
                B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
            });

            int CurrentMonth1 = Convert.ToInt32(Smonth);
            int CurrentMonth = 1; //January  
            if (iSelectedYear == DateTime.Now.Year)
            {
                CurrentMonth = 12;
                months = Enumerable.Range(1, CurrentMonth).Select(i => new
                {
                    A = i,
                    B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
                });
            }
            foreach (var item in months)
            {
                ddlMonths.Add(new SelectListItem { Text = item.B.ToString(), Value = item.A.ToString() });
            }

            //Default It will Select Current Month  
            return new SelectList(ddlMonths, "Value", "Text", CurrentMonth1);

        }

        #region Sync Working 20Dec2022

      

        string syncVessel = "";

        public async Task<JsonResult> vesselwiseSync(string vslSync)
        {
            syncVessel = vslSync;

            //getAllAttachmentNew();
            GetAllAttachmentNew();
            //ImportData();

            List<string> uniqueValues = uploadVslName.ToUpper().Split(',').Distinct().ToList();
            string UniqueString = string.Join(",", uniqueValues);

            List<string> uniqueValues1 = uploadVslId.Split(',').Distinct().ToList();
            string UniqueString1 = string.Join(",", uniqueValues1);

            String[] strs1 = vslSync.Split(',');
            String[] strs2 = UniqueString1.Split(',');
            var res = strs1.Except(strs2).Union(strs2.Except(strs1));
            String result = String.Join(",", res);

            string notmatchedvessel = "";
            try
            {
                result = result.TrimEnd(',');
                using (SqlDataAdapter adp = new SqlDataAdapter("select VesselName from VesselDetail where ImoNo in(" + result + ")", ConnectionBulder.con))
                {
                    DataTable dt = new DataTable();
                    adp.Fill(dt);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        notmatchedvessel += dt.Rows[i][0].ToString() + ",";
                    }
                }
            }
            catch { }

            //List<string> uniqueValues1 = notuploadVslName.ToUpper().Split(',').Distinct().ToList();
            //string UniqueString1 = string.Join(",", uniqueValues1);

            return Json(new { Result = true, UploadVessel = UniqueString.TrimEnd(','), NoUploadVessel = notmatchedvessel.TrimEnd(',') }, JsonRequestBehavior.AllowGet);


        }

        public string GetAccessToken()
        {
            string clientId = ConfigurationManager.AppSettings["ida:ClientIdGraphApi"];
            string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
            string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            var tokenRequestContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("resource", "https://graph.microsoft.com")
        });

            var tokenResponse = _httpClient.PostAsync(tokenEndpoint, tokenRequestContent).Result;
            tokenResponse.EnsureSuccessStatusCode();

            var tokenResponseContent = tokenResponse.Content.ReadAsStringAsync().Result;
            var tokenResponseJson = JObject.Parse(tokenResponseContent);
            return tokenResponseJson["access_token"].ToString();
        }

        public void GetAllAttachmentNew()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    string folderPath = Server.MapPath("~/Inbox/");
                    string userEmail = ConfigurationManager.AppSettings["ida:GraphMail"];

                    //string userEmail = ConfigurationManager.AppSettings["ida:GraphMail"];
                    httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                    string token = GetAccessToken();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


                    DateTime time = DateTime.UtcNow;
                    string timeOnly = time.ToString("HH:mm");
                    TimeSpan currentTime = TimeSpan.Parse(timeOnly);
                    TimeSpan Mrning = TimeSpan.Parse("03:25");
                    TimeSpan Evng = TimeSpan.Parse("11:25");


                    if (currentTime > Evng)
                    {
                        DateTime twoDaysAgo = DateTime.UtcNow.Date;

                        // 12 Hrs
                        DateTime startDatePart = twoDaysAgo.AddHours(2);
                        DateTime endDatePart = startDatePart.AddHours(12);

                        string startDatePartString = startDatePart.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePartString = endDatePart.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart = $"$filter=receivedDateTime ge {startDatePartString} and receivedDateTime lt {endDatePartString}";

                        var responsePart = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart}").Result;
                        responsePart.EnsureSuccessStatusCode();
                        var responseContentPart = responsePart.Content.ReadAsStringAsync().Result;
                        var messagesPart = JObject.Parse(responseContentPart)["value"];

                        ProcessMessages(messagesPart, folderPath, httpClient, userEmail);

                        //Next 12 Hrs
                        DateTime startDatePart1 = twoDaysAgo.AddHours(6);
                        DateTime endDatePart1 = startDatePart1.AddHours(12);

                        string startDatePart1String = startDatePart1.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePart1String = endDatePart1.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart1 = $"$filter=receivedDateTime ge {startDatePart1String} and receivedDateTime lt {endDatePart1String}";

                        var responsePart1 = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart1}").Result;
                        responsePart1.EnsureSuccessStatusCode();
                        var responseContentPart1 = responsePart1.Content.ReadAsStringAsync().Result;
                        var messagesPart1 = JObject.Parse(responseContentPart1)["value"];

                        ProcessMessages(messagesPart1, folderPath, httpClient, userEmail);


                    }
                    else if (currentTime > Mrning)
                    {

                        // 12 Hrs
                        DateTime twoDaysAgo = DateTime.UtcNow.AddDays(-1).Date;
                        DateTime startDatePart2 = twoDaysAgo.AddHours(12);
                        DateTime endDatePart2 = startDatePart2.AddHours(12);

                        string startDatePart2String = startDatePart2.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePart2String = endDatePart2.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart2 = $"$filter=receivedDateTime ge {startDatePart2String} and receivedDateTime lt {endDatePart2String}";

                        var responsePart2 = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart2}").Result;
                        responsePart2.EnsureSuccessStatusCode();
                        var responseContentPart2 = responsePart2.Content.ReadAsStringAsync().Result;
                        var messagesPart2 = JObject.Parse(responseContentPart2)["value"];

                        ProcessMessages(messagesPart2, folderPath, httpClient, userEmail);


                        // Next 12 Hrs
                        DateTime startDatePart3 = endDatePart2;
                        DateTime endDatePart3 = startDatePart3.AddHours(12);

                        string startDatePart3String = startDatePart3.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePart3String = endDatePart3.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart3 = $"$filter=receivedDateTime ge {startDatePart3String} and receivedDateTime lt {endDatePart3String}";

                        var responsePart3 = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart3}").Result;
                        responsePart3.EnsureSuccessStatusCode();
                        var responseContentPart3 = responsePart3.Content.ReadAsStringAsync().Result;
                        var messagesPart3 = JObject.Parse(responseContentPart3)["value"];

                        ProcessMessages(messagesPart3, folderPath, httpClient, userEmail);


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read emails: {ex.Message}");
                throw;
            }
        }

        private void ProcessMessages(JToken messages, string folderPath, HttpClient httpClient, string userEmail)
        {
            foreach (var message in messages)
            {
            
                if (message["hasAttachments"] != null && (bool)message["hasAttachments"])
                {
                    string messageId = (string)message["id"];
                    var attachmentsResponse = httpClient.GetAsync($"users/{userEmail}/messages/{messageId}/attachments").Result;
                    attachmentsResponse.EnsureSuccessStatusCode();

                    var attachmentsContent = attachmentsResponse.Content.ReadAsStringAsync().Result;
                    var attachments = JArray.Parse(JObject.Parse(attachmentsContent)["value"].ToString());

                    foreach (var attachment in attachments)
                    {
                        if ((string)attachment["@odata.type"] == "#microsoft.graph.fileAttachment")
                        {
                            string fileName = (string)attachment["name"];
                            string contentType = (string)attachment["contentType"];

                            // Check if the attachment is related to "Sis_Nova"
                            if (!string.IsNullOrEmpty(fileName) && fileName.Contains("Sis_Nova"))
                            {
                                string attachmentId = (string)attachment["id"];
                                var attachmentResponse = httpClient.GetAsync($"users/{userEmail}/messages/{messageId}/attachments/{attachmentId}/$value").Result;
                                attachmentResponse.EnsureSuccessStatusCode();

                                byte[] attachmentData = attachmentResponse.Content.ReadAsByteArrayAsync().Result;

                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }

                                string filePath = Path.Combine(folderPath, fileName);

                                System.IO.File.WriteAllBytes(filePath, attachmentData);

                               
                                ImportData();
                            }
                        }
                    }
                }
            }
        }

        private void getAllAttachmentNew()
        {
            string location = Server.MapPath("~/Inbox/");
            string dt = DateTime.Now.AddHours(-24).ToString("yyyy-MM-dd");

            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                //MailServer oServer = new MailServer("imap.gmail.com", "sisnova@sishipping.com", "zedinmucmddngkzr", ServerProtocol.Imap4);

                //client.Authenticate("ws1.49web@gmail.com", "inyemftdduwdtnkp");
                client.Authenticate("sisnova@sishipping.com", "zedinmucmddngkzr");

                //client.Connect(Constant.GoogleImapHost, Constant.ImapPort, SecureSocketOptions.SslOnConnect);
                //client.AuthenticationMechanisms.Remove(Constant.GoogleOAuth);
                //client.Authenticate(Constant.GoogleUserName, Constant.GenericPassword);



                client.Inbox.Open(FolderAccess.ReadWrite);
                IList<UniqueId> uids = client.Inbox.Search(SearchQuery.All);
                //var query = MailKit.Search.SearchQuery.FromContains("anyone@gmail.com")
                //            .And(MailKit.Search.SearchQuery.SubjectContains("Your Subject"));
                var query1 = client.Inbox.Search(SearchQuery.DeliveredAfter(DateTime.Parse(dt)));
                //var count2 = oMail.Attachments.Where(x => x.Name.Contains("Sis_Nova")).ToList().Count;
                foreach (UniqueId uid in query1)
                {
                    MimeMessage message = client.Inbox.GetMessage(uid);

                    foreach (MimeEntity attachment in message.Attachments.Where(x => x.ContentType.Name.Contains("Sis_Nova") && x.ContentType.Name != null).ToList())
                    {
                        var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                        if (attachment is MessagePart)
                        {
                            var fileName2 = attachment.ContentDisposition?.FileName;
                            var rfc822 = (MessagePart)attachment;

                            if (string.IsNullOrEmpty(fileName2))
                                fileName2 = "attached-message.eml";

                            using (var stream = System.IO.File.Create(location))
                                rfc822.Message.WriteTo(stream);
                        }
                        else
                        {
                            var part = (MimePart)attachment;
                            var fileName2 = part.FileName;

                            //using (var stream = File.Create(location))
                            using (var stream = System.IO.File.Create(location + fileName2))
                                part.Content.DecodeTo(stream);

                            
                        }
                    }

                }
            }
        }


        int checkError = 0;
        private async void ImportData()
        {
            try
            {
                checkError = 0;

                //string location = Server.MapPath("~/Inbox/");
                string location1 = Server.MapPath("~/Inbox/");

                string locationArchieve = Server.MapPath("~/Archive/");
                string location = "";
                DirectoryInfo place = new DirectoryInfo(location1);

                FileInfo[] Files = place.GetFiles();

                foreach (FileInfo m in Files)
                {
                    location = Server.MapPath("~/Inbox/");
                    location += m.Name;


                    // string[] filePaths = Directory.GetFiles(@"c:\Maps\", "*.txt",SearchOption.TopDirectoryOnly);
                    using (XLWorkbook workBook = new XLWorkbook(location))
                    {
                        int worksheetcount = workBook.Worksheets.Count;
                        for (int s = 1; s <= worksheetcount; s++)
                        {
                            IXLWorksheet workSheet = workBook.Worksheet(s);
                            var sheetName = workBook.Worksheet(s).Name;
                            //Create a new DataTable.
                            DataTable dtx = new DataTable();

                            if (sheetName == "Fuel_Cons_NR")
                            {
                                Task<DataTable> dtxx = generateTable(location, sheetName);


                                var getErrorsheet = InsertImportedData(sheetName, await dtxx);
                                if (!string.IsNullOrEmpty(getErrorsheet))
                                {
                                    //ErrorINimportedTables += getErrorsheet + ", ";
                                }
                            }
                            else
                            {

                                //Loop through the Worksheet rows.
                                bool firstRow = true;
                                foreach (IXLRow row in workSheet.Rows())
                                {
                                    int lastrow = workSheet.LastRowUsed().RowNumber();
                                    var rows = workSheet.Rows(1, lastrow);
                                    foreach (IXLRow row1 in rows)
                                    {
                                        if (row1.IsEmpty())
                                            row1.Delete();
                                    }
                                    //Use the first row to add columns to DataTable.
                                    if (firstRow)
                                    {
                                        foreach (IXLCell cell in row.Cells())
                                        {
                                            dtx.Columns.Add(cell.Value.ToString());
                                        }
                                        firstRow = false;
                                    }
                                    else
                                    {
                                        //Add rows to DataTable.
                                        dtx.Rows.Add();
                                        int i = 0;

                                        //var col = row.Cells(row.FirstCellUsed().Address.ColumnNumber.ToString());
                                        //var row121 = row.LastCellUsed().Address.ColumnNumber;

                                        foreach (IXLCell cell in row.Cells(1, dtx.Columns.Count))
                                        {
                                            if (cell.Value.ToString() != "")
                                            {
                                                dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                                i++;
                                            }
                                            else
                                            {
                                                dtx.Rows[dtx.Rows.Count - 1][i] = DBNull.Value;
                                                i++;
                                            }
                                        }

                                        //foreach (IXLCell cell in row.Cells())
                                        //{
                                        //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                        //    i++;
                                        //}
                                        //foreach (IXLCell cell in row.Cells(row.FirstCellUsed().Address.ColumnNumber, row.LastCellUsed().Address.ColumnNumber))
                                        //{
                                        //    dtx.Rows[dtx.Rows.Count - 1][i] = cell.Value.ToString();
                                        //    i++;
                                        //}
                                    }
                                }


                                var getErrorsheet = InsertImportedData(sheetName, dtx);
                                if (!string.IsNullOrEmpty(getErrorsheet))
                                {
                                    // ErrorINimportedTables += getErrorsheet + ", ";
                                }
                            }
                        }
                    }

                    if (checkError == 0)
                    {
                        string sourcePath2 = Server.MapPath(string.Format("~/Inbox/" + m.Name));
                        string targetPath2 = Server.MapPath(string.Format("~/Archive/ " + m.Name));

                        System.IO.File.Copy(sourcePath2, targetPath2, true);
                        //File.Copy(Path.Combine(source, destination),
                        m.Delete();
                    }
                    else
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("insert into ErrorLog values( '" + m.Name + "',getdate() )", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        List<DateTime> maxdt = new List<DateTime>();
        string uploadVslName = "", uploadVslId = "", notuploadVslName = "";
        private string InsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {
                string syncingVsl = syncVessel;

                //syncingVsl = "9293143,9293131,9293129,9298820,9272400,9198305,9242156";
                var distinctValues = tbls.AsEnumerable()
                        .Select(row => new {VesselID = row.Field<string>("VesselId"),}).Distinct();

                var disVsl = distinctValues.Select(x => x.VesselID).SingleOrDefault();

                string diffVsl = disVsl;
                var vslMatchorNot = syncingVsl.Contains(diffVsl);

                if (vslMatchorNot == true)
                {
                    try
                    {
                        using (SqlDataAdapter adp=new SqlDataAdapter ("select VesselName from VesselDetail where ImoNo="+ diffVsl + "", ConnectionBulder.con))
                        {
                            DataTable dt = new DataTable();
                            adp.Fill(dt);

                            uploadVslName += dt.Rows[0][0].ToString() + ",";
                            uploadVslId += diffVsl + ",";

                        }
                    }
                    catch { }

                    checkError = 0;
                    string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
                    if (sheetName == "DailyNoonReport" || sheetName == "ArrivalReport" || sheetName == "DepartureReport" || sheetName == "DepartureReport")
                    {
                        object maxDate = tbls.Compute("MAX(ModifiedDate)", null);
                        maxdt.Add(Convert.ToDateTime(maxDate));
                    }
                    if (sheetName == "DischargingReport" || sheetName == "LoadingReport")
                    {
                        object maxDate = tbls.Compute("MAX(ModifyDate)", null);
                        maxdt.Add(Convert.ToDateTime(maxDate));
                    }
                    if (sheetName == "ExportLog")
                    {
                        DateTime biggestDate = maxdt.Max(p => p);
                        for (int i = 0; i < tbls.Rows.Count; i++)
                        {
                            int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                            string VoyageId = tbls.Rows[i]["VoyageId"].ToString();
                            int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                            string importedby = User.Identity.GetUserName();
                            string exportedby = tbls.Rows[i]["ExportedBy"].ToString();
                            DateTime exporteddate = Convert.ToDateTime(tbls.Rows[i]["ExportedDate"]);
                            DateTime crntdate = DateTime.UtcNow;
                            using (SqlDataAdapter adp = new SqlDataAdapter("insert into ImportLog values( '" + VesselId + "','" + VoyageId + "','" + exporteddate + "', '" + crntdate + "','" + exportedby + "', '" + importedby + "','" + biggestDate + "' )", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);
                            }
                        }
                    }
                    else if (sheetName == "VoyageLeg")
                    {
                        for (int i = 0; i < tbls.Rows.Count; i++)
                        {
                            int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                            int VoyageId = Convert.ToInt32(tbls.Rows[i]["VoyageId"]);
                            string LegPort_A = tbls.Rows[i]["LegPort_A"].ToString();
                            int ReasonforPortCall_A = Convert.ToInt32(tbls.Rows[i]["ReasonforPortCall_A"]);
                            string LegPort_B = tbls.Rows[i]["LegPort_B"].ToString();
                            int ReasonforPortCall_B = Convert.ToInt32(tbls.Rows[i]["ReasonforPortCall_B"]);
                            decimal DTG = Convert.ToDecimal(tbls.Rows[i]["DTG"]);
                            decimal CP_SOG = Convert.ToDecimal(tbls.Rows[i]["CP_SOG"]);
                            decimal CP_Log_Speed = Convert.ToDecimal(tbls.Rows[i]["CP_Log_Speed"]);
                            DateTime CreatedDate = Convert.ToDateTime(tbls.Rows[i]["CreatedDate"]);
                            bool IsActive = Convert.ToBoolean(tbls.Rows[i]["IsActive"]);
                            int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                            using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertVoyagelegImport", ConnectionBulder.con))
                            {
                                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                                adapter.SelectCommand.Parameters.AddWithValue("@id", Id);
                                adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", VoyageId);
                                adapter.SelectCommand.Parameters.AddWithValue("@LegPort_A", LegPort_A);
                                adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_A", ReasonforPortCall_A);
                                adapter.SelectCommand.Parameters.AddWithValue("@LegPort_B", LegPort_B);
                                adapter.SelectCommand.Parameters.AddWithValue("@ReasonforPortCall_B", ReasonforPortCall_B);

                                adapter.SelectCommand.Parameters.AddWithValue("@DTG", DTG);
                                adapter.SelectCommand.Parameters.AddWithValue("@CP_SOG", CP_SOG);
                                adapter.SelectCommand.Parameters.AddWithValue("@CP_Log_Speed", CP_Log_Speed);


                                adapter.SelectCommand.Parameters.AddWithValue("@CreatedDate", CreatedDate);
                                adapter.SelectCommand.Parameters.AddWithValue("@IsActive", IsActive);
                                adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);

                                //adapter.SelectCommand.Parameters.AddWithValue("@StatementType", Action);
                                // SqlDataAdapter adapter = new SqlDataAdapter(command);
                                using (DataTable dataTable = new DataTable())
                                {
                                    adapter.Fill(dataTable);

                                }
                            }
                        }
                    }
                    else
                    {


                        // if (!destinationTableName.Equals("VersionTable", StringComparison.OrdinalIgnoreCase))
                        //if (sheetName == "LoadingReport")
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();

                            using (SqlCommand cmd = new SqlCommand(string.Format("Update_{0}", sheetName)))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = connection;
                                cmd.Parameters.AddWithValue(string.Format("{0}TableType", sheetName), tbls);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                else
                {
                    checkError = 0;
                    try
                    {
                                          
                        var distinctValues1 = tbls.AsEnumerable()
                                .Select(row => new { VesselID = row.Field<string>("VesselId"), }).Distinct();

                        var disVsl1 = distinctValues1.Select(x => x.VesselID).SingleOrDefault();

                        string diffVsl1 = disVsl1;
                        var vslMatchorNot1 = syncingVsl.Contains(diffVsl1);

                        if (vslMatchorNot == true)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter("select VesselName from VesselDetail where ImoNo=" + diffVsl1 + "", ConnectionBulder.con))
                            {
                                DataTable dt = new DataTable();
                                adp.Fill(dt);

                                notuploadVslName += dt.Rows[0][0].ToString() + ",";

                            }
                        }
                    }
                    catch { }
                }

                return "";

            }
            catch (Exception ex)
            {
                checkError = 1;
                //return ex.Message.ToString();
                return sheetName;

            }


        }

        private async Task<DataTable> generateTable(string location, string sheetname)
        {
            //string filepath = @"D:\Sis_Nova_26102022_Export_9242156 (1).xlsx";
            //string sqlquery = "Select * From [Fuel_Cons_NR$]";
            string filepath = location;
            string sqlquery = "Select * From [" + sheetname + "$]";

            string constring = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filepath + ";Extended Properties=\"Excel 12.0;HDR=YES;\"";
            using (OleDbConnection con = new OleDbConnection(constring + ""))
            {
                using (OleDbDataAdapter da = new OleDbDataAdapter(sqlquery, con))
                {
                    using (DataSet ds = new DataSet())
                    {
                        da.Fill(ds);
                        return ds.Tables[0];
                    }
                }
            }
        }

        #endregion
    }
}