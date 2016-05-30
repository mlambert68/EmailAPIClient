using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SendEmail
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var emailMessageContent = new EmailMessageContent()
            {
                From = "GMQuoting@ngic.com",
                To = "mark.lambert@ngic.com",
                Subject = "Email Quote",
                Body = "Test",
                Attachments = Directory
                   .EnumerateFiles("C:\\Users\\i805597\\Pictures")
                   .Where(file => new[] { ".jpg", ".png" }.Contains(Path.GetExtension(file)))
                   .Select(file => new FileAttachment
                   {
                       FileName = Path.GetFileName(file),
                       MimeType = MimeMapping.GetMimeMapping(file),
                       FileData = File.ReadAllBytes(file)
                   })
                   .ToList()
            };

            SendImageSet(emailMessageContent);
        }

        private static void SendImageSet(EmailMessageContent emailMessageContent)
        {
            var multipartContent = new MultipartFormDataContent();

            var emailMessageContentJson = JsonConvert.SerializeObject(emailMessageContent,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            multipartContent.Add(
                new StringContent(emailMessageContentJson, Encoding.UTF8, "application/json"),
                "emailmessagecontent"
                );

            int counter = 0;
            foreach (var iAttachment in emailMessageContent.Attachments)
            {
                var fileContent = new ByteArrayContent(iAttachment.FileData);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(iAttachment.MimeType);
                multipartContent.Add(fileContent, "image" + counter++, iAttachment.FileName);
            }

            var response = new HttpClient()
                .PostAsync("http://localhost:48000/api/EmailServiceAPI/test", multipartContent)
                .Result;

            var responseContent = response.Content.ReadAsStringAsync().Result;
            Trace.Write(responseContent);
        }
    }
}
