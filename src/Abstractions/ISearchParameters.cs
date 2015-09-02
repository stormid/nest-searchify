namespace Nest.Searchify.Abstractions
{
	public interface ISearchParameters : ICommonParameters
	{
		string Query { get; set; }
	}
}