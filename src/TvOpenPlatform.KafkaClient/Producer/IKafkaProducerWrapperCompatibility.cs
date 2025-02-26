using System;
using TvOpenPlatform.KafkaClient.Models;

namespace TvOpenPlatform.KafkaClient
{
    [Obsolete("Prefer using IProducer")]
    public interface IKafkaProducerWrapper<A, B> : IKafkaProducerWrapper
    {

    }
}