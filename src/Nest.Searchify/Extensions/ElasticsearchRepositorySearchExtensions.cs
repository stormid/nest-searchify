using Elasticsearch.Net;
using Nest.Queryify.Abstractions;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Extensions
{
    public static class ElasticsearchRepositorySearchExtensions
    {
        public static SearchResult<TDocument, ICommonParameters> Search<TDocument>(this IElasticsearchRepository repository, ICommonParameters parameters, string index = null)
            where TDocument : class
        {
            return repository.Query(new CommonParametersQuery<TDocument>(parameters), index);
        }
        
        public static SearchResult<TDocument, TParameters> Search<TDocument, TParameters>(this IElasticsearchRepository repository,
            CommonParametersQuery<TParameters, TDocument, SearchResult<TDocument, TParameters>> query, string index = null) 
            where TDocument : class
            where TParameters : class, ICommonParameters
        {
            return repository.Query(query, index);
        }

        public static TSearchResult Search<TDocument, TSearchParameters, TSearchResult>(this IElasticsearchRepository repository,
            CommonParametersQuery<TSearchParameters, TDocument, TSearchResult> query, string index = null)
            where TDocument : class
            where TSearchParameters : ICommonParameters
            where TSearchResult : SearchResult<TDocument, TSearchParameters>
        {
            return repository.Query(query, index);
        }

        public static SearchResult<TDocument, TReturnDocument, ICommonParameters> Search<TDocument, TReturnDocument>(this IElasticsearchRepository repository,
            CommonParametersQuery<TDocument, TReturnDocument> query, string index = null) where TDocument : class
            where TReturnDocument : class
        {
            return repository.Query(query, index);
        }

        public static TSearchResult Search<TDocument, TReturnDocument, TSearchParameters, TSearchResult>(this IElasticsearchRepository repository,
            CommonParametersQuery<TSearchParameters, TDocument, TReturnDocument, TSearchResult> query, string index = null)
            where TDocument : class
            where TReturnDocument : class
            where TSearchParameters : ICommonParameters
            where TSearchResult : SearchResult<TDocument, TReturnDocument, TSearchParameters>
        {
            return repository.Query(query, index);
        }
    }
}
