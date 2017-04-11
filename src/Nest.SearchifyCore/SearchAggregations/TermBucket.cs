using System.Collections.Generic;
using System.Linq;

namespace Nest.Searchify.SearchAggregations
{
    public class TermBucket : Bucket<TermKeyItem>, ITypedAggregration
    {
        public string Type => "Term";

        public TermBucket()
        {
        }

        public static KeyValuePair<string, IAggregation> From(KeyValuePair<string, IAggregation> aggregation)
        {
            return new KeyValuePair<string, IAggregation>(aggregation.Key, new TermBucket(aggregation.Value));
        }

        private static IEnumerable<IAggregation> GetItems(IAggregation aggregation)
        {
            if (aggregation is Bucket)
            {
                var b = aggregation as Bucket;
                return b.Items;
            }
            return Enumerable.Empty<IAggregation>();
        }

        public TermBucket(IAggregation aggregation) : this(GetItems(aggregation))
        {
            
        }

        public TermBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<KeyItem>().Select(x => new TermKeyItem(x)).ToList();
        }

        public TermBucket(IDictionary<string, IAggregation> aggregations) : base(aggregations)
        {
        }
    }
}