using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer.Result;

namespace TvOpenPlatform.Consumer.Routing
{
    public class Route
    {
        private readonly ILogger _logger;

        public Route(MethodInfo method, ILogger logger)
        {
            Topic = GetTopic(method);
            Controller = method.ReflectedType;
            Action = method;
            _logger = logger;
        }

        private TopicAttribute GetTopic(MethodInfo method)
        {
            return method.GetCustomAttribute<TopicAttribute>();
        }

        public TopicAttribute Topic { get; }
        public Type Controller { get; }
        public MethodInfo Action { get; }

        public async Task<IResult> Run(object controller, params object[] parameters)
        {
            _logger.LogDebug($"Invoking action {Action.Name} on controller {controller.GetType()}");
            if (IsResultReturnType())
            {
                var result = await ((Task<IResult>)Action.Invoke(controller, parameters)).ConfigureAwait(false);

                if (result == null)
                    return new EmptyResult();

                return result;
            }
            else
            {
                await ((Task)Action.Invoke(controller, parameters)).ConfigureAwait(false);
                return await Task.FromResult(new EmptyResult());
            }
        }

        private bool IsResultReturnType()
        {
            return (Action.ReturnType.IsAssignableFrom(typeof(Task<IResult>)) && Action.ReturnType.IsGenericType) 
                || Action.ReturnType.IsAssignableFrom(typeof(IResult));
        }
    }
}
