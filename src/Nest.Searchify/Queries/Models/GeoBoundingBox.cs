using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Nest.Searchify.Converters;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries.Models
{
    [JsonConverter(typeof(GeoBoundingBoxJsonConverter))]
    [TypeConverter(typeof(GeoBoundingBoxTypeConverter))]
    public class GeoBoundingBox : IEquatable<GeoBoundingBox>, IBoundingBox, IFormattable
    {
        private static readonly Regex Pattern = new Regex(@"^\[(?<topLeft>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]\s?\[(?<bottomRight>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
        
        [JsonProperty("top_left")]
        public GeoLocation TopLeft { get; set; }
        
        [JsonProperty("bottom_right")]
        
        public GeoLocation BottomRight { get; set; }
        
        public string WellKnownText { get; set; }

        public GeoBoundingBox()
        {
            
        }

        
        public GeoBoundingBox(GeoLocation topLeft, GeoLocation bottomRight) : this()
        {
            BottomRight = bottomRight;
            TopLeft = topLeft;
        }

        public BoundingBox ToBoundingBox()
        {
            return new BoundingBox
            {
                BottomRight = BottomRight,
                TopLeft = TopLeft
            };
        }

        public static implicit operator BoundingBox(GeoBoundingBox value)
        {
            return value.ToBoundingBox();
        }

        public static implicit operator GeoBoundingBox(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var match = Pattern.Match(value);
                if (!match.Success) throw new FormatException("Unable to create GeoBoundingBox from input string, string must in in the format '[topLeftLat,topleftLon][bottomRightLat,bottomRightLon]'");
                GeoLocation topLeft = match.Groups["topLeft"].Value;
                GeoLocation bottomRight = match.Groups["bottomRight"].Value;

                return new GeoBoundingBox(topLeft, bottomRight);
            }
            throw new ArgumentNullException(nameof(value), "No value specified");
        }

        public static implicit operator string(GeoBoundingBox value)
        {
            return value?.ToString();
        }

        public bool Equals(GeoBoundingBox other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return TopLeft.Equals(other.TopLeft) && BottomRight.Equals(other.BottomRight);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}][{1}]", TopLeft.ToString(), BottomRight.ToString());
        }

        public string ToString(string format, IFormatProvider formatProvider) => ToString();    }
}