using Newtonsoft.Json;
using System;

namespace TvOpenPlatform.Consumer.UnitTests.Deserializers.Models
{
    public class TestMessage
    {
        public long Id { get; set; }
        public string Value { get; set; }

        public string GetSchemaJson()
        {
            return @"{
  'description': 'Test Message',
  'type': 'object',
  'properties':
  {
    'Id': {'type':'number'},
    'Value': {'type':'string'},    
  }
}";
        }

        public string GetObjectAsText()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
