using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Abstractions
{
    public interface ISearchService
    {
        TSearchResult Search<TSearchParameters, TDocument, TSearchResult>(
            IElasticClientQueryObject<TSearchResult> query)
            where TSearchResult : class, ISearchResult<TSearchParameters, TDocument>
            where TSearchParameters : class, ICommonParameters
            where TDocument : class;

        ISearchResult<TSearchParameters, TDocument> Search<TSearchParameters, TDocument>(
            IElasticClientQueryObject<SearchResult<TSearchParameters, TDocument>> query)
            where TSearchParameters : class, ICommonParameters
            where TDocument : class;
    }
}