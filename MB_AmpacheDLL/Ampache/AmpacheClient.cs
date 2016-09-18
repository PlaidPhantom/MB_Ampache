using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
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

        private string AuthToken { get; set; }

        public AmpacheClient(string baseUrl)
        {
            AmpacheUrl = baseUrl;
        }

        public event EventHandler<HandshakeEventArgs> HandshakeCompleted;

        public void StartHandshake(string username, string password)
        {
            var sha256 = SHA256.Create();

            var timestamp = ((int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds)).ToString();

            var passbytes = Encoding.UTF8.GetBytes(password);

            var hash = sha256.ComputeHash(passbytes);

            var hashstring = timestamp + string.Join("", hash.Select(b => b.ToString("x2")));

            var hashhexbytes = Encoding.UTF8.GetBytes(hashstring);

            var authhash = sha256.ComputeHash(hashhexbytes);

            var hex = string.Join("", authhash.Select(b => b.ToString("x2")));

            var parameters = new Dictionary<string, string>
            {
                ["action"] = "handshake",
                ["user"] = username,
                ["version"] = "350001",
                ["timestamp"] = timestamp,
                ["auth"] = hex
            };

            var url = new Uri(ToApiUrl(parameters));

            var apiClient = new WebClient();

            apiClient.DownloadDataCompleted += (sender, args) =>
            {
                var result = args.Result;

                Debug.Write(Encoding.UTF8.GetString(result));

                var serializer = new DataContractSerializer(typeof(HandshakeResponse));

                var stream = new MemoryStream(result);
                var response = (HandshakeResponse)serializer.ReadObject(stream);

                if (response.HasError)
                    throw new AmpacheException(response.ErrorMessage);

                AuthToken = response.AuthToken;

                HandshakeCompleted(this, new HandshakeEventArgs { Response = response });
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
