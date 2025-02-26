using System;
using System.Runtime.Serialization;

namespace TvOpenPlatform.KafkaClient.Exceptions
{
    [Serializable]
    public class KafkaUnavailableException : Exception
    {
        public KafkaUnavailableException()
        {
        }

        public KafkaUnavailableException(string message) : base(message)
        {
        }

        public KafkaUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected KafkaUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}