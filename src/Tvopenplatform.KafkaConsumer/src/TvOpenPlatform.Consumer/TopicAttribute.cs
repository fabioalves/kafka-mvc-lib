using System;

namespace TvOpenPlatform.Consumer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TopicAttribute : Attribute
    {
        public string Name { get; }
        public string Version { get; }
        public TopicType Type { get; }
        public TopicAttribute(string name, string version = null, TopicType type = TopicType.Default)
        {
            Name = name;
            Version = version;
            Type = type;
        }
    }
}