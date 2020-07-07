using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IAcrManager
    {
        Task Create(string name, string resourceGroupName, string location, string sku);

        Task<bool> IsNameAvailable(String name);

        Task GetAcr(String name, String resourceGroupName);

        Task<AcrCredential> GetAcrCredential(String Name, String ResourceGroupName);
    }
}