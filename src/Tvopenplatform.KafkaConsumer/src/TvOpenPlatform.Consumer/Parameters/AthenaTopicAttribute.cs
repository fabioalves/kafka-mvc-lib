using System;

namespace TvOpenPlatform.Consumer.Parameters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AthenaTopicAttribute : TopicAttribute
    {
        public AthenaTopicAttribute(string defaultValue, string component, string group, string key, string version = null, TopicType type = TopicType.Default)
            : base(defaultValue, version, type)
        {
            Component = component;
            Group = group;
            Key = key;
        }

        public string Component { get; }
        public string Group { get; }
        public string Key { get; }
    }
}