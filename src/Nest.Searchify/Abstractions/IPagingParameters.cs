using System;

namespace Nest.Searchify.Abstractions
{
    public interface IPagingParameters : ICloneable
    {
        int Start();
        int Size { get; set; }
        int? Page { get; set; }
        bool HasSort();
    }
}