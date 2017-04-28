using System.Collections.Generic;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    public interface IParameterisedAggregate<TParameters> : IAggregate
        where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        bool Selected { get; set; }
        TParameters Parameters { get; set; }
    }

    public interface IParameterisedBucketAggregate<TParameters> : IParameterisedAggregate<TParameters>
        where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        string Term { get; set; }
        string Value { get; set; }
        long? DocCount { get; set; }

    }

    public class AggregationFilterItemModel<TParameters> : IParameterisedBucketAggregate<TParameters> where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        public string Term { get; set; }
        public string Value { get; set; }
        public long? DocCount { get; set; }
        public bool Selected { get; set; }
        public virtual TParameters Parameters { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, object> Meta { get; set; }
    }
}