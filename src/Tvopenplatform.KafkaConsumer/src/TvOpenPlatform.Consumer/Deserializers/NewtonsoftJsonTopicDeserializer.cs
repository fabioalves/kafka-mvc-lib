using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers.Exceptions;

namespace TvOpenPlatform.Consumer.Deserializers
{
    public class NewtonsoftJsonTopicDeserializer : ITopicDeserializer
    {
        private readonly ILogger _logger;
        private readonly TypeNameHandling _typeNameHandling;

        public NewtonsoftJsonTopicDeserializer(ILogger logger, string typeNameHandling = "None")
        {
            _logger = logger;
            _typeNameHandling = (TypeNameHandling)Enum.Parse(typeof(TypeNameHandling), typeNameHandling);
        }

        public Task<object> DeserializeAsync(Type type, byte[] message)
        {
            try
            {
                using (var stream = new MemoryStream(message))
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    return Task.FromResult(JsonConvert.DeserializeObject(sr.ReadToEnd(), type, new JsonSerializerSettings
                    {
                        TypeNameHandling = _typeNameHandling
                    }));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error trying to deserialize message with value: {message} [Exception: {ex}]");
                throw new DeserializeException(ex?.Message, ex);
            }
        }
    }
}
