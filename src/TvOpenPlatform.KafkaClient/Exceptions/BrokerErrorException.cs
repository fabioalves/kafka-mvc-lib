using Confluent.Kafka;
using System;
using System.Runtime.Serialization;

namespace TvOpenPlatform.KafkaClient.Exceptions
{
    [Serializable]
    public class BrokerErrorException : Exception
    {

        public KafkaException KafkaException { get; }

        public BrokerErrorException(string message, KafkaException exception) : base(message)
        {
            this.KafkaException = exception;
        }

    }
}