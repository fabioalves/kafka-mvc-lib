using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer.Result
{
    public interface IResult
    {
        Task PostProcess<T>(ILogger logger, IKafkaConsumerWrapper<T> kafkaConsumer, ConsumeResult<string, T> consumeResult);
    }
}
