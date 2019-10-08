using Elasticsearch.Net;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace Nest.Searchify
{
    public class TypeAndConnectionSettingsAwareJsonNetSerializer : ConnectionSettingsAwareSerializerBase
    {
        public static ConnectionSettings.SourceSerializerFactory Default => (builtIn, settings) => new TypeAndConnectionSettingsAwareJsonNetSerializer(builtIn, settings);

        public TypeAndConnectionSettingsAwareJsonNetSerializer(IElasticsearchSerializer builtinSerializer, IConnectionSettingsValues connectionSettings)
            : base(builtinSerializer, connectionSettings) { }

        protected override JsonSerializerSettings CreateJsonSerializerSettings() =>
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            };

        protected override ConnectionSettingsAwareContractResolver CreateContractResolver() => new TypeAndConnectionSettingsAwareContractResolver(ConnectionSettings);
    }
}
