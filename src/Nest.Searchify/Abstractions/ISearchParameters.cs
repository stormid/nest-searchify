namespace Nest.Searchify.Abstractions
{
	public interface ISearchParameters : IParameters
	{
		string Query { get; set; }
	}
}