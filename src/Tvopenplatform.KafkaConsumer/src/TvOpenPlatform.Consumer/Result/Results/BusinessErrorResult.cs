using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Result.Options;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer.Result
{
    public class BusinessErrorResult : AsyncResult
    {
        private readonly BusinessErrorOptions _options;

        public BusinessErrorResult(BusinessErrorOptions options)
        {
            _options = options;
        }
        public override async Task PostProcess<T>(ILogger logger, IKafkaConsumerWrapper<T> kafkaConsumer, ConsumeResult<string, T> consumeResult)
        {
            logger.LogError($"Business error returned on action {_options.ActionName} on controller {_options.ControllerName}");
            await base.PostProcess(logger, kafkaConsumer, consumeResult);
        }
    }
}
