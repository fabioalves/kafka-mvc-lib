using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers.Exceptions;
using TvOpenPlatform.Consumer.SchemaRegistry;

namespace TvOpenPlatform.Consumer.Deserializers
{
    public class JsonSchemaTopicDeserializer : ITopicDeserializer
    {
        private readonly ILogger _logger;
        private readonly CustomJsonSchemaDeserializer _deserializer;

        public JsonSchemaTopicDeserializer(ILogger logger, IJsonSchemaValidator jSchemaValidatingReaderFactory)
        {
            _logger = logger;
            _deserializer = new CustomJsonSchemaDeserializer(jSchemaValidatingReaderFactory);
        }

        public async Task<object> DeserializeAsync(Type type, byte[] message)
        {
            try
            {
                return await _deserializer.DeserializeAsync(new ReadOnlyMemory<byte>(message), false, type, SerializationContext.Empty);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogError($"Message in the topic is not in a valid JsonSchema format [Exception: {ex}]");
                throw new DeserializeException(ex?.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error trying to deserialize message with value: {message} [Exception: {ex}]");
                throw new DeserializeException(ex?.Message, ex);
            }
        }
    }
}
