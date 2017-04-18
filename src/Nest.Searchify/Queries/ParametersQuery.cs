using System;
using System.Collections.Generic;
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

    public class ParametersQuery<TParameters, TDocument, TSearchResult, TOutputEntity> : SearchResultQuery<TParameters, TDocument, TSearchResult, TOutputEntity>
        where TParameters : class, IPagingParameters, ISortingParameters, new()
        where TDocument : class
        where TOutputEntity : class
        where TSearchResult : SearchResult<TParameters, TDocument, TOutputEntity>
    {
#if !NETSTANDARD
        public ParametersQuery() : this(new System.Collections.Specialized.NameValueCollection())
        {
        }

        public ParametersQuery(System.Collections.Specialized.NameValueCollection parameters) : base(QueryStringParser<TParameters>.Parse(parameters))
        {
        }
#else
        public ParametersQuery() : this(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(""))
        {
        }

        public ParametersQuery(Dictionary<string, Microsoft.Extensions.Primitives.StringValues> parameters) : base(QueryStringParser<TParameters>.Parse(parameters))
        {
        }
#endif

        public ParametersQuery(TParameters parameters) : base(parameters)
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
            
            descriptor.Sort(s => ApplySortingCore(s, parameters));

            descriptor.Query(q => BuildQueryCore(q, parameters));

            descriptor.Aggregations(agg => ApplyAggregationsCore(agg, parameters));

            ModifySearchDescriptor(descriptor, parameters);

            return descriptor;
        }

        private AggregationContainerDescriptor<TDocument> ApplyAggregationsCore(AggregationContainerDescriptor<TDocument> descriptor, TParameters parameters)
        {
            return ApplyAggregations(descriptor, parameters);
        }

        protected virtual AggregationContainerDescriptor<TDocument> ApplyAggregations(AggregationContainerDescriptor<TDocument> descriptor, TParameters parameters)
        {
            return descriptor;
        }

        protected virtual void ApplyPaging(SearchDescriptor<TDocument> descriptor, TParameters parameters)
        {
            descriptor
                .From(parameters.Start())
                .Size(parameters.Size);
        }

        protected virtual SortDescriptor<TDocument> ApplySortingCore(SortDescriptor<TDocument> descriptor, TParameters parameters)
        {
            var sortField = string.IsNullOrWhiteSpace(parameters.SortBy) ? null : SortByField(parameters.SortBy);
            return ApplySorting(descriptor, parameters, sortField, GetSortOrderFromParameters());
        }

        protected virtual SortDescriptor<TDocument> ApplySorting(SortDescriptor<TDocument> descriptor, TParameters parameters, Field sortField, SortOrder sortOrder)
        {
            if (parameters.HasSort() && sortField != null)
            {
                return descriptor.Field(sortField, sortOrder);
            }
            return null;
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

        protected virtual Field SortByField(string sortBy)
        {
            return string.IsNullOrWhiteSpace(sortBy) ? null : $"{sortBy}.keyword";
        }
    }
}