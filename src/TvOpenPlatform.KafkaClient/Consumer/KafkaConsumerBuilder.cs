using Confluent.Kafka;
using System;
using System.Collections.Generic;
using TvOpenPlatform.Logger;
using TvOpenPlatform.KafkaClient.LogAdapter;

namespace TvOpenPlatform.KafkaClient.Consumer
{
    public class KafkaConsumerBuilder<T> : IKafkaConsumerBuilder<T>
    {
        public IConsumer<string, T> Build(
         ConsumerConfig consumerConfig,
         Action<Error> errorHandler,
         Action<List<TopicPartition>> partitionsAssignedHandler,
         Action<List<TopicPartitionOffset>> partitionsRevokedHandler) 
        {
            return this.Build(consumerConfig, errorHandler, partitionsAssignedHandler, partitionsRevokedHandler, logger: null);
        }

        public IConsumer<string, T> Build(
            ConsumerConfig consumerConfig,
            Action<Error> errorHandler,
            Action<List<TopicPartition>> partitionsAssignedHandler,
            Action<List<TopicPartitionOffset>> partitionsRevokedHandler,
            ILogger logger)
        {
            var builder = new ConsumerBuilder<string, T>(consumerConfig)
                            .SetErrorHandler((_, e) => errorHandler(e))
                            .SetLogHandler((producer, logMessage) => logger?.Log(logMessage.Level.ToTvOpenPlatformLogLevel(), logMessage.Facility, $"[KAFKA CONSUMER] {logMessage.Name} {logMessage.Message}"))
                            .SetPartitionsAssignedHandler((c, partitions) =>
                            {
                                partitionsAssignedHandler(partitions);
                            })                            
                            .SetPartitionsRevokedHandler((c, partitions) =>
                            {
                                partitionsRevokedHandler(partitions);
                            });

            return builder.Build();
        }
    }
}
