using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using TvOpenPlatform.Logger;

namespace TvOpenPlatform.KafkaClient.Producer
{
    public interface ILoggerHelper
    {       
        void LogMessage(string method, string topic, string key, string messageValue, string messageId, Headers headers);
    }
}
