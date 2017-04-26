using System.Collections.Generic;
using Nest.Searchify.Abstractions;

namespace Nest.Searchify
{
    public class AggregationFilterModel<TParameters> where TParameters : class, ISearchParameters, new()
    {
        public string Name { get; set; }
        public IEnumerable<AggregationFilterItemModel<TParameters>> Items { get; set; } = new List<AggregationFilterItemModel<TParameters>>();
    }
}
