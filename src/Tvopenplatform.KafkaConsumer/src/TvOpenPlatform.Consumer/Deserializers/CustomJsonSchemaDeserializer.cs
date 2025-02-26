using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers.Exceptions;
using TvOpenPlatform.Consumer.SchemaRegistry;

namespace TvOpenPlatform.Consumer.Deserializers
{
    public class CustomJsonSchemaDeserializer
    {
        private readonly int _headerSize = sizeof(int) + sizeof(byte);
        public const byte MagicByte = 0;
        private readonly IJsonSchemaValidator _jsonSchemaValidator;

        public CustomJsonSchemaDeserializer(IJsonSchemaValidator jSchemaValidatingReaderFactory)
        {
            _jsonSchemaValidator = jSchemaValidatingReaderFactory;
        }

        public async Task<object> DeserializeAsync(ReadOnlyMemory<byte> data, bool isNull, Type type, SerializationContext context)
        {
            if (isNull) { return Task.FromResult<object>(null); }

            try
            {
                var array = data.ToArray();

                if (array.Length < 5)
                {
                    throw new InvalidDataException($"Expecting data framing of length 5 bytes or more but total data size is {array.Length} bytes");
                }

                if (array[0] != MagicByte)
                {
                    throw new InvalidDataException($"Expecting message {context.Component.ToString()} with Confluent Schema Registry framing. Magic byte was {array[0]}, expecting {MagicByte}");
                }

                using (var stream = new MemoryStream(array, _headerSize, array.Length - _headerSize))
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    var schemaId = GetSchemaId(array);
                    var messageBody = await sr.ReadToEndAsync();

                    var isValidMessage = await _jsonSchemaValidator.IsValid(schemaId, messageBody);
                    if (!isValidMessage) throw new DeserializeException();
                   
                    return JsonConvert.DeserializeObject(messageBody, type);
                }
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        private static int GetSchemaId(byte[] array)
        {
            //the schemaId is part of the message, but it is in a big endian format.
            byte[] schemaIdValue = new byte[4];
            Array.Copy(array, 1, schemaIdValue, 0, 4);
            return (schemaIdValue[0] << 24) | (schemaIdValue[1] << 16) | (schemaIdValue[2] << 8) | schemaIdValue[3];
        }
    }
}
