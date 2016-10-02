using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    public class Tag
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("albums")]
        public int NumAlbums { get; set; }
        [XmlElement("artists")]
        public int NumArtists { get; set; }
        [XmlElement("songs")]
        public int NumSongs { get; set; }
        [XmlElement("videos")]
        public int NumVideos { get; set; }
        [XmlElement("playlists")]
        public int NumPlaylists { get; set; }
        [XmlElement("stream")]
        public int NumStreams { get; set; }
    }

    [XmlRoot("tag")]
    public class TagReference
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlText]
        public string Name { get; set; }

        [XmlAttribute("count")]
        public int Count { get; set; }
    }

    [XmlRoot("root")]
    public class TagsResponse : AmpacheResponse
    {
        [XmlElement("total_count")]
        public int TotalCount { get; set; }

        [XmlElement("tag")]
        public Tag[] Tags { get; set; }
    }
}
