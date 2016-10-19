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

        private Timer pingTimer;

        public DateTimeOffset LastUpdate { get; private set; }
        public DateTimeOffset LastAdd { get; private set; }
        public DateTimeOffset LastClean { get; private set; }

        public int TotalSongs { get; private set; }
        public int TotalAlbums { get; private set; }
        public int TotalArtists { get; private set; }
        public int TotalTags { get; private set; }
        public int TotalPlaylists { get; private set; }
        public int TotalVideos { get; private set; }
        public int TotalCatalogs { get; private set; }


        public AmpacheClient(string baseUrl, string username, string passwordHash)
        {
            pingTimer = new Timer(Ping, null, Timeout.Infinite, Timeout.Infinite);

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

        public bool IsConnected => AuthToken != null;

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

            var result = apiClient.DownloadData(url);

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

            SchedulePing(response.SessionExpiration);
        }

        public void Disconnect()
        {
            pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void SchedulePing(DateTimeOffset sessionExpiration)
        {
            var refreshTime = sessionExpiration - DateTimeOffset.Now - TimeSpan.FromMinutes(1);

            pingTimer.Change(refreshTime, TimeSpan.FromMilliseconds(-1));
        }

        public void Ping(object state)
        {
            var response = MakeApiCall<PingResponse>("ping", null);

            SchedulePing(response.SessionExpiration);
        }

        public Song UrlToSong(string songUrl)
        {
            var response = MakeApiCall<SongsResponse>("url_to_song", new Dictionary<string, string> { ["url"] = songUrl });

            return response.Songs[0];
        }

        #region Artists

        public Artist[] GetArtists(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
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

            var response = MakeApiCall<ArtistsResponse>("artists", options);

            return response.Artists;
        }

        public Artist GetArtist(int artistId)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", artistId.ToString());

            var response = MakeApiCall<ArtistsResponse>("artists", options);

            return response.Artists[0];
        }

        public Song[] GetArtistSongs(int artistId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", artistId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("artist_songs", options);

            return response.Songs;
        }

        public Album[] GetArtistAlbums(int artistId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", artistId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<AlbumsResponse>("artist_albums", options);

            return response.Albums;
        }

        #endregion

        #region Albums

        public Album[] GetAlbums(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
            {
                options.Add("filter", filter);
                options.Add("exact", filterIsExact.ToString());
            }

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<AlbumsResponse>("albums", options);

            return response.Albums;
        }

        public Album GetAlbum(int albumId)
        {
            var options = new Dictionary<string, string>();
            
            options.Add("filter", albumId.ToString());

            var response = MakeApiCall<AlbumsResponse>("album", options);

                return response.Albums[0];
        }

        public Song[] GetAlbumSongs(int albumId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", albumId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("album_songs", options);

                return response.Songs;
        }

        #endregion

        #region Tags

        public Tag[] GetTags(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
            {
                options.Add("filter", filter);
                options.Add("exact", filterIsExact.ToString());
            }

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<TagsResponse>("tags", options);

                return response.Tags;
        }

        public Tag GetTag(int tagId)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", tagId.ToString());

            var response = MakeApiCall<TagsResponse>("tag", options);

                return response.Tags[0];
        }

        public Artist[] GetTagArtists(int tagId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", tagId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<ArtistsResponse>("tag_albums", options);

                return response.Artists;
        }

        public Album[] GetTagAlbums(int tagId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", tagId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<AlbumsResponse>("tag_albums", options);

                return response.Albums;
        }

        public Song[] GetTagSongs(int tagId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", tagId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("tag_songs", options);

                return response.Songs;
        }

        #endregion

        #region Songs

        public Song[] GetSongs(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
            {
                options.Add("filter", filter);
                options.Add("exact", filterIsExact.ToString());
            }

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("songs", options);

                return response.Songs;
        }

        public Song GetSong(int songId)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", songId.ToString());

            var response = MakeApiCall<SongsResponse>("song", options);

            return response.Songs[0];
        }

        public Song[] SearchSongs(string filter, int? offset = null, int? limit = null)
        {
            if (string.IsNullOrEmpty(filter))
                throw new ArgumentException("filter must have a value", nameof(filter));

            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
                options.Add("filter", filter);

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("search_songs", options);

                return response.Songs;
        }

        #endregion

        #region Playlists

        public Playlist[] GetPlaylists(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
                options.Add("filter", filter);

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<PlaylistsResponse>("playlists", options);

                return response.Playlists;
        }

        public Playlist GetPlaylist(int playlistId)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", playlistId.ToString());

            var response = MakeApiCall<PlaylistsResponse>("playlist", options);

                return response.Playlists[0];
        }

        public Song[] GetPlaylistSongs(int playlistId, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", playlistId.ToString());

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<SongsResponse>("playlist_songs", options);

                return response.Songs;
        }

        public Playlist CreatePlaylist(string name, string type = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Playlist name is required.", nameof(name));

            if (type != null && type != "public" && type != "private")
                throw new ArgumentException("Playlist type, if provided, must be \"public\" or \"private\".");

            var options = new Dictionary<string, string>();

            options.Add("name", name);

            if (type != null)
                options.Add("type", type);

            var response = MakeApiCall<PlaylistsResponse>("playlist_create", options);

                return response.Playlists[0];
        }

        public void DeletePlaylist(int playlistId, Action callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", playlistId.ToString());

            var response = MakeApiCall<AmpacheResponse>("playlist_songs", options);
        }

        public void PlaylistAddSong(int playlistId, int songId, Action callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", playlistId.ToString());
            options.Add("song", songId.ToString());

            var response = MakeApiCall<AmpacheResponse>("playlist_add_song", options);
        }

        public void PlaylistRemoveTrack(int playlistId, int trackNumber, Action callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", playlistId.ToString());
            options.Add("track", trackNumber.ToString());

            var response = MakeApiCall<AmpacheResponse>("playlist_remove_song", options);
        }

        #endregion

        #region Videos

        public Video[] GetVideos(string filter, bool filterIsExact = false, int? offset = null, int? limit = null)
        {
            var options = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(filter))
                options.Add("filter", filter);

            if (offset.HasValue)
                options.Add("offset", offset.Value.ToString());

            if (limit.HasValue)
                options.Add("limit", limit.Value.ToString());

            var response = MakeApiCall<VideosResponse>("videos", options);

                return response.Videos;
        }

        public Video GetVideo(int videoId)
        {
            var options = new Dictionary<string, string>();

            options.Add("filter", videoId.ToString());

            var response = MakeApiCall<VideosResponse>("playlist", options);

                return response.Videos[0];
        }

        #endregion

        #region Rating

        public void Rate(string type, int id, int rating, Action callback)
        {
            var options = new Dictionary<string, string>();

            options.Add("type", type);
            options.Add("id", id.ToString());
            options.Add("rating", rating.ToString());

            var response = MakeApiCall<VideosResponse>("rate", options);
        }

        #endregion

        private T MakeApiCall<T>(string action, Dictionary<string, string> parameters) where T : AmpacheResponse
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

            var result = apiClient.DownloadData(url);

            var serializer = new XmlSerializer(typeof(T));

            var stream = new MemoryStream(result);
            var response = (T)serializer.Deserialize(stream);

            if (response.HasError)
                throw new AmpacheException(response.ErrorMessage);

            return response;
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
