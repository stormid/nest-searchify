using System.Collections.Generic;
using System.Linq;

namespace Nest.Searchify.SearchAggregations
{
    internal static class SearchAggregationParser
    {
        public static IDictionary<string, IAggregate> Parse(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            if (aggregations == null || !aggregations.Any()) return null;
            var keyValuePairs = ParseCore(aggregations);
            return keyValuePairs.ToDictionary(k => k.Key, v => v.Value);
        }

        private static IEnumerable<KeyValuePair<string, IAggregate>> ParseCore(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            var d = new Dictionary<string, IAggregate>();
            foreach (var agg in aggregations)
            {
                var key = agg.Key;
                var value = agg.Value;
                switch (agg.Value)
                {
                    case BucketAggregate b:
                        switch (b.Items)
                        {
                            case IReadOnlyCollection<KeyedBucket<object>> k:
                                // term bucket
                                value = b;
                                break;
                        }

                        break;
                }
                d.Add(key, value);
            }
            return d;
        }
    }
}