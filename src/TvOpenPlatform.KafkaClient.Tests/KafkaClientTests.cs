using System;
using System.Collections.Generic;
using System.Text;
using TvOpenPlatform.KafkaClient.Models;
using TvOpenPlatform.KafkaClient.Exceptions;
using Xunit;
using Confluent.Kafka;
using TvOpenPlatform.Logger;
using Moq;
using TvOpenPlatform.KafkaClient.Producer;
using ProducerConfig = TvOpenPlatform.KafkaClient.Models.ProducerConfig;

namespace TvOpenPlatform.KafkaClient.Tests
{
    public class KafkaClientTests
    {
        private readonly string _liveTestTopic = "gvp.test";
        private readonly ILogger _logger = new Logger.DebugLogger();

        [Fact(Skip = "Live Test")]
        public void Should_PostContentToDevServer_ThroughClearCacheFox_ChangingConfiguration()
        {
            var kafkaClient = new KafkaClearCacheFox.KafkaClearCacheFox(this.BuildProducerConfig1(), _logger);

            var data = new KafkaClearCacheFox.Models.ClearCacheFoxData()
            {
                Key = "Teste",
                Topic = _liveTestTopic,
                Events = new KafkaClearCacheFox.Models.ClearCacheFoxEvent()
                {
                    country_code = "br",
                    ob = "1",
                    operation = "mib/cache",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    transaction_id = Guid.NewGuid().ToString(),
                    user_id = "1234"
                }
            };

            var teste = KafkaClearCacheFox.KafkaClearCacheFox.GetInstance(null);

            kafkaClient.Produce(data);

            //-----------------------------------------------------------------

            var kafkaClient1 = new KafkaClearCacheFox.KafkaClearCacheFox(this.BuildProducerConfig2(), _logger);

            var newData = new KafkaClearCacheFox.Models.ClearCacheFoxData()
            {
                Key = "Teste",
                Topic = _liveTestTopic,
                Events = new KafkaClearCacheFox.Models.ClearCacheFoxEvent()
                {
                    country_code = "br",
                    ob = "2",
                    operation = "mib/cache",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    transaction_id = Guid.NewGuid().ToString(),
                    user_id = "1234"
                }
            };

            var teste1 = KafkaClearCacheFox.KafkaClearCacheFox.GetInstance(null);

            kafkaClient1.Produce(newData);
            kafkaClient1.Produce(newData);
        }


        [Fact]
        public void Should_PostContentToDevServer()
        {
            var config = this.BuildProducerConfig1("kafka.gvp-dev.com:30300");
            var kafkaClient = KafkaProducerWrapper.Init(config, _logger);

            for (var i = 0; i < 10; i++)
            {                
                kafkaClient.Produce("AutomaticCreationTopic", "KEY", "1", false);                
            }
            kafkaClient.Flush(-1);
        }

        [Fact(Skip = "Live Test")]
        public void Should_PostContentToDevServer_When_IsConfigRenewed()
        {
            for (var i = 0; i < 10; i++)
            {
                var value = "1";
                Models.ProducerConfig config;
                if (i % 2 == 0)
                    config = this.BuildProducerConfig1();
                else
                {
                    config = this.BuildProducerConfig2();
                    value = "2";
                }

                var kafkaClient = KafkaProducerWrapper.Init(config, _logger);
                kafkaClient.Produce(_liveTestTopic, "KEY", value);
            }
        }

        [Fact(Skip = "Live Test")]
        public void Should_PostContentToDevServer_ThroughClearCacheFox()
        {
            var kafkaClient = new KafkaClearCacheFox.KafkaClearCacheFox(this.BuildProducerConfig1(), _logger);

            var data = new KafkaClearCacheFox.Models.ClearCacheFoxData()
            {
                Key = "Teste",
                Topic = _liveTestTopic,
                Events = new KafkaClearCacheFox.Models.ClearCacheFoxEvent()
                {
                    country_code = "br",
                    ob = "123",
                    operation = "mib/cache",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    transaction_id = Guid.NewGuid().ToString(),
                    user_id = "1234"
                }
            };

            kafkaClient.Produce(data);
            kafkaClient.Flush(-1);
        }

        [Fact]
        public void Should_Not_Call_LogMessage_With_Headers_When_VerboseEnabled_Is_False()
        {
            var loggerMock = new Mock<ILogger>();
            var logMessage = new Mock<ILoggerHelper>();
            var produceConfig = new ProducerConfig();
            var kafkaClient = new KafkaProducerWrapper(produceConfig, Mock.Of<IProducer<string, string>>(), logMessage.Object, loggerMock.Object);
            KafkaProducerWrapper.VerboseEnabled = false;
            KafkaProducerWrapper.Serializer = new Mock<IProducerSerializer>().Object;

            var headers = new Mock<Dictionary<string, byte[]>>();            
            var message = new MessageWrapper<string>(new Topic("topic"), new Key("key"), "message", new MessageHeaders(headers.Object));
            kafkaClient.ProduceAndAwaitDeliveryAsync(message);

            logMessage.Verify(x => x.LogMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Headers>()), Times.Never);
        }

