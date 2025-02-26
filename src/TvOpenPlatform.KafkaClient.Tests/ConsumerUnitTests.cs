using Confluent.Kafka;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Consumer;
using TvOpenPlatform.KafkaClient.Models;
using TvOpenPlatform.Logger;
using Xunit;

namespace TvOpenPlatform.KafkaClient.Tests
{
    public class ConsumerUnitTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<IKafkaConsumerBuilder> _builderMock;
        private Mock<IConsumer<string, string>> _consumerMock;
        private Models.ConsumerConfig _consumerWrapperConfig;

        public ConsumerUnitTests()
        {
            _loggerMock = new Mock<ILogger>();
            _builderMock = new Mock<IKafkaConsumerBuilder>();
            _consumerMock = new Mock<IConsumer<string, string>>();
            _consumerWrapperConfig = BuildConsumerConfig();
        }

        [Fact]
        public void Should_Build_IConsumer()
        {
            var builder = new KafkaConsumerBuilder();
            var config = Tools.Tools.SetUpConsumerConfig(_consumerWrapperConfig);
            var consumer = builder.Build(config,
                new Action<Error>((_) => { }),
                new Action<List<TopicPartition>>((_) => { }),
                new Action<List<TopicPartitionOffset>>((_) => { })
                );

            Assert.IsAssignableFrom<IConsumer<string, string>>(consumer);
        }

        [Fact]
        public void Should_Rebuild_Consumer_When_Restart_Is_Enabled()
        {
            //Arrange
            var topic = "fake-topic";
            var consumeResultFake = new ConsumeResult<string, string>()
            {
                Message = new Message<string, string>()
            };
            _consumerMock.Setup(_ => _.Consume(It.IsAny<TimeSpan>())).Returns(consumeResultFake);
            _consumerMock.Setup(_ => _.Commit(consumeResultFake)).Throws(new KafkaException(new Error(ErrorCode.InvalidCommitOffsetSize, "")));
            _consumerMock.SetupGet(_ => _.Assignment).Returns(new List<TopicPartition>()
            {
                new TopicPartition(topic, Partition.Any)
            });

            _builderMock.Setup(_ =>
            _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>(), It.IsAny<ILogger>())
            ).Returns(_consumerMock.Object);

            var consumer = new KafkaConsumerWrapper<string>(_consumerWrapperConfig, _builderMock.Object, _loggerMock.Object);

            //Act
            Assert.Throws<KafkaException>(() => consumer.StartConsumption(
                new List<string> { topic },
                (ConsumeResult<string, string> consumeResult) => { },
                delayTime: null));

            //Assert
            _builderMock.Verify(_ => _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>(),
                It.IsAny<ILogger>()

                ), Times.Exactly(_consumerWrapperConfig.MaxConsecutiveRestartAttempts + 1));
        }

        [Fact]
        public void Should_Not_Rebuild_Consumer_When_Restart_Is_Disabled()
        {
            //Arrange
            var topic = "fake-topic";

            var consumeResultFake = new ConsumeResult<string, string>()
            {
                Message = new Message<string, string>()
            };

            _consumerMock.Setup(_ => _.Consume(It.IsAny<TimeSpan>())).Returns(consumeResultFake);
            _consumerMock.Setup(_ => _.Commit(consumeResultFake)).Throws(new KafkaException(new Error(ErrorCode.InvalidCommitOffsetSize, "")));
            _consumerMock.SetupGet(_ => _.Assignment).Returns(new List<TopicPartition>()
            {
                new TopicPartition(topic, Partition.Any)
            });

            _builderMock.Setup(_ =>
            _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>(), It.IsAny<ILogger>())
            ).Returns(_consumerMock.Object);

            _consumerWrapperConfig.ShouldTryRestartIfConsumeFails = false;

            var consumer = new KafkaConsumerWrapper<string>(_consumerWrapperConfig, _builderMock.Object, _loggerMock.Object);

            //Act
            Assert.Throws<KafkaException>(() => consumer.StartConsumption(
                new List<string> { topic },
                (ConsumeResult<string, string> consumeResult) => { },
                delayTime: null));

            //Assert
            _builderMock.Verify(_ => _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>()
                ), Times.AtMostOnce);
        }

        [Fact]
        public void Should_Commit_When_Message_Is_Not_NUll()
        {
            //Arrange
            var topic = "fake-topic";
            _builderMock.Setup(_ =>
            _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>(), It.IsAny<ILogger>())
            ).Returns(_consumerMock.Object);

            var consumeResultFake = new ConsumeResult<string, string>()
            {
                Message = new Message<string, string>()
            };

            _consumerMock.Setup(_ => _.Consume(It.IsAny<CancellationToken>())).Returns(consumeResultFake);
            var cts = new CancellationTokenSource();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(_consumerWrapperConfig, _builderMock.Object, _loggerMock.Object);

            var messageHandler = new Action<ConsumeResult<string, string>>((_) => { });

            var task = new Task(() =>
            {
                kafkaConsumerWrapper.StartConsumption(new List<string>() { topic }, messageHandler, new CancellationTokenSource().Token, null);
            }, cts.Token);

            messageHandler = new Action<ConsumeResult<string, string>>((_) =>
            {
                kafkaConsumerWrapper.CommitOffset(consumeResultFake);
            });

            //Act
            task.Start();
            task.Wait(300);

            //Assert
            _consumerMock.Verify(x => x.Commit(consumeResultFake), Times.AtLeastOnce);
            cts.Cancel();
        }

        [Fact]
        public void Should_RestartConsumer_When_Commit_Throws_KafkaException_With_ErrorCode_UnknownMemberId()
        {
            //Arrange
            var topic = "fake-topic";
            _builderMock.Setup(_ =>
            _.Build(
                It.IsAny<Confluent.Kafka.ConsumerConfig>(),
                It.IsAny<Action<Error>>(),
                It.IsAny<Action<List<TopicPartition>>>(),
                It.IsAny<Action<List<TopicPartitionOffset>>>(), It.IsAny<ILogger>())
            ).Returns(_consumerMock.Object);

            var consumeResultFake = new ConsumeResult<string, string>()
            {
                Message = new Message<string, string>()
            };

            _consumerMock.Reset();
            _consumerMock.Setup(_ => _.Consume(It.IsAny<CancellationToken>())).Returns(consumeResultFake);
            _consumerMock.Setup(_ => _.Commit(It.IsAny<ConsumeResult<string, string>>())).Throws(
                new KafkaException(ErrorCode.UnknownMemberId));

            var cts = new CancellationTokenSource();
            var kafkaConsumerWrapper = new KafkaConsumerWrapper<string>(_consumerWrapperConfig, _builderMock.Object, _loggerMock.Object);

            var messageHandler = new Action<ConsumeResult<string, string>>((_) => { });

            var task = new Task(() =>
            {
                Assert.Throws<KafkaException>(() =>
                {
                    kafkaConsumerWrapper.StartConsumption(new List<string>() { topic }, messageHandler, new CancellationTokenSource().Token, null);
                });
            }, cts.Token);

            messageHandler = new Action<ConsumeResult<string, string>>((_) =>
            {
                kafkaConsumerWrapper.CommitOffset(consumeResultFake);
            });

            //Act
            task.Start();
            task.Wait(300);
            
            //Assert
            _consumerMock.Verify(x => x.Close(), Times.AtLeastOnce);
            _consumerMock.Verify(x => x.Dispose(), Times.AtLeastOnce);
            cts.Cancel();
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
    }
}
