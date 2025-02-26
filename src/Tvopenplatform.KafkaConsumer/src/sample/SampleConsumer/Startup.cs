using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using TvOpenPlatform.Configuration.Log;
using TvOpenPlatform.Consumer;
using TvOpenPlatform.Consumer.Configuration;
using TvOpenPlatform.Settings.Athena;
using TvOpenPlatform.HttpClientExtensions;
using TvOpenPlatform.TraceIdentifier;
using TvOpenPlatform.Consumer.Deserializers;

namespace SampleConsumer
{
    public static class Startup
    {
        public static IServiceProvider Configure<T>()
        {
            var services = new ServiceCollection();
            var configuration = ConfigureEnvironment();

            var baseLogger = new ServiceCollection()
              .AddLogging(config => config.SetMinimumLevel(LogLevel.Trace))              
              .BuildServiceProvider()
              .GetService<ILogger<Program>>();

            services.AddLogging(config => config.SetMinimumLevel(LogLevel.Trace));

            services.AddKafkaConsumer<T>(GetConsumerAgentConfiguration(configuration));

            services.AddSingleton<ITopicNameRedirection>(provider =>
            {
                return new TopicNameRedirection()
                {
                    TopicKeyMapping = new Dictionary<string, string>
                    {
                        { "gvp-dynamic-topic1", "gvp-dynamic-redirected1,gvp-dynamic-redirected2,gvp-dynamic-redirected3,gvp-dynamic-redirected4" }
                    }
                };
            });

            services.AddSingleton<ITraceIdentifier, TraceIdentifier>();

            services.AddEventInfrastructure();

            services
                .AddHttpClient("teste")
                .AddDefaultMessageHandlers(typeof(Program), configuration);

            services.AddSingleton<IAthenaClientAsync>(provider =>
            {
                var baseUrl = new Uri("http://docker.gvp-dev.com/dev93/athena/");
                var logger = provider.GetService<ILogger<T>>();
                var httpClient = new HttpClient() { BaseAddress = baseUrl };
                var defaultPartitions = new List<int> { 0 };
                return new AthenaClient(httpClient, new TvOpenPlatform.Logger.Microsoft.DefaultMicrosoftLogger(logger), defaultPartitions);
            });

            var serviceProvider = services.BuildServiceProvider();
            BuildLogger<T>(serviceProvider, configuration);

            return serviceProvider;
        }

        private static ConsumerAgentConfiguration GetConsumerAgentConfiguration(IConfigurationRoot configuration)
        {
            var consumerIncluded = configuration.GetSection("CONSUMER_INCLUDED").Value;
            var cliendIdPrefix = configuration.GetSection("CONSUMER_CLIENT_ID_PREFIX").Value;
            var consumerAgentConfiguration = new ConsumerAgentConfiguration()
            {
                DefaultConsumerEnabled = string.IsNullOrWhiteSpace(consumerIncluded) || consumerIncluded.Contains("default"),
                RetryConsumerEnabled = string.IsNullOrWhiteSpace(consumerIncluded) || consumerIncluded.Contains("_retry"),
                TopicFiltersInclude = configuration.GetSection("TOPICS_INCLUDED").Value,
                TopicFiltersExclude = configuration.GetSection("TOPICS_EXCLUDED").Value,
                RetryDelaySeconds = int.Parse(configuration.GetSection("RETRY_DELAY_SECONDS").Value),
                CliendIdPrefix = cliendIdPrefix,
                ConsumerConfig = configuration.GetSection("KAFKA:Consumer"),
                ExecutingAssembly = Assembly.GetExecutingAssembly(),
                InstanceIds = new List<int> { 2, 25, 3 },
                IgnoreNullMessages = true,
                AsyncConsumeEnabled = true,
                TopicDeserializer = TopicDeserializer.JsonSchema,
                SchemaRegistryUrl = "http://localhost:8085"
            };
            return consumerAgentConfiguration;
        }

        private static IConfigurationRoot ConfigureEnvironment()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();
            return configuration;
        }

        private static ILogger<T> BuildLogger<T>(IServiceProvider provider, IConfigurationRoot configuration)
        {
            var loggerFactory = provider.GetService<ILoggerFactory>();
            loggerFactory.ConfigureTvOpenPlatformLog(configuration, TvOpenPlatform.Logging.LogTraceTag.CorrelationId, Assembly.GetExecutingAssembly());

            var logger = provider.GetService<ILogger<T>>();
            return logger;
        }

    }
}
