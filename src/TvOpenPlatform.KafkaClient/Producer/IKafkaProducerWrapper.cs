using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Models;

namespace TvOpenPlatform.KafkaClient
{
    
    public interface IKafkaProducerWrapper
    {      
        void Flush(int timeoutMs);

        [Obsolete("Prefer using IProducer")]
        void Produce(string topicName, string key, string messageValue, bool isDeliveryReportEnabled = false);

        [Obsolete("Prefer using IProducer")]
        Task ProduceAsync(string topic, string key, string messageValue, bool isDeliveryReportEnabled = false);
        ProducerConfig GetConfig();
    }
}