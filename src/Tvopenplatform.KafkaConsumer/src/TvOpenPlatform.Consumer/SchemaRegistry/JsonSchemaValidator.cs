using Confluent.SchemaRegistry;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using System.Linq;
using System.Threading.Tasks;

namespace TvOpenPlatform.Consumer.SchemaRegistry
{
    public class JsonSchemaValidator : IJsonSchemaValidator
    {
        private readonly ISchemaRegistryClient _schemaRegistryClient;
        private readonly ILogger<JsonSchemaValidator> _logger;

        public JsonSchemaValidator(ISchemaRegistryClient schemaRegistryClient, ILogger<JsonSchemaValidator> logger)
        {
            _schemaRegistryClient = schemaRegistryClient;
            _logger = logger;
        }
        public async Task<bool> IsValid(int schemaId, string text)
        {

            var registeredSchema = await _schemaRegistryClient.GetSchemaAsync(schemaId);
            var schema = await JsonSchema.FromJsonAsync(registeredSchema.SchemaString);
            var errors = schema.Validate(text);

            if(errors.Any())
            {
                errors.ToList().ForEach(x => _logger.LogError(x.ToString()));
                return false;
            }

            return true;
        }
    }
}
