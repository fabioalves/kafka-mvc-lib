using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using TvOpenPlatform.Logger;

namespace TvOpenPlatform.KafkaClient.Producer
{
    public class LogMessage : ILoggerHelper
    {
        private readonly ILogger _logger;

        private LogMessage(ILogger logger)
        {
            _logger = logger;
        }
        public static LogMessage Create(ILogger logger)
        {
            return new LogMessage(logger);
        }

        private string GetHeaderString(Headers headers)
        {
            if (headers == null || headers.Count < 1)
            {
                return "[]";
            }

            StringBuilder headersString = new StringBuilder();
            headersString.Append("[");
            foreach (Header header in headers)
            {
                headersString.Append("{");
                headersString.AppendFormat("{0}: {1}", header.Key, Encoding.UTF8.GetString(header.GetValueBytes()));
                headersString.Append("}, ");
            }
            headersString.Remove(headersString.Length - 2, 2);
            headersString.Append("]");

            return headersString.ToString();
        }

        void ILoggerHelper.LogMessage(string method, string topic, string key, string messageValue, string messageId, Headers headers)
        {
            _logger?.Log(LogLevel.Verbose, "SEND", $"[Kafka Producer] Sending Message to Kafka. [Topic: {topic}, Key: {key}, Value: {messageValue}, MessageId: {messageId}, Headers: {GetHeaderString(headers)}]");            
        }
    }
}
