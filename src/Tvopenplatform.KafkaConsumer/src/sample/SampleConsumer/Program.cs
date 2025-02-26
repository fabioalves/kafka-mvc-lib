using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using TvOpenPlatform.Consumer.Consumers;

namespace SampleConsumer
{
    class Program
    {
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            _serviceProvider = Startup.Configure<Program>();
            IEnumerable<IConsumer> consumers = _serviceProvider.GetServices<IConsumer>();
            consumers.ToList().ForEach(_ => _.Run());
        }
    }
}
