using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("artist")]
    public class Artist
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }


        [XmlElement("albums")]
        public int NumAlbums { get; set; }

        [XmlElement("songs")]
        public int NumSongs { get; set; }

        [XmlElement("preciserating")]
        public decimal PreciseRating { get; set; }
        [XmlElement("rating")]
        public decimal Rating { get; set; }

        public TagReference[] Tags { get; set; }
    }

    [XmlRoot("artist")]
    public class ArtistReference
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlText]
        public string Name { get; set; }
    }
}
