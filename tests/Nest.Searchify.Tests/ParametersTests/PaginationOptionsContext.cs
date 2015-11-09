using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest.Searchify.Extensions;
using Nest.Searchify.SearchResults;
using Xunit;

namespace Nest.Searchify.Tests.ParametersTests
{
    public class PaginationOptionsContext
    {
        public enum SomeOption
        {
            OptionOne,
            OptionTwo
        }

        public class MyParameters : Searchify.Queries.Parameters
        {
            public IEnumerable<string> ContentType { get; set; }
            public IEnumerable<SomeOption> Type { get; set; }
        }


        [Theory]
        [InlineData(1,100,1)]
        [InlineData(2,100,2)]
        [InlineData(10,100,10)]
        public void ShouldHaveCorrectPage(int currentPage, int totalResults,int expected)
        {
            var parameters = Create(currentPage);

            var pagingOptions = Create(parameters, totalResults);
            pagingOptions.Page.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 100, 1)]
        [InlineData(2, 100, 11)]
        [InlineData(10, 100, 91)]
        public void ShouldHaveCorrectFrom(int currentPage, int totalResults, int expected)
        {
            var parameters = Create(currentPage);

            var pagingOptions = Create(parameters, totalResults);
            pagingOptions.From.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, 100, 10)]
        [InlineData(2, 100, 20)]
        [InlineData(10, 100, 100)]
        public void ShouldHaveCorrectTo(int currentPage, int totalResults, int expected)
        {
            var parameters = Create(currentPage);

            var pagingOptions = Create(parameters, totalResults);
            pagingOptions.To.Should().Be(expected);
        }

        [Theory]
        [InlineData(1, "http://localtest.me/search", "http://localtest.me/search?page=2", 100)]
        [InlineData(5, "http://localtest.me/search", "http://localtest.me/search?page=6", 100)]
        [InlineData(10, "http://localtest.me/search", "http://localtest.me/search", 100)]
        [InlineData(100, "http://localtest.me/search", "http://localtest.me/search", 100)]
        [InlineData(-1, "http://localtest.me/search", "http://localtest.me/search?page=2", 100)]
        public void ShouldGenerateNextPageUri(int currentPage, string baseUriString, string expectedUriString, int total)
        {
            var parameters = Create(new MyParameters() { Page = currentPage }, total);
            var baseUri = new Uri(baseUriString);
            var expectedNextPageUri = new Uri(expectedUriString);
            var nextPageUri = parameters.GetNextPageUri(baseUri);

            nextPageUri.Should().Be(expectedNextPageUri);
        }

        [Theory]
        [InlineData(1, "http://localtest.me/search", "http://localtest.me/search", 100)]
        [InlineData(5, "http://localtest.me/search", "http://localtest.me/search?page=4", 100)]
        [InlineData(10, "http://localtest.me/search", "http://localtest.me/search?page=9", 100)]
        [InlineData(100, "http://localtest.me/search", "http://localtest.me/search?page=99", 100)]
        [InlineData(-1, "http://localtest.me/search", "http://localtest.me/search", 100)]
        public void ShouldGeneratePreviousPageUri(int currentPage, string baseUriString, string expectedUriString, int total)
        {
            var parameters = Create(new MyParameters() { Page = currentPage }, total);
            var baseUri = new Uri(baseUriString);
            var expectedPageUri = new Uri(expectedUriString);
            var nextPageUri = parameters.GetPreviousPageUri(baseUri);

            nextPageUri.Should().Be(expectedPageUri);
        }

        private PaginationOptions<MyParameters> Create(MyParameters parameters, int max)
        {
            return new PaginationOptions<MyParameters>(parameters, max);
        }

        private MyParameters Create(int page)
        {
             return  new MyParameters
            {
                ContentType = new[] { "test", "test2" },
                Type = new[] { SomeOption.OptionOne },
                Page = page
            };
        }

    }
}