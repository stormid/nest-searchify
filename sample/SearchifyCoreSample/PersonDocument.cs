using System.Collections.Generic;
using Nest;
using Nest.Searchify;

namespace SearchifyCoreSample
{
    public class PersonDocument
    {
        [Keyword]
        public string Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public FilterField Country { get; set; }

        public IEnumerable<FilterField> Tags { get; set; }

        //[GeoPoint]
        //public GeoLocation Location { get; set; }
    }
}