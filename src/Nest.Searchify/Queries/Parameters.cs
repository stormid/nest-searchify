using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Web;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Nest.Searchify.Queries
{
	public class Parameters : IPagingParameters, ISortingParameters
    {
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
        public int Size { get; set; }

		public string SortBy { get; set; }

		public SortDirectionOption? SortDirection { get; set; }

        [DefaultValue(DefaultPage)]
        public int? Page { get; set; }

		public bool HasSort()
		{
			return !string.IsNullOrWhiteSpace(SortBy);
		}
        

	    public Parameters() : this(DefaultPageSize, DefaultPage) { }

		public Parameters(int size, int page)
		{
			Size = size <= 0 ? DefaultPageSize : size;
			Page = page <= 0 ? DefaultPage : page;
		}

	    public object Clone()
	    {
	        return MemberwiseClone();
	    }
	}
}