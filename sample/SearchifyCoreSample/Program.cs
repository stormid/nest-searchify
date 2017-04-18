using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Bogus;
using Elasticsearch.Net;
using Nest;
using Nest.Searchify;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;
using Newtonsoft.Json;

namespace SearchifyCoreSample
{
    class Program
    {
        static void ES()
        {
            var connectionSettings = new ConnectionSettings(new Uri("http://localhost:9200"));
            connectionSettings.DefaultIndex("my-application");
            connectionSettings.EnableDebugMode(c =>
            {
                if (c.Uri.PathAndQuery.Contains("_search"))
                {
                    Console.WriteLine(c.DebugInformation);
                }
            });

            connectionSettings.InferMappingFor<PersonDocument>(m => m.TypeName("person"));
            connectionSettings.ThrowExceptions();

            var client = new ElasticClient(connectionSettings);

            CreateIndex(client);
            SeedIndex(client);

            var parameters = new PersonSearchParameters()
            {
                Query = "phil",
                Country = "UK"
            };

            var result = client.Search(new SampleSearchQuery(parameters));

            // ParseAggregations(result.Aggregations);

            //Console.WriteLine($"Total: {result.Documents.Count()}");

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

        }

        static void Main(string[] args)
        {
            ES();
            Console.WriteLine("Done!");
            Console.ReadKey(true);
        }

        private static void ParseAggregations(IReadOnlyDictionary<string, IAggregate> aggs)
        {
            var d = new Dictionary<string, IAggregate>();

            foreach (var agg in aggs)
            {
                switch (agg.Value)
                {
                    case BucketAggregate t:
                        var terms = t.Items.OfType<KeyedBucket<object>>().Select(b => new KeyedBucket<SearchifyKey>(b?.Aggregations?.ToDictionary(k => k.Key, v => v.Value))
                        {
                            DocCount = b.DocCount,
                            Key = new SearchifyKey(b.Key.ToString()),
                            KeyAsString = b.KeyAsString
                        }).ToList();

                        var ta = new TermsAggregate<SearchifyKey>
                        {
                            Buckets = new ReadOnlyCollection<KeyedBucket<SearchifyKey>>(terms),
                            SumOtherDocCount = t.SumOtherDocCount,
                            Meta = t.Meta
                        };
                        d.Add(agg.Key, ta);
                        break;
                }
            }
        }

        private static void SeedIndex(ElasticClient client)
        {
            var faker = new Faker<PersonDocument>();
            faker
                .RuleFor(r => r.Id, f => Guid.NewGuid().ToString())
                .RuleFor(r => r.Name, f => $"{f.Person.FirstName} {f.Person.LastName}")
                .RuleFor(r => r.Country, f => FilterField.Create(f.Address.Country(), f.Address.CountryCode()))
                ;
            var list = faker.Generate(5).ToList();
            list.Add(new PersonDocument {Id = Guid.NewGuid().ToString(), Name = "Phil Oyston", Country = FilterField.Create("United Kingdom", "UK")});
            list.Add(new PersonDocument { Id = Guid.NewGuid().ToString(), Name = "John Doe", Country = FilterField.Create("United Kingdom", "Uk") });
            list.Add(new PersonDocument { Id = Guid.NewGuid().ToString(), Name = "John Smith", Country = FilterField.Create("United Kingdom", "uk") });

            client.Bulk(b => b.IndexMany(list).Refresh(Refresh.True));
        }

        private static void CreateIndex(ElasticClient client)
        {
            var response = client.IndexExists(client.ConnectionSettings.DefaultIndex);
            if (response.Exists)
            {
                client.DeleteIndex(client.ConnectionSettings.DefaultIndex);
            }
            client.CreateIndex(client.ConnectionSettings.DefaultIndex, c => c.Mappings(m => m.Map<PersonDocument>(mm => mm.AutoMap())));
        }
    }

    public class PersonDocument
    {
        [Keyword]
        public string Id { get; set; }
        public string Name { get; set; }

        public FilterField Country { get; set; }
    }

    public class PersonSearchParameters : SearchParameters
    {
        public PersonSearchParameters()
        {
            
        }

        public PersonSearchParameters(int size, int page) : base(size, page) { }

        public string Country { get; set; }
    }

    public class SampleSearchQuery : SearchParametersQuery<PersonSearchParameters, PersonDocument, SearchResult<PersonSearchParameters, PersonDocument>>
    {
        public SampleSearchQuery(string queryTerm, int page = 1, int size = 10) : this(new PersonSearchParameters(size, page) {Query = queryTerm})
        {
            
        }

        public SampleSearchQuery(PersonSearchParameters parameters) : base(parameters)
        {
        }
        
        protected override QueryContainer WithQuery(IQueryContainer query, string queryTerm)
        {
            return Query<PersonDocument>.Match(f => f.Field(fld => fld.Name).Query(queryTerm));
        }

        protected override QueryContainer WithFilters(IQueryContainer query, PersonSearchParameters parameters)
        {
            return Query<PersonDocument>.Term(t => t.Field(f => f.Country.Value).Value(parameters.Country));
        }
        
        protected override AggregationContainerDescriptor<PersonDocument> ApplyAggregations(AggregationContainerDescriptor<PersonDocument> descriptor, PersonSearchParameters parameters)
        {
            return 
                descriptor.Terms(nameof(PersonSearchParameters.Country), t => t.Field(f => f.Country.Key));
        }
    }
}