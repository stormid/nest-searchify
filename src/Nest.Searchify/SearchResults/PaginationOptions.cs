using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Queries;

namespace Nest.Searchify.SearchResults
{
	public class PaginationOptions<TParameters> : IPaginationOptions<TParameters> where TParameters : class, IPagingParameters, ISortingParameters, new()
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

	    public TParameters FirstPage()
	    {
            var p = (TParameters)_parameters.Clone();
            p.Page = 1;
            return p;
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

	    public TParameters LastPage()
	    {
            var p = (TParameters)_parameters.Clone();
            p.Page = (int)Pages;
            return p;
        }

        public IEnumerable<Tuple<int, NameValueCollection>> PagingGroup(int range = 5)
        {
            var fromPage = (Page - range) <= 0 ? 1 : Page - range;
            var toPage = (Page + range) > Pages ? Pages : Page + range;

            for (var page = fromPage; page <= toPage; page++)
            {
                var nvc = QueryStringParser<TParameters>.Parse(ForPage(page));
                yield return new Tuple<int, NameValueCollection>(page, nvc);
            }
        }
    }
}