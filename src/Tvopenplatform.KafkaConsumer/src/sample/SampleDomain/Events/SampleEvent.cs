using Newtonsoft.Json;
using TvOpenPlatform.DDD.Domain.Common;

namespace SampleDomain.Events
{
    public class SampleEvent : IDomainEvent
    {
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "age")]
        public long Age { get; set; }

        //"firstName":"test","lastName":"surname","age":30
    }
}
