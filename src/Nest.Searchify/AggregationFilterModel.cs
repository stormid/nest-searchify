using System.Collections.Generic;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    public class AggregationFilterModel<TParameters> : IAggregate where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public string Type { get; set; }
        public IEnumerable<IParameterisedAggregate<TParameters>> Items { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, object> Meta { get; set; }
    }
}
