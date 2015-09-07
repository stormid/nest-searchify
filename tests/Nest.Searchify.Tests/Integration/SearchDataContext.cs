using System;
using System.Diagnostics;
using Nest.Queryify;
using Nest.Queryify.Abstractions;

namespace Nest.Searchify.Tests.Integration
{
    public class SearchDataContext : IDisposable
    {
        public string IndexName { get; }

        private readonly IElasticClient _client;
        public IElasticsearchRepository Repository { get; }

        public SearchDataContext()
        {
            IndexName = $"nest-searchify-{DateTime.UtcNow.TimeOfDay.TotalSeconds.ToString("F0")}";
            _client = new ElasticClient(new ConnectionSettings(defaultIndex: IndexName));

            _client.CreateIndex(i => i.Index(IndexName).AddMapping<Person>(m => m.MapFromAttributes()));

            Repository = new ElasticsearchRepository(_client);

            Debug.WriteLine("Creating search data");
            var data = Person.LoadFromResource();
            Repository.Bulk(data, refreshOnSave: true);
        }

        public void Dispose()
        {
            Debug.WriteLine("Clearing search data");
            _client.DeleteIndex(IndexName);
        }
    }
}