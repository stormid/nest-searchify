using System.Collections.Specialized;

namespace Nest.Searchify.Queries
{
	public class SearchParametersFilteredQuery<TDocument> :
		SearchParametersFilteredQuery<SearchParameters, TDocument>
		where TDocument : class
	{
		public SearchParametersFilteredQuery()
		{
		}

		public SearchParametersFilteredQuery(NameValueCollection parameters)
			: base(parameters)
		{
		}

		public SearchParametersFilteredQuery(SearchParameters parameters) : base(parameters)
		{
		}
	}

	public class SearchParametersFilteredQuery<TSearchParameters, TDocument> : SearchParametersFilteredQuery<TSearchParameters, TDocument, TDocument>
		where TSearchParameters : SearchParameters, new()
	where TDocument : class
	{
		public SearchParametersFilteredQuery()
		{
		}

		public SearchParametersFilteredQuery(NameValueCollection parameters)
			: base(parameters)
		{
		}

		public SearchParametersFilteredQuery(TSearchParameters parameters)
			: base(parameters)
		{
		}
	}

	public class SearchParametersFilteredQuery<TSearchParameters, TDocument, TReturnAs> : CommonParametersQuery<TSearchParameters, TDocument, TReturnAs>
		where TSearchParameters : SearchParameters, new()
		where TDocument : class
		where TReturnAs : class
	{
		public SearchParametersFilteredQuery() : base(new NameValueCollection()) { }

		public SearchParametersFilteredQuery(NameValueCollection parameters) : base(parameters) { }

		public SearchParametersFilteredQuery(TSearchParameters parameters)
			: base(parameters)
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
