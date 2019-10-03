using System;
using System.Reflection;
using Nest.Searchify.Queries.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nest.Searchify.Converters
{
    public class GeoBoundingBoxJsonConverter : JsonConverter
    {
        public static readonly string TopLeftPropertyName = typeof(GeoBoundingBox).GetProperty(nameof(GeoBoundingBox.TopLeft)).GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? nameof(GeoBoundingBox.TopLeft);
        public static readonly string BottomRightPropertyName = typeof(GeoBoundingBox).GetProperty(nameof(GeoBoundingBox.BottomRight)).GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? nameof(GeoBoundingBox.BottomRight);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            GeoBoundingBox bbox = value as GeoBoundingBox ?? value as string;
            if (bbox != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(TopLeftPropertyName);
                serializer.Serialize(writer, bbox.TopLeft);

                writer.WritePropertyName(BottomRightPropertyName);
                serializer.Serialize(writer, bbox.BottomRight);
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);

                var topLeft = o.GetValue(TopLeftPropertyName)?.ToObject<GeoLocation>(serializer);
                var bottomRight = o.GetValue(BottomRightPropertyName)?.ToObject<GeoLocation>(serializer);
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

    public class BoundingBoxJsonConverter : JsonConverter
    {
        public static readonly string TopLeftPropertyName = typeof(BoundingBox).GetProperty(nameof(BoundingBox.TopLeft)).GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? nameof(BoundingBox.TopLeft);
        public static readonly string BottomRightPropertyName = typeof(BoundingBox).GetProperty(nameof(BoundingBox.BottomRight)).GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? nameof(BoundingBox.BottomRight);
        public static readonly string WKTPropertyName = typeof(BoundingBox).GetProperty(nameof(BoundingBox.WellKnownText)).GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? nameof(BoundingBox.WellKnownText);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bbox = value as IBoundingBox;
            if (bbox != null)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(TopLeftPropertyName);
                serializer.Serialize(writer, bbox.TopLeft);

                writer.WritePropertyName(BottomRightPropertyName);
                serializer.Serialize(writer, bbox.BottomRight);
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var o = JObject.Load(reader);

                var topLeft = o.GetValue(TopLeftPropertyName)?.ToObject<GeoLocation>(serializer);
                var bottomRight = o.GetValue(BottomRightPropertyName)?.ToObject<GeoLocation>(serializer);
                var wkt = o.GetValue(WKTPropertyName)?.Value<string>(serializer);
                var bbox = new BoundingBox()
                {
                    TopLeft = topLeft,
                    BottomRight = bottomRight,
                    WellKnownText = wkt
                };

                return bbox;
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(BoundingBox);
        }
    }
}