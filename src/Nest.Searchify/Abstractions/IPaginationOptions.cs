namespace Nest.Searchify.Abstractions
{
	public interface IPaginationOptions
	{
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
		int PageSize { get; }
		int Page { get; }
		long Total { get; }
		long Pages { get; }
	}
}