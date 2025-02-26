using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient;
using TvOpenPlatform.KafkaClient.Models;

namespace TvOpenPlatform.KafkaClient
{
    [Obsolete("Prefer using IProducer")]
    public class KafkaProducerWrapperCompatibility<A, B> : IKafkaProducerWrapper<A, B>
    {
        IKafkaProducerWrapper _kafkaProducerWrapper;

        public KafkaProducerWrapperCompatibility(IKafkaProducerWrapper kafkaProducerWrapper)
        {
            _kafkaProducerWrapper = kafkaProducerWrapper;
        }

        public void Flush(int timeoutMs)
        {
            _kafkaProducerWrapper.Flush(timeoutMs);
        }

        public ProducerConfig GetConfig()
        {
            return _kafkaProducerWrapper.GetConfig();
        }


        public void Produce(string topicName, string key, string messageValue, bool isDeliveryReportEnabled = false)
        {
            _kafkaProducerWrapper.Produce(topicName, key, messageValue, isDeliveryReportEnabled);
        }

        public Task ProduceAsync(string topic, string key, string messageValue, bool isDeliveryReportEnabled = false)
        {
            return _kafkaProducerWrapper.ProduceAsync(topic, key, messageValue, isDeliveryReportEnabled);
        }
    }
}
