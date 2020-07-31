using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IServicePrincipalManager
    {
        Task<bool> Exists(String DisplayName);

        Task Remove(String DisplayName);

        Task RemoveApplication(String DisplayName);

        Task<ServicePrincipal> CreateServicePrincipal(string name, string subscription, string resourceGroup, string role = "Reader");
    }
}