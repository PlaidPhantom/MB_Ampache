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

        [XmlElement("art")]
        public string ArtworkUrl { get; set; }

        [XmlElement("mbid")]
        public string MBID { get; set; }
        [XmlElement("summary")]
        public string Summary { get; set; }
        [XmlElement("yearformed")]
        public int? YearFormed { get; set; }
        [XmlElement("placeformed")]
        public string PlaceFormed { get; set; }
        

        [XmlElement("preciserating")]
        public decimal PreciseRating { get; set; }
        [XmlElement("rating")]
        public decimal Rating { get; set; }
        [XmlElement("averagerating")]
        public decimal AverageRating { get; set; }

        [XmlElement("tag")]
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

    [XmlRoot("root")]
    public class ArtistsResponse : AmpacheResponse
    {
        [XmlElement("total_count")]
        public int TotalCount { get; set; }

        [XmlElement("artist")]
        public Artist[] Artists { get; set; }
    }
}
