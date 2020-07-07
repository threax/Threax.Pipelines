using Newtonsoft.Json;

namespace Threax.Provision
{
    public interface IProvisionJsonSerializerProvider
    {
        JsonSerializer GetSerializer();
    }
}