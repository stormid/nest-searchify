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
        [JsonProperty(SizeParameter, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Size { get; set; }

        [JsonProperty(SortByParameter, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string SortBy { get; set; }

        [JsonProperty(SortDirectionParameter, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SortDirectionOption? SortDirection { get; set; }

	    private int? page;
        [DefaultValue(DefaultPage)]
	    [JsonProperty(PageParameter, DefaultValueHandling = DefaultValueHandling.Ignore)]
	    public int? Page
	    {
	        get => page;
            set => SetPage(value.GetValueOrDefault(DefaultPage));
        }

	    public virtual bool HasSort()
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
	        this.page = page <= 0 ? DefaultPage : page;
	    }
	}
}