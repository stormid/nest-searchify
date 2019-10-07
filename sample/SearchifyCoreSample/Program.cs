using System;
using System.Linq;
using Bogus;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Nest.Queryify.Extensions;
using Nest.Searchify;
using Nest.Searchify.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SearchifyCoreSample
{
    class Program
    {
        static void ES()
        {
            var connectionSettings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), TypeAndConnectionSettingsAwareJsonNetSerializer.Default);
            connectionSettings.DefaultIndex("my-application");
            connectionSettings.EnableDebugMode(c =>
            {
                //if (c.Uri.PathAndQuery.Contains("_search"))
                //{
                Console.WriteLine(c.DebugInformation);
                //}
            });

            connectionSettings.DefaultTypeName("_doc");
            connectionSettings.ThrowExceptions();

            var client = new ElasticClient(connectionSettings);

            CreateIndex(client);
            SeedIndex(client);
            SeedSportingTeamDocuments(client);

            var parameters = new PersonSearchParameters()
            {
                Country = "uk",
                //Location = new GeoLocationParameter(55.9, -3.1),
                //Radius = 50
            };

            var result = client.Query(new SampleSearchQuery(parameters));

            var result2 = client.Query(new SampleSportingTeamSearchQuery(new SearchParameters()));

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

            Console.WriteLine("\n-----------------");

            Console.WriteLine(JsonConvert.SerializeObject(result2, Formatting.Indented));

        }

        static void Main(string[] args)
        {
            ES();
            Console.WriteLine("Done!");
            Console.ReadKey(true);
        }

        private static void SeedIndex(ElasticClient client)
        {
            var faker = new Faker<PersonDocument>();
            faker
                .RuleFor(r => r.Id, f => Guid.NewGuid().ToString())
                .RuleFor(r => r.Name, f => $"{f.Person.FirstName} {f.Person.LastName}")
                .RuleFor(r => r.Age, f => f.Random.Number(16, 65))
                .RuleFor(r => r.Country, f => FilterField.Create(f.Address.Country(), f.Address.CountryCode().ToLowerInvariant()))
                .RuleFor(r => r.Tags, f => f.Commerce.Categories(5).Select(FilterField.Create))
                ;
            var list = faker.Generate(5).ToList();
            list.Add(new PersonDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Phil Oyston",
                Age = 20,
                Country = FilterField.Create("United Kingdom", "uk"),
                Tags = new[] { FilterField.Create("Baby") },
            });
            list.Add(new PersonDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John Doe",
                Age = 30,
                Country = FilterField.Create("United Kingdom", "uk"),
                Tags = new[] { FilterField.Create("Grocery") },
            });
            list.Add(new PersonDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John Smith",
                Age = 40,
                Country = FilterField.Create("United Kingdom", "uk"),
                Tags = new[] { FilterField.Create("Baby"), FilterField.Create("Grocery") },
            });

            client.Bulk(b => b.IndexMany(list).Refresh(Refresh.True));
        }

        private static void SeedSportingTeamDocuments(ElasticClient client)
        {
            var faker = new Faker<SportingTeamDocument>();
            faker
                .RuleFor(r => r.Id, f => Guid.NewGuid().ToString())
                .RuleFor(r => r.Name, f => $"{f.Person.FirstName} {f.Person.LastName}")
                .RuleFor(r => r.SportType, f => FilterField.Create(f.Person.Company.Name))
                ;

            var list = faker.Generate(3).ToList();

            list.Add(new SportingTeamDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Edinburgh Somethings",
                SportType = FilterField.Create("Football"),
            });

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
                .Mappings(m => m
                    .Map<dynamic>(mm => mm
                        .Properties(p => p.Keyword(k => k.Name("$type")))
                        .AutoMap<PersonDocument>()
                        .AutoMap<SportingTeamDocument>()
                    )
                )
            );
        }
    }
}