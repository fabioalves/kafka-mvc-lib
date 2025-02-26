using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Consumer;
using TvOpenPlatform.KafkaClient.Models;
using TvOpenPlatform.KafkaClient.Tests.Producer.SchemaRegistry;
using TvOpenPlatform.Logger;
using Xunit;

namespace TvOpenPlatform.KafkaClient.Tests
{
    public class ConsumerIntegrationTests
    {
        private List<string> MessagesConsumed = new List<string>();
        private readonly ILogger _logger = new DebugLogger();

        [Fact(Skip = "Live Test")]
        public void ShouldConsumeByte()
        {
            var topic = "gvp-test-json-4";
            var kafkaConsumerBuilder = new KafkaConsumerBuilder<byte[]>();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<byte[]>(BuildConsumerConfig(), kafkaConsumerBuilder, _logger);
            kafkaConsumerWrapper.StartConsumption(new List<string> { topic }, MessageHandlerByte);
        }


        [Fact(Skip = "Live Test")]
        public async Task ShouldProduceWithSchemaProducer()
        {
            var topic = "gvp-test-json-4";
            var producerConfig = new Confluent.Kafka.ProducerConfig()
            {
                BootstrapServers = "localhost:9092"
            };
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = "http://localhost:8085"
            };
            var jsonSerializerConfig = new JsonSerializerConfig()
            {
                BufferBytes = 100
            };

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            {
                var serializer = new JsonSerializer<Teste>(schemaRegistry, jsonSerializerConfig);

                var producer = new Producer<Teste>(producerConfig, schemaRegistryConfig, serializer);
                await producer.ProduceAsync(topic, Guid.NewGuid().ToString(), new Teste
                {
                    firstName = "test",
                    lastName = "surname",
                    age = 30
                });
            }
        }

        [Fact(Skip = "LiveTest")]
        public void ShouldConsumeWithJsonDeserialization()
        {
            var topic = "gvp-test-json-3";
            var kafkaConsumerBuilder = new KafkaConsumerBuilder<Teste>();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<Teste>(BuildConsumerConfig(), kafkaConsumerBuilder, _logger);
            kafkaConsumerWrapper.StartConsumption(new List<string> { topic }, MessageHandler<Teste>);
        }

        private void MessageHandler<T>(ConsumeResult<string, T> consumeResult)
        {
            Console.WriteLine("test");
        }

        private void MessageHandlerByte(ConsumeResult<string, byte[]> consumeResult)
        {
            var messageValue = Deserialize<Teste>(consumeResult.Message.Value);
            Console.WriteLine("test");
        }
        public static T Deserialize<T>(byte[] data) where T : class, new()
        {
            return new JsonDeserializer<T>().DeserializeAsync(new ReadOnlyMemory<byte>(data), false, SerializationContext.Empty).Result;
        }

        [Fact(Skip = "Live Test")]
        public void ShouldConsumeCreatedMessagesInTopic()
        {
            var mainTopic = "gvp.test." + Guid.NewGuid().ToString();
            var kafkaConsumerBuilder = new KafkaConsumerBuilder<string>();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(BuildConsumerConfig(), kafkaConsumerBuilder, _logger);

            var task = Task.Run(() =>
                kafkaConsumerWrapper.StartConsumption(new List<string>() { mainTopic }, MessageHandler, null));

            int numberOfMessages = 5;
            ProduceTestMessages(numberOfMessages, mainTopic);
            task.Wait(10000);

            Assert.Equal(numberOfMessages, MessagesConsumed.Count);
        }

        private void MessageHandler(ConsumeResult<string, string> consumeResult)
        {
            this.MessagesConsumed.Add(consumeResult.Message.Value);
        }

        [Fact(Skip = "Live Test")]
        public void ShouldConsumeThenRetry()
        {
            //CONSUMING
            var mainTopic = "gvp.test." + Guid.NewGuid().ToString();
            var kafkaConsumerBuilder = new KafkaConsumerBuilder<string>();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(BuildConsumerConfig(), kafkaConsumerBuilder, _logger);
            var task = Task.Run(() =>
                kafkaConsumerWrapper.StartConsumption(new List<string>() { mainTopic }, MessageHandler, null, null));

            int numberOfMessages = 5;
            ProduceTestMessages(numberOfMessages, mainTopic);

            task.Wait(20000);
            Assert.Equal(numberOfMessages, MessagesConsumed.Count);

            //RETRYING
            this.MessagesConsumed = new List<string>();
            var retryTopic = "gvp.test." + Guid.NewGuid().ToString() + ".retry";
            var kafkaConsumerWrapperRetry = new KafkaConsumerWrapper<string>(BuildConsumerConfig(), kafkaConsumerBuilder, _logger);
            var taskRetry = Task.Run(() =>
            kafkaConsumerWrapperRetry.StartConsumption(new List<string>() { mainTopic }, MessageHandler, null, TimeSpan.FromSeconds(2)));

            var retryMsg = MessagesConsumed.Take(2).ToList();
            ProduceTestMessages(retryMsg.Count, retryTopic, retryMsg);
            taskRetry.Wait(15000);

            Assert.Equal(retryMsg.Count, MessagesConsumed.Count);
        }

        private void ProduceTestMessages(int numberOfMessages, string topic, List<string> value = null)
        {
            var kafkaProducerWrapper = KafkaProducerWrapper.Init(BuildProducerConfig(), _logger);

            for (var i = 0; i < numberOfMessages; i++)
            {
                var messageValue = value != null ? value[i] : "valor" + i;
                kafkaProducerWrapper.Produce(topic, Guid.NewGuid().ToString(), messageValue, true);
            }
            kafkaProducerWrapper.Flush(-1);
        }

        private Models.ConsumerConfig BuildConsumerConfig()
        {
            var config = new Models.ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "exampleconsumer4",
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = "earliest",
                EnablePartitionEof = true,
                CommitAtferHandlingMessage = true,
                Debug = "consumer",
                SocketTimeoutMs = 6000,
                MaxPollIntervalMs = 6000,
                AckMode = DefaultTopicConfigAckModeEnum.All.GetStringValue(),
                ShouldTryRestartIfConsumeFails = true,
                MaxConsecutiveRestartAttempts = 3
            };
            return config;
        }

        private Models.ProducerConfig BuildProducerConfig(string bootstrapServers = "localhost:9092", int queueBufferingMaxMessages = 5)
        {
            return new Models.ProducerConfig()
            {
                BootstrapServers = bootstrapServers,
                CompressionCodec = CompressionCodecEnum.None,
                MessageSendMaxRetries = 3,
                SocketTimeoutMs = 2000,
                SocketKeepAliveEnable = false,
                SocketNagleDisable = false,
                SocketReceiveBufferBytes = 0,
                SocketSendBufferBytes = 0,
                AckMode = DefaultTopicConfigAckModeEnum.All.GetStringValue(),
                QueueBufferingMaxMessages = queueBufferingMaxMessages,
                QueueBufferingMaxMs = 2000,
                MessageTimeout = 3000
            };
        }
    }

    public class Teste
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int age { get; set; }

    }
}
