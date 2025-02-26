namespace TvOpenPlatform.KafkaClient.Models
{
    public class ProducerConfig : ClientConfig
    {
        public CompressionCodecEnum CompressionCodec { get; set; }
        public int MessageSendMaxRetries { get; set; }
        public int QueueBufferingMaxMessages { get; set; }
        public int QueueBufferingMaxMs { get; set; }
        public int MessageTimeout { get; set; }
        public int LingerMs { get; set; }
        public bool ForceDeliveryReportLogging { get; set; } = true;
        public bool EnableProducerLogHandler { get; set; } = true;
        public bool EnableProducerErrorHandler { get; set; } = true;
        public bool ForceDeliveryReportTaskAwait { get; set; } = false;
        public int MetadataMaxAgeMs { get; set; } = 900000;
    }
}
