using System.Collections.Generic;

namespace TvOpenPlatform.Consumer.Message
{
    public interface IMessageContext
    {
        string CorrelationId { get; }
        Dictionary<string, byte[]> Headers { get; }
        string Key { get; }
        string Topic { get; }
        void SetCorrelationId(object[] actionParameters);
    }
}