using Nest.Searchify.Abstractions;

namespace Nest.Searchify
{
    public class AggregationFilterItemModel<TParameters> where TParameters : class, ISearchParameters, new()
    {
        public string Term { get; set; }
        public string Value { get; set; }
        public long? DocCount { get; set; }
        public bool Selected { get; set; }
        public TParameters Parameters { get; set; }
    }
}