using Nest.Searchify.Converters;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Nest.Searchify.Queries
{
    [TypeConverter(typeof(GeoLocationParameterTypeConverter))]
    [JsonConverter(typeof(GeoLocationParameterJsonConverter))]
    [JsonObject]
    public class GeoLocationParameter : GeoLocation
    {
        protected GeoLocationParameter(GeoLocation location) : this(location.Latitude, location.Longitude)
        {
        }

        public GeoLocationParameter(double latitude, double longitude) : base(latitude, longitude)
        {
        }

        public static implicit operator GeoLocationParameter(string value)
        {
            return new GeoLocationParameter(value);
        }

        public static implicit operator string(GeoLocationParameter value)
        {
            return value.ToString();
        }
    }
}