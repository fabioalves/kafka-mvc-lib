using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.SchemaRegistry;
using TvOpenPlatform.Consumer.UnitTests.Deserializers.Models;
using Xunit;
using static TvOpenPlatform.Consumer.UnitTests.Deserializers.Models.TestMessageLists;

namespace TvOpenPlatform.Consumer.UnitTests.Deserializers
{
    public class JsonSchemaTopicDeserializerTests
    {
        private readonly string testTopic;
        private readonly ISchemaRegistryClient schemaRegistryClient;
        private Dictionary<string, int> store = new Dictionary<string, int>();

        public JsonSchemaTopicDeserializerTests()
        {
            testTopic = "topic";
            var schemaRegistryMock = new Mock<ISchemaRegistryClient>();
            schemaRegistryMock.Setup(x => x.ConstructValueSubjectName(testTopic, It.IsAny<string>())).Returns($"{testTopic}-value");
            schemaRegistryMock.Setup(x => x.RegisterSchemaAsync("topic-value", It.IsAny<string>())).ReturnsAsync(
                (string topic, string schema) => store.TryGetValue(schema, out int id) ? id : store[schema] = store.Count + 1
            );
            schemaRegistryMock.Setup(x => x.GetSchemaAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(
                (int id, string format) => new Schema(store.Where(x => x.Value == id).First().Key, null, SchemaType.Json)
            );
            schemaRegistryClient = schemaRegistryMock.Object;
        }

        [Fact]
        public async Task Should_Deserialize_When_Have_Default_Constructor()
        {
            var message = new TestMessage
            {
                Id = 1,
                Value = "text"
            };

            var messageAsText = message.GetObjectAsText();
            var serializedObject = await Serialize(message);
            var jsonSchemaValidator = new Mock<IJsonSchemaValidator>();
            jsonSchemaValidator.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var deserializer = new JsonSchemaTopicDeserializer(Mock.Of<ILogger>(), jsonSchemaValidator.Object);
            var deserializedObject = await deserializer.DeserializeAsync(typeof(TestMessage), serializedObject);
            var result = (TestMessage)deserializedObject;

            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task Should_Deserialize_When_Have_Default_Constructor_ListObject()
        {
            var message = new TestMessageLists
            {
                ProgramTitle = new List<TitleModel>(),
                LongTitle1 = new List<TitleModel>(),
                SimpleTitleModel = new TitleModel() { LanguageCode = "xis", Value = "TitleModel single object" },
                EnvironmentId = "ABCDEFGHIJKLMNOPQRSTUVWXYZA",
                EnvironmentTimeStamp = 864,
                Id = 1
            };
            message.ProgramTitle.Add(new TitleModel() { LanguageCode = "es", Value = "Las recetas" });
            message.LongTitle1.Add(new TitleModel() { LanguageCode = "AB", Value = "ABCDLO" });

            var messageAsText = JsonConvert.SerializeObject(message);
            var serializedObject = await Serialize(message);
            var jSchemaValidatingReaderFactoryMock = new Mock<IJsonSchemaValidator>();
            jSchemaValidatingReaderFactoryMock.Setup(x => x.IsValid(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var deserializer = new JsonSchemaTopicDeserializer(Mock.Of<ILogger>(), jSchemaValidatingReaderFactoryMock.Object);
            var deserializedObject = await deserializer.DeserializeAsync(typeof(TestMessageLists), serializedObject);
            var result = (TestMessageLists)deserializedObject;

            Assert.Equal(1, result.Id);
        }

        private async Task<byte[]> Serialize<T>(T text) where T : class, new()
        {
            var jsonSerializer = new JsonSerializer<T>(schemaRegistryClient);
            return await jsonSerializer.SerializeAsync(text, SerializationContext.Empty);
        }
    }
}
