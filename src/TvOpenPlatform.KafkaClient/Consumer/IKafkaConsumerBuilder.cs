using System;
using System.Collections.Generic;
using Confluent.Kafka;
using TvOpenPlatform.Logger;

namespace TvOpenPlatform.KafkaClient.Consumer
{
    public interface IKafkaConsumerBuilder<T>
    {
        IConsumer<string, T> Build(ConsumerConfig consumerConfig, Action<Error> errorHandler, Action<List<TopicPartition>> partitionsAssignedHandler, Action<List<TopicPartitionOffset>> partitionsRevokedHandler);
        IConsumer<string, T> Build(ConsumerConfig consumerConfig, Action<Error> errorHandler, Action<List<TopicPartition>> partitionsAssignedHandler, Action<List<TopicPartitionOffset>> partitionsRevokedHandler, ILogger logger);
    }
}