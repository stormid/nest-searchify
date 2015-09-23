using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Xunit;

namespace Nest.Searchify.Tests.Parameters
{

    public enum EnumTypeOptions
    {
        OptionOne,
        OptionTwo
    }


    public class ParametersContext
    {
        public class MyParameters : Queries.Parameters
        {
            public IEnumerable<string> ContentType { get; set; }
            public IEnumerable<EnumTypeOptions> Type { get; set; }
        }

        private readonly MyParameters _parameters;

        public ParametersContext()
        {
            _parameters = new MyParameters();
            _parameters.ContentType = new[] {  "test", "test2" };
            _parameters.Type = new[] {EnumTypeOptions.OptionOne, EnumTypeOptions.OptionTwo, };
        }

        [Fact]
        public void Should()
        {
            QueryStringParser<MyParameters>.AddConverter<IEnumerable<EnumTypeOptions>>(QueryStringParser<MyParameters>.TypeParsers.ParseFromStringArray<EnumTypeOptions>);
            var nvc = QueryStringParser<MyParameters>.Parse(_parameters);
            nvc.Count.Should().Be(2);

            var qs = nvc.ToString();

            qs.Should().Be("contentType=test&contentType=test2&type=OptionOne&type=OptionTwo");
        }
    }
}
