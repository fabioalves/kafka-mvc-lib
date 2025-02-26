using SampleDomain.Events;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer;

namespace SampleConsumer.Controllers
{
    public class MultipleTopicsController
    {
        [Topic("gvp-multiple-1,gvp-multiple-2,gvp-multiple-3")]
        public Task MultipleAction(SampleEvent domainEvent)
        {
            return Task.CompletedTask;
        }
    }
}
