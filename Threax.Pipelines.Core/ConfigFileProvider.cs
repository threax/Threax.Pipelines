using Newtonsoft.Json;
using System;
using System.IO;

namespace Threax.Pipelines.Core
{
    class ConfigFileProvider : IConfigFileProvider
    {
        private readonly String path;

        public ConfigFileProvider(String path)
        {
            this.path = path;
        }

        public String GetConfigText()
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public T GetConfig<T>()
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream))
            using(var jsonReader = new JsonTextReader(reader))
            {
                return JsonSerializer.CreateDefault().Deserialize<T>(jsonReader);
            }
        }
    }
}
