using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        [Fact]
        public void WhenCreatingAFilterFieldWithDefaultConstructor()
        {
            var filter = new FilterField()
            {
                Text = "text",
                Value = "value"
            };

            filter.Key.Should().Be("value||text");
        }

        [Fact]
        public void WhenSerialisingFilterFieldWithDefaultDelimiter()
        {
            var filter = new FilterField()
            {
                Text = "text",
                Value = "value"
            };

            var jObject = JObject.FromObject(filter);

            jObject.SelectToken("Delimiter").Should().BeNull();
        }

        [Fact]
        public void WhenSerialisingFilterFieldWithCustomDelimiter()
        {
            var filter = new FilterField("!!")
            {
                Text = "text",
                Value = "value"
            };

            var jObject = JObject.FromObject(filter);

            jObject.SelectToken("Delimiter").Should().NotBeNull();
            jObject.SelectToken("Delimiter").Value<string>().Should().Be("!!");
        }

        [Fact]
        public void WhenSerialisingFilterFieldWithCustomDelimiterViaPropertyModification()
        {
            var filter = new FilterField()
            {
                Text = "text",
                Value = "value"
            };
            filter.Delimiter = "!!";

            var jObject = JObject.FromObject(filter);

            jObject.SelectToken("Delimiter").Should().NotBeNull();
            jObject.SelectToken("Delimiter").Value<string>().Should().Be("!!");
        }

        [Theory]
        [InlineData("value||text", "text", "value", FilterField.DefaultDelimiter)]
        [InlineData("value##text", "text", "value", "##")]
        [InlineData("value!!text", "text", "value", "!!")]
        public void WhenParsingAFilterField(string key, string text, string value, string delimiter)
        {
            var filter = FilterField.Parse(key, delimiter);

            filter.Value.Should().Be(value);
            filter.Text.Should().Be(text);
            filter.Key.Should().Be($"{value}{delimiter}{text}");
        }

        [Theory]
        [InlineData(null, FilterField.DefaultDelimiter)]
        [InlineData("", FilterField.DefaultDelimiter)]
        public void WhenParsingFilterFieldWithInvalidKey(string key, string delimiter)
        {
            Action exception = () => FilterField.Parse(key, delimiter);

            exception.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("value||text", null)]
        [InlineData("value||text", "")]
        public void WhenParsingFilterFieldWithInvalidDelimiter(string key, string delimiter)
        {
            Action exception = () => FilterField.Parse(key, delimiter);

            exception.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("text", FilterField.DefaultDelimiter)]
        [InlineData("value||text||something-else", FilterField.DefaultDelimiter)]
        public void WhenParsingFilterFieldWithBadlyFormattedKey(string key, string delimiter)
        {
            Action exception = () => FilterField.Parse(key, delimiter);

            exception.Should().Throw<ArgumentOutOfRangeException>();
        }

        public class WhenComparingFilterFields
        {
            [Theory]
            [InlineData("hello", "hello", true)]
            [InlineData("hello", "world", false)]
            public void ShouldMatchAgainstFilterFieldValueOnly(string text1, string text2, bool result)
            {
                FilterField f1 = text1;
                FilterField f2 = text2;

                f1.Equals(f2).Should().Be(result);
                f2.Equals(f1).Should().Be(result);
            }

            [Fact]
            public void ShouldIgnoreTextValue()
            {
                var f1 = FilterField.Create("Hello world", "world");
                var f2 = FilterField.Create("Goodbye world", "world");

                f1.Equals(f2).Should().Be(true);
                f2.Equals(f1).Should().Be(true);
            }

            [Fact]
            public void ShouldIgnoreDelimiterDifferences()
            {
                var f1 = FilterField.CreateWithCustomDelimiter("Hello world", "||");
                var f2 = FilterField.CreateWithCustomDelimiter("Hello world", "!!");

                f1.Equals(f2).Should().Be(true);
                f2.Equals(f1).Should().Be(true);
            }
        }

    }
}
