using SampleDomain.Events;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Parameters;
using TvOpenPlatform.Consumer.Result;

namespace SampleConsumer.Controllers
{
    public class AthenaTopicsController
    {
        [AthenaTopic(defaultValue: "test-athena", component: "gvp.notifications", "OPEN_PLATFORM", "FEEDBACK_TOPIC")]
        public Task<IResult> MultipleAction(SampleEvent domainEvent)
        {
            return Task.FromResult((IResult)new EmptyResult());
        }
    }
}
