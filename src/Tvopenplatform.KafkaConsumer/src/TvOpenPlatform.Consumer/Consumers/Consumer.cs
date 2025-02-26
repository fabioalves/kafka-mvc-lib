using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Consumers;
using TvOpenPlatform.Consumer.Deserializers.Exceptions;
using TvOpenPlatform.Consumer.Message;
using TvOpenPlatform.Consumer.Routing;
using TvOpenPlatform.KafkaClient;

namespace TvOpenPlatform.Consumer
{
    public abstract class Consumer : IConsumer
    {
        protected string ConsumerName;
        protected IKafkaConsumerWrapper<byte[]> KafkaConsumerWrapper;
        protected ILogger Logger;
        protected IRouter Router;
        protected readonly ConsumerAgentConfiguration ConsumerAgentConfiguration;

        public Consumer(string consumerName, IKafkaConsumerWrapper<byte[]> kafkaConsumerWrapper, IRouter router, ILogger logger, ConsumerAgentConfiguration consumerAgentConfiguration)
        {
            ConsumerName = consumerName;
            KafkaConsumerWrapper = kafkaConsumerWrapper;
            Logger = logger;
            ConsumerAgentConfiguration = consumerAgentConfiguration;
            Router = router;
        }

        public void Run(CancellationToken cancellation = default)
        {
            new Thread(new ThreadStart(() => Consume(cancellation))).Start();
        }

        protected abstract void Consume(CancellationToken cancellationToken = default);

        protected async Task MessageHandler(ConsumeResult<string, byte[]> consumeResult)
        {
            try
            {
                if (consumeResult.Message == null)
                    return;

                var eventId = Guid.NewGuid().ToString();
                using (Logger.BeginScope($"Consumer Name: {ConsumerName}"))
                using (Logger.BeginScope($"EventId: {eventId}"))
                {
                    if (IsMessageNullOrEmpty(consumeResult) && ConsumerAgentConfiguration.IgnoreNullMessages)
                    {
                        Logger.LogDebug("Ignoring message empty or null");
                        return;
                    }

                    Logger.LogInformation($"Handling message {System.Text.Encoding.UTF8.GetString(consumeResult.Message.Value)} on topic {consumeResult.Topic}");
                    var messageContext = new MessageContext(consumeResult.Topic, consumeResult.Message.Key, consumeResult.Message.Headers, eventId);
                    Result.IResult result = await Router.Execute(messageContext, consumeResult.Message.Value).ConfigureAwait(false);

                    await result.PostProcess(Logger, KafkaConsumerWrapper, consumeResult);
                }
            }
            catch (DeserializeException ex)
            {
                Logger.LogError($"Error while deserializing message. Ignoring message in invalid format. [Exception: {ex}]");
            }
            catch (Exception ex)
            {
                Logger.LogError("Could not handle event");
                Logger.LogError(ex.Message);
                Logger.LogError(ex.StackTrace);
            }
        }

        private static bool IsMessageNullOrEmpty(ConsumeResult<string, byte[]> consumeResult)
        {
            return (consumeResult?.Message?.Value == null || consumeResult.Message.Value.Length == 0);
        }

        protected void InvokeAsyncHandler(Func<ConsumeResult<string, byte[]>, Task> handler, ConsumeResult<string, byte[]> consumeResult)
        {
            KafkaConsumerWrapper.Pause(consumeResult.TopicPartition);
            Task.Run(async () =>
            {
                await handler(consumeResult).ConfigureAwait(false);
                KafkaConsumerWrapper.Resume(consumeResult.TopicPartition);
            }).ConfigureAwait(false);
        }
    }
}
