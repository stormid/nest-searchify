using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
                //if (c.Uri.PathAndQuery.Contains("_search"))
                //{
                //    Console.WriteLine(c.DebugInformation);
                //}
            });

            connectionSettings.InferMappingFor<PersonDocument>(m => m.TypeName("person"));
            connectionSettings.ThrowExceptions();
            
            var client = new ElasticClient(connectionSettings);

            CreateIndex(client);
            SeedIndex(client);

            var parameters = new PersonSearchParameters()
            {
                Tags = new[] { "baby", "grocery" },
                Country = "uk",
                AgeRange = (int)AgeRangeEnum.MiddleAge
            };

            var result = client.Search(new SampleSearchQuery(parameters));

            UseAggregationFilterHelper(result);

            // ParseAggregations(result.Aggregations);

            //Console.WriteLine($"Total: {result.Documents.Count()}");

            //Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

        }

        private static void UseAggregationFilterHelper(ISearchResult<PersonSearchParameters, PersonDocument> result)
        {
            var filters = new[]
            {
                SearchResultFilterHelpers.FilterFor(result, p => p.Tags),
                SearchResultFilterHelpers.FilterFor(result, p => p.Country),
                SearchResultFilterHelpers.FilterFor(result, p => p.AgeRange)
            }.Where(f => f != null).ToList();

            foreach (var filter in filters)
            {
                Console.WriteLine(filter.Name);
                foreach (var item in filter.Items)
                {
                    Console.WriteLine("[{2}] {0} ({1})", item.Term, item.DocCount, item.Selected ? "X" : " ");
                    //Console.WriteLine($"{item.Term} ({item.DocCount})\t\t[{(item.Selected ? "X" : "")}]");
                }
            }
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
                .RuleFor(r => r.Age, f => f.Random.Number(16, 65))
                .RuleFor(r => r.Country, f => FilterField.Create(f.Address.Country(), f.Address.CountryCode().ToLowerInvariant()))
                .RuleFor(r => r.Tags, f =>f.Commerce.Categories(5).Select(FilterField.Create))
                ;
            var list = faker.Generate(100).ToList();
            list.Add(new PersonDocument { Id = Guid.NewGuid().ToString(), Name = "Phil Oyston", Age = 20, Country = FilterField.Create("United Kingdom", "uk"), Tags = new[] { FilterField.Create("Baby") }});
            list.Add(new PersonDocument { Id = Guid.NewGuid().ToString(), Name = "John Doe", Age = 30, Country = FilterField.Create("United Kingdom", "uk"), Tags = new[] { FilterField.Create("Grocery") } });
            list.Add(new PersonDocument { Id = Guid.NewGuid().ToString(), Name = "John Smith", Age = 40, Country = FilterField.Create("United Kingdom", "uk"), Tags = new[] { FilterField.Create("Baby"), FilterField.Create("Grocery") } });

            client.Bulk(b => b.IndexMany(list).Refresh(Refresh.True));
        }

        private static void CreateIndex(ElasticClient client)
        {
            var response = client.IndexExists(client.ConnectionSettings.DefaultIndex);
            if (response.Exists)
            {
                client.DeleteIndex(client.ConnectionSettings.DefaultIndex);
            }
            client.CreateIndex(client.ConnectionSettings.DefaultIndex, c => c
                .Settings(s => s
                    .Analysis(a => a.Analyzers(aa => aa.Language("english", l => l.Language(Language.English)))
                    )
                )
                .Mappings(m => m.Map<PersonDocument>(mm => mm.AutoMap()))
            );
        }
    }
    
    public class PersonDocument
    {
        [Keyword]
        public string Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public FilterField Country { get; set; }

        public IEnumerable<FilterField> Tags { get; set; }
    }

    public enum AgeRangeEnum : int
    {
        Young,
        MiddleAge,
        Older
    }

    public class PersonSearchParameters : SearchParameters
    {
        public const string AgeRangeParameter = "age";

        public PersonSearchParameters()
        {
            
        }

        public PersonSearchParameters(int size, int page) : base(size, page) { }

        public string Country { get; set; }

        [JsonProperty(AgeRangeParameter)]
        public int? AgeRange { get; set; }

        public IEnumerable<string> Tags { get; set; }
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
            return 
                Query<PersonDocument>.Term(t => t.Field(f => f.Country.Value).Value(parameters.Country))
                &&
                Query<PersonDocument>.Terms(t => t.Field(f => f.Tags.First().Value).Terms(parameters.Tags))
                ;
        }
        
        protected override AggregationContainerDescriptor<PersonDocument> ApplyAggregations(AggregationContainerDescriptor<PersonDocument> descriptor, PersonSearchParameters parameters)
        {
            return
                descriptor
                    .Terms(nameof(PersonSearchParameters.Tags), t => t
                        .Meta(m => m.Add("type", "multi_term"))
                        .Field(f => f.Tags.First().Key)

                    )
                    .Terms(nameof(PersonSearchParameters.Country), t => t
                        .Meta(m => m.Add("type", "term"))
                        .Field(f => f.Country.Key)
                    )
                    .Range(PersonSearchParameters.AgeRangeParameter, r => r
                        .Meta(m => m.Add("type", "range"))
                        .Field(f => f.Age)
                        .Ranges(
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.Young))
                                      .From(0)
                                      .To(20),
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.MiddleAge))
                                      .From(21)
                                      .To(40),
                            rng => rng.Key(FilterField.Create(AgeRangeEnum.Older)).From(41)
                        )
                    )
                ;
        }
    }
}