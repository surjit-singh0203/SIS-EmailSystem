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

                        string filterPart = $"$filter=receivedDateTime ge {startDatePartString} and receivedDateTime lt {endDatePartString}";

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

                        MoveFilesToArchive(folderPath, foldermovePath);

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


        private void MoveFilesToArchive(string inboxPath, string archivePath)
        {

            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
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
                        // string Modified_Date = (tbls.Rows[i]["Modified_Date"]).ToString();

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
                            // adapter.SelectCommand.Parameters.AddWithValue("@Modified_Date", Modified_Date);

                            using (DataTable dataTable = new DataTable())
                            {
                                adapter.Fill(dataTable);

                            }
                        }
                    }
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



                    //// Generate and save Bunker Report Excel to Files folder (after Update)
                    if (sheetName == "BunkerReport")
                    {
                        try
                        {
                            SaveBunkerReportExcelToFiles(tbls);
                        }
                        catch (Exception exBunker) { }
                    }

                    // Generate and save Fresh Water Report Excel to Files folder (after Update)
                    if (sheetName == "FreshWaterReport")
                    {
                        try
                        {
                            SaveFreshWaterReportExcelToFiles(tbls);
                        }
                        catch (Exception exFreshWater) { }
                    }


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
                AddKeyValueRow(ws, ref row, "MECC Consumption (Ltrs)", r.LO_HO_Cons_MECC);
                AddKeyValueRow(ws, ref row, "MECC ROB (Ltrs)", r.LO_HO_Cons_MECC_ROB);
                AddKeyValueRow(ws, ref row, "MECYL Consumption (Ltrs)", r.LO_HO_Cons_MECYL);
                AddKeyValueRow(ws, ref row, "MECYL ROB (Ltrs)", r.LO_HO_Cons_MECYL_ROB);
                AddKeyValueRow(ws, ref row, "AECC Consumption (Ltrs)", r.LO_HO_Cons_AECC);
                AddKeyValueRow(ws, ref row, "AECC ROB (Ltrs)", r.LO_HO_Cons_AECC_ROB);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil Consumption (Ltrs)", r.LO_HO_Cons_HYDR_Oil);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil ROB (Ltrs)", r.LO_HO_Cons_HYDR_Oil_ROB);
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
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].Ullage);
                row++;
                ws.Cell(row, 1).Value = "MT Qty";
                for (int c = 0; c < cTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), cTanks[c].Qty_MT);
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
                for (int c = 0; c < bTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), bTanks[c].Sounding);
                row++;
                ws.Cell(row, 1).Value = "Cubic Vol";
                for (int c = 0; c < bTanks.Count; c++)
                    SetCellValueWithDecimalFormat(ws.Cell(row, c + 2), bTanks[c].Qty_Vol);
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private const string ExcelDecimalFormat = "0.000";

        private void AddKeyValueRow(IXLWorksheet ws, ref int row, string label, object value)
        {
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            SetCellValueWithDecimalFormat(ws.Cell(row, 2), value, ExcelDecimalFormat);
            row++;
        }

        /// <summary>
        /// Sets cell value; for numeric types applies decimal format only when value has fractional part (e.g. 6.75 -> 6.750).
        /// IDs and whole numbers (e.g. Voy No. 9413779) are not forced to decimal format.
        /// </summary>
        private void SetCellValueWithDecimalFormat(IXLCell cell, object value, string numberFormat = "0.000")
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
                // Only apply decimal format when value has fractional part; IDs/whole numbers stay as-is
                if (d != Math.Truncate(d))
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

        private string FormatDec(decimal d) { return d.ToString("0.000"); }
        private string FormatCargoVal(object val)
        {
            if (val == null || val == DBNull.Value) return "0.00";
            decimal d;
            return decimal.TryParse(val.ToString(), out d) ? d.ToString("0.00") : "0.00";
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
                // Resolve Leg from VoyageLeg scoped by VoyageId+VesselId, trying DepLegPortId then NextLegPortId.
                if (string.IsNullOrEmpty(legText))
                {
                    try
                    {
                        foreach (int legId in new[] { r.DepLegPortId, r.NextLegPortId })
                        {
                            if (legId <= 0 || !string.IsNullOrEmpty(legText)) continue;
                            using (SqlDataAdapter adp = new SqlDataAdapter(
                                "select LegPort_A + ' to ' + LegPort_B as Leg from VoyageLeg where Id=" + legId + " and VoyageId=" + r.VoyageId + " and VesselId=" + r.VesselId, ConnectionBulder.con))
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
                AddKeyValueRow(ws, ref row, "Departure Port", r.DeparturePort ?? "");
                AddKeyValueRow(ws, ref row, "Next Port", r.NextPort ?? "");
                AddKeyValueRow(ws, ref row, "Report Date", r.ReportDate != null ? Convert.ToDateTime(r.ReportDate).ToString(ExcelDateFormat) : "");
                AddKeyValueRow(ws, ref row, "ETA", r.ETA != null ? Convert.ToDateTime(r.ETA).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Manoeuvring & SBE/RFA";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Manoeuvring Hrs", r.Manoeuvring_Hrs);
                AddKeyValueRow(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance);
                AddKeyValueRow(ws, ref row, "SBE Date & Time", r.SBE_DateT != null ? Convert.ToDateTime(r.SBE_DateT).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "RFA Date & Time", r.RFA_DateT != null ? Convert.ToDateTime(r.RFA_DateT).ToString(ExcelDateTimeFormat) : "");
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

            ws.Cell(row, 1).Value = "Departure Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            ws.Columns().AdjustToContents();
        }

        private void AddDepartureEngineSheet(XLWorkbook wb, DepartureReport r, DataTable dtFuelCons, DataTable dtFuelROB, DataTable dtBunker)
        {
            var ws = wb.Worksheets.Add("Engine");
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            int row = 1;
            ws.Cell(row, 1).Value = "Departure Report - Engine";
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
            }
            row++;

            ws.Cell(row, 1).Value = "Lube Oil & Hydraulic Oil";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "MECC Consumption (Ltrs)", r.LO_HO_Cons_MECC);
                AddKeyValueRow(ws, ref row, "MECC ROB (Ltrs)", r.LO_HO_Cons_MECC_ROB);
                AddKeyValueRow(ws, ref row, "MECYL Consumption (Ltrs)", r.LO_HO_Cons_MECYL);
                AddKeyValueRow(ws, ref row, "MECYL ROB (Ltrs)", r.LO_HO_Cons_MECYL_ROB);
                AddKeyValueRow(ws, ref row, "AECC Consumption (Ltrs)", r.LO_HO_Cons_AECC);
                AddKeyValueRow(ws, ref row, "AECC ROB (Ltrs)", r.LO_HO_Cons_AECC_ROB);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil Consumption (Ltrs)", r.LO_HO_Cons_HYDR_Oil);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil ROB (Ltrs)", r.LO_HO_Cons_HYDR_Oil_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT (SBE/RFA)";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string sbe = dr.Table.Columns.Contains("SBE") ? dr["SBE"]?.ToString() : "";
                    string fwe = dr.Table.Columns.Contains("RFA") ? dr["RFA"]?.ToString() : "";
                    string robVal = string.IsNullOrEmpty(sbe) && string.IsNullOrEmpty(fwe) ? "" : (sbe ?? "-") + " / " + (fwe ?? "-");
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal);
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

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                var vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
                var mdo = GetFuelConsByType(dtFuelCons, "MDO");
                AddKeyValueRow(ws, ref row, "VLSFO (Total)", FormatDec(vlsfo));
                AddKeyValueRow(ws, ref row, "MDO (Total)", FormatDec(mdo));
            }
            ws.Columns().AdjustToContents();
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

            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "B/L QTY";
            ws.Cell(row, 3).Value = "Load Portal Actual";
            ws.Cell(row, 4).Value = "Cargo Temp.";
            ws.Range(row, 1, row, 4).Style.Font.Bold = true;
            row++;
            if (dtDRCargo != null && dtDRCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtDRCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    ws.Cell(row, 2).Value = FormatCargoVal(dr.Table.Columns.Contains("BL_Qty") ? dr["BL_Qty"] : null);
                    ws.Cell(row, 3).Value = FormatCargoVal(dr.Table.Columns.Contains("LoadPortalActual") ? dr["LoadPortalActual"] : null);
                    ws.Cell(row, 4).Value = FormatCargoVal(dr.Table.Columns.Contains("Cargo_Temp") ? dr["Cargo_Temp"] : null);
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
        /// Parses sync Excel names: ReportType_vesselId_dd_MM_yyyy_HHmmss.xlsx or ..._dd_MM_yyyy_R{rowId}_HHmmss.xlsx (R+id matches Excel row for HTML email).
        /// Uses strict date validation when possible; falls back to legacy segment rules so older filenames still queue for email.
        /// </summary>
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

            var toSend = new List<string>();
            foreach (string path in savedFilePaths)
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                string fn = Path.GetFileName(path);
                if (!TryParseSyncReportExportFileName(fn, out string reportType, out int vesselId, out string datePart, out _))
                    continue;
                vesselID = vesselId;
                if (IsReportEmailAlreadySent(reportType, vesselId, datePart)) continue;
                toSend.Add(path);
            }
            if (toSend.Count == 0) return;

            string from = ConfigurationManager.AppSettings["mailmsg"] ?? "noreply@mooringplan.com";
            string smtpHost = ConfigurationManager.AppSettings["smtpclnt"] ?? "smtp.zeptomail.in";
            string smtpUser = ConfigurationManager.AppSettings["ntwrkcrd"] ?? "";
            string smtpPwd = ConfigurationManager.AppSettings["pwd"] ?? "";
            string fallbackTo = ConfigurationManager.AppSettings["ImportNotificationTo"] ?? "";

            var emailListnew = CommonClass.GetSyncEmailVesselsReport(vesselID);

            foreach (var item in emailListnew)
            {
                if (item.LastSent.Date < DateTime.Now.Date)
                {
                    foreach (string path in toSend)
                    {
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

                                using (var smtp = new SmtpClient(smtpHost))
                                {
                                    smtp.Port = 587;
                                    smtp.EnableSsl = true;
                                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPwd);
                                    smtp.Send(msg);
                                }
                                //Update function
                                LogReportEmailSent(reportType, vesselId, datePart, fn);
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                            /* log if needed */
                        }

                        //continue
                    }

                    CommonClass.UpdateSyncEmailVesselsReportLastSent(item.Id, DateTime.Now.Date);

                }
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
        /// Checks if a report (ReportType_VesselId_DatePart) has already been emailed (per ReportEmailLog.txt).
        /// </summary>
        private bool IsReportEmailAlreadySent(string reportType, int vesselId, string datePart)
        {
            try
            {
                string logPath = Path.Combine(Server.MapPath("~/Files/"), ReportEmailLogFile);
                if (!File.Exists(logPath)) return false;
                string vIdStr = vesselId.ToString();
                foreach (var line in File.ReadAllLines(logPath))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 6 && string.Equals(parts[1], reportType, StringComparison.OrdinalIgnoreCase)
                        && parts[2] == vIdStr && parts[3] == datePart && parts[5].Trim() == "EmailSent")
                        return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Logs that a report email was sent to ReportEmailLog.txt. Format: DateTime|ReportType|VesselId|DatePart|FileName|EmailSent
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
                AddKeyValueRow(ws, ref row, "Latitude", r.Latitude ?? "");
                AddKeyValueRow(ws, ref row, "Longitude", r.Longitude ?? "");
                AddKeyValueRow(ws, ref row, "Place", r.Place ?? "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "NOR", r.NOR != null ? Convert.ToDateTime(r.NOR).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "EOSP", r.EOSP != null ? Convert.ToDateTime(r.EOSP).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "ETB", r.ETB != null ? Convert.ToDateTime(r.ETB).ToString(ExcelDateTimeFormat) : "");
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
                AddKeyValueRow(ws, ref row, "Drop Anchor Date & Time", r.Anchor_DateT != null ? Convert.ToDateTime(r.Anchor_DateT).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Anchoring Position - Latitude", r.AnchorPos_Latitude ?? "");
                AddKeyValueRow(ws, ref row, "Anchoring Position - Longitude", r.AnchorPos_Longitude ?? "");
                AddKeyValueRow(ws, ref row, "FWE Date & Time", r.AnchorFWE_DateT != null ? Convert.ToDateTime(r.AnchorFWE_DateT).ToString(ExcelDateTimeFormat) : "");
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
                AddKeyValueRow(ws, ref row, "Stmg Time Noon to Noon(Hrs)", r.StmgTime);
                AddKeyValueRow(ws, ref row, "Manoeuvring Hrs", r.Manoeuvring_Hrs);
                AddKeyValueRow(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance);
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
            }
            row++;

            ws.Cell(row, 1).Value = "Lube Oil & Hydraulic Oil";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "MECC Consumption (Ltrs)", r.LO_HO_Cons_MECC);
                AddKeyValueRow(ws, ref row, "MECC ROB (Ltrs)", r.LO_HO_Cons_MECC_ROB);
                AddKeyValueRow(ws, ref row, "MECYL Consumption (Ltrs)", r.LO_HO_Cons_MECYL);
                AddKeyValueRow(ws, ref row, "MECYL ROB (Ltrs)", r.LO_HO_Cons_MECYL_ROB);
                AddKeyValueRow(ws, ref row, "AECC Consumption (Ltrs)", r.LO_HO_Cons_AECC);
                AddKeyValueRow(ws, ref row, "AECC ROB (Ltrs)", r.LO_HO_Cons_AECC_ROB);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil Consumption (Ltrs)", r.LO_HO_Cons_HYDR_Oil);
                AddKeyValueRow(ws, ref row, "Hydraulic Oil ROB (Ltrs)", r.LO_HO_Cons_HYDR_Oil_ROB);
            }
            row++;

            ws.Cell(row, 1).Value = "Fuel ROB in MT (EOSP/FWE)";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "EOSP ROB", r.EOSP_ROB);
                AddKeyValueRow(ws, ref row, "FWE ROB", r.FWE_ROB);
            }
            if (dtFuelROB != null)
            {
                foreach (DataRow dr in dtFuelROB.Rows)
                {
                    string eosp = dr.Table.Columns.Contains("EOSP") ? dr["EOSP"]?.ToString() : "";
                    string fwe = dr.Table.Columns.Contains("FWE") ? dr["FWE"]?.ToString() : "";
                    string robVal = string.IsNullOrEmpty(eosp) && string.IsNullOrEmpty(fwe) ? "" : (eosp ?? "-") + " / " + (fwe ?? "-");
                    AddKeyValueRow(ws, ref row, dr["FuelType"]?.ToString() ?? "", robVal);
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

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtFuelCons != null && dtFuelCons.Rows.Count > 0)
            {
                var vlsfo = GetFuelConsByType(dtFuelCons, "VLSFO");
                var mdo = GetFuelConsByType(dtFuelCons, "MDO");
                AddKeyValueRow(ws, ref row, "VLSFO (Total)", FormatDec(vlsfo));
                AddKeyValueRow(ws, ref row, "MDO (Total)", FormatDec(mdo));
            }
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

            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "Cargo";
            ws.Cell(row, 2).Value = "Qty Grade 1";
            ws.Cell(row, 3).Value = "Qty Grade 2";
            ws.Range(row, 1, row, 3).Style.Font.Bold = true;
            row++;
            if (dtARCargo != null && dtARCargo.Rows.Count > 0)
            {
                foreach (DataRow dr in dtARCargo.Rows)
                {
                    string cName = (dr["CargoName"]?.ToString() ?? "").Trim();
                    string pName = (dr["PortName"]?.ToString() ?? "").Trim();
                    ws.Cell(row, 1).Value = string.IsNullOrEmpty(cName) ? "Cargo" : cName + (string.IsNullOrEmpty(pName) ? "" : " (" + pName + ")");
                    ws.Cell(row, 2).Value = FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade1") ? dr["Qty_Grade1"] : null);
                    ws.Cell(row, 3).Value = FormatCargoVal(dr.Table.Columns.Contains("Qty_Grade2") ? dr["Qty_Grade2"] : null);
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
                    using (SqlDataAdapter adp = new SqlDataAdapter("select ChartererAccount, Hours from tblNonRoutineCommon where Report_Table_Id=4 and ReportType_Id=" + id + " and VesselId=" + vesselId + " and IsActive=1 order by Id", ConnectionBulder.con))
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
                    AddBerthingCargoSheet(wb, berthRBind);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
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
                AddKeyValueRow(ws, ref row, "Facility", facilityName);
                AddKeyValueRow(ws, ref row, "Berth", r.BerthName ?? "");
                AddKeyValueRow(ws, ref row, "Port Status", portStatusText);
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date", r.ReportDate != null ? Convert.ToDateTime(r.ReportDate).ToString(ExcelDateFormat) : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            ws.Cell(row, 1).Value = "Manoeuvring & SBE/RFA";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Manoeuvring Hrs", r.Manoeuvring_Hrs);
                AddKeyValueRow(ws, ref row, "Manoeuvring Distance", r.Manoeuvring_Distance);
                AddKeyValueRow(ws, ref row, "SBE Date & Time", r.SBE_DateT != null ? Convert.ToDateTime(r.SBE_DateT).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "RFA Date & Time", r.RFA_DateT != null ? Convert.ToDateTime(r.RFA_DateT).ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "SBE ROB", r.SBE_ROB);
                AddKeyValueRow(ws, ref row, "RFA ROB", r.RFA_ROB);
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

            ws.Cell(row, 1).Value = "Fuel ROB in MT (SBE/RFA)";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            ws.Cell(row, 1).Value = "Fuel Type";
            ws.Cell(row, 2).Value = "SBE";
            ws.Cell(row, 3).Value = "RFA";
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
            else
            {
                ws.Cell(row, 1).Value = "VLSFO";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = "";
                ws.Cell(row, 3).Value = "";
                row++;
                ws.Cell(row, 1).Value = "MDO";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 2).Value = "";
                ws.Cell(row, 3).Value = "";
                row++;
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

            ws.Cell(row, 1).Value = "Fuel Consumption in MT";
            ApplyLightGrayTitle(ws, row, 1, 7);
            row++;
            ws.Cell(row, 1).Value = "";
            ws.Cell(row, 2).Value = "AT SEA";
            ws.Cell(row, 3).Value = "MANOEUV";
            ws.Cell(row, 4).Value = "ANCHOR/WAIT";
            ws.Cell(row, 5).Value = "BERTH";
            ws.Cell(row, 6).Value = "SUB TOTAL";
            ws.Cell(row, 7).Value = "TOTAL";
            ws.Range(row, 1, row, 7).Style.Font.Bold = true;
            row++;
            // VLSFO rows
            {
                decimal vlsfoMeAtSea = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 2);
                decimal vlsfoMeMan = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 3);
                decimal vlsfoMeWait = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 4);
                decimal vlsfoMeBerth = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 5);
                decimal vlsfoMeSub = vlsfoMeAtSea + vlsfoMeMan + vlsfoMeWait + vlsfoMeBerth;
                ws.Cell(row, 1).Value = "VLSFO ME"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), vlsfoMeAtSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), vlsfoMeMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), vlsfoMeWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), vlsfoMeBerth);
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), vlsfoMeSub);
                row++;

                decimal vlsfoAeAtSea = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 7);
                decimal vlsfoAeMan = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 8);
                decimal vlsfoAeWait = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 9);
                decimal vlsfoAeBerth = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 10);
                decimal vlsfoAeSub = vlsfoAeAtSea + vlsfoAeMan + vlsfoAeWait + vlsfoAeBerth;
                ws.Cell(row, 1).Value = "VLSFO AE"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), vlsfoAeAtSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), vlsfoAeMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), vlsfoAeWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), vlsfoAeBerth);
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), vlsfoAeSub);
                row++;

                decimal vlsfoBlrAtSea = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 11);
                decimal vlsfoBlrMan = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 12);
                decimal vlsfoBlrWait = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 13);
                decimal vlsfoBlrBerth = GetFuelConsByTypeAndConsType(dtFuelCons, "VLSFO", 14);
                ws.Cell(row, 1).Value = "VLSFO Boiler"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), vlsfoBlrAtSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), vlsfoBlrMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), vlsfoBlrWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), vlsfoBlrBerth);
                row++;

                decimal vlsfoTotal = GetFuelConsByType(dtFuelCons, "VLSFO");
                ws.Cell(row, 1).Value = "VLSFO Total"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 7), vlsfoTotal);
                row++;
            }
            // MDO rows
            {
                decimal mdoMeAtSea = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 2);
                decimal mdoMeMan = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 3);
                decimal mdoMeWait = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 4);
                decimal mdoMeBerth = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 5);
                decimal mdoMeSub = mdoMeAtSea + mdoMeMan + mdoMeWait + mdoMeBerth;
                ws.Cell(row, 1).Value = "MDO ME"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), mdoMeAtSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), mdoMeMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), mdoMeWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), mdoMeBerth);
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), mdoMeSub);
                row++;

                decimal mdoAeAtSea = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 7);
                decimal mdoAeMan = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 8);
                decimal mdoAeWait = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 9);
                decimal mdoAeBerth = GetFuelConsByTypeAndConsType(dtFuelCons, "MDO", 10);
                decimal mdoAeSub = mdoAeAtSea + mdoAeMan + mdoAeWait + mdoAeBerth;
                ws.Cell(row, 1).Value = "MDO AE"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), mdoAeAtSea);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), mdoAeMan);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), mdoAeWait);
                SetCellValueWithDecimalFormat(ws.Cell(row, 5), mdoAeBerth);
                SetCellValueWithDecimalFormat(ws.Cell(row, 6), mdoAeSub);
                row++;

                decimal mdoTotal = GetFuelConsByType(dtFuelCons, "MDO");
                ws.Cell(row, 1).Value = "MDO Total"; ws.Cell(row, 1).Style.Font.Bold = true;
                SetCellValueWithDecimalFormat(ws.Cell(row, 7), mdoTotal);
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private void AddBerthingCargoSheet(XLWorkbook wb, BerthingReport r)
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

            ws.Cell(row, 1).Value = "Cargo";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Qty Grade 1", r.Qty_Grade1);
                AddKeyValueRow(ws, ref row, "Qty Grade 2", r.Qty_Grade2);
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
                SetCellValueWithDecimalFormat(ws.Cell(row, 2), r.SlopsROB_Oil);
                SetCellValueWithDecimalFormat(ws.Cell(row, 3), r.SlopsROB_Water);
                SetCellValueWithDecimalFormat(ws.Cell(row, 4), r.SlopsROB_Total);
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
                    // Cargo: fetch all cargoes for the current voyage (cargo accumulates across loading
                    // reports under the same VoyageId). Old LRId-only filter missed cargoes loaded
                    // at earlier ports in the same voyage.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select * from LR_Cargo where VesselId=" + vesselId + " and (LRId=" + id + " or VoyageId=" + loadingRBind.VoyageId + ")", ConnectionBulder.con))
                        adp.Fill(dtCargo);
                    // Stoppage: actual column is `Stoppage` (renamed from `Reason`). Scope to loading
                    // stoppages (LoadingDischarged=0) to mirror the web form's query.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select Stoppage as Reason, DateTimeFrom, DateTimeTo from LR_Stoppage where LRId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=0", ConnectionBulder.con))
                        adp.Fill(dtStoppage);
                    // Pumps: table is `tblPump` (singular) per the Loading controller; `tblPumps` returns no rows.
                    using (SqlDataAdapter adp = new SqlDataAdapter(
                        "select a.*, b.Name as PumpName from LR_DCR_PumpsUse a left join tblPump b on a.PumpId=b.Id where a.LRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
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
                AddKeyValueRow(ws, ref row, "Report Date & Time", r.ReportDateTime != null ? Convert.ToDateTime(r.ReportDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "ETD Date & Time", r.ETDDateTime != null ? Convert.ToDateTime(r.ETDDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
            }
            row++;

            // LOP Fields
            ws.Cell(row, 1).Value = "Letter of Protest (LOP)";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Times", r.Times ?? "");
                AddKeyValueRow(ws, ref row, "Rate", r.Rate ?? "");
                AddKeyValueRow(ws, ref row, "Hose Connection", r.Hose_Connection ?? "");
                AddKeyValueRow(ws, ref row, "High H2S", r.High_H2S ?? "");
            }
            row++;

            // Cargo List
            ws.Cell(row, 1).Value = "Cargo Details";
            ApplyLightGrayTitle(ws, row, 1, 8);
            row++;
            if (dtCargo != null && dtCargo.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Cargo Name";
                ws.Cell(row, 2).Value = "Loading Date/Time";
                ws.Cell(row, 3).Value = "Terminal Loading Rate";
                ws.Cell(row, 4).Value = "Loading Rate Accepted";
                ws.Cell(row, 5).Value = "Avg Achieved Rate";
                ws.Cell(row, 6).Value = "Qty Onboard";
                ws.Cell(row, 7).Value = "Balance Qty Loaded";
                ws.Cell(row, 8).Value = "Actual Comp Date/Time";
                ws.Range(row, 1, row, 8).Style.Font.Bold = true;
                row++;

                foreach (DataRow dr in dtCargo.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("CargoName") ? (dr["CargoName"]?.ToString() ?? "") : "";
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("LoadingDatetime") && dr["LoadingDatetime"] != DBNull.Value ? Convert.ToDateTime(dr["LoadingDatetime"]).ToString("yyyy-MM-dd HH:mm") : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("TerminalLoadingRate") ? dr["TerminalLoadingRate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), dr.Table.Columns.Contains("LoadingRateAccepted") ? dr["LoadingRateAccepted"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 5), dr.Table.Columns.Contains("AverageAchievedLoadingRate") ? dr["AverageAchievedLoadingRate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6), dr.Table.Columns.Contains("QuantityOnboard") ? dr["QuantityOnboard"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 7), dr.Table.Columns.Contains("BalanceQuantityLoaded") ? dr["BalanceQuantityLoaded"] : null);
                    ws.Cell(row, 8).Value = dr.Table.Columns.Contains("ActualCompDateTime") && dr["ActualCompDateTime"] != DBNull.Value ? Convert.ToDateTime(dr["ActualCompDateTime"]).ToString("yyyy-MM-dd HH:mm") : "";
                    row++;
                }
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

                foreach (DataRow dr in dtPumpsUse.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("PumpName") ? (dr["PumpName"]?.ToString() ?? "") : (dr.Table.Columns.Contains("Name") ? (dr["Name"]?.ToString() ?? "") : "");
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

                try
                {
                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from DS_Cargo where DSId=" + id + " and VesselId=" + vesselId, ConnectionBulder.con))
                        adp.Fill(dtCargo);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select * from LR_Stoppage where DCId=" + id + " and VesselId=" + vesselId + " and LoadingDischarged=1", ConnectionBulder.con))
                        adp.Fill(dtStoppage);
                    using (SqlDataAdapter adp = new SqlDataAdapter("select a.*, b.Name as PumpName from LR_DCR_PumpsUse a left join tblPumps b on a.PumpId=b.Id where a.DCRId=" + id + " and a.VesselId=" + vesselId, ConnectionBulder.con))
                        adp.Fill(dtPumpsUse);
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
                    AddDischargingDetailsSheet(wb, disRBind, dtCargo, dtStoppage, dtPumpsUse, dtMain);
                    wb.SaveAs(fullPath);
                }
                LogReportExport(reportType, vesselId, datePart, fileName, "Saved");
                lock (_savedReportFilesForCurrentImport) { _savedReportFilesForCurrentImport.Add(fullPath); }
            }
        }

        private void AddDischargingDetailsSheet(XLWorkbook wb, DischargingReport r, DataTable dtCargo, DataTable dtStoppage, DataTable dtPumpsUse, DataTable dtMain)
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

                AddKeyValueRow(ws, ref row, "Voy No.", voyNo);
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "Vessel", r.VesselName ?? "");
                AddKeyValueRow(ws, ref row, "Leg", legText);
                AddKeyValueRow(ws, ref row, "Report Date & Time", r.ReportDateTime != null ? Convert.ToDateTime(r.ReportDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "ETD Date & Time", r.ETDDateTime != null ? Convert.ToDateTime(r.ETDDateTime).ToString("yyyy-MM-dd HH:mm") : "");
                AddKeyValueRow(ws, ref row, "Draft Fwd (Mtrs)", r.DraftFwd);
                AddKeyValueRow(ws, ref row, "Draft Mid (Mtrs)", r.DraftMid);
                AddKeyValueRow(ws, ref row, "Draft Aft (Mtrs)", r.DraftAft);
                AddKeyValueRow(ws, ref row, "Power Packs Onboard", r.Power_Packs_onboard);
                AddKeyValueRow(ws, ref row, "Power Packs Used", r.Power_Packs_Used);
            }
            row++;

            // LOP Fields
            ws.Cell(row, 1).Value = "Letter of Protest (LOP)";
            ApplyLightGrayTitle(ws, row, 1, 4);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Times", r.Times ?? "");
                AddKeyValueRow(ws, ref row, "Rate", r.Rate ?? "");
                AddKeyValueRow(ws, ref row, "Hose Connection", r.Hose_Connection ?? "");
                AddKeyValueRow(ws, ref row, "High H2S", r.High_H2S ?? "");
            }
            row++;

            // Cargo List
            ws.Cell(row, 1).Value = "Cargo Details";
            ApplyLightGrayTitle(ws, row, 1, 10);
            row++;
            if (dtCargo != null && dtCargo.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Cargo Name";
                ws.Cell(row, 2).Value = "Discharge Date/Time";
                ws.Cell(row, 3).Value = "Terminal Acceptable Rate";
                ws.Cell(row, 4).Value = "Discharge Pressure Req.";
                ws.Cell(row, 5).Value = "Avg Discharge Rate";
                ws.Cell(row, 6).Value = "Avg Discharge Pressure";
                ws.Cell(row, 7).Value = "No. of Pumps";
                ws.Cell(row, 8).Value = "Total Cargo Discharged";
                ws.Cell(row, 9).Value = "Balance Cargo";
                ws.Cell(row, 10).Value = "Actual Comp Date/Time";
                ws.Range(row, 1, row, 10).Style.Font.Bold = true;
                row++;

                foreach (DataRow dr in dtCargo.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("CargoName") ? (dr["CargoName"]?.ToString() ?? "") : "";
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("DischargeDatetime") && dr["DischargeDatetime"] != DBNull.Value ? Convert.ToDateTime(dr["DischargeDatetime"]).ToString("yyyy-MM-dd HH:mm") : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), dr.Table.Columns.Contains("Terminal_Acceptable_Discharging_Rate") ? dr["Terminal_Acceptable_Discharging_Rate"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), dr.Table.Columns.Contains("Discharging_pressure_Requested") ? dr["Discharging_pressure_Requested"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 5), dr.Table.Columns.Contains("Average_Discharge_Rate_ByVessel") ? dr["Average_Discharge_Rate_ByVessel"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 6), dr.Table.Columns.Contains("Average_Discharge_pressure_ByVessel") ? dr["Average_Discharge_pressure_ByVessel"] : null);
                    ws.Cell(row, 7).Value = dr.Table.Columns.Contains("No_of_Pumps_Use") && dr["No_of_Pumps_Use"] != DBNull.Value ? dr["No_of_Pumps_Use"].ToString() : "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 8), dr.Table.Columns.Contains("Total_CargoDischarged") ? dr["Total_CargoDischarged"] : null);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 9), dr.Table.Columns.Contains("Balance_Cargo_ToBe_Deischarged") ? dr["Balance_Cargo_ToBe_Deischarged"] : null);
                    ws.Cell(row, 10).Value = dr.Table.Columns.Contains("ActualCompDateTime") && dr["ActualCompDateTime"] != DBNull.Value ? Convert.ToDateTime(dr["ActualCompDateTime"]).ToString("yyyy-MM-dd HH:mm") : "";
                    row++;
                }
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
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("Reason") ? (dr["Reason"]?.ToString() ?? "") : (dr.Table.Columns.Contains("Stoppage") ? (dr["Stoppage"]?.ToString() ?? "") : "");
                    ws.Cell(row, 2).Value = dr.Table.Columns.Contains("DateTimeFrom") && dr["DateTimeFrom"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeFrom"]).ToString("yyyy-MM-dd HH:mm") : "";
                    ws.Cell(row, 3).Value = dr.Table.Columns.Contains("DateTimeTo") && dr["DateTimeTo"] != DBNull.Value ? Convert.ToDateTime(dr["DateTimeTo"]).ToString("yyyy-MM-dd HH:mm") : "";
                    row++;
                }
            }
            row++;

            // Pumps Use
            ws.Cell(row, 1).Value = "Cargo Pump Use";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (dtPumpsUse != null && dtPumpsUse.Rows.Count > 0)
            {
                ws.Cell(row, 1).Value = "Pump Name";
                ws.Cell(row, 2).Value = "Rate";
                ws.Range(row, 1, row, 2).Style.Font.Bold = true;
                row++;

                foreach (DataRow dr in dtPumpsUse.Rows)
                {
                    ws.Cell(row, 1).Value = dr.Table.Columns.Contains("PumpName") ? (dr["PumpName"]?.ToString() ?? "") : (dr.Table.Columns.Contains("Name") ? (dr["Name"]?.ToString() ?? "") : "");
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

                // Fetch fuel details
                var fuelList = CommonMethods.editBunkerFuelList(rowId, vesselId.ToString(), "BunkerFReport");
                if (fuelList != null)
                {
                    foreach (var item in fuelList)
                    {
                        if (item.Fuel_type_Id == 5) item.Fuel_type = "VLSFO";
                        if (item.Fuel_type_Id == 2) item.Fuel_type = "MDO";
                    }
                }

                // Fetch main details from stored procedure
                DataTable dtMain = new DataTable();
                try
                {
                    using (SqlCommand cmd = new SqlCommand("USP_GetSyncEmailReportDetailsByID", ConnectionBulder.con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoyageId", bunkerRBind.VoyageId);
                        cmd.Parameters.AddWithValue("@ReportDate", bunkerRBind.BargeAlongside.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@VesselId", vesselId);
                        cmd.Parameters.AddWithValue("@Action", "BunkerReport");
                        cmd.Parameters.AddWithValue("@id", rowId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            da.Fill(dtMain);
                    }
                }
                catch { }

                DateTime rptDt = bunkerRBind.BargeAlongside;
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
                AddKeyValueRow(ws, ref row, "Port", portDisplay ?? "");
                AddKeyValueRow(ws, ref row, "Supplier", r.Supplier ?? "");
                AddKeyValueRow(ws, ref row, "Barge Name", r.BargeName ?? "");
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
                AddKeyValueRow(ws, ref row, "Bunker Hose Disconnected", r.BunkerHosedisconnected != DateTime.MinValue ? r.BunkerHosedisconnected.ToString(ExcelDateTimeFormat) : "");
                AddKeyValueRow(ws, ref row, "Barge Cast Off", r.BargeCastOff != DateTime.MinValue ? r.BargeCastOff.ToString(ExcelDateTimeFormat) : "");
            }
            row++;

            ws.Cell(row, 1).Value = "Bunker Report Remarks";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "Remarks", r?.Remarks ?? "");
            row++;

            ws.Cell(row, 1).Value = "Reported By";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                string reportedBy = ((r.FirstName ?? "") + " " + (r.LastName ?? "")).Trim();
                AddKeyValueRow(ws, ref row, "Name", reportedBy);
            }
            row++;

            ws.Cell(row, 1).Value = "Lab Analysis Report";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "File Name", r?.LabAnalysisReport_Name ?? "");
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
            ws.Cell(row, 2).Value = "BDN (MT)";
            ws.Cell(row, 3).Value = "Fuel Density";
            ws.Cell(row, 4).Value = "Sulphur Content";
            ws.Cell(row, 5).Value = "BDN Number";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            row++;

            if (fuelList != null && fuelList.Count > 0)
            {
                foreach (var fuel in fuelList)
                {
                    ws.Cell(row, 1).Value = fuel.Fuel_type ?? "";
                    SetCellValueWithDecimalFormat(ws.Cell(row, 2), fuel.BDN);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 3), fuel.Fuel_Density);
                    SetCellValueWithDecimalFormat(ws.Cell(row, 4), fuel.Sulphur_content);
                    ws.Cell(row, 5).Value = fuel.BDN_Number ?? "";
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

                DateTime rptDt = fwRBind.Received_Date;
                if (rptDt == DateTime.MinValue)
                {
                    // fallback to Received_Date from import row
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
            }
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
                AddKeyValueRow(ws, ref row, "Port", r.PortName ?? "");
                AddKeyValueRow(ws, ref row, "Facility Name", r.Facility_Name ?? "");
                AddKeyValueRow(ws, ref row, "Vendor Details", r.VendorDetails ?? "");
                AddKeyValueRow(ws, ref row, "Received Date", r.Received_Date != DateTime.MinValue ? r.Received_Date.ToString(ExcelDateTimeFormat) : "");
            }
            row++;

            ws.Cell(row, 1).Value = "Meter Reading & Quantity";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            if (r != null)
            {
                AddKeyValueRow(ws, ref row, "Initial Meter Reading (MT)", r.Intial_Meter_Reading_MT_supplied);
                AddKeyValueRow(ws, ref row, "Final Meter Reading (MT)", r.Final_Meter_Reading_MT);
                AddKeyValueRow(ws, ref row, "Difference in Meter Reading (MT)", r.Difference_in_Meter_Reading_MT);
                AddKeyValueRow(ws, ref row, "Qty Supplied (MT)", r.QTY_supplied_MT);
            }
            row++;

            ws.Cell(row, 1).Value = "Attached Document";
            ApplyLightGrayTitle(ws, row, 1, 3);
            row++;
            AddKeyValueRow(ws, ref row, "File Name", r?.File_Name ?? "");
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