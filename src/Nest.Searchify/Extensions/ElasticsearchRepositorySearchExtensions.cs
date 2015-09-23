using Nest.Queryify.Abstractions;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Extensions
{
    public static class ElasticsearchRepositorySearchExtensions
    {
        public static TSearchResult Search<TSearchParameters, TDocument, TSearchResult>(this IElasticsearchRepository repository,
            IElasticClientQueryObject<TSearchResult> query)
            where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
            where TSearchParameters : class, IPagingParameters, ISortingParameters
            where TDocument : class
        {
            return repository.Query(query);
        }

        public static ISearchResult<TSearchParameters, TDocument> Search<TSearchParameters, TDocument>(this IElasticsearchRepository repository,
            IElasticClientQueryObject<SearchResult<TSearchParameters, TDocument>> query)
            where TSearchParameters : class, IPagingParameters, ISortingParameters
            where TDocument : class
        {
            return repository.Query(query);
        }
    }
}