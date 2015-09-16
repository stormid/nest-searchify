using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Nest.Searchify.Queries
{
	public partial class Parameters : IParameters
	{
		public static TParameters ParametersFromNameValueCollection<TParameters>(NameValueCollection nvc) where TParameters : class, IParameters
		{
			var values = nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
			var jsonString = JsonConvert.SerializeObject(values);
			return JsonConvert.DeserializeObject<TParameters>(jsonString);
		}

		public static string ToJson<TParameters>(TParameters parameters) where TParameters : Parameters
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

		public int Size { get; set; }

		public string SortBy { get; set; }

		public SortDirectionOption? SortDirection { get; set; }

		public int? Page { get; set; }

		public bool HasSort()
		{
			return !string.IsNullOrWhiteSpace(SortBy);
		}

	    public string ToJson()
	    {
	        return ToJson(this);
	    }

	    public static TParameters FromQueryString<TParameters>(string queryString) where TParameters : class, IParameters
	    {
	        var nvc = HttpUtility.ParseQueryString(queryString);
	        return ParametersFromNameValueCollection<TParameters>(nvc);
	    }

	    public NameValueCollection ToQueryString()
	    {
	        var nvc = HttpUtility.ParseQueryString("");
	        foreach (var item in this.AsDictionary().Where(item => item.Value != null).OrderBy(o => o.Key))
	        {
	            var key = item.Key.Camelize();

	            if (item.Value is IEnumerable)
	            {
	                foreach (var val in item.Value as IEnumerable)
	                {
                        nvc.Add(key, val.ToString());
                    }
	            }
	            else
	            {
	                nvc.Add(key, item.Value.ToString());
	            }
	        }
	        return nvc;
	    }

	    public Parameters() : this(DefaultPageSize, 1) { }

		public Parameters(int size, int page)
		{
			Size = size <= 0 ? DefaultPageSize : size;
			Page = page;
		}

	    object ICloneable.Clone()
	    {
	        return MemberwiseClone();
	    }
	}

    public partial class Parameters : IParameters
    {
    }
}