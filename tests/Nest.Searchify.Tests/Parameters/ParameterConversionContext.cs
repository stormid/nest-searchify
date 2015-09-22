using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Elasticsearch.Net;
using FluentAssertions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.Parameters
{
    public static class QuerystringParser<TParameters> where TParameters : IParameters
    {
        private static IDictionary<Type, Action<TParameters, PropertyInfo, NameValueCollection, string>> _resolvers = new Dictionary<Type, Action<TParameters, PropertyInfo, NameValueCollection, string>>()
        {
            { typeof(IEnumerable<string>), ParseStringArray },
            { typeof(string), ParseString },
            { typeof(IEnumerable<int>), ParseIntegerArray },
            { typeof(IEnumerable<double>), ParseDoubleArray },
            { typeof(double), ParseDouble },
            { typeof(int), ParseInteger },
            { typeof(double?), ParseDouble },
            { typeof(int?), ParseInteger }
        };

        public static void ParseStringArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(IEnumerable<string>)))
            {
                var values = nvc.GetValues(key);
                if (values != null && values.Any())
                {
                    prop.SetValue(parameters, values);
                }
            }
        }

        public static void ParseDoubleArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(IEnumerable<double>)))
            {
                var values = nvc.GetValues(key)?.Select(double.Parse);
                if (values != null && values.Any())
                {
                    prop.SetValue(parameters, values);
                }
            }
        }

        public static void ParseString(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            prop.SetValue(parameters, nvc[key]);
        }

        public static void ParseInteger(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            var value = 0;
            if (int.TryParse(nvc[key], out value))
            {
                prop.SetValue(parameters, value);
            }
        }

        public static void ParseDouble(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            double value = 0;
            if (double.TryParse(nvc[key], out value))
            {
                prop.SetValue(parameters, value);
            }
        }

        public static void ParseIntegerArray(TParameters parameters, PropertyInfo prop, NameValueCollection nvc, string key)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(IEnumerable<int>)))
            {
                var values = nvc.GetValues(key)?.Select(int.Parse);
                if (values != null && values.Any())
                {
                    prop.SetValue(parameters, values);
                }
            }
        }

        public static void Populate(NameValueCollection nvc, TParameters parameters)
        {
            var properties = typeof (TParameters).GetProperties();
            foreach (var key in nvc.AllKeys)
            {
                var prop =
                    properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (prop != null)
                {
                    if (_resolvers.ContainsKey(prop.PropertyType))
                    {
                        _resolvers[prop.PropertyType](parameters, prop, nvc, key);
                    }
                }
            }
        }
    }

    public class ParameterConversionContext
    {
        public class MyParameters : SearchParameters
        {
            public IEnumerable<string> Options { get; set; }
            public IEnumerable<int> Numbers { get; set; }
            public IEnumerable<double> Doubles { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        [Fact]
        public void Should()
        {
            var queryString = HttpUtility.ParseQueryString("page=1&size=10&query=test&options=o1&options=o2&numbers=1&numbers=5&doubles=99.99&doubles=8&latitude=-53.3&longitude=3.234");
            var p = new MyParameters();

            QuerystringParser<MyParameters>.Populate(queryString, p);

            p.Query.Should().Be("test");
            p.Page.Should().Be(1);
            p.Size.Should().Be(10);
            p.Options.Should().NotBeNull();
            p.Options.Count().Should().Be(2);
            p.Numbers.Count().Should().Be(2);
            p.Doubles.Count().Should().Be(2);
            p.Latitude.Should().Be(-53.3);
            p.Longitude.Should().Be(3.234);
        }
    }
}
