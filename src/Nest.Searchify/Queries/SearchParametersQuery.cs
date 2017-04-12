using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public class SearchParametersQuery<TSearchParameters, TDocument, TSearchResult> : SearchParametersQuery<TSearchParameters, TDocument, TSearchResult, TDocument>
		where TSearchParameters : class, ISearchParameters, new()
	where TDocument : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument>
	{
        public SearchParametersQuery(TSearchParameters parameters) : base(parameters)
        {
        }
    }

    public class SearchParametersQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity> :
        ParametersQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity>
        where TSearchParameters : class, ISearchParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument, TOutputEntity>
    {
        public SearchParametersQuery(TSearchParameters parameters) : base(parameters)
        {
        }
        
        protected virtual QueryContainer WithQueryCore(IQueryContainer query, TSearchParameters parameters)
        {
            return !string.IsNullOrWhiteSpace(parameters.Query)
                ? Query<TDocument>.QueryString(q => q.Query(parameters.Query))
                : Query<TDocument>.MatchAll();
        }
        
        protected sealed override QueryContainer BuildQueryCore(QueryContainer query, TSearchParameters parameters)
        {
            return WithQueryCore(query, parameters);
        }
    }
}
