using SampleDomain.Events;
using TvOpenPlatform.DDD.Domain.Common;

namespace SampleDomain.EventHandlers
{
    public class MessageHandler : IDomainEventSubscriber<SampleEvent>
    {
        public void Handle(SampleEvent domainEvent)
        {
            
        }
    }
}
