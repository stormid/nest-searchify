using System;
using Nest.Searchify.Queries.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify.Converters
{
    public class GeoBoundingBoxJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);
                var topLeft = o.GetValue("topLeft")?.ToObject<GeoPoint>();
                var bottomRight = o.GetValue("bottomRight")?.ToObject<GeoPoint>();
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