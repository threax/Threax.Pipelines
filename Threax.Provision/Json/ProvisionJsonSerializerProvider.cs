using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threax.Provision
{
    public class ProvisionJsonSerializerProvider : IProvisionJsonSerializerProvider
    {
        private readonly ProvisionConfig config;
        private JsonSerializer serializer = null;

        public ProvisionJsonSerializerProvider(ProvisionConfig config)
        {
            this.config = config;
        }

        public JsonSerializer GetSerializer()
        {
            if (serializer == null)
            {
                var serializationBinder = new ResourceSerializationBinder();
                foreach(var item in config.TypeMap)
                {
                    serializationBinder.RegisterType(item.Key, item.Value);
                }
                
                var settings = new JsonSerializerSettings();
                settings.SerializationBinder = serializationBinder;
                settings.TypeNameHandling = TypeNameHandling.Auto;
                serializer = JsonSerializer.Create(settings);
            }

            return serializer;
        }
    }
}
