using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify
{
	public abstract class SearchResultBase<TParameters> : ISearchResult<TParameters> where TParameters : ICommonParameters
	{
		protected SearchResultBase(TParameters parameters)
		{
			Parameters = parameters;			
		}

		private PaginationOptions<TParameters> _pagination;
		[JsonProperty("pagination")]
		public IPaginationOptions Pagination {
			get
			{
				if (_pagination == null)
				{
					_pagination = new PaginationOptions<TParameters>(Parameters, GetSearchResultTotal());
				}
				return _pagination;
			} 
		}

		protected abstract int GetResponseTimeTaken();
		protected abstract long GetSearchResultTotal();

		[JsonProperty("parameters")]
		public TParameters Parameters { get; private set; }

		[JsonProperty("timeTaken")]
		public int TimeTaken
		{
			get { return GetResponseTimeTaken(); }
		}

	}
}