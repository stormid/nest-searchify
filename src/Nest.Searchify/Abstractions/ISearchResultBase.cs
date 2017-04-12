namespace Nest.Searchify.Abstractions
{
	public interface ISearchResultBase<out TParameters> 
        where TParameters : IPagingParameters, ISortingParameters
	{
		IPaginationOptions<TParameters> Pagination { get; }
		TParameters Parameters { get; }
		long TimeTaken { get; }

	}
}