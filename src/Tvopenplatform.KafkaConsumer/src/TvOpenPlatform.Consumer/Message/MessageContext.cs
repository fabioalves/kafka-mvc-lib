using Confluent.Kafka;
using System.Collections.Generic;
using System.Linq;

namespace TvOpenPlatform.Consumer.Message
{
    public class MessageContext : IMessageContext
    {
        private readonly string _eventId;

        public MessageContext(string topic, string key, Headers kafkaHeaders, string eventId)
        {
            _eventId = eventId;
            Topic = topic;
            Key = key;
            Headers = GetHeaders(kafkaHeaders);
            Headers.TryGetValue("correlationId", out var correlationId);
            CorrelationId = correlationId != null ? System.Text.Encoding.Default.GetString(correlationId) : null;
        }

        public string Topic { get; }
        public string Key { get; }
        public string CorrelationId { get; private set; }

        public Dictionary<string, byte[]> Headers { get; private set; }

        //TODO: Add unit tests for this method.
        private static Dictionary<string, byte[]> GetHeaders(Headers kafkaHeaders)
        {
            var headers = new Dictionary<string, byte[]>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var header in kafkaHeaders)
            {
                headers.Add(header.Key, header.GetValueBytes());
            }
            return headers;
        }

        //TODO: Add unit tests for this method.
        public void SetCorrelationId(object[] actionParameters)
        {
            if (string.IsNullOrWhiteSpace(CorrelationId))
            {
                var mainObject = actionParameters.Where(x => x != null && !typeof(IMessageContext).IsAssignableFrom(x.GetType())).FirstOrDefault();

                if (mainObject != null)
                {
                    var correlationIdProperty = mainObject.GetType()?.GetProperties()?.Where(x => x.Name == "CorrelationId").FirstOrDefault();
                    var correlationIdPropertyValue = correlationIdProperty?.GetValue(mainObject);
                    var fallbackCorrelationId = string.IsNullOrEmpty(Key) ? _eventId : Key;
                    CorrelationId = correlationIdPropertyValue != null ? (string)correlationIdPropertyValue : fallbackCorrelationId;
                }
            }
        }
    }
}
