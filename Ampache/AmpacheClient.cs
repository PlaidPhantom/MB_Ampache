using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MusicBeePlugin.Ampache
{
    public class AmpacheClient
    {
        private WebClient apiClient { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }

        public AmpacheClient(Uri url, string username, string password)
        {
            var builder = new UriBuilder(url);

            builder.Path += "/server/xml.server.php";

            apiClient = new WebClient { BaseAddress = builder.Uri.ToString() };

            Username = username;
            Password = password;
        }

        public void Handshake()
        {
            var sha256 = SHA256.Create();

            var timestamp = ((int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds)).ToString();

            var timebytes = Encoding.UTF8.GetBytes(timestamp);
            var passbytes = Encoding.UTF8.GetBytes(Password);

            var hash = sha256.ComputeHash(passbytes);

            var authhash = sha256.ComputeHash(timebytes.Concat(passbytes).ToArray());

            var auth = Convert.ToBase64String(authhash);

            var parameters = new Dictionary<string, string>
            {
                ["action"] = "handshake",
                ["user"] = Username,
                ["version"] = "350001",
                ["timestamp"] = timestamp,
                ["auth"] = auth
            };

            var url = new Uri(ToQueryString(parameters));

            apiClient.DownloadStringAsync(url);
        }

        private static string ToQueryString(Dictionary<string, string> dict)
        {
            StringBuilder s = new StringBuilder("?");

            foreach (var pair in dict)
            {
                if (s.Length > 1)
                    s.Append("&");

                s.Append(HttpUtility.UrlEncode(pair.Key)).Append("=").Append(HttpUtility.UrlEncode(pair.Value));
            }

            return s.ToString();
        }
    }
}
