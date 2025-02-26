using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TvOpenPlatform.TraceIdentifier;

namespace SampleConsumer
{
    class TraceIdentifier : ITraceIdentifier
    {
        public string GetKey()
        {
            return "X-Request-Id";
        }

        public string GetValue()
        {
            if (Trace.CorrelationManager.LogicalOperationStack.Count > 0)
            {
                return Trace.CorrelationManager.LogicalOperationStack.Peek().ToString();
            }

            return string.Empty;
        }
    }
}
