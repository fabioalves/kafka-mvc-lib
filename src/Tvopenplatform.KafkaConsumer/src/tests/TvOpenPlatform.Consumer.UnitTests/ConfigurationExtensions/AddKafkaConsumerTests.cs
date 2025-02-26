using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using TvOpenPlatform.Consumer.Configuration;
using TvOpenPlatform.Consumer.Consumers;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.KafkaClient;
using TvOpenPlatform.KafkaClient.Consumer;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.ConfigurationExtensions
{
    public class AddKafkaConsumerTests
    {
        private readonly ServiceCollection _services;
        private readonly ServiceProvider _provider;

        public AddKafkaConsumerTests()
        {
            ConsumerAgentConfiguration programConfiguration = SetupProgramConfiguration();

            _services = new ServiceCollection();
            _services.AddLogging();
            _services.AddKafkaConsumer<AddKafkaConsumerTests>(programConfiguration);
            _provider = _services.BuildServiceProvider();
        }

        [Fact]
        public void Should_Contains_Singleton_Service_Of_ProgramConfiguration()
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(ConsumerAgentConfiguration) && 
                        item.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_Contains_Singleton_Service_Of_ConsumerConfig()
        {
            var type = typeof(IConfigureOptions<KafkaClient.Models.ConsumerConfig>);
            Assert.Contains(_services,
                item => item.ServiceType == type && 
                        item.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_Contains_Transient_Service_Of_IKafkaConsumerBuilder()
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(IKafkaConsumerBuilder<byte[]>) &&
                        item.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void Should_Contains_Transient_Service_Of_IKafkaConsumerWrapper()
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(IKafkaConsumerWrapper<byte[]>) &&
                        item.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void Should_Contains_Singleton_Service_Of_ITopicDeserializer()
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(ITopicDeserializer) &&
                        item.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_Contains_Singleton_Service_Of_IRouter()
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(IRouter) &&
                        item.Lifetime == ServiceLifetime.Singleton);
        }

        [Theory]
        [InlineData(typeof(DefaultConsumer))]
        [InlineData(typeof(RetryConsumer))]
        public void Should_Contains_Transient_Service_Of_IConsumer(Type implementationType)
        {
            Assert.Contains(_services,
                item => item.ServiceType == typeof(IConsumer) &&
                        item.ImplementationFactory.Method.ReturnType == implementationType &&
                        item.Lifetime == ServiceLifetime.Transient);

        }

        [Fact]
        public void Should_BuildServiceProvider()
        {
            Assert.IsAssignableFrom<IServiceProvider>(_provider);
        }

        [Fact]
        public void Should_GetService_ProgramConfiguration()
        {
            var programConfiguration = _provider.GetService<ConsumerAgentConfiguration>();
            Assert.IsType<ConsumerAgentConfiguration>(programConfiguration);
        }

        [Fact]
        public void Should_GetService_ConsumerConfig()
        {
            var instance = _provider.GetService<IConfigureOptions<KafkaClient.Models.ConsumerConfig>>();
            Assert.IsAssignableFrom<IConfigureOptions<KafkaClient.Models.ConsumerConfig>>(instance);
        }

        [Fact]
        public void Should_GetService_IKafkaConsumerBuilder()
        {
            var instance = _provider.GetService<IKafkaConsumerBuilder<byte[]>>();
            Assert.IsAssignableFrom<IKafkaConsumerBuilder<byte[]>>(instance);
        }

        [Fact]
        public void Should_GetService_IKafkaConsumerWrapper()
        {
            var instance = _provider.GetService<IKafkaConsumerWrapper<byte[]>>();
            Assert.IsAssignableFrom<IKafkaConsumerWrapper<byte[]>>(instance);
        }

        [Fact]
        public void Should_GetService_ITopicDeserializer()
        {
            var instance = _provider.GetService<ITopicDeserializer>();
            Assert.IsAssignableFrom<ITopicDeserializer>(instance);
        }

        [Fact]
        public void Should_GetService_IRouter()
        {
            var instance = _provider.GetService<IRouter>();
            Assert.IsAssignableFrom<IRouter>(instance);
        }

        [Fact]
        public void Should_GetServices_IConsumer()
        {
            var instances = _provider.GetServices<IConsumer>();
            Assert.All(
                instances, 
                new Action<IConsumer>(
                    target => Assert.IsAssignableFrom<IConsumer>(target)));
        }

        private static ConsumerAgentConfiguration SetupProgramConfiguration()
        {
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupProperty(config => config.Value, "");

            var programConfiguration = new ConsumerAgentConfiguration()
            {
                ConsumerConfig = configurationSectionMock.Object,
                ExecutingAssembly = Assembly.GetExecutingAssembly()
            };
            return programConfiguration;
        }
    }
}
