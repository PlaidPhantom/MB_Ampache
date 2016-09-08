using System;

namespace MusicBeePlugin.Ampache
{
    internal class HandshakeResult
    {
        public string auth { get; set; }
        public string version { get; set; }

        public DateTimeOffset update { get; set; }
        public DateTimeOffset add { get; set; }
        public DateTimeOffset clean { get; set; }

        public int songs { get; set; }
        public int artists { get; set; }
        public int albums { get; set; }
        public int tags { get; set; }
        public int videos { get; set; }
    }
}