using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;

namespace Nest.Searchify.Queries
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ParameterAttribute : Attribute
    {
        public string Name { get; }

        public ParameterAttribute(string name)
        {
            Name = name;
        }
    }

    public static class QueryStringParser<TParameters> where TParameters : class, new()
    {
        public static class TypeParsers
        {

            #region Helpers

            public static NameValueCollection Sort(NameValueCollection nvc)
            {
                var sortedNvc = HttpUtility.ParseQueryString("");
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

            public static void ParseFromStringArray<T>(NameValueCollection nvc, object values, string propertyName)
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

            public static void ParseFromString(NameValueCollection nvc, object value, string propertyName)
            {
                if (value != null) nvc.Add(propertyName, value.ToString());
            }

            public static void ParseFromGeoPoint(NameValueCollection nvc, object value, string propertyName)
            {
                var point = value as GeoPoint;
                if (point != null) nvc.Add(propertyName, point.ToString());
            }

            public static IEnumerable<string> ParseToStringArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var values = nvc.GetValues(key);
                if (values != null && values.Any())
                {
                    return values;
                }
                throw new InvalidCastException($"Unable to parse [{key}] as array of string");
            }

            public static IEnumerable<double> ParseToDoubleArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var values = nvc.GetValues(key)?.Select(Double.Parse).ToList();
                if (values != null && values.Any())
                {
                    return values;
                }
                return null;
            }

            public static IEnumerable<int> ParseToIntegerArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var values = nvc.GetValues(key)?.Select(Int32.Parse).ToList();
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

            public static GeoPoint ParseToGeoPoint(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var value = nvc.Get(key);
                var points = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
                return new GeoPoint(points[0], points[1]);
            }

            public static object ParseToInteger(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                return int.Parse(nvc[key]);
            }

            public static object ParseToDouble(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                return double.Parse(nvc[key]);
            }

            public static object ParseToEnum<TEnum>(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key) where TEnum : struct
            {
                if (nvc.AllKeys.Contains(key) && nvc[key] != null)
                {
                    return (TEnum)Enum.Parse(typeof(TEnum), nvc[key], true);
                }
                return null;
            }

            #endregion
        }

        private static readonly IDictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>> Resolvers = new Dictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>>()
        {
            { typeof(IEnumerable<string>), TypeParsers.ParseToStringArray },
            { typeof(string), TypeParsers.ParseToString },
            { typeof(IEnumerable<int>), TypeParsers.ParseToIntegerArray },
            { typeof(IEnumerable<double>), TypeParsers.ParseToDoubleArray },
            { typeof(double), TypeParsers.ParseToDouble },
            { typeof(int), TypeParsers.ParseToInteger },
            { typeof(double?), TypeParsers.ParseToDouble },
            { typeof(int?), TypeParsers.ParseToInteger },
            { typeof(SortDirectionOption?), TypeParsers.ParseToEnum<SortDirectionOption> },
            { typeof(GeoPoint), TypeParsers.ParseToGeoPoint }
        };

        private static readonly IDictionary<Type, Action<NameValueCollection, object, string>> Converters = new Dictionary
            <Type, Action<NameValueCollection, object, string>>()
        {
            { typeof (IEnumerable<string>), TypeParsers.ParseFromStringArray<string> },
            { typeof (IEnumerable<int>), TypeParsers.ParseFromStringArray<int> },
            { typeof (IEnumerable<double>), TypeParsers.ParseFromStringArray<double> },
            { typeof (string), TypeParsers.ParseFromString },
            { typeof (int), TypeParsers.ParseFromString },
            { typeof (double), TypeParsers.ParseFromString },
            { typeof (int?), TypeParsers.ParseFromString },
            { typeof (double?), TypeParsers.ParseFromString },
            { typeof (SortDirectionOption?), TypeParsers.ParseFromString },
            { typeof (GeoPoint), TypeParsers.ParseFromGeoPoint },
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

        public static NameValueCollection Parse(TParameters parameters)
        {
            var nvc = HttpUtility.ParseQueryString("");
            var properties = typeof(TParameters).GetProperties();
            foreach (var prop in properties.OrderBy(o => o.Name))
            {
                if (Converters.ContainsKey(prop.PropertyType))
                {
                    var o = prop.GetValue(parameters);
                    if (o != null)
                    {
                        var defaultValue = GetDefaultValue(prop);
                        if (defaultValue == null || o.ToString() != defaultValue.ToString())
                        {
                            Converters[prop.PropertyType](nvc, o, GetParameterName(prop).Camelize());
                        }
                    }
                }
            }
            return TypeParsers.Sort(nvc);
        }

        public static TParameters Parse(NameValueCollection queryString)
        {
            var p = new TParameters();
            Populate(queryString, p);
            return p;
        }

        public static TParameters Parse(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return Parse(nvc);
        }

        public static void Populate(NameValueCollection nvc, TParameters parameters)
        {
            var properties = typeof(TParameters).GetProperties();
            foreach (var key in nvc.AllKeys)
            {
                var prop =
                    properties.FirstOrDefault(p => GetParameterName(p).Equals(key, StringComparison.InvariantCultureIgnoreCase));

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

        private static string GetParameterName(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.GetCustomAttribute<ParameterAttribute>(true)?.Name ?? propertyInfo.Name;
        }

        private static object GetDefaultValue(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.GetCustomAttribute<DefaultValueAttribute>(true)?.Value;
        }
    }
}
