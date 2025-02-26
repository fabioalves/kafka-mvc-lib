using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleConsumer.Models
{
    public class MyComplexMessage
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("programTitle")]
        public List<TitleModel> ProgramTitle { get; set; }
        [JsonProperty("longTitle1")]
        public List<TitleModel> LongTitle1 { get; set; }

        [JsonProperty("envID")]
        public string EnvironmentId { get; set; }

        [JsonProperty("envTimestamp")]
        public long EnvironmentTimeStamp { get; set; }

        [JsonProperty("simpleTitleModel")]
        public TitleModel SimpleTitleModel { get; set; }
    }

    public class TitleModel
    {
        [JsonProperty("langCode")]
        public string LanguageCode { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    //Example: {"id": "1","programTitle":[{"langCode":"es","value":"Las recetas"}], "longTitle1":[{"langCode":"AB","value":"ABCDLO"}], "envID":"ABCDEFGHIJKLMNOPQRSTUVWXYZA","envTimestamp":864,"simpleTitleModel":{"langCode":"sdd","value":"singleTitle model"}}
}
