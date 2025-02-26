using System;
using System.Runtime.Serialization;

namespace TvOpenPlatform.Consumer.Deserializers.Exceptions
{
    [Serializable]
    public class DeserializeException : Exception
    {
        public DeserializeException()
        {
        }

        public DeserializeException(string message) : base(message)
        {
        }

        public DeserializeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeserializeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
