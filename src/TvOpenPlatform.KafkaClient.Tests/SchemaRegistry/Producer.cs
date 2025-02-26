using Confluent.Kafka;
using Confluent.SchemaRegistry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TvOpenPlatform.KafkaClient.Tests.Producer.SchemaRegistry
{
    public class Producer<T>
    {
        private readonly ProducerConfig _producerConfig;
        private readonly SchemaRegistryConfig _schemaRegistryConfig;
        private readonly IAsyncSerializer<T> _serializer;

        public Producer(ProducerConfig producerConfig, SchemaRegistryConfig schemaRegistryConfig, IAsyncSerializer<T> serializer)
        {
            _producerConfig = producerConfig;
            _schemaRegistryConfig = schemaRegistryConfig;
            _serializer = serializer;
        }

        public async Task ProduceAsync(string topic, string key, T message)
        {
            using (var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig))
            using (var producer =
                new ProducerBuilder<string, T>(_producerConfig)
                    .SetValueSerializer(_serializer)
                    .Build())
            {
                await producer.ProduceAsync(topic, new Message<string, T> { Key = key, Value = message })
                        .ContinueWith(task => task.IsFaulted
                            ? $"error producing message: {task.Exception.Message}"
                            : $"produced to: {task.Result.TopicPartitionOffset}");
            }
        }
    }
}
