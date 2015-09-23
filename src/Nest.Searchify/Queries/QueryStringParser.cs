using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;

namespace Nest.Searchify.Queries
{
    public static class QueryStringParser<TParameters> where TParameters : IParameters
    {
        private static readonly IDictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>> Resolvers = new Dictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>>()
        {
            { typeof(IEnumerable<string>), ParseToStringArray },
            { typeof(string), ParseToString },
            { typeof(IEnumerable<int>), ParseToIntegerArray },
            { typeof(IEnumerable<double>), ParseToDoubleArray },
            { typeof(double), ParseToDouble },
            { typeof(int), ParseToInteger },
            { typeof(double?), ParseToDouble },
            { typeof(int?), ParseToInteger },
            { typeof(SortDirectionOption?), ParseToEnum<SortDirectionOption> },
            { typeof(GeoPoint), ParseToGeoPoint }
        };

        private static readonly IDictionary<Type, Action<NameValueCollection, object, string>> Converters = new Dictionary
            <Type, Action<NameValueCollection, object, string>>()
        {
            { typeof (IEnumerable<string>), ParseFromStringArray<string> },
            { typeof (IEnumerable<int>), ParseFromStringArray<int> },
            { typeof (IEnumerable<double>), ParseFromStringArray<double> },
            { typeof (string), ParseFromString },
            { typeof (int), ParseFromString },
            { typeof (double), ParseFromString },
            { typeof (int?), ParseFromString },
            { typeof (double?), ParseFromString },
            { typeof (SortDirectionOption?), ParseFromString },
            { typeof (GeoPoint), ParseFromGeoPoint },
        };

        public static void AddResolver<T>(Func<TParameters, PropertyInfo, NameValueCollection, string, object> resolver)
        {
            if (!Resolvers.ContainsKey(typeof(T)))
            {
                Resolvers.Add(typeof(T), resolver);
            }
            else
            {
                Resolvers[typeof(T)] = resolver;
            }
        }

        public static void AddConverter<T>(Action<NameValueCollection, object, string> converter)
        {
            if (!Converters.ContainsKey(typeof(T)))
            {
                Converters.Add(typeof(T), converter);
            }
            else
            {
                Converters[typeof(T)] = converter;
            }
        }
        public static NameValueCollection Parse(string queryString)
        {
            return HttpUtility.ParseQueryString(queryString);
        }

        public static NameValueCollection Parse(TParameters parameters)
        {
            var nvc = Parse("");
            var properties = typeof(TParameters).GetProperties();
            foreach (var prop in properties.OrderBy(o => o.Name))
            {
                if (Converters.ContainsKey(prop.PropertyType))
                {
                    var o = prop.GetValue(parameters);
                    Converters[prop.PropertyType](nvc, o, prop.Name.Camelize());
                }
            }
            return Sort(nvc);
        }

        private static NameValueCollection Sort(NameValueCollection nvc)
        {
            var sortedNvc = Parse("");
            foreach (var key in nvc.AllKeys.OrderBy(s => s))
            {
                var values = nvc.GetValues(key);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        sortedNvc.Add(key, value);
                    }
                }
            }
            return sortedNvc;
        }

        private static void ParseFromStringArray<T>(NameValueCollection nvc, object values, string propertyName)
        {
            var stringValues = values as IEnumerable<T>;
            if (stringValues != null)
            {
                foreach (var value in stringValues)
                {
                    nvc.Add(propertyName, value.ToString());
                }
            }
        }

        private static void ParseFromString(NameValueCollection nvc, object value, string propertyName)
        {
            if (value != null) nvc.Add(propertyName, value.ToString());
        }

        private static void ParseFromGeoPoint(NameValueCollection nvc, object value, string propertyName)
        {
            var point = value as GeoPoint;
            if(point != null) nvc.Add(propertyName, point.ToString());
        }

        private static IEnumerable<string> ParseToStringArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            var values = nvc.GetValues(key);
            if (values != null && values.Any())
            {
                return values;
            }
            throw new InvalidCastException($"Unable to parse [{key}] as array of string");
        }

        private static IEnumerable<double> ParseToDoubleArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            var values = nvc.GetValues(key)?.Select(double.Parse).ToList();
            if (values != null && values.Any())
            {
                return values;
            }
            return null;
        }

        private static IEnumerable<int> ParseToIntegerArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            var values = nvc.GetValues(key)?.Select(int.Parse).ToList();
            if (values != null && values.Any())
            {
                return values;
            }
            return null;
        }

        public static string ParseToString(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            return nvc[key];
        }

        private static GeoPoint ParseToGeoPoint(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            var value = nvc.Get(key);
            var points = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
            return new GeoPoint(points[0], points[1]);
        }

        private static object ParseToInteger(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            return int.Parse(nvc[key]);
        }

        private static object ParseToDouble(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            return double.Parse(nvc[key]);
        }

        private static object ParseToEnum<TEnum>(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            if (nvc.AllKeys.Contains(key))
            {
                return (TEnum)Enum.Parse(typeof(TEnum), nvc[key], true);
            }
            return null;
        }

        public static void Populate(NameValueCollection nvc, TParameters parameters)
        {
            var properties = typeof(TParameters).GetProperties();
            foreach (var key in nvc.AllKeys)
            {
                var prop =
                    properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (prop != null)
                {
                    if (Resolvers.ContainsKey(prop.PropertyType))
                    {
                        try
                        {
                            var value = Resolvers[prop.PropertyType](parameters, prop, nvc, key);
                            prop.SetValue(parameters, value);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidCastException($"Unable to parse [{key}] as [{prop.PropertyType.Name}]", ex);
                        }
                    }
                }
            }
        }
    }
}
