using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.KafkaClient;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.Consumers.Retry
{
    public class RunTests
    {
        [Fact]
        public void Should_StartComsumption()
        {
            var consumerWrapperMock = new Mock<IKafkaConsumerWrapper<byte[]>>();
            var routerMock = new Mock<IRouter>();
            routerMock.SetupGet(x => x.RetryTopics).Returns(new List<string>() { "topic1" });
            var loggerMock = new Mock<ILogger<Consumer>>();
            var topicTypes = new Dictionary<string, Type>();
            var programConfiguration = new ConsumerAgentConfiguration()
            {
                RetryConsumerEnabled = true
            };
            var retryDelaySeconds = 10;
            CancellationToken cancellationToken = new CancellationTokenSource().Token;

            var consumer = new RetryConsumer("", programConfiguration, consumerWrapperMock.Object, routerMock.Object, loggerMock.Object, retryDelaySeconds);
            consumer.Run();
            
            Task.Delay(10).Wait();

            consumerWrapperMock.Verify(x => x.StartConsumption(
                It.IsAny<List<string>>(),
                It.IsAny<Action<ConsumeResult<string, byte[]>>> (),
                It.Is<CancellationToken>(_ => _.ToString().Equals(cancellationToken.ToString())), 
                TimeSpan.FromSeconds(retryDelaySeconds)
                ), Times.Once);
        }

        [Fact]
        public void Should_Not_Call_StartComsumption_When_DefaultTopics_Is_Empty()
        {
            var consumerWrapperMock = new Mock<IKafkaConsumerWrapper<byte[]>>();
            var routerMock = new Mock<IRouter>();
            routerMock.SetupGet(x => x.RetryTopics).Returns(new List<string>());
            var loggerMock = new Mock<ILogger<Consumer>>();
            var topicTypes = new Dictionary<string, Type>();
            var programConfiguration = new ConsumerAgentConfiguration()
            {
                RetryConsumerEnabled = true
            };
            var consumer = new RetryConsumer("", programConfiguration, consumerWrapperMock.Object, routerMock.Object, loggerMock.Object, retryDelayTimeSeconds:10);
            consumer.Run();

            Task.Delay(10).Wait();
            consumerWrapperMock.Verify(x => x.StartConsumption(It.IsAny<List<string>>(), It.IsAny<Action<ConsumeResult<string, byte[]>>> (), null, It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public void Should_Not_Call_StartComsumption_When_DefaultConsumerEnabled_Is_False()
        {
            var consumerWrapperMock = new Mock<IKafkaConsumerWrapper<byte[]>>();
            var routerMock = new Mock<IRouter>();
            var loggerMock = new Mock<ILogger<Consumer>>();
            var topicTypes = new Dictionary<string, Type>();
            var programConfiguration = new ConsumerAgentConfiguration()
            {
                RetryConsumerEnabled = false
            };
            var consumer = new RetryConsumer("", programConfiguration, consumerWrapperMock.Object, routerMock.Object, loggerMock.Object, retryDelayTimeSeconds: 10);
            consumer.Run();

            Task.Delay(10).Wait();
            consumerWrapperMock.Verify(x => x.StartConsumption(It.IsAny<List<string>>(), It.IsAny<Action<ConsumeResult<string, byte[]>>> (), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan>()), Times.Never);
        }
    }
}
