using Nest.Queryify.Abstractions;
using Nest.Queryify.Extensions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Extensions
{
    public static class SearchifySearchExtensions
    {
        public static SearchResult<TDocument, ICommonParameters> Search<TDocument>(this IElasticClient client,
            CommonParametersQuery<TDocument> query, string index = null) where TDocument : class
        {
            return client.Query(query, index);
        }

        public static TSearchResult Search<TDocument, TSearchParameters, TSearchResult>(this IElasticClient client,
            CommonParametersQuery<TSearchParameters, TDocument, TSearchResult> query, string index = null) 
            where TDocument : class
            where TSearchParameters : ICommonParameters
            where TSearchResult : SearchResult<TDocument, TSearchParameters>
        {
            return client.Query(query, index);
        }

        public static SearchResult<TDocument, TReturnDocument, ICommonParameters> Search<TDocument, TReturnDocument>(this IElasticClient client,
            CommonParametersQuery<TDocument, TReturnDocument> query, string index = null) where TDocument : class 
            where TReturnDocument : class
        {
            return client.Query(query, index);
        }

        public static TSearchResult Search<TDocument, TReturnDocument, TSearchParameters, TSearchResult>(this IElasticClient client,
            CommonParametersQuery<TSearchParameters, TDocument, TReturnDocument, TSearchResult> query, string index = null)
            where TDocument : class
            where TReturnDocument : class
            where TSearchParameters : ICommonParameters
            where TSearchResult : SearchResult<TDocument, TReturnDocument, TSearchParameters>
        {
            return client.Query(query, index);
        }

    }
}
