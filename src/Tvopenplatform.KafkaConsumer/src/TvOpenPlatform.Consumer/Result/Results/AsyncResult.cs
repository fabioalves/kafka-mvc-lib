using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer.Result
{
    public abstract class AsyncResult : IResult
    {
        public virtual Task PostProcess<T>(ILogger logger, IKafkaConsumerWrapper<T> kafkaConsumer, ConsumeResult<string, T> consumeResult)
        {
            kafkaConsumer.CommitOffset(consumeResult);

            string message = $"{consumeResult.Message.Value}";

            if (consumeResult.Message.Value.GetType() == typeof(byte[]))
            {
                message = $"{System.Text.Encoding.UTF8.GetString(consumeResult.Message.Value as byte[])}";
            }

            logger.LogInformation($"Message sucessfully processed with result: {this.GetType()}");
            logger.LogInformation($"Processing finished for {message} on topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}");
            return Task.CompletedTask;
        }
    }
}