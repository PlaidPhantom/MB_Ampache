using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    public class AmpacheClient
    {
        private string AmpacheUrl { get; set; }
        private string Username { get; set; }
        private string PasswordHash { get; set; }

        private string AuthToken { get; set; }

        private Task refreshTokenTask;
        private CancellationTokenSource cancellationSignal;

        public DateTimeOffset LastUpdate { get; set; }
        public DateTimeOffset LastAdd { get; set; }
        public DateTimeOffset LastClean { get; set; }

        public int TotalSongs { get; set; }
        public int TotalAlbums { get; set; }
        public int TotalArtists { get; set; }
        public int TotalTags { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalVideos { get; set; }
        public int TotalCatalogs { get; set; }


        public AmpacheClient(string baseUrl, string username, string passwordHash)
        {
            AmpacheUrl = baseUrl;
            Username = username;
            PasswordHash = passwordHash;
        }

        public static string PreHash(string password)
        {
            var sha256 = SHA256.Create();

            var passbytes = Encoding.UTF8.GetBytes(password);

            var hash = sha256.ComputeHash(passbytes);

            return string.Join("", hash.Select(b => b.ToString("x2")));
        }

        public event EventHandler<AmpacheConnectedEventArgs> Connected;

        public void Connect()
        {
            var sha256 = SHA256.Create();

            var timestamp = ((int)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds)).ToString();

            var hashstring = timestamp + PasswordHash;

            var hashhexbytes = Encoding.UTF8.GetBytes(hashstring);

            var authhash = sha256.ComputeHash(hashhexbytes);

            var hex = string.Join("", authhash.Select(b => b.ToString("x2")));

            var parameters = new Dictionary<string, string>
            {
                ["action"] = "handshake",
                ["user"] = Username,
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
                
                var serializer = new XmlSerializer(typeof(HandshakeResponse));

                var stream = new MemoryStream(result);
                var response = (HandshakeResponse)serializer.Deserialize(stream);

                if (response.HasError)
                    throw new AmpacheException(response.ErrorMessage);

                AuthToken = response.AuthToken;
                LastAdd = response.LastAdd;
                LastClean = response.LastClean;
                LastUpdate = response.LastUpdate;
                TotalAlbums = response.TotalAlbums;
                TotalArtists = response.TotalArtists;
                TotalCatalogs = response.TotalCatalogs;
                TotalPlaylists = response.TotalPlaylists;
                TotalSongs = response.TotalSongs;
                TotalTags = response.TotalTags;
                TotalVideos = response.TotalVideos;

                var refreshTime = response.SessionExpiration - DateTimeOffset.Now - TimeSpan.FromMinutes(1);

                cancellationSignal = new CancellationTokenSource();

                refreshTokenTask = Task.Factory.StartNew(() =>
                {
                    cancellationSignal.Token.WaitHandle.WaitOne(refreshTime);

                    cancellationSignal.Token.ThrowIfCancellationRequested();

                    Connect();
                }, cancellationSignal.Token);

                Connected(this, new AmpacheConnectedEventArgs { Response = response });
            };

            apiClient.DownloadDataAsync(url);
        }

        public void Disconnect()
        {
            cancellationSignal.Cancel();

            try
            {
                refreshTokenTask.Wait();
            }
            catch(AggregateException e)
            {
                if (e.InnerExceptions.Any(ex => ex.GetType() != typeof(TaskCanceledException)))
                    throw e;
            }
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
