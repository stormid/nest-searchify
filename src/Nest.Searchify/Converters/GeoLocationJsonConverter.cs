using System;
using Nest.Searchify.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify.Converters
{
    public sealed class GeoLocationParameterJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var point = value as GeoLocationParameter ?? value as string;

            if (point != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Latitude");
                writer.WriteValue(point.Latitude);
                writer.WritePropertyName("Longitude");
                writer.WriteValue(point.Longitude);
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var geoLocationJObject = JObject.Load(reader);
                var lat = (geoLocationJObject.SelectToken("lat") ?? geoLocationJObject.SelectToken(nameof(GeoLocation.Latitude)))?.Value<double>() ?? 0;
                var lon = (geoLocationJObject.SelectToken("lon") ?? geoLocationJObject.SelectToken(nameof(GeoLocation.Longitude)))?.Value<double>() ?? 0;
                //var lat = geoLocationJObject.Value<double>("lat");
                //var lon = geoLocationJObject.Value<double>("lon");
                var point = GeoLocation.TryCreate(lat, lon);
                if (objectType == typeof(string)) return point.ToString();
                return point;
            }
            if (reader.TokenType == JsonToken.String)
            {
                GeoLocationParameter point = (string)reader.Value;
                return point;
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeoLocationParameter) || objectType == typeof(string);
        }
    }
}