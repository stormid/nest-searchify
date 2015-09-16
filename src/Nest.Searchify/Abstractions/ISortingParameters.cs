using System;

namespace Nest.Searchify.Abstractions
{
    public interface ISortingParameters : ICloneable
    {
        string SortBy { get; set; }
        SortDirectionOption? SortDirection { get; set; }
    }
}