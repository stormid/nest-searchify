using System.Collections;
using System.Collections.Generic;
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

        protected virtual QueryContainer WithQuery(IQueryContainer query, string queryTerm)
        {
            return !string.IsNullOrWhiteSpace(queryTerm)
                ? Query<TDocument>.QueryString(q => q.Query(queryTerm))
                : Query<TDocument>.MatchAll();
        }

        protected virtual QueryContainer BuildQuery(TSearchParameters parameters, QueryContainer query, QueryContainer filters)
        {
            return Query<TDocument>
                .Bool(b => b
                    .Must(query)
                    .Filter(filters)
                );
        }

        protected virtual QueryContainer WithFilters(IQueryContainer query, TSearchParameters parameters)
        {
            return null;
        }

        protected sealed override QueryContainer BuildQueryCore(QueryContainer query, TSearchParameters parameters)
        {
            return BuildQuery(
                parameters,
                WithQuery(query, parameters.Query),
                WithFilters(query, parameters)
            );
        }
    }
}
