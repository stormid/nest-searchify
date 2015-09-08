using System;
using System.Diagnostics;
using Nest.Queryify;
using Nest.Queryify.Abstractions;

namespace Nest.Searchify.Tests.Integration
{
    public class SearchDataContext : IDisposable
    {
        public string IndexName { get; }

        public IElasticClient Client { get; }
        public IElasticsearchRepository Repository { get; }

        public SearchDataContext()
        {
            IndexName = $"nest-searchify-{DateTime.UtcNow.TimeOfDay.TotalSeconds.ToString("F0")}";
            Client = new ElasticClient(new ConnectionSettings(defaultIndex: IndexName));

            Client.CreateIndex(i => i.Index(IndexName).AddMapping<Person>(m => m.MapFromAttributes()));

            Repository = new ElasticsearchRepository(Client);

            Debug.WriteLine("Creating search data");
            var data = Person.LoadFromResource();
            Repository.Bulk(data, refreshOnSave: true);
        }

        public void Dispose()
        {
            Debug.WriteLine("Clearing search data");
            Client.DeleteIndex(IndexName);
        }
    }
}