using Confluent.Kafka;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Exceptions;
using TvOpenPlatform.Logger;
using ProducerConfig = TvOpenPlatform.KafkaClient.Models.ProducerConfig;
using TvOpenPlatform.KafkaClient.LogAdapter;
using System.Collections.Generic;
using TvOpenPlatform.KafkaClient.Producer;
using TvOpenPlatform.KafkaClient.Models;
using System.Threading;
using System.Text;

namespace TvOpenPlatform.KafkaClient
{
    public class KafkaProducerWrapper : IKafkaProducerWrapper, IProducer
    {
        private static KafkaProducerWrapper _kafkaProducerWrapper;
        private readonly ProducerConfig _config;
        private readonly ILogger _logger;
        private readonly IProducer<string, string> _producer;
        public static Action<string, Message<string, string>> MessageInterceptor { get; set; }
        public static Action<string, Message<string, string>> DeadLetterChannel { get; set; }

        public static bool EnableDeadLetterChannel { get; set; } = false;

        public static IProducerSerializer Serializer { get; set; }
        public static bool VerboseEnabled { get; set; } = false;

        public static KafkaProducerWrapper Init(ProducerConfig producerConfig, ILogger logger)
        {
            if (_kafkaProducerWrapper == null ||
                Tools.Tools.IsConfigRenewed(
                    Tools.Tools.SetUpConfig(_kafkaProducerWrapper.GetConfig()),
                producerConfig))
            {
                logger.Verbose("Bootstrapping Kafka Producer instance");
                Reset();
                _kafkaProducerWrapper = new KafkaProducerWrapper(producerConfig, logger);
            }
            else
            {
                logger.Verbose("Re-using Kafka Producer instance");
            }

            
            return _kafkaProducerWrapper;
        }

        public static IKafkaProducerWrapper<A, B> InitCompatible<A, B>(ProducerConfig producerConfig, ILogger logger)
        {
            var kafkaProducerWrapper = Init(producerConfig, logger);
            return new KafkaProducerWrapperCompatibility<A, B>(kafkaProducerWrapper);
        }

        public static IKafkaProducerWrapper GetInstance()
        {
            return _kafkaProducerWrapper;
        }

        public void Flush(int timeoutMs)
        {
            try
            {
                var keepFlushing = false;
                do
                {
                    Log(LogLevel.Info, "FLUSH", "Flushing Kafka Messages");
                    var remainingDeliveryReport = _producer.Flush(TimeSpan.FromMilliseconds(timeoutMs));
                    keepFlushing = remainingDeliveryReport > 0;

                    if (keepFlushing)
                    {
                        _logger?.Warning($"There are still message waiting to be delivered. Trying to flush them again. [Remaining Messages: {remainingDeliveryReport}]");
                    }

                } while (keepFlushing);

                Log(LogLevel.Info, "FLUSH", "All Messages Flushed successfully ");
            }
            catch (Exception e)
            {
                HandleKafkaException(e, topic: string.Empty, messageId: "(none)", message: null);
            }
        }

        private async Task ProduceAsyncInternal(string topic, string key, string messageValue, bool isDeliveryReportEnabled, Headers headers)
        {
            var messageId = Guid.NewGuid().ToString();

            var message = CreateMessage(key, messageValue, headers);

            MessageInterceptor?.Invoke(topic, message);

            if(VerboseEnabled)
            {
                _logMessage.LogMessage("SEND", topic, key, messageValue, messageId, headers);                
            }   

            var deliveryResultTask = _producer
                                           .ProduceAsync(topic, message);

            var awaitDeliveryReportTaskCompletion = isDeliveryReportEnabled || _config.ForceDeliveryReportTaskAwait;

            if (_config.ForceDeliveryReportLogging || awaitDeliveryReportTaskCompletion)
            {                
                Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] DeliveryReport Callback is enabled. [MessageId: {messageId}]");

                deliveryResultTask
                    .ContinueWith(deliveryResult =>
                    {
                        
                        HandleKafkaException(
                                exception: deliveryResult.Exception, 
                                topic: topic, 
                                messageId: messageId, 
                                message: message
                        );              
                        
                        Log(Logger.LogLevel.Verbose, "SEND", $"[Kafka Producer] Produced '{deliveryResult.Result?.Value}' to '{deliveryResult.Result?.TopicPartitionOffset}'  [MessageId: {messageId}]");                        
                    }, 
                    cancellationToken: CancellationToken.None, 
                    continuationOptions: TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.DenyChildAttach, 
                    scheduler: TaskScheduler.Default);
            }
            else
            {
                Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] DeliveryReport Callback is disabled. [MessageId: {messageId}]");
            }

