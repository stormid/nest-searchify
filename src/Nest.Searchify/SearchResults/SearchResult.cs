using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public abstract partial class SearchResult<TParameters, TDocument, TOutputEntity>
        where TDocument : class
        where TOutputEntity : class
        where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        private static readonly IEnumerable<KeyValuePair<string, PropertyInfo>> ParameterPropertyCache = typeof(TParameters).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToParameterLookups().ToList();
        private readonly Dictionary<string, Func<string, IAggregate>> filterRegistry = new Dictionary<string, Func<string, IAggregate>>();

        protected void AddFilterAggregationProvider(string type, Func<string, IAggregate> provider)
        {
            if (filterRegistry.ContainsKey(type))
            {
                filterRegistry[type] = provider;
            }
            else
            {
                filterRegistry.Add(type, provider);
            }
        }

        protected IAggregate FilterFor(string filterName)
        {
            var aggTypeDescriptor = string.Empty;
            if (Response.Aggregations.TryGetValue(filterName, out IAggregate agg))
            {
                if (agg.Meta != null)
                {
                    if (agg.Meta.TryGetValue("type", out object type))
                    {
                        aggTypeDescriptor = type.ToString();
                    }
                }
            }

            if (!filterRegistry.TryGetValue(aggTypeDescriptor, out Func<string, IAggregate> provider))
            {
                if (!Response.Aggregations.ContainsKey(filterName))
                {
                    throw new ArgumentOutOfRangeException(nameof(filterName), "Unable to find filter (aggregation) with the given name");
                }
                return Response.Aggregations[filterName];
            }

            return provider(filterName);
        }

        protected AggregationFilterModel<TParameters> SignificantTermsFilterFor(string filterName)
        {
            var propertyInfo = ParameterPropertyCache.FirstOrDefault(c => c.Key == filterName);
            if (string.IsNullOrWhiteSpace(propertyInfo.Key))
            {
                throw new ArgumentOutOfRangeException(nameof(filterName), $"Unable to find parameter named '{filterName}', ensure that either the parameters has a matching property or that a JsonProperty attribute is assigned");
            }

            var filterValue = propertyInfo.Value.GetValue(Parameters);

            var model = new AggregationFilterModel<TParameters>();
            var agg = AggregationHelper.SignificantTerms(filterName);
            if (agg == null)
            {
                return model;
            }

            model.Name = filterName;
            model.Type = nameof(AggregationHelper.SignificantTerms);

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue);

                if (propertyInfo.Value != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.Value.PropertyType) ??
                                                  propertyInfo.Value.PropertyType;
                    propertyInfo.Value.SetValue(parameters,
                        isSelected ? null : Convert.ChangeType(value, convertablePropertyInfo));
                }

                return new AggregationFilterItemModel<TParameters>()
                {
                    Term = term,
                    Value = value,
                    DocCount = item.DocCount,
                    Selected = isSelected,
                    Parameters = parameters
                };

            }).ToList();

            return model;
        }

        protected AggregationFilterModel<TParameters> SignificantTermsFilterFor(Expression<Func<TParameters, object>> expression)
        {
            var filterName = GetFilterNameFrom(expression);
            return SignificantTermsFilterFor(filterName);
        }

        protected AggregationFilterModel<TParameters> TermFilterFor(string filterName)
        {
            var propertyInfo = ParameterPropertyCache.FirstOrDefault(c => c.Key == filterName);
            if (string.IsNullOrWhiteSpace(propertyInfo.Key))
            {
                throw new ArgumentOutOfRangeException(nameof(filterName), $"Unable to find parameter named '{filterName}', ensure that either the parameters has a matching property or that a JsonProperty attribute is assigned");
            }

            if (propertyInfo.Value.PropertyType != typeof(string))
            {
                return MultiTermFilterFor(filterName);
            }

            var filterValue = propertyInfo.Value.GetValue(Parameters);

            var model = new AggregationFilterModel<TParameters>();
            var agg = AggregationHelper.Terms(filterName);
            if (agg == null)
            {
                return model;
            }

            model.Name = filterName;
            model.Type = nameof(AggregationHelper.Terms);

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue);

                if (propertyInfo.Value != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.Value.PropertyType) ??
                                                  propertyInfo.Value.PropertyType;
                    propertyInfo.Value.SetValue(parameters,
                        isSelected ? null : Convert.ChangeType(value, convertablePropertyInfo));
                }

                return new AggregationFilterItemModel<TParameters>()
                {
                    Term = term,
                    Value = value,
                    DocCount = item.DocCount,
                    Selected = isSelected,
                    Parameters = parameters
                };

            }).ToList();

            return model;
        }

        protected AggregationFilterModel<TParameters> TermFilterFor(Expression<Func<TParameters, string>> expression)
        {
            var filterName = GetFilterNameFrom(expression);
            return TermFilterFor(filterName);
        }

        protected AggregationFilterModel<TParameters> MultiTermFilterFor(string filterName)
        {
            var propertyInfo = ParameterPropertyCache.FirstOrDefault(c => c.Key == filterName);
            if (string.IsNullOrWhiteSpace(propertyInfo.Key))
            {
                throw new ArgumentOutOfRangeException(nameof(filterName), $"Unable to find parameter named '{filterName}', ensure that either the parameters has a matching property or that a JsonProperty attribute is assigned");
            }

            var filterValue = ((IEnumerable<string>)propertyInfo.Value.GetValue(Parameters)).ToList();

            var model = new AggregationFilterModel<TParameters>();
            var agg = AggregationHelper.Terms(filterName);
            if (agg == null)
            {
                return model;
            }

            model.Name = filterName;
            model.Type = nameof(AggregationHelper.Terms);
            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var filterValueList = new HashSet<string>(filterValue);

                var isSelected = filterValue.Contains(value);

                if (isSelected)
                {
                    filterValueList.Remove(value);
                }
                else
                {
                    filterValueList.Add(value);
                }

                propertyInfo.Value.SetValue(parameters, filterValueList);

                return new AggregationFilterItemModel<TParameters>()
                {
                    Term = term,
                    Value = value,
                    DocCount = item.DocCount,
                    Selected = isSelected,
                    Parameters = parameters
                };

            }).ToList();

            return model;
        }

        protected AggregationFilterModel<TParameters> MultiTermFilterFor(Expression<Func<TParameters, IEnumerable<string>>> expression)
        {
            var filterName = GetFilterNameFrom(expression);
            return MultiTermFilterFor(filterName);
        }

        protected AggregationFilterModel<TParameters> RangeFilterFor(string filterName)
        {
            var propertyInfo = ParameterPropertyCache.FirstOrDefault(c => c.Key == filterName);
            if (string.IsNullOrWhiteSpace(propertyInfo.Key))
            {
                throw new ArgumentOutOfRangeException(nameof(filterName), $"Unable to find parameter named '{filterName}', ensure that either the parameters has a matching property or that a JsonProperty attribute is assigned");
            }

            var filterValue = propertyInfo.Value.GetValue(Parameters);

            var model = new AggregationFilterModel<TParameters>();
            var agg = AggregationHelper.Range(filterName);
            if (agg == null)
            {
                return model;
            }

            model.Name = filterName;
            model.Type = nameof(AggregationHelper.Range);
            model.Type = "range";

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue?.ToString());
                var isSelected = value.Equals(filterValue.ToString());

                if (propertyInfo.Value != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.Value.PropertyType) ??
                                                  propertyInfo.Value.PropertyType;
                    propertyInfo.Value.SetValue(parameters,
                        isSelected ? null : Convert.ChangeType(value, convertablePropertyInfo));
                }
                return new AggregationFilterItemModel<TParameters>()
                {
                    Term = term,
                    Value = value,
                    DocCount = item.DocCount,
                    Selected = isSelected,
                    Parameters = parameters
                };

            }).ToList();

            return model;
        }

        protected AggregationFilterModel<TParameters> RangeFilterFor(Expression<Func<TParameters, object>> expression)
        {
            var filterName = GetFilterNameFrom(expression);
            return RangeFilterFor(filterName);
        }

        private static string GetFilterNameFrom(Expression<Func<TParameters, object>> expression)
        {
            MemberExpression memberExpression;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    memberExpression = expression.Body as MemberExpression;
                    break;
                default:
                    memberExpression = (expression.Body as UnaryExpression)?.Operand as MemberExpression;
                    break;
            }

            var filterName = memberExpression?.Member.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ??
                             memberExpression?.Member.Name;
            return filterName;
        }
    }

    public abstract partial class SearchResult<TParameters, TDocument, TOutputEntity> : SearchResultBase<TParameters>, ISearchResult<TParameters, TOutputEntity>
        where TDocument : class
        where TOutputEntity : class
        where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
        protected SearchResult(TParameters parameters) : base(parameters) { }

        [JsonProperty("documents", NullValueHandling = NullValueHandling.Ignore)]
        public virtual IEnumerable<TOutputEntity> Documents { get; protected set; }

        protected ISearchResponse<TDocument> Response { get; }

        #region Aggregations

        [JsonIgnore]
        public AggregationsHelper AggregationHelper => Response.Aggs;

        [JsonProperty("aggregations")]
        public virtual IReadOnlyDictionary<string, IAggregate> Aggregations { get; private set; }

        #endregion

        protected SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters)
        {
            Response = response;
            SetDocuments();
            SetAggregations();
        }

        private void SetAggregations()
        {
            AddFilterAggregationProvider("term", TermFilterFor);
            AddFilterAggregationProvider("multi_term", MultiTermFilterFor);
            AddFilterAggregationProvider("range", RangeFilterFor);

            Aggregations = AlterAggregations(Response.Aggregations);
        }

        private void SetDocuments()
        {
            Documents = TransformResultCore(Response);
        }

        protected IReadOnlyDictionary<string, IAggregate> AlterAggregationsCore(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            return AlterAggregations(aggregations);
        }

        protected virtual IReadOnlyDictionary<string, IAggregate> AlterAggregations(IReadOnlyDictionary<string, IAggregate> aggregations)
        {
            return aggregations.Select(a => new KeyValuePair<string, IAggregate>(a.Key, FilterFor(a.Key))).ToDictionary(k => k.Key, v => v.Value);
        }

        protected virtual IEnumerable<TDocument> ResponseToDocuments(ISearchResponse<TDocument> response)
        {
            return response.Documents;
        }

        private IEnumerable<TOutputEntity> TransformResultCore(ISearchResponse<TDocument> response)
        {
            return TransformResult(ResponseToDocuments(response)).Where(x => x != null);
        }

        protected abstract IEnumerable<TOutputEntity> TransformResult(IEnumerable<TDocument> entities);

        protected override long GetResponseTimeTaken()
        {
            return Response.Took;
        }

        protected override long GetSearchResultTotal()
        {
            return Response.Total;
        }
    }

    public class SearchResult<TParameters, TDocument> : SearchResult<TParameters, TDocument, TDocument>
        where TDocument : class
		where TParameters : class, IPagingParameters, ISortingParameters, new()
    {
		public SearchResult(TParameters parameters, ISearchResponse<TDocument> response) : base(parameters, response)
		{
        }

        protected override IEnumerable<TDocument> TransformResult(IEnumerable<TDocument> entities)
        {
            return entities;
        }

        protected override long GetResponseTimeTaken()
		{
			return Response.Took;
		}

		protected override long GetSearchResultTotal()
		{
			return Response.Total;
		}
	}
}