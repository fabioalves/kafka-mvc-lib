using System.Collections.Generic;

namespace TvOpenPlatform.Consumer
{
    public interface ITopicNameRedirection
    {
        IDictionary<string, string> TopicKeyMapping { get; set; }
    }
}
