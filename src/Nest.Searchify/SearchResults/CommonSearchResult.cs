using Nest.Searchify.Abstractions;

namespace Nest.Searchify.SearchResults
{
    public class CommonSearchResult<TEntity, TOutputEntity> : SearchResult<TEntity, TOutputEntity, ICommonParameters>
        where TEntity : class
        where TOutputEntity : class
    {
        public CommonSearchResult(ICommonParameters parameters, ISearchResponse<TEntity> response)
            : base(parameters, response)
        {
        }
    }

    public class CommonSearchResult<TEntity> : SearchResult<TEntity, TEntity, ICommonParameters>
        where TEntity : class
    {
        public CommonSearchResult(ICommonParameters parameters, ISearchResponse<TEntity> response) : base(parameters, response)
        {
        }
    }
}