            if (awaitDeliveryReportTaskCompletion)
            {
                Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] As DeliveryReport is enabled, producer will wait Task Completion. [MessageId: {messageId}; isDeliveryReportEnabled: {isDeliveryReportEnabled}; ForceDeliveryReportAwait: {_config.ForceDeliveryReportTaskAwait}]");
                await deliveryResultTask.ConfigureAwait(false);
                Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] Task Completed successfully [MessageId: {messageId}]");
            }
            else
            {
                Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] As DeliveryReport is disabled, messaged will be buffered. [MessageId: {messageId}]");
            }
        }

        
       
        private static Message<string, string> CreateMessage(string key, string messageValue, Headers headers)
        {
            var message = new Message<string, string>()
            {
                Key = key,
                Value = messageValue,
                Headers = headers
            };
            
            return message;
        }       

        public ProducerConfig GetConfig()
        {
            return _kafkaProducerWrapper?._config;
        }

        private KafkaProducerWrapper(ProducerConfig producerConfig, ILogger logger)
        {
            _config = producerConfig;
            _logger = logger;
            _logMessage = Producer.LogMessage.Create(logger);
            try
            {
                _producer = GetProducer(producerConfig, _logger);
            }
            catch (KafkaException e)
            {
                Log(LogLevel.Error, "SEND", $" [SCOM] An error occurred with the Kafka Producer. Exception of type {e.InnerException} thrown with message: {e.Error.Reason}");
                throw;
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, "SEND", $" [SCOM] An internal application error happened (not in Kafka broker). Exception thrown: {e.Message}");
                throw;
            }

            _logger = logger;
        }

        ILoggerHelper _logMessage;

        public KafkaProducerWrapper(ProducerConfig producerConfig, IProducer<string, string> producer, ILoggerHelper logMessage, ILogger logger)
        {
            _config = producerConfig;
            _producer = producer;
            _logger = logger;
            _logMessage = logMessage;
        }

        private static void Reset()
        {
            if (_kafkaProducerWrapper?._config != null)
            {
                _kafkaProducerWrapper.Dispose();
            }

            _kafkaProducerWrapper = null;
        }

        public void Dispose()
        {
            Flush(_kafkaProducerWrapper._config.QueueBufferingMaxMs);
            _producer?.Dispose();
            _kafkaProducerWrapper = null;
            Log(LogLevel.Verbose, "FLUSH", "Kafka Client instance Disposed");
        }

        private void HandleKafkaException(Exception exception, string topic, string messageId, Message<string, string> message)
        {
            if (exception == null)
            {
                return;
            }

            if (EnableDeadLetterChannel) {
                DeadLetterChannel?.Invoke(topic, message);
            }
            
            if (exception.InnerException is KafkaException)
            {
                var kafkaException = exception.InnerException as KafkaException;
                Log(LogLevel.Critical, "SEND", $"[SCOM] [DELIVERY REPORT HANDLER] Kafka delivery report returned an error. Producer failed to deliver message: {kafkaException?.Error?.Reason}. [IsError:{kafkaException?.Error?.IsError}, IsBrokerError:{kafkaException?.Error?.IsBrokerError}, IsFatalError:{kafkaException?.Error?.IsFatal}, MessageId: {messageId}, ErrorCode: {kafkaException?.Error?.Code}, Message: {kafkaException?.Message}, Error: {kafkaException?.Error?.ToString()}, StackTrace: {kafkaException}]");
                throw new BrokerErrorException(kafkaException.Error.Reason, kafkaException);
            }
            else
            {
                Log(LogLevel.Critical, "SEND", $"[SCOM] [DELIVERY REPORT HANDLER] An internal application error happened (not in Kafka broker). [MessageId: {messageId}, Exception: {exception}]");
                throw new KafkaUnavailableException(exception.Message);
            }
        }

        private void Log(LogLevel logLevel, string method, string message)
        {
            _logger?.Log(logLevel, method, message);            
        }

        private IProducer<string, string> GetProducer(ProducerConfig producerConfig, ILogger logger)
        {
            var kafkaProducerConfig = Tools.Tools.SetUpConfig(producerConfig);
            var producerBuilder = new ProducerBuilder<string, string>(kafkaProducerConfig);

            producerBuilder.SetLogHandler((producer, logMessage) =>
            {
                if (producerConfig.EnableProducerLogHandler)
                {
                    logger?.Log(
                        GetLogLevel(logMessage), 
                        logMessage.Facility, 
                        $"[KAFKA PRODUCER] [SysLogLevel: {logMessage.Level}] {logMessage.Name} {logMessage.Message}");
                }
            });

            if (producerConfig.EnableProducerErrorHandler)
            {
                logger?.Verbose("Producer Error Handler is enabled");
                producerBuilder.SetErrorHandler((producer, logMessage) =>
                {
                    if (logMessage.IsFatal)
                    {
                        logger?.Log(LogLevel.Critical, "Kafka Client Producer Error Handler", $"[SCOM] [KAFKA PRODUCER CRITICAL ERROR HANDLER] Error Code: {logMessage.Code} Error Reason: {logMessage.Reason} [IsError:{logMessage.IsError}, IsBrokerError:{logMessage.IsBrokerError}, IsFatalError:{logMessage.IsFatal}]");
                    }
                    else
                    {
                        logger?.Log(LogLevel.Error, "Kafka Client Producer Error Handler", $"[KAFKA PRODUCER ERROR HANDLER] Error Code: {logMessage.Code} Error Reason: {logMessage.Reason} [IsError:{logMessage.IsError}, [IsBrokerError:{logMessage.IsBrokerError}, IsFatalError:{logMessage.IsFatal}]");
                    }
                });
            }
            else
            {
                logger?.Verbose("Producer Error Handler is disabled");
            }

            return producerBuilder.Build();
        }

        private static LogLevel GetLogLevel(Confluent.Kafka.LogMessage logMessage)
        {
            if(logMessage.Message.Contains("encountered error"))
            {
                return LogLevel.Error;
            }
            return logMessage.Level.ToTvOpenPlatformLogLevel();
        }

        public void Produce(string topicName, string key, string messageValue, bool isDeliveryReportEnabled = false)
        {
            ProduceAsyncInternal(topicName, key, messageValue, isDeliveryReportEnabled, new Headers())
                    .GetAwaiter()
                    .GetResult();
        }

        public Task ProduceAsync(string topic, string key, string messageValue, bool isDeliveryReportEnabled = false)
        {
            return ProduceAsyncInternal(topic, key, messageValue, isDeliveryReportEnabled, new Headers());
        }

        public Task ProduceAndAwaitDeliveryAsync<T>(MessageWrapper<T> message)
        {
            if (Serializer == null)
            {
                throw new Exception("Serializer not available");
            }

            var serialized = Serializer.Serialize(message.Message);
            return ProduceAsyncInternal(message.Topic, message.Key, serialized, isDeliveryReportEnabled: true, headers: message.Headers);
        }

        public Task ProduceAndForgetDeliveryAsync<T>(MessageWrapper<T> message)
        {
            if (Serializer == null)
            {
                throw new Exception("Serializer not available");
            }

            var serialized = Serializer.Serialize(message.Message);
            return ProduceAsyncInternal(message.Topic, message.Key, serialized, isDeliveryReportEnabled: false, headers: message.Headers);
        }
    }
}
