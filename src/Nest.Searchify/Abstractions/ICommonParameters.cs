using System;
using System.Collections.Specialized;
using Elasticsearch.Net;

namespace Nest.Searchify.Abstractions
{
	public interface ICommonParameters : ICloneable
	{
		int Start();
		int Size { get; set; }
		string SortBy { get; set; }
		SortDirectionOption? SortDirection { get; set; }
		int? Page { get; set; }
		bool HasSort();

	    string ToJson();

	    NameValueCollection ToQueryString();
	}
}