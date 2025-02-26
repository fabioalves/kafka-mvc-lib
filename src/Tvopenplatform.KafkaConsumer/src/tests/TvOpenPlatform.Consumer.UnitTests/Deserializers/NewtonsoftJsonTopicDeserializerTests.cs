using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.UnitTests.Deserializers.Models;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.Deserializers
{
    public class NewtonsoftJsonTopicDeserializerTests
    {
        [Fact]
        public async Task Should_Deserialize_When_Have_Custom_Constructor()
        {
            var jsonText = "{ \"id\":1, \"value\":\"text\" }";
            var newtonsoftJson = new NewtonsoftJsonTopicDeserializer(Mock.Of<ILogger>());
            var deserializedObject = await newtonsoftJson.DeserializeAsync(typeof(TestMessageCustomCtor), Encoding.UTF8.GetBytes(jsonText));
            var result = (TestMessageCustomCtor)deserializedObject;
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task Should_Deserialize_When_Have_Default_Constructor()
        {
            var jsonText = "{ \"id\":1, \"value\":\"text\" }";
            var newtonsoftJson = new NewtonsoftJsonTopicDeserializer(Mock.Of<ILogger>());
            var result = (TestMessage) await newtonsoftJson.DeserializeAsync(typeof(TestMessage), Encoding.UTF8.GetBytes(jsonText));

            Assert.Equal(1, result.Id);
        }
    }
}
