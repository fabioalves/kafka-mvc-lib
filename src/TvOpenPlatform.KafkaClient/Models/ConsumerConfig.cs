using System;

namespace TvOpenPlatform.KafkaClient.Models
{
    public class ConsumerConfig:ClientConfig
    {
        public string GroupId { get; set; }
        public int SessionTimeoutMs { get; set; } = 10000;
        public string AutoOffsetReset { get; set; }
        public bool EnablePartitionEof { get; set; }
        public bool CommitAtferHandlingMessage { get; set; }
        public bool EnableAutoCommit { get; set; }
        public int AutoCommitIntervalMs { get; set; }
        public int FetchMaxBytes { get; set; }
        public int MaxPollIntervalMs { get; set; } = 300000;
        public bool EnableAutoOffsetStore { get; set; } = true;
        public int MaxConsecutiveRestartAttempts { get; set; } = 0;
        public bool ShouldTryRestartIfConsumeFails { get; set; }
        public int? CoordinatorQueryIntervalMs { get;  set; } = 600000;
        public int? HeartbeatIntervalMs { get;  set; } = 3000;
        public int? QueuedMinMessages { get;  set; } = 100000;
        public int? QueuedMaxMessagesKbytes { get;  set; } = 1048576;
        public int? FetchWaitMaxMs { get;  set; } = 100;
        public int? MaxPartitionFetchBytes { get;  set; } = 1048576;
        public int? FetchErrorBackoffMs { get;  set; } = 500;
        public int MaxWaitTimeToConsumeMs { get; set; }
        public string ClientId { get; set; }

        public bool AllowAutoCreateTopics { get; set; } = true;
    }
}