using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Consumer;
using TvOpenPlatform.Logger;
using ConsumerConfig = TvOpenPlatform.KafkaClient.Models.ConsumerConfig;

namespace TvOpenPlatform.KafkaClient
{
    public class KafkaConsumerWrapper<T> : IKafkaConsumerWrapper<T>
    {
        private readonly ConsumerConfig _config;
        private readonly IKafkaConsumerBuilder<T> _builder;
        private readonly ILogger _logger;
        private List<string> _topics;
        private Confluent.Kafka.ConsumerConfig _kafkaConsumerConfig;
        private IConsumer<string, T> _consumer;
        private List<TopicPartition> _topicPartitions;
        private int _consecutiveRestartAttempts = 0;

        public KafkaConsumerWrapper(ConsumerConfig consumerConfig, IKafkaConsumerBuilder<T> builder, ILogger logger)
        {
            _config = consumerConfig;
            _builder = builder;
            _logger = logger;
            _topicPartitions = new List<TopicPartition>();
        }

        public void StartConsumption(List<string> topics, Action<ConsumeResult<string, T>> messageHandler, CancellationToken? cancellationToken = null, TimeSpan? delayTime = null)
        {
            try
            {
                _topics = topics;
                _kafkaConsumerConfig = Tools.Tools.SetUpConsumerConfig(_config);
                SetupTopicPartitions(GetInitialTopicPartitionsAssignment().ToList());
                StartConsumer(_topicPartitions);
                CheckAssignment();

                while (true)
                {
                    try
                    {
                        var timer = new Stopwatch();
                        timer.Start();
                        var consumeResult = Consume(cancellationToken);
                        timer.Stop();
                        LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"Message consumed in {timer.ElapsedMilliseconds} milliseconds");

                        timer.Restart();

                        if (consumeResult != null)
                        {
                            if (consumeResult.Message != null)
                            {
                                messageHandler(consumeResult);
                                AutoCommitOffset(consumeResult);
                                timer.Stop();
                                LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"Message {consumeResult.Message.Value} processed in {timer.ElapsedMilliseconds} milliseconds");
                                HandleDelay(delayTime, consumeResult);
                            }

                            if (consumeResult.IsPartitionEOF && consumeResult.Message == null)
                            {
                                LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"EOF - {consumeResult.TopicPartition}{consumeResult.Offset}");
                                timer.Stop();
                                HandleDelay(delayTime, consumeResult);
                            }
                        }
                        else
                        {
                            LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"No new Messages found after {_config.MaxWaitTimeToConsumeMs} milliseconds.");
                            timer.Stop();
                        }

