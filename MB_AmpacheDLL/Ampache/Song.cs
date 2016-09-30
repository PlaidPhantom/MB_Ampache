using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("song")]
    public class Song
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlElement("art")]
        public string ArtworkUrl { get; set; }

        [XmlElement("artist")]
        public ArtistReference Artist { get; set; }
        [XmlElement("album")]
        public AlbumReference Album { get; set; }
        [XmlElement("composer")]
        public string Composer { get; set; }

        [XmlElement("comment")]
        public string Comment { get; set; }
        [XmlElement("publisher")]
        public string Publisher { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("filename")]
        public string FilePath { get; set; }

        [XmlElement("tag")]
        TagReference[] Tags { get; set; }

        [XmlElement("track")]
        public int Track { get; set; }

        [XmlElement("time")]
        public int TimeSeconds { get; set; }
        [XmlElement("size")]
        public int SizeBytes { get; set; }

        [XmlElement("year")]
        public int? Year { get; set; }

        [XmlElement("bitrate")]
        public int BitRate { get; set; }
        [XmlElement("rate")]
        public int SampleRate { get; set; }
        [XmlElement("mode")]
        public string EncodingMode { get; set; }
        [XmlElement("mime")]
        public string MimeType { get; set; }
        [XmlElement("channels")]
        public string Channels { get; set; }


        [XmlElement("mbid")]
        public string MBID { get; set; }
        [XmlElement("album_mbid")]
        public string AlbumMBID { get; set; }
        [XmlElement("artist_mbid")]
        public string ArtistMBID { get; set; }
        [XmlElement("albumartist_mbid")]
        public string AlbumArtistMBID { get; set; }

        [XmlElement("preciserating")]
        public decimal PreciseRating { get; set; }
        [XmlElement("rating")]
        public decimal Rating { get; set; }
        [XmlElement("averagerating")]
        public decimal AverageRating { get; set; }

        [XmlElement("replaygain_album_gain")]
        public decimal ReplayGainAlbumGain { get; set; }
        [XmlElement("replaygain_album_peak")]
        public decimal ReplayGainAlbumPeak { get; set; }
        [XmlElement("replaygain_track_gain")]
        public decimal ReplayGainTrackGain { get; set; }
        [XmlElement("replaygain_track_peak")]
        public decimal ReplayGainTrackPeak { get; set; }
    }

    [XmlRoot("root")]
    public class SongsResponse : AmpacheResponse
    {
        [XmlElement("total_count")]
        public int TotalCount { get; set; }

        [XmlElement("song")]
        public Song[] Songs { get; set; }
    }
}
