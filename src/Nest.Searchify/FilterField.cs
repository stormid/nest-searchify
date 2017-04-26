using System;
using System.Linq;
using Nest.Searchify.Abstractions;
using Nest.Searchify.Extensions;

namespace Nest.Searchify
{
	public class FilterField : IEquatable<FilterField>
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

        /// <summary>
        /// Parses a <paramref name="key">key</paramref> in the form '{Value}{Delimiter}{Text}' into a <see cref="FilterField"/>
        /// </summary>
        /// <param name="key">key value e.g. '{Value}{Delimiter}{Text}'</param>
        /// <param name="delimiter">the expected delimiter between values</param>
        /// <returns>new <see cref="FilterField"/></returns>
        /// <exception cref="ArgumentNullException">if <paramref name="key"/> is null, empty or whitespace</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="key"/> does not parse into 2 elements with given delimiter</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="delimiter"/> is null, empty or whitespace</exception>
        public static FilterField Parse(string key, string delimiter = DefaultDelimiter)
        {
            if (string.IsNullOrWhiteSpace(delimiter))
            {
                throw new ArgumentNullException(nameof(delimiter), "A delimiter value is required");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var elements = key.Split(new[] {delimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (elements.Count != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(key), $"expected 2 elements (value and text), found {elements.Count}");
            }

            return Create(elements.ElementAt(1), elements.ElementAt(0), delimiter);
        }

        public static bool TryParse(string key, out FilterField value, string delimiter = DefaultDelimiter)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(delimiter))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var elements = key.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (elements.Count != 2)
            {
                return false;
            }

            value = Create(elements.ElementAt(1), elements.ElementAt(0), delimiter);

            return true;
        }

        public static implicit operator string(FilterField value)
        {
            return value.Key;
        }

        public static implicit operator FilterField(string value)
	    {
	        return Create(value);
	    }

		public FilterField(string delimiter = DefaultDelimiter)
		{
			Delimiter = delimiter;
		}

        protected virtual string GetKey()
        {
            return $"{Value}{Delimiter}{Text}";
        }

        [Keyword]
        public string Key => GetKey();

	    public string Text { get; set; }

        [Keyword]
        public string Value { get; set; }

        [Keyword(Ignore = true)]
        public string Delimiter { get; set; }

        /// <inheritdoc />
        public bool Equals(FilterField other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FilterField) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }
    }
}