namespace Nest.Searchify.Abstractions
{
	public interface IPaginationOptions<out TParameters> where TParameters : ICommonParameters
    {
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
		int PageSize { get; }
		int Page { get; }
		long Total { get; }
		long Pages { get; }

	    TParameters NextPage();
        TParameters PreviousPage();
    }
}