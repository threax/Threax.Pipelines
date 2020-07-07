using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultManager
    {
        Task CreateVault(string keyVaultName, string resourceGroupName, string location);
        Task<string> GetSecret(string keyVaultName, string name);
        Task LockSecrets(string keyVaultName, Guid userId);
        Task RemoveVault(string keyVaultName, string location);
        Task SetSecret(string keyVaultName, string name, string value);
        Task UnlockSecrets(string keyVaultName, Guid userId);
    }
}