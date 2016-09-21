using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicBeePlugin
{
    [Serializable]
    class Settings
    {

        public Protocol Protocol { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

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
    }

    enum Protocol
    {
        HTTP,
        HTTPS
    }
}
