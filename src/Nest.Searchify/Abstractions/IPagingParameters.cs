using System;

namespace Nest.Searchify.Abstractions
{
    public interface IPagingParameters
    {
        int Start();
        int Size { get; set; }
        int? Page { get; set; }
        bool HasSort();
    }
}