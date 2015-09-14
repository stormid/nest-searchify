using System.Collections.Specialized;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public class SearchParametersFilteredQuery<TDocument> :
        SearchParametersFilteredQuery<ISearchParameters, TDocument, SearchResult<ISearchParameters, TDocument>>
        where TDocument : class
    {
        public SearchParametersFilteredQuery(ISearchParameters parameters) : base(parameters)
        {
        }
    }

    public class SearchParametersFilteredQuery<TDocument, TSearchResult> :
		SearchParametersFilteredQuery<ISearchParameters, TDocument, TSearchResult>
		where TDocument : class
        where TSearchResult : SearchResult<ISearchParameters, TDocument>
	{
        public SearchParametersFilteredQuery(ISearchParameters parameters) : base(parameters)
        {
        }
    }

    public class SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult> : SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult, TDocument>
		where TSearchParameters : class, ISearchParameters
	where TDocument : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument>
	{
        public SearchParametersFilteredQuery(TSearchParameters parameters) : base(parameters)
        {
        }
    }

    public class SearchParametersFilteredQuery<TSearchParameters, TDocument, TSearchResult, TReturnAs> : CommonParametersQuery<TSearchParameters, TDocument, TSearchResult, TReturnAs>
		where TSearchParameters : class, ISearchParameters
		where TDocument : class
		where TReturnAs : class
        where TSearchResult : SearchResult<TSearchParameters, TDocument>
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
			return !string.IsNullOrWhiteSpace(parameters.Query) ? WithQuery(query, parameters.Query.ToLowerInvariant()) : Query<TDocument>.MatchAll();
		}

		protected FilterContainer WithFilterCore(IFilterContainer filter, TSearchParameters parameters)
		{
			return WithFilter(filter, parameters);
		}

		protected virtual FilterContainer WithFilter(IFilterContainer filter, TSearchParameters parameters)
		{
			return null;
		}

		protected sealed override QueryContainer BuildQueryCore(QueryContainer query, TSearchParameters parameters)
		{
			return Query<TDocument>
				.Filtered(fq => fq
					.Filter(f => WithFilterCore(f, parameters))
					.Query(q => WithQueryCore(q, parameters))
				);
		}
	}
}
