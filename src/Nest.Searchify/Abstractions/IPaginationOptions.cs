namespace Nest.Searchify.Abstractions
{
	public interface IPaginationOptions<out TParameters> where TParameters : IPagingParameters, ISortingParameters
    {
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
		int PageSize { get; }
		int Page { get; }
		long Total { get; }
		long Pages { get; }

        long From { get; }

        long To { get; }

	    TParameters NextPage();
        TParameters PreviousPage();
    }
}