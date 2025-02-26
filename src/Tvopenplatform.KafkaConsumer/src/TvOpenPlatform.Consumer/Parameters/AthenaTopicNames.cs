using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TvOpenPlatform.Settings.Athena;
using TvOpenPlatform.Consumer.Parameters.Extensions;

namespace TvOpenPlatform.Consumer.Parameters
{
    public class AthenaTopicNames : IAthenaTopicNames
    {
        public AthenaTopicNames(IAthenaClientAsync athenaClient, Assembly assembly, IList<int> instanceIds)
        {
            MethodInfo[] methods = assembly.GetTypes()
                                .Where(t => t.Name.EndsWith("Controller"))
                                .SelectMany(x => x.GetMethods())
                                .Where(x => x.GetCustomAttribute<AthenaTopicAttribute>() != null)
                                .ToArray();

            TopicsByMethod = new Dictionary<string, string>();
            foreach (MethodInfo method in methods)
            {
                var athenaTopic = method.GetCustomAttribute<AthenaTopicAttribute>();
                
                if (athenaTopic == null) continue;

                var topicsByInstance = athenaClient.Get(instanceIds, athenaTopic.Component, athenaTopic.Group, athenaTopic.Key);

                TopicsByMethod.Add(athenaTopic.Name, string.Join(",", topicsByInstance.Select(x => x).Distinct()));
            }
        }

        public IDictionary<string, string> TopicsByMethod { get; set; }
    }
}