        [Fact]
        public void Should_Call_LogMessage_With_Headers_When_VerboseEnabled_Is_True()
        {
            var loggerMock = new Mock<ILogger>();
            var logMessage = new Mock<ILoggerHelper>();
            var produceConfig = new ProducerConfig();
            var kafkaClient = new KafkaProducerWrapper(produceConfig, Mock.Of<IProducer<string, string>>(), logMessage.Object, loggerMock.Object);
            KafkaProducerWrapper.VerboseEnabled = true;
            KafkaProducerWrapper.Serializer = new Mock<IProducerSerializer>().Object;

            var headers = new Mock<Dictionary<string, byte[]>>();
            var message = new MessageWrapper<string>(new Topic("topic"), new Key("key"), "message", new MessageHeaders(headers.Object));
            kafkaClient.ProduceAndAwaitDeliveryAsync(message);

            logMessage.Verify(x => x.LogMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Headers>()), Times.AtLeastOnce);
        }

        [Fact]
        public void Should_Not_Flush_When_Instance_Is_Null()
        {
            var loggerMock = new Mock<ILogger>();
            var kafkaClient = new KafkaClearCacheFox.KafkaClearCacheFox((IKafkaProducerWrapper)null, loggerMock.Object);
            kafkaClient.Flush(-1);

            loggerMock.Verify(x => x.Log(LogLevel.Verbose, "Flush", It.IsAny<string>()), Times.Never);
        }

        [Fact(Skip = "Live Test")]
        public void Should_ThrownKafkaUnavailableException_WhenKafkaIsUnavailable()
        {
            var config = this.BuildProducerConfig1(bootstrapServers: Guid.NewGuid().ToString(), queueBufferingMaxMessages: 1);
            var kafkaClient = KafkaProducerWrapper.Init(config, _logger);
            Assert.Throws<KafkaUnavailableException>(() => kafkaClient.Produce(_liveTestTopic, "KEY", "1", isDeliveryReportEnabled: true));
        }

        [Fact]
        public void IsConfigRenewed_Should_Return_False_When_Config_IsEqual()
        {
            var producerConfig = BuildProducerConfig1();

            var oldConfig = new Confluent.Kafka.ProducerConfig()
            {
                SocketSendBufferBytes = producerConfig.SocketSendBufferBytes,
                SocketReceiveBufferBytes = producerConfig.SocketReceiveBufferBytes,
                SocketKeepaliveEnable = producerConfig.SocketKeepAliveEnable,
                SocketNagleDisable = producerConfig.SocketNagleDisable,
                CompressionType = (CompressionType)producerConfig.CompressionCodec,
                BootstrapServers = producerConfig.BootstrapServers,
                SocketTimeoutMs = producerConfig.SocketTimeoutMs,
                MessageSendMaxRetries = producerConfig.MessageSendMaxRetries,
                QueueBufferingMaxMessages = producerConfig.QueueBufferingMaxMessages,
                LingerMs = producerConfig.QueueBufferingMaxMs,
                MessageTimeoutMs = producerConfig.MessageTimeout,
                Acks = Acks.All,
                EnableIdempotence = true,                
                TopicMetadataRefreshIntervalMs = producerConfig.TopicMetadataRefreshIntervalMs,
                Debug = producerConfig.Debug,
                SocketMaxFails = producerConfig.SocketMaxFails,
                LogConnectionClose = producerConfig.LogConnectionClose,
                TopicBlacklist = producerConfig.TopicBlacklist,
                MessageMaxBytes = producerConfig.MessageMaxBytes,
                StatisticsIntervalMs = producerConfig.StatisticsIntervalMs
            };
            var isConfigRenewed = Tools.Tools.IsConfigRenewed(oldConfig, producerConfig);

            Assert.False(isConfigRenewed);
        }

        [Fact]
        public void IsConfigRenewed_Should_Return_True_When_Config_IsNotEqual()
        {
            var producerConfig = BuildProducerConfig1();

            var oldConfig = new Confluent.Kafka.ProducerConfig()
            {
                SocketSendBufferBytes = producerConfig.SocketSendBufferBytes,
                SocketReceiveBufferBytes = producerConfig.SocketReceiveBufferBytes,
                SocketKeepaliveEnable = producerConfig.SocketKeepAliveEnable,
                SocketNagleDisable = producerConfig.SocketNagleDisable,
                CompressionType = (CompressionType)producerConfig.CompressionCodec,
                BootstrapServers = producerConfig.BootstrapServers,
                SocketTimeoutMs = producerConfig.SocketTimeoutMs,
                MessageSendMaxRetries = producerConfig.MessageSendMaxRetries,
                QueueBufferingMaxMessages = producerConfig.QueueBufferingMaxMessages,
                LingerMs = producerConfig.QueueBufferingMaxMs,
                MessageTimeoutMs = producerConfig.MessageTimeout,
                Acks = Acks.None,
                EnableIdempotence = true,
                TopicMetadataRefreshIntervalMs = producerConfig.TopicMetadataRefreshIntervalMs,
                Debug = producerConfig.Debug,
                SocketMaxFails = producerConfig.SocketMaxFails,
                LogConnectionClose = producerConfig.LogConnectionClose,
                TopicBlacklist = producerConfig.TopicBlacklist,
                MessageMaxBytes = producerConfig.MessageMaxBytes,
                StatisticsIntervalMs = producerConfig.StatisticsIntervalMs
            };


            var isConfigRenewed = Tools.Tools.IsConfigRenewed(oldConfig, producerConfig);

            Assert.True(isConfigRenewed);
        }

        [Fact]
        public void Init_Should_Return_New_KafkaClient()
        {
            var kafkaClient =
                KafkaProducerWrapper.Init(BuildProducerConfig1(), _logger);

            Assert.IsType<KafkaProducerWrapper>(kafkaClient);
        }

        [Fact]
        public void Init_Should_Return_Existent_KafkaClient()
        {
            var expectedKafkaClient =
                KafkaProducerWrapper.Init(BuildProducerConfig1(), _logger);

            var actualKafkaClient = KafkaProducerWrapper.Init(BuildProducerConfig1(), _logger);
            Assert.Equal(expectedKafkaClient, actualKafkaClient);
        }

        [Fact]
        public void Dispose_Then_Init_Should_Return_New_KafkaClient()
        {
            var expectedKafkaClient =
                KafkaProducerWrapper.Init(BuildProducerConfig1(), _logger);

            expectedKafkaClient.Dispose();

            var actualKafkaClient = KafkaProducerWrapper.Init(BuildProducerConfig1(), _logger);
            Assert.NotEqual(expectedKafkaClient, actualKafkaClient);
        }

        [Fact(Skip = "Live Test")]
        public void Should_ThrowKafkaUnavailableException_WhenProducingAndKafkaIsUnrecheableOrUnavailable()
        {
            var kafkaProducerWrapper = KafkaProducerWrapper.Init(BuildProducerConfig1(bootstrapServers: Guid.NewGuid().ToString(), queueBufferingMaxMessages: 1), null);
            Assert.Throws<KafkaUnavailableException>(() => kafkaProducerWrapper.Produce("gvp-amazon-sync", Guid.NewGuid().ToString(), "Value", true));
        }

        private Models.ProducerConfig BuildProducerConfig1(string bootstrapServers = "localhost:9092", int queueBufferingMaxMessages = 11)
        {
            return new Models.ProducerConfig()
            {
                BootstrapServers = bootstrapServers,
                CompressionCodec = CompressionCodecEnum.None,
                MessageSendMaxRetries = 3,
                SocketTimeoutMs = 20000,
                SocketKeepAliveEnable = false,
                SocketNagleDisable = false,
                SocketReceiveBufferBytes = 0,
                SocketSendBufferBytes = 0,
                AckMode = DefaultTopicConfigAckModeEnum.All.GetStringValue(),
                QueueBufferingMaxMessages = queueBufferingMaxMessages,
                QueueBufferingMaxMs = 2000,
                MessageTimeout = 3000,
                MessageMaxBytes = 30000,
                Debug = "broker,topic,msg"

            };
        }

        private Models.ProducerConfig BuildProducerConfig2(string bootstrapServers = "34.234.9.61:5192", int queueBufferingMaxMessages = 1)
        {
            return new Models.ProducerConfig()
            {
                BootstrapServers = bootstrapServers,
                CompressionCodec = CompressionCodecEnum.None,
                MessageSendMaxRetries = 3,
                SocketTimeoutMs = 2000,
                SocketKeepAliveEnable = true,
                SocketNagleDisable = false,
                SocketReceiveBufferBytes = 0,
                SocketSendBufferBytes = 0,
                AckMode = DefaultTopicConfigAckModeEnum.All.GetStringValue(),
                QueueBufferingMaxMessages = 1,
                QueueBufferingMaxMs = 1000,
                MessageTimeout = 3000,
                MessageMaxBytes = 30000
            };
        }

    }
}

