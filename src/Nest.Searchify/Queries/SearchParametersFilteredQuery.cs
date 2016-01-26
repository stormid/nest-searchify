using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public class SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult> : SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult, TDocument>
		where TSearchParameters : class, ISearchParameters, new()
	where TDocument : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument>
	{
        public SearchParametersFilteredQuery(TSearchParameters parameters) : base(parameters)
        {
        }
    }

    public class SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity> :
        ParametersQuery<TSearchParameters, TDocument, TSearchResult, TOutputEntity>
        where TSearchParameters : class, ISearchParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument, TOutputEntity>
    {
        public SearchParametersFilteredQuery(TSearchParameters parameters) : base(parameters)
        {
        }

        protected virtual QueryContainer WithQuery(IQueryContainer query, string queryTerm)
        {
            return Query<TDocument>.QueryString(q => q.Query(queryTerm));
        }

        protected QueryContainer WithQueryCore(IQueryContainer query, TSearchParameters parameters)
        {
            return !string.IsNullOrWhiteSpace(parameters.Query)
                ? WithQuery(query, parameters.Query.ToLowerInvariant())
                : Query<TDocument>.MatchAll();
        }

        protected FilterContainer WithFilterCore(IFilterContainer filter, TSearchParameters parameters)
        {
            return WithFilter(filter, parameters);
        }

        protected virtual FilterContainer WithFilter(IFilterContainer filter, TSearchParameters parameters)
        {
            return null;
        }

        protected override sealed QueryContainer BuildQueryCore(QueryContainer query, TSearchParameters parameters)
        {
            return Query<TDocument>
                .Filtered(fq => fq
                    .Filter(f => WithFilterCore(f, parameters))
                    .Query(q => WithQueryCore(q, parameters))
                );
        }
    }
}
