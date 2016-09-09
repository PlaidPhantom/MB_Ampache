using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    public class AmpacheClient
    {
        private string AmpacheUrl { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }

        public AmpacheClient(string baseUrl, string username, string password)
        {
            AmpacheUrl = baseUrl;
            Username = username;
            Password = password;
        }

        public event EventHandler<HandshakeEventArgs> HandshakeCompleted;

        public void StartHandshake()
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

            var url = new Uri(ToApiUrl(parameters));

            var apiClient = new WebClient();

            apiClient.DownloadDataCompleted += (sender, args) =>
            {
                var response = args.Result;

                var serializer = new XmlSerializer(typeof(HandshakeResult));

                var stream = new MemoryStream(response);
                var result = (HandshakeResult)serializer.Deserialize(stream); // TODO deserialize

                HandshakeCompleted(this, new HandshakeEventArgs { Result = result });
            };

            apiClient.DownloadDataAsync(url);
        }

        private string ToApiUrl(Dictionary<string, string> dict)
        {
            StringBuilder s = new StringBuilder(AmpacheUrl);

            if (!AmpacheUrl.EndsWith("/"))
                s.Append("/");

            s.Append("server/xml.server.php");

            bool first = true;

            foreach (var pair in dict)
            {
                s.Append(first ? "?" : "&");

                first = false;

                s.Append(HttpUtility.UrlEncode(pair.Key)).Append("=").Append(HttpUtility.UrlEncode(pair.Value));
            }

            return s.ToString();
        }
    }
}
