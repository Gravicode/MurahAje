using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MurahAje.Web.Tools
{
    public class SmsClient
    {
        private readonly string ZENZIVAURL = "https://reguler.zenziva.net/apps/smsapi.php";
        public string UserKey { get; set; }
        public string PassKey { get; set; }

        public SmsClient(string userKey, string passKey)
        {
            this.UserKey = userKey;
            this.PassKey = passKey;
        }

        public async Task<bool> SendSmsAsync(string sendTo, string message)
        {
            try
            {
                string url = ZENZIVAURL;
                Dictionary<string, string> query = new Dictionary<string, string> { { "userkey", UserKey }, { "passkey", PassKey }, { "nohp", sendTo }, { "pesan", message } };
                string newUri = QueryHelpers.AddQueryString(url, query);
                HttpWebRequest request = WebRequest.Create(newUri) as HttpWebRequest;
                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                XDocument xmlDoc = XDocument.Load(response.GetResponseStream());

                string status = xmlDoc.Root.Element("message").Element("status").Value;

                if (status == "0")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
