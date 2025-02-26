using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Parameters;

namespace TvOpenPlatform.Consumer.UnitTests.Routing.Fakes
{
    public class AthenaTestController
    {
        [AthenaTopic("test", "component", "group", "key")]
        public Task AthenaTestAction()
        {
            return Task.CompletedTask;
        }

        [AthenaTopic("test2", "component1", "group1", "key1")]
        public Task AthenaTestActionWithoutDefaultValue()
        {
            return Task.CompletedTask;
        }
    }
}
