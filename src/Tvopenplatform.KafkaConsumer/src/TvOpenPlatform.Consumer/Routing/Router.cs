using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Deserializers;
using TvOpenPlatform.Consumer.Message;
using TvOpenPlatform.Consumer.Result;

namespace TvOpenPlatform.Consumer.Routing
{
    public class Router : IRouter
    {
        private readonly Dictionary<string, Route> _routes;
        private readonly IServiceProvider _provider;
        private readonly ITopicDeserializer _topicDeserializer;
        private readonly ILogger _logger;

        public IEnumerable<string> DefaultTopics => GetTopics(TopicType.Default);
        public IEnumerable<string> RetryTopics => GetTopics(TopicType.Retry);

        public Router(IServiceProvider provider, ITopicDeserializer topicDeserializer, ILogger logger)
        {
            _provider = provider;
            _topicDeserializer = topicDeserializer;
            _logger = logger;
            _routes = new Dictionary<string, Route>();
        }

        private IEnumerable<string> GetTopics(TopicType topicType)
        {
            return _routes.Where(x => x.Value.Topic.Type == topicType).Select(x => x.Key);
        }

        public void Add(string name, Route route)
        {
            if (_routes.ContainsKey(name))
                throw new ArgumentException($"Too many actions to topic {name}");

            _routes[name] = route;
        }

        public async Task<IResult> Execute(IMessageContext messageContext, byte[] message)
        {
            Route route = _routes[messageContext.Topic];
            return await BuildAction(messageContext, message, route).ConfigureAwait(false);
        }

        private async Task<IResult> BuildAction(IMessageContext messageContext, byte[] message, Route route)
        {
            var parameters = await ParseParameters(route, message, messageContext);
            messageContext.SetCorrelationId(parameters);

            using (var scope = _provider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                var controller = ActivatorUtilities.CreateInstance(provider, route.Controller);
                _logger.LogDebug($"Controller instatiated: {controller.GetType()}");

                Dictionary<string, object> data = GetScopeInformation(messageContext);

                using (_logger.BeginScope(data))
                {
                    Trace.CorrelationManager.StartLogicalOperation(messageContext.CorrelationId ?? Guid.NewGuid().ToString());
                    var response = await route.Run(controller, parameters).ConfigureAwait(false);
                    Trace.CorrelationManager.StopLogicalOperation();
                    return response;
                }

            }
        }

        private static Dictionary<string, object> GetScopeInformation(IMessageContext messageContext)
        {
            return new Dictionary<string, object>
            {
                ["CorrelationId"] = messageContext.CorrelationId
            };
        }

        public static object GetPropertyValue(object car, string propertyName)
        {
            return car.GetType().GetProperties()
               .Single(pi => pi.Name == propertyName)
               .GetValue(car, null);
        }

        private async Task<object[]> ParseParameters(Route route, byte[] message, IMessageContext messageContext)
        {
            _logger.LogDebug("Trying to parse parameters");
            ParameterInfo[] parameters = route.Action.GetParameters();
            var parsedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                if (typeof(IMessageContext).IsAssignableFrom(parameters[i].ParameterType))
                {
                    parsedParameters[i] = messageContext;
                }
                else
                {
                    parsedParameters[i] = await _topicDeserializer.DeserializeAsync(parameters[i].ParameterType, message).ConfigureAwait(false);
                }
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Parameters parsed: {string.Join(",", parsedParameters)}");
            }

            return parsedParameters;
        }
    }
}
