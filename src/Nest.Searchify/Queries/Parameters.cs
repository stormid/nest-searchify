using System.ComponentModel;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries
{
	public class Parameters : IPagingParameters, ISortingParameters
	{
	    public const string SizeParameter = "size";
        public const string PageParameter = "page";
        public const string SortByParameter = "sortby";
        public const string SortDirectionParameter = "sortdir";

        public const int DefaultPage = 1;
		public const int DefaultPageSize = 10;

		public int Start()
		{
			var start = (Page.GetValueOrDefault(1) - 1)*Size;
			if (start < 0)
			{
				Page = 1;
				Size = DefaultPageSize;
				start = 0;
			}
			return start;
		}

        [DefaultValue(DefaultPageSize)]
        [JsonProperty(SizeParameter)]
        public int Size { get; set; }

        [JsonProperty(SortByParameter)]
        public string SortBy { get; set; }

        [JsonProperty(SortDirectionParameter)]
        public SortDirectionOption? SortDirection { get; set; }

	    private int? _page;
        [DefaultValue(DefaultPage)]
	    [JsonProperty(PageParameter)]
	    public int? Page
	    {
	        get => _page;
            set => SetPage(value.GetValueOrDefault(DefaultPage));
        }

	    public bool HasSort()
		{
			return !string.IsNullOrWhiteSpace(SortBy);
		}
        

	    public Parameters() : this(DefaultPageSize, DefaultPage) { }

		public Parameters(int size, int page)
		{
			Size = size <= 0 ? DefaultPageSize : size;
			SetPage(page);
		}

	    private void SetPage(int page)
	    {
	        _page = page <= 0 ? DefaultPage : page;
	    }
	}
}