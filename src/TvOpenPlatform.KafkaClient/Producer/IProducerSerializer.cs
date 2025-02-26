namespace TvOpenPlatform.KafkaClient
{
    public interface IProducerSerializer
    {
        string Serialize<T>(T obj);
    }
}