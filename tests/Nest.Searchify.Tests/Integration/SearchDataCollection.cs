using System.Linq;
using FluentAssertions;
using Nest.Queryify;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class TestSearchQueryContext
    {
        public TestSearchQueryContext()
        {
            
        }

        [Fact]
        public void Should()
        {
            var parameters = new SearchDataCollection.CustomSearchQuery.PersonParameters();
            var q = new TestSearchQuery(parameters);

            var repository = new ElasticsearchRepository("default");
            var searchService = new RepositorySearchService(repository);

            var r = searchService.Search(q);
        }
    }

    [CollectionDefinition(nameof(SearchDataCollection))]
    public class SearchDataCollection : ICollectionFixture<SearchDataContext>
    {
        [Trait("Category", "Integration")]
        [Collection(nameof(SearchDataCollection))]
        public class WhenAccessingTermsAggregation
        {
            private readonly CustomSearchQuery.PersonSearchResults _result;

            public WhenAccessingTermsAggregation(SearchDataContext context)
            {
                var parameters = new CustomSearchQuery.PersonParameters();
                _result = context.Repository.Query(new PersonSearchQueryWithTermsAggregation(parameters));
            }

            [Fact]
            public void ShouldHaveExpectedAggregationCount()
            {
                _result.AggregationHelper.Terms("country").Should().NotBeNull();
                _result.AggregationHelper.Terms("age").Should().NotBeNull();
            }

            [Fact]
            public void ShouldHaveExpectedAggregation()
            {
                var json = JsonConvert.SerializeObject(_result, Formatting.Indented);
            }
        }

        [Trait("Category", "Integration")]
        [Collection(nameof(SearchDataCollection))]
        public class CommonParametersSearchWithDefaultArgs
        {
            private readonly ISearchResult<ICommonParameters, Person> _result;

            public CommonParametersSearchWithDefaultArgs(SearchDataContext context)
            {
                _result = context.Repository.Query(new CommonParametersQuery<ICommonParameters, Person>(new CommonParameters()));
            }

            [Fact]
            public void ShouldQueryAllAvailableDocuments()
            {
                _result.Pagination.Total.Should().Be(100);
            }

            [Fact]
            public void ShouldReturnFirstPageOfDocuments()
            {
                _result.Documents.Count().Should().Be(10);
            }

            [Fact]
            public void ShouldHaveNextPage()
            {
                _result.Pagination.HasNextPage.Should().BeTrue();
            }

            [Fact]
            public void ShouldNotHavePreviousPage()
            {
                _result.Pagination.HasPreviousPage.Should().BeFalse();
            }

            [Fact]
            public void ShouldProvideNextPageParameters()
            {
                _result.Pagination.NextPage().Page.Should().Be(2);
            }

            [Fact]
            public void ShouldProvidePreviousPageParameters()
            {
                _result.Pagination.PreviousPage().Page.Should().Be(1);
            }
        }

        [Trait("Category", "Integration")]
        [Collection(nameof(SearchDataCollection))]
        public class CommonParametersSearchResultWithDefaultArgs
        {
            private readonly SearchResult<ICommonParameters, Person> _result;

            public CommonParametersSearchResultWithDefaultArgs(SearchDataContext context)
            {
                var query = new CommonParametersQuery<ICommonParameters, Person, SearchResult<ICommonParameters, Person>>(new CommonParameters());
                _result = context.Repository.Query(query);
            }

            [Fact]
            public void ShouldQueryAllAvailableDocuments()
            {
                _result.Pagination.Total.Should().Be(100);
            }

            [Fact]
            public void ShouldReturnFirstPageOfDocuments()
            {
                _result.Documents.Count().Should().Be(10);
            }

            [Fact]
            public void ShouldHaveNextPage()
            {
                _result.Pagination.HasNextPage.Should().BeTrue();
            }

            [Fact]
            public void ShouldNotHavePreviousPage()
            {
                _result.Pagination.HasPreviousPage.Should().BeFalse();
            }

            [Fact]
            public void ShouldProvideNextPageParameters()
            {
                _result.Pagination.NextPage().Page.Should().Be(2);
            }

            [Fact]
            public void ShouldProvidePreviousPageParameters()
            {
                _result.Pagination.PreviousPage().Page.Should().Be(1);
            }
        }

        [Trait("Category", "Integration")]
        [Collection(nameof(SearchDataCollection))]
        public class CustomSearchQuery
        {
            public class PersonParameters : CommonParameters
            {
                
            }

            public class PersonSearchQuery : CommonParametersQuery<PersonParameters, Person, PersonSearchResults>
            {
                public PersonSearchQuery(PersonParameters parameters) : base(parameters)
                {
                }
            }

            public class PersonSearchResults : SearchResult<PersonParameters, Person>
            {
                public PersonSearchResults(PersonParameters parameters, ISearchResponse<Person> response) : base(parameters, response)
                {
                }
            }

            private readonly PersonSearchResults _result;

            public CustomSearchQuery(SearchDataContext context)
            {
                var query = new PersonSearchQuery(new PersonParameters());
                _result = context.Repository.Query(query);
            }

            [Fact]
            public void ShouldQueryAllAvailableDocuments()
            {
                _result.Pagination.Total.Should().Be(100);
            }

            [Fact]
            public void ShouldReturnFirstPageOfDocuments()
            {
                _result.Documents.Count().Should().Be(10);
            }

            [Fact]
            public void ShouldHaveNextPage()
            {
                _result.Pagination.HasNextPage.Should().BeTrue();
            }

            [Fact]
            public void ShouldNotHavePreviousPage()
            {
                _result.Pagination.HasPreviousPage.Should().BeFalse();
            }

            [Fact]
            public void ShouldProvideNextPageParameters()
            {
                _result.Pagination.NextPage().Page.Should().Be(2);
            }

            [Fact]
            public void ShouldProvidePreviousPageParameters()
            {
                _result.Pagination.PreviousPage().Page.Should().Be(1);
            }
        }

        [Trait("Category", "Integration")]
        [Collection(nameof(SearchDataCollection))]
        public class WhenSearchingSpecificPageAndSizeWithCommonParametersSearch
        {
            private readonly ISearchResult<ICommonParameters, Person> _result;

            public WhenSearchingSpecificPageAndSizeWithCommonParametersSearch(SearchDataContext context)
            {
                _result = context.Repository.Query(new CommonParametersQuery<ICommonParameters, Person>(new CommonParameters(1, 100)));
            }

            [Fact]
            public void ShouldQueryAllAvailableDocuments()
            {
                _result.Pagination.Total.Should().Be(100);
            }

            [Fact]
            public void ShouldReturnFirstPageOfDocuments()
            {
                _result.Documents.Count().Should().Be(1);
            }

            [Fact]
            public void ShouldHaveNextPage()
            {
                _result.Pagination.HasNextPage.Should().BeFalse();
            }

            [Fact]
            public void ShouldNotHavePreviousPage()
            {
                _result.Pagination.HasPreviousPage.Should().BeTrue();
            }

            [Fact]
            public void ShouldProvideNextPageParameters()
            {
                _result.Pagination.NextPage().Page.Should().Be(100);
            }

            [Fact]
            public void ShouldProvidePreviousPageParameters()
            {
                _result.Pagination.PreviousPage().Page.Should().Be(99);
            }
        }
    }
}
