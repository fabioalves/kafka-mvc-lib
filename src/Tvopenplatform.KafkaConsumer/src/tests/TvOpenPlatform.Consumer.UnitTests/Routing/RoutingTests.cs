using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.Parameters;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.Consumer.UnitTests.Routing.Helpers;
using TvOpenPlatform.Settings.Athena;
using TvOpenPlatform.Settings.Athena.Model;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.Routing
{
    public class RoutingTests
    {
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new Mock<IServiceProvider>();
        private readonly Mock<ITopicDeserializer> _deserializerMock = new Mock<ITopicDeserializer>();
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private const int ExistingRoutesCount = 7; //Routes that can be mapped in the Fakes 

        [Fact]
        public void ShouldNot_FillMappers()
        {
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object);

            Assert.Empty(router.DefaultTopics);
            Assert.Empty(router.RetryTopics);
        }

        [Fact]
        public void ShouldNot_FillMappers_MultipleTopics()
        {
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object);
            builder.Build(_assembly);
            Assert.Equal(ExistingRoutesCount, router.DefaultTopics.Count());
        }

        [Fact]
        public void Should_FindTopicMapper()
        {
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object);
            builder.Build(_assembly);
            Assert.Contains(Constants.TestTopic, router.DefaultTopics);
        }

        [Fact]
        public void Should_RenameTopicKey_With_AthenaTopic()
        {
            var redirect = "new-name";
            var topicsByMethod = new Dictionary<string, string>();
            topicsByMethod.Add("method", redirect);

            var athenaClient = new Mock<IAthenaClientAsync>();

            SetupAthenaGetParameters(athenaClient, parameterKeyValue: redirect);

            var topicNameParameters = new AthenaTopicNames(athenaClient.Object, _assembly, instanceIds: new List<int> { 25 });
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .WithAthenaRedirect(topicNameParameters)
                .Build(_assembly);

            Assert.Equal(redirect, router.DefaultTopics.FirstOrDefault());
        }

        [Fact]
        public void Should_RenameTopicKey_With_TopicNameParameters()
        {
            var redirect = "new-name";
            var mapping = new Dictionary<string, string>
            {
                {Constants.TestTopic, redirect}
            };
            var topicNameRedirection = new TopicNameRedirection { TopicKeyMapping = mapping };
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .WithRedirect(topicNameRedirection)
                .Build(_assembly);

            Assert.Contains(redirect, router.DefaultTopics);
            Assert.DoesNotContain(Constants.TestTopic, router.DefaultTopics);
        }

        [Fact]
        public void Should_RenameTopicKey_With_AthenaTopic_And_TopicNameParameters()
        {
            var redirectAthena = "new-name-athena";
            var topicsByMethod = new Dictionary<string, string>();
            topicsByMethod.Add("method", redirectAthena);

            var athenaClient = new Mock<IAthenaClientAsync>();

            SetupAthenaGetParameters(athenaClient, parameterKeyValue: redirectAthena);

            var redirectTopicName = "new-name-topic";
            var mapping = new Dictionary<string, string>
            {
                {Constants.TestTopic, redirectTopicName}
            };
            var topicNameRedirection = new TopicNameRedirection { TopicKeyMapping = mapping };

            var athenaTopicNameParameters = new AthenaTopicNames(athenaClient.Object, _assembly, instanceIds: new List<int> { 25 });
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .WithRedirect(topicNameRedirection)
                .WithAthenaRedirect(athenaTopicNameParameters)
                .Build(_assembly);

            Assert.Contains(redirectAthena, router.DefaultTopics);
            Assert.Contains(redirectTopicName, router.DefaultTopics);
        }

        [Fact]
        public void Should_RemoveRenamedTopic_By_TopicNameParameters()
        {
            var redirect = "new-name";
            var mapping = new Dictionary<string, string>
            {
                {Constants.TestTopic, redirect}
            };
            var topicNameRedirection = new TopicNameRedirection { TopicKeyMapping = mapping };

            var router1 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder1 = new RouterConfiguration(router1, _loggerMock.Object)
                .WithRedirect(topicNameRedirection)
                .Build(_assembly);

            var router2 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder2 = new RouterConfiguration(router2, _loggerMock.Object)
                .WithRedirect(topicNameRedirection)
                .Except(redirect)
                .Build(_assembly);

            Assert.Contains(redirect, router1.DefaultTopics);
            Assert.DoesNotContain(redirect, router2.DefaultTopics);
            Assert.Equal(router1.DefaultTopics.Count() - 1, router2.DefaultTopics.Count());
        }

        [Fact]
        public void Should_RemoveTopicKey()
        {
            var router1 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder1 = new RouterConfiguration(router1, _loggerMock.Object)
                .Build(_assembly);


            var router2 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder2 = new RouterConfiguration(router2, _loggerMock.Object)
                .Except(Constants.TestTopic)
                .Build(_assembly);

            Assert.Contains(Constants.TestTopic, router1.DefaultTopics);
            Assert.DoesNotContain(Constants.TestTopic, router2.DefaultTopics);
            Assert.Equal(router1.DefaultTopics.Count() - 1, router1.DefaultTopics.Count() - 1);
        }

        [Fact]
        public void Should_ContainsOnlyOneTopicKey()
        {
            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .FilterBy(Constants.TestTopic)
                .Build(_assembly);

            Assert.Contains(Constants.TestTopic, router.DefaultTopics);
            Assert.Single(router.DefaultTopics);
        }

        [Fact]
        public void Should_RemoveRenamedTopic_By_AthenaTopic()
        {
            var redirect = "new-name";
            var topicsByMethod = new Dictionary<string, string>();
            topicsByMethod.Add("method", redirect);

            var athenaClient = new Mock<IAthenaClientAsync>();

            SetupAthenaGetParameters(athenaClient, parameterKeyValue: redirect);

            var topicNameParameters = new AthenaTopicNames(athenaClient.Object, _assembly, instanceIds: new List<int> { 25 });

            var router1 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder1 = new RouterConfiguration(router1, _loggerMock.Object)
                .WithAthenaRedirect(topicNameParameters)
                .Build(_assembly);

            var router2 = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder2 = new RouterConfiguration(router2, _loggerMock.Object)
                .WithAthenaRedirect(topicNameParameters)
                .Except(redirect)
                .Build(_assembly);

            Assert.Contains(redirect, router1.DefaultTopics);
            Assert.DoesNotContain(redirect, router2.DefaultTopics);
            Assert.Equal(router1.DefaultTopics.Count() - 1, router2.DefaultTopics.Count());
        }

        [Fact]
        public void Should_FilterRenamedTopic_By_AthenaTopic()
        {
            var redirect = "new-name";
            var topicsByMethod = new Dictionary<string, string>();
            topicsByMethod.Add("method", redirect);

            var athenaClient = new Mock<IAthenaClientAsync>();

            SetupAthenaGetParameters(athenaClient, parameterKeyValue: redirect);

            var topicNameParameters = new AthenaTopicNames(athenaClient.Object, _assembly, instanceIds: new List<int> { 25 });

            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .WithAthenaRedirect(topicNameParameters)
                .FilterBy(redirect)
                .Build(_assembly);

            Assert.Contains(redirect, router.DefaultTopics);
            Assert.Single(router.DefaultTopics);
        }

        [Fact]
        public void ShouldNot_MapWhenDontHaveDefaultValue()
        {
            var redirect = "new-name";
            var redirectWithoutDefault = "should-not-map";
            var topicsByMethod = new Dictionary<string, string>();
            topicsByMethod.Add("method", redirect);

            var athenaClient = new Mock<IAthenaClientAsync>();

            SetupAthenaGetParameters(athenaClient, parameterKeyValue: redirect);
            SetupAthenaGetParameters(athenaClient, redirectWithoutDefault, "component1", "group1", "key1");

            var topicNameParameters = new AthenaTopicNames(athenaClient.Object, _assembly, instanceIds: new List<int> { 25 });

            var router = new Router(_serviceProviderMock.Object, _deserializerMock.Object, _loggerMock.Object);
            var builder = new RouterConfiguration(router, _loggerMock.Object)
                .WithAthenaRedirect(topicNameParameters)
                .FilterBy(redirect)
                .Build(_assembly);

            Assert.DoesNotContain(redirectWithoutDefault, router.DefaultTopics);
            Assert.DoesNotContain(redirectWithoutDefault, router.RetryTopics);
            Assert.Contains(redirect, router.DefaultTopics);
        }

        private static void SetupAthenaGetParameters(Mock<IAthenaClientAsync> athenaClient, string parameterKeyValue, string component = "component", string group = "group", string key = "key")
        {
            var parameters = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            var groupDict = new Dictionary<string, Dictionary<string, object>>();
            var keys = new Dictionary<string, object>();
            keys.Add(key, parameterKeyValue);
            groupDict.Add(group, keys);
            parameters.Add(component, groupDict);

            athenaClient.Setup(x => x.GetParameters(It.IsAny<int>(), component, group, key, null, false, true))
                .Returns(parameters);

            athenaClient.Setup(x => x.ListValues(component, group, key, null, false))
                .Returns(new List<InstanceValues<object>>() { 
                    new InstanceValues<object>
                    {
                        Instance = 0,
                        Partition = 0,
                        Value = parameterKeyValue
                    } 
                });
        }
    }
}
