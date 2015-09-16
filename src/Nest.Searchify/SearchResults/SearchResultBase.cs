using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
    public abstract class SearchResultBase<TParameters> : ISearchResultBase<TParameters> where TParameters : class, IPagingParameters, ISortingParameters
	{
		protected SearchResultBase(TParameters parameters)
		{
			Parameters = parameters;			
		}

		private PaginationOptions<TParameters> _pagination;
		[JsonProperty("pagination")]
		public IPaginationOptions<TParameters> Pagination => _pagination ?? (_pagination = new PaginationOptions<TParameters>(Parameters, GetSearchResultTotal()));

        protected abstract int GetResponseTimeTaken();
		protected abstract long GetSearchResultTotal();

		[JsonProperty("parameters")]
		public TParameters Parameters { get; }

		[JsonProperty("timeTaken")]
		public int TimeTaken => GetResponseTimeTaken();
	}
}