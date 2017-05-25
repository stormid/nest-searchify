using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nest.Searchify.Queries;

namespace Nest.Searchify.Extensions
{
    public static class SearchifyFilterExtensions
    {
        public static readonly string AggregationTypeKey = nameof(AggregationFilterModel<SearchParameters>.Type);
        public static readonly string AggregationDisplayNameKey = nameof(AggregationFilterModel<SearchParameters>.DisplayName);

        private static readonly IReadOnlyDictionary<Type, string> AggregrationTypeLookup = new Dictionary<Type, string>
        {
            { typeof(ITermsAggregation), nameof(AggregationContainer.Terms) },
            { typeof(ISignificantTermsAggregation), nameof(AggregationContainer.SignificantTerms) },
            { typeof(IRangeAggregation), nameof(AggregationContainer.Range) },
            { typeof(IGeoDistanceAggregation), nameof(AggregationContainer.GeoDistance) }
        };

        private static string ResolveAggregationTypeName<TAggregation>(TAggregation aggregation)
            where TAggregation : IAggregation
        {
            var type = AggregrationTypeLookup.Keys.FirstOrDefault(t => t.GetTypeInfo().IsInstanceOfType(aggregation));
            if (type != null)
            {
                return AggregrationTypeLookup.TryGetValue(type, out string typeName) ? typeName : null;
            }
            return null;
        }

        public static TAggregation AsSearchifyFilter<TAggregation>(this TAggregation descriptor, Action<SearchifyAggregationConfigurer<TAggregation>> configure = null)
            where TAggregation : class, IAggregation
        {
            var typeName = ResolveAggregationTypeName(descriptor);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new NotSupportedException("Unfortunately this aggregation is not currently supported by the searchify filter abstraction, raise an issue if you would like to see it included");
            }
            var config = new SearchifyAggregationConfigurer<TAggregation>(descriptor);
            configure?.Invoke(config);

            var fd = new FluentDictionary<string, object>(descriptor.Meta);
            fd.AddOrUpdate(nameof(AggregationFilterModel<SearchParameters>.Type), typeName);
            descriptor.Meta = fd;
            return descriptor;
        }

        internal static string SearchifyAggregationType<TAggregate>(this TAggregate aggregate) where TAggregate : IAggregate
        {
            if (aggregate.Meta == null)
            {
                return null;
            }
            return aggregate.Meta.TryGetValue(AggregationTypeKey, out object value) ? value?.ToString() : null;
        }

        internal static string SearchifyDisplayName<TAggregate>(this TAggregate aggregate) where TAggregate : IAggregate
        {
            if (aggregate.Meta == null)
            {
                return null;
            }
            return aggregate.Meta.TryGetValue(AggregationDisplayNameKey, out object value) ? value?.ToString() : null;
        }
    }

    public sealed class SearchifyAggregationConfigurer<TAggregation> where TAggregation : class, IAggregation
    {
        private readonly TAggregation descriptor;

        internal SearchifyAggregationConfigurer(TAggregation descriptor)
        {
            this.descriptor = descriptor;
        }

        public SearchifyAggregationConfigurer<TAggregation> WithDisplayName(string displayName)
        {
            var fd = new FluentDictionary<string, object>(descriptor.Meta);
            fd.AddOrUpdate(SearchifyFilterExtensions.AggregationDisplayNameKey, displayName, v => !string.IsNullOrWhiteSpace(displayName));
            descriptor.Meta = fd;
            
            return this;
        }
    }
}
