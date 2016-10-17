using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentAssertions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Nest.Searchify.Tests.ParametersTests
{
    public class QueryStringParserContext
    {
        public enum SomeOption
        {
            OptionOne,
            OptionTwo
        }

        public class CustomParameters : Parameters
        {
            public IEnumerable<string> Options { get; set; }
        }

        public class MyParameters : SearchParameters, IGeoPointParameters
        {
            public IEnumerable<string> Options { get; set; }
            public IEnumerable<int> Numbers { get; set; }
            public IEnumerable<double> Doubles { get; set; }
            [JsonProperty("lat")]
            public double Latitude { get; set; }
            [JsonProperty("lng")]
            public double Longitude { get; set; }

            public GeoPoint Location { get; set; }
            public SomeOption EnumOptions { get; set; }
        }
        
        [Theory]
        [InlineData("", "", 0)]
        [InlineData("page=1", "", 0)]
        [InlineData("page=2", "page=2", 1)]
        [InlineData("size=10", "", 0)]
        [InlineData("size=100", "size=100", 1)]
        [InlineData("page=1&size=100", "size=100", 1)]
        [InlineData("sortdir=Asc&sortby=column", "sortby=column&sortdir=Asc", 2)]
        [InlineData("page=", "", 0)]
        public void ParseQueryStringForParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<Parameters>.Parse(actual);

            var nvc = QueryStringParser<Parameters>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = nvc.ToString();
            qs.Should().Be(expected);
        }

        [Theory]
        [InlineData("q=test", "q=test", 1)]
        [InlineData("query=test", "", 0)]
        [InlineData("q=test&page=2", "page=2&q=test", 2)]
        [InlineData("q=", "", 0)]
        public void ParseQueryStringForSearchParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<SearchParameters>.Parse(actual);

            var nvc = QueryStringParser<SearchParameters>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = nvc.ToString();
            qs.Should().Be(expected);
        }

        [Theory]
        [InlineData("options=alpha&options=beta", "options=alpha&options=beta", 1)]
        [InlineData("options=beta&options=alpha", "options=alpha&options=beta", 1)]
        [InlineData("options=2&options=zulu", "options=2&options=zulu", 1)]
        [InlineData("options=zulu&options=6", "options=6&options=zulu", 1)]
        public void ParseQueryStringForCustomParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<CustomParameters>.Parse(actual);

            var nvc = QueryStringParser<CustomParameters>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = nvc.ToString();
            qs.Should().Be(expected);
        }

        [Fact]
        public void FromQueryString()
        {
            var queryString = HttpUtility.ParseQueryString("q=test&options=o1&options=o2&numbers=1&numbers=5&doubles=99.99&doubles=8&lat=-53.3&lng=3.234&location=1,2&sortdir=Asc&enumOptions=optionone");
            var p = new MyParameters();

            QueryStringParser<MyParameters>.Populate(queryString, p);

            p.Query.Should().Be("test");
            p.Page.Should().Be(1);
            p.Size.Should().Be(10);
            p.Options.Should().NotBeNull();
            p.Options.Count().Should().Be(2);
            p.Numbers.Count().Should().Be(2);
            p.Doubles.Count().Should().Be(2);
            p.Latitude.Should().Be(-53.3);
            p.Longitude.Should().Be(3.234);
            p.Location.Latitude.Should().Be(1);
            p.Location.Longitude.Should().Be(2);
            p.SortDirection.Should().Be(SortDirectionOption.Asc);
            p.EnumOptions.Should().Be(SomeOption.OptionOne);
        }

        [Fact]
        public void ToQueryString()
        {
            var p = new MyParameters();
            p.Query = "test";
            p.Options = new[] {"o1", "o2"};
            p.Numbers = new[] {1, 5};
            p.Doubles = new[] { 2.3, 17.5 };
            p.Latitude = -53.1;
            p.Longitude = -3;
            p.Location = GeoPoint.TryCreate(-53.1, -3);

            var nvc = QueryStringParser<MyParameters>.Parse(p);
            nvc.Count.Should().Be(7);

            foreach (var item in nvc.AllKeys)
            {
                Console.WriteLine($"{item} => {nvc.Get(item)}");
            }
            Console.WriteLine(nvc);
        }
    }


}
