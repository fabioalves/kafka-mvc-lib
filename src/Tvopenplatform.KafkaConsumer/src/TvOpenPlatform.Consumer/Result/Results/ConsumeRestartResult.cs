using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Result.Options;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer.Result
{
    public class ConsumeRestartResult : IResult
    {
        private readonly ConsumeRestartOptions _consumeRestartOptions;

        public ConsumeRestartResult(ConsumeRestartOptions consumeRestartOptions)
        {
            _consumeRestartOptions = consumeRestartOptions;
        }

        public async Task PostProcess<T>(ILogger logger, IKafkaConsumerWrapper<T> kafkaConsumer, ConsumeResult<string, T> consumeResult)
        {
            string message = $"{consumeResult.Message.Value}";

            if (consumeResult.Message.Value.GetType() == typeof(byte[]))
            {
                message = $"{System.Text.Encoding.UTF8.GetString(consumeResult.Message.Value as byte[])}";
            }

            logger.LogInformation($"Message sucessfully processed with result: {GetType()}");
            logger.LogInformation($"Processing finished for {message} on topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}");

            if (_consumeRestartOptions != null)
            {
                logger.LogInformation($"Delaying execution for re-processing: {_consumeRestartOptions.DelayBeforeRestartInSeconds} seconds");
                await Task.Delay(TimeSpan.FromSeconds(_consumeRestartOptions.DelayBeforeRestartInSeconds));
            }

            logger.LogInformation($"Restart consumer for re-processing");
            kafkaConsumer.ReprocessMessage(consumeResult.TopicPartitionOffset);
        }
    }
}
