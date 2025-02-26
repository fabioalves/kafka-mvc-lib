using System;
using System.Threading.Tasks;

namespace TvOpenPlatform.Consumer.Deserializers
{
    public interface ITopicDeserializer
    {
        Task<object> DeserializeAsync(Type type, byte[] message);
    }
}
