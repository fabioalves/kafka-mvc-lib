using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TvOpenPlatform.KafkaClient.Models;

namespace TvOpenPlatform.KafkaClient.Producer
{
    public interface IProducer
    {        
        Task ProduceAndAwaitDeliveryAsync<T>(MessageWrapper<T> message);
        Task ProduceAndForgetDeliveryAsync<T>(MessageWrapper<T> message);
    }
}
