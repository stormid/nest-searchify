using System.Collections.Generic;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace SearchifyCoreSample
{
    public class PersonSearchParameters : SearchParameters
    {
        public const string AgeRangeParameter = "age";

        public PersonSearchParameters()
        {
            
        }

        public PersonSearchParameters(int size, int page) : base(size, page) { }

        public string Country { get; set; }

        [JsonProperty(AgeRangeParameter)]
        public int? AgeRange { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public int? LocationRange { get; set; }

        [JsonProperty(PropertyName = "locpt")]
        public GeoLocationParameter Location { get; set; }

        public double? Radius { get; set; }
    }
}