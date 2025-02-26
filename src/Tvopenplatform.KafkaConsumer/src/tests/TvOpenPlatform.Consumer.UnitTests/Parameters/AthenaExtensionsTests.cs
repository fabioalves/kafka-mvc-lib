using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TvOpenPlatform.Consumer.Parameters.Extensions;
using TvOpenPlatform.Settings.Athena;
using Xunit;

namespace TvOpenPlatform.Consumer.UnitTests.Parameters
{
    public class AthenaExtensionsTests
    {
        [Fact(Skip = "live test")]
        public void Test()
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://docker.gvp-dev.com/dev85/athena/")
            };
            var athenaClient = new AthenaClient(httpClient, null, defaultPartitions: new List<int> { 0 });



            var result = athenaClient.Get(new List<int> { 2, 25 }, "gvp.notifications", "OPEN_PLATFORM", "FEEDBACK_TOPIC");

            //var result = athenaClient.Get(new List<int> { 2, 25 }, "gvp.notifications", "OPEN_PLATFORM", "FEEDBACK_TOPIC");
        }

    }
}
