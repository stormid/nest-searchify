using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify
{
    public sealed class GeoLocationJsonConverter : JsonConverter
    {
        // public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var point = value as GeoLocation ?? value as string;

            if (point != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("lat");
                writer.WriteValue(point.Latitude);
                writer.WritePropertyName("lon");
                writer.WriteValue(point.Longitude);
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var geoLocationJObject = JObject.Load(reader);
                var lat = geoLocationJObject.Value<double>("lat");
                var lon = geoLocationJObject.Value<double>("lon");
                var point = GeoLocation.TryCreate(lat, lon);
                if (objectType == typeof(string)) return point.ToString();
                return point;
            }
            if (reader.TokenType == JsonToken.String)
            {
                GeoLocation point = (string)reader.Value;
                return point;
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeoLocation) || objectType == typeof(string);
        }
    }
}