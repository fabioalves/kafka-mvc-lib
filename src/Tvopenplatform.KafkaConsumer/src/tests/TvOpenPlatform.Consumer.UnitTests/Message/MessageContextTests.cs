using Confluent.Kafka;
using System;
using System.Text;
using TvOpenPlatform.Consumer.Message;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.Message
{
    public class MessageContextTests
    {
        private const string DefaultTopic = "some_default_topic_value";
        private const string DefaultKey = "some_default_key_value";
        private const string DefaultEventId = "some_default_event_id";

        [Fact]
        public void Constructor_Should_Set_Topic_Correctly()
        {
            //Arrange
            var expectedTopic = Guid.NewGuid().ToString();

            //Act
            var unitUnderTest = new MessageContext(expectedTopic, DefaultKey, CreateDefaultHeaders(), DefaultEventId);

            //Assert
            Assert.Equal(expectedTopic, unitUnderTest.Topic);
        }

        [Fact]
        public void Constructor_Should_Set_Key_Correctly()
        {
            //Arrange
            var expectedKey = Guid.NewGuid().ToString();

            //Act
            var unitUnderTest = new MessageContext(DefaultTopic, expectedKey, CreateDefaultHeaders(), DefaultEventId);

            //Assert
            Assert.Equal(expectedKey, unitUnderTest.Key);
        }

        [Fact(Skip = "Must verify if needed, property is private only")]
        public void Constructor_Should_Set_EventId_Correctly()
        {
            //Arrange
            var expectedEventId = Guid.NewGuid().ToString();

            //Act
            var unitUnderTest = new MessageContext(DefaultTopic, DefaultKey, CreateDefaultHeaders(), expectedEventId);

            //Assert
            Assert.Equal(expectedEventId, unitUnderTest.Key);
        }

        [Fact]
        public void Constructor_Should_Set_CorrelationId_Correctly()
        {
            //Arrange
            var expectedValue = Guid.NewGuid().ToString();

            var headers = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes(expectedValue) }
            };

            //Act
            var unitUnderTest = new MessageContext(DefaultTopic, DefaultKey, headers, DefaultEventId);

            //Assert
            Assert.Equal(expectedValue, unitUnderTest.CorrelationId);
        }

        [Fact]
        public void Constructor_Should_Set_CorrelationId_As_Null_When_Not_PresentInHeaders()
        {
            //Act
            var unitUnderTest = new MessageContext(DefaultTopic, DefaultKey, new Headers(), DefaultEventId);

            //Assert
            Assert.Null(unitUnderTest.CorrelationId);
        }

        [Fact]
        public void SetCorrelationId_Should_Do_Nothing_When_CorrelationId_Is_Not_NullOrWhitespace()
        {
            //Arrange
            var expectedValue = Guid.NewGuid().ToString();

            var headers = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes(expectedValue) }
            };

            var unitUnderTest = new MessageContext(DefaultTopic, DefaultKey, headers, DefaultEventId);

            //Act
            unitUnderTest.SetCorrelationId(null);

            //Assert
            Assert.Equal(expectedValue, unitUnderTest.CorrelationId);
        }

        private Headers CreateDefaultHeaders()
        {
            var result = new Headers
            {
                { "correlationId", Encoding.UTF8.GetBytes("some_correlation_id_value") }
            };

            return result;
        }
    }
}
