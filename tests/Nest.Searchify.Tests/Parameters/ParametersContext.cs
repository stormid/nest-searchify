using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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
            var nvc = _parameters.ToQueryString();
            nvc.Count.Should().Be(4);

            var qs = nvc.ToString();

            qs.Should().Be("contentType=test&contentType=test2&page=1&size=10&type=OptionOne&type=OptionTwo");
        }
    }
}
