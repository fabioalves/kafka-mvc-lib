namespace SampleConsumer.Models
{
    public class MyHeader
    {
        public MyHeader(int instanceId)
        {
            InstanceId = instanceId;
        }

        public int InstanceId { get; }
    }
}
