using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MusicBeePlugin.Ampache
{
    [DataContract(Name = "root", Namespace = "")]
    public class AmpacheResponse
    {
        [DataMember(Name = "error", IsRequired = false, Order = 0)]
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
