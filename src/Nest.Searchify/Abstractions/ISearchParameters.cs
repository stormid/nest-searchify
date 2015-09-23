namespace Nest.Searchify.Abstractions
{
	public interface ISearchParameters : IPagingParameters, ISortingParameters
    {
		string Query { get; set; }
	}
}