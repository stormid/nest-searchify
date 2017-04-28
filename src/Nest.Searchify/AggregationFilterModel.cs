using System.Collections.Generic;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    public class AggregationFilterModel<TParameters> : IAggregate where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<AggregationFilterItemModel<TParameters>> Items { get; set; } = new List<AggregationFilterItemModel<TParameters>>();

        [JsonIgnore]
        public IReadOnlyDictionary<string, object> Meta { get; set; }
    }
}
