using System.Collections.Generic;

namespace TvOpenPlatform.Consumer.Parameters
{
    public interface IAthenaTopicNames
    {
        IDictionary<string, string> TopicsByMethod { get; set; }
    }
}