using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using Nest.Queryify.Abstractions.Queries;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries
{
    public class CommonParametersQuery<TDocument> : CommonParametersQuery<ICommonParameters, TDocument, SearchResult<TDocument, ICommonParameters>>
		where TDocument : class
	{
        public CommonParametersQuery(ICommonParameters parameters) : base(parameters)
        {
        }
    }

    public class CommonParametersQuery<TDocument, TReturnAs> :
		CommonParametersQuery<ICommonParameters, TDocument, TReturnAs, SearchResult<TDocument, TReturnAs, ICommonParameters>>
		where TDocument : class
		where TReturnAs : class
	{
		public CommonParametersQuery(NameValueCollection parameters)
			: base(parameters)
		{
		}

		public CommonParametersQuery(ICommonParameters parameters)
			: base(parameters)
		{
		}

		public CommonParametersQuery()
		{
		}
	}
    
    public class CommonParametersQuery<TParameters, TDocument, TSearchResult> : CommonParametersQuery<TParameters, TDocument, TDocument, TSearchResult>
    where TParameters : ICommonParameters
    where TDocument : class
    where TSearchResult : SearchResult<TDocument, TDocument, TParameters>
    {
        public CommonParametersQuery(TParameters parameters) : base(parameters)
        {
        }
    }

    public class CommonParametersQuery<TParameters, TDocument, TReturnAs, TSearchResult> : SearchResultQuery<TDocument, TReturnAs, TParameters, TSearchResult>
        where TParameters : ICommonParameters
		where TDocument : class
		where TReturnAs : class
        where TSearchResult : SearchResult<TDocument, TReturnAs, TParameters>
	{
		private static TParameters ParametersFromNameValueCollection(NameValueCollection nvc)
		{
			var values = nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
			var jsonString = JsonConvert.SerializeObject(values);
			return JsonConvert.DeserializeObject<TParameters>(jsonString);
		}

		public CommonParametersQuery(NameValueCollection parameters) : this(ParametersFromNameValueCollection(parameters))
		{
			
		}

		public CommonParametersQuery(TParameters parameters) : base(parameters)
		{
		}

		public CommonParametersQuery() : this(new NameValueCollection())
		{
		}

		protected virtual TParameters ModifyParameters(TParameters parameters)
		{
			if(parameters == null) throw new ArgumentNullException(nameof(parameters), "the parameters object must not be null");
			return parameters;
		}

		protected virtual QueryContainer BuildQueryCore(QueryContainer query, TParameters parameters)
		{
			return Query<TDocument>.MatchAll();
		}

		protected virtual void ModifySearchDescriptor(SearchDescriptor<TDocument> descriptor, TParameters parameters)
		{

		}

		protected sealed override SearchDescriptor<TDocument> BuildQuery(SearchDescriptor<TDocument> descriptor)
		{
			var parameters = ModifyParameters(Parameters);

			ApplyPaging(descriptor, parameters);

			if (!string.IsNullOrWhiteSpace(parameters.SortBy))
			{
				ApplySorting(descriptor, parameters);
			}

			descriptor.Query(q => BuildQueryCore(q, parameters));

			descriptor.Aggregations(agg => ApplyAggregationsCore(agg, parameters));

			ModifySearchDescriptor(descriptor, parameters);

			return descriptor;			
		}

		private AggregationDescriptor<TDocument> ApplyAggregationsCore(AggregationDescriptor<TDocument> descriptor, TParameters parameters)
		{
			return ApplyAggregations(descriptor, parameters);
		}

		protected virtual AggregationDescriptor<TDocument> ApplyAggregations(AggregationDescriptor<TDocument> descriptor, TParameters parameters)
		{
			return descriptor;
		}

		protected virtual void ApplyPaging(SearchDescriptor<TDocument> descriptor, TParameters parameters)
		{
			descriptor
				.From(parameters.Start())
				.Size(parameters.Size);
		}

		protected virtual void ApplySorting(SearchDescriptor<TDocument> descriptor, TParameters parameters)
		{
			if (parameters.HasSort())
			{
				descriptor.Sort(sort => ModifySortCore(WithSort(sort, parameters)));
			}
		}

		protected virtual IFieldSort ModifySortCore(IFieldSort withSort)
		{
			return withSort;
		}

		protected SortOrder GetSortOrderFromParameters()
		{
			switch (Parameters.SortDirection)
			{
				case SortDirectionOption.Desc:
					return SortOrder.Descending;
				default:
					return SortOrder.Ascending;
			}
		}

		protected virtual string SortByField(string sortBy)
		{
			return $"{sortBy}.sort";
		}

		private IFieldSort WithSort(SortFieldDescriptor<TDocument> sort, TParameters parameters)
		{
			return sort
				.OnField(SortByField(parameters.SortBy))
				.Order(GetSortOrderFromParameters());
		}

		protected FilterContainer TermFilterFor<T, K>(Expression<Func<T,K>> field, K value) where T : class
		{
			return Filter<T>.Term(field, value);
		}

		protected FilterContainer MultiTermAndFilterFor<T, K>(Expression<Func<T, K>> field, IEnumerable<K> values) where T : class
		{
			if (values != null)
			{
				return MultiTermAndFilterFor(field, values.ToArray());
			}
			return null;
		}

		protected FilterContainer MultiTermOrFilterFor<T, K>(Expression<Func<T, K>> field, IEnumerable<K> values) where T : class
		{
			if (values != null)
			{
				return MultiTermOrFilterFor(field, values.ToArray());
			}
			return null;
		}

		protected FilterContainer MultiTermAndFilterFor<T, K>(Expression<Func<T, K>> field, params K[] values) where T : class
		{
			if (values != null)
			{
				var valuesList = values.ToList();

				if (valuesList.Any())
				{

					var filters = valuesList.Select(
						termFilter => new Func<FilterDescriptor<T>, FilterContainer>(filter => Filter<T>.Term(field, termFilter)))
						.ToArray();

					return Filter<T>.And(filters);
				}
			}
			return null;
		}

		protected FilterContainer MultiTermOrFilterFor<T, K>(Expression<Func<T,K>> field, params K[] values) where T : class
		{
			if (values != null)
			{
				var valuesList = values.ToList();

				if (valuesList.Any())
				{

					var filters = valuesList.Select(
						termFilter => new Func<FilterDescriptor<T>, FilterContainer>(filter => Filter<T>.Term(field, termFilter)))
						.ToArray();

					return Filter<T>.Or(filters);
				}
			}
			return null;
		}
	}
}