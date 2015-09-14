using Nest.Queryify.Abstractions.Queries;

namespace Nest.Searchify.Abstractions
{
    public interface ISearchResultQuery<out TSearchParameters>
        where TSearchParameters : class, ICommonParameters
    {
        TSearchParameters Parameters { get; }
    }
}