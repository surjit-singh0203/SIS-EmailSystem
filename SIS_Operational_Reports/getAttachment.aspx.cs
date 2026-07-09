using ClosedXML.Excel;
using DataBuildingLayer;
using EAGetMail;
using MailKit;
using MailKit.Net.Imap;
//using System.Web.Services.Description;
using MailKit.Net.Pop3;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using MimeKit;
using Newtonsoft.Json.Linq;
using SIS_Operational_Reports.Common;
using SIS_Operational_Reports.Areas.Report.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
//using Microsoft.Kiota.Abstractions;
//using Renci.SshNet;



namespace SIS_Operational_Reports
{
    public partial class getAttachment : System.Web.UI.Page
    {

        private static HttpClient _httpClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //ImportData();
                GetAllAttachmentNew();
                // getAllAttachmentNew1();
                //GetAttachmentNew();
                AutoDelete();

            }
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
                    string foldermovePath = Server.MapPath("~/Archive/");
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

                        string filterPart = $"$filter=receivedDateTime ge {startDatePartString} and receivedDateTime lt {endDatePartString}&$orderby=receivedDateTime desc";

                        var responsePart = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart}").Result;
                        responsePart.EnsureSuccessStatusCode();
                        var responseContentPart = responsePart.Content.ReadAsStringAsync().Result;
                        var messagesPart = JObject.Parse(responseContentPart)["value"];

                        MoveFilesToArchive(folderPath, foldermovePath);

                        ProcessMessages(messagesPart, folderPath, httpClient, userEmail);

                        //Next 12 Hrs
                        DateTime startDatePart1 = twoDaysAgo.AddHours(6);
                        DateTime endDatePart1 = startDatePart1.AddHours(12);

                        string startDatePart1String = startDatePart1.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePart1String = endDatePart1.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart1 = $"$filter=receivedDateTime ge {startDatePart1String} and receivedDateTime lt {endDatePart1String}&$orderby=receivedDateTime desc";

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

                        string filterPart2 = $"$filter=receivedDateTime ge {startDatePart2String} and receivedDateTime lt {endDatePart2String}&$orderby=receivedDateTime desc";

                        var responsePart2 = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart2}").Result;
                        responsePart2.EnsureSuccessStatusCode();
                        var responseContentPart2 = responsePart2.Content.ReadAsStringAsync().Result;
                        var messagesPart2 = JObject.Parse(responseContentPart2)["value"];

                        MoveFilesToArchive(folderPath, foldermovePath);

                        ProcessMessages(messagesPart2, folderPath, httpClient, userEmail);


                        // Next 12 Hrs
                        DateTime startDatePart3 = endDatePart2;
                        DateTime endDatePart3 = startDatePart3.AddHours(12);

                        string startDatePart3String = startDatePart3.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endDatePart3String = endDatePart3.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterPart3 = $"$filter=receivedDateTime ge {startDatePart3String} and receivedDateTime lt {endDatePart3String}&$orderby=receivedDateTime desc";

                        var responsePart3 = httpClient.GetAsync($"users/{userEmail}/messages?{filterPart3}").Result;
                        responsePart3.EnsureSuccessStatusCode();
                        var responseContentPart3 = responsePart3.Content.ReadAsStringAsync().Result;
                        var messagesPart3 = JObject.Parse(responseContentPart3)["value"];

                        ProcessMessages(messagesPart3, folderPath, httpClient, userEmail);


                    }
                    else
                    {
                        // Dead-zone coverage: UTC 00:00-03:25 (IST 05:30-08:55). Previously this
                        // window did nothing, causing the page to "load for 1 second then stop".
                        // Process the last 48 hours of emails so manual hits during this window
                        // still pick up overnight messages.
                        DateTime windowStart = DateTime.UtcNow.AddHours(-48);
                        DateTime windowEnd   = DateTime.UtcNow;

                        string startStr = windowStart.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        string endStr   = windowEnd.ToString("yyyy-MM-ddTHH:mm:ssZ");

                        string filterDead = $"$filter=receivedDateTime ge {startStr} and receivedDateTime lt {endStr}&$orderby=receivedDateTime desc&$top=100";

                        var responseDead = httpClient.GetAsync($"users/{userEmail}/messages?{filterDead}").Result;
                        responseDead.EnsureSuccessStatusCode();
                        var responseContentDead = responseDead.Content.ReadAsStringAsync().Result;
                        var messagesDead = JObject.Parse(responseContentDead)["value"];

                        MoveFilesToArchive(folderPath, foldermovePath);
                        ProcessMessages(messagesDead, folderPath, httpClient, userEmail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read emails: {ex.Message}");
                throw;
            }
        }


        private void MoveFilesToArchive(string inboxPath, string archivePath)
        {

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            if (!Directory.Exists(inboxPath))
            {
                return;
            }

            var files = Directory.GetFiles(inboxPath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                var destFile = Path.Combine(archivePath, fileName);

                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }

                File.Move(file, destFile);
            }
        }


        private void ProcessMessages(JToken messages, string folderPath, HttpClient httpClient, string userEmail)
        {
            foreach (var message in messages)
            {
                // Check for attachments
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
                                if (!Directory.Exists(folderPath))
                                {
                                    Directory.CreateDirectory(folderPath);
                                }

                                string filePath = Path.Combine(folderPath, fileName);

                                // Messages are sorted newest-first; if this filename
                                // already exists in Inbox, a newer copy was saved this
                                // run — skip the older duplicate.
                                if (File.Exists(filePath))
                                {
                                    continue;
                                }

                                string attachmentId = (string)attachment["id"];
                                var attachmentResponse = httpClient.GetAsync($"users/{userEmail}/messages/{messageId}/attachments/{attachmentId}/$value").Result;
                                attachmentResponse.EnsureSuccessStatusCode();

                                byte[] attachmentData = attachmentResponse.Content.ReadAsByteArrayAsync().Result;

                                System.IO.File.WriteAllBytes(filePath, attachmentData);

                                // Safety net: also copy the inbound Excel into the Bunker and
                                // FreshWater attachment folders so the download link in their
                                // emails (which points to /Bunker_LabAnalysisReport/<name> or
                                // /FreshWaterReport/<name>) resolves even if the per-row file
                                // reconstruction from the BunkerReport_Files / FreshWaterReport_Files
                                // sheet didn't run. Either path may fail (permissions, missing
                                // folder) — swallow errors so one bad folder doesn't break import.
                                try
                                {
                                    string bunkerFolder = Server.MapPath("~/Bunker_LabAnalysisReport/");
                                    if (!Directory.Exists(bunkerFolder)) Directory.CreateDirectory(bunkerFolder);
                                    System.IO.File.WriteAllBytes(Path.Combine(bunkerFolder, fileName), attachmentData);
                                }
                                catch { }
                                try
                                {
                                    string freshWaterFolder = Server.MapPath("~/FreshWaterReport/");
                                    if (!Directory.Exists(freshWaterFolder)) Directory.CreateDirectory(freshWaterFolder);
                                    System.IO.File.WriteAllBytes(Path.Combine(freshWaterFolder, fileName), attachmentData);
                                }
                                catch { }

                                // Call your import data method if needed
                                ImportData();
                            }
                        }
                    }
                }
            }
        }


        public void getAllAttachmentNew()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    string folderPath = Server.MapPath("~/Inbox/");
                    DateTime twoDaysAgo = DateTime.UtcNow.AddDays(-1).Date;

                    string userEmail = ConfigurationManager.AppSettings["ida:GraphMail"];
                    httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                    string token = GetAccessToken();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Calculate the date 2 days ago
                    //DateTime twoDaysAgo = DateTime.UtcNow.AddDays(-2);
                    string filter = $"$filter=receivedDateTime ge {twoDaysAgo.ToString("o")}";

                    var response = httpClient.GetAsync($"users/{userEmail}/messages?{filter}").Result;
                    response.EnsureSuccessStatusCode();

                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var messages = JObject.Parse(responseContent)["value"];


                    foreach (var message in messages)
                    {
                        // Check for attachments
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
                                        if (fileName == "Sis_Nova_02062024_Export_9544592")
                                        {

                                        }
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to read emails: {ex.Message}");
                throw;
            }
        }


        private void getAllAttachmentNew2()
        {
            string location = Server.MapPath("~/Inbox/");
            string dt = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            var email = System.Configuration.ConfigurationManager.AppSettings["User"];
            var password = System.Configuration.ConfigurationManager.AppSettings["Password"];

            using (var client = new ImapClient())
            {
                client.Connect("outlook.office365.com", 993, SecureSocketOptions.SslOnConnect);
                client.Authenticate(email, password);

                client.Inbox.Open(FolderAccess.ReadWrite);
                IList<UniqueId> uids = client.Inbox.Search(SearchQuery.All);

                var query1 = client.Inbox.Search(SearchQuery.DeliveredAfter(DateTime.Parse(dt)));

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

                            using (var stream = File.Create(location))
                                rfc822.Message.WriteTo(stream);
                        }
                        else
                        {
                            var part = (MimePart)attachment;
                            var fileName2 = part.FileName;

                            using (var stream = File.Create(location + fileName2))
                                part.Content.DecodeTo(stream);

                            ImportData();
                        }
                    }

                }
            }
        }



        //private void getAllAttachmentNew()
        //{
        //    string location = Server.MapPath("~/Inbox/");
        //    string dt = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");

        //    using (var client = new ImapClient())
        //    {

        //        client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        //        client.AuthenticationMechanisms.Remove("XOAUTH2");

        //        client.Authenticate("sisnova@sishipping.com", "zedinmucmddngkzr");


        //        client.Inbox.Open(FolderAccess.ReadWrite);
        //        IList<UniqueId> uids = client.Inbox.Search(SearchQuery.All);

        //        var query1 = client.Inbox.Search(SearchQuery.DeliveredAfter(DateTime.Parse(dt)));

        //        foreach (UniqueId uid in query1)
        //        {
        //            MimeMessage message = client.Inbox.GetMessage(uid);

        //            foreach (MimeEntity attachment in message.Attachments.Where(x => x.ContentType.Name.Contains("Sis_Nova") && x.ContentType.Name != null).ToList())
        //            {
        //                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
        //                if (attachment is MessagePart)
        //                {
        //                    var fileName2 = attachment.ContentDisposition?.FileName;
        //                    var rfc822 = (MessagePart)attachment;

        //                    if (string.IsNullOrEmpty(fileName2))
        //                        fileName2 = "attached-message.eml";

        //                    using (var stream = File.Create(location))
        //                        rfc822.Message.WriteTo(stream);
        //                }
        //                else
        //                {
        //                    var part = (MimePart)attachment;
        //                    var fileName2 = part.FileName;

        //                    using (var stream = File.Create(location + fileName2))
        //                        part.Content.DecodeTo(stream);

        //                    ImportData();
        //                }
        //            }

        //        }
        //    }
        //}

        public static void DownloadMessages()
        {
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);

                client.Authenticate("ws1.49web@gmail.com", "inyemftdduwdtnkp");

                client.Inbox.Open(FolderAccess.ReadOnly);

                var uids = client.Inbox.Search(SearchQuery.All);

                var query = SearchQuery.SubjectContains("Sis_Nova").Or(SearchQuery.BodyContains("Sis_Nova"));

                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);

                    // write the message to a file
                    message.WriteTo(string.Format("{0}.eml", uid));
                }

                client.Disconnect(true);
            }

            //using (var client = new ImapClient(new ProtocolLogger("imap.log")))
            //{
            //    client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);

            //    client.Authenticate("ws1.49web@gmail.com", "inyemftdduwdtnkp");
            //    //client.Authenticate("username", "password");

            //    client.Inbox.Open(FolderAccess.ReadOnly);


            //    //client.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.DateRange;

            //    //client.GetMailInfosParam.DateRange.SINCE = System.DateTime.Now.AddDays(-1);
            //    //client.GetMailInfosParam.DateRange.BEFORE = System.DateTime.Now.AddDays(1);

            //    var uids = client.Inbox.Search(SearchQuery.All);

            //    foreach (var uid in uids)
            //    {
            //        var message = client.Inbox.GetMessage(uid);

            //        // write the message to a file
            //        message.WriteTo(string.Format("{0}.eml", uid));
            //    }

            //    client.Disconnect(true);
            //}
        }

        private void getAllAttachment()
        {
            //=== inyemftdduwdtnkp >> this is app password which is generated through gmail settings
            //==== for refrense use below link
            //  https://devanswers.co/create-application-specific-password-gmail/

            // MailServer oServer = new MailServer("imap.gmail.com", "ws1.49web@gmail.com", "inyemftdduwdtnkp", ServerProtocol.Imap4);

            MailServer oServer = new MailServer("imap.gmail.com", "sisnova@sishipping.com", "zedinmucmddngkzr", ServerProtocol.Imap4);
            //MailServer oServer = new MailServer("imap.gmail.com", "qa1.49web@gmail.com", "tdmdloklarlzrfxt", ServerProtocol.Imap4);
            MailClient oClient = new MailClient("TryIt");

            oServer.SSLConnection = true;
            oServer.Port = 993;
            //oServer.SSLConnection = false;
            //oServer.Port = 143;
            //oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;
            oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.DateRange;

            oClient.GetMailInfosParam.DateRange.SINCE = System.DateTime.Now.AddDays(-1);
            oClient.GetMailInfosParam.DateRange.BEFORE = System.DateTime.Now.AddDays(1);

            oClient.Connect(oServer);
            MailInfo[] infos = oClient.GetMailInfos();
            for (int i = infos.Length - 1; i > 0; i--)
            {
                MailInfo info = infos[i];
                Mail oMail = oClient.GetMail(info);

                var count = oMail.Attachments.ToList().Count;
                var count2 = oMail.Attachments.Where(x => x.Name.Contains("Sis_Nova")).ToList().Count;

                //var count1 = oMail.Search(SearchQuery.CcContains("b.c@gmail.com")).Count;
                for (int j = 0; j < count2; j++)
                {
                    oMail.Attachments[j].SaveAs(Server.MapPath("~/Inbox") + "\\" + oMail.Attachments[j].Name, true); // true for overWrite file
                    ImportData();
                }
            }
        }

        int checkError = 0;
        private List<string> _savedReportFilesForCurrentImport = new List<string>();

        // Per (reportType|vesselId), tracks the path of the file whose source row in the imported
        // Excel had the maximum ModifiedDate. SendImportCompletionEmail attaches only these winner
        // files. Cleared at the start of each .eml import below.
        private Dictionary<string, string> _latestFilePathPerGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DateTime> _latestModDatePerGroup = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        // Every per-row Excel saved during this import, paired with that row's ModifiedDate.
        // Used by SendImportCompletionEmail to give first-time recipients (LastSent == NULL)
        // the full set of files, while returning recipients keep getting only the winners above.
        private List<KeyValuePair<string, DateTime>> _allSavedFiles = new List<KeyValuePair<string, DateTime>>();

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
                    _savedReportFilesForCurrentImport.Clear();
                    _latestFilePathPerGroup.Clear();
                    _latestModDatePerGroup.Clear();
                    _allSavedFiles.Clear();
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

                            // Attachment-file sheets carry chunked base64 PDFs/Excels for
                            // Bunker (LabAnalysisReport) and FreshWater (File_Name). Reconstruct
                            // them HERE so they're saved before any other processing — and
                            // independent of whether the regular if/else-if chain in
                            // InsertImportedData processes the same sheet. Reads cells directly
                            // through ClosedXML so very long base64 strings don't get truncated
                            // by the generic row-loader below.
                            if (sheetName.Equals("BunkerReport_Files", StringComparison.OrdinalIgnoreCase))
                            {
                                try { ExtractAttachmentSheetDirect(workSheet, Server.MapPath("~/Bunker_LabAnalysisReport/"), "BunkerReport_Files"); }
                                catch (Exception ex) { LogReportExport("BunkerReport_Files", 0, "", "", "ERROR-Direct: " + ex.Message); }
                                continue;
                            }
                            if (sheetName.Equals("FreshWaterReport_Files", StringComparison.OrdinalIgnoreCase))
                            {
                                try { ExtractAttachmentSheetDirect(workSheet, Server.MapPath("~/FreshWaterReport/"), "FreshWaterReport_Files"); }
                                catch (Exception ex) { LogReportExport("FreshWaterReport_Files", 0, "", "", "ERROR-Direct: " + ex.Message); }
                                continue;
                            }

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

                        if (_savedReportFilesForCurrentImport.Count > 0)
                        {
                            try { SendImportCompletionEmail(_savedReportFilesForCurrentImport, m.Name); }
                            catch (Exception exEmail) { /* log if needed */ }
                        }

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
        private string InsertImportedData(string sheetName, System.Data.DataTable tbls)
        {
            try
            {
                checkError = 0;
                string connectionString = Convert.ToString(ConfigurationManager.ConnectionStrings["SISContext"]);
                if (sheetName == "ArrivalReport")
                {
                    object maxDate = tbls.Compute("MAX(ModifiedDate)", null);
                }
                if (sheetName == "DailyNoonReport" || sheetName == "ArrivalReport" || sheetName == "DepartureReport" || sheetName == "BerthingReport")
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
                //else if (sheetName == "Fuel_Cons_NR")
                //{
                //    for (int i = 0; i < tbls.Rows.Count; i++)
                //    {
                //        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                //        int Noon_Report_Id = Convert.ToInt32(tbls.Rows[i]["Noon_Report_Id"]);
                //        int FuelTypeId = Convert.ToInt32(tbls.Rows[i]["FuelTypeId"]);
                //        int ConsTypeId = Convert.ToInt32(tbls.Rows[i]["ConsTypeId"]);
                //        decimal Value = Convert.ToDecimal(tbls.Rows[i]["Value"]);
                //        int ReportType_Id = Convert.ToInt32(tbls.Rows[i]["ReportType_Id"]);
                //        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                //        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertUpdate_FuelConsNR", ConnectionBulder.con))
                //        {
                //            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                //            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@Noon_Report_Id", Noon_Report_Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@FuelTypeId", FuelTypeId);
                //            adapter.SelectCommand.Parameters.AddWithValue("@ConsTypeId", ConsTypeId);
                //            adapter.SelectCommand.Parameters.AddWithValue("@Value", Value);
                //            adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                //            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                //            using (DataTable dataTable = new DataTable())
                //            {
                //                adapter.Fill(dataTable);

                //            }
                //        }
                //    }
                //}
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

                else if (sheetName == "tbl_BunkerLReceipt_NoonReport")
                {
                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int FuelType_Id = Convert.ToInt32(tbls.Rows[i]["FuelType_Id"]);
                        //string EOSP = tbls.Rows[i]["EOSP"].ToString();
                        // int FWE = Convert.ToInt32(tbls.Rows[i]["FWE"]);
                        decimal Receipt = Convert.ToDecimal(tbls.Rows[i]["Receipt"]);
                        int TableMax_Id = Convert.ToInt32(tbls.Rows[i]["TableMax_Id"]);
                        int ReportType_Id = Convert.ToInt32(tbls.Rows[i]["ReportType_Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);

                        using (SqlDataAdapter adapter = new SqlDataAdapter("tbl_BunkerLReceipt_NoonReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@FuelType_Id", FuelType_Id);
                            // adapter.SelectCommand.Parameters.AddWithValue("@EOSP", EOSP);
                            // adapter.SelectCommand.Parameters.AddWithValue("@FWE", FWE);
                            adapter.SelectCommand.Parameters.AddWithValue("@Receipt", Receipt);
                            adapter.SelectCommand.Parameters.AddWithValue("@TableMax_Id", TableMax_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@ReportType_Id", ReportType_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }
                else if (sheetName == "BunkerReport")
                {
                    //string zipFilePath = @"C:\Users\Developer\Desktop\files.zip";
                    ////string folderPath = @"E:\Neeraj\SiS_Nova\SIS Office Latest\SIS_Office_Reports\SIS_Operational_Reports\Bunker_LabAnalysisReport";
                    //string folderPath = Server.MapPath(string.Format("~/Bunker_LabAnalysisReport"));

                    //if (System.IO.File.Exists(zipFilePath))
                    //{
                    //    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                    //    {
                    //        foreach (ZipArchiveEntry entry in archive.Entries)
                    //        {
                    //            if (Path.GetExtension(entry.Name).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    //            {
                    //                string fileToExtract = Path.Combine(folderPath, entry.Name);
                    //                entry.ExtractToFile(fileToExtract, true);
                    //            }
                    //        }
                    //    }
                    //}

                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {
                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        string PortName = (tbls.Rows[i]["PortName"]).ToString();
                        int VoyageId = Convert.ToInt32(tbls.Rows[i]["VoyageId"]);
                        string Supplier = (tbls.Rows[i]["Supplier"]).ToString();
                        string BargeAlongside = (tbls.Rows[i]["BargeAlongside"]).ToString();
                        string BunkerHoseConnected = (tbls.Rows[i]["BunkerHoseConnected"]).ToString();
                        string CommencedBunkering = (tbls.Rows[i]["CommencedBunkering"]).ToString();
                        string BunkeringCompleted = (tbls.Rows[i]["BunkeringCompleted"]).ToString();
                        string BunkerHosedisconnected = (tbls.Rows[i]["BunkerHosedisconnected"]).ToString();
                        string BargeCastOff = (tbls.Rows[i]["BargeCastOff"]).ToString();
                        string BargeName = (tbls.Rows[i]["BargeName"]).ToString();
                        string Remarks = (tbls.Rows[i]["Remarks"]).ToString();
                        string LabAnalysisReport_Name = (tbls.Rows[i]["LabAnalysisReport_Name"]).ToString();
                        string FirstName = (tbls.Rows[i]["FirstName"]).ToString();
                        string LastName = (tbls.Rows[i]["LastName"]).ToString();
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);
                        string Created_Date = (tbls.Rows[i]["Created_Date"]).ToString();
                        string Modified_Date = "";
                        if (tbls.Rows[i]["Modified_Date"] != null)
                        {
                            Modified_Date = (tbls.Rows[i]["Modified_Date"]).ToString();
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter("InsertUpdateBunkerReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            adapter.SelectCommand.Parameters.AddWithValue("@PortName", PortName);
                            adapter.SelectCommand.Parameters.AddWithValue("@VoyageId", VoyageId);
                            adapter.SelectCommand.Parameters.AddWithValue("@Supplier", Supplier);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeAlongside", BargeAlongside);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkerHoseConnected", BunkerHoseConnected);
                            adapter.SelectCommand.Parameters.AddWithValue("@CommencedBunkering", CommencedBunkering);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkeringCompleted", BunkeringCompleted);
                            adapter.SelectCommand.Parameters.AddWithValue("@BunkerHosedisconnected", BunkerHosedisconnected);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeCastOff", BargeCastOff);
                            adapter.SelectCommand.Parameters.AddWithValue("@BargeName", BargeName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Remarks", Remarks);
                            adapter.SelectCommand.Parameters.AddWithValue("@LabAnalysisReport_Name", LabAnalysisReport_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@FirstName", FirstName);
                            adapter.SelectCommand.Parameters.AddWithValue("@LastName", LastName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Created_Date", Created_Date);
                            adapter.SelectCommand.Parameters.AddWithValue("@Modified_Date", Modified_Date);

                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }

                    // BunkerReport is handled by its dedicated branch above (InsertUpdateBunkerReport SP)
                    // and therefore never falls into the trailing `else` block where DailyNoon/Arrival/etc.
                    // get their Save*ReportExcelToFiles call. Generate the per-row Excel here instead so
                    // SendImportCompletionEmail can pick it up.
                    try { SaveBunkerReportExcelToFiles(tbls); }
                    catch (Exception exBunker) { }
                }

                else if (sheetName == "FreshWaterReport")
                {

                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {

                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        string PortName = tbls.Rows[i]["PortName"].ToString();
                        string Facility_Name = tbls.Rows[i]["Facility_Name"].ToString();
                        string VendorDetails = tbls.Rows[i]["VendorDetails"].ToString();
                        decimal Intial_Meter_Reading_MT_supplied = Convert.ToDecimal(tbls.Rows[i]["Intial_Meter_Reading_MT_supplied"]);
                        decimal Final_Meter_Reading_MT = Convert.ToDecimal(tbls.Rows[i]["Final_Meter_Reading_MT"]);
                        decimal Difference_in_Meter_Reading_MT = Convert.ToDecimal(tbls.Rows[i]["Difference_in_Meter_Reading_MT"]);
                        decimal QTY_supplied_MT = Convert.ToDecimal(tbls.Rows[i]["QTY_supplied_MT"]);
                        string File_Name = (tbls.Rows[i]["File_Name"]).ToString();
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        string Received_Date = tbls.Rows[i]["Received_Date"].ToString();
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);
                        string Created_Date = (tbls.Rows[i]["Created_Date"]).ToString();
                         string Modified_Date = (tbls.Rows[i]["Modified_Date"]).ToString();

                        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertFreshWaterReport", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@PortName", PortName);
                            adapter.SelectCommand.Parameters.AddWithValue("@Facility_Name", Facility_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@VendorDetails", VendorDetails);
                            adapter.SelectCommand.Parameters.AddWithValue("@Intial_Meter_Reading_MT_supplied", Intial_Meter_Reading_MT_supplied);
                            adapter.SelectCommand.Parameters.AddWithValue("@Final_Meter_Reading_MT", Final_Meter_Reading_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@Difference_in_Meter_Reading_MT", Difference_in_Meter_Reading_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@QTY_supplied_MT", QTY_supplied_MT);
                            adapter.SelectCommand.Parameters.AddWithValue("@File_Name", File_Name);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            adapter.SelectCommand.Parameters.AddWithValue("@Received_Date", Received_Date);
                            adapter.SelectCommand.Parameters.AddWithValue("@Created_Date", Created_Date);
                            adapter.SelectCommand.Parameters.AddWithValue("@Modified_Date", Modified_Date);

                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }

                    // Same reason as BunkerReport above: FreshWaterReport short-circuits the if/else-if
                    // chain via its dedicated branch, so the Save call in the trailing `else` block never
                    // runs for it. Trigger the per-row Excel generation here.
                    try { SaveFreshWaterReportExcelToFiles(tbls); }
                    catch (Exception exFreshWater) { }
                }

                // Attachment-file sheets: each row is a base64-encoded chunk (FileName + PartIndex
                // + FileData). Reassemble the original PDF / image and write it to the static folder
                // the email templates link to. Scope is intentionally limited to Bunker + FreshWater
                // per user request — other reports keep their current behavior. Sheet-name match is
                // case-insensitive so spelling variants (BunkerReport_files, bunkerreport_files, etc.)
                // still trigger the reconstruction. Errors are logged (not swallowed) so we can
                // diagnose why files aren't appearing on disk.
                else if (sheetName.Equals("BunkerReport_Files", StringComparison.OrdinalIgnoreCase))
                {
                    try { SaveAttachmentFilesFromSheet(tbls, Server.MapPath("~/Bunker_LabAnalysisReport/"), "BunkerReport_Files"); }
                    catch (Exception ex) { LogReportExport("BunkerReport_Files", 0, "", "", "ERROR-Outer: " + ex.Message); }
                }
                else if (sheetName.Equals("FreshWaterReport_Files", StringComparison.OrdinalIgnoreCase))
                {
                    try { SaveAttachmentFilesFromSheet(tbls, Server.MapPath("~/FreshWaterReport/"), "FreshWaterReport_Files"); }
                    catch (Exception ex) { LogReportExport("FreshWaterReport_Files", 0, "", "", "ERROR-Outer: " + ex.Message); }
                }

                else if (sheetName == "tbl_BunkerFuelType")
                {

                    for (int i = 0; i < tbls.Rows.Count; i++)
                    {

                        int Id = Convert.ToInt32(tbls.Rows[i]["Id"]);
                        int Fuel_type_Id = Convert.ToInt32(tbls.Rows[i]["Fuel_type_Id"]);
                        decimal BDN = Convert.ToDecimal(tbls.Rows[i]["BDN"]);
                        decimal Fuel_Density = Convert.ToDecimal(tbls.Rows[i]["Fuel_Density"]);
                        decimal Sulphur_content = Convert.ToDecimal(tbls.Rows[i]["Sulphur_content"]);
                        string BDN_Number = (tbls.Rows[i]["BDN_Number"]).ToString();
                        int MaxR_Id = Convert.ToInt32(tbls.Rows[i]["MaxR_Id"]);
                        int VesselId = Convert.ToInt32(tbls.Rows[i]["VesselId"]);
                        bool Is_Active = Convert.ToBoolean(tbls.Rows[i]["Is_Active"]);

                        using (SqlDataAdapter adapter = new SqlDataAdapter("spInsertBunkerFuelType", ConnectionBulder.con))
                        {
                            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                            adapter.SelectCommand.Parameters.AddWithValue("@Is_Active", Is_Active);
                            adapter.SelectCommand.Parameters.AddWithValue("@Id", Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@Fuel_type_Id", Fuel_type_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@BDN", BDN);
                            adapter.SelectCommand.Parameters.AddWithValue("@Fuel_Density", Fuel_Density);
                            adapter.SelectCommand.Parameters.AddWithValue("@Sulphur_content", Sulphur_content);
                            adapter.SelectCommand.Parameters.AddWithValue("@BDN_Number", BDN_Number);
                            adapter.SelectCommand.Parameters.AddWithValue("@MaxR_Id", MaxR_Id);
                            adapter.SelectCommand.Parameters.AddWithValue("@VesselId", VesselId);
                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
                }

                else
                {

                    if (sheetName == "DailyNoonReport")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 92)
                        {
                            tbls.Columns.Add("BR_Extra_Run_Reason1", typeof(string)).DefaultValue = "N/A";
                            tbls.Columns.Add("BR_Extra_Run_Reason2", typeof(string)).DefaultValue = "N/A";

                            tbls.Columns.Add("BR_RungHrs_No1", typeof(decimal)).DefaultValue = 0;
                            tbls.Columns.Add("BR_RungHrs_No2", typeof(decimal)).DefaultValue = 0;
                            tbls.Columns.Add("AE_RungHrs_No4", typeof(string)).DefaultValue = 0;
                            tbls.Columns.Add("AE_Load_No4", typeof(string)).DefaultValue = 0;
                        }

                    }

                    if (sheetName == "LR_Cargo")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 19)
                        {
                            tbls.Columns.Add("EstCompDateTime", typeof(DateTime)).DefaultValue = DBNull.Value;

                        }

                    }

                    if (sheetName == "DS_Cargo")
                    {
                        int checkCount = tbls.Columns.Count;

                        if (checkCount == 21)
                        {
                            tbls.Columns.Add("EstCompDateTime", typeof(DateTime)).DefaultValue = DBNull.Value;

                        }

                    }

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

                    // Generate and save Daily Noon Report Excel to Files folder (after Update)
                    if (sheetName == "DailyNoonReport")
                    {
                        try
                        {
                            SaveDailyNoonReportExcelToFiles(tbls);
                        }
                        catch (Exception exDailyNoon) { }
                    }

                    //Generate and save Arrival Report Excel to Files folder(after Update)
                    if (sheetName == "ArrivalReport")
                    {
                        try
                        {
                            SaveArrivalReportExcelToFiles(tbls);
                        }
                        catch (Exception exArrival) { }
                    }

                    // Generate and save Departure Report Excel to Files folder (after Update)
                    if (sheetName == "DepartureReport")
                    {
                        try
                        {
                            SaveDepartureReportExcelToFiles(tbls);
                        }
                        catch (Exception exDeparture) { }
                    }

                    // Generate and save Berthing Report Excel to Files folder (after Update)
                    if (sheetName == "BerthingReport")
                    {
                        try
                        {
                            SaveBerthingReportExcelToFiles(tbls);
                        }
                        catch (Exception exBerthing) { }
                    }

                    // Generate and save Loading Report Excel to Files folder(after Update)
                    if (sheetName == "LoadingReport")
                    {
                        try
                        {
                            SaveLoadingReportExcelToFiles(tbls);
                        }
                        catch (Exception exLoading) { }
                    }

                    // Generate and save Discharging Report Excel to Files folder (after Update)
                    if (sheetName == "DischargingReport")
                    {
                        try
                        {
                            SaveDischargingReportExcelToFiles(tbls);
                        }
                        catch (Exception exDischarging) { }
                    }



                    // BunkerReport and FreshWaterReport save calls live inside their dedicated branches
                    // earlier in the if/else-if chain — they short-circuit and never reach this block.


                    ///////////////////////////////////////////////////////////////

                    //// Generate and save Noon Report Allow Excel to Files folder (after Update)
                    //if (sheetName == "NoonReport_allow")
                    //{
                    //    try
                    //    {
                    //        SaveNoonReportAllowExcelToFiles(tbls);
                    //    }
                    //    catch (Exception exNoonReportAllow) { }
                    //}
                    //// Generate and save Bulk Noon Report Excel to Files folder (after Update)
                    //if (sheetName == "DailyNoonReport")
                    //{
                    //    try
                    //    {
                    //        SaveBulkNoonReportExcelToFiles(tbls);
                    //    }
                    //    catch (Exception exBulkNoon) { }
                    //}

                    //// Generate and save Consumption Report Excel to Files folder (after Update)
                    //if (sheetName == "DailyNoonReport")
                    //{
                    //    try
                    //    {
                    //        SaveConsumptionReportExcelToFiles(tbls);
                    //    }
                    //    catch (Exception exConsumption) { }
                    //}
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

        /// <summary>
        /// Generates Daily Noon Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses EditFromDashboard logic (reportdate, vesselid) to fetch all details.
        /// </summary>
        private void SaveDailyNoonReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasDate = tbls.Columns.Contains("Date");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            if (!hasVesselId || (!hasDate && !hasModifiedDate)) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasDate && row["Date"] != DBNull.Value && row["Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Date"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new DailyNoonReport();
                vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselId, "DailyNoonReport");
                var noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (noonRBind == null) continue;

                int id = noonRBind.Id;
                var cargoTanks = DailyNoonController.GetCargoTankList(id, vesselId);
                var ballastTanks = DailyNoonController.GetBallastTankList(id, vesselId);
                var voidSpaces = DailyNoonController.GetVoid_SpaceList(id, vesselId);

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtNRCargo = new DataTable();

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    if (dtFuelROB.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, OtherROB from tbl_FuelROB where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                        {
                            DataTable dtRob = new DataTable();
                            adp.Fill(dtRob);
                            foreach (DataRow r in dtRob.Rows)
                            {
                                int ftId = Convert.ToInt32(r["FuelType_Id"]);
                                var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                                string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                                dtFuelROB.Rows.Add(fuelType, r["OtherROB"]?.ToString() ?? "");
                            }
                        }
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                            adp.Fill(dtNRCargo);
                    }
                    catch
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from NR_Cargo a left join LR_Cargo b on a.lr_cargo_id=b.Id where a.VesselId=" + vesselId + " and a.NoonReport_Id=" + id, ConnectionBulder.con))
                            adp.Fill(dtNRCargo);
                    }
                }
                catch { }

                DataTable dtMain = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", noonRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "DailyNoonReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddDailyNoonNavigationSheet(wb, noonRBind, dtNonRoutine, dtMain);
                    AddDailyNoonEngineSheet(wb, noonRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddDailyNoonCargoSheet(wb, noonRBind, cargoTanks, ballastTanks, voidSpaces, dtNRCargo);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private const string ExcelDateFormat = "yyyy-MM-dd";
        private const string ExcelDateTimeFormat = "yyyy-MM-dd HH:mm";

        private void AddDailyNoonNavigationSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = r.LegPortName ?? "";
                string portStatusText = r.PortStatus?.ToString() ?? "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber"))
                        voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                    if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
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
                // Resolve Leg from VoyageLeg scoped by LegPortId+VoyageId+VesselId.
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }
                // Resolve "In Port Status" label from PortStatus FK so the Excel shows
                // "Others" instead of the raw id "7" (matches the dropdown text on the form).
                if (string.IsNullOrWhiteSpace(portStatusText) || int.TryParse(portStatusText, out _))
                {
                    try
                    {
                        if (r.PortStatus.HasValue && r.PortStatus.Value > 0)
                        {
                            var portStatusList = DailyNoonReport.portSList();
                            var psMatch = portStatusList?.FirstOrDefault(p => p.Id == r.PortStatus.Value);
                            if (psMatch != null && !string.IsNullOrWhiteSpace(psMatch.Status))
                                portStatusText = psMatch.Status.Trim();
                        }
                    }
                    catch { }
                }
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Status", r.VesselStatus ?? "");
                AddKeyValueRow(ws, ref row, "Latitude", FormatLatLonForExcel(r.Latitude));
                AddKeyValueRow(ws, ref row, "Longitude", FormatLatLonForExcel(r.Longitude));
                AddKeyValueRow(ws, ref row, "At Sea/In Port", r.AtSeaOrPort ?? "");
                AddKeyValueRow(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueRow(ws, ref row, "Displacement(MT)", r.Displacement);
                AddKeyValueRow(ws, ref row, "CP Speed(Kts)", r.CP_Speed);
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date", r.Date != null ? Convert.ToDateTime(r.Date).ToString(ExcelDateFormat) : "");
                AddKeyValueRow(ws, ref row, "ETA", r.ETA != null ? Convert.ToDateTime(r.ETA).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Speed - Distance - Time";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist);
                AddKeyValueRow(ws, ref row, "Log Dist(NM)", r.LogDist);
                AddKeyValueRow(ws, ref row, "Engine Dist(NM)", r.EngineDist);
                AddKeyValueRow(ws, ref row, "Total Distance (Dep to Curr)(NM)", r.TotalDistance);
                AddKeyValueRow(ws, ref row, "Dist to Go (DTG)(NM)", r.DistToGo_DTG);
                AddKeyValueRow(ws, ref row, "Stmg Time Noon to Noon(Hrs)", r.StmgTime);
                AddKeyValueRow(ws, ref row, "Total Time (Dep to Curr)(Hrs)", r.TotalTime);
                AddKeyValueRow(ws, ref row, "Actual Speed Noon to Noon(Kts)", r.Act_Speed);
                AddKeyValueRow(ws, ref row, "Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Event Name";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Noon Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddDailyNoonEngineSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 9);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueRow(ws, ref row, "RPM", r.RPM);
                AddKeyValueRow(ws, ref row, "BHP(hp)", r.BHP);
                AddKeyValueRow(ws, ref row, "MCR%", r.MCR);
                AddKeyValueRow(ws, ref row, "M/E Control Location", r.ME_ControlLoc);
                AddKeyValueRow(ws, ref row, "SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress);
                AddKeyValueRow(ws, ref row, "SCAV. Temp (Deg Centigrade)", r.SCAV_Temp);
                AddKeyValueRow(ws, ref row, "Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp);
                AddKeyValueRow(ws, ref row, "Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp);
                AddKeyValueRow(ws, ref row, "SW Temp (Deg Centigrade)", r.SW_Temp);
                AddKeyValueRow(ws, ref row, "ER Temp (Deg Centigrade)", r.ER_Temp);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string robVal = dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"]?.ToString() : "";
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal ?? "");
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Bunker Received in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty);
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Aux. Engine";
            ApplyLightGrayTitle(ws, row, 1, 6);
            row++;
            if (r != null)
            {
                // Grid: columns No.1/No.2/No.3/No.4/Shaft Gen; rows Running Hrs, Load (KW), Extra Run
                // Reason. Mirrors the Daily Noon email + web view. Running Hrs are decimal(18,2) -> "0.00";
                // Load (KW) are decimal(18,3) -> "0.000" (shown as stored).
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "No. 1";
                ws.Cell(row, 3).Value = "No. 2";
                ws.Cell(row, 4).Value = "No. 3";
                ws.Cell(row, 5).Value = "No. 4";
                ws.Cell(row, 6).Value = "Shaft Gen";
                ws.Range(row, 1, row, 6).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Running Hrs";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.AE_RungHrs_No1, "0.00");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.AE_RungHrs_No2, "0.00");
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.AE_RungHrs_No3, "0.00");
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), r.AE_RungHrs_No4, "0.00");
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), r.AE_RungHrs_ShaftGen, "0.00");
                row++;
                ws.Cell(row, 1).Value = "Load (KW)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.AE_Load_No1, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.AE_Load_No2, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.AE_Load_No3, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), r.AE_Load_No4, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), r.AE_Load_ShaftGen, "0.000");
                row++;
                ws.Cell(row, 1).Value = "Extra Run Reason";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.AE_Extra_Run_Reason);
                ws.Range(row, 2, row, 6).Merge();
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Consumption";
                ws.Cell(row, 3).Value = "ROB";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "MECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECC);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECC_ROB);
                row++;
                ws.Cell(row, 1).Value = "MECYL (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECYL);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECYL_ROB);
                row++;
                ws.Cell(row, 1).Value = "AECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_AECC);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_AECC_ROB);
                row++;
                ws.Cell(row, 1).Value = "HYDRAULIC Oil (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_HYDR_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_HYDR_Oil_ROB);
                row++;
            }
            row++;

            // E/R Tanks — rendered immediately after LO & HO Consumptions per the
            // engine-section grouping. Fields come from DailyNoonReport (ER_Bilge_ROB,
            // ER_Sludge_ROB, ER_WasteOil_ROB).
            ws.Cell(row, 1).Value = "E/R Tanks";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Bilge";
                ws.Cell(row, 3).Value = "Sludge";
                ws.Cell(row, 4).Value = "Waste Oil";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.ER_Bilge_ROB);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.ER_Sludge_ROB);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.ER_WasteOil_ROB);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Boiler's";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                // Grid: columns Boiler No. 1 / Boiler No. 2; rows Running Hrs (decimal(18,2) -> "0.00",
                // shown as stored) and Extra Run Reason. Mirrors the Daily Noon email + web view.
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Boiler No. 1";
                ws.Cell(row, 3).Value = "Boiler No. 2";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Running Hrs";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.BR_RungHrs_No1, "0.00");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.BR_RungHrs_No2, "0.00");
                row++;
                ws.Cell(row, 1).Value = "Extra Run Reason";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.BR_Extra_Run_Reason1);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.BR_Extra_Run_Reason2);
                ws.Range(row, 2, row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                row++;
            }
            row++;

            // Fuel Consumption in MT — full layout mirroring email template:
            // Main Engine, Aux Engine, Boiler, FRAMO, IGG & Incinerator, Events, Total
            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 9);
            row++;
            AddFuelConsEngineBlock(ws, ref row, "Main Engine", true, dtFuelCons, 2, 3, 4, 5);
            AddFuelConsEngineBlock(ws, ref row, "Aux Engine", true, dtFuelCons, 7, 8, 9, 10);
            AddFuelConsEngineBlock(ws, ref row, "Boiler", false, dtFuelCons, 11, 12, 13, 14);
            AddFuelConsEngineBlock(ws, ref row, "Framo System", false, dtFuelCons, 15, 16, 17, 18);
            AddFuelConsIggIncBlock(ws, ref row, dtFuelCons);
            AddFuelConsEventsBlock(ws, ref row, dtFuelCons);
            AddFuelConsTotalBlock(ws, ref row, dtFuelCons);

            ws.Columns().AdjustToContents();
        }

        /// <summary>Writes an engine-style fuel consumption block (Main/Aux/Boiler/FRAMO):
        /// title row, header (Fuel | At Sea | MANOEUV | Anchor/Wait | Berth [| Sub Total]), VLSFO row, MDO row.</summary>
        private void AddFuelConsEngineBlock(IXLWorksheet ws, ref int row, string title, bool showSubTotal, DataTable dtFuelCons, int ctSea, int ctMan, int ctWait, int ctBerth)
        {
            int cols = showSubTotal ? 6 : 5;
            ws.Cell(row, 1).Value = title;
            ApplyLightGrayTitle(ws, row, 1, cols);
            row++;
            ws.Cell(row, 1).Value = "Fuel";
            ws.Cell(row, 2).Value = "AT SEA";
            ws.Cell(row, 3).Value = "MANOEUV";
            ws.Cell(row, 4).Value = "ANCHOR/WAIT";
            ws.Cell(row, 5).Value = "BERTH";
            if (showSubTotal) ws.Cell(row, 6).Value = "SUB TOTAL";
            ws.Range(row, 1, row, cols).Style.Font.Bold = true;
            row++;
            string[] fuels = { "VLSFO", "MDO" };
            foreach (string ft in fuels)
            {
                decimal vSea = GetFuelConsByTypeAndConsType(dtFuelCons, ft, ctSea);
                decimal vMan = GetFuelConsByTypeAndConsType(dtFuelCons, ft, ctMan);
                decimal vWait = GetFuelConsByTypeAndConsType(dtFuelCons, ft, ctWait);
                decimal vBerth = GetFuelConsByTypeAndConsType(dtFuelCons, ft, ctBerth);
                ws.Cell(row, 1).Value = ft;
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), vSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), vMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), vWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), vBerth);
                if (showSubTotal)
                {
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6), vSea + vMan + vWait + vBerth);
                    ws.Cell(row, 6).Style.Font.Bold = true;
                }
                row++;
            }
            row++;
        }

        /// <summary>Writes the IGG & Incinerator block: title row, header (Fuel | IGG | Incinerator), VLSFO row, MDO row.
        /// IGG=ConsTypeId 19, Incinerator=ConsTypeId 28.</summary>
        private void AddFuelConsIggIncBlock(IXLWorksheet ws, ref int row, DataTable dtFuelCons)
        {
            ws.Cell(row, 1).Value = "IGG & Incinerator";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Fuel";
            ws.Cell(row, 2).Value = "IGG";
            ws.Cell(row, 3).Value = "Incinerator";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] fuels = { "VLSFO", "MDO" };
            foreach (string ft in fuels)
            {
                ws.Cell(row, 1).Value = ft;
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), GetFuelConsByTypeAndConsType(dtFuelCons, ft, 19));
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), GetFuelConsByTypeAndConsType(dtFuelCons, ft, 28));
                row++;
            }
            row++;
        }

        /// <summary>Writes the Events block: title row, header (Fuel + 8 event names), VLSFO row, MDO row.
        /// Event ConsTypeIds 20..27 = Stoppage, Deviation, SlowSteaming, BadWeather, COTPrep, CargoHeating, BWExchange, Others.</summary>
        private void AddFuelConsEventsBlock(IXLWorksheet ws, ref int row, DataTable dtFuelCons)
        {
            int[] eventConsTypeIds = { 20, 21, 22, 23, 24, 25, 26, 27 };
            string[] eventLabels = { "Stoppage", "Deviation", "Slow Steaming", "Bad Weather", "COT Prep", "Cargo Heating", "BW Exchange", "Others" };
            ws.Cell(row, 1).Value = "Events";
            ApplyLightGrayTitle(ws, row, 1, 9);
            row++;
            ws.Cell(row, 1).Value = "Fuel";
            for (int i = 0; i < eventLabels.Length; i++) ws.Cell(row, i + 2).Value = eventLabels[i];
            ws.Range(row, 1, row, 9).Style.Font.Bold = true;
            row++;
            string[] fuels = { "VLSFO", "MDO" };
            foreach (string ft in fuels)
            {
                ws.Cell(row, 1).Value = ft;
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int i = 0; i < eventConsTypeIds.Length; i++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, i + 2), GetFuelConsByTypeAndConsType(dtFuelCons, ft, eventConsTypeIds[i]));
                row++;
            }
            row++;
        }

        /// <summary>Writes the Total summary block: title row, then VLSFO Total and MDO Total rows
        /// pinned to 3 decimal places (no "MT" suffix) to match the web view.</summary>
        private void AddFuelConsTotalBlock(IXLWorksheet ws, ref int row, DataTable dtFuelCons)
        {
            decimal vlsfoTotal = GetFuelConsByType(dtFuelCons, "VLSFO");
            decimal mdoTotal = GetFuelConsByType(dtFuelCons, "MDO");
            ws.Cell(row, 1).Value = "Total";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            ws.Cell(row, 1).Value = "VLSFO TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = vlsfoTotal.ToString("0.000");
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = "MDO TOTAL";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = mdoTotal.ToString("0.000");
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
        }

        private void AddDailyNoonCargoSheet(XLWorkbook wb, DailyNoonReport r, List<DNR_Cargo_Tank> cargoTanks, List<DNR_Ballast_Tank> ballastTanks, List<DNR_Void_Space> voidSpaces, DataTable dtNRCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Daily Noon Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 7);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "B/L QTY";
            ws.Cell(row, 3).Value = "Load Portal Actual";
            ws.Cell(row, 4).Value = "Today's Actual";
            ws.Cell(row, 5).Value = "QTY diff";
            ws.Cell(row, 6).Value = "Reason for QTY diff";
            ws.Cell(row, 7).Value = "Cargo Temp.";
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            row++;
            if (dtNRCargo != null && dtNRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtNRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    ws.Cell(row, 2).Value = FormatCargoVal(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    ws.Cell(row, 3).Value = FormatCargoVal(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    ws.Cell(row, 4).Value = FormatCargoVal(dr.Table.Columns.Contains("TodaysActual") ? dr["TodaysActual"] : null);
                    ws.Cell(row, 5).Value = FormatCargoVal(dr.Table.Columns.Contains("Qty_Diff") ? dr["Qty_Diff"] : null);
                    ws.Cell(row, 6).Value = dr.Table.Columns.Contains("Reasonfor_Qty_Diff") ? (dr["Reasonfor_Qty_Diff"]?.ToString() ?? "") : "";
                    ws.Cell(row, 7).Value = FormatCargoVal(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
                    row++;
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Slops ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SLOPS_ROB_OXY_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SLOPS_ROB_OXY_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SLOPS_ROB_OXY_Total);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "ROB (MT)", r?.Ballast_ROB);
            row++;

            ws.Cell(row, 1).Value = "Void Spaces Soundings in mtrs";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            foreach (var vs in voidSpaces ?? new List<DNR_Void_Space>())
                AddKeyValueRow(ws, ref row, vs.TankName ?? "", vs.Sounding);
            row++;

            ws.Cell(row, 1).Value = "Other Soundings in mtrs";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Pump Room bilge max sounding", r.PumpRoomMaxSounding);
                AddKeyValueRow(ws, ref row, "Chain Locker 1", r.ChainLocker1);
                AddKeyValueRow(ws, ref row, "Chain Locker 2", r.ChainLocker2);
            }
            row++;

            ws.Cell(row, 1).Value = "Fresh Water";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueRow(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueRow(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Cargo Tanks";
            ApplyLightGrayTitle(ws, row, 1, Math.Max(2, (cargoTanks?.Count ?? 0) + 1));
            row++;
            var cTanks = cargoTanks ?? new List<DNR_Cargo_Tank>();
            if (cTanks.Count > 0)
            {
                ws.Cell(row, 1).Value = "Tank";
                for (int c = 0; c < cTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = cTanks[c].TankName ?? "";
                ws.Range(row, 1, row, cTanks.Count + 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Ullage (mtrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].Ullage);
                row++;
                ws.Cell(row, 1).Value = "MT Qty";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].Qty_MT);
                row++;
                ws.Cell(row, 1).Value = "Oxygen (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].Oxygen);
                row++;
                ws.Cell(row, 1).Value = "H2S (PPM)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].H2S);
                row++;
                ws.Cell(row, 1).Value = "HC (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].HC);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast Tanks";
            ApplyLightGrayTitle(ws, row, 1, Math.Max(2, (ballastTanks?.Count ?? 0) + 1));
            row++;
            var bTanks = ballastTanks ?? new List<DNR_Ballast_Tank>();
            if (bTanks.Count > 0)
            {
                ws.Cell(row, 1).Value = "Tank";
                for (int c = 0; c < bTanks.Count; c++)
                    ws.Cell(row, c + 2).Value = bTanks[c].TankName ?? "";
                ws.Range(row, 1, row, bTanks.Count + 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Sounding (mtrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), bTanks[c].Sounding);
                row++;
                ws.Cell(row, 1).Value = "Cubic Vol";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), bTanks[c].Qty_Vol);
                row++;
                ws.Cell(row, 1).Value = "HC (% Volume)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int c = 0; c < bTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), bTanks[c].HC);
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        // "0.##########" strips trailing zeros in Excel display: 9.750 → 9.75, 10 → 10, 20.7 → 20.7.
        // Matches the email template's V(decimal?) behaviour.
        private const string ExcelDecimalFormat = "0.##########";

        private void AddKeyValueRow(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), value, ExcelDecimalFormat);
            row++;
        }

        /// <summary>Like AddKeyValueRow but lets the caller force a specific Excel number format
        /// (e.g. "0.000" for Draft Mtrs so trailing zeros are preserved).</summary>
        private void AddKeyValueRowWithFormat(IXLWorksheet ws, ref int row, string label, object value, string numberFormat)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), value, numberFormat);
            row++;
        }

        /// <summary>Like AddKeyValueRow but writes the value as plain text and forces the cell's
        /// data type to Text — used for pre-formatted date strings so Excel doesn't reinterpret
        /// "2025-10-03 06:42" through the workstation locale.</summary>
        private void AddKeyValueTextRow(IXLWorksheet ws, ref int row, string label, string value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            var valCell = ws.Cell(row, 2);
            // Set format BEFORE Value so ClosedXML doesn't try to interpret the date-looking string as a number.
            valCell.Style.NumberFormat.Format = "@";
            valCell.SetDataType(XLCellValues.Text);
            valCell.Value = value ?? "";
            row++;
        }

        /// <summary>True when the Bunker Received DataTable has at least one row with a non-empty
        /// Receipt value. Used to skip the "Bunker Received in MT" section in the Arrival Excel
        /// when no bunker data was entered for this report.</summary>
        private bool ArrivalBunkerHasData(DataTable dtBunker)
        {
            if (dtBunker == null || dtBunker.Rows.Count == 0) return false;
            if (!dtBunker.Columns.Contains("Receipt")) return false;
            foreach (DataRow dr in dtBunker.Rows)
            {
                object v = dr["Receipt"];
                if (v == null || v == DBNull.Value) continue;
                if (!string.IsNullOrWhiteSpace(v.ToString())) return true;
            }
            return false;
        }

        /// <summary>Writes a DataRow date column to the given cell as text in "yyyy-MM-dd HH:mm"
        /// format. Forces the cell type to Text so Excel doesn't reinterpret the date string
        /// through the workstation locale.</summary>
        private void AddDateTextCell(IXLCell cell, DataRow dr, string col)
        {
            string s = "";
            if (dr.Table.Columns.Contains(col) && dr[col] != null && dr[col] != DBNull.Value)
            {
                DateTime d;
                if (DateTime.TryParse(dr[col].ToString(), out d))
                    s = d.ToString("yyyy-MM-dd HH:mm");
            }
            cell.Style.NumberFormat.Format = "@";
            cell.SetDataType(XLCellValues.Text);
            cell.Value = s;
        }

        /// <summary>
        /// Sets cell value; for numeric values applies a format that strips trailing zeros
        /// ("0.##########"): 9.750 → 9.75, 20.7 → 20.7. Whole numbers (66, 10, 0) skip the
        /// format so Excel doesn't render a trailing decimal point ("66." bug). Non-numeric
        /// strings are written as text without any formatting.
        /// </summary>
        private void SetCellValueWithDecimalFormat(IXLCell cell, object value, string numberFormat = "0.##########")
        {
            if (value == null || value == DBNull.Value)
            {
                cell.Value = "";
                return;
            }
            decimal d;
            if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d))
            {
                cell.Value = d;
                // Skip the decimal format for whole numbers — Excel's number-format engine
                // renders "0.##########" as "66." (trailing dot) for integer values because
                // the literal '.' in the format string is always emitted. Caller can still
                // force a specific format (e.g. "0.000" for Draft Mtrs) by passing it explicitly.
                bool isWholeNumber = d == Math.Truncate(d);
                bool callerForcedFormat = !string.Equals(numberFormat, "0.##########", StringComparison.Ordinal);
                if (!isWholeNumber || callerForcedFormat)
                    cell.Style.NumberFormat.Format = numberFormat;
            }
            else
            {
                cell.Value = value.ToString();
            }
        }

        private void ApplyLightGrayTitle(IXLWorksheet ws, int row, int colStart, int colEnd)
        {
            var rng = ws.Range(row, colStart, row, colEnd);
            rng.Merge();
            rng.Style.Font.Bold = true;
            rng.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rng.Style.Fill.BackgroundColor = XLColor.FromArgb(211, 211, 211);
        }

        /// <summary>Format nautical Latitude/Longitude for Excel: "21,58.29 S" → "21° 58.29' S".
        /// Returns "" for null/empty, original trimmed string when unparseable. Mirrors the email template's FormatLatLon.</summary>
        private string FormatLatLonForExcel(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();
            var m = System.Text.RegularExpressions.Regex.Match(s, @"^\s*(\d+)\s*,\s*([\d.]+)\s*([NSEWnsew])?\s*$");
            if (!m.Success) return s;
            string dir = m.Groups[3].Success ? (" " + m.Groups[3].Value.ToUpperInvariant()) : "";
            return m.Groups[1].Value + "° " + m.Groups[2].Value + "'" + dir;
        }

        private decimal GetFuelConsByType(DataTable dt, string fuelType)
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

        private decimal GetFuelConsByTypeAndConsType(DataTable dt, string fuelType, int consTypeId)
        {
            if (dt == null) return 0;
            decimal sum = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if ((dr["FuelType"]?.ToString() ?? "").Equals(fuelType, StringComparison.OrdinalIgnoreCase))
                {
                    int cId = 0;
                    if (dr.Table.Columns.Contains("ConsTypeId") && dr["ConsTypeId"] != null && dr["ConsTypeId"] != DBNull.Value)
                        int.TryParse(dr["ConsTypeId"].ToString(), out cId);
                    if (cId == consTypeId)
                    {
                        var v = dr["Value"];
                        if (v != null && v != DBNull.Value) sum += Convert.ToDecimal(v);
                    }
                }
            }
            return sum;
        }

        private string FormatDec(decimal d) { return d.ToString("0.##########"); }
        private string FormatCargoVal(object val)
        {
            if (val == null || val == DBNull.Value) return "0";
            decimal d;
            return decimal.TryParse(val.ToString(), out d) ? d.ToString("0.##########") : "0";
        }


        private bool IsReportAlreadySaved(string filesPath, string reportType, int vesselId, string datePart)
        {
            try
            {
                string pattern = reportType + "_" + vesselId + "_" + datePart + "_*.xlsx";
                string[] existing = Directory.GetFiles(filesPath, pattern);
                return existing != null && existing.Length > 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Records <paramref name="fullPath"/> as the winner file for (reportType, vesselId) if the
        /// supplied <paramref name="rowModifiedDate"/> exceeds the currently tracked max. Each
        /// Save*ReportExcelToFiles method calls this immediately after writing a file, using the
        /// row's ModifiedDate from the imported Excel — that is the source of truth for which file
        /// represents the vessel's latest noon/arrival/etc. entry.
        /// </summary>
        private void TrackLatestModifiedRowForGroup(string reportType, int vesselId, DateTime rowModifiedDate, string fullPath)
        {
            if (string.IsNullOrEmpty(reportType) || vesselId <= 0 || string.IsNullOrEmpty(fullPath)) return;
            string key = reportType + "|" + vesselId;
            DateTime cur;
            if (!_latestModDatePerGroup.TryGetValue(key, out cur) || rowModifiedDate > cur)
            {
                _latestModDatePerGroup[key] = rowModifiedDate;
                _latestFilePathPerGroup[key] = fullPath;
            }
            _allSavedFiles.Add(new KeyValuePair<string, DateTime>(fullPath, rowModifiedDate));
        }

        /// <summary>
        /// Returns the parsed ModifiedDate (or ModifyDate fallback) from the imported Excel row.
        /// Returns DateTime.MinValue when no parseable value is present, so the very first valid
        /// row will always win the comparison.
        /// </summary>
        private DateTime ReadRowModifiedDate(DataRow row)
        {
            if (row == null) return DateTime.MinValue;
            // Try modified-date column names first. Added "Modified_Date" (with underscore)
            // for Bunker and FreshWater report sheets which use this exact spelling — without
            // it those reports never emailed because ReadRowModifiedDate returned MinValue and
            // the LastSent gate always rejected them.
            string[] modifiedCandidates = { "ModifiedDate", "ModifyDate", "Modified_Date" };
            foreach (string col in modifiedCandidates)
            {
                if (!row.Table.Columns.Contains(col)) continue;
                if (row[col] == DBNull.Value || row[col] == null) continue;
                DateTime d;
                if (DateTime.TryParse(row[col].ToString(), out d)) return d;
            }
            // Fall back to created-date when modified is empty/null. Bunker and FreshWater rows
            // commonly have Modified_Date NULL on the first import (never edited), but always
            // have Created_Date populated. Using Created_Date as the bookmark date lets these
            // rows clear the LastSent gate and shoot emails like other reports do.
            string[] createdCandidates = { "CreatedDate", "CreateDate", "Created_Date" };
            foreach (string col in createdCandidates)
            {
                if (!row.Table.Columns.Contains(col)) continue;
                if (row[col] == DBNull.Value || row[col] == null) continue;
                DateTime d;
                if (DateTime.TryParse(row[col].ToString(), out d)) return d;
            }
            return DateTime.MinValue;
        }

        private void LogReportExport(string reportType, int vesselId, string datePart, string fileName, string action)
        {
            try
            {
                string filesDir = Server.MapPath("~/Files/");
                if (!Directory.Exists(filesDir)) Directory.CreateDirectory(filesDir);
                string logPath = Path.Combine(filesDir, "ReportExportLog.txt");
                string line = string.Format("{0}|{1}|{2}|{3}|{4}|{5}{6}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), reportType, vesselId, datePart, fileName ?? "", action, Environment.NewLine);
                File.AppendAllText(logPath, line);
            }
            catch { }
        }


        private void SaveArrivalReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasDate = tbls.Columns.Contains("Date");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            bool hasEOSP = tbls.Columns.Contains("EOSP");
            bool hasNOR = tbls.Columns.Contains("NOR");
            if (!hasVesselId) return;
            if (!hasDate && !hasModifiedDate && !hasEOSP && !hasNOR) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasDate && row["Date"] != DBNull.Value && row["Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Date"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasEOSP && row["EOSP"] != DBNull.Value && row["EOSP"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["EOSP"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasNOR && row["NOR"] != DBNull.Value && row["NOR"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["NOR"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new ArrivalReport();
                vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselId, "ArrivalReport");
                var arrRBind = vd.GetArrivalRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (arrRBind == null)
                {
                    vd.GetArrivalRList = CommonMethods.editarrivalRListdashboard(reportdate, vesselId, "ArrivalReportR");
                    arrRBind = vd.GetArrivalRList?.Where(x => x.Id > 0).FirstOrDefault();
                }
                if (arrRBind == null && tbls.Columns.Contains("Id"))
                {
                    int rowId = 0;
                    if (row["Id"] != DBNull.Value && row["Id"] != null && int.TryParse(row["Id"].ToString(), out rowId) && rowId > 0)
                    {
                        vd.GetArrivalRList = CommonMethods.editarrivalRList(rowId, vesselId, "ArrivalReport");
                        arrRBind = vd.GetArrivalRList?.Where(x => x.Id == rowId).FirstOrDefault();
                    }
                }
                if (arrRBind == null) continue;

                int id = arrRBind.Id;

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtARCargo = new DataTable();

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.EOSP, a.FWE from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=2", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=2 order by FuelType_Id", ConnectionBulder.con))
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=2 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from AR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.arrivalreport_id=" + id, ConnectionBulder.con))
                        adp.Fill(dtARCargo);
                }
                catch { }

                DataTable dtMain = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", arrRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "ArrivalReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "ArrivalReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddArrivalNavigationSheet(wb, arrRBind, dtNonRoutine, dtMain);
                    AddArrivalEngineSheet(wb, arrRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddArrivalCargoSheet(wb, arrRBind, dtARCargo);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        /// <summary>
        /// Generates Departure Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editdepartureRListDashboard logic (reportdate, vesselid) to fetch all details.
        /// </summary>
        private void SaveDepartureReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasDate = tbls.Columns.Contains("Date");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            bool hasReportDate = tbls.Columns.Contains("ReportDate");
            if (!hasVesselId) return;
            if (!hasDate && !hasModifiedDate && !hasReportDate) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasDate && row["Date"] != DBNull.Value && row["Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Date"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasReportDate && row["ReportDate"] != DBNull.Value && row["ReportDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ReportDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new DepartureReport();
                vd.GetDepRList = CommonMethods.editdepartureRListDashboard(reportdate, vesselId, "DepartureReport");
                var depRBind = vd.GetDepRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (depRBind == null)
                {
                    vd.GetDepRList = CommonMethods.editdepartureRListDashboard(reportdate, vesselId, "DepartureReportR");
                    depRBind = vd.GetDepRList?.Where(x => x.Id > 0).FirstOrDefault();
                }
                if (depRBind == null && tbls.Columns.Contains("Id"))
                {
                    int rowId = 0;
                    if (row["Id"] != DBNull.Value && row["Id"] != null && int.TryParse(row["Id"].ToString(), out rowId) && rowId > 0)
                    {
                        vd.GetDepRList = CommonMethods.editdepartureRList(rowId, vesselId, "DepartureReport");
                        depRBind = vd.GetDepRList?.Where(x => x.Id == rowId).FirstOrDefault();
                    }
                }
                if (depRBind == null) continue;

                int id = depRBind.Id;

                // Backfill LO/HO + Cargo_Temp from DepartureReport table directly because the dashboard SP
                // aliases columns to short names (AECC_ROB, MECC_ROB, ...) that don't map to the long-named
                // model properties (LO_HO_Cons_AECC_ROB, ...). Without this, those fields stay null in the model.
                try
                {
                    using (SqlDataAdapter adpBf = new SqlDataAdapter(
                        "select LO_HO_Cons_MECC, LO_HO_Cons_MECC_ROB, LO_HO_Cons_MECYL, LO_HO_Cons_MECYL_ROB, " +
                        "LO_HO_Cons_AECC, LO_HO_Cons_AECC_ROB, LO_HO_Cons_HYDR_Oil, LO_HO_Cons_HYDR_Oil_ROB, " +
                        "Cargo_Temp from DepartureReport where Id=" + id, ConnectionBulder.con))
                    {
                        DataTable dtBf = new DataTable();
                        adpBf.Fill(dtBf);
                        if (dtBf.Rows.Count > 0)
                        {
                            DataRow br = dtBf.Rows[0];
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

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtMain = new DataTable();
                DataTable dtDRCargo = new DataTable();

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.EOSP as SBE, a.FWE as RFA from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=3", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=3 order by FuelType_Id", ConnectionBulder.con))
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=3 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    try
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from DR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.depreport_id=" + id, ConnectionBulder.con))
                            adp.Fill(dtDRCargo);
                    }
                    catch
                    {
                        try
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.CargoName, b.PortName from DR_Cargo a inner join LR_Cargo b on a.LR_Cargo_Id=b.Id and a.VesselId=b.VesselId where a.VesselId=" + vesselId + " and a.depreport_id=" + id, ConnectionBulder.con))
                                adp.Fill(dtDRCargo);
                        }
                        catch { }
                    }
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", depRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "DepartureReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "DepartureReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddDepartureNavigationSheet(wb, depRBind, dtNonRoutine, dtMain);
                    AddDepartureEngineSheet(wb, depRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddDepartureCargoSheet(wb, depRBind, dtDRCargo);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddDepartureNavigationSheet(XLWorkbook wb, DepartureReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Departure Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
                }
                if (string.IsNullOrWhiteSpace(voyNo) && r.VoyageId > 0)
                {
                    try
                    {
                        var voyages = CommonMethods.GetVoyageList(r.VesselId);
                        var match = voyages?.FirstOrDefault(v => v.Id == r.VoyageId);
                        if (match != null && !string.IsNullOrWhiteSpace(match.VoyageNumber)) voyNo = match.VoyageNumber.Trim();
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(voyNo)) voyNo = r.VoyageId.ToString();
                // Resolve Dep leg and Next leg separately so the header has two distinct fields
                // (Leg + Next Leg) matching the web view.
                string depLegText = legText; // dtMain.Leg fallback if already populated
                string nextLegText = "";
                try
                {
                    if (string.IsNullOrEmpty(depLegText) && r.DepLegPortId > 0)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter(
                            "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.DepLegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                        {
                            DataTable dtLeg = new DataTable();
                            adp.Fill(dtLeg);
                            if (dtLeg.Rows.Count > 0) depLegText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                        }
                    }
                    if (r.NextLegPortId > 0)
                    {
                        using (SqlDataAdapter adp = new SqlDataAdapter(
                            "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.NextLegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                        {
                            DataTable dtLeg = new DataTable();
                            adp.Fill(dtLeg);
                            if (dtLeg.Rows.Count > 0) nextLegText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                        }
                    }
                }
                catch { }
                // Header field order matches the web view: Voy No., Leg, Dep. Port, Draft Fwd,
                // Next Leg, Next Port, ETA, Draft Mid, Report Date, Draft Aft.
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Leg", depLegText);
                AddKeyValueRow(ws, ref row, "Dep. Port", r.DeparturePort ?? "");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd, "0.000");
                AddKeyValueRow(ws, ref row, "Next Leg", nextLegText);
                AddKeyValueRow(ws, ref row, "Next Port", r.NextPort ?? "");
                AddKeyValueTextRow(ws, ref row, "ETA", r.ETA != null ? Convert.ToDateTime(r.ETA).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid, "0.000");
                AddKeyValueTextRow(ws, ref row, "Report Date", r.ReportDate != null ? Convert.ToDateTime(r.ReportDate).ToString(ExcelDateFormat) : "");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft, "0.000");
            }
            row++;

            // Manoeuvring section — only Hours (2 decimals) and Distance. SBE/RFA date fields
            // are intentionally NOT rendered, matching the web view.
            ws.Cell(row, 1).Value = "Manoeuvring";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                // Show Manoeuvring Hours / Distance exactly as stored, matching the email (which
                // uses the raw decimal ToString()). Written as text so the stored scale is preserved
                // verbatim: 3 -> "3", 3.5 -> "3.5", 3.50 -> "3.50" (no forced or trimmed decimals).
                AddKeyValueTextRow(ws, ref row, "Manoeuvring Hours", r.Manoeuvring_Hrs.HasValue ? r.Manoeuvring_Hrs.Value.ToString() : "-");
                AddKeyValueTextRow(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance.HasValue ? r.Manoeuvring_Distance.Value.ToString() : "-");
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Event Name";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null);
                row++;
            }
            row++;

            // Other Receipts, Repairs, Crew Change & Landed — between Non-Routine Events and Weather.
            ws.Cell(row, 1).Value = "Other Receipts, Repairs, Crew Change & Landed";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "CTM (Mention Currency & Amount)", r.CTM ?? "");
                AddKeyValueRow(ws, ref row, "SPARES (Mention Revision Numbers)", r.Spares ?? "");
                AddKeyValueRow(ws, ref row, "STORES (Mention Revision Numbers)", r.Stores ?? "");
                AddKeyValueRow(ws, ref row, "REPAIRS CONDUCTED (Mention details)", r.RepairsConducted ?? "");
                AddKeyValueRow(ws, ref row, "CREW CHANGE (No of Crew)", r.CrewChange);
                AddKeyValueRow(ws, ref row, "ITEMS LANDED (Mention Details)", r.ItemsLanded ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Departure Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            // Per spec: empty value → leave blank in Excel (no "-" placeholder).
            AddKeyValueRow(ws, ref row, "Departure Report Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddDepartureEngineSheet(XLWorkbook wb, DepartureReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Departure Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 9);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r == null) { ws.Columns().AdjustToContents(); return; }

            // Table 1: Slops / Bilge Disposed & ROB (label | Oil | Water | Total) — 3-decimal cells.
            ws.Cell(row, 1).Value = "Slops / Bilge Disposed & ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Oil";
            ws.Cell(row, 3).Value = "Water";
            ws.Cell(row, 4).Value = "Total";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            WriteSlopsRow(ws, ref row, "Slops Disposed(m3)", r.SlopsDisposed_Oil, r.SlopsDisposed_Water, r.SlopsDisposed_Total);
            WriteSlopsRow(ws, ref row, "Slops ROB(m3)", r.SlopsROB_Oil, r.SlopsROB_Water, r.SlopsROB_Total);
            WriteSlopsRow(ws, ref row, "Bilge Disposed(m3)", r.BilgesDisposed_Oil, r.BilgesDisposed_Water, r.BilgesDisposed_Total);
            WriteSlopsRow(ws, ref row, "Bilge ROB(m3)", r.BilgesROB_Oil, r.BilgesROB_Water, r.BilgesROB_Total);
            row++;

            // Table 2: Other Disposal.
            ws.Cell(row, 1).Value = "Other Disposal";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRowWithFormat(ws, ref row, "Sludge(m3)", r.Sludge, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "Garbage - Plastic(m3)", r.GarbagePlastic, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "Garbage - Others(m3)", r.GarbageOthers, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "Other Disposal(m3)", r.OtherDisposal, "0.000");
            row++;

            // Table 3: Date & Time (SBE / RFA). Force text type so locale can't reformat ISO string.
            ws.Cell(row, 1).Value = "Date & Time";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueTextRow(ws, ref row, "SBE", r.SBE_DateT.HasValue ? r.SBE_DateT.Value.ToString(ExcelDateTimeFormat) : "");
            AddKeyValueTextRow(ws, ref row, "RFA", r.RFA_DateT.HasValue ? r.RFA_DateT.Value.ToString(ExcelDateTimeFormat) : "");
            row++;

            // Table 4: Engine — 3-decimal values for SLIP%, RPM, BHP, MCR%.
            ws.Cell(row, 1).Value = "Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRowWithFormat(ws, ref row, "SLIP%", r.Slip, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "RPM", r.RPM, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "BHP(hp)", r.BHP, "0.000");
            AddKeyValueRowWithFormat(ws, ref row, "MCR%", r.MCR, "0.000");
            row++;

            // Table 5: LO & HO Consumptions — 3-col (label | Consumptions | Received | ROB).
            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Consumptions";
            ws.Cell(row, 3).Value = "Received";
            ws.Cell(row, 4).Value = "ROB";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            WriteLoHoRow(ws, ref row, "MECC(Ltrs)", r.LO_HO_Cons_MECC, r.Bunker_LO_Rec_MECC, r.LO_HO_Cons_MECC_ROB);
            WriteLoHoRow(ws, ref row, "MECYL(Ltrs)", r.LO_HO_Cons_MECYL, r.Bunker_LO_Rec_MECYL, r.LO_HO_Cons_MECYL_ROB);
            WriteLoHoRow(ws, ref row, "AECC(Ltrs)", r.LO_HO_Cons_AECC, r.Bunker_LO_Rec_AECC, r.LO_HO_Cons_AECC_ROB);
            WriteLoHoRow(ws, ref row, "HYDRAULIC Oil(Ltrs)", r.LO_HO_Cons_HYDR_Oil, r.Bunker_LO_Rec_HYDR_Oil, r.LO_HO_Cons_HYDR_Oil_ROB);
            row++;

            // Table 6: Other ROB (Full / In Use / Empty), 3-decimal values.
            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Full";
            ws.Cell(row, 3).Value = "In Use";
            ws.Cell(row, 4).Value = "Empty";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = "Oxygen (Bottles)";
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty, "0.000");
            row++;
            ws.Cell(row, 1).Value = "Acetylene (Bottles)";
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty, "0.000");
            row++;
            row++;

            // Table 7: Fuel ROB in MT — 3-col (Fuel | SBE | RFA), 3-decimal cells.
            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "SBE";
            ws.Cell(row, 3).Value = "RFA";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    ws.Cell(row, 1).Value = dr["FuelType"]?.ToString() ?? "";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("SBE") ? dr["SBE"] : null, "0.000");
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("RFA") ? dr["RFA"] : null, "0.000");
                    row++;
                }
            }
            row++;

            // Table 8: Fuel Consumption in MT — full 7-block layout matching Daily Noon.
            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 9);
            row++;
            AddFuelConsEngineBlock(ws, ref row, "Main Engine", true, dtFuelCons, 2, 3, 4, 5);
            AddFuelConsEngineBlock(ws, ref row, "Aux Engine", true, dtFuelCons, 7, 8, 9, 10);
            AddFuelConsEngineBlock(ws, ref row, "Boiler", true, dtFuelCons, 11, 12, 13, 14);
            AddFuelConsEngineBlock(ws, ref row, "Framo System", false, dtFuelCons, 15, 16, 17, 18);
            AddFuelConsIggIncBlock(ws, ref row, dtFuelCons);
            AddFuelConsEventsBlock(ws, ref row, dtFuelCons);
            AddFuelConsTotalBlock(ws, ref row, dtFuelCons);

            ws.Columns().AdjustToContents();
        }

        /// <summary>Helper for the Slops / Bilge Disposed &amp; ROB table — writes a row with
        /// 3-decimal formatted cells (Oil / Water / Total).</summary>
        private void WriteSlopsRow(IXLWorksheet ws, ref int row, string label, decimal? oil, decimal? water, decimal? total)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), oil, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 3), water, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 4), total, "0.000");
            row++;
        }

        /// <summary>Helper for the LO &amp; HO Consumptions table — writes a row with
        /// 3-decimal formatted cells (Consumption / Received / ROB).</summary>
        private void WriteLoHoRow(IXLWorksheet ws, ref int row, string label, decimal? cons, decimal? rec, decimal? rob)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), cons, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 3), rec, "0.000");
            SetCellValueWithDecimalFormat(ws.Cell(row, 4), rob, "0.000");
            row++;
        }

        private void AddDepartureCargoSheet(XLWorkbook wb, DepartureReport r, DataTable dtDRCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Departure Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Cargo — 6-col table matching the web view exactly.
            //   Cargo | B/L QTY(MT) | Load Portal Actual(MT) | Cargo Temp.(Deg centigrade)
            //         | Completion Date & Time | Rate(m3/hr)
            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 6);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "B/L QTY(MT)";
            ws.Cell(row, 3).Value = "Load Portal Actual(MT)";
            ws.Cell(row, 4).Value = "Cargo Temp.(Deg centigrade)";
            ws.Cell(row, 5).Value = "Completion Date & Time";
            ws.Cell(row, 6).Value = "Rate(m3/hr)";
            ws.Range(row, 1, row, 6).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Style.Alignment.WrapText = true;
            row++;
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    // B/L QTY(MT) and Load Portal Actual(MT) — force 3 decimals.
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null, "0.000");
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null, "0.000");
                    // Cargo Temp. — strip trailing zeros (37.30 → 37.3).
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
                    // Completion Date & Time — force text type so locale can't reformat the ISO string.
                    string compDt = "";
                    if (dr.Table.Columns.Contains("Completion_DateT") && dr["Completion_DateT"] != null && dr["Completion_DateT"] != DBNull.Value)
                    {
                        if (DateTime.TryParse(dr["Completion_DateT"].ToString(), out DateTime cd)) compDt = cd.ToString(ExcelDateTimeFormat);
                    }
                    var compCell = ws.Cell(row, 5);
                    compCell.Style.NumberFormat.Format = "@";
                    compCell.SetDataType(XLCellValues.Text);
                    compCell.Value = compDt;
                    // Rate(m3/hr) — strip trailing zeros (2900.000 → 2900).
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6), dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null);
                    row++;
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Slops Disposed / ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Disposed (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SlopsDisposed_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SlopsDisposed_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SlopsDisposed_Total);
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SlopsROB_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SlopsROB_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SlopsROB_Total);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            // Label "ROB" (no MT suffix) with forced 3 decimals (500 → 500.000) per web view.
            AddKeyValueRowWithFormat(ws, ref row, "ROB", r?.Ballast_ROB, "0.000");
            row++;

            ws.Cell(row, 1).Value = "Fresh Water";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRowWithFormat(ws, ref row, "FW Generated (MT)", r.FW_Generated, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Consumption (MT)", r.FW_Consumption, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "ROB (MT)", r.FW_ROB, "0.000");
            }
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Parses sync Excel names: ReportType_vesselId_dd_MM_yyyy_HHmmss.xlsx or ..._dd_MM_yyyy_R{rowId}_HHmmss.xlsx (R+id matches Excel row for HTML email).
        /// Uses strict date validation when possible; falls back to legacy segment rules so older filenames still queue for email.
        /// </summary>
        /// <summary>
        /// For report types that carry a user-uploaded supporting file (Bunker = BDN Report,
        /// FreshWater = Attachment), returns the absolute path of that file on disk so it
        /// can be added to the outgoing email. Returns null for any other report type, or
        /// when no reportId is known, or when the DB has no file name on record.
        /// </summary>
        private string ResolveUserUploadedFilePath(string reportType, int? reportId)
        {
            if (!reportId.HasValue || reportId.Value <= 0) return null;
            string folder, column, table;
            if (reportType != null && reportType.Equals("BunkerReport", StringComparison.OrdinalIgnoreCase))
            {
                folder = "~/Bunker_LabAnalysisReport/";
                column = "LabAnalysisReport_Name";
                table  = "BunkerReport";
            }
            else if (reportType != null && reportType.Equals("FreshWaterReport", StringComparison.OrdinalIgnoreCase))
            {
                folder = "~/FreshWaterReport/";
                column = "File_Name";
                table  = "FreshWaterReport";
            }
            else
            {
                return null;
            }

            string fileName = null;
            try
            {
                using (var cmd = new SqlCommand("SELECT " + column + " FROM " + table + " WHERE Id=@Id", ConnectionBulder.con))
                {
                    cmd.Parameters.AddWithValue("@Id", reportId.Value);
                    if (ConnectionBulder.con.State != ConnectionState.Open) ConnectionBulder.con.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) fileName = result.ToString();
                }
            }
            catch { return null; }

            if (string.IsNullOrWhiteSpace(fileName)) return null;
            try { return Server.MapPath(folder + fileName); }
            catch { return null; }
        }

        private static bool TryParseSyncReportExportFileName(string fileName, out string reportType, out int vesselId, out string datePart, out int? reportId)
        {
            reportType = null;
            vesselId = 0;
            datePart = null;
            reportId = null;
            string fn = Path.GetFileNameWithoutExtension(fileName ?? "");
            if (string.IsNullOrEmpty(fn)) return false;
            var parts = fn.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return false;
            reportType = parts[0];
            if (!int.TryParse(parts[1], out vesselId) || vesselId <= 0) return false;

            if (parts.Length >= 5)
            {
                datePart = parts[2] + "_" + parts[3] + "_" + parts[4];
                if (parts.Length >= 7 && parts[5].Length > 1 && (parts[5][0] == 'R' || parts[5][0] == 'r') && int.TryParse(parts[5].Substring(1), out int rid) && rid > 0)
                    reportId = rid;
                if (int.TryParse(parts[2], out int dd) && int.TryParse(parts[3], out int mm) && int.TryParse(parts[4], out int yyyy))
                {
                    try { new DateTime(yyyy, mm, dd); } catch { /* keep datePart as segments anyway */ }
                }
                return true;
            }
            datePart = parts[2];
            return true;
        }


        private void SendImportCompletionEmail(List<string> savedFilePaths, string importedFileName)
        {
            var vesselID = 0;
            if (savedFilePaths == null || savedFilePaths.Count == 0) return;

            // Pick the winner file per (reportType, vesselId) from the in-memory map populated
            // during the Save*ReportExcelToFiles calls. The winner is the file whose source row in
            // the imported Excel had the maximum ModifiedDate — i.e., the latest noon/arrival/etc.
            // the vessel submitted. We do NOT scan ~/Files/ or query the DB: the imported sheet
            // is the source of truth, and the DB ModifiedDate may have been bumped by later in-app
            // edits on older rows, which would otherwise mis-win here.
            // Carry each winner file's ModifiedDate (from the imported Excel row that won the
            // per-(reportType, vesselId) comparison) so the recipient-level gate below can send
            // only files newer than the recipient's LastSent.
            var toSend = new List<KeyValuePair<string, DateTime>>();
            foreach (var kv in _latestFilePathPerGroup)
            {
                string winnerPath = kv.Value;
                if (string.IsNullOrEmpty(winnerPath) || !File.Exists(winnerPath)) continue;

                string fn = Path.GetFileName(winnerPath);
                if (!TryParseSyncReportExportFileName(fn, out string reportType, out int vesselId, out string datePart, out _))
                    continue;

                vesselID = vesselId;
                DateTime winnerModifiedDate;
                if (!_latestModDatePerGroup.TryGetValue(kv.Key, out winnerModifiedDate))
                    winnerModifiedDate = DateTime.MinValue;
                toSend.Add(new KeyValuePair<string, DateTime>(winnerPath, winnerModifiedDate));
            }
            if (toSend.Count == 0) return;

            // Parallel list for first-time recipients (LastSent == NULL). Contains every per-row
            // Excel saved during this import, not just the winners. A recipient with no LastSent
            // bookmark gets the full set so they are caught up; subsequent runs flip them into
            // the winner-only branch automatically once LastSent is stamped below.
            var fullSend = new List<KeyValuePair<string, DateTime>>();
            foreach (var entry in _allSavedFiles)
            {
                if (string.IsNullOrEmpty(entry.Key) || !File.Exists(entry.Key)) continue;
                fullSend.Add(entry);
            }

            string from = ConfigurationManager.AppSettings["mailmsg"] ?? "noreply@mooringplan.com";
            string smtpHost = ConfigurationManager.AppSettings["smtpclnt"] ?? "smtp.zeptomail.in";
            string smtpUser = ConfigurationManager.AppSettings["ntwrkcrd"] ?? "";
            string smtpPwd = ConfigurationManager.AppSettings["pwd"] ?? "";
            string fallbackTo = ConfigurationManager.AppSettings["ImportNotificationTo"] ?? "";

            var emailListnew = CommonClass.GetSyncEmailVesselsReport(vesselID);

            foreach (var item in emailListnew)
            {
                // First-time recipient (LastSent NULL → DateTime.MinValue here) gets the full set
                // of files saved this import; returning recipients get only the per-group winners
                // and only those whose row ModifiedDate is newer than their LastSent bookmark.
                bool isFirstSend = item.LastSent == DateTime.MinValue;
                var listForThisRecipient = isFirstSend ? fullSend : toSend;
                DateTime maxModifiedDateSent = DateTime.MinValue;

                foreach (var entry in listForThisRecipient)
                {
                    string path = entry.Key;
                    DateTime reportModifiedDate = entry.Value;
                        // For returning recipients, gate by their stamped LastSent. First-time
                        // recipients fall through and receive every file in the list.
                        if (!isFirstSend && reportModifiedDate <= item.LastSent) continue;
                        string fn = Path.GetFileName(path ?? "");
                        if (!TryParseSyncReportExportFileName(fn, out string reportType, out int vesselId, out string datePart, out int? reportIdFromFile))
                            continue;
                        string dateDisplay = FormatDateForDisplay(datePart);
                        string reportDisplayName = GetReportDisplayName(reportType);
                        string vesselDisplay = CommonClass.GetVesselNamesByImoNo(vesselId.ToString());
                        if (string.IsNullOrEmpty(vesselDisplay)) vesselDisplay = "Vessel " + vesselId;
                        string subject = string.Format("SIS {0} – {1} | {2}", reportDisplayName, vesselDisplay, dateDisplay);

                        var toAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var ccAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var emailList = CommonClass.GetSyncEmailVesselsReport(vesselId);

                        foreach (var e in emailList)
                        {
                            //
                            foreach (var addr in SplitEmailAddresses(e.EmailTo)) if (!string.IsNullOrWhiteSpace(addr)) toAddresses.Add(addr.Trim());
                            foreach (var addr in SplitEmailAddresses(e.EmailCC)) if (!string.IsNullOrWhiteSpace(addr)) ccAddresses.Add(addr.Trim());

                        }
                        if (toAddresses.Count == 0 && !string.IsNullOrWhiteSpace(fallbackTo))
                            foreach (var addr in SplitEmailAddresses(fallbackTo)) if (!string.IsNullOrWhiteSpace(addr)) toAddresses.Add(addr.Trim());
                        if (toAddresses.Count == 0) continue;

                        try
                        {
                            string body;
                            bool isHtml;
                            if (reportType.Equals("DailyNoonReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.DailyNoonReportEmailTemplate.BuildHtml(vesselId, datePart);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("ArrivalReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.ArrivalReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("DepartureReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.DepartureReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("BerthingReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.BerthingReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("LoadingReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.LoadingReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("DischargingReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.DischargingReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("BulkNoonReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.BulkNoonReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("ConsumptionReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.ConsumptionReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("BunkerReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.BunkerReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("FreshWaterReport", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.FreshWaterReportEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else if (reportType.Equals("NoonReportAllow", StringComparison.OrdinalIgnoreCase))
                            {
                                var htmlBody = SIS_Operational_Reports.Common.NoonReportAllowEmailTemplate.BuildHtml(vesselId, datePart, reportIdFromFile);
                                if (!string.IsNullOrEmpty(htmlBody))
                                {
                                    body = htmlBody;
                                    isHtml = true;
                                }
                                else
                                {
                                    body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                    isHtml = true;
                                }
                            }
                            else
                            {
                                body = BuildReportDetailsHtmlTable(reportDisplayName, vesselDisplay, dateDisplay);
                                isHtml = true;
                            }

                            using (var msg = new MailMessage())
                            {
                                msg.From = new System.Net.Mail.MailAddress(from);
                                foreach (var a in toAddresses) msg.To.Add(a);
                                foreach (var a in ccAddresses) msg.CC.Add(a);
                                msg.Subject = subject;
                                msg.Body = body;
                                msg.IsBodyHtml = isHtml;
                                msg.Attachments.Add(new System.Net.Mail.Attachment(path));

                                // For FreshWater and Bunker reports, also attach the user-uploaded
                                // file (PDF / DOC / image) so recipients receive both the generated
                                // Excel summary AND the original supporting document. The body
                                // additionally renders the file name as a download link to the portal.
                                string userFilePath = ResolveUserUploadedFilePath(reportType, reportIdFromFile);
                                if (!string.IsNullOrEmpty(userFilePath) && System.IO.File.Exists(userFilePath))
                                {
                                    try { msg.Attachments.Add(new System.Net.Mail.Attachment(userFilePath)); }
                                    catch { /* ignore — fall back to link-only in the body */ }
                                }

                                using (var smtp = new SmtpClient(smtpHost))
                                {
                                    smtp.Port = 587;
                                    smtp.EnableSsl = true;
                                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPwd);
                                    smtp.Send(msg);
                                }
                                //Update function
                                LogReportEmailSent(reportType, vesselId, datePart, fn);
                                if (reportModifiedDate > maxModifiedDateSent) maxModifiedDateSent = reportModifiedDate;
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                            /* log if needed */
                        }

                        //continue
                    }

                // Stamp LastSent with the current send timestamp. Future runs gate by
                // reportModifiedDate > LastSent, so only rows modified AFTER this send qualify.
                if (maxModifiedDateSent > DateTime.MinValue)
                    CommonClass.UpdateSyncEmailVesselsReportLastSent(item.Id, DateTime.Now);
            }

            // After sending all report emails, delete only Excel files from Files directory (keep logger files for tracking)
            try
            {
                string filesDir = Server.MapPath("~/Files/");
                if (Directory.Exists(filesDir))
                {
                    foreach (string f in Directory.GetFiles(filesDir, "*.xlsx"))
                    {
                        try { File.Delete(f); } catch { }
                    }
                }
            }
            catch { }
        }

        private static string GetReportDisplayName(string reportType)
        {
            if (string.IsNullOrEmpty(reportType)) return reportType ?? "";
            if (reportType.Equals("DailyNoonReport", StringComparison.OrdinalIgnoreCase)) return "Daily Noon Report";
            if (reportType.Equals("ArrivalReport", StringComparison.OrdinalIgnoreCase)) return "Arrival Report";
            if (reportType.Equals("DepartureReport", StringComparison.OrdinalIgnoreCase)) return "Departure Report";
            if (reportType.Equals("BerthingReport", StringComparison.OrdinalIgnoreCase)) return "Berthing Report";
            if (reportType.Equals("LoadingReport", StringComparison.OrdinalIgnoreCase)) return "Loading Report";
            if (reportType.Equals("DischargingReport", StringComparison.OrdinalIgnoreCase)) return "Discharging Report";
            if (reportType.Equals("BulkNoonReport", StringComparison.OrdinalIgnoreCase)) return "Bulk Noon Report";
            if (reportType.Equals("ConsumptionReport", StringComparison.OrdinalIgnoreCase)) return "Consumption Report";
            if (reportType.Equals("BunkerReport", StringComparison.OrdinalIgnoreCase)) return "Bunker Report";
            if (reportType.Equals("FreshWaterReport", StringComparison.OrdinalIgnoreCase)) return "Fresh Water Report";
            if (reportType.Equals("NoonReportAllow", StringComparison.OrdinalIgnoreCase)) return "Noon Report Allow";
            return reportType;
        }

        /// <summary>
        /// Formats datePart (dd_MM_yyyy) to full date display e.g. 18-03-2026 or 18 March 2026.
        /// </summary>
        private static string FormatDateForDisplay(string datePart)
        {
            if (string.IsNullOrEmpty(datePart)) return datePart;
            var p = datePart.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length >= 3 && int.TryParse(p[0], out int d) && int.TryParse(p[1], out int m) && int.TryParse(p[2], out int y))
            {
                try
                {
                    var dt = new DateTime(y, m, d);
                    return dt.ToString("dd-MMM-yyyy");
                }
                catch { }
            }
            return datePart.Replace("_", "-");
        }

        /// <summary>
        /// Builds fallback HTML email body when report template returns no data. Uses simple intro/footer (no Report Type table).
        /// </summary>
        private static string BuildReportDetailsHtmlTable(string reportDisplayName, string vesselDisplay, string dateDisplay)
        {
            string reportName = System.Web.HttpUtility.HtmlEncode(reportDisplayName ?? "");
            string vessel = System.Web.HttpUtility.HtmlEncode(vesselDisplay ?? "");
            string date = System.Web.HttpUtility.HtmlEncode(dateDisplay ?? "");
            return string.Format(@"
<!DOCTYPE html><html><head><meta charset=""utf-8""></head><body style=""margin:0;padding:12px;font-family:Arial,sans-serif;font-size:12px;"">
<p style=""margin:0 0 12px 0;"">Hello,</p>
<p style=""margin:0 0 12px 0;"">Please note that this is an automated email generated by the system.</p>
<p style=""margin:0 0 12px 0;"">The {0} for vessel {1} dated {2} has been successfully generated and is attached to this email in Excel format for your reference.</p>
<p style=""margin:12px 0 0 0;"">Please note that this is a no-reply email, and responses to this mailbox are not monitored.</p>
<p style=""margin:4px 0 0 0;"">For any queries or assistance, please contact the concerned team through the designated communication channel.</p>
<p style=""margin:12px 0 0 0;"">Thank you,</p>
<p style=""margin:4px 0 0 0;"">Team SIS</p>
</body></html>", reportName, vessel, date);
        }

        private const string ReportEmailLogFile = "ReportEmailLog.txt";

        /// <summary>
        /// Logs that a report email was sent to ReportEmailLog.txt. Format: DateTime|ReportType|VesselId|DatePart|FileName|EmailSent.
        /// Kept for audit/observability only — no longer used to suppress re-sends across imports.
        /// </summary>
        private void LogReportEmailSent(string reportType, int vesselId, string datePart, string fileName)
        {
            try
            {
                string filesDir = Server.MapPath("~/Files/");
                if (!Directory.Exists(filesDir)) Directory.CreateDirectory(filesDir);
                string logPath = Path.Combine(filesDir, ReportEmailLogFile);
                string line = string.Format("{0}|{1}|{2}|{3}|{4}|EmailSent{5}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), reportType, vesselId, datePart, fileName ?? "", Environment.NewLine);
                File.AppendAllText(logPath, line);
            }
            catch { }
        }

        private static IEnumerable<string> SplitEmailAddresses(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) yield break;
            foreach (var s in value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                yield return s.Trim();
        }

        private void AddArrivalNavigationSheet(XLWorkbook wb, ArrivalReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
                }
                // Fallback: resolve display VoyageNumber from the Voyage table by VoyageId.
                if (string.IsNullOrWhiteSpace(voyNo) && r.VoyageId > 0)
                {
                    try
                    {
                        var voyages = CommonMethods.GetVoyageList(r.VesselId);
                        var match = voyages?.FirstOrDefault(v => v.Id == r.VoyageId);
                        if (match != null && !string.IsNullOrWhiteSpace(match.VoyageNumber)) voyNo = match.VoyageNumber.Trim();
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(voyNo)) voyNo = r.VoyageId.ToString();
                // Fallback: resolve Leg from VoyageLeg scoped by LegPortId+VoyageId+VesselId.
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Latitude", FormatLatLonForExcel(r.Latitude));
                AddKeyValueRow(ws, ref row, "Longitude", FormatLatLonForExcel(r.Longitude));
                AddKeyValueRow(ws, ref row, "Place", r.Place ?? "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                // Force date cells to text so Excel doesn't reinterpret the ISO string
                // "2025-10-04 06:00" through the workstation locale and re-render it as
                // "04-10-2025 06:00" or similar on a non-ISO machine.
                AddKeyValueTextRow(ws, ref row, "NOR", r.NOR != null ? Convert.ToDateTime(r.NOR).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueTextRow(ws, ref row, "EOSP", r.EOSP != null ? Convert.ToDateTime(r.EOSP).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueTextRow(ws, ref row, "ETB", r.ETB != null ? Convert.ToDateTime(r.ETB).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Anchorage";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Anchorage Name", r.Anchor_Name);
                AddKeyValueTextRow(ws, ref row, "Drop Anchor Date & Time", r.Anchor_DateT != null ? Convert.ToDateTime(r.Anchor_DateT).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Anchoring Position - Latitude", FormatLatLonForExcel(r.AnchorPos_Latitude));
                AddKeyValueRow(ws, ref row, "Anchoring Position - Longitude", FormatLatLonForExcel(r.AnchorPos_Longitude));
                AddKeyValueTextRow(ws, ref row, "FWE Date & Time", r.AnchorFWE_DateT != null ? Convert.ToDateTime(r.AnchorFWE_DateT).ToString(ExcelDateTimeFormat) : "");
            }
            row++;

            ws.Cell(row, 1).Value = "Speed - Distance - Time";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Dist Noon to Arrival (DMG)(NM)", r.NoonToNoonDMG_Dist);
                AddKeyValueRow(ws, ref row, "Log Dist(NM)", r.LogDist);
                AddKeyValueRow(ws, ref row, "Engine Dist(NM)", r.EngineDist);
                AddKeyValueRow(ws, ref row, "Total Distance (Dep to Curr)(NM)", r.TotalDistance);
                AddKeyValueRow(ws, ref row, "Dist to Go (DTG)(NM)", r.DistToGo_DTG);
                AddKeyValueRow(ws, ref row, "Stmg Time Noon to Arrival(Hrs)", r.StmgTime);
                AddKeyValueRow(ws, ref row, "Total Time (Dep to Curr)(Hrs)", r.TotalTime);
                AddKeyValueRow(ws, ref row, "Actual Speed Noon to Arrival(Kts)", r.Act_Speed);
                AddKeyValueRow(ws, ref row, "Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed);
            }
            row++;

            // Manoeuvring — separate section between Speed-Distance-Time and Non-Routine Events.
            ws.Cell(row, 1).Value = "Manoeuvring";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Manoeuvring Hours", r.Manoeuvring_Hrs);
                AddKeyValueRow(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Event Name";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Arrival Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddArrivalEngineSheet(XLWorkbook wb, ArrivalReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 9);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueRow(ws, ref row, "RPM", r.RPM);
                AddKeyValueRow(ws, ref row, "BHP(hp)", r.BHP);
                AddKeyValueRow(ws, ref row, "MCR%", r.MCR);
            }
            row++;

            // 1. LO & HO Consumptions — 3-col layout (label | Consumption | ROB) matching Daily Noon.
            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Consumption";
                ws.Cell(row, 3).Value = "ROB";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "MECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECC);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECC_ROB);
                row++;
                ws.Cell(row, 1).Value = "MECYL (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECYL);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECYL_ROB);
                row++;
                ws.Cell(row, 1).Value = "AECC (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_AECC);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_AECC_ROB);
                row++;
                ws.Cell(row, 1).Value = "HYDRAULIC Oil (Ltrs)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_HYDR_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_HYDR_Oil_ROB);
                row++;
            }
            row++;

            // 2. Fuel ROB in MT — 3-col layout (Fuel | EOSP | FWE).
            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "EOSP";
            ws.Cell(row, 3).Value = "FWE";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    ws.Cell(row, 1).Value = dr["FuelType"]?.ToString() ?? "";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("EOSP") ? dr["EOSP"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("FWE") ? dr["FWE"] : null);
                    row++;
                }
            }
            row++;

            // Bunker Received in MT — only emit the section header and rows when the report has
            // bunker data entered. If no rows have a non-empty Receipt value, skip entirely so
            // there's no orphan section title in the Excel sheet.
            if (ArrivalBunkerHasData(dtBunker))
            {
                ws.Cell(row, 1).Value = "Bunker Received in MT";
                ApplyLightGrayTitle(ws, row, 1, 3);
                row++;
                foreach (DataRow dr in dtBunker.Rows)
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "");
                row++;
            }

            // 3. Other ROB — keep existing 4-col layout (label | Full | In Use | Empty).
            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty);
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty);
                row++;
            }
            row++;

            // 4. Fuel Consumption in MT — full 7-block layout matching Daily Noon.
            // Boiler shows Sub Total for Arrival (Daily Noon's Boiler stays without subtotal).
            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 9);
            row++;
            AddFuelConsEngineBlock(ws, ref row, "Main Engine", true, dtFuelCons, 2, 3, 4, 5);
            AddFuelConsEngineBlock(ws, ref row, "Aux Engine", true, dtFuelCons, 7, 8, 9, 10);
            AddFuelConsEngineBlock(ws, ref row, "Boiler", true, dtFuelCons, 11, 12, 13, 14);
            AddFuelConsEngineBlock(ws, ref row, "Framo System", false, dtFuelCons, 15, 16, 17, 18);
            AddFuelConsIggIncBlock(ws, ref row, dtFuelCons);
            AddFuelConsEventsBlock(ws, ref row, dtFuelCons);
            AddFuelConsTotalBlock(ws, ref row, dtFuelCons);

            ws.Columns().AdjustToContents();
        }

        private void AddArrivalCargoSheet(XLWorkbook wb, ArrivalReport r, DataTable dtARCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Arrival Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Cargo tab section order matches the web view:
            //   1. Fresh Water Noon To Report   2. Slops ROB   3. Cargo   4. Ballast

            // 1. Fresh Water Noon To Report
            ws.Cell(row, 1).Value = "Fresh Water Noon To Report";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueRow(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueRow(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            row++;

            // 2. Slops ROB — Oil/Water headers now include (m3) unit.
            ws.Cell(row, 1).Value = "Slops ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil(m3)";
                ws.Cell(row, 3).Value = "Water(m3)";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SLOPS_ROB_OXY_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SLOPS_ROB_OXY_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SLOPS_ROB_OXY_Total);
                row++;
            }
            row++;

            // 3. Cargo — single Qty(MT) column, no decimal padding (40476.00 → 40476).
            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "Qty(MT)";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    // Use the default 0.########## format so 40476.00 renders as 40476.
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null);
                    row++;
                }
            }
            row++;

            // 4. Ballast — label simplified to "ROB" (no "(MT)" suffix).
            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "ROB", r?.Ballast_ROB);
            row++;

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Berthing Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editberthingRListDashboard logic (reportdate, vesselid) to fetch all details.
        /// </summary>
        private void SaveBerthingReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasReportDate = tbls.Columns.Contains("ReportDate");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            if (!hasVesselId) return;
            if (!hasReportDate && !hasModifiedDate) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasReportDate && row["ReportDate"] != DBNull.Value && row["ReportDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ReportDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new BerthingReport();
                vd.GetBerthRList = CommonMethods.editberthingRListDashboard(reportdate, vesselId, "BerthingReport");
                var berthRBind = vd.GetBerthRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (berthRBind == null)
                {
                    vd.GetBerthRList = CommonMethods.editberthingRListDashboard(reportdate, vesselId, "BerthingReportR");
                    berthRBind = vd.GetBerthRList?.Where(x => x.Id > 0).FirstOrDefault();
                }
                if (berthRBind == null && tbls.Columns.Contains("Id"))
                {
                    int rowId = 0;
                    if (row["Id"] != DBNull.Value && row["Id"] != null && int.TryParse(row["Id"].ToString(), out rowId) && rowId > 0)
                    {
                        vd.GetBerthRList = CommonMethods.editberthingRList(rowId, vesselId, "BerthingReport");
                        berthRBind = vd.GetBerthRList?.Where(x => x.Id == rowId).FirstOrDefault();
                    }
                }
                if (berthRBind == null) continue;

                int id = berthRBind.Id;

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtMain = new DataTable();
                DataTable dtCargo = new DataTable();

                try
                {
                    // Berthing uses ReportType_Id=5 in tbl_FuelROB / Fuel_Cons_NR / tbl_BunkerLReceipt
                    // (matching the working email template). Excel was using =4 → empty fuel ROB cells.
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.EOSP as SBE, a.FWE as RFA from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=5", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=5 order by FuelType_Id", ConnectionBulder.con))
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
                    // Report_Table_Id=5 matches what BerthingController writes/reads (see
                    // BerthingController.cs around line 764). Older value 4 belongs to a different
                    // report type, so it always returned 0 rows — the Excel then rendered blank
                    // ChartererAccount and 0 Hours for every Non-Routine Event.
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=5 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", berthRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "BerthingReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                // Berthing cargo: own try-catch so the cargo fetch survives even if any earlier
                // query (USP, fuel, stoppage) throws. Same join pattern as BerthingController.GetBR_CargoEdit.
                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.*, b.cargoname, b.PortName from BR_Cargo a inner join LR_Cargo b on a.lr_cargo_id=b.Id and b.VesselId=" + vesselId + " where a.VesselId=" + vesselId + " and berthingreport_id=" + id, ConnectionBulder.con))
                        adp.Fill(dtCargo);
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "BerthingReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddBerthingNavigationSheet(wb, berthRBind, dtNonRoutine, dtMain);
                    AddBerthingEngineSheet(wb, berthRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddBerthingCargoSheet(wb, berthRBind, dtCargo);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddBerthingNavigationSheet(XLWorkbook wb, BerthingReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Berthing Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = "";
                string portStatusText = r.PortStatus.ToString();
                string facilityName = r.FacilityName ?? "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber"))
                        voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                    if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
                    if (string.IsNullOrEmpty(facilityName) && dtMain.Columns.Contains("FacilityName"))
                        facilityName = dr["FacilityName"]?.ToString() ?? "";
                }
                // Fallback: resolve display VoyageNumber from the Voyage table by VoyageId,
                // matching the same lookup the Berthing email body uses. Avoids leaking the
                // numeric VoyageId (e.g. 14) into the "Voy No." cell when the dashboard SP
                // doesn't return the VoyageNumber column.
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
                // Resolve Leg scoped by LegPortId + VoyageId + VesselId. VoyageLeg.Id is not unique
                // (same id reused across vessels/voyages), so the old query returned wrong legs.
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "Facility Name", facilityName);
                AddKeyValueRow(ws, ref row, "Berth Name", r.BerthName ?? "");
                AddKeyValueRow(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueRow(ws, ref row, "Leg", legText);
                // Report Date uses inline "dd-MM-yyyy" format (Berthing-only spec) instead of
                // the global ExcelDateFormat which is "yyyy-MM-dd" and stays correct for other reports.
                AddKeyValueRow(ws, ref row, "Report Date", r.ReportDate != null ? Convert.ToDateTime(r.ReportDate).ToString("dd-MM-yyyy") : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd(Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid(Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft(Mtrs)", r.DraftAft);
            }
            row++;

            // Manoeuvring — webpage shows ONLY Hours (2 decimals) and Distance (3 decimals).
            // SBE/FWE Date & Time and SBE/FWE ROB removed per webpage parity.
            ws.Cell(row, 1).Value = "Manoeuvring";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRowWithFormat(ws, ref row, "Manoeuvring Hours", r.Manoeuvring_Hrs, "0.00");
                AddKeyValueRowWithFormat(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance, "0.000");
            }
            row++;

            // Non-Routine Events — Event Name header removed per webpage; the labels appear in the first column of each data row.
            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                // Hrs defaults to 0 (not blank) when no data — matches webpage which shows 0.
                object hrsVal = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null;
                if (hrsVal == null || hrsVal == DBNull.Value) hrsVal = 0;
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), hrsVal);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                string windDir = string.IsNullOrEmpty(r.WindDirection) || r.WindDirection == "---Select---" ? "" : r.WindDirection;
                string swellDir = string.IsNullOrEmpty(r.SwellDirection) || r.SwellDirection == "---Select---" ? "" : r.SwellDirection;
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", windDir);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", swellDir);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Berthing Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddBerthingEngineSheet(XLWorkbook wb, BerthingReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Berthing Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Engine — RPM/BHP forced to 3 decimals per webpage.
            ws.Cell(row, 1).Value = "Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueRowWithFormat(ws, ref row, "RPM", r.RPM, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "BHP(hp)", r.BHP, "0.000");
                AddKeyValueRow(ws, ref row, "MCR%", r.MCR);
            }
            row++;

            // Date & Time — SBE / FWE under "Date & Time" header (matches webpage).
            ws.Cell(row, 1).Value = "Date & Time";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Date & Time";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "SBE", r.SBE_DateT.HasValue ? r.SBE_DateT.Value.ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "FWE", r.RFA_DateT.HasValue ? r.RFA_DateT.Value.ToString("yyyy-MM-dd HH:mm") : "");
            }
            row++;

            // LO & HO Consumptions — header + row labels match Daily Noon / webpage reference; 3-decimal values.
            ws.Cell(row, 1).Value = "LO & HO Consumptions";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Consumption";
            ws.Cell(row, 3).Value = "ROB";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "MECC (Ltrs)"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECC, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECC_ROB, "0.000");
                row++;
                ws.Cell(row, 1).Value = "MECYL (Ltrs)"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_MECYL, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_MECYL_ROB, "0.000");
                row++;
                ws.Cell(row, 1).Value = "AECC (Ltrs)"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_AECC, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_AECC_ROB, "0.000");
                row++;
                ws.Cell(row, 1).Value = "HYDRAULIC Oil (Ltrs)"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.LO_HO_Cons_HYDR_Oil, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.LO_HO_Cons_HYDR_Oil_ROB, "0.000");
                row++;
            }
            row++;

            // Fuel ROB in MT — header rename (no "(SBE/RFA)" suffix); SBE/FWE as separate columns;
            // SetCellValueWithDecimalFormat default "0.##########" strips trailing zeros (745.200 → 745.2).
            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "SBE";
            ws.Cell(row, 3).Value = "FWE";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtFuelROB != null && dtFuelROB.Rows.Count > 0)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    ws.Cell(row, 1).Value = dr["FuelType"]?.ToString() ?? "";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("SBE") ? dr["SBE"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("RFA") ? dr["RFA"] : null);
                    row++;
                }
            }
            row++;

            // Other ROB (3-decimal values per webpage).
            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty, "0.000");
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty, "0.000");
                row++;
            }
            row++;

            // Fuel Consumption in MT — Daily Noon Report style sub-tables (Main Engine / Aux Engine /
            // Boiler / Framo System / IGG & Incinerator / Events / Total). Reuses the shared
            // AddFuelCons*Block helpers; Events uses a Berthing-specific label set ("Stoppage at Sea"
            // long form per webpage) so the shared block isn't touched.
            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 9);
            row++;
            AddFuelConsEngineBlock(ws, ref row, "Main Engine",  true,  dtFuelCons, 2, 3, 4, 5);
            AddFuelConsEngineBlock(ws, ref row, "Aux Engine",   true,  dtFuelCons, 7, 8, 9, 10);
            AddFuelConsEngineBlock(ws, ref row, "Boiler",       false, dtFuelCons, 11, 12, 13, 14);
            AddFuelConsEngineBlock(ws, ref row, "Framo System", false, dtFuelCons, 15, 16, 17, 18);
            AddFuelConsIggIncBlock(ws, ref row, dtFuelCons);
            // Berthing-specific Events block: same shape as shared AddFuelConsEventsBlock but with
            // "Stoppage at Sea" instead of "Stoppage" per webpage spec.
            {
                int[] eventConsTypeIds = { 20, 21, 22, 23, 24, 25, 26, 27 };
                string[] eventLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Prep", "Cargo Heating", "BW Exchange", "Others" };
                ws.Cell(row, 1).Value = "Events";
                ApplyLightGrayTitle(ws, row, 1, 9);
                row++;
                ws.Cell(row, 1).Value = "Fuel";
                for (int i = 0; i < eventLabels.Length; i++) ws.Cell(row, i + 2).Value = eventLabels[i];
                ws.Range(row, 1, row, 9).Style.Font.Bold = true;
                row++;
                foreach (string ft in new[] { "VLSFO", "MDO" })
                {
                    ws.Cell(row, 1).Value = ft;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    for (int i = 0; i < eventConsTypeIds.Length; i++)
                        SetCellValueWithDecimalFormat(ws.Cell(row, i + 2), GetFuelConsByTypeAndConsType(dtFuelCons, ft, eventConsTypeIds[i]), "0.000");
                    row++;
                }
                row++;
            }
            AddFuelConsTotalBlock(ws, ref row, dtFuelCons);

            ws.Columns().AdjustToContents();
        }

        private void AddBerthingCargoSheet(XLWorkbook wb, BerthingReport r, DataTable dtCargo)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Berthing Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Cargo — one row per BR_Cargo record with "CargoName ( PortName )" as label and Qty(MT) as the column.
            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "Qty(MT)";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            if (dtCargo != null && dtCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtCargo.Rows)
                {
                    string cName = dr.Table.Columns.Contains("cargoname") && dr["cargoname"] != DBNull.Value ? dr["cargoname"].ToString().Trim() :
                                    (dr.Table.Columns.Contains("CargoName") && dr["CargoName"] != DBNull.Value ? dr["CargoName"].ToString().Trim() : "");
                    string pName = dr.Table.Columns.Contains("PortName") && dr["PortName"] != DBNull.Value ? dr["PortName"].ToString().Trim() : "";
                    string label = string.IsNullOrEmpty(cName) ? "" : cName + (string.IsNullOrEmpty(pName) ? "" : " ( " + pName + " )");
                    ws.Cell(row, 1).Value = label;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null);
                    row++;
                }
            }
            row++;

            // Slops ROB — row label "ROB" (no "(m3)"); 3-decimal values.
            ws.Cell(row, 1).Value = "Slops ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SlopsROB_Oil, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SlopsROB_Water, "0.000");
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SlopsROB_Total, "0.000");
                row++;
            }
            row++;

            // Fresh Water — moved BEFORE Ballast per webpage order; 3-decimal values.
            ws.Cell(row, 1).Value = "Fresh Water";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRowWithFormat(ws, ref row, "FW Generated (MT)", r.FW_Generated, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Consumption (MT)", r.FW_Consumption, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "ROB (MT)", r.FW_ROB, "0.000");
            }
            row++;

            // Ballast — row label "ROB" (no "(MT)"); 3-decimal value.
            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRowWithFormat(ws, ref row, "ROB", r?.Ballast_ROB, "0.000");
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Loading Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editloadingRListDashboard logic (reportdate, vesselid) to fetch all details.
        /// </summary>
        private void SaveLoadingReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasReportDateTime = tbls.Columns.Contains("ReportDateTime");
            bool hasModifyDate = tbls.Columns.Contains("ModifyDate");
            if (!hasVesselId) return;
            if (!hasReportDateTime && !hasModifyDate) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasReportDateTime && row["ReportDateTime"] != DBNull.Value && row["ReportDateTime"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ReportDateTime"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifyDate && row["ModifyDate"] != DBNull.Value && row["ModifyDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifyDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var loadingRBind = (LoadingReport)null;
                var loadList = CommonMethods.editloadingRListDashboard(reportdate, vesselId, "LoadingReport");
                loadingRBind = loadList?.Where(x => x.Id > 0).FirstOrDefault();
                if (loadingRBind == null)
                {
                    loadList = CommonMethods.editloadingRListDashboard(reportdate, vesselId, "LoadingReportR");
                    loadingRBind = loadList?.Where(x => x.Id > 0).FirstOrDefault();
                }
                if (loadingRBind == null && tbls.Columns.Contains("Id"))
                {
                    int rowId = 0;
                    if (row["Id"] != DBNull.Value && row["Id"] != null && int.TryParse(row["Id"].ToString(), out rowId) && rowId > 0)
                    {
                        loadList = CommonMethods.editloadingRList(rowId, vesselId, "LoadingReport");
                        loadingRBind = loadList?.Where(x => x.Id == rowId).FirstOrDefault();
                    }
                }
                if (loadingRBind == null) continue;

                int id = loadingRBind.Id;

                DataTable dtCargo = new DataTable();
                DataTable dtStoppage = new DataTable();
                DataTable dtPumpsUse = new DataTable();
                DataTable dtMain = new DataTable();

                try
                {
                    // Cargo: filter by LRId AND dedupe by CargoName + LoadingDatetime, taking the row
                    // with the highest Id (the latest save). LR_Cargo has been observed accumulating
                    // duplicate rows for the same report from double-submits; without the dedupe an
                    // older row would show stale values (e.g. Size_of_Manifold_Hoses_by_Vessel = 1
                    // instead of the updated 12).
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.* from LR_Cargo a " +
                        "inner join (select CargoName, LoadingDatetime, max(Id) as MaxId " +
                        "            from LR_Cargo where VesselId=" + vesselId + " and LRId=" + id +
                        "            group by CargoName, LoadingDatetime) g on a.Id = g.MaxId " +
                        "where a.VesselId=" + vesselId + " and a.LRId=" + id + " order by a.Id",
                        ConnectionBulder.con))
                        adp.Fill(dtCargo);
                    // Stoppage: actual column is `Stoppage` (renamed from `Reason`). Scope to loading
                    // stoppages (LoadingDischarged=0) to mirror the web form. Dedupe by
                    // (Stoppage + DateTimeFrom) taking the row with the highest Id — LR_Stoppage
                    // has been observed accumulating duplicates from double-submits.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.Stoppage as Reason, a.DateTimeFrom, a.DateTimeTo from LR_Stoppage a " +
                        "inner join (select Stoppage, DateTimeFrom, max(Id) as MaxId " +
                        "            from LR_Stoppage where LRId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=0 " +
                        "            group by Stoppage, DateTimeFrom) g on a.Id = g.MaxId " +
                        "where a.LRId=" + id + " and a.VesselId=" + vesselId + " and a.LoadingDischarged=0 order by a.Id",
                        ConnectionBulder.con))
                        adp.Fill(dtStoppage);
                    // Pumps: table is `tblPump` (singular) per the Loading controller; `tblPumps` returns no rows.
                    // Dedupe by pump Name (the value the user sees) — picks the latest row (MAX Id) per
                    // unique name so previous saves that duplicated tblPump or LR_DCR_PumpsUse rows
                    // don't render the same pump multiple times in the Excel sheet.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                        "inner join (select bb.Name as Name, max(aa.Id) as Id from LR_DCR_PumpsUse aa left join tblPump bb on aa.PumpId=bb.Id where aa.LRId=" + id + " and aa.VesselId=" + vesselId + " group by bb.Name) g on a.Id=g.Id " +
                        "left join tblPump b on a.PumpId=b.Id " +
                        "where a.LRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                        adp.Fill(dtPumpsUse);
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", loadingRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "LoadingReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "LoadingReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddLoadingDetailsSheet(wb, loadingRBind, dtCargo, dtStoppage, dtPumpsUse, dtMain);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddLoadingDetailsSheet(XLWorkbook wb, LoadingReport r, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Loading Details");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Loading Report";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // General Info
            if (r != null)
            {
                string voyNo = "";
                string legText = "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? "";
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
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
                // Resolve Leg via LegPortId scoped by VoyageId+VesselId.
                // VoyageLeg.Id is not unique across vessels/voyages, so the previous unscoped
                // query returned the wrong leg (e.g. "Others to Jamnagar" from another vessel).
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }

                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "Vessel", r.VesselName ?? "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                // Force date cells to text so Excel doesn't reinterpret "2025-10-03 06:42"
                // through the workstation locale and re-render as "03-10-2025 06:42".
                AddKeyValueTextRow(ws, ref row, "Report Date & Time", r.ReportDateTime != null ? Convert.ToDateTime(r.ReportDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueTextRow(ws, ref row, "ETD Date & Time", r.ETDDateTime != null ? Convert.ToDateTime(r.ETDDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                // Drafts keep 3 decimal places per maritime convention (11.750, not 11.75).
                AddKeyValueRowWithFormat(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft, "0.000");
            }
            row++;

            // Cargo List — 14 columns matching the web edit form headers exactly.
            // Rendered BEFORE Letter of Protests per stakeholder request.
            ws.Cell(row, 1).Value = "Cargo Details";
            ApplyLightGrayTitle(ws, row, 1, 14);
            row++;
            if (dtCargo != null && dtCargo.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value  = "Cargo Grades";
                ws.Cell(row, 2).Value  = "Commence Loading Date & Time";
                ws.Cell(row, 3).Value  = "Terminal Loading Rate (m3/hr)";
                ws.Cell(row, 4).Value  = "Loading Rate Accepted by Vessel (m3/hr)";
                ws.Cell(row, 5).Value  = "Average Achieved Loading Rate (m3/hr)";
                ws.Cell(row, 6).Value  = "No of Manifold / Hoses by Terminal";
                ws.Cell(row, 7).Value  = "Size of Manifold / Hoses by Terminal (Inches)";
                ws.Cell(row, 8).Value  = "No of Manifold / Hoses by Vessel";
                ws.Cell(row, 9).Value  = "Size of Manifold / Hoses by Vessel (Inches)";
                ws.Cell(row, 10).Value = "Shore Line Distance (mtrs)";
                ws.Cell(row, 11).Value = "Quantity onboard (MT)";
                ws.Cell(row, 12).Value = "Balance Quantity to be Loaded (MT)";
                ws.Cell(row, 13).Value = "ETC Comp Date & Time";
                ws.Cell(row, 14).Value = "Actual Comp Date & Time";
                ws.Range(row, 1, row, 14).Style.Font.Bold = true;
                ws.Range(row, 1, row, 14).Style.Alignment.WrapText = true;
                row++;

                foreach (DataRow dr in dtCargo.Rows)
                {
                    // Cargo Grades — webpage shows "CargoName ( PortName )" (e.g. "MS BS-VI ( Doha )").
                    // LR_Cargo has both CargoName and PortName columns directly. Mirrors the Discharging Excel.
                    string cName = dr.Table.Columns.Contains("CargoName") && dr["CargoName"] != DBNull.Value ? dr["CargoName"].ToString().Trim() : "";
                    string pName = dr.Table.Columns.Contains("PortName") && dr["PortName"] != DBNull.Value ? dr["PortName"].ToString().Trim() : "";
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "" : cName + (string.IsNullOrEmpty(pName) ? "" : " ( " + pName + " )");
                    AddDateTextCell(ws.Cell(row, 2), dr, "LoadingDatetime");
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3),  dr.Table.Columns.Contains("TerminalLoadingRate") ? dr["TerminalLoadingRate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4),  dr.Table.Columns.Contains("LoadingRateAccepted") ? dr["LoadingRateAccepted"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 5),  dr.Table.Columns.Contains("AverageAchievedLoadingRate") ? dr["AverageAchievedLoadingRate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6),  dr.Table.Columns.Contains("No_Manifold_Hoses_by_Terminal") ? dr["No_Manifold_Hoses_by_Terminal"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 7),  dr.Table.Columns.Contains("Size_of_Manifold_Hoses_by_Terminal") ? dr["Size_of_Manifold_Hoses_by_Terminal"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 8),  dr.Table.Columns.Contains("No_Manifold_Hoses_by_Vessel") ? dr["No_Manifold_Hoses_by_Vessel"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 9),  dr.Table.Columns.Contains("Size_of_Manifold_Hoses_by_Vessel") ? dr["Size_of_Manifold_Hoses_by_Vessel"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 10), dr.Table.Columns.Contains("ShoreLineDistance") ? dr["ShoreLineDistance"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 11), dr.Table.Columns.Contains("QuantityOnboard") ? dr["QuantityOnboard"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 12), dr.Table.Columns.Contains("BalanceQuantityLoaded") ? dr["BalanceQuantityLoaded"] : null);
                    AddDateTextCell(ws.Cell(row, 13), dr, "EstCompDateTime");
                    AddDateTextCell(ws.Cell(row, 14), dr, "ActualCompDateTime");
                    row++;
                }
            }
            row++;

            // LOP Fields — rendered AFTER Cargo Details per stakeholder request.
            ws.Cell(row, 1).Value = "Letter of Protests";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Time", r.Times ?? "");
                AddKeyValueRow(ws, ref row, "Rate", r.Rate ?? "");
                AddKeyValueRow(ws, ref row, "Hose Connection", r.Hose_Connection ?? "");
                AddKeyValueRow(ws, ref row, "High H2S", r.High_H2S ?? "");
            }
            row++;

            // Stoppage List
            ws.Cell(row, 1).Value = "Stoppage Details";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (dtStoppage != null && dtStoppage.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Reason";
                ws.Cell(row, 2).Value = "From";
                ws.Cell(row, 3).Value = "To";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;

                foreach (DataRow dr in dtStoppage.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("Reason") ? (dr["Reason"]?.ToString() ?? "") : "";
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("DateTimeFrom") && dr["DateTimeFrom"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeFrom"]).ToString("yyyy-MM-dd HH:mm") : "";
                    ws.Cell(row, 3).Value = dr.Table.Columns.Contains("DateTimeTo") && dr["DateTimeTo"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeTo"]).ToString("yyyy-MM-dd HH:mm") : "";
                    row++;
                }
            }
            row++;

            // Pumps Use
            ws.Cell(row, 1).Value = "Ballast Pump Use";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtPumpsUse != null && dtPumpsUse.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Pump Name";
                ws.Cell(row, 2).Value = "Rate";
                ws.Range(row, 1, row, 2).Style.Font.Bold = true;
                row++;

                // Dedupe by pump name so duplicates in lr_dcr_pumpsuse or tblPump don't render multiple rows.
                var seenPumpsInUse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow dr in dtPumpsUse.Rows)
                {
                    string pumpName = dr.Table.Columns.Contains("PumpName") ? (dr["PumpName"]?.ToString() ?? "") : (dr.Table.Columns.Contains("Name") ? (dr["Name"]?.ToString() ?? "") : "");
                    if (!seenPumpsInUse.Add(pumpName.Trim())) continue;
                    ws.Cell(row, 1).Value = pumpName;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null);
                    row++;
                }
            }
            row++;

            // Remarks
            ws.Cell(row, 1).Value = "Remarks";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Discharging Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editdischargingRListDashbord logic (reportdate, vesselid) to fetch all details.
        /// </summary>
        private void SaveDischargingReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasReportDateTime = tbls.Columns.Contains("ReportDateTime");
            bool hasModifyDate = tbls.Columns.Contains("ModifyDate");
            if (!hasVesselId) return;
            if (!hasReportDateTime && !hasModifyDate) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasReportDateTime && row["ReportDateTime"] != DBNull.Value && row["ReportDateTime"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ReportDateTime"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifyDate && row["ModifyDate"] != DBNull.Value && row["ModifyDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifyDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                DischargingReport disRBind = null;
                var disList = CommonMethods.editdischargingRListDashbord(reportdate, vesselId, "DischargingReport");
                disRBind = disList?.Where(x => x.Id > 0).FirstOrDefault();
                if (disRBind == null)
                {
                    disList = CommonMethods.editdischargingRListDashbord(reportdate, vesselId, "DischargingReportR");
                    disRBind = disList?.Where(x => x.Id > 0).FirstOrDefault();
                }
                if (disRBind == null && tbls.Columns.Contains("Id"))
                {
                    int rowId = 0;
                    if (row["Id"] != DBNull.Value && row["Id"] != null && int.TryParse(row["Id"].ToString(), out rowId) && rowId > 0)
                    {
                        disList = CommonMethods.editdischargingRList(rowId, vesselId, "DischargingReport");
                        disRBind = disList?.Where(x => x.Id == rowId).FirstOrDefault();
                    }
                }
                if (disRBind == null) continue;

                int id = disRBind.Id;

                DataTable dtCargo = new DataTable();
                DataTable dtStoppage = new DataTable();
                DataTable dtPumpsUse = new DataTable();
                DataTable dtMain = new DataTable();
                // Pivoted pump tables — matches DischargingReportEmailTemplate + webpage layout
                // (Ballast Pumps in Use uses PumpUseId=2; Cargo Pumps in Use uses PumpUseId=1).
                DataTable dtBallastPumps = new DataTable();
                DataTable dtCargoPumpsInUse = new DataTable();

                try
                {
                    // DS_Cargo FK to DischargingReport is `LRId` (per spInsertDischargeingCargoList SP
                    // and DischargingController.GetDischargeCargo query). The previous filter `DSId=`
                    // didn't match any rows so dtCargo was empty and the Cargo Details sheet had no rows.
                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from DS_Cargo where LRId=" + id + " and VesselId=" + vesselId, ConnectionBulder.con))
                        adp.Fill(dtCargo);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from LR_Stoppage where DCId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=1", ConnectionBulder.con))
                        adp.Fill(dtStoppage);
                    // tblPump is singular — earlier code used "tblPumps" (typo) which threw and
                    // short-circuited subsequent fetches in this try block. LEFT JOIN so pumps
                    // without a matching tblPump row still appear.
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.Name as PumpName from LR_DCR_PumpsUse a left join tblPump b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                        adp.Fill(dtPumpsUse);
                    // Ballast pumps in use (PumpUseId=2) — INNER JOIN tblPump on PumpId AND PumpUseId
                    // so the pump's true type in tblPump must match. Mirrors
                    // DischargingController.GetPumpsINUse exactly; prevents pumps mis-saved with
                    // wrong PumpUseId from leaking into this section.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                        // ROW_NUMBER prefers a NON-ZERO Rate then latest Id — LR_DCR_PumpsUse accumulates
                        // duplicate rows on re-save (real-rate row + a later 0.000 row), so the old
                        // max(Id) picked the 0.000 duplicate. Mirrors the discharging email template.
                        "inner join (select aa.Id, ROW_NUMBER() over (partition by bb.Name order by (case when aa.Rate <> 0 then 1 else 0 end) desc, aa.Id desc) as rn from LR_DCR_PumpsUse aa inner join tblPump bb on aa.PumpId=bb.Id and aa.PumpUseId=bb.PumpUseId where aa.DCRId=" + id + " and aa.VesselId=" + vesselId + " and aa.PumpUseId=2) g on a.Id=g.Id and g.rn=1 " +
                        "inner join tblPump b on a.PumpId=b.Id and a.PumpUseId=b.PumpUseId " +
                        "where a.DCRId=" + id + " and a.VesselId=" + vesselId + " and a.PumpUseId=2", ConnectionBulder.con))
                        adp.Fill(dtBallastPumps);
                    // Cargo pumps in use (PumpUseId=1) — same strict-join pattern so B/P# 1
                    // mis-saved with PumpUseId=1 won't appear in the Cargo Pumps pivot.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a " +
                        "inner join (select aa.Id, ROW_NUMBER() over (partition by bb.Name order by (case when aa.Rate <> 0 then 1 else 0 end) desc, aa.Id desc) as rn from LR_DCR_PumpsUse aa inner join tblPump bb on aa.PumpId=bb.Id and aa.PumpUseId=bb.PumpUseId where aa.DCRId=" + id + " and aa.VesselId=" + vesselId + " and aa.PumpUseId=1) g on a.Id=g.Id and g.rn=1 " +
                        "inner join tblPump b on a.PumpId=b.Id and a.PumpUseId=b.PumpUseId " +
                        "where a.DCRId=" + id + " and a.VesselId=" + vesselId + " and a.PumpUseId=1", ConnectionBulder.con))
                        adp.Fill(dtCargoPumpsInUse);
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", disRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "DischargingReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "DischargingReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddDischargingDetailsSheet(wb, disRBind, dtCargo, dtStoppage, dtPumpsUse, dtBallastPumps, dtCargoPumpsInUse, dtMain);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddDischargingDetailsSheet(XLWorkbook wb, DischargingReport r, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtBallastPumps, DataTable dtCargoPumpsInUse, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Discharging Details");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Discharging Report";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // General Info
            if (r != null)
            {
                string voyNo = "";
                string legText = "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (dtMain.Columns.Contains("VoyageNumber")) voyNo = dr["VoyageNumber"]?.ToString() ?? "";
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? "";
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

                // Resolve Leg from VoyageLeg when the sync SP doesn't carry it — same
                // inline pattern as Loading/Departure Excel above (and DischargingReportEmailTemplate).
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }

                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date & Time", r.ReportDateTime != null ? Convert.ToDateTime(r.ReportDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "ETD Date & Time", r.ETDDateTime != null ? Convert.ToDateTime(r.ETDDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Fwd", r.DraftFwd, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Mid", r.DraftMid, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Draft Aft", r.DraftAft, "0.000");
            }
            row++;

            // Cargo List — placed BEFORE Letter of Protests to match webpage order.
            // 15-column layout per webpage: Cargo Grades, Commence Discharge Date & Time,
            // Terminal Acceptable Discharging Rate, Discharging Pressure Requested,
            // Average Discharge Rate By Vessel, Average Discharge pressure By Vessel,
            // No of Pumps in use, No of Manifold/Hoses by Terminal,
            // Size of Manifold/Hoses by Terminal (Inches), No of Manifold/Hoses by Vessel,
            // Size of Manifold/Hoses by Vessel (Inches), Total Cargo Discharged,
            // Balance Cargo to be Discharged, ETC Comp Date & Time, Actual Comp Date & Time.
            ws.Cell(row, 1).Value = "Cargo Details";
            ApplyLightGrayTitle(ws, row, 1, 15);
            row++;
            if (dtCargo != null && dtCargo.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Cargo Grades";
                ws.Cell(row, 2).Value = "Commence Discharge Date & Time";
                ws.Cell(row, 3).Value = "Terminal Acceptable Discharging Rate";
                ws.Cell(row, 4).Value = "Discharging Pressure Requested";
                ws.Cell(row, 5).Value = "Average Discharge Rate By Vessel";
                ws.Cell(row, 6).Value = "Average Discharge pressure By Vessel";
                ws.Cell(row, 7).Value = "No of Pumps in use";
                ws.Cell(row, 8).Value = "No of Manifold / Hoses by Terminal";
                ws.Cell(row, 9).Value = "Size of Manifold / Hoses by Terminal (Inches)";
                ws.Cell(row, 10).Value = "No of Manifold / Hoses by Vessel";
                ws.Cell(row, 11).Value = "Size of Manifold / Hoses by Vessel (Inches)";
                ws.Cell(row, 12).Value = "Total Cargo Discharged";
                ws.Cell(row, 13).Value = "Balance Cargo to be Discharged";
                ws.Cell(row, 14).Value = "ETC Comp Date & Time";
                ws.Cell(row, 15).Value = "Actual Comp Date & Time";
                ws.Range(row, 1, row, 15).Style.Font.Bold = true;
                ws.Range(row, 1, row, 15).Style.Alignment.WrapText = true;
                row++;

                foreach (DataRow dr in dtCargo.Rows)
                {
                    // Cargo Grades — webpage shows "CargoName ( PortName )"; concat per spInsertDischargeingCargoList schema (both columns live on DS_Cargo).
                    string cName = dr.Table.Columns.Contains("CargoName") && dr["CargoName"] != DBNull.Value ? dr["CargoName"].ToString().Trim() : "";
                    string pName = dr.Table.Columns.Contains("PortName") && dr["PortName"] != DBNull.Value ? dr["PortName"].ToString().Trim() : "";
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "" : cName + (string.IsNullOrEmpty(pName) ? "" : " ( " + pName + " )");
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("DischargeDatetime") && dr["DischargeDatetime"] != DBNull.Value ? Convert.ToDateTime(dr["DischargeDatetime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("Terminal_Acceptable_Discharging_Rate") ? dr["Terminal_Acceptable_Discharging_Rate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), dr.Table.Columns.Contains("Discharging_pressure_Requested") ? dr["Discharging_pressure_Requested"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 5), dr.Table.Columns.Contains("Average_Discharge_Rate_ByVessel") ? dr["Average_Discharge_Rate_ByVessel"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6), dr.Table.Columns.Contains("Average_Discharge_pressure_ByVessel") ? dr["Average_Discharge_pressure_ByVessel"] : null);
                    ws.Cell(row, 7).Value = dr.Table.Columns.Contains("No_of_Pumps_Use") && dr["No_of_Pumps_Use"] != DBNull.Value ? dr["No_of_Pumps_Use"].ToString() : "";
                    ws.Cell(row, 8).Value = dr.Table.Columns.Contains("No_Manifold_Hoses_by_Terminal") && dr["No_Manifold_Hoses_by_Terminal"] != DBNull.Value ? dr["No_Manifold_Hoses_by_Terminal"].ToString() : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 9), dr.Table.Columns.Contains("Size_of_Manifold_Hoses_by_Terminal") ? dr["Size_of_Manifold_Hoses_by_Terminal"] : null);
                    ws.Cell(row, 10).Value = dr.Table.Columns.Contains("No_Manifold_Hoses_by_Vessel") && dr["No_Manifold_Hoses_by_Vessel"] != DBNull.Value ? dr["No_Manifold_Hoses_by_Vessel"].ToString() : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 11), dr.Table.Columns.Contains("Size_of_Manifold_Hoses_by_Vessel") ? dr["Size_of_Manifold_Hoses_by_Vessel"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 12), dr.Table.Columns.Contains("Total_CargoDischarged") ? dr["Total_CargoDischarged"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 13), dr.Table.Columns.Contains("Balance_Cargo_ToBe_Deischarged") ? dr["Balance_Cargo_ToBe_Deischarged"] : null);
                    ws.Cell(row, 14).Value = dr.Table.Columns.Contains("EstCompDateTime") && dr["EstCompDateTime"] != DBNull.Value ? Convert.ToDateTime(dr["EstCompDateTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    ws.Cell(row, 15).Value = dr.Table.Columns.Contains("ActualCompDateTime") && dr["ActualCompDateTime"] != DBNull.Value ? Convert.ToDateTime(dr["ActualCompDateTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "";
                    row++;
                }
            }
            row++;

            // LOP Fields — placed AFTER Cargo Details to match webpage order.
            ws.Cell(row, 1).Value = "Letter of Protests";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Time", r.Times ?? "");
                AddKeyValueRow(ws, ref row, "Rate", r.Rate ?? "");
                AddKeyValueRow(ws, ref row, "Hose Connection", r.Hose_Connection ?? "");
                AddKeyValueRow(ws, ref row, "High H2S", r.High_H2S ?? "");
            }
            row++;

            // Stoppage List
            ws.Cell(row, 1).Value = "Stoppage Reason";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (dtStoppage != null && dtStoppage.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Stoppage Reason";
                ws.Cell(row, 2).Value = "Date Time From";
                ws.Cell(row, 3).Value = "Date Time To";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                row++;

                foreach (DataRow dr in dtStoppage.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("Reason") ? (dr["Reason"]?.ToString() ?? "") : (dr.Table.Columns.Contains("Stoppage") ? (dr["Stoppage"]?.ToString() ?? "") : "");
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("DateTimeFrom") && dr["DateTimeFrom"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeFrom"]).ToString("yyyy-MM-dd HH:mm") : "";
                    ws.Cell(row, 3).Value = dr.Table.Columns.Contains("DateTimeTo") && dr["DateTimeTo"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeTo"]).ToString("yyyy-MM-dd HH:mm") : "";
                    row++;
                }
            }
            row++;

            // Ballast Pumps in Use — column-header layout (Name | Rate) with one row per pump.
            ws.Cell(row, 1).Value = "Ballast Pumps in Use";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            ws.Cell(row, 1).Value = "Name";
            ws.Cell(row, 2).Value = "Rate";
            ws.Range(row, 1, row, 2).Style.Font.Bold = true;
            row++;
            if (dtBallastPumps != null && dtBallastPumps.Rows.Count > 0)
            {
                // Dedupe by PumpName — same pattern as Loading's Ballast Pump Use loop.
                var seenBallastPumps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow dr in dtBallastPumps.Rows)
                {
                    string pumpName = (dr.Table.Columns.Contains("PumpName") ? dr["PumpName"]?.ToString() : "")?.Trim() ?? "";
                    if (!seenBallastPumps.Add(pumpName)) continue;
                    ws.Cell(row, 1).Value = pumpName;
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null, "0.000");
                    row++;
                }
            }
            row++;

            // Cargo Pumps in Use — pivoted layout: pump names span column headers, then a "Name"
            // row repeats the names and a "Rate" row shows each pump's rate.
            // Dedupe cargo pump rows once before pivoting (same pattern as Ballast Pump loop above)
            // so the pivot doesn't explode into 15 columns when LR_DCR_PumpsUse has repeated rows.
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
            ws.Cell(row, 1).Value = "Cargo Pumps in Use";
            int cargoPumpCount = cargoPumps.Count;
            int cargoPumpSpan = Math.Max(2, cargoPumpCount + 1);
            ApplyLightGrayTitle(ws, row, 1, cargoPumpSpan);
            row++;
            if (cargoPumpCount > 0)
            {
                // Column-header row — blank label + pump names
                ws.Cell(row, 1).Value = "";
                for (int i = 0; i < cargoPumpCount; i++)
                {
                    var dr = cargoPumps[i];
                    ws.Cell(row, i + 2).Value = dr["PumpName"]?.ToString() ?? "";
                }
                ws.Range(row, 1, row, cargoPumpSpan).Style.Font.Bold = true;
                row++;
                // Name row — pump names again as data
                ws.Cell(row, 1).Value = "Name";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int i = 0; i < cargoPumpCount; i++)
                {
                    var dr = cargoPumps[i];
                    ws.Cell(row, i + 2).Value = dr["PumpName"]?.ToString() ?? "";
                }
                row++;
                // Rate row — each pump's rate, 3-decimal format
                ws.Cell(row, 1).Value = "Rate";
                ws.Cell(row, 1).Style.Font.Bold = true;
                for (int i = 0; i < cargoPumpCount; i++)
                {
                    var dr = cargoPumps[i];
                    SetCellValueWithDecimalFormat(ws.Cell(row, i + 2), dr.Table.Columns.Contains("Rate") ? dr["Rate"] : null, "0.000");
                }
                row++;
            }
            row++;

            // Power Packs — two key-value rows sourced from DischargingReport model.
            ws.Cell(row, 1).Value = "Power Packs";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            AddKeyValueRow(ws, ref row, "No of Power Packs onboard", r?.Power_Packs_onboard);
            AddKeyValueRow(ws, ref row, "No of Power Packs used", r?.Power_Packs_Used);
            row++;

            // Discharge Report Remarks — single "Remarks" key-value row.
            ws.Cell(row, 1).Value = "Discharge Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 2);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Bulk Noon Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editnoonRListdashboard logic (reportdate, vesselid) to fetch all details.
        /// Structured like Berthing Report with Navigation, Engine, Cargo sheets.
        /// </summary>
        private void SaveBulkNoonReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasDate = tbls.Columns.Contains("Date");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            if (!hasVesselId || (!hasDate && !hasModifiedDate)) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasDate && row["Date"] != DBNull.Value && row["Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Date"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new DailyNoonReport();
                vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselId, "DailyNoonReport");
                var noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (noonRBind == null) continue;

                int id = noonRBind.Id;

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtMain = new DataTable();

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    if (dtFuelROB.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, OtherROB from tbl_FuelROB where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                        {
                            DataTable dtRob = new DataTable();
                            adp.Fill(dtRob);
                            foreach (DataRow r in dtRob.Rows)
                            {
                                int ftId = Convert.ToInt32(r["FuelType_Id"]);
                                var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                                string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                                dtFuelROB.Rows.Add(fuelType, r["OtherROB"]?.ToString() ?? "");
                            }
                        }
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", noonRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "BulkNoonReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddBulkNoonNavigationSheet(wb, noonRBind, dtNonRoutine, dtMain);
                    AddBulkNoonEngineSheet(wb, noonRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddBulkNoonCargoSheet(wb, noonRBind);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddBulkNoonNavigationSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Bulk Noon Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = r.LegPortName ?? "";
                string portStatusText = r.PortStatus?.ToString() ?? "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber"))
                        voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                    if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
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
                // Resolve Leg from VoyageLeg scoped by LegPortId+VoyageId+VesselId.
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        if (r.LegPortId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + r.LegPortId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                        if (string.IsNullOrEmpty(legText) && r.VoyageId > 0)
                        {
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select top 1 LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId + " and IsActive=1", ConnectionBulder.con))
                            {
                                DataTable dtLeg = new DataTable();
                                adp.Fill(dtLeg);
                                if (dtLeg.Rows.Count > 0) legText = dtLeg.Rows[0]["Leg"]?.ToString() ?? "";
                            }
                        }
                    }
                    catch { }
                }
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Status", r.VesselStatus ?? "");
                AddKeyValueRow(ws, ref row, "Latitude", r.Latitude ?? "");
                AddKeyValueRow(ws, ref row, "Longitude", r.Longitude ?? "");
                AddKeyValueRow(ws, ref row, "At Sea/In Port", r.AtSeaOrPort ?? "");
                AddKeyValueRow(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueRow(ws, ref row, "Displacement(MT)", r.Displacement);
                AddKeyValueRow(ws, ref row, "CP Speed(Kts)", r.CP_Speed);
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date", r.Date != null ? Convert.ToDateTime(r.Date).ToString(ExcelDateFormat) : "");
                AddKeyValueRow(ws, ref row, "ETA", r.ETA != null ? Convert.ToDateTime(r.ETA).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Speed - Distance - Time";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist);
                AddKeyValueRow(ws, ref row, "Log Dist(NM)", r.LogDist);
                AddKeyValueRow(ws, ref row, "Engine Dist(NM)", r.EngineDist);
                AddKeyValueRow(ws, ref row, "Total Distance (Dep to Curr)(NM)", r.TotalDistance);
                AddKeyValueRow(ws, ref row, "Dist to Go (DTG)(NM)", r.DistToGo_DTG);
                AddKeyValueRow(ws, ref row, "Stmg Time Noon to Noon(Hrs)", r.StmgTime);
                AddKeyValueRow(ws, ref row, "Total Time (Dep to Curr)(Hrs)", r.TotalTime);
                AddKeyValueRow(ws, ref row, "Actual Speed Noon to Noon(Kts)", r.Act_Speed);
                AddKeyValueRow(ws, ref row, "Gen Avg Speed (Dep to Curr)(Kts)", r.Gen_Avg_Speed);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Event Name";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Bulk Noon Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddBulkNoonEngineSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Bulk Noon Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueRow(ws, ref row, "RPM", r.RPM);
                AddKeyValueRow(ws, ref row, "BHP(hp)", r.BHP);
                AddKeyValueRow(ws, ref row, "MCR%", r.MCR);
                AddKeyValueRow(ws, ref row, "M/E Control Location", r.ME_ControlLoc);
                AddKeyValueRow(ws, ref row, "SCAV. Manifold Pressure (Bars)", r.SCAV_ManiPress);
                AddKeyValueRow(ws, ref row, "SCAV. Temp (Deg Centigrade)", r.SCAV_Temp);
                AddKeyValueRow(ws, ref row, "Max Exhaust Temp (Deg Centigrade)", r.Max_Exhaust_Temp);
                AddKeyValueRow(ws, ref row, "Min Exhaust Temp (Deg Centigrade)", r.Min_Exhaust_Temp);
                AddKeyValueRow(ws, ref row, "SW Temp (Deg Centigrade)", r.SW_Temp);
                AddKeyValueRow(ws, ref row, "ER Temp (Deg Centigrade)", r.ER_Temp);
            }
            row++;

            ws.Cell(row, 1).Value = "Lube Oil & Hydraulic Oil";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "ME Crosshead Cons", r.LO_HO_Cons_MECC);
                AddKeyValueRow(ws, ref row, "ME Cylinder Cons", r.LO_HO_Cons_MECYL);
                AddKeyValueRow(ws, ref row, "AE Crosshead Cons", r.LO_HO_Cons_AECC);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil);
                AddKeyValueRow(ws, ref row, "ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB);
                AddKeyValueRow(ws, ref row, "ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB);
                AddKeyValueRow(ws, ref row, "AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string robVal = dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"]?.ToString() : "";
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal ?? "");
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Bunker Received in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty);
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Aux. Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Running Hrs No.1", r.AE_RungHrs_No1);
                AddKeyValueRow(ws, ref row, "Running Hrs No.2", r.AE_RungHrs_No2);
                AddKeyValueRow(ws, ref row, "Running Hrs No.3", r.AE_RungHrs_No3);
                AddKeyValueRow(ws, ref row, "Running Hrs No.4", r.AE_RungHrs_No4);
                AddKeyValueRow(ws, ref row, "Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen);
                AddKeyValueRow(ws, ref row, "Load No.1 (KW)", r.AE_Load_No1);
                AddKeyValueRow(ws, ref row, "Load No.2 (KW)", r.AE_Load_No2);
                AddKeyValueRow(ws, ref row, "Load No.3 (KW)", r.AE_Load_No3);
                AddKeyValueRow(ws, ref row, "Load No.4 (KW)", r.AE_Load_No4);
                AddKeyValueRow(ws, ref row, "Load Shaft Gen (KW)", r.AE_Load_ShaftGen);
                AddKeyValueRow(ws, ref row, "Extra Run Reason", r.AE_Extra_Run_Reason);
            }
            row++;

            ws.Cell(row, 1).Value = "Boiler's";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Boiler No.1 Running Hrs", r.BR_RungHrs_No1);
                AddKeyValueRow(ws, ref row, "Boiler No.2 Running Hrs", r.BR_RungHrs_No2);
                AddKeyValueRow(ws, ref row, "Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1);
                AddKeyValueRow(ws, ref row, "Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                var vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
                var mdo = GetFuelConsByType(dtFuelCons, "MDO");
                AddKeyValueRow(ws, ref row, "VLSFO (Main/Aux/Boiler/Total)", FormatDec(vlsfo));
                AddKeyValueRow(ws, ref row, "MDO (Main/Aux/Boiler/Total)", FormatDec(mdo));
            }
            ws.Columns().AdjustToContents();
        }

        private void AddBulkNoonCargoSheet(XLWorkbook wb, DailyNoonReport r)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Bulk Noon Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Slops ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SLOPS_ROB_OXY_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SLOPS_ROB_OXY_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SLOPS_ROB_OXY_Total);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "ROB (MT)", r?.Ballast_ROB);
            row++;

            ws.Cell(row, 1).Value = "Fresh Water";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueRow(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueRow(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Consumption Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editnoonRListdashboard logic (reportdate, vesselid) to fetch all details.
        /// Structured like Berthing Report with Navigation, Engine, Cargo sheets but focused on consumption data.
        /// </summary>
        private void SaveConsumptionReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasDate = tbls.Columns.Contains("Date");
            bool hasModifiedDate = tbls.Columns.Contains("ModifiedDate");
            if (!hasVesselId || (!hasDate && !hasModifiedDate)) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                DateTime? reportDateVal = null;
                if (hasDate && row["Date"] != DBNull.Value && row["Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Date"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue && hasModifiedDate && row["ModifiedDate"] != DBNull.Value && row["ModifiedDate"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["ModifiedDate"].ToString(), out d)) reportDateVal = d;
                }
                if (!reportDateVal.HasValue) continue;

                string reportdate = reportDateVal.Value.ToString("yyyy-MM-dd");
                string key = vesselId + "_" + reportdate;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var vd = new DailyNoonReport();
                vd.GetNoonRList = CommonMethods.editnoonRListdashboard(reportdate, vesselId, "DailyNoonReport");
                var noonRBind = vd.GetNoonRList?.Where(x => x.Id > 0).FirstOrDefault();
                if (noonRBind == null) continue;

                int id = noonRBind.Id;

                DataTable dtFuelCons = new DataTable();
                DataTable dtFuelROB = new DataTable();
                DataTable dtBunker = new DataTable();
                DataTable dtNonRoutine = new DataTable();
                DataTable dtMain = new DataTable();

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.Value, a.ConsTypeId, b.FuelType from Fuel_Cons_NR a inner join tblFuelType b on a.FuelTypeId=b.Id where a.Noon_Report_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1 and a.ConsTypeId not in (1,6) order by a.FuelTypeId, a.ConsTypeId", ConnectionBulder.con))
                        adp.Fill(dtFuelCons);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.OtherROB from tbl_FuelROB a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtFuelROB);
                    if (dtFuelROB.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, OtherROB from tbl_FuelROB where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
                        {
                            DataTable dtRob = new DataTable();
                            adp.Fill(dtRob);
                            foreach (DataRow r in dtRob.Rows)
                            {
                                int ftId = Convert.ToInt32(r["FuelType_Id"]);
                                var ftRow = dtFuelTypes.AsEnumerable().FirstOrDefault(x => Convert.ToInt32(x["Id"]) == ftId);
                                string fuelType = ftRow != null ? ftRow["FuelType"].ToString() : "";
                                dtFuelROB.Rows.Add(fuelType, r["OtherROB"]?.ToString() ?? "");
                            }
                        }
                    }
                    using (SqlDataAdapter adp = new SqlDataAdapter("select b.FuelType, a.Receipt from tbl_BunkerLReceipt a inner join tblFuelType b on a.FuelType_Id=b.Id where a.TableMax_Id=" + id + " and a.VesselId=" + vesselId + " and a.ReportType_Id=1", ConnectionBulder.con))
                        adp.Fill(dtBunker);
                    if (dtBunker.Rows.Count == 0)
                    {
                        DataTable dtFuelTypes = new DataTable();
                        using (SqlDataAdapter adp = new SqlDataAdapter("select Id, FuelType from tblFuelType order by Id", ConnectionBulder.con))
                            adp.Fill(dtFuelTypes);
                        using (SqlDataAdapter adp = new SqlDataAdapter("select FuelType_Id, Receipt from tbl_BunkerLReceipt where TableMax_Id=" + id + " and VesselId=" + vesselId + " and ReportType_Id=1 order by FuelType_Id", ConnectionBulder.con))
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=1 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
                        adp.Fill(dtNonRoutine);
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", noonRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", reportdate);
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "DailyNoonReport");
                        cmd.Parameters.AddWithValue("@id", id);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = reportDateVal.Value;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "ConsumptionReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + id + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddConsumptionNavigationSheet(wb, noonRBind, dtNonRoutine, dtMain);
                    AddConsumptionEngineSheet(wb, noonRBind, dtFuelCons, dtFuelROB, dtBunker);
                    AddConsumptionCargoSheet(wb, noonRBind);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddConsumptionNavigationSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtNonRoutine, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Navigation");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Consumption Report - Navigation";
            var rngNav = ws.Range(row, 1, row, 3);
            rngNav.Merge();
            rngNav.Style.Font.Bold = true;
            rngNav.Style.Font.FontSize = 16;
            rngNav.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                string legText = r.LegPortName ?? "";
                string portStatusText = r.PortStatus?.ToString() ?? "";
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber"))
                        voyNo = dr["VoyageNumber"]?.ToString();
                    if (dtMain.Columns.Contains("Leg")) legText = dr["Leg"]?.ToString() ?? legText;
                    if (dtMain.Columns.Contains("PortStatusName")) portStatusText = dr["PortStatusName"]?.ToString() ?? portStatusText;
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
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Status", r.VesselStatus ?? "");
                AddKeyValueRow(ws, ref row, "Laden/Ballast", r.VesselStatus ?? "");
                AddKeyValueRow(ws, ref row, "At Sea/In Port", r.AtSeaOrPort ?? "");
                AddKeyValueRow(ws, ref row, "In Port Status", portStatusText);
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date", r.Date != null ? Convert.ToDateTime(r.Date).ToString(ExcelDateFormat) : "");
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
            }
            row++;

            ws.Cell(row, 1).Value = "Speed - Distance";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Dist Noon to Noon (DMG)(NM)", r.NoonToNoonDMG_Dist);
                AddKeyValueRow(ws, ref row, "Actual Speed Noon to Noon(Kts)", r.Act_Speed);
                AddKeyValueRow(ws, ref row, "SLIP%", r.Slip);
                AddKeyValueRow(ws, ref row, "RPM", r.RPM);
            }
            row++;

            ws.Cell(row, 1).Value = "Non-Routine Events";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            ws.Cell(row, 1).Value = "Event Name";
            ws.Cell(row, 2).Value = "Owners/Charterers Account";
            ws.Cell(row, 3).Value = "Hrs.";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            string[] nreLabels = { "Stoppage at Sea", "Deviation", "Slow Steaming", "Bad Weather", "COT Preparation", "Cargo Heating", "BW Exchange" };
            for (int i = 0; i < 7; i++)
            {
                ws.Cell(row, 1).Value = nreLabels[i];
                ws.Cell(row, 2).Value = (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? (dtNonRoutine.Rows[i]["ChartererAccount"]?.ToString() ?? "") : "";
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), (dtNonRoutine != null && i < dtNonRoutine.Rows.Count) ? dtNonRoutine.Rows[i]["Hours"] : null);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Weather";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Sea State", r.SeaState);
                AddKeyValueRow(ws, ref row, "Wind Direction", r.WindDirection);
                AddKeyValueRow(ws, ref row, "Wind Force(BF Scale)", r.WindForce);
                AddKeyValueRow(ws, ref row, "Swell Direction", r.SwellDirection);
                AddKeyValueRow(ws, ref row, "Swell Height (mtrs)", r.SwellHeight);
                AddKeyValueRow(ws, ref row, "Wave Length (mtrs)", r.WaveLength);
                AddKeyValueRow(ws, ref row, "Wave Height (mtrs)", r.WaveHeight);
            }
            row++;

            ws.Cell(row, 1).Value = "Consumption Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddConsumptionEngineSheet(XLWorkbook wb, DailyNoonReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Consumption Report - Engine";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                var vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
                var mdo = GetFuelConsByType(dtFuelCons, "MDO");
                AddKeyValueRow(ws, ref row, "VLSFO (Main/Aux/Boiler/Total)", FormatDec(vlsfo));
                AddKeyValueRow(ws, ref row, "MDO (Main/Aux/Boiler/Total)", FormatDec(mdo));
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string robVal = dr.Table.Columns.Contains("OtherROB") ? dr["OtherROB"]?.ToString() : "";
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal ?? "");
                }
            }
            row++;

            ws.Cell(row, 1).Value = "Bunker Received in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtBunker != null)
            {
                foreach (DataRow dr in dtBunker.Rows)
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", dr["Receipt"]?.ToString() ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Lube Oil & Hydraulic Oil";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "ME Crosshead Cons", r.LO_HO_Cons_MECC);
                AddKeyValueRow(ws, ref row, "ME Cylinder Cons", r.LO_HO_Cons_MECYL);
                AddKeyValueRow(ws, ref row, "AE Crosshead Cons", r.LO_HO_Cons_AECC);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil Cons", r.LO_HO_Cons_HYDR_Oil);
                AddKeyValueRow(ws, ref row, "ME Crosshead ROB", r.LO_HO_Cons_MECC_ROB);
                AddKeyValueRow(ws, ref row, "ME Cylinder ROB", r.LO_HO_Cons_MECYL_ROB);
                AddKeyValueRow(ws, ref row, "AE Crosshead ROB", r.LO_HO_Cons_AECC_ROB);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil ROB", r.LO_HO_Cons_HYDR_Oil_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Other ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Full";
                ws.Cell(row, 3).Value = "In Use";
                ws.Cell(row, 4).Value = "Empty";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "Oxygen (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_OXY_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_OXY_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_OXY_Empty);
                row++;
                ws.Cell(row, 1).Value = "Acetylene (Bottles)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.OT_ROB_ACYT_Full);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.OT_ROB_ACYT_InUse);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.OT_ROB_ACYT_Empty);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Aux. Engine";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Running Hrs No.1", r.AE_RungHrs_No1);
                AddKeyValueRow(ws, ref row, "Running Hrs No.2", r.AE_RungHrs_No2);
                AddKeyValueRow(ws, ref row, "Running Hrs No.3", r.AE_RungHrs_No3);
                AddKeyValueRow(ws, ref row, "Running Hrs No.4", r.AE_RungHrs_No4);
                AddKeyValueRow(ws, ref row, "Running Hrs Shaft Gen", r.AE_RungHrs_ShaftGen);
                AddKeyValueRow(ws, ref row, "Load No.1 (KW)", r.AE_Load_No1);
                AddKeyValueRow(ws, ref row, "Load No.2 (KW)", r.AE_Load_No2);
                AddKeyValueRow(ws, ref row, "Load No.3 (KW)", r.AE_Load_No3);
                AddKeyValueRow(ws, ref row, "Load No.4 (KW)", r.AE_Load_No4);
                AddKeyValueRow(ws, ref row, "Load Shaft Gen (KW)", r.AE_Load_ShaftGen);
                AddKeyValueRow(ws, ref row, "Extra Run Reason", r.AE_Extra_Run_Reason);
            }
            row++;

            ws.Cell(row, 1).Value = "Boiler's";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Boiler No.1 Running Hrs", r.BR_RungHrs_No1);
                AddKeyValueRow(ws, ref row, "Boiler No.2 Running Hrs", r.BR_RungHrs_No2);
                AddKeyValueRow(ws, ref row, "Boiler No.1 Extra Run Reason", r.BR_Extra_Run_Reason1);
                AddKeyValueRow(ws, ref row, "Boiler No.2 Extra Run Reason", r.BR_Extra_Run_Reason2);
            }
            ws.Columns().AdjustToContents();
        }

        private void AddConsumptionCargoSheet(XLWorkbook wb, DailyNoonReport r)
        {
            var ws = wb.Worksheets.Add("Cargo");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Consumption Report - Cargo";
            var rngHdr = ws.Range(row, 1, row, 4);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Slops ROB";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                ws.Cell(row, 1).Value = "";
                ws.Cell(row, 2).Value = "Oil";
                ws.Cell(row, 3).Value = "Water";
                ws.Cell(row, 4).Value = "Total";
                ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = "ROB (m3)";
                ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SLOPS_ROB_OXY_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SLOPS_ROB_OXY_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SLOPS_ROB_OXY_Total);
                row++;
            }
            row++;

            ws.Cell(row, 1).Value = "Ballast";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "ROB (MT)", r?.Ballast_ROB);
            row++;

            ws.Cell(row, 1).Value = "Fresh Water";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "FW Generated (MT)", r.FW_Generated);
                AddKeyValueRow(ws, ref row, "Consumption (MT)", r.FW_Consumption);
                AddKeyValueRow(ws, ref row, "ROB (MT)", r.FW_ROB);
            }
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Calls spCommonEditList for Bunker fuel rows on a fresh dedicated SqlConnection
        /// so it isn't affected by state left on the shared static ConnectionBulder.con.
        /// Falls back to the shared CommonMethods helper if the isolated call fails.
        /// </summary>
        private List<BukerFuelList> LoadBunkerFuelListIsolated(int reportId, int vesselId)
        {
            var list = new List<BukerFuelList>();
            try
            {
                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["SISContext"].ConnectionString;
                using (var con = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("spCommonEditList", con) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.AddWithValue("@Id", reportId);
                    cmd.Parameters.AddWithValue("@VesselId", vesselId.ToString());
                    cmd.Parameters.AddWithValue("@Action", "BunkerFReport");
                    con.Open();
                    using (var dt = new DataTable())
                    using (var adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(dt);
                        foreach (DataRow fr in dt.Rows)
                        {
                            var f = new BukerFuelList();
                            foreach (System.Reflection.PropertyInfo p in typeof(BukerFuelList).GetProperties())
                            {
                                if (!fr.Table.Columns.Contains(p.Name)) continue;
                                object v = fr[p.Name];
                                if (v == null || v == DBNull.Value) continue;
                                try
                                {
                                    Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                                    p.SetValue(f, Convert.ChangeType(v, t), null);
                                }
                                catch { }
                            }
                            list.Add(f);
                        }
                    }
                }
            }
            catch
            {
                list.Clear();
            }
            if (list.Count == 0)
            {
                list = CommonMethods.editBunkerFuelList(reportId, vesselId.ToString(), "BunkerFReport") ?? new List<BukerFuelList>();
            }
            return list;
        }

        /// <summary>
        /// Generates Bunker Report Excel for each vessel/date in tbls and saves to Files folder.
        /// Uses editBunkerRList logic (id, vesselid) to fetch all details.
        /// Structured with Bunker Details, Fuel Details sheets.
        /// </summary>
        private void SaveBunkerReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasId = tbls.Columns.Contains("Id");
            if (!hasVesselId || !hasId) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                int rowId = 0;
                if (row["Id"] != DBNull.Value && row["Id"] != null)
                    int.TryParse(row["Id"].ToString(), out rowId);
                if (rowId <= 0) continue;

                string key = vesselId + "_" + rowId;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var bunkerRList = CommonMethods.editBunkerRList(rowId, vesselId, "BunkerReport");
                var bunkerRBind = bunkerRList?.Where(x => x.Id == rowId).FirstOrDefault();
                if (bunkerRBind == null) continue;

                // Backfill from BunkerReport table directly in case spCommonEditList aliases column
                // names differently from the model property names (same family of bug as Departure / FreshWater).
                try
                {
                    using (SqlDataAdapter adpBf = new SqlDataAdapter(
                        "select PortName, PortName_others, Supplier, BargeName, Remarks, " +
                        "BargeAlongside, BunkerHoseConnected, CommencedBunkering, BunkeringCompleted, " +
                        "BunkerHosedisconnected, BargeCastOff, FirstName, LastName, LabAnalysisReport_Name " +
                        "from BunkerReport where Id=" + rowId, ConnectionBulder.con))
                    {
                        DataTable dtBf = new DataTable();
                        adpBf.Fill(dtBf);
                        if (dtBf.Rows.Count > 0)
                        {
                            DataRow br = dtBf.Rows[0];
                            if (br["PortName"] != DBNull.Value) bunkerRBind.PortName = br["PortName"].ToString();
                            if (br["PortName_others"] != DBNull.Value) bunkerRBind.PortName_others = br["PortName_others"].ToString();
                            if (br["Supplier"] != DBNull.Value) bunkerRBind.Supplier = br["Supplier"].ToString();
                            if (br["BargeName"] != DBNull.Value) bunkerRBind.BargeName = br["BargeName"].ToString();
                            if (br["Remarks"] != DBNull.Value) bunkerRBind.Remarks = br["Remarks"].ToString();
                            if (br["BargeAlongside"] != DBNull.Value) bunkerRBind.BargeAlongside = Convert.ToDateTime(br["BargeAlongside"]);
                            if (br["BunkerHoseConnected"] != DBNull.Value) bunkerRBind.BunkerHoseConnected = Convert.ToDateTime(br["BunkerHoseConnected"]);
                            if (br["CommencedBunkering"] != DBNull.Value) bunkerRBind.CommencedBunkering = Convert.ToDateTime(br["CommencedBunkering"]);
                            if (br["BunkeringCompleted"] != DBNull.Value) bunkerRBind.BunkeringCompleted = Convert.ToDateTime(br["BunkeringCompleted"]);
                            if (br["BunkerHosedisconnected"] != DBNull.Value) bunkerRBind.BunkerHosedisconnected = Convert.ToDateTime(br["BunkerHosedisconnected"]);
                            if (br["BargeCastOff"] != DBNull.Value) bunkerRBind.BargeCastOff = Convert.ToDateTime(br["BargeCastOff"]);
                            if (br["FirstName"] != DBNull.Value) bunkerRBind.FirstName = br["FirstName"].ToString();
                            if (br["LastName"] != DBNull.Value) bunkerRBind.LastName = br["LastName"].ToString();
                            if (br["LabAnalysisReport_Name"] != DBNull.Value) bunkerRBind.LabAnalysisReport_Name = br["LabAnalysisReport_Name"].ToString();
                        }
                    }
                }
                catch { }

                // Fetch fuel details with a FRESH dedicated SqlConnection rather than the
                // shared static ConnectionBulder.con. The shared connection is a process-wide
                // singleton; under concurrent use it can silently return truncated result
                // sets (observed: 5 rows instead of 7). A dedicated connection isolates this
                // call from any state left by other threads.
                var fuelList = LoadBunkerFuelListIsolated(rowId, vesselId);
                foreach (var item in fuelList)
                {
                    if (item.Fuel_type_Id == 5) item.Fuel_type = "VLSFO";
                    if (item.Fuel_type_Id == 2) item.Fuel_type = "MDO";
                }

                // Trigger date: prefer Modified_Date → fall back to Created_Date → fall back
                // to BargeAlongside. Matches the FreshWater fallback so both reports shoot
                // emails on the same logical date even when the domain date is empty.
                DateTime triggerDt = DateTime.MinValue;
                if (tbls.Columns.Contains("Modified_Date") && row["Modified_Date"] != DBNull.Value && row["Modified_Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Modified_Date"].ToString(), out d)) triggerDt = d;
                }
                if (triggerDt == DateTime.MinValue && tbls.Columns.Contains("Created_Date") && row["Created_Date"] != DBNull.Value && row["Created_Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Created_Date"].ToString(), out d)) triggerDt = d;
                }
                if (triggerDt == DateTime.MinValue) triggerDt = bunkerRBind.BargeAlongside;

                // Fetch main details from stored procedure
                DataTable dtMain = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", bunkerRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", triggerDt.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "BunkerReport");
                        cmd.Parameters.AddWithValue("@id", rowId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = triggerDt;
                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "BunkerReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + rowId + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddBunkerDetailsSheet(wb, bunkerRBind, dtMain);
                    AddBunkerFuelDetailsSheet(wb, bunkerRBind, fuelList);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddBunkerDetailsSheet(XLWorkbook wb, BunkerReport r, DataTable dtMain)
        {
            var ws = wb.Worksheets.Add("Bunker Details");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Bunker Report - Details";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string voyNo = r.voyagenumber;
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    var dr = dtMain.Rows[0];
                    if (string.IsNullOrWhiteSpace(voyNo) && dtMain.Columns.Contains("VoyageNumber"))
                        voyNo = dr["VoyageNumber"]?.ToString();
                }
                // Fallback: resolve display VoyageNumber from the Voyage table by VoyageId so the
                // "Voy No." cell shows the user-facing number (e.g. 61) rather than the FK id.
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
                // Resolve "Others" port → custom name from PortName_others (matches FreshWater pattern).
                string portDisplay = r.PortName?.Trim();
                string othersPort = r.PortName_others?.Trim();
                bool isOthers = !string.IsNullOrEmpty(portDisplay) && portDisplay.Equals("Others", StringComparison.OrdinalIgnoreCase);
                if ((isOthers || string.IsNullOrEmpty(portDisplay)) && !string.IsNullOrEmpty(othersPort)) portDisplay = othersPort;
                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Port Name", portDisplay ?? "");
                AddKeyValueRow(ws, ref row, "Supplier", r.Supplier ?? "");
                AddKeyValueRow(ws, ref row, "Barge Name", r.BargeName ?? "");
                AddKeyValueRow(ws, ref row, "Master Name", r.FirstName ?? "");
                AddKeyValueRow(ws, ref row, "Chief Engineer", r.LastName ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Bunker Timing";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Barge Alongside", r.BargeAlongside != DateTime.MinValue ? r.BargeAlongside.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Bunker Hose Connected", r.BunkerHoseConnected != DateTime.MinValue ? r.BunkerHoseConnected.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Commenced Bunkering", r.CommencedBunkering != DateTime.MinValue ? r.CommencedBunkering.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Bunkering Completed", r.BunkeringCompleted != DateTime.MinValue ? r.BunkeringCompleted.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Bunker Hose disconnected", r.BunkerHosedisconnected != DateTime.MinValue ? r.BunkerHosedisconnected.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Barge Cast Off", r.BargeCastOff != DateTime.MinValue ? r.BargeCastOff.ToString(ExcelDateTimeFormat) : "");
            }
            row++;

            ws.Cell(row, 1).Value = "Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            row++;

            ws.Cell(row, 1).Value = "BDN Report";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "BDN Report", r?.LabAnalysisReport_Name ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddBunkerFuelDetailsSheet(XLWorkbook wb, BunkerReport r, List<BukerFuelList> fuelList)
        {
            var ws = wb.Worksheets.Add("Fuel Details");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Bunker Report - Fuel Details";
            var rngHdr = ws.Range(row, 1, row, 5);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            ws.Cell(row, 1).Value = "Fuel Type";
            ApplyLightGrayTitle(ws, row, 1, 5);
            row++;

            // Header row
            ws.Cell(row, 1).Value = "Fuel Type";
            ws.Cell(row, 2).Value = "Quantity Received(MT)";
            ws.Cell(row, 3).Value = "BDN Number";
            ws.Cell(row, 4).Value = "Fuel Density(Kg/m3)";
            ws.Cell(row, 5).Value = "Sulphur content(%)";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            row++;

            if (fuelList != null && fuelList.Count > 0)
            {
                foreach (var fuel in fuelList)
                {
                    ws.Cell(row, 1).Value = fuel.Fuel_type ?? "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), fuel.BDN, "0.000");
                    ws.Cell(row, 3).Value = fuel.BDN_Number ?? "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), fuel.Fuel_Density, "0.000");
                    SetCellValueWithDecimalFormat(ws.Cell(row, 5), fuel.Sulphur_content, "0.000");
                    row++;
                }
            }
            else
            {
                ws.Cell(row, 1).Value = "No fuel data available";
                ws.Range(row, 1, row, 5).Merge();
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Fresh Water Report Excel for each vessel/row in tbls and saves to Files folder.
        /// Uses editFreshWaterRList logic (id, vesselid) to fetch all details.
        /// </summary>
        private void SaveFreshWaterReportExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("VesselId");
            bool hasId = tbls.Columns.Contains("Id");
            if (!hasVesselId || !hasId) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                if (row["VesselId"] != DBNull.Value && row["VesselId"] != null)
                    int.TryParse(row["VesselId"].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                int rowId = 0;
                if (row["Id"] != DBNull.Value && row["Id"] != null)
                    int.TryParse(row["Id"].ToString(), out rowId);
                if (rowId <= 0) continue;

                string key = vesselId + "_" + rowId;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                var fwRList = CommonMethods.editFreshWaterRList(rowId, vesselId, "FreshWaterReport");
                var fwRBind = fwRList?.Where(x => x.Id == rowId).FirstOrDefault();
                if (fwRBind == null) continue;

                // Backfill from FreshWaterReport directly — editFreshWaterRList (spCommonEditList)
                // may not surface PortName_others, and we need it for the "Others" port resolve.
                try
                {
                    using (var adp = new SqlDataAdapter(
                        "select PortName, PortName_others from FreshWaterReport where Id=" + rowId, ConnectionBulder.con))
                    {
                        var dtBackfill = new DataTable();
                        adp.Fill(dtBackfill);
                        if (dtBackfill.Rows.Count > 0)
                        {
                            var br = dtBackfill.Rows[0];
                            if (br["PortName"] != DBNull.Value) fwRBind.PortName = br["PortName"].ToString();
                            if (br["PortName_others"] != DBNull.Value) fwRBind.PortName_others = br["PortName_others"].ToString();
                        }
                    }
                }
                catch { }

                // Trigger date: prefer ModifiedDate (last edit) → fall back to CreatedDate
                // (when report has never been edited) → fall back to Received_Date (the
                // original domain date) → finally to import-row Received_Date if domain is
                // empty. Matches the Bunker fallback so both reports shoot emails on the
                // same logical date.
                DateTime rptDt = DateTime.MinValue;
                if (tbls.Columns.Contains("Modified_Date") && row["Modified_Date"] != DBNull.Value && row["Modified_Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Modified_Date"].ToString(), out d)) rptDt = d;
                }
                if (rptDt == DateTime.MinValue && tbls.Columns.Contains("Created_Date") && row["Created_Date"] != DBNull.Value && row["Created_Date"] != null)
                {
                    DateTime d;
                    if (DateTime.TryParse(row["Created_Date"].ToString(), out d)) rptDt = d;
                }
                if (rptDt == DateTime.MinValue) rptDt = fwRBind.Received_Date;
                if (rptDt == DateTime.MinValue)
                {
                    // final fallback to Received_Date from import row
                    if (tbls.Columns.Contains("Received_Date") && row["Received_Date"] != DBNull.Value && row["Received_Date"] != null)
                    {
                        DateTime d;
                        if (DateTime.TryParse(row["Received_Date"].ToString(), out d)) rptDt = d;
                    }
                }
                if (rptDt == DateTime.MinValue) continue;

                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "FreshWaterReport";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + rowId + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddFreshWaterDetailsSheet(wb, fwRBind);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        /// <summary>
        /// Reconstructs attachment files from a BunkerReport_Files or FreshWaterReport_Files
        /// import sheet and writes them to <paramref name="targetFolderPath"/>. Each row in
        /// the sheet is a chunk: groups rows by FileName, orders by PartIndex, concatenates
        /// base64 chunks, decodes, and writes the file to disk. Logs each step to
        /// ReportExportLog.txt so we can diagnose why files aren't appearing.
        /// Also tries variant column names (FileName / File_Name / fileName) since column
        /// spelling has differed across exports.
        /// </summary>
        private void SaveAttachmentFilesFromSheet(DataTable tbls, string targetFolderPath, string logSource)
        {
            if (tbls == null || tbls.Rows.Count == 0)
            {
                LogReportExport(logSource, 0, "", "", "Skipped-EmptySheet");
                return;
            }

            // Try variant column names. DataTable column lookup is case-insensitive by default
            // so "FileName" matches "filename" — but underscored variants (File_Name) need
            // explicit candidates.
            string fileNameCol = FindFirstExistingColumn(tbls, "FileName", "File_Name", "Name");
            string dataCol     = FindFirstExistingColumn(tbls, "FileData", "File_Data", "Data", "Base64");
            string partCol     = FindFirstExistingColumn(tbls, "PartIndex", "Part_Index", "Part", "ChunkIndex", "Index");

            // Log what columns we found vs what's actually in the sheet so we can debug
            // mismatched export schemas.
            string colsActual = string.Join(",", tbls.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            LogReportExport(logSource, 0, "", "", $"Enter rows={tbls.Rows.Count} cols=[{colsActual}] resolved fileName={fileNameCol ?? "?"} data={dataCol ?? "?"} part={partCol ?? "?"}");

            if (fileNameCol == null || dataCol == null)
            {
                LogReportExport(logSource, 0, "", "", "Skipped-MissingRequiredColumn");
                return;
            }

            if (!Directory.Exists(targetFolderPath))
            {
                try { Directory.CreateDirectory(targetFolderPath); }
                catch (Exception exDir)
                {
                    LogReportExport(logSource, 0, "", targetFolderPath, "ERROR-CreateDir: " + exDir.Message);
                    return;
                }
            }

            // Group by FileName so multi-part files reassemble in order. Use a SortedDictionary
            // keyed by PartIndex so chunks concatenate in the correct sequence regardless of the
            // order the rows arrived in the sheet.
            var groups = new Dictionary<string, SortedDictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in tbls.Rows)
            {
                string fileName = row[fileNameCol]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(fileName)) continue;
                if (row[dataCol] == DBNull.Value || row[dataCol] == null) continue;
                string chunk = row[dataCol].ToString();
                if (string.IsNullOrEmpty(chunk)) continue;

                int partIndex = 0;
                if (partCol != null && row[partCol] != DBNull.Value && row[partCol] != null)
                    int.TryParse(row[partCol].ToString(), out partIndex);

                if (!groups.ContainsKey(fileName)) groups[fileName] = new SortedDictionary<int, string>();
                groups[fileName][partIndex] = chunk;
            }

            LogReportExport(logSource, 0, "", "", $"Grouped fileCount={groups.Count}");

            foreach (var kvp in groups)
            {
                try
                {
                    string fullBase64 = string.Concat(kvp.Value.Values);
                    byte[] fileBytes = Convert.FromBase64String(fullBase64);
                    string fullPath = Path.Combine(targetFolderPath, kvp.Key);
                    File.WriteAllBytes(fullPath, fileBytes);
                    LogReportExport(logSource, 0, "", kvp.Key, $"Saved bytes={fileBytes.Length} parts={kvp.Value.Count}");
                }
                catch (Exception exSave)
                {
                    LogReportExport(logSource, 0, "", kvp.Key, "ERROR-Save: " + exSave.Message);
                }
            }
        }

        /// <summary>
        /// Reads chunked base64 attachments directly from a ClosedXML worksheet (rather than
        /// going through the DataTable importer in ImportData, which has been observed to
        /// truncate very long cell strings). Locates FileName / PartIndex / FileData by header
        /// row, groups by FileName, sorts chunks by PartIndex, concatenates, base64-decodes,
        /// and writes to <paramref name="targetFolderPath"/>. Logs every step.
        /// </summary>
        private void ExtractAttachmentSheetDirect(IXLWorksheet sheet, string targetFolderPath, string logSource)
        {
            if (sheet == null) { LogReportExport(logSource, 0, "", "", "Skipped-NullSheet"); return; }

            int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
            int lastCol = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            if (lastRow < 2 || lastCol < 2)
            {
                LogReportExport(logSource, 0, "", "", $"Skipped-NoData rows={lastRow} cols={lastCol}");
                return;
            }

            // Map header row → column index (1-based for ClosedXML).
            int colFileName = 0, colPartIndex = 0, colFileData = 0;
            var headerNames = new List<string>();
            for (int c = 1; c <= lastCol; c++)
            {
                string h = sheet.Cell(1, c).GetString()?.Trim() ?? "";
                headerNames.Add(h);
                string hLower = h.ToLowerInvariant().Replace("_", "");
                if (colFileName == 0 && (hLower == "filename" || hLower == "name")) colFileName = c;
                else if (colPartIndex == 0 && (hLower == "partindex" || hLower == "part" || hLower == "chunkindex" || hLower == "index")) colPartIndex = c;
                else if (colFileData == 0 && (hLower == "filedata" || hLower == "data" || hLower == "base64")) colFileData = c;
            }

            LogReportExport(logSource, 0, "", "", $"Direct-Enter rows={lastRow - 1} cols=[{string.Join(",", headerNames)}] fileName@{colFileName} part@{colPartIndex} data@{colFileData}");

            if (colFileName == 0 || colFileData == 0)
            {
                LogReportExport(logSource, 0, "", "", "Skipped-MissingRequiredColumn");
                return;
            }

            if (!Directory.Exists(targetFolderPath))
            {
                try { Directory.CreateDirectory(targetFolderPath); }
                catch (Exception exDir)
                {
                    LogReportExport(logSource, 0, "", targetFolderPath, "ERROR-CreateDir: " + exDir.Message);
                    return;
                }
            }

            // Group rows by FileName, with chunks sorted by PartIndex. Use SortedDictionary
            // so chunks concatenate in the correct order regardless of row order.
            var groups = new Dictionary<string, SortedDictionary<int, string>>(StringComparer.OrdinalIgnoreCase);
            for (int r = 2; r <= lastRow; r++)
            {
                string fileName = sheet.Cell(r, colFileName).GetString()?.Trim();
                if (string.IsNullOrEmpty(fileName)) continue;

                string chunk = sheet.Cell(r, colFileData).GetString();
                if (string.IsNullOrEmpty(chunk)) continue;

                int partIndex = 0;
                if (colPartIndex > 0)
                {
                    string p = sheet.Cell(r, colPartIndex).GetString();
                    int.TryParse(p, out partIndex);
                }

                if (!groups.ContainsKey(fileName)) groups[fileName] = new SortedDictionary<int, string>();
                groups[fileName][partIndex] = chunk;
            }

            LogReportExport(logSource, 0, "", "", $"Direct-Grouped fileCount={groups.Count}");

            foreach (var kvp in groups)
            {
                try
                {
                    // Trim whitespace from each chunk before concatenating — exports sometimes
                    // include line breaks every N chars that break Convert.FromBase64String.
                    var sb = new StringBuilder();
                    foreach (var part in kvp.Value.Values)
                        sb.Append(part.Replace("\r", "").Replace("\n", "").Replace(" ", ""));
                    byte[] fileBytes = Convert.FromBase64String(sb.ToString());
                    string fullPath = Path.Combine(targetFolderPath, kvp.Key);
                    File.WriteAllBytes(fullPath, fileBytes);
                    LogReportExport(logSource, 0, "", kvp.Key, $"Direct-Saved bytes={fileBytes.Length} parts={kvp.Value.Count}");
                }
                catch (Exception exSave)
                {
                    LogReportExport(logSource, 0, "", kvp.Key, "ERROR-Save: " + exSave.Message);
                }
            }
        }

        /// <summary>Returns the first column name in <paramref name="candidates"/> that exists
        /// in the table (case-insensitive), or null if none match.</summary>
        private static string FindFirstExistingColumn(DataTable tbl, params string[] candidates)
        {
            if (tbl == null || candidates == null) return null;
            foreach (var c in candidates)
            {
                if (string.IsNullOrEmpty(c)) continue;
                if (tbl.Columns.Contains(c)) return c;
            }
            return null;
        }

        private void AddFreshWaterDetailsSheet(XLWorkbook wb, FreshWaterReport r)
        {
            var ws = wb.Worksheets.Add("Fresh Water Details");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Fresh Water Report - Details";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            if (r != null)
            {
                string port = r.PortName?.Trim();
                string others = r.PortName_others?.Trim();
                bool isOthers = !string.IsNullOrEmpty(port) && port.Equals("Others", StringComparison.OrdinalIgnoreCase);
                string portDisplay = ((isOthers || string.IsNullOrEmpty(port)) && !string.IsNullOrEmpty(others)) ? others : (port ?? "");

                AddKeyValueRow(ws, ref row, "Port Name", portDisplay);
                AddKeyValueRow(ws, ref row, "Facility Name", r.Facility_Name ?? "");
                AddKeyValueRow(ws, ref row, "Received Date", r.Received_Date != DateTime.MinValue ? r.Received_Date.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Vendor Details", r.VendorDetails ?? "");
            }
            row++;

            ws.Cell(row, 1).Value = "Meter Reading & Quantity";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRowWithFormat(ws, ref row, "Initial Meter Reading Supply(MT)", r.Intial_Meter_Reading_MT_supplied, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Final Meter Reading(MT)", r.Final_Meter_Reading_MT, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "Difference Meter Reading(MT)", r.Difference_in_Meter_Reading_MT, "0.000");
                AddKeyValueRowWithFormat(ws, ref row, "QTY Received(MT)", r.QTY_supplied_MT, "0.000");
            }
            row++;

            ws.Cell(row, 1).Value = "Attached Document";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Attachment", r?.File_Name ?? "");
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Generates Noon Report Allow Excel for each vessel/row in tbls and saves to Files folder.
        /// Uses GetNoonReportallowList logic to fetch all details.
        /// </summary>
        private void SaveNoonReportAllowExcelToFiles(DataTable tbls)
        {
            if (tbls == null || tbls.Rows.Count == 0) return;
            bool hasVesselId = tbls.Columns.Contains("Vessel_Id");
            bool hasId = tbls.Columns.Contains("Id");
            if (!hasVesselId && !tbls.Columns.Contains("VesselId")) return;
            if (!hasId) return;

            string filesPath = Server.MapPath("~/Files/");
            if (!Directory.Exists(filesPath)) Directory.CreateDirectory(filesPath);

            var processed = new HashSet<string>();
            foreach (DataRow row in tbls.Rows)
            {
                int vesselId = 0;
                string vesselCol = hasVesselId ? "Vessel_Id" : "VesselId";
                if (row[vesselCol] != DBNull.Value && row[vesselCol] != null)
                    int.TryParse(row[vesselCol].ToString(), out vesselId);
                if (vesselId <= 0) continue;

                int rowId = 0;
                if (row["Id"] != DBNull.Value && row["Id"] != null)
                    int.TryParse(row["Id"].ToString(), out rowId);
                if (rowId <= 0) continue;

                string key = vesselId + "_" + rowId;
                if (processed.Contains(key)) continue;
                processed.Add(key);

                string noonDate = "";
                if (tbls.Columns.Contains("Noon_date") && row["Noon_date"] != DBNull.Value && row["Noon_date"] != null)
                    noonDate = row["Noon_date"].ToString();
                else if (tbls.Columns.Contains("allow_Noondate") && row["allow_Noondate"] != DBNull.Value && row["allow_Noondate"] != null)
                    noonDate = row["allow_Noondate"].ToString();

                string userName = "";
                if (tbls.Columns.Contains("userName") && row["userName"] != DBNull.Value && row["userName"] != null)
                    userName = row["userName"].ToString();

                string vesselName = "";
                try
                {
                    vesselName = CommonClass.GetVesselNamesByImoNo(vesselId.ToString());
                }
                catch { }
                if (string.IsNullOrEmpty(vesselName)) vesselName = "Vessel " + vesselId;

                DateTime rptDt = DateTime.Now;
                if (!string.IsNullOrEmpty(noonDate))
                {
                    DateTime d;
                    if (DateTime.TryParse(noonDate, out d)) rptDt = d;
                }

                string datePart = rptDt.ToString("dd") + "_" + rptDt.ToString("MM") + "_" + rptDt.ToString("yyyy");
                string reportType = "NoonReportAllow";

                if (IsReportAlreadySaved(filesPath, reportType, vesselId, datePart))
                {
                    LogReportExport(reportType, vesselId, datePart, null, "Skipped-AlreadySaved");
                    continue;
                }

                string uniqueId = DateTime.Now.ToString("HHmmss");
                string fileName = reportType + "_" + vesselId + "_" + datePart + "_R" + rowId + "_" + uniqueId + ".xlsx";
                string fullPath = Path.Combine(filesPath, fileName);
                using (XLWorkbook wb = new XLWorkbook())
                {
                    AddNoonReportAllowDetailsSheet(wb, rowId, vesselId, vesselName, noonDate, userName);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
                TrackLatestModifiedRowForGroup(reportType, vesselId, ReadRowModifiedDate(row), fullPath);
            }
        }

        private void AddNoonReportAllowDetailsSheet(XLWorkbook wb, int id, int vesselId, string vesselName, string noonDate, string userName)
        {
            var ws = wb.Worksheets.Add("Noon Report Allow");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Noon Report Allow - Details";
            var rngHdr = ws.Range(row, 1, row, 3);
            rngHdr.Merge();
            rngHdr.Style.Font.Bold = true;
            rngHdr.Style.Font.FontSize = 16;
            rngHdr.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            AddKeyValueRow(ws, ref row, "Vessel Name", vesselName ?? "");
            AddKeyValueRow(ws, ref row, "Vessel ID (IMO No.)", vesselId.ToString());
            row++;

            ws.Cell(row, 1).Value = "Bypass Date Details";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;

            string formattedDate = "";
            if (!string.IsNullOrEmpty(noonDate))
            {
                DateTime d;
                if (DateTime.TryParse(noonDate, out d))
                    formattedDate = d.ToString(ExcelDateFormat);
                else
                    formattedDate = noonDate;
            }

            AddKeyValueRow(ws, ref row, "Noon Report Allow Date", formattedDate);
            AddKeyValueRow(ws, ref row, "User Name", userName ?? "");
            AddKeyValueRow(ws, ref row, "Report ID", id.ToString());
            ws.Columns().AdjustToContents();
        }

        private void AutoDelete()
        {
            try
            {
                string locationArchieve = Server.MapPath("~/Archive/");
                string[] files = Directory.GetFiles(locationArchieve);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
                        fi.Delete();
                }
            }
            catch { }
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




    }

    internal class ReceivedEMails
    {
        public string EmailCode { get; set; }
        public string EmailSubject { get; set; }
        public string EmailSender { get; set; }
        public string EmailContent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    #region IMap4 Code
    //public class MailRepository
    //{
    //    private Imap4Client _client = null;

    //    public MailRepository(string mailServer, int port, bool ssl, string login, string password)
    //    {
    //        if (ssl)
    //            Client.ConnectSsl(mailServer, port);
    //        else
    //            Client.Connect(mailServer, port);
    //        Client.Login(login, password);
    //    }

    //    public IEnumerable<Message> GetAllMails(string mailBox)
    //    {
    //        return GetMails(mailBox, "ALL").Cast<Message>();
    //    }

    //    public IEnumerable<Message> GetUnreadMails(string mailBox)
    //    {
    //        return GetMails(mailBox, "UNSEEN").Cast<Message>();
    //    }

    //    protected Imap4Client Client
    //    {
    //        get
    //        {
    //            if (_client == null)
    //                _client = new Imap4Client();
    //            return _client;
    //        }
    //    }

    //    private MessageCollection GetMails(string mailBox, string searchPhrase)
    //    {
    //        Mailbox mails = Client.SelectMailbox(mailBox);
    //        MessageCollection messages = mails.SearchParse(searchPhrase);
    //        return messages;
    //    }
    //}
    //protected void TestMail()
    //{
    //    MailRepository rep = new MailRepository("imap.gmail.com", 993, true, "ws1.49web@gmail.com", "Y4499@web");
    //    foreach (Message email in rep.GetUnreadMails("Inbox"))
    //    {
    //      Response.Write(string.Format("{0}: {1}{2}", email.From, email.Subject, email.BodyHtml.Text));
    //      if (email.Attachments.Count > 0)
    //        {
    //            foreach (MimePart attachment in email.Attachments)
    //            {
    //                Response.Write(string.Format("Attachment: {0}{1}", attachment.ContentName, attachment.ContentType.MimeType));
    //            }
    //        }
    //    }
    //}

    //[Serializable]
    //public class Email
    //{
    //    public Email()
    //    {
    //        this.Attachments = new List<Attachment>();
    //    }
    //    public int MessageNumber { get; set; }
    //    public string From { get; set; }
    //    public string Subject { get; set; }
    //    public string Body { get; set; }
    //    public DateTime DateSent { get; set; }
    //    public List<Attachment> Attachments { get; set; }
    //}
    //[Serializable]
    //public class Attachment
    //{
    //    public string FileName { get; set; }
    //    public string ContentType { get; set; }
    //    public byte[] Content { get; set; }
    //}


    #endregion
}
