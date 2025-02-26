using System.Collections.Generic;
using TvOpenPlatform.Consumer;

namespace SampleConsumer
{
    public class TopicNameRedirection : ITopicNameRedirection
    {        
        public IDictionary<string, string> TopicKeyMapping { get; set; }
    }
}
