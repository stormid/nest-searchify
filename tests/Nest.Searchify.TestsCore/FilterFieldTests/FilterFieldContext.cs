using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Nest.Searchify.Tests.FilterFieldTests
{
    public class FilterFieldContext
    {
        [Theory]
        [InlineData("Text Value", "text-value", "||")]
        [InlineData("UPPERCASE", "uppercase", "||")]
        [InlineData("Text/Value", "text%2Fvalue", "||")]
        [InlineData("Text_Value", "text_value", "||")]
        [InlineData("very long textual value with encoding requirements#", "very-long-textual-value-with-encoding-requirements%23", "||")]
        public void WhenCreatingAFilterFieldFromText(string text, string value, string delimiter)
        {
            var filter = FilterField.Create(text);

            filter.Value.Should().Be(value);
            filter.Text.Should().Be(text);
            filter.Key.Should().Be($"{value}{delimiter}{text}");
        }

        [Theory]
        [InlineData("Text Value", "another-value", "||")]
        [InlineData("UPPERCASE", "lowercase", "||")]
        public void WhenCreatingAFilterFieldFromTextWithValue(string text, string value, string delimiter)
        {
            var filter = FilterField.Create(text, value);

            filter.Value.Should().Be(value);
            filter.Text.Should().Be(text);
            filter.Key.Should().Be($"{value}{delimiter}{text}");
        }

        [Theory]
        [InlineData("Text Value", "another-value", "##")]
        [InlineData("UPPERCASE", "lowercase", "!!")]
        public void WhenCreatingAFilterFieldWithCustomDelimiter(string text, string value, string delimiter)
        {
            var filter = FilterField.CreateWithCustomDelimiter(text, value, delimiter);

            filter.Value.Should().Be(value);
            filter.Text.Should().Be(text);
            filter.Key.Should().Be($"{value}{delimiter}{text}");
        }
    }
}
