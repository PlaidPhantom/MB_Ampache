using System;
using System.Globalization;
using System.Xml.Serialization;

namespace MusicBeePlugin.Ampache
{
    [XmlRoot("root")]
    public class PingResponse : AmpacheResponse
    {
        [XmlIgnore]
        public DateTimeOffset SessionExpiration { get; set; }

        [XmlElement("server")]
        public string ServerVersion { get; set; }
        [XmlElement("version")]
        public string ApiVersion { get; set; }
        [XmlElement("compatible")]
        public string CompatibilityVersion { get; set; }


        // ----------------

        private static string iso8601Format = "yyyy-MM-ddTHH:mm:sszzz";

        [XmlElement("session_expire")]
        public string SessionExpirationStr
        {
            get { return SessionExpiration.ToString(iso8601Format); }
            set { SessionExpiration = DateTimeOffset.ParseExact(value, iso8601Format, CultureInfo.InvariantCulture); }
        }
    }
}
