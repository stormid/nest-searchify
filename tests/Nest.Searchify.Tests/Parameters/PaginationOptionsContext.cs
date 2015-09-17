using System;
using System.Collections.Generic;
using FluentAssertions;
using Nest.Searchify.SearchResults;
using Xunit;

namespace Nest.Searchify.Tests.Parameters
{
    public class PaginationOptionsContext
    {
        public class MyParameters : Queries.Parameters
        {
            public IEnumerable<string> ContentType { get; set; }
            public IEnumerable<EnumTypeOptions> Type { get; set; }
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

        private PaginationOptions<MyParameters> Create(MyParameters parameters, int max)
        {
            return new PaginationOptions<MyParameters>(parameters, max);
        }

        private MyParameters Create(int page)
        {
             return  new MyParameters
            {
                ContentType = new[] { "test", "test2" },
                Type = new[] { EnumTypeOptions.OptionOne },
                Page = page
            };
        }

    }
}