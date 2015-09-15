using System;
using System.Collections.Specialized;
using Elasticsearch.Net;

namespace Nest.Searchify.Abstractions
{
    public interface IParameters : ICloneable, IPagingParameters, ISortingParameters
	{
	    string ToJson();

	    NameValueCollection ToQueryString();
	}
}