using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace TvOpenPlatform.Consumer.Extensions
{
    public static class PropertyExtensions
    {
        public static T FillDefaultProperties<T>(this T obj) where T : class
        {
            var props = obj.GetType().GetProperties();
            foreach (var prop in props.Where(x => x.GetValue(obj) == null))
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                    prop.SetValue(obj, d.Value);
            }

            return obj;
        }
    }
}
