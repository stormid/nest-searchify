namespace Nest.Searchify.Queries
{
#if NETSTANDARD

    using System.Text.Encodings.Web;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Primitives;
    using Abstractions;
    using Extensions;
    using Newtonsoft.Json;

    public static class QueryStringParser<TParameters> where TParameters : class, new()
    {
        public static class TypeParsers
        {
            public static Dictionary<string, StringValues> Sort(Dictionary<string, StringValues> nvc)
            {
                var sortedNvc = QueryHelpers.ParseQuery("");
                foreach (var key in nvc.Keys.OrderBy(s => s))
                {
                    if (nvc.ContainsKey(key))
                    {
                        var values = nvc[key].OrderBy(o => o).ToArray();
                        sortedNvc.Add(key, new StringValues(values));
                    }
                }
                return sortedNvc;
            }

            public static void ParseFromStringArray<T>(Dictionary<string, StringValues> nvc, object values, string propertyName)
            {
                var stringValues = (values as IEnumerable<T>).ToList();
                if (stringValues != null)
                {
                    nvc.Add(propertyName, new StringValues(stringValues.Select(x => x.ToString()).ToArray()));
                }
            }

            public static void ParseFromString(Dictionary<string, StringValues> nvc, object value, string propertyName)
            {
                if (value != null) nvc.Add(propertyName, value.ToString());
            }

            public static void ParseFromGeoLocation(Dictionary<string, StringValues> nvc, object value, string propertyName)
            {
                var point = value as GeoLocation;
                if (point != null) nvc.Add(propertyName, point.ToString());
            }

            public static void ParseFromGeoLocationParameter(Dictionary<string, StringValues> nvc, object value, string propertyName)
            {
                var point = value as GeoLocationParameter;
                if (point != null) nvc.Add(propertyName, point.ToString());
            }

            public static IEnumerable<string> ParseToStringArray(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                if (nvc.ContainsKey(key))
                {
                    var values = nvc[key];
                    if (values.Any())
                    {
                        return values;
                    }
                }
                throw new InvalidCastException($"Unable to parse [{key}] as array of string");
            }

            public static IEnumerable<double> ParseToDoubleArray(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                if (nvc.ContainsKey(key))
                {
                    var values = nvc[key].Select(Double.Parse).ToList();
                    if (values != null && values.Any())
                    {
                        return values;
                    }
                }
                return null;
            }

            public static IEnumerable<int> ParseToIntegerArray(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                if (nvc.ContainsKey(key))
                {
                    var values = nvc[key].Select(Int32.Parse).ToList();
                    if (values != null && values.Any())
                    {
                        return values;
                    }
                }
                return null;

            }

            public static string ParseToString(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                var value = nvc[key];
                return string.IsNullOrWhiteSpace(value.ToString()) ? null : value.ToString();
            }

            public static GeoLocation ParseToGeoLocation(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                var value = nvc[key];
                return value.ToString();
            }

            public static GeoLocationParameter ParseToGeoLocationParameter(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                var value = nvc[key];
                return value.ToString();
            }

            public static object ParseToInteger(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                var value = ParseToString(parameters, prop, nvc, key);
                if (value == null) return null;

                return int.Parse(value);
            }

            public static object ParseToDouble(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key)
            {
                var value = ParseToString(parameters, prop, nvc, key);
                if (value == null) return null;

                return double.Parse(value);
            }

            public static object ParseToEnum<TEnum>(TParameters parameters, PropertyInfo prop, Dictionary<string, StringValues> nvc, string key) where TEnum : struct
            {
                if (nvc.Keys.Contains(key) && nvc.ContainsKey(key))
                {
                    return (TEnum)Enum.Parse(typeof(TEnum), nvc[key], true);
                }
                return null;
            }
        }

        private static readonly IDictionary<Type, Func<TParameters, PropertyInfo, Dictionary<string, StringValues>, string, object>> Resolvers =
new Dictionary<Type, Func<TParameters, PropertyInfo, Dictionary<string, StringValues>, string, object>>()
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
            { typeof(GeoLocation), TypeParsers.ParseToGeoLocation },
            { typeof(GeoLocationParameter), TypeParsers.ParseToGeoLocationParameter }
        };

        private static readonly IDictionary<Type, Action<Dictionary<string, StringValues>, object, string>> Converters = new Dictionary
            <Type, Action<Dictionary<string, StringValues>, object, string>>()
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
            { typeof (GeoLocation), TypeParsers.ParseFromGeoLocation },
            { typeof (GeoLocationParameter), TypeParsers.ParseFromGeoLocationParameter },
        };

        /// <summary>
        /// Resolvers are used to resolve from querystring value (string) to actual parameter property type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resolver"></param>
        public static void AddResolver<T>(Func<TParameters, PropertyInfo, Dictionary<string, StringValues>, string, object> resolver)
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

        /// <summary>
        /// Converters are used to augment a name value collection with additional parameter properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        public static void AddConverter<T>(Action<Dictionary<string, StringValues>, object, string> converter)
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

        public static Dictionary<string, StringValues> Parse(TParameters parameters)
        {
            var nvc = QueryHelpers.ParseQuery("");
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

        public static TParameters Parse(Dictionary<string, StringValues> queryString)
        {
            var p = new TParameters();
            Populate(queryString, p);
            return p;
        }

        public static TParameters Parse(string queryString)
        {
            var nvc = QueryHelpers.ParseQuery(queryString);
            return Parse(nvc);
        }

        public static void Populate(Dictionary<string, StringValues> nvc, TParameters parameters)
        {
            var properties = typeof(TParameters).GetProperties();
            foreach (var key in nvc.Keys)
            {
                var prop =
                    properties.FirstOrDefault(p => GetParameterName(p).Equals(key, StringComparison.CurrentCultureIgnoreCase));

                if (prop != null)
                {
                    if (Resolvers.ContainsKey(prop.PropertyType))
                    {
                        try
                        {
                            var value = Resolvers[prop.PropertyType](parameters, prop, nvc, key);
                            if (value != null)
                            {
                                prop.SetValue(parameters, value);
                            }
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
            return propertyInfo.GetCustomAttribute<JsonPropertyAttribute>(true)?.PropertyName ?? propertyInfo.Name;
        }

        private static object GetDefaultValue(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.GetCustomAttribute<DefaultValueAttribute>(true)?.Value;
        }
        public static TParameters Copy(TParameters parameters)
        {
            var p = new TParameters();
            Populate(Parse(parameters), p);
            return p;
        }

        public static string ToQueryString(TParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
            var stringBuilder = new System.Text.StringBuilder();
            var flag = false;
            foreach (var keyValuePair in Parse(parameters))
            {
                foreach (var val in keyValuePair.Value)
                {
                    if (flag)
                    {
                        stringBuilder.Append('&');
                    }
                    stringBuilder.Append(UrlEncoder.Default.Encode(keyValuePair.Key));
                    stringBuilder.Append('=');
                    stringBuilder.Append(UrlEncoder.Default.Encode(val));
                    flag = true;
                }
            }
            return stringBuilder.ToString();
        }
    }
}
#endif

#if !NETSTANDARD
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using Abstractions;
    using Extensions;
    using Newtonsoft.Json;

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
                        foreach (var value in values.OrderBy(v => v))
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
                        var strValue = value.ToString();
                        if (!string.IsNullOrWhiteSpace(strValue))
                        {
                            nvc.Add(propertyName, value.ToString());
                        }
                    }
                }
            }

            public static void ParseFromString(NameValueCollection nvc, object value, string propertyName)
            {
                if (value != null) nvc.Add(propertyName, value.ToString());
            }

            public static void ParseFromGeoLocation(NameValueCollection nvc, object value, string propertyName)
            {
                var point = value as GeoLocation;
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
                var value = nvc[key];
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            public static GeoLocation ParseToGeoLocation(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var value = nvc.Get(key);
                var points = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToArray();
                return new GeoLocation(points[0], points[1]);
            }

            public static object ParseToInteger(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var value = ParseToString(parameters, prop, nvc, key);
                if (value == null) return null;

                return int.Parse(value);
            }

            public static object ParseToDouble(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
            {
                var value = ParseToString(parameters, prop, nvc, key);
                if (value == null) return null;

                return double.Parse(value);
            }

            public static object ParseToEnum<TEnum>(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key) where TEnum : struct
            {
                if (nvc.AllKeys.Contains(key) && nvc[key] != null)
                {
                    return (TEnum) Enum.Parse(typeof(TEnum), nvc[key], true);
                }
                return null;
            }

#endregion
        }

        private static readonly IDictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>> Resolvers =
            new Dictionary<Type, Func<TParameters, PropertyInfo, NameValueCollection, string, object>>()
            {
                {typeof(IEnumerable<string>), TypeParsers.ParseToStringArray},
                {typeof(string), TypeParsers.ParseToString},
                {typeof(IEnumerable<int>), TypeParsers.ParseToIntegerArray},
                {typeof(IEnumerable<double>), TypeParsers.ParseToDoubleArray},
                {typeof(double), TypeParsers.ParseToDouble},
                {typeof(int), TypeParsers.ParseToInteger},
                {typeof(double?), TypeParsers.ParseToDouble},
                {typeof(int?), TypeParsers.ParseToInteger},
                {typeof(SortDirectionOption?), TypeParsers.ParseToEnum<SortDirectionOption>},
                {typeof(GeoLocation), TypeParsers.ParseToGeoLocation}
            };

        private static readonly IDictionary<Type, Action<NameValueCollection, object, string>> Converters = new Dictionary
            <Type, Action<NameValueCollection, object, string>>()
            {
                {typeof(IEnumerable<string>), TypeParsers.ParseFromStringArray<string>},
                {typeof(IEnumerable<int>), TypeParsers.ParseFromStringArray<int>},
                {typeof(IEnumerable<double>), TypeParsers.ParseFromStringArray<double>},
                {typeof(string), TypeParsers.ParseFromString},
                {typeof(int), TypeParsers.ParseFromString},
                {typeof(double), TypeParsers.ParseFromString},
                {typeof(int?), TypeParsers.ParseFromString},
                {typeof(double?), TypeParsers.ParseFromString},
                {typeof(SortDirectionOption?), TypeParsers.ParseFromString},
                {typeof(GeoLocation), TypeParsers.ParseFromGeoLocation},
            };

        /// <summary>
        /// Resolvers are used to resolve from querystring value (string) to actual parameter property type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resolver"></param>
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

        /// <summary>
        /// Converters are used to augment a name value collection with additional parameter properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
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
                            if (value != null)
                            {
                                prop.SetValue(parameters, value);
                            }
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
            return propertyInfo.GetCustomAttribute<JsonPropertyAttribute>(true)?.PropertyName ?? propertyInfo.Name;
        }

        private static object GetDefaultValue(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            return propertyInfo.GetCustomAttribute<DefaultValueAttribute>(true)?.Value;
        }

        public static TParameters Copy(TParameters parameters)
        {
            var p = new TParameters();
            Populate(Parse(parameters), p);
            return p;
        }

        public static string ToQueryString(TParameters parameters)
        {
            return Parse(parameters).ToString();
        }
    }
}

#endif