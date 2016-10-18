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
    public class GeoBoundingBox : IEquatable<GeoBoundingBox>
    {
        private static readonly Regex Pattern = new Regex(@"^\[(?<topLeft>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]\s?\[(?<bottomRight>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
        [JsonProperty("topLeft")]
        public GeoPoint TopLeft { get; set; }
        [JsonProperty("bottomRight")]
        public GeoPoint BottomRight { get; set; }

        public GeoBoundingBox()
        {
            
        }

        public GeoBoundingBox(GeoPoint topLeft, GeoPoint bottomRight) : this()
        {
            BottomRight = bottomRight;
            TopLeft = topLeft;
        }

        public static implicit operator GeoBoundingBox(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var match = Pattern.Match(value);
                if (!match.Success) throw new FormatException("Unable to create GeoBoundingBox from input string, string must in in the format '[topLeftLat,topleftLon][bottomRightLat,bottomRightLon]'");
                GeoPoint topLeft = match.Groups["topLeft"].Value;
                GeoPoint bottomRight = match.Groups["bottomRight"].Value;

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
    }
}