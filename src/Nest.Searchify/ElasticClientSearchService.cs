using Nest.Queryify.Abstractions.Queries;
using Nest.Queryify.Extensions;
using Nest.Searchify.Abstractions;

namespace Nest.Searchify
{
    public class ElasticClientSearchService : ISearchService
    {
        private readonly IElasticClient _client;

        public ElasticClientSearchService(IElasticClient client)
        {
            _client = client;
        }

        public TSearchResult Search<TSearchParameters, TDocument, TSearchResult>(IElasticClientQueryObject<TSearchResult> query) where TSearchParameters : class, ICommonParameters where TDocument : class where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
        {
            return _client.Query(query);
        }

        public ISearchResult<TSearchParameters, TDocument> Search<TSearchParameters, TDocument>(IElasticClientQueryObject<ISearchResult<TSearchParameters, TDocument>> query) where TSearchParameters : class, ICommonParameters where TDocument : class
        {
            return _client.Query(query);
        }
    }
}