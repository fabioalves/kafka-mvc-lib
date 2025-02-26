using Microsoft.Extensions.DependencyInjection;
using SampleDomain.EventHandlers;
using System;
using TvOpenPlatform.DDD.Domain.Common;
using TvOpenPlatform.DDD.Infrastructure.Events;
using TvOpenPlatform.DDD.Infrastructure.IoC;

namespace SampleConsumer
{
    public static class EventInfrastructureConfiguration
    {
        public static ServiceCollection AddEventInfrastructure(this ServiceCollection services)
        {
            services.AddHandlers();
            services.AddTransient<IEventDispatcher>(provider => new EventDispatcher(provider, GetHandlers().AllHandlerTypes));
            services.AddScoped<IEventPublisher, EventPublisher>();
            return services;
        }

        private static CompositeSettings GetHandlers()
        {
            var handlerTypes = new Type[]
            {
                typeof(MessageHandler)
            };

            var handlers = new CompositeSettings()
            {
                AllHandlerTypes = handlerTypes
            };

            return handlers;
        }

        private static ServiceCollection AddHandlers(this ServiceCollection services)
        {
            var handlers = GetHandlers();

            services.AddSingleton(handlers);
            return services;
        }
    }
}
