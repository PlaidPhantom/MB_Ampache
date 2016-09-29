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

        ArtistReference Artist { get; set; }
        AlbumReference Album { get; set; }

        TagReference[] Tags { get; set; }

        [XmlElement("track")]
        public int Track { get; set; }

        [XmlElement("time")]
        public int TimeSeconds { get; set; }
        [XmlElement("size")]
        public int SizeBytes { get; set; }

        [XmlElement("art")]
        public string ArtworkUrl { get; set; }

        [XmlElement("preciserating")]
        public decimal PreciseRating { get; set; }
        [XmlElement("rating")]
        public decimal Rating { get; set; }
    }
}
