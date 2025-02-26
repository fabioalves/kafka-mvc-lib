using System;
using System.Collections.Generic;
using System.Text;

namespace SampleConsumer.Models
{
    public class MyMessage
    {
        public int Id { get; set; }
        public string CorrelationId { get; set; }
    }
}
