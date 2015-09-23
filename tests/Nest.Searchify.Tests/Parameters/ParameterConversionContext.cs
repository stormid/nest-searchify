using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using FluentAssertions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;
using Nest.Searchify.Queries;
using Xunit;

namespace Nest.Searchify.Tests.Parameters
{

    public class QueryStringParserContext
    {
        public class MyParameters : SearchParameters
        {
            public IEnumerable<string> Options { get; set; }
            public IEnumerable<int> Numbers { get; set; }
            public IEnumerable<double> Doubles { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public GeoPoint Location { get; set; }
        }

        [Fact]
        public void FromQueryString()
        {
            var queryString = HttpUtility.ParseQueryString("page=1&size=10&query=test&options=o1&options=o2&numbers=1&numbers=5&doubles=99.99&doubles=8&latitude=-53.3&longitude=3.234&location=1,2&sortDirection=Asc");
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
            p.SortDirection = SortDirectionOption.Asc;

            var nvc = QueryStringParser<MyParameters>.Parse(p);
            nvc.Count.Should().Be(10);

            foreach (var item in nvc.AllKeys)
            {
                Console.WriteLine($"{item} => {nvc.Get(item)}");
            }
            Console.WriteLine(nvc);
        }
    }


}
