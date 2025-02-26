using System.Collections.Generic;
using TvOpenPlatform.Settings.Athena;
using System.Linq;

namespace TvOpenPlatform.Consumer.Parameters.Extensions
{
    public static class AthenaExtensions
    {
        public static IEnumerable<string> Get(this IAthenaClientAsync athenaClient, IList<int> instanceIds, string component, string group, string key)
        {
            var parametersByInstance = new List<string>();

            if (instanceIds.Count > 0)
            {
                parametersByInstance.AddRange(GetParametersByInstance(athenaClient, instanceIds, component, group, key));
            }
            else
            {
                parametersByInstance.AddRange(GetDefaultParameter(athenaClient, component, group, key));
            }

            return parametersByInstance;
        }

        private static IEnumerable<string> GetDefaultParameter(IAthenaClientAsync athenaClient, string component, string group, string key)
        {
            var parameters = athenaClient?.GetParameters(0, component, group, key);

            if (parameters != null)
                yield return FlattenParameters(parameters);
        }

        private static IEnumerable<string> GetParametersByInstance(IAthenaClientAsync athenaClient, IList<int> instanceIds, string component, string group, string key)
        {
            var instanceValues = athenaClient?.ListValues(component, group, key, partitions: null);

            if (instanceValues == null) return new List<string>();

            return instanceValues.Where(x => x.Instance == 0 || instanceIds.Contains(x.Instance)).Select(x => x.Value.ToString());
        }

        private static string FlattenParameters(Dictionary<string, Dictionary<string, Dictionary<string, object>>> parameters)
        {
            var parameterFlatten = parameters.SelectMany(p => p.Value).SelectMany(p => p.Value).FirstOrDefault();

            return parameterFlatten.Value.ToString();
        }
    }
}
