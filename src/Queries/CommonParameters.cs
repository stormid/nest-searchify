using System.Collections.Specialized;
using System.Linq;
using Nest.Searchify.Abstractions;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries
{
	public class CommonParameters : ICommonParameters
	{
		public static TParameters ParametersFromNameValueCollection<TParameters>(NameValueCollection nvc) where TParameters : CommonParameters
		{
			var values = nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
			var jsonString = JsonConvert.SerializeObject(values);
			return JsonConvert.DeserializeObject<TParameters>(jsonString);
		}

		public static string ToJson<TParameters>(TParameters parameters) where TParameters : CommonParameters
		{
			return JsonConvert.SerializeObject(parameters);
		}

		public void PopulateFrom(NameValueCollection nvc)
		{
			var values = nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
			var jsonString = JsonConvert.SerializeObject(values);
			JsonConvert.PopulateObject(jsonString, this);
		}

		public const int DefaultPageSize = 10;

		public const string StartParameter = "start";
		public const string SizeParameter = "size";
		public const string PageParameter = "page";
		public const string SortByParameter = "sortby";
		public const string SortDirectionParameter = "sortdir";

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

		[JsonProperty(SizeParameter, NullValueHandling = NullValueHandling.Ignore)]
		public int Size { get; set; }

		[JsonProperty(SortByParameter, NullValueHandling = NullValueHandling.Ignore)]
		public string SortBy { get; set; }

		[JsonProperty(SortDirectionParameter, NullValueHandling = NullValueHandling.Ignore)]
		public SortDirectionOption? SortDirection { get; set; }

		[JsonProperty(PageParameter, NullValueHandling = NullValueHandling.Ignore)]
		public int? Page { get; set; }

		public bool HasSort()
		{
			return !string.IsNullOrWhiteSpace(SortBy);
		}

		public CommonParameters() : this(DefaultPageSize, 1) { }

		public CommonParameters(int size, int page)
		{
			Size = size <= 0 ? DefaultPageSize : size;
			Page = page;
		}		
	}
}