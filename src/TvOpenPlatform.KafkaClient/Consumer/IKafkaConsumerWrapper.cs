using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;

namespace TvOpenPlatform.KafkaClient
{
    public interface IKafkaConsumerWrapper<T>
    {
        void CommitOffset(ConsumeResult<string, T> consumeResult);
        void StartConsumption(List<string> topics, Action<ConsumeResult<string, T>> messageHandler, CancellationToken? cancellationToken = null, TimeSpan? delayTime = null);
        void RestartConsumer();
        void ReprocessMessage(TopicPartitionOffset topicPartitionOffset);
        void Pause(TopicPartition topicPartition);
        void Resume(TopicPartition topicPartition);
    }
}