using System;
using System.Diagnostics;
using System.Text;
using Elasticsearch.Net.Connection;
using Nest.Queryify;
using Nest.Queryify.Abstractions;

namespace Nest.Searchify.Tests.Integration
{
    public abstract class InMemoryElasticsearchContext<TResponse> : InMemoryElasticsearchContext where TResponse : class
    {
        protected TResponse Response { get; set; }
        protected int ResponseStatusCode { get; set; } = 200;
        
        protected abstract Tuple<string, int> ExpectedResponse(INestSerializer serializer);

        protected override IConnection CreateConnection(IConnectionSettingsValues settings)
        {
            var response = ExpectedResponse(Serializer);
            return new InMemoryConnection(settings, response.Item1, response.Item2);
        }
    }

    public abstract class InMemoryElasticsearchContext
    {
        protected INestSerializer Serializer { get; private set; }
        protected IElasticsearchRepository Repository { get; private set; }
        protected IElasticClient Client { get; private set; }
        protected IConnection Connection { get; private set; }
        protected IConnectionSettingsValues Settings { get; private set; }
        protected string DefaultIndex { get; }

        protected InMemoryElasticsearchContext()
        {
            DefaultIndex = SetDefaultIndexCore();

            ContextCore();
            Because();
        }

        protected abstract void Because();

        private string SetDefaultIndexCore()
        {
            return SetDefaultIndex();
        }

        protected virtual string SetDefaultIndex()
        {
            var processId = Process.GetCurrentProcess().Id;
            var contextName = GetType().Name.ToLowerInvariant();
            return $"nest-searchify-{contextName}-{processId}";
        }

        protected virtual IConnection CreateConnection(IConnectionSettingsValues settings)
        {
            return new InMemoryConnection(settings);
        }

        private void ContextCore()
        {
            Context();
            Settings = new ConnectionSettings(defaultIndex: DefaultIndex);
            Serializer = new NestSerializer(Settings);

            Connection = CreateConnection(Settings);

            Client = new ElasticClient(Settings, Connection);
            Repository = new ElasticsearchRepository(Client);
        }

        protected virtual void Context()
        {

        }
    }
}