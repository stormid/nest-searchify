namespace Nest.Searchify.Abstractions
{
	public interface IFilterField
	{
		string Key { get; }
		string Text { get; set; }
		string Value { get; set; }
		string Delimiter { get; set; }
	}
}