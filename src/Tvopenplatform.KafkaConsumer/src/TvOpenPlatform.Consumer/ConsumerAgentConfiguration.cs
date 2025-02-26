using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Reflection;
using TvOpenPlatform.Consumer.Deserializers;

namespace TvOpenPlatform.Consumer
{
    public class ConsumerAgentConfiguration
    {
        public bool DefaultConsumerEnabled { get; set; }
        public bool RetryConsumerEnabled { get; set; }
        public IConfigurationSection ConsumerConfig { get; set; }
        public string CliendIdPrefix { get; set; }
        public string TopicFiltersInclude { get; set; }
        public string TopicFiltersExclude { get; set; }
        public int RetryDelaySeconds { get; set; }
        public Assembly ExecutingAssembly { get; set; }
        public bool IgnoreNullMessages { get; set; }
        public IList<int> InstanceIds { get; set; }
        public IList<string> DefaultPartitions { get; set; }
        public string DeserializerTypeNameHandling { get; set; } = "None";
        public bool AsyncConsumeEnabled { get; set; }
        public TopicDeserializer TopicDeserializer { get; set; } = TopicDeserializer.NewtonsoftJson;
        public string SchemaRegistryUrl { get; set; }
    }
}
