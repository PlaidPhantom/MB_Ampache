using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    public class HandshakeResponse : AmpacheResponse
    {
        [XmlElement("auth")]
        public string AuthToken { get; set; }
        [XmlIgnore]
        public DateTimeOffset SessionExpiration { get; set; }

        [XmlElement("api")]
        [XmlElement("version")]
        [XmlChoiceIdentifier("ApiVersionTag")]
        public string ApiVersion { get; set; }

        [XmlIgnore]
        public DateTimeOffset LastUpdate { get; set; }
        [XmlIgnore]
        public DateTimeOffset LastAdd { get; set; }
        [XmlIgnore]
        public DateTimeOffset LastClean { get; set; }

        [XmlElement("songs")]
        public int TotalSongs { get; set; }
        [XmlElement("albums")]
        public int TotalAlbums { get; set; }
        [XmlElement("artists")]
        public int TotalArtists { get; set; }
        [XmlElement("tags")]
        public int TotalTags { get; set; }
        [XmlElement("playlists")]
        public int TotalPlaylists { get; set; }
        [XmlElement("videos")]
        public int TotalVideos { get; set; }
        [XmlElement("catalogs")]
        public int TotalCatalogs { get; set; }

        // Serialization Helpers

        [XmlIgnore]
        public ApiVersionTags ApiVersionTag = ApiVersionTags.api;

        [XmlType(IncludeInSchema = false)]
        public enum ApiVersionTags
        {
            api, version
        }

        // DateTimeOffset helpers uggh

        private static string iso8601Format = "yyyy-MM-ddTHH:mm:sszzz";

        [XmlElement("session_expire")]
        public string SessionExpirationStr
        {
            get { return SessionExpiration.ToString(iso8601Format); }
            set { SessionExpiration = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [XmlElement("update")]
        public string LastUpdateStr
        {
            get { return LastUpdate.ToString(iso8601Format); }
            set { LastUpdate = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [XmlElement("add")]
        public string LastAddStr
        {
            get { return LastAdd.ToString(iso8601Format); }
            set { LastAdd = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
        [XmlElement("clean")]
        public string LastCleanStr
        {
            get { return LastClean.ToString(iso8601Format); }
            set { LastClean = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
    }
}