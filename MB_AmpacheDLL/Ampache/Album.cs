﻿using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("album")]
    public class Album
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("artist")]
        public ArtistReference Artist { get; set; }

        [XmlElement("year")]
        public int Year { get; set; }
        [XmlElement("tracks")]
        public int Tracks { get; set; }
        [XmlElement("disk")]
        public int Disk { get; set; }

        [XmlElement("art")]
        public string ArtworkUrl { get; set; }

        [XmlElement("preciserating")]
        public decimal PreciseRating { get; set; }
        [XmlElement("rating")]
        public decimal Rating { get; set; }
        [XmlElement("averagerating")]
        public decimal AverageRating { get; set; }

        [XmlElement("mbid")]
        public string MBID { get; set; }

        [XmlElement("tag")]
        public TagReference[] Tags { get; set; }
    }

    [XmlRoot("album")]
    public class AlbumReference
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlText]
        public string Name { get; set; }
    }

    [XmlRoot("root")]
    public class AlbumsResponse : AmpacheResponse
    {
        [XmlElement("total_count")]
        public int TotalCount { get; set; }
        [XmlElement("album")]
        public Album[] Albums { get; set; }
    }
}
