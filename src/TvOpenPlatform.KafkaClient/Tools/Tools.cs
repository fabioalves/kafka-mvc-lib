using Confluent.Kafka;

namespace TvOpenPlatform.KafkaClient.Tools
{
    public class Tools
    {
        public static Confluent.Kafka.ProducerConfig SetUpConfig(Models.ProducerConfig config)
        {
            return new Confluent.Kafka.ProducerConfig()
            {
                SocketSendBufferBytes = config.SocketSendBufferBytes,
                SocketReceiveBufferBytes = config.SocketReceiveBufferBytes,
                SocketKeepaliveEnable = config.SocketKeepAliveEnable,
                SocketNagleDisable = config.SocketNagleDisable,
                CompressionType = (CompressionType)config.CompressionCodec,
                BootstrapServers = config.BootstrapServers,
                SocketTimeoutMs = config.SocketTimeoutMs,
                MessageSendMaxRetries = config.MessageSendMaxRetries,
                LingerMs = config.QueueBufferingMaxMs,
                MessageTimeoutMs = config.MessageTimeout,
                TopicBlacklist = config.TopicBlacklist,
                TopicMetadataRefreshIntervalMs = config.TopicMetadataRefreshIntervalMs,
                Debug = config.Debug,
                SocketMaxFails = config.SocketMaxFails,
                LogConnectionClose = config.LogConnectionClose,
                Acks = GetAckMode(config.AckMode.ToString()),
                StatisticsIntervalMs = config.StatisticsIntervalMs,
                BatchNumMessages = config.QueueBufferingMaxMessages,
                MessageMaxBytes = config.MessageMaxBytes,
                QueueBufferingMaxMessages = config.QueueBufferingMaxMessages,
                MetadataMaxAgeMs = config.MetadataMaxAgeMs
            };
        }

        public static Confluent.Kafka.ConsumerConfig SetUpConsumerConfig(Models.ConsumerConfig config)
        {
            var consumerConfig = new Confluent.Kafka.ConsumerConfig()
            {
                SocketSendBufferBytes = config.SocketSendBufferBytes,
                SocketReceiveBufferBytes = config.SocketReceiveBufferBytes,
                SocketKeepaliveEnable = config.SocketKeepAliveEnable,
                SocketNagleDisable = config.SocketNagleDisable,
                SocketTimeoutMs = config.SocketTimeoutMs,
                AutoCommitIntervalMs = config.AutoCommitIntervalMs,
                AutoOffsetReset = GetAutoOffsetReset(config.AutoOffsetReset),
                BootstrapServers = config.BootstrapServers,
                EnableAutoCommit = config.EnableAutoCommit,
                EnablePartitionEof = config.EnablePartitionEof,
                GroupId = config.GroupId,
                SessionTimeoutMs = config.SessionTimeoutMs,
                StatisticsIntervalMs = config.StatisticsIntervalMs,
                FetchMaxBytes = GetFetchMaxBytes(config),
                MessageMaxBytes = GetMessageMaxBytes(config),
                TopicBlacklist = config.TopicBlacklist,
                TopicMetadataRefreshIntervalMs = config.TopicMetadataRefreshIntervalMs,
                SocketMaxFails = config.SocketMaxFails,
                LogConnectionClose = config.LogConnectionClose,
                Acks = GetAckMode(config.AckMode.ToString()),
                MaxPollIntervalMs = config.MaxPollIntervalMs,
                EnableAutoOffsetStore = config.EnableAutoOffsetStore,
                CoordinatorQueryIntervalMs = config.CoordinatorQueryIntervalMs,
                HeartbeatIntervalMs = config.HeartbeatIntervalMs,
                QueuedMinMessages = config.QueuedMinMessages,
                QueuedMaxMessagesKbytes = config.QueuedMaxMessagesKbytes,
                FetchWaitMaxMs = config.FetchWaitMaxMs,
                MaxPartitionFetchBytes = config.MaxPartitionFetchBytes,
                FetchErrorBackoffMs = config.FetchErrorBackoffMs,
                AllowAutoCreateTopics = config.AllowAutoCreateTopics
            };

            if (!string.IsNullOrWhiteSpace(config.Debug))
            {
                consumerConfig.Debug = config.Debug;
            }

            if (!string.IsNullOrWhiteSpace(config.ClientId))
            {
                consumerConfig.ClientId = config.ClientId;
            }

            return consumerConfig;
        }

        public static bool IsConfigRenewed(Confluent.Kafka.ProducerConfig oldConfig, Models.ProducerConfig producerConfig)
        {
            var newConfig = SetUpConfig(producerConfig);

            if (oldConfig.Acks != newConfig.Acks) return true;
            if (oldConfig.SocketSendBufferBytes != newConfig.SocketSendBufferBytes) return true;
            if (oldConfig.SocketReceiveBufferBytes != newConfig.SocketReceiveBufferBytes) return true;
            if (oldConfig.SocketKeepaliveEnable != newConfig.SocketKeepaliveEnable) return true;
            if (oldConfig.SocketNagleDisable != newConfig.SocketNagleDisable) return true;
            if (oldConfig.CompressionType != newConfig.CompressionType) return true;
            if (oldConfig.BootstrapServers != newConfig.BootstrapServers) return true;
            if (oldConfig.SocketTimeoutMs != newConfig.SocketTimeoutMs) return true;
            if (oldConfig.MessageSendMaxRetries != newConfig.MessageSendMaxRetries) return true;
            if (oldConfig.QueueBufferingMaxMessages != newConfig.QueueBufferingMaxMessages) return true;
            if (oldConfig.LingerMs != newConfig.LingerMs) return true;
            if (oldConfig.TopicMetadataRefreshIntervalMs != newConfig.TopicMetadataRefreshIntervalMs) return true;
            if (oldConfig.Debug != newConfig.Debug) return true;
            if (oldConfig.SocketMaxFails != newConfig.SocketMaxFails) return true;
            if (oldConfig.LogConnectionClose != newConfig.LogConnectionClose) return true;
            if (oldConfig.TopicBlacklist != newConfig.TopicBlacklist) return true;
            if (oldConfig.MessageMaxBytes != newConfig.MessageMaxBytes) return true;
            if (oldConfig.StatisticsIntervalMs != newConfig.StatisticsIntervalMs) return true;

            return false;
        }

        private static Acks GetAckMode(string ackMode)
        {
            switch (ackMode)
            {
                case "0":
                    return Acks.None;
                case "1":
                    return Acks.Leader;
                default:
                    return Acks.All;
            }
        }

        private static AutoOffsetReset GetAutoOffsetReset(string value)
        {
            switch (value)
            {
                case "latest":
                    return AutoOffsetReset.Latest;
                case "earliest":
                    return AutoOffsetReset.Earliest;
                default:
                    return AutoOffsetReset.Error;
            }
        }

        private static int GetFetchMaxBytes(Models.ConsumerConfig config)
        {
            return config.FetchMaxBytes == 0 ? 52428800 : config.FetchMaxBytes;
        }

        private static int? GetMessageMaxBytes(Models.ConsumerConfig config)
        {
            return config.MessageMaxBytes == 0 ? 10000000 : config.MessageMaxBytes;
        }

    }
}
