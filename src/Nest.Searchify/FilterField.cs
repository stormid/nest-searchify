using System;
using Nest.Searchify.Extensions;

namespace Nest.Searchify
{
	public class FilterField
	{
		public const string DefaultDelimiter = "||";

		public static FilterField Empty(string text = "N/A", string delimiter = DefaultDelimiter)
		{
			if (string.IsNullOrWhiteSpace(text)) text = "N/A";
			return Create(text, text.ToUrl(), delimiter);
		}

		public static FilterField Create(Enum value, string delimiter = DefaultDelimiter)
		{
			return Create(value.ToString("G"), value.ToString("D"), delimiter);
		}
        public static FilterField CreateWithCustomDelimiter(string text, string delimiter)
        {
            return CreateWithCustomDelimiter(text, text.ToUrl(), delimiter);
        }

        public static FilterField CreateWithCustomDelimiter(string text, string value, string delimiter)
        {
            return Create(text, value, delimiter);
        }

        public static FilterField Create(string text)
		{
			return Create(text, text.ToUrl());
		}

		public static FilterField Create(string text, string value, string delimiter = DefaultDelimiter)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return Empty(delimiter: delimiter);
			}

			if (string.IsNullOrEmpty(text))
			{
				text = value;
			}

			return new FilterField(delimiter)
			{
				Text = text, Value = value
			};
		}

	    public static implicit operator FilterField(string value)
	    {
	        return Create(value);
	    }

		public FilterField(string delimiter = DefaultDelimiter)
		{
			Delimiter = delimiter;
		}

		[ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
		public string Key => $"{Value}{Delimiter}{Text}";

	    public string Text { get; set; }

		[ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
		public string Value { get; set; }

		[ElasticProperty(Index = FieldIndexOption.NotAnalyzed)]
		public string Delimiter { get; set; }
	}
}