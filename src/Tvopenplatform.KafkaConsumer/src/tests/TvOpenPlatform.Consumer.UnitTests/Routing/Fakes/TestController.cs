using System.Threading.Tasks;

namespace TvOpenPlatform.Consumer.UnitTests.Routing.Fakes
{
    public class TestController
    {
        [Topic(Constants.TestTopic)]
        public Task TestAction()
        {
            return Task.CompletedTask;
        }

        [Topic(Constants.TestTopic2)]
        public Task TestAction2()
        {
            return Task.CompletedTask;
        }

        [Topic(Constants.MultipleTopic)]
        public Task MultipleTopicAction()
        {
            return Task.CompletedTask;
        }
    }
}
