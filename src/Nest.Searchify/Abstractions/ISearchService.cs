using Nest.Queryify.Abstractions.Queries;

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
            IElasticClientQueryObject<ISearchResult<TSearchParameters, TDocument>> query)
            where TSearchParameters : class, ICommonParameters
            where TDocument : class;
    }
}