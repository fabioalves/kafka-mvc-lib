using Microsoft.Extensions.Logging;
using SampleConsumer.Models;
using SampleDomain.Events;
using System.Net.Http;
using System.Threading.Tasks;
using TvOpenPlatform.Consumer;
using TvOpenPlatform.Consumer.Message;
using TvOpenPlatform.Consumer.Result;
using TvOpenPlatform.Consumer.Result.Options;
using TvOpenPlatform.DDD.Domain.Common;

namespace SampleConsumer.Controllers
{
    public class SampleController
    {
        private IEventDispatcher _dispatcher;
        private ILogger<SampleController> _logger;
        private IHttpClientFactory _httpClientFactory;
        public SampleController(IEventDispatcher dispatcher, IHttpClientFactory httpClientFactory, ILogger<SampleController> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [Topic("gvp-test-json-4")]
        public async Task<IResult> SampleAction(SampleEvent domainEvent)
        {
            var teste = domainEvent;
           
            _logger.LogInformation("Executing Action A");
            _dispatcher.Dispatch(domainEvent);
            return await Task.FromResult(new ConsumeRestartResult(new ConsumeRestartOptions(10)));
        }

        [Topic("gvp-sample-message")]
        public async Task SampleActionWithBody(IMessageContext messageContext, MyMessage message)
        {
            _logger.LogInformation("Executing Action C");
            await Task.Delay(3000);
        }

        [Topic("gvp-sample-message-with-request")]
        public async Task SampleActionWithRequest(IMessageContext messageContext, MyMessage message)
        {
            _logger.LogInformation("Executing Action C");
            await _httpClientFactory
               .CreateClient("teste")
               .GetAsync("http://docker.gvp-dev.com/common/mockingbird/mock/weather/1/api");


            await Task.Delay(15000);

            await _httpClientFactory
               .CreateClient("teste")
               .GetAsync("http://docker.gvp-dev.com/common/mockingbird/mock/weather/1/api");

        }

        [Topic("gvp-sample-event-with-context")]
        public async Task SampleActionWithContext(IMessageContext messageContext, SampleEvent domainEvent)
        {
            var teste = domainEvent;
            var key = messageContext.Key;

            _logger.LogInformation("Executing Action B");
            await Task.Delay(4000);
            _dispatcher.Dispatch(domainEvent);
        }

        [Topic("gvp-test-json-complex-message")]
        public async Task<IResult> SampleAction(MyComplexMessage message)
        {
            var teste = message;

            _logger.LogInformation("Executing deserializing complex message");
            return await Task.FromResult(new ConsumeRestartResult(new ConsumeRestartOptions(10)));
        }
    }
}
