using System.Threading;

namespace TvOpenPlatform.Consumer.Consumers
{
    public interface IConsumer
    {
        void Run(CancellationToken cancellation = default);
    }
}
