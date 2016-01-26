using System;
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
    }

    public static class PaginationOptionsExtensions
    {

        public static Uri GetPageUri<TParameters>(this IPaginationOptions<TParameters> pagination, int page, Uri baseUri) where TParameters : class, IPagingParameters, ISortingParameters, new()
        {
            var parameters = pagination.ForPage(page);
            if (parameters != null)
            {
                var ub = new UriBuilder(baseUri) { Query = QueryStringParser<TParameters>.Parse(parameters).ToString() };
                return ub.Uri;
            }
            return baseUri;

        }

        public static Uri GetNextPageUri<TParameters>(this IPaginationOptions<TParameters> pagination, Uri baseUri) where TParameters : class, IPagingParameters, ISortingParameters, new()
        {
            var nextPage = pagination.NextPage();
            if (nextPage != null)
            {
                var ub = new UriBuilder(baseUri) {Query = QueryStringParser<TParameters>.Parse(nextPage).ToString()};
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
                    Query = QueryStringParser<TParameters>.Parse(previousPage).ToString()
                };
                return ub.Uri;
            }
            return baseUri;
        }
    }
}