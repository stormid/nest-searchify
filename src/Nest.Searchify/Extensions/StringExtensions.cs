using System.Net;

namespace Nest.Searchify.Extensions
{
    public static class StringExtensions
    {
        public static string ToUrl(this string input)
        {
            return WebUtility.UrlEncode(input.Replace(" ", "-").ToLowerInvariant());
        }
    }
}