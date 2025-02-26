using System;
using System.Collections.Generic;
using System.Text;

namespace TvOpenPlatform.KafkaClient.Models
{
    public class ClientConfig
    {
        public string BootstrapServers { get; set; }
        public int SocketSendBufferBytes { get; set; }
        public int SocketReceiveBufferBytes { get; set; }
        public bool SocketKeepAliveEnable { get; set; }
        public bool SocketNagleDisable { get; set; }
        public int SocketTimeoutMs { get; set; }
        public object AckMode { get; set; }        
        public int MessageMaxBytes { get; set; } = 1000000;
        public string TopicBlacklist { get; set; }
        public int TopicMetadataRefreshIntervalMs { get; set; } = 300000;
        public string Debug { get; set; }
        public int SocketMaxFails { get; set; } = 1;
        public bool LogConnectionClose { get; set; } = true;
        public int StatisticsIntervalMs { get; set; }

    }
}
