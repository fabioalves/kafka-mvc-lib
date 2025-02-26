using System;

namespace TvOpenPlatform.KafkaClient.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KafkaTopic : Attribute
    {
        public KafkaTopic(string topic, string consumer = "default")
        {
            Topic = topic;
            Consumer = consumer;
        }

        public string Topic { get; }
        public string Consumer { get; }
    }
}
