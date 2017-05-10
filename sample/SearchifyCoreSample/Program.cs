using System;
using System.Linq;
using Bogus;
using Elasticsearch.Net;
using Nest;
using Nest.Queryify.Extensions;
using Nest.Searchify;
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
                Console.WriteLine(c.DebugInformation);
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
                //AgeRange = (int)AgeRangeEnum.MiddleAge
            };

            var result = client.Query(new SampleSearchQuery(parameters));

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

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
}