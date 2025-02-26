using System.Collections.Generic;
using System.ComponentModel;
using TvOpenPlatform.Consumer.Extensions;

namespace TvOpenPlatform.Consumer.UnitTests.Routing.Helpers
{
    public class TopicNameRedirection : ITopicNameRedirection
    {
        public IDictionary<string, string> TopicKeyMapping { get; set; }
    }
}
