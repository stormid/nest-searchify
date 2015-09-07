using System.Collections.Generic;
using System.Linq;

namespace Nest.Searchify.SearchAggregations
{
    public class SignificantTermBucket : BucketWithDocCount<SignificantTermKeyItem>
    {
        public string Type { get { return "SignificantTerm"; } }

        public SignificantTermBucket()
        {
        }

        public SignificantTermBucket(IEnumerable<IAggregation> items)
        {
            Items = items.OfType<SignificantTermItem>().Select(x => new SignificantTermKeyItem(x)).ToList();
        }

        public SignificantTermBucket(IDictionary<string, IAggregation> aggregations) : base(aggregations)
        {
        }
    }
}