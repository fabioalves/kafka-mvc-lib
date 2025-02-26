using System;

namespace TvOpenPlatform.KafkaClient.Consumer
{
    public interface IMessageHandler
    {
        void Handle(Type type, string messageValue);
    }
}
