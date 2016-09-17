using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicBeePlugin.Ampache
{
    public class AmpacheResponse
    {
        public string error { get; set; }
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(error);
            }
        }
    }
}
