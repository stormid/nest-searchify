using System;
using System.Collections.Generic;
using System.Linq;
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

        public class ParametersWithBool : Parameters
        {
            public bool MandatoryBool { get; set; }
            public bool? OptionalBool { get; set; }
        }

        public class CustomParameters : Parameters
        {
            public IEnumerable<string> Options { get; set; }

            public IEnumerable<long> LongValues { get; set; }

            public long? OptionalLongValue { get; set; }

        }

        public class MyParameters : SearchParameters
        {
            public IEnumerable<string> Options { get; set; }
            public IEnumerable<int> Numbers { get; set; }
            public IEnumerable<double> Doubles { get; set; }
            public SomeOption EnumOptions { get; set; }

            public long LongValue { get; set; }

            public IEnumerable<long> LongValues { get; set; }

            public long? OptionalLongValue { get; set; }
        }

        [Theory]
        [InlineData("", "", 0)]
        [InlineData("mandatoryBool=true", "mandatoryBool=true", 1)]
        [InlineData("mandatoryBool=1", "mandatoryBool=true", 1)]
        [InlineData("mandatoryBool=false", "", 0)]
        [InlineData("mandatoryBool=0", "", 0)]
        [InlineData("mandatoryBool=True", "mandatoryBool=true", 1)]
        [InlineData("mandatoryBool=False", "", 0)]
        [InlineData("mandatoryBool=2", "", 0)]
        [InlineData("optionalBool=1", "optionalBool=true", 1)]
        [InlineData("optionalBool=0", "optionalBool=false", 1)]
        [InlineData("optionalBool=", "", 0)]
        [InlineData("optionalBool=2", "", 0)]
        public void ParseQueryStringForBoolParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<ParametersWithBool>.Parse(actual);

            var nvc = QueryStringParser<ParametersWithBool>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = QueryStringParser<ParametersWithBool>.ToQueryString(parameters);
            qs.Should().Be(expected);
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
        [InlineData("sortdir=&sortby=column", "sortby=column", 1)]
        [InlineData("sortdir=invalid&sortby=column", "sortby=column", 1)]
        [InlineData("sortdir=ASC&sortby=column", "sortby=column&sortdir=Asc", 2)]
        public void ParseQueryStringForParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<Parameters>.Parse(actual);

            var nvc = QueryStringParser<Parameters>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = QueryStringParser<Parameters>.ToQueryString(parameters);
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

            var qs = QueryStringParser<SearchParameters>.ToQueryString(parameters);
            qs.Should().Be(expected);
        }

        [Theory]
        [InlineData("options=alpha&options=beta", "options=alpha&options=beta", 1)]
        [InlineData("options=beta&options=alpha", "options=alpha&options=beta", 1)]
        [InlineData("options=2&options=zulu", "options=2&options=zulu", 1)]
        [InlineData("options=zulu&options=6", "options=6&options=zulu", 1)]
        [InlineData("optionalLongValue=1234567", "optionalLongValue=1234567", 1)]
        [InlineData("longValues=1234567&longValues=89768945", "longValues=1234567&longValues=89768945", 1)]
        public void ParseQueryStringForCustomParameters(string actual, string expected, int paramCount)
        {
            var parameters = QueryStringParser<CustomParameters>.Parse(actual);

            var nvc = QueryStringParser<CustomParameters>.Parse(parameters);
            nvc.Count.Should().Be(paramCount);

            var qs = QueryStringParser<CustomParameters>.ToQueryString(parameters);
            qs.Should().Be(expected);
        }

        [Fact]
        public void FromQueryString()
        {
            var queryString = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery("q=test&options=o1&options=o2&numbers=1&numbers=5&doubles=99.99&doubles=8&sortdir=Asc&enumOptions=optionone");
            var p = new MyParameters();

            QueryStringParser<MyParameters>.Populate(queryString, p);

            p.Query.Should().Be("test");
            p.Page.Should().Be(1);
            p.Size.Should().Be(10);
            p.Options.Should().NotBeNull();
            p.Options.Count().Should().Be(2);
            p.Numbers.Count().Should().Be(2);
            p.Doubles.Count().Should().Be(2);
            p.SortDirection.Should().Be(SortDirectionOption.Asc);
            p.EnumOptions.Should().Be(SomeOption.OptionOne);
        }

        [Fact]
        public void ToQueryString()
        {
            var p = new MyParameters
            {
                Query = "test",
                Options = new[] {"o1", "o2"},
                Numbers = new[] {1, 5},
                Doubles = new[] {2.3, 17.5},
                LongValue = 999999999,
                LongValues = new long[] { 11111111, 22222222, 33333333 }
            };

            var nvc = QueryStringParser<MyParameters>.Parse(p);
            nvc.Count.Should().Be(6);
        }
    }


}
