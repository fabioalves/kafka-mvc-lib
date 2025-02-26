using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using TvOpenPlatform.Consumer.Consumers;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.Parameters;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.Consumer.Parameters.Extensions;
using TvOpenPlatform.KafkaClient;
using TvOpenPlatform.KafkaClient.Consumer;
using TvOpenPlatform.KafkaClient.Models;
using System.Collections.Generic;
using TvOpenPlatform.Consumer.SchemaRegistry;
using Confluent.SchemaRegistry;

namespace TvOpenPlatform.Consumer.Configuration
{
    public static class ConfigurationExtension
    {
        public static ServiceCollection AddKafkaConsumer<T>(this ServiceCollection services, ConsumerAgentConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddAthenaTopics(configuration.ExecutingAssembly, configuration.InstanceIds ?? new List<int>());
            services.AddConsumer<T>(configuration);
            return services;
        }

        private static ServiceCollection AddConsumer<T>(this ServiceCollection services, ConsumerAgentConfiguration configuration)
        {
            services
                .Configure<ConsumerConfig>(options =>
                {
                    configuration.ConsumerConfig.Bind(options);
                    options.CommitAtferHandlingMessage = false;
                    options.EnableAutoCommit = false;
                })
                .AddSingleton(provider => provider.GetRequiredService<IOptions<ConsumerConfig>>().Value);

            services.AddTransient<IKafkaConsumerBuilder<byte[]>, KafkaConsumerBuilder<byte[]>>();
            services.AddTransient<IKafkaConsumerWrapper<byte[]>>(provider =>
            {
                var consumerConfig = provider.GetRequiredService<ConsumerConfig>();
                consumerConfig.ClientId = GetClientId(configuration.CliendIdPrefix);

                var logger = provider.GetService<ILogger<Consumer>>();
                var builder = provider.GetService<IKafkaConsumerBuilder<byte[]>>();
                return new KafkaConsumerWrapper<byte[]>(consumerConfig, builder, new TvOpenPlatform.Logger.Microsoft.DefaultMicrosoftLogger(logger));
            });

            services.AddDeserializer(configuration);

            services.AddSingleton<IRouter, Router>(provider =>
            {
                var deserializer = provider.GetService<ITopicDeserializer>();
                var athenaTopicNames = provider.GetService<IAthenaTopicNames>();
                var topicNames = provider.GetService<ITopicNameRedirection>();

                var router = new Router(provider, deserializer, provider.GetService<ILogger<Router>>());
                new RouterConfiguration(router, provider.GetService<ILogger<RouterConfiguration>>())
                        .FilterBy(configuration.TopicFiltersInclude)
                        .Except(configuration.TopicFiltersExclude)
                        .WithRedirect(topicNames)
                        .WithAthenaRedirect(athenaTopicNames)
                        .Build(configuration.ExecutingAssembly);

                return router;
            });

            services.AddTransient<IConsumer, DefaultConsumer>(provider =>
            {
                var consumerName = GetClientId(configuration.CliendIdPrefix);
                var programConfiguration = provider.GetRequiredService<ConsumerAgentConfiguration>();
                var kafkaConsumerWrapper = provider.GetRequiredService<IKafkaConsumerWrapper<byte[]>>();
                var router = provider.GetRequiredService<IRouter>();
                var logger = provider.GetRequiredService<ILogger<T>>();
                return new DefaultConsumer(consumerName, programConfiguration, kafkaConsumerWrapper, router, logger);
            });

            services.AddTransient<IConsumer, RetryConsumer>(provider =>
            {
                var consumerName = GetClientId(configuration.CliendIdPrefix);
                var programConfiguration = provider.GetRequiredService<ConsumerAgentConfiguration>();
                var kafkaConsumerWrapper = provider.GetRequiredService<IKafkaConsumerWrapper<byte[]>>();
                var router = provider.GetRequiredService<IRouter>();
                var logger = provider.GetRequiredService<ILogger<T>>();
                return new RetryConsumer(consumerName, programConfiguration, kafkaConsumerWrapper, router, logger, configuration.RetryDelaySeconds);
            });

            return services;
        }

        private static void AddDeserializer(this ServiceCollection services, ConsumerAgentConfiguration configuration)
        {
            services.AddSingleton<ITopicDeserializer>(provider =>
            {
                if (configuration.TopicDeserializer == TopicDeserializer.JsonSchema)
                {
                    return new JsonSchemaTopicDeserializer(
                        provider.GetService<ILogger<JsonSchemaTopicDeserializer>>(),
                        new JsonSchemaValidator(new CachedSchemaRegistryClient(
                            new SchemaRegistryConfig{
                                Url = configuration.SchemaRegistryUrl
                            }
                        ),
                        provider.GetRequiredService<ILogger<JsonSchemaValidator>>()));
                }

                return new NewtonsoftJsonTopicDeserializer(
                    provider.GetService<ILogger<NewtonsoftJsonTopicDeserializer>>(),
                    configuration.DeserializerTypeNameHandling);
            });
        }

        private static string GetClientId(string cliendIdPrefix)
        {
            return !string.IsNullOrWhiteSpace(cliendIdPrefix) ? $"{cliendIdPrefix} - {Guid.NewGuid()}" : Guid.NewGuid().ToString();
        }
    }
}
