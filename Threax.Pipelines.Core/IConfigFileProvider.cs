using Newtonsoft.Json.Linq;

namespace Threax.Pipelines.Core
{
    public interface IConfigFileProvider
    {
        string GetConfigPath();

        string GetConfigText();

        JObject GetConfigJObject();

        T GetConfig<T>();
    }
}