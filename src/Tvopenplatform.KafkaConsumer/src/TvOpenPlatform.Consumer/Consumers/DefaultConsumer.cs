using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer
{
    public class DefaultConsumer : Consumer
    {
        public DefaultConsumer(
            string consumerName,
            ConsumerAgentConfiguration consumerAgentConfiguration,
            IKafkaConsumerWrapper<byte[]> kafkaConsumerWrapper,
            IRouter router,
            ILogger logger)
            : base(consumerName, kafkaConsumerWrapper, router, logger, consumerAgentConfiguration)
        {
        }

        protected override void Consume(CancellationToken cancellationToken)
        {
            if (!ConsumerAgentConfiguration.DefaultConsumerEnabled)
            {
                Logger.LogInformation("DefaultRunner consumer disabled");
                return;
            }

            var topics = Router.DefaultTopics.ToList();
            if (!topics.Any())
            {
                Logger.LogInformation($"No default topics informed. Shutting down thread");
                return;
            }

            Logger.LogInformation($"Start consuming default events");
            KafkaConsumerWrapper.StartConsumption(topics, ProcessingHandler, cancellationToken);
        }

        private void ProcessingHandler(ConsumeResult<string, byte[]> consumeResult)
        {
            if (!ConsumerAgentConfiguration.AsyncConsumeEnabled)
            {
                MessageHandler(consumeResult).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                InvokeAsyncHandler(MessageHandler, consumeResult);
            }
        }
    }
}
