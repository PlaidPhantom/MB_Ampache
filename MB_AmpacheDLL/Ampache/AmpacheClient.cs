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

        public void Connect(Action onConnected)
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

                    Ping();
                }, cancellationSignal.Token);

                onConnected();
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

        public void Ping()
        {
            MakeApiCall<PingResponse>("ping", null, (response) =>
            {
                var refreshTime = response.SessionExpiration - DateTimeOffset.Now - TimeSpan.FromMinutes(1);

                cancellationSignal = new CancellationTokenSource();

                refreshTokenTask = Task.Factory.StartNew(() =>
                {
                    cancellationSignal.Token.WaitHandle.WaitOne(refreshTime);

                    cancellationSignal.Token.ThrowIfCancellationRequested();

                    Ping();
                }, cancellationSignal.Token);
            });
        }

        public void UrlToSong(string songUrl, Action<Song> callback)
        {
            MakeApiCall<SongsResponse>("url_to_song", new Dictionary<string, string> { ["url"] = songUrl }, (response) =>
            {
                callback(response.Songs[0]);
            });
        }

        public void GetArtists(string filter, Action<Artist[]> callback, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if(!string.IsNullOrEmpty(filter))
            {
                options.Add("filter", filter);
                options.Add("exact", filterIsExact.ToString());
            }

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            MakeApiCall<ArtistsResponse>("artists", options, (response) =>
            {
                callback(response.Artists);
            });
        }

        public void GetArtist(int artistId, Action<Artist> callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", artistId.ToString());

            MakeApiCall<ArtistsResponse>("artists", options, (response) =>
            {
                callback(response.Artists[0]);
            });
        }

        public void GetArtistSongs(int artistId, Action<Song[]> callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", artistId.ToString());

            MakeApiCall<SongsResponse>("artist_songs", options, (response) =>
            {
                callback(response.Songs);
            });
        }

        private void MakeApiCall<T>(string action, Dictionary<string, string> parameters, Action<T> callback) where T : AmpacheResponse
        {
            var urlParams = new Dictionary<string, string>
            {
                ["action"] = action,
                ["auth"] = AuthToken,
            };

            if(parameters != null)
                foreach(var pair in parameters)
                    urlParams.Add(pair.Key, pair.Value);

            var url = new Uri(ToApiUrl(urlParams));

            var apiClient = new WebClient();

            apiClient.DownloadDataCompleted += (sender, args) =>
            {
                var result = args.Result;

                var serializer = new XmlSerializer(typeof(T));

                var stream = new MemoryStream(result);
                var response = (T)serializer.Deserialize(stream);

                if (response.HasError)
                    throw new AmpacheException(response.ErrorMessage);

                callback(response);

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
