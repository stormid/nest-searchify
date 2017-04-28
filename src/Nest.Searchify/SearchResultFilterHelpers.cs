using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    public static class SearchResultFilterHelpers
    {
        public static AggregationFilterModel<TParameters> FilterFor<TParameters, TDocument>(
            ISearchResult<TParameters, TDocument> searchResult,
            Expression<Func<TParameters, object>> expression,
            string filterDisplayName = null, 
            IDictionary<string, Func<ISearchResult<TParameters, TDocument>, Expression<Func<TParameters, object>>, string, AggregationFilterModel<TParameters>>> additionalFilterProviders = null)
            where TDocument : class
            where TParameters : class, ISearchParameters, new()
        {
            var providers =
                new Dictionary<string, Func<ISearchResult<TParameters, TDocument>, Expression<Func<TParameters, object>>
                    , string, AggregationFilterModel<TParameters>>>(additionalFilterProviders ?? new Dictionary<string, Func<ISearchResult<TParameters, TDocument>, Expression<Func<TParameters, object>>, string, AggregationFilterModel<TParameters>>>())
                {
                    {"term", TermFilterFor },
                    {"sigterm", SignificantTermFilterFor },
                    {"multi_term", MultiTermFilterFor },
                    {"range", RangeFilterFor }
                };

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

            if (searchResult.Aggregations.TryGetValue(filterName, out IAggregate agg))
            {
                if (agg.Meta != null)
                {
                    if (agg.Meta.TryGetValue("type", out object type))
                    {
                        if (providers.TryGetValue(type.ToString(),
                            out Func<ISearchResult<TParameters, TDocument>,
                                Expression<Func<TParameters, object>>,
                                string, AggregationFilterModel<TParameters>> provider))
                        {
                            return provider(searchResult, expression, filterDisplayName);
                        }
                    }
                }
            }
            return null;
        }

        private static AggregationFilterModel<TParameters> MultiTermFilterFor<TParameters, TDocument>(
            ISearchResult<TParameters, TDocument> searchResult,
            Expression<Func<TParameters, object>> expression,
            string filterDisplayName = null)
            where TDocument : class
            where TParameters : class, ISearchParameters, new()
        {
            var model = new AggregationFilterModel<TParameters>();

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

            var agg = searchResult.AggregationHelper.Terms(filterName);
            if (agg == null)
            {
                return model;
            }

            var filterValue = ((IEnumerable<string>)expression.Compile().Invoke(searchResult.Parameters) ?? Enumerable.Empty<string>()).ToList();
            var propertyInfo = (PropertyInfo)memberExpression?.Member;
            model.Name = filterDisplayName ?? filterName;
            model.Type = "multi_term";

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(searchResult.Parameters);

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

                propertyInfo?.SetValue(parameters, filterValueList);

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

        private static AggregationFilterModel<TParameters> TermFilterFor<TParameters, TDocument>(
            ISearchResult<TParameters, TDocument> searchResult,
            Expression<Func<TParameters, object>> expression,
            string filterDisplayName = null)
            where TDocument : class
            where TParameters : class, ISearchParameters, new()
        {
            var model = new AggregationFilterModel<TParameters>();

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

            var agg = searchResult.AggregationHelper.Terms(filterName);
            if (agg == null)
            {
                return model;
            }

            var filterValue = expression.Compile().Invoke(searchResult.Parameters)?.ToString() ?? string.Empty;
            var propertyInfo = (PropertyInfo)memberExpression?.Member;
            model.Name = filterDisplayName ?? filterName;
            model.Type = "term";

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(searchResult.Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue);

                if (propertyInfo != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ??
                                                  propertyInfo.PropertyType;
                    propertyInfo.SetValue(parameters,
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

        private static AggregationFilterModel<TParameters> SignificantTermFilterFor<TParameters, TDocument>(
            ISearchResult<TParameters, TDocument> searchResult,
            Expression<Func<TParameters, object>> expression,
            string filterDisplayName = null)
            where TDocument : class
            where TParameters : class, ISearchParameters, new()
        {
            var model = new AggregationFilterModel<TParameters>();

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

            var agg = searchResult.AggregationHelper.SignificantTerms(filterName);
            if (agg == null)
            {
                return model;
            }

            var filterValue = expression.Compile().Invoke(searchResult.Parameters);
            var propertyInfo = (PropertyInfo)memberExpression?.Member;
            model.Name = filterDisplayName ?? filterName;
            model.Type = "sigterm";

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(searchResult.Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue.ToString());

                if (propertyInfo != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ??
                                                  propertyInfo.PropertyType;
                    propertyInfo.SetValue(parameters,
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

        private static AggregationFilterModel<TParameters> RangeFilterFor<TParameters, TDocument>(
            ISearchResult<TParameters, TDocument> searchResult,
            Expression<Func<TParameters, object>> expression,
            string filterDisplayName = null)
            where TDocument : class
            where TParameters : class, ISearchParameters, new()
        {
            var model = new AggregationFilterModel<TParameters>();

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

            // ++ Range
            var agg = searchResult.AggregationHelper.Range(filterName);
            if (agg == null)
            {
                return model;
            }
            // -- Range
            
            var filterValue = expression.Compile().Invoke(searchResult.Parameters) ?? string.Empty;
            var propertyInfo = (PropertyInfo)memberExpression?.Member;
            model.Name = filterDisplayName ?? filterName;
            model.Type = "range";

            model.Items = agg.Buckets.Select(item =>
            {
                var parameters = QueryStringParser<TParameters>.Copy(searchResult.Parameters);

                var term = item.Key;
                var value = item.Key;

                if (FilterField.TryParse(item.Key, out FilterField filterField))
                {
                    term = filterField.Text;
                    value = filterField.Value;
                }

                var isSelected = value.Equals(filterValue.ToString());

                if (propertyInfo != null)
                {
                    var convertablePropertyInfo = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ??
                                                  propertyInfo.PropertyType;
                    propertyInfo.SetValue(parameters,
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
    }
}