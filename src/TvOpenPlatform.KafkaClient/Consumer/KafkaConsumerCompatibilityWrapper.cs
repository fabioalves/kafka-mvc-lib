using TvOpenPlatform.KafkaClient.Consumer;
using TvOpenPlatform.KafkaClient.Models;
using TvOpenPlatform.Logger;

namespace TvOpenPlatform.KafkaClient
{
    public interface IKafkaConsumerWrapper : IKafkaConsumerWrapper<string> { }
    public sealed class KafkaConsumerWrapper : KafkaConsumerWrapper<string>, IKafkaConsumerWrapper
    {
        public KafkaConsumerWrapper(ConsumerConfig consumerConfig, IKafkaConsumerBuilder<string> builder, ILogger logger)
            : base(consumerConfig, builder, logger)
        {}
    }
}