                        _consecutiveRestartAttempts = 0;
                    }
                    catch (ConsumeException e)
                    {
                        ConsumerErrorHandler(e.Error, topics);
                    }
                    catch (KafkaException e)
                    {
                        LogMessage(LogLevel.Error, "KafkaConsumerWrapper.StartConsumption", "Error on consuming messages. " + e.Error.Reason);
                        if (_config.ShouldTryRestartIfConsumeFails && _consecutiveRestartAttempts < _config.MaxConsecutiveRestartAttempts)
                        {
                            RestartConsumer();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            catch (Exception e)
            {
                LogMessage(LogLevel.Error, "KafkaConsumerWrapper.StartConsumption", "Error on consuming messages. " + e.Message);
                _consumer?.Close();
                throw;
            }
            finally
            {
                _consumer?.Dispose();
            }
        }

        public void CommitOffset(ConsumeResult<string, T> consumeResult)
        {
            try
            {
                if (consumeResult.Message != null && !_config.EnableAutoCommit)
                {
                    _consumer.Commit(consumeResult);
                }
            }
            catch (KafkaException e)
            {
                if (e.Error.Code == ErrorCode.UnknownMemberId)
                {
                    RestartConsumer();
                    _consumer.Commit(consumeResult);
                }
                else
                {
                    LogMessage(LogLevel.Error, "KafkaConsumerWrapper.CommitOffset", "Error commiting offset: " + e.Error.Reason);
                    throw;
                }
            }
        }

        public void RestartConsumer()
        {
            _consecutiveRestartAttempts++;
            _consumer.Close();
            _consumer.Dispose();

            LogMessage(LogLevel.Warning, "KafkaConsumerWrapper.RestartConsumer", "Trying to restart Kafka Consumer");
            var topicsPartitionsKey = _topics.Select(x => new TopicPartition(x, Partition.Any));
            StartConsumer(topicsPartitionsKey.ToList());
        }

        public void ReprocessMessage(TopicPartitionOffset topicPartitionOffset)
        {
            LogMessage(LogLevel.Info, "KafkaConsumerWrapper.ReprocessMessage", $"Setting offset {topicPartitionOffset.Offset} of Partition {topicPartitionOffset.Partition} in Topic {topicPartitionOffset.Topic} to be reprocessed again");
            _consumer.Seek(topicPartitionOffset);
        }

        public void Pause(TopicPartition topicPartition)
        {
            _consumer.Pause(new List<TopicPartition>() { topicPartition });
        }

        public void Resume(TopicPartition topicPartition)
        {
            _consumer.Resume(new List<TopicPartition>() { topicPartition });
        }

        private ConsumeResult<string, T> Consume(CancellationToken? cancellationToken)
        {
            if (cancellationToken.HasValue)
            {
                return _consumer.Consume(cancellationToken.Value);
            }
            else
            {
                return _consumer.Consume(TimeSpan.FromMilliseconds(_config.MaxWaitTimeToConsumeMs));
            }
        }

        private void StartConsumer(List<TopicPartition> assignment)
        {
            LogMessage(LogLevel.Info, "KafkaConsumerWrapper.StartConsumer", "Starting Kafka Consumer");
            _consumer = BuildConsumer(_kafkaConsumerConfig);
            _consumer.Assign(assignment);
            _consumer.Subscribe(assignment.Select(_ => _.Topic));
        }

        private void CheckAssignment()
        {
            if (_consumer.Assignment == null || !_consumer.Assignment.Any())
            {
                _logger.Warning($"Consumer {_consumer.MemberId} without assignment. Restarting consumer");
                RestartConsumer();
            }
        }

        private void SetupTopicPartitions(List<TopicPartition> topicPartitions)
        {
            LogMessage(LogLevel.Verbose, "SetupTopicPartitions", $"Assigned partitions: [{string.Join(", ", topicPartitions.Select(x => x.Topic + x.Partition.Value))}]");

            var topicsPartitionsKey = topicPartitions.Select(x => new TopicPartition(
                x.Topic, x.Partition));

            lock (_topicPartitions)
            {
                _topicPartitions = topicsPartitionsKey.ToList();
            }
        }

        private IEnumerable<TopicPartition> GetInitialTopicPartitionsAssignment()
        {
            foreach (var topic in _topics)
            {
                yield return new TopicPartition(topic, Partition.Any);
            }
        }

        private IConsumer<string, T> BuildConsumer(Confluent.Kafka.ConsumerConfig kafkaConsumerConfig)
        {
            return _builder.Build(kafkaConsumerConfig, ErrorHandler, SetupTopicPartitions, PartitionsRevokedHandler, logger: _logger);
        }

        private void PartitionsRevokedHandler(List<TopicPartitionOffset> partitions)
        {
            LogMessage(LogLevel.Verbose, "SetPartitionsRevokedHandler", $"Revoking assignment: [{string.Join(", ", partitions)}]");
        }

        private void HandleDelay(TimeSpan? delayTime, ConsumeResult<string, T> consumeResult)
        {
            if (delayTime.HasValue)
            {
                var topicPartitionKey = consumeResult.TopicPartition.Topic + consumeResult.TopicPartition.Partition.Value;
                var topicPartitionKeys = GetTopicPartitionsKeys();
                if (topicPartitionKeys.Last() == topicPartitionKey)
                {
                    LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"Pausing consume for: {delayTime.Value.TotalSeconds} seconds");
                    Task.Delay(delayTime.Value).GetAwaiter().GetResult();
                    LogMessage(LogLevel.Verbose, "KafkaConsumerWrapper.StartConsumption", $"Resuming consume");
                }
            }
        }

        private IEnumerable<string> GetTopicPartitionsKeys()
        {
            return _topicPartitions.Select(x => x.Topic + x.Partition.Value);
        }

        private void LogMessage(LogLevel logLevel, string method, string message)
        {
            _logger?.Log(logLevel, method, message);
        }

        private void AutoCommitOffset(ConsumeResult<string, T> consumeResult)
        {
            if (_config.CommitAtferHandlingMessage)
            {
                CommitOffset(consumeResult);
            }
        }

        private void ErrorHandler(Error error)
        {
            LogMessage(LogLevel.Error, "KafkaConsumerWrapper.StartConsumption", "Error: " + error.Reason);
        }

        private void ConsumerErrorHandler(Error error, List<string> topics)
        {
            LogMessage(LogLevel.Error, "KafkaConsumerWrapper.StartConsumption",
                $"Error consuming from topics {string.Join(",", topics)}: {error.Reason}"
                );

            if (_config.AllowAutoCreateTopics && error.Code == ErrorCode.UnknownTopicOrPart)
            {
                RestartConsumer();
                return;
            }

            if (_config.ShouldTryRestartIfConsumeFails && _consecutiveRestartAttempts < _config.MaxConsecutiveRestartAttempts)
            {
                RestartConsumer();
                return;
            }

            throw new Exception(error.Reason);
        }
    }
}
