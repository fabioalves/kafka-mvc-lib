using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TvOpenPlatform.Consumer.SchemaRegistry
{
    public interface IJsonSchemaValidator
    {
        Task<bool> IsValid(int schemaId, string text);
    }
}
