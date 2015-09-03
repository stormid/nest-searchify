namespace Nest.Searchify.Abstractions
{
	public interface ISearchResult<out TSearchParameters> where TSearchParameters : ICommonParameters
	{
		IPaginationOptions Pagination { get; }
		TSearchParameters Parameters { get; }
		int TimeTaken { get; }

	}
}