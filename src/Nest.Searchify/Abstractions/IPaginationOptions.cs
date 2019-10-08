using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Nest.Searchify.Abstractions
{
    public interface IPaginationOptions<out TParameters> where TParameters : IPagingParameters, ISortingParameters
    {
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        int PageSize { get; }
        int Page { get; }
        long Total { get; }
        long Pages { get; }

        long From { get; }

        long To { get; }

        TParameters FirstPage();
        TParameters NextPage();
        TParameters PreviousPage();
        TParameters ForPage(int page);
        TParameters LastPage();

        /// <summary>
        /// Generates a group of pages around the current page, will always include
        /// </summary>
        /// <param name="range">the range of pages to generate, by default 5 pages either side of the current page will be generated (or the available pages if that is less than the <paramref name="range"/></param>
        /// <returns>Group of page numbers along with a dictionary containing the required querystring for the page</returns>
        IEnumerable<Tuple<int, Dictionary<string, Microsoft.Extensions.Primitives.StringValues>>> PagingGroup(int range = 5);
    }
}