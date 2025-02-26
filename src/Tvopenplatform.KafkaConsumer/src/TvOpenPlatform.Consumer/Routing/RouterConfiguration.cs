using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TvOpenPlatform.Consumer.Parameters;
using TvOpenPlatform.Consumer.Routing;

namespace TvOpenPlatform.Consumer
{
    public class RouterConfiguration
    {
        private Dictionary<string, string> _renameTo;
        private IEnumerable<string> _filterTopics;
        private IEnumerable<string> _excludedTopics;
        private IRouter _router;
        private readonly ILogger _logger;

        public RouterConfiguration(IRouter router, ILogger logger)
        {
            _router = router;
            _logger = logger;
            _filterTopics = Array.Empty<string>();
            _excludedTopics = Array.Empty<string>();
            _renameTo = new Dictionary<string, string>();
        }

        public RouterConfiguration FilterBy(string filters)
        {
            if (!string.IsNullOrWhiteSpace(filters))
                _filterTopics = filters.Split(',');

            return this;
        }

        public RouterConfiguration Except(string except)
        {
            if (!string.IsNullOrWhiteSpace(except))
                _excludedTopics = except?.Split(',');

            return this;
        }

        public RouterConfiguration WithAthenaRedirect(IAthenaTopicNames topicNameParameters)
        {
            if (topicNameParameters == null)
                return this;

            var redirects = topicNameParameters.TopicsByMethod
                .Select(x =>
                    new
                    {
                        defaultTopicName = x.Key,
                        customTopicName = x.Value
                    })
                .Where(x =>
                    x.defaultTopicName != x.customTopicName
                    && !string.IsNullOrWhiteSpace(x.defaultTopicName)
                    && !string.IsNullOrWhiteSpace(x.customTopicName))
                .ToDictionary(x => x.defaultTopicName, x => x.customTopicName);

            foreach (var redirect in redirects)
            {
                _renameTo.Add(redirect.Key, redirect.Value);
            }

            return this;
        }

        public RouterConfiguration WithRedirect(ITopicNameRedirection topicNameParameters)
        {
            if (topicNameParameters == null)
                return this;

            foreach(var mapping in topicNameParameters.TopicKeyMapping)
            {
                _renameTo.Add(mapping.Key, mapping.Value);
            }

            return this;
        }

        public RouterConfiguration Build(Assembly assembly)
        {
            MethodInfo[] methods = GetMethods(assembly);

            foreach (var method in methods)
            {
                var route = new Route(method, _logger);

                if (string.IsNullOrWhiteSpace(route.Topic.Name))
                {
                    _logger.LogWarning("Topic Name is empty. Check if any annotation is missing [Topic,DefaultValue]");
                    continue;
                }

                var topics = route.Topic.Name.Split(',');
                if (topics.Length > 1)
                {
                    Array.ForEach(topics, (topic) => BuildRoute(route, topic));
                    continue;
                }

                BuildRoute(route, route.Topic.Name);
            }

            return this;
        }

        private void BuildRoute(Route route, string topic)
        {
            _renameTo.TryGetValue(topic, out var redirect);

            var redirectTopics = redirect?.Split(',');
            if (redirectTopics != null && redirectTopics.Length > 0)
            {
                foreach (var redirectTopic in redirectTopics)
                    AddRoute(route, redirectTopic);
            }
            else
            {
                AddRoute(route, topic);
            }
        }

        private void AddRoute(Route route, string topicName)
        {
            if (_excludedTopics.Contains(topicName))
                return;

            if (!_filterTopics.Contains(topicName) && _filterTopics.Any())
                return;

            _router.Add(topicName, route);
        }

        private static MethodInfo[] GetMethods(Assembly assembly)
        {
            return assembly.GetTypes()
                            .Where(t => t.Name.EndsWith("Controller"))
                            .SelectMany(x => x.GetMethods())
                            .Where(x => x.GetCustomAttribute<TopicAttribute>() != null)
                            .ToArray();
        }
    }
}
