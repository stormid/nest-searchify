using System;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;
using Newtonsoft.Json;

namespace Nest.Searchify.SearchResults
{
	public class PaginationOptions<TParameters> : IPaginationOptions<TParameters> where TParameters : class, IPagingParameters, ISortingParameters
	{
		private readonly TParameters _parameters;

		public PaginationOptions(TParameters parameters, long total)
		{
			Total = total;
			_parameters = parameters;
		}

	    public bool HasPage(int page)
	    {
	        return page <= Pages;
	    }

		public bool HasPreviousPage => _parameters.Page > 1;

        public bool HasNextPage => _parameters.Page < Pages;

		public int PageSize => _parameters.Size;

		public int Page => _parameters.Page.GetValueOrDefault(1);

		public long Total { get; }

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

	    public long From => _parameters.Start() + 1;

	    public long To
	    {
	        get
	        {
	            var to = _parameters.Start() + PageSize;
	            return to > Total ? Total : to;
	        }
	    }

	    public TParameters NextPage()
	    {
	        if (!HasNextPage) return null;
	        var p = (TParameters)_parameters.Clone();
	        p.Page += 1;
	        return p;
	    }

	    public TParameters PreviousPage()
	    {
	        if (!HasPreviousPage) return null;
	        var p = (TParameters)_parameters.Clone();
	        p.Page -= 1;
	        return p;
        }

        public TParameters ForPage(int page)
        {
            if (!HasPage(page)) return null;
            var p = (TParameters)_parameters.Clone();
            p.Page = page;
            return p;
        }
    }
}