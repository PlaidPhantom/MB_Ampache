using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("playlist")]
    public class Playlist
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }
        
        [XmlElement("owner")]
        public string Owner { get; set; }

        [XmlElement("items")]
        public int Items { get; set; }

        [XmlElement("tag")]
        TagReference[] Tags { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }
    }

    [XmlRoot("root")]
    public class PlaylistsResponse : AmpacheResponse
    {
        [XmlElement("total_count")]
        public int TotalCount { get; set; }

        [XmlElement("playlist")]
        public Playlist[] Playlists { get; set; }
    }
}
