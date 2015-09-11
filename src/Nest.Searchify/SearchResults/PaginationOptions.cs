using System;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
	public class PaginationOptions<TParameters> : IPaginationOptions<TParameters> where TParameters : ICommonParameters
	{
		private readonly TParameters _parameters;

		public PaginationOptions(TParameters parameters, long total)
		{
			Total = total;
			_parameters = parameters;
		}

        [JsonProperty("has_previous")]
		public bool HasPreviousPage => _parameters.Page > 1;

	    [JsonProperty("has_next")]
        public bool HasNextPage => _parameters.Page < Pages;

	    [JsonProperty(CommonParameters.SizeParameter)]
		public int PageSize => _parameters.Size;

	    [JsonProperty(CommonParameters.PageParameter)]
		public int Page => _parameters.Page.GetValueOrDefault(1);

	    [JsonProperty("total")]
		public long Total { get; private set; }

		[JsonProperty("pages")]
		public long Pages
		{
			get
			{
				long pages = 0;
				if (Total > 0 && PageSize > 0)
				{
					pages = (long)Math.Ceiling((double)Total / (double)PageSize);
				}
				return pages == 0 ? 1 : pages;
			}
		}

	    public TParameters NextPage()
	    {
	        if (!HasNextPage) return _parameters;
	        var p = (TParameters)_parameters.Clone();
	        p.Page += 1;
	        return p;
	    }

	    public TParameters PreviousPage()
	    {
	        if (!HasPreviousPage) return _parameters;
	        var p = (TParameters)_parameters.Clone();
	        p.Page -= 1;
	        return p;
        }
	}
}