using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nest.Searchify.Queries.Models
{
    public class GeoBoundingBox
    {
        private static readonly Regex Pattern = new Regex(@"^\[(?<topLeft>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]\s?\[(?<bottomRight>(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?))\]$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);
        public GeoPoint TopLeft { get; set; }
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
                if (!match.Success) throw new FormatException("Unable to create GeoBoundingBox from input string, string must in in the format '[topLeftLat,topleftLon],[bottomRightLat,bottomRightLon]'");
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

        public override string ToString()
        {
            return $"[{TopLeft}],[{BottomRight}]";
        }
    }
}