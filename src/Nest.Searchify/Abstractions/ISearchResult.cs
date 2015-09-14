using System.Collections.Generic;

namespace Nest.Searchify.Abstractions
{
    public interface ISearchResult<out TParameters, out TOutputEntity> : ISearchResultBase<TParameters>
        where TParameters : class, ICommonParameters
        where TOutputEntity : class
    {
        IEnumerable<TOutputEntity> Documents { get; }
        AggregationsHelper AggregationHelper { get; }
        IDictionary<string, IAggregation> Aggregations { get; }
    }
}