using Nest.Queryify.Abstractions.Queries;
using Nest.Queryify.Extensions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Extensions
{
    public static class ElasticClientSearchExtensions
    {
        public static TSearchResult Search<TSearchParameters, TDocument, TSearchResult>(this IElasticClient client, 
            IElasticClientQueryObject<TSearchResult> query)
            where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
            where TSearchParameters : class, IParameters
            where TDocument : class
        {
            return client.Query(query);
        }

        public static ISearchResult<TSearchParameters, TDocument> Search<TSearchParameters, TDocument>(this IElasticClient client, 
            IElasticClientQueryObject<SearchResult<TSearchParameters, TDocument>> query)
            where TSearchParameters : class, IParameters
            where TDocument : class
        {
            return client.Query(query);
        }
    }
}