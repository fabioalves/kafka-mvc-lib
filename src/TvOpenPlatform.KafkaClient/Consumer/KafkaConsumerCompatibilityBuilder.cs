using Confluent.Kafka;
using System;
using System.Collections.Generic;
using TvOpenPlatform.Logger;

namespace TvOpenPlatform.KafkaClient.Consumer
{
    [Obsolete]
    public interface IKafkaConsumerBuilder : IKafkaConsumerBuilder<string> { }
    
    [Obsolete]
    public class KafkaConsumerBuilder : KafkaConsumerBuilder<string>, IKafkaConsumerBuilder
    {
        public IConsumer<string, string> Build(
            ConsumerConfig consumerConfig,
            Action<Error> errorHandler,
            Action<List<TopicPartition>> partitionsAssignedHandler,
            Action<List<TopicPartitionOffset>> partitionsRevokedHandler,
            ILogger logger)
        {
            return base.Build(consumerConfig, errorHandler, partitionsAssignedHandler, partitionsRevokedHandler, logger);
        }
    }
}
