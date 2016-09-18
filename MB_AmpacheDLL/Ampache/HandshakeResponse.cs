using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    public class HandshakeEventArgs : EventArgs
    {
        public HandshakeResponse Response { get; set; }
    }

    [DataContract(Name = "root", Namespace = "")]
    public class HandshakeResponse : AmpacheResponse
    {
        [DataMember(Name = "auth", Order = 1)]
        public string AuthToken { get; set; }
        [IgnoreDataMember]
        public DateTimeOffset SessionExpiration { get; set; }

        [DataMember(Name = "api", Order = 2)]
        public string ApiVersion { get; set; }

        [IgnoreDataMember]
        public DateTimeOffset LastUpdate { get; set; }
        [IgnoreDataMember]
        public DateTimeOffset LastAdd { get; set; }
        [IgnoreDataMember]
        public DateTimeOffset LastClean { get; set; }

        [DataMember(Name = "songs", Order = 7)]
        public int TotalSongs { get; set; }
        [DataMember(Name = "artists", Order = 9)]
        public int TotalArtists { get; set; }
        [DataMember(Name = "albums", Order = 8)]
        public int TotalAlbums { get; set; }
        [DataMember(Name = "tags", IsRequired = false, Order = 10)]
        public int TotalTags { get; set; }
        [DataMember(Name = "playlists", Order = 10)]
        public int TotalPlaylists { get; set; }
        [DataMember(Name = "videos", Order = 11)]
        public int TotalVideos { get; set; }
        [DataMember(Name = "catalogs", Order = 12)]
        public int TotalCatalogs { get; set; }

        // the api docs still list this, but my Ampache doesn't return it

        [DataMember(Name = "version", IsRequired = false, Order = 2)]
        private string Version
        {
            get { return ApiVersion; }
            set { ApiVersion = value; }
        }

        // DateTimeOffset helpers uggh

        private static string iso8601Format = "yyyy-MM-ddTHH:mm:sszzz";

        [DataMember(Name = "session_expire", Order = 3)]
        private string SessionExpirationStr
        {
            get { return SessionExpiration.ToString(iso8601Format); }
            set { SessionExpiration = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [DataMember(Name = "update", Order = 4)]
        private string LastUpdateStr
        {
            get { return LastUpdate.ToString(iso8601Format); }
            set { LastUpdate = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [DataMember(Name = "add", Order = 5)]
        private string LastAddStr
        {
            get { return LastAdd.ToString(iso8601Format); }
            set { LastAdd = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [DataMember(Name = "clean", Order = 6)]
        private string LastCleanStr
        {
            get { return LastClean.ToString(iso8601Format); }
            set { LastClean = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
    }
}