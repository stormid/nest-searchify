namespace Nest.Searchify.Abstractions
{
	public interface ISearchService<in TSearchParameters, out TSearchResult>
		where TSearchParameters : ICommonParameters
		where TSearchResult : ISearchResult<TSearchParameters>
	{
		TSearchResult Search(TSearchParameters parameters);
	}
}
