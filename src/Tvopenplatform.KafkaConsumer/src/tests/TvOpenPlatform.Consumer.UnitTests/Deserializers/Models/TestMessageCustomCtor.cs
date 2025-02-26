namespace TvOpenPlatform.Consumer.UnitTests.Deserializers.Models
{
    public class TestMessageCustomCtor
    {
        public TestMessageCustomCtor(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }
        public string Value { get; }
    }
}
