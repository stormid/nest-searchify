namespace Nest.Searchify.Abstractions
{
	public interface ISearchResult<out TSearchParameters> where TSearchParameters : ICommonParameters
	{
		IPaginationOptions<TSearchParameters> Pagination { get; }
		TSearchParameters Parameters { get; }
		int TimeTaken { get; }

	}
}