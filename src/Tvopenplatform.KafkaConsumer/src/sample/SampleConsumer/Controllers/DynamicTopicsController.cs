using SampleDomain.Events;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer;
using TvOpenPlatform.Consumer.Result;

namespace SampleConsumer.Controllers
{
    public class DynamicTopicsController
    {
        [Topic("gvp-dynamic-topic1")]
        public Task<IResult> DynamicTopicAction(SampleEvent simpleEvent)
        {
            return Task.FromResult((IResult)new EmptyResult());
        }
    }
}
