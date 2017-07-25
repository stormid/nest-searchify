using System;
using System.Collections.Generic;
using System.Net;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;

namespace Nest.Searchify.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrl(this string input)
        {
            return WebUtility.UrlEncode(input.Replace(" ", "-").ToLowerInvariant());
        }

        public static bool ToBool(this string value, Action<Dictionary<string, bool>> conversions = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var boolConversions = new Dictionary<string, bool>
            {
                {"", false },
                {"0", false},
                {"1", true},
                {"true", true },
                {"false", false }
            };

            conversions?.Invoke(boolConversions);

            if (boolConversions.TryGetValue(value.ToLowerInvariant(), out bool result))
            {
                return result;
            }

            return false;
        }

        public static bool? ToNullableBool(this string value, Action<Dictionary<string, bool?>> conversions = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var boolConversions = new Dictionary<string, bool?>
            {
                {"", null },
                {"0", false},
                {"1", true},
                {"true", true },
                {"false", false }
            };

            conversions?.Invoke(boolConversions);

            if (boolConversions.TryGetValue(value.ToLowerInvariant(), out bool? result))
            {
                return result;
            }

            return null;
        }
    }

    public static class PaginationOptionsExtensions
    {

        public static Uri GetPageUri<TParameters>(this IPaginationOptions<TParameters> pagination, int page, Uri baseUri) where TParameters : class, IPagingParameters, ISortingParameters, new()
        {
            var parameters = pagination.ForPage(page);
            if (parameters != null)
            {
                var ub = new UriBuilder(baseUri) { Query = QueryStringParser<TParameters>.ToQueryString(parameters) };
                return ub.Uri;
            }
            return baseUri;

        }

        public static Uri GetNextPageUri<TParameters>(this IPaginationOptions<TParameters> pagination, Uri baseUri) where TParameters : class, IPagingParameters, ISortingParameters, new()
        {
            var nextPage = pagination.NextPage();
            if (nextPage != null)
            {
                var ub = new UriBuilder(baseUri) {Query = QueryStringParser<TParameters>.ToQueryString(nextPage)};
                return ub.Uri;
            }
            return baseUri;

        }

        public static Uri GetPreviousPageUri<TParameters>(this IPaginationOptions<TParameters> pagination, Uri baseUri) where TParameters : class, IPagingParameters, ISortingParameters, new()
        {
            var previousPage = pagination.PreviousPage();
            if (previousPage != null)
            {
                var ub = new UriBuilder(baseUri)
                {
                    Query = QueryStringParser<TParameters>.ToQueryString(previousPage)
                };
                return ub.Uri;
            }
            return baseUri;
        }
    }
}