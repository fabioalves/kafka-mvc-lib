using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace TvOpenPlatform.KafkaClient.LogAdapter
{
    public static class LogLevelAdapter
    {
        public static Logger.LogLevel ToTvOpenPlatformLogLevel(this SyslogLevel syslogLevel)
        {
            if (syslogLevel == SyslogLevel.Warning || syslogLevel == SyslogLevel.Notice)
            {
                return Logger.LogLevel.Warning;
            }
            else if (syslogLevel == SyslogLevel.Info)
            {
                return Logger.LogLevel.Info;
            }
            else if (syslogLevel == SyslogLevel.Error)
            {
                return Logger.LogLevel.Error;
            }
            else if (syslogLevel == SyslogLevel.Critical || syslogLevel == SyslogLevel.Emergency || syslogLevel == SyslogLevel.Alert)
            {
                return Logger.LogLevel.Critical;
            }
            else
            {
                return Logger.LogLevel.Verbose;
            }
        }
    }
}
