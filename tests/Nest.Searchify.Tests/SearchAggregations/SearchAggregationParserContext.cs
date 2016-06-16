using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchAggregations;
using Nest.Searchify.SearchResults;
using Nest.Searchify.Tests.Integration;
using Newtonsoft.Json;
using Xunit;

namespace Nest.Searchify.Tests.SearchAggregations
{
    public class SearchAggregationParserContext
    {
        [Fact]
        public void ShouldParseTermAggregation()
        {
            var input = @"
{
    'documents': [],
    'aggregations': {
        'person' : {
             'doc_count_error_upper_bound': 0,
             'sum_other_doc_count': 0,
             'buckets': [
                {
                   'key': '1||First',
                   'doc_count': 1
                },
                {
                   'key': '2||Second',
                   'doc_count': 1
                }
             ]
        }
    }
}
";
            var result = GetResultFromJson(input);

            var bucket = result.Aggregations["person"] as TermBucket;

            bucket.Type.Should().Be("Term");

            bucket.Items.First().Text.Should().Be("First");
            bucket.Items.First().Value.Should().Be("1");
            bucket.Items.First().Key.Should().Be("1||First");

            bucket.Items.Last().Text.Should().Be("Second");
            bucket.Items.Last().Value.Should().Be("2");
            bucket.Items.Last().Key.Should().Be("2||Second");


        }

        [Fact]
        public void ShouldParseNestedAggregation()
        {
            var input = @"
{
    'documents': [],
    'aggregations': {
      'nested_person_types': {
         'doc_count': 6,
            'person_type': {
                'doc_count_error_upper_bound': 0,
                'sum_other_doc_count': 0,
                'buckets': [
                    {
                      'key': '1||FirstTop',
                      'doc_count': 4,
                      'person_types_subcategories': {
                         'doc_count_error_upper_bound': 0,
                         'sum_other_doc_count': 0,
                         'buckets': [
                            {
                               'key': '11||FirstSub1',
                               'doc_count': 3
                            },
                            {
                               'key': '12||FirstSub2',
                               'doc_count': 2
                            },
                            {
                               'key': '13||FirstSub3',
                               'doc_count': 1
                            }
                         ]
                      }
                   },
                   {
                      'key': '2||SecondTop',
                      'doc_count': 2,
                      'person_types_subcategories': {
                         'doc_count_error_upper_bound': 0,
                         'sum_other_doc_count': 0,
                         'buckets': [
                            {
                               'key': '21||SecondSub1',
                               'doc_count': 2
                            }
                         ]
                      }
                   }
                ]
            }
        },    
    }
}
";
            var result = GetResultFromJson(input);

            var bucket = result.Aggregations["nested_person_types"] as SingleBucket;

            var firstLevel = bucket.Aggregations["person_type"] as TermBucket;

            firstLevel.Items.First().Text.Should().Be("FirstTop");
            firstLevel.Items.First().Value.Should().Be("1");
            firstLevel.Items.First().Key.Should().Be("1||FirstTop");

            var secondLevel = firstLevel.Items.First().Aggregations["person_types_subcategories"] as TermBucket;
            secondLevel.Items.First().Text.Should().Be("FirstSub1");
            secondLevel.Items.First().Value.Should().Be("11");
            secondLevel.Items.First().Key.Should().Be("11||FirstSub1");

            secondLevel.Items.Last().Text.Should().Be("FirstSub3");
            secondLevel.Items.Last().Value.Should().Be("13");
            secondLevel.Items.Last().Key.Should().Be("13||FirstSub3");

            firstLevel.Items.Last().Text.Should().Be("SecondTop");
            firstLevel.Items.Last().Value.Should().Be("2");
            firstLevel.Items.Last().Key.Should().Be("2||SecondTop");


        }


        #region Helpers
        private AggregationSearchResults GetResultFromJson(string input)
        {
            var client = new ElasticClient();
            var response = client.Serializer.Deserialize<SearchResponse<Person>>(GenerateStreamFromString(input));

            var result = new AggregationSearchResults(null, response);
            return result;
        }


        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public class AggregationSearchResults : SearchResult<SearchParameters, Person, Person>
        {
            public AggregationSearchResults(SearchParameters parameters) : base(parameters)
            {
            }

            public AggregationSearchResults(SearchParameters parameters, ISearchResponse<Person> response) : base(parameters, response)
            {
            }

            protected override IEnumerable<Person> TransformResult(IEnumerable<Person> entities)
            {
                // We're only worried about the aggregations
                return Enumerable.Empty<Person>();
            }
        }

#endregion
    }

    
}