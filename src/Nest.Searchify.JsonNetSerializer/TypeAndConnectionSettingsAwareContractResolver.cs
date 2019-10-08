using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Nest.Searchify
{
    public class TypeAndConnectionSettingsAwareContractResolver : ConnectionSettingsAwareContractResolver
    {
        public TypeAndConnectionSettingsAwareContractResolver(IConnectionSettingsValues connectionSettings)
            : base(connectionSettings) { }

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            if (contract is JsonContainerContract containerContract)
            {
                if (containerContract.ItemTypeNameHandling == null)
                    containerContract.ItemTypeNameHandling = TypeNameHandling.None;
            }

            return contract;
        }
    }
}
