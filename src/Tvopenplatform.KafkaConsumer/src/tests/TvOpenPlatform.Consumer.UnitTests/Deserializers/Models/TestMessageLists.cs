using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace TvOpenPlatform.Consumer.UnitTests.Deserializers.Models
{
    public class TestMessageLists
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

        public string GetSchemaJson()
        {
            var schema = new JsonSchema();
            schema.Type = JsonSchemaType.Object;
            schema.Properties = new Dictionary<string, JsonSchema>
            {
                { "id", new JsonSchema { Type = JsonSchemaType.Integer } },
                {
                    "programTitle",
                    new JsonSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new List<JsonSchema>
                        {
                            new JsonSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, JsonSchema>
                                {
                                    {
                                        "langCode",
                                        new JsonSchema { Type = JsonSchemaType.String }
                                    },
                                    {
                                        "value",
                                        new JsonSchema { Type = JsonSchemaType.String }
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "longTitle1",
                    new JsonSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new List<JsonSchema>
                        {
                            new JsonSchema
                            {
                                Type = JsonSchemaType.Object,
                                Properties = new Dictionary<string, JsonSchema>
                                {
                                    {
                                        "langCode",
                                        new JsonSchema { Type = JsonSchemaType.String }
                                    },
                                    {
                                        "value",
                                        new JsonSchema { Type = JsonSchemaType.String }
                                    }
                                }
                            }
                        }
                    }
                },
                { "envID", new JsonSchema { Type = JsonSchemaType.String } },
                { "envTimestamp", new JsonSchema { Type = JsonSchemaType.Integer } },
                {
                    "simpleTitleModel",
                    new JsonSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, JsonSchema>
                        {
                            {
                                "langCode",
                                new JsonSchema { Type = JsonSchemaType.String }
                             },
                             {
                                "value",
                                new JsonSchema { Type = JsonSchemaType.String }
                             }
                         }
                     }
                }
            };

            return schema.ToString();
        }

        public class TitleModel
        {
            [JsonProperty("langCode")]
            public string LanguageCode { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
        }
    }
}
