using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace pfSenseBackup.Classes
{
    public class PfSense
    {
        readonly string username;
        readonly string password;
        readonly string url;

        CookieContainer cookieContainer = new CookieContainer();
        string csrfToken;

        public PfSense(string url, string username, string password)
        {
            this.username = username;
            this.password = password;
            this.url = url;
        }

        public bool DownloadBackup()
        {
            try
            {
                if (Stage1() && Stage2() && Stage3())
                {
                    Console.WriteLine($"{DateTime.Now} [SUCCESS] Downloaded a backup copy!");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} [FATAL] Could not download config: {ex.Message}");
                return false;
            }

        }

        private bool Stage1()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{url}/diag_backup.php");
            request.CookieContainer = cookieContainer;
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string html;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                html = reader.ReadToEnd();
            }

            ExtractCSRFToken(html);

            return true;
        }

        private bool Stage2()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{url}/diag_backup.php");
            request.CookieContainer = cookieContainer;
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            request.Method = "POST";

            var postData = "login=Login";
            postData += $"&usernamefld={username}";
            postData += $"&passwordfld={password}";
            postData += $"&__csrf_magic={csrfToken}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string html;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                html = reader.ReadToEnd();
            }

            // pfSense does not throw a 403 or some sort, so we need to check the contents if it contains username/password invalid string
            if (html.Contains("Username or Password incorrect"))
                throw new UsernamePasswordInvalidException("Username or Password incorrect");

            ExtractCSRFToken(html);

            return true;
        }

        private bool Stage3()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{url}/diag_backup.php");
            request.CookieContainer = cookieContainer;
            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            request.Method = "POST";

            var postData = "download=download";
            postData += "&donotbackuprrd=yes";
            postData += $"&__csrf_magic={csrfToken}";
            var data = Encoding.ASCII.GetBytes(postData);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                // Create file
                string fileName = $"pfSenseBackup-{DateTime.Now}.xml";
                Directory.CreateDirectory("backups"); // create directory if it does not exist
                using (FileStream fs = File.Create(Path.Combine("backups", fileName.ToSafeFileName())))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(reader.ReadToEnd());
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            return true;
        }

        private void ExtractCSRFToken(string html)
        {
            // Parse html, extract the CSRF token
            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html);

            csrfToken = htmlDocument.DocumentNode.Descendants("input")
                .FirstOrDefault(x => x.Attributes["name"].Value == "__csrf_magic")
                .Attributes["value"].Value;
        }

    }
}
