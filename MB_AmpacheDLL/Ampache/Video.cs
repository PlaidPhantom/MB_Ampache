using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("video")]
    public class Video
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("mime")]
        public string MimeType { get; set; }

        [XmlElement("resolution")]
        public string Resolution { get; set; }

        [XmlElement("size")]
        public int SizeBytes { get; set; }

        [XmlElement("tag")]
        TagReference[] Tags { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }
    }
}
