using System;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
	public class PaginationOptions<TParameters> : IPaginationOptions where TParameters : ICommonParameters
	{
		private readonly TParameters _parameters;

		public PaginationOptions(TParameters parameters, long total)
		{
			Total = total;
			_parameters = parameters;
		}

        [JsonProperty("has_previous")]
		public bool HasPreviousPage
		{
		    get { return _parameters.Page > 1; }
		}

        [JsonProperty("has_next")]
        public bool HasNextPage
		{
		    get { return _parameters.Page < Pages; }
		}

		[JsonProperty(CommonParameters.SizeParameter)]
		public int PageSize { get { return _parameters.Size; } }

		[JsonProperty(CommonParameters.PageParameter)]
		public int Page { get { return _parameters.Page.GetValueOrDefault(1); } }

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

	}
}