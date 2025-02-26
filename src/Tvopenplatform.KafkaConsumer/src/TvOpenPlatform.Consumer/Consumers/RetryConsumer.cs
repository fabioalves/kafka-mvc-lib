using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer
{
    public class RetryConsumer : Consumer
    {
        private readonly TimeSpan _retryDelay;
        public RetryConsumer(
            string consumerName,
            ConsumerAgentConfiguration programConfiguration,
            IKafkaConsumerWrapper<byte[]> kafkaConsumerWrapper,
            IRouter router,
            ILogger logger,
            int retryDelayTimeSeconds = 30)
            : base(consumerName, kafkaConsumerWrapper, router, logger, programConfiguration)
        {
            _retryDelay = TimeSpan.FromSeconds(retryDelayTimeSeconds);
        }

        private async Task RetryMessageHandler(ConsumeResult<string, byte[]> result)
        {
            await MessageHandler(result).ConfigureAwait(false);
            DelayExecution();
        }
        private void DelayExecution()
        {
            var timer = new Stopwatch();
            timer.Start();
            Logger.LogInformation($"Delaying execution of retry handler for {_retryDelay.TotalSeconds} seconds");
            Thread.Sleep(_retryDelay);
            timer.Stop();
            Logger.LogInformation($"Resuming execution of retry handle after {timer.Elapsed.TotalSeconds} seconds");
        }

        protected override void Consume(CancellationToken cancellationToken)
        {
            if (!ConsumerAgentConfiguration.RetryConsumerEnabled)
            {
                Logger.LogInformation("RetryRunner consumer disabled");
                return;
            }

            var topics = Router.RetryTopics.ToList();
            if (!topics.Any())
            {
                Logger.LogInformation("No retry topics informed. Shutting down thread");
                return;
            }

            Logger.LogInformation("Start consuming retry events");
            KafkaConsumerWrapper.StartConsumption(topics, ProcessingHandler, cancellationToken: cancellationToken, delayTime: _retryDelay);

        }

        private void ProcessingHandler(ConsumeResult<string, byte[]> consumeResult)
        {
            if (!ConsumerAgentConfiguration.AsyncConsumeEnabled)
            {
                RetryMessageHandler(consumeResult).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            else
            {
                InvokeAsyncHandler(RetryMessageHandler, consumeResult);
            }
        }
    }
}
