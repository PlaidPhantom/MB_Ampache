using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicBeePlugin
{
    [Serializable]
    public class Settings
    {

        public Protocol Protocol { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public string MakeUrl()
        {
            var builder = new UriBuilder(Server);

            switch(Protocol)
            {
                case Protocol.HTTP:
                    builder.Scheme = "HTTP";
                    break;
                case Protocol.HTTPS:
                    builder.Scheme = "HTTPS";
                    break;
            }

            builder.Port = Port;

            return builder.Uri.ToString();
        }

        public static bool operator ==(Settings lhs, Settings rhs)
        {
            return lhs.Protocol == rhs.Protocol
                && lhs.Server == rhs.Server
                && lhs.Port == rhs.Port
                && lhs.Username == rhs.Username
                && (string.IsNullOrEmpty(lhs.PasswordHash)
                     || string.IsNullOrEmpty(rhs.PasswordHash)
                     || lhs.PasswordHash == rhs.PasswordHash);
        }

        public static bool operator !=(Settings lhs, Settings rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(Settings) && (Settings)obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum Protocol
    {
        HTTP,
        HTTPS
    }
}
