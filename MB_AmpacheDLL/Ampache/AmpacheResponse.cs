using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot(ElementName = "root", Namespace = "")]
    public class AmpacheResponse
    {
        [XmlElement("error")]
        public string ErrorMessage { get; set; }

        [IgnoreDataMember]
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(ErrorMessage);
            }
        }
    }
}
