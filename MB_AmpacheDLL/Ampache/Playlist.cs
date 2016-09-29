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

        TagReference[] Tags { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }
    }
}
