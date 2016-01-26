using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;
using Nest.Searchify.Tests.Integration;
using Xunit;

namespace Nest.Searchify.Tests.Queries
{
    public class SearchResultQueryContext
    {
        public class TestParameters : Parameters { }

        public class TestSearchParameters : SearchParameters { }
        
        public class WhenUsingParametersQueryForDocument
        {
            private readonly Mock<IElasticClient> _clientMock;
            private readonly ParametersQuery<TestParameters, Person, SearchResult<TestParameters, Person>> _query;
            private SearchResult<TestParameters, Person> _result;
            private readonly Stream _stream;

            public WhenUsingParametersQueryForDocument()
            {
                _stream =
                    "Nest.Searchify.Tests.Data.search_person_match_all.json"
                        .ReadFromEmbeddedResource<WhenUsingParametersQueryForDocument>();

                _clientMock = new Mock<IElasticClient>(MockBehavior.Strict);
                _clientMock.Setup(
                    s => s.Search<Person, Person>(It.IsAny<Func<SearchDescriptor<Person>, SearchDescriptor<Person>>>()))
                    .Returns(DeserialiseSearchResponse);

                var parameters = new TestParameters();
                _query = new ParametersQuery<TestParameters, Person, SearchResult<TestParameters, Person>>(parameters);
            }

            private ISearchResponse<Person> DeserialiseSearchResponse()
            {
                var serializer = new NestSerializer(new ConnectionSettings());
                return serializer.Deserialize<SearchResponse<Person>>(_stream);
            }

            [Fact]
            public void ShouldReturnExpectedSearchResult()
            {
                _result = _query.Execute(_clientMock.Object, "test");
                _result.Pagination.Total.Should().Be(6);
                _result.Documents.Should().HaveCount(6);
            }
        }

        public class WhenUsingParametersQueryForDocumentWithOutputEntity
        {
            public class CustomSearchResult : SearchResult<TestParameters, Person, Document>
            {
                public CustomSearchResult(TestParameters parameters) : base(parameters)
                {
                }

                public CustomSearchResult(TestParameters parameters, ISearchResponse<Person> response) : base(parameters, response)
                {
                }

                protected override IEnumerable<Document> TransformResult(IEnumerable<Person> entities)
                {
                    return entities.Select(p => new Document());
                }
            }

            private readonly Mock<IElasticClient> _clientMock;
            private readonly ParametersQuery<TestParameters, Person, CustomSearchResult, Document> _query;
            private SearchResult<TestParameters, Person, Document> _result;
            private readonly Stream _stream;

            public WhenUsingParametersQueryForDocumentWithOutputEntity()
            {
                _stream =
                    "Nest.Searchify.Tests.Data.search_person_match_all.json"
                        .ReadFromEmbeddedResource<WhenUsingParametersQueryForDocumentWithOutputEntity>();

                _clientMock = new Mock<IElasticClient>(MockBehavior.Strict);
                _clientMock.Setup(
                    s => s.Search<Person, Person>(It.IsAny<Func<SearchDescriptor<Person>, SearchDescriptor<Person>>>()))
                    .Returns(DeserialiseSearchResponse);

                var parameters = new TestParameters();
                _query = new ParametersQuery<TestParameters, Person, CustomSearchResult, Document>(parameters);
            }

            private ISearchResponse<Person> DeserialiseSearchResponse()
            {
                var serializer = new NestSerializer(new ConnectionSettings());
                return serializer.Deserialize<SearchResponse<Person>>(_stream);
            }

            [Fact]
            public void ShouldReturnExpectedSearchResult()
            {
                _result = _query.Execute(_clientMock.Object, "test");
                _result.Pagination.Total.Should().Be(6);
                _result.Documents.Should().HaveCount(6);
                _result.Documents.First().Should().BeOfType<Document>();
            }
        }

        public class WhenUsingSearchParametersFilteredQueryForDocument
        {
            private readonly Mock<IElasticClient> _clientMock;
            private readonly SearchParametersFilteredQuery<TestSearchParameters, Person, SearchResult<TestSearchParameters, Person>> _query;
            private SearchResult<TestSearchParameters, Person> _result;
            private readonly Stream _stream;

            public WhenUsingSearchParametersFilteredQueryForDocument()
            {
                _stream =
                    "Nest.Searchify.Tests.Data.search_person_match_all.json"
                        .ReadFromEmbeddedResource<WhenUsingSearchParametersFilteredQueryForDocument>();

                _clientMock = new Mock<IElasticClient>(MockBehavior.Strict);
                _clientMock.Setup(
                    s => s.Search<Person, Person>(It.IsAny<Func<SearchDescriptor<Person>, SearchDescriptor<Person>>>()))
                    .Returns(DeserialiseSearchResponse);

                var parameters = new TestSearchParameters();
                parameters.SortBy = "field";
                parameters.Query = "keyword";
                _query = new SearchParametersFilteredQuery<TestSearchParameters, Person, SearchResult<TestSearchParameters, Person>>(parameters);
            }

            private ISearchResponse<Person> DeserialiseSearchResponse()
            {
                var serializer = new NestSerializer(new ConnectionSettings());
                return serializer.Deserialize<SearchResponse<Person>>(_stream);
            }

            [Fact]
            public void ShouldReturnExpectedSearchResult()
            {
                _result = _query.Execute(_clientMock.Object, "test");

                _result.Pagination.Total.Should().Be(6);
                _result.Documents.Should().HaveCount(6);
                _result.Parameters.SortBy.Should().Be("field");
                _result.Parameters.Query.Should().Be("keyword");
            }
        }
    }
}
