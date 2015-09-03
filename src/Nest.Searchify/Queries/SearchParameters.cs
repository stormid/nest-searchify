using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries
{
	public class SearchParameters : CommonParameters, ISearchParameters
	{
        public const string QueryParameter = "q";

		[JsonProperty(QueryParameter, NullValueHandling = NullValueHandling.Ignore)]
        public string Query { get; set; }

		public SearchParameters() : this(DefaultPageSize, 1) { }

		public SearchParameters(int size, int page) : base(size, page)
        {
        }
    }
}