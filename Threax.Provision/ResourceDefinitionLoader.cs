using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Threax.Provision
{
    public class ResourceDefinitionLoader : IResourceDefinitionLoader
    {
        private readonly IProvisionJsonSerializerProvider provisionJsonSerializerProvider;

        public ResourceDefinitionLoader(IProvisionJsonSerializerProvider provisionJsonSerializerProvider)
        {
            this.provisionJsonSerializerProvider = provisionJsonSerializerProvider;
        }

        public ResourceDefinition LoadFromFile(String filePath)
        {
            var serializer = provisionJsonSerializerProvider.GetSerializer();
            using var jsonReader = new JsonTextReader(new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)));
            var resources = serializer.Deserialize<ResourceDefinition>(jsonReader);
            return resources;
        }
    }
}
