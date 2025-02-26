using System.Collections.Generic;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Message;
using TvOpenPlatform.Consumer.Result;

namespace TvOpenPlatform.Consumer.Routing
{
    public interface IRouter
    {
        IEnumerable<string> DefaultTopics { get; }
        IEnumerable<string> RetryTopics { get; }
        void Add(string name, Route route);
        Task<IResult> Execute(IMessageContext messageContext, byte[] message);
    }
}
