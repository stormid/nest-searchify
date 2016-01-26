using System;
using System.Collections.Specialized;
using Nest.Searchify.Abstractions;
using Nest.Searchify.SearchResults;

namespace Nest.Searchify.Queries
{
    public class ParametersQuery<TParameters, TDocument, TSearchResult> : ParametersQuery<TParameters, TDocument, TSearchResult, TDocument>
        where TParameters : class, IPagingParameters, ISortingParameters, new()
		where TDocument : class
        where TSearchResult : SearchResult<TParameters, TDocument>
	{
        public ParametersQuery(TParameters parameters) : base(parameters) { }
    }

    public partial class ParametersQuery<TParameters, TDocument, TSearchResult, TOutputEntity> : SearchResultQuery<TParameters, TDocument, TSearchResult, TOutputEntity>
        where TParameters : class, IPagingParameters, ISortingParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TParameters, TOutputEntity>
    {
        public ParametersQuery(NameValueCollection parameters) : base(QueryStringParser<TParameters>.Parse(parameters))
        {
        }

        public ParametersQuery(TParameters parameters) : base(parameters)
        {
        }

        public ParametersQuery() : this(new NameValueCollection())
        {
        }

        protected virtual TParameters ModifyParameters(TParameters parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters), "the parameters object must not be null");
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
    }
}