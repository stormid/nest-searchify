using System;
using Nest.Searchify.Queries.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify
{
    public class GeoBoundingBoxJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoBoundingBox bbox = value as GeoBoundingBox ?? value as string;
            if (bbox != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("topLeft");
                serializer.Serialize(writer, bbox.TopLeft);
                writer.WritePropertyName("bottomRight");
                serializer.Serialize(writer, bbox.BottomRight);
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);
                var topLeft = o.GetValue("topLeft")?.ToObject<GeoLocation>();
                var bottomRight = o.GetValue("bottomRight")?.ToObject<GeoLocation>();
                var bbox = new GeoBoundingBox(topLeft, bottomRight);
                if (objectType == typeof(string)) return bbox.ToString();
                return bbox;
            }
            if (reader.TokenType == JsonToken.String)
            {
                GeoBoundingBox bbox = (string) reader.Value;
                return bbox;
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(GeoBoundingBox);
        }
    }
}