using System.ComponentModel;

namespace TvOpenPlatform.KafkaClient.Models
{
    public enum DefaultTopicConfigAckModeEnum
    {
        [Description("all")]
        All = 1,
        [Description("1")]
        One = 2,
        [Description("0")]
        Zero = 3
    }
    public static class AttributesHelperExtension
    {
        public static string GetStringValue(this DefaultTopicConfigAckModeEnum value)
        {
            var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return da.Length > 0 ? da[0].Description : value.ToString();
        }
    }
}
