using System.Collections.Generic;

namespace Nest.Searchify.Abstractions
{
    public interface ISearchResult<out TParameters, out TReturnAs> : ISearchResultBase<TParameters>
        where TParameters : class, ICommonParameters
        where TReturnAs : class
    {
        IEnumerable<TReturnAs> Documents { get; }
        AggregationsHelper AggregationHelper { get; }
        IDictionary<string, IAggregation> Aggregations { get; }
    }
}