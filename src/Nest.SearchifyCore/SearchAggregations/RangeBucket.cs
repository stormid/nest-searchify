using System.Collections.Generic;
using System.Linq;

namespace Nest.Searchify.SearchAggregations
{
    public class RangeBucket : Bucket<RangeKeyItem>, ITypedAggregration
    {
        public string Type => "Range";

        public RangeBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<RangeItem>().Select(x => new RangeKeyItem(x)).ToList();
        }
    }
}