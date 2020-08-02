using System.Threading.Tasks;

namespace Threax.Provision.CheapAzure.Services
{
    public interface IMachineIpManager
    {
        Task<string> GetExternalIp();
    }
}