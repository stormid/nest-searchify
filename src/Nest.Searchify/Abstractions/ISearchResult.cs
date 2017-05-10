using System.Collections.Generic;

namespace Nest.Searchify.Abstractions
{
    public interface ISearchResultAggregations<out TParameters> : ISearchResultBase<TParameters>
        where TParameters : class, IPagingParameters, ISortingParameters
    {
        AggregationsHelper AggregationHelper { get; }
        IReadOnlyDictionary<string, IAggregate> Aggregations { get; }
    }

    public interface ISearchResult<out TParameters, out TReturnAs> : ISearchResultAggregations<TParameters>
        where TParameters : class, IPagingParameters, ISortingParameters
        where TReturnAs : class
    {
        IEnumerable<TReturnAs> Documents { get; }
    }
}