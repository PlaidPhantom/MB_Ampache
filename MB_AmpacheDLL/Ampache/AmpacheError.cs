using System;
using System.Runtime.Serialization;

namespace MusicBeePlugin.Ampache
{
    [Serializable]
    internal class AmpacheException : Exception
    {
        public AmpacheException()
        {
        }

        public AmpacheException(string message) : base(message)
        {
        }

        public AmpacheException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AmpacheException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}