using System.Collections.Generic;
using System.Linq;

namespace Nest.Searchify.SearchAggregations
{
    internal static class SearchAggregationParser
    {
        public static IDictionary<string, IAggregation> Parse(IDictionary<string, IAggregation> aggregations)
        {
            if (aggregations == null || !aggregations.Any()) return null;
            return ParseCore(aggregations).ToDictionary(k => k.Key, v => v.Value);
        }

        private static IEnumerable<KeyValuePair<string, IAggregation>> ParseCore(IDictionary<string, IAggregation> aggregations)
        {
            // TODO : yes, this is a mess
            foreach (var agg in aggregations)
            {
                if (agg.Value is Bucket) // if its a bucket
                {
                    var bucket = agg.Value as Bucket;
                    if (bucket.Items.All(x => x.GetType() == typeof(KeyItem)))
                    {
                        yield return TermBucket.From(agg);
                    }
                    else if (bucket.Items.All(x => x.GetType() == typeof (RangeItem)))
                    {
                        yield return new KeyValuePair<string, IAggregation>(agg.Key, new RangeBucket(bucket.Items));
                    }
                    else if (bucket.Items.All(x => x.GetType() == typeof (SignificantTermItem)))
                    {
                        yield return new KeyValuePair<string, IAggregation>(agg.Key, new SignificantTermBucket(bucket.Items));
                    }
                }
                else
                {
                    yield return agg;
                }
            }
        }
    }
}