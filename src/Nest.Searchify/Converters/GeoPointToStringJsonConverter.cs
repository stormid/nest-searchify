using System;
using Newtonsoft.Json;

namespace Nest.Searchify.Converters
{
    public class GeoPointToStringJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = reader.Value;
            if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<GeoPoint>(reader)?.ToString();
            }
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}