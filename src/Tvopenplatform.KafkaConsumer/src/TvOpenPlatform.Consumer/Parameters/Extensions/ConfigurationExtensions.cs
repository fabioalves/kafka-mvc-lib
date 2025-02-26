using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TvOpenPlatform.Settings.Athena;

namespace TvOpenPlatform.Consumer.Parameters.Extensions
{
    public static class ConfigurationExtensions
    {
        public static ServiceCollection AddAthenaTopics(this ServiceCollection services, Assembly assembly, IList<int> instanceIds)
        {
            services.AddSingleton<IAthenaTopicNames>(provider =>
            {
                var athenaClient = provider.GetService<IAthenaClientAsync>();
                return new AthenaTopicNames(athenaClient, assembly, instanceIds);
            });
            return services;
        }
    }
}
