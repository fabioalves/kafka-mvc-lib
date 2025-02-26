using System;
using System.Collections.Generic;
using System.Text;

namespace TvOpenPlatform.Consumer.Result.Options
{
    public class ConsumeRestartOptions
    {
        public ConsumeRestartOptions(int delayBeforeRestartInSeconds)
        {
            DelayBeforeRestartInSeconds = delayBeforeRestartInSeconds;
        }
        public int DelayBeforeRestartInSeconds { get; }
    }
}
