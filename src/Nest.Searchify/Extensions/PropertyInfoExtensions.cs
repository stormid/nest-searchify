using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nest.Searchify.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static IEnumerable<KeyValuePair<string, PropertyInfo>> ToParameterLookups(this PropertyInfo[] props)
        {
            if (props == null)
            {
                throw new ArgumentNullException(nameof(props));
            }

            return props.Select(ToParameterLookup);
        }


        public static KeyValuePair<string, PropertyInfo> ToParameterLookup(this PropertyInfo prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            return new KeyValuePair<string, PropertyInfo>(GetPropertyName(prop), prop);
        }

        private static string GetPropertyName(PropertyInfo prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            return prop.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? prop.Name;
        }
    }
}
