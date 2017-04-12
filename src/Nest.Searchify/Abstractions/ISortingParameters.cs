using System;

namespace Nest.Searchify.Abstractions
{
    public interface ISortingParameters
    {
        string SortBy { get; set; }
        SortDirectionOption? SortDirection { get; set; }
    }
}