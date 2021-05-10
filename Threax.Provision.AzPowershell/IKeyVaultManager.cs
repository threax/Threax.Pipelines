using System;
using System.Security;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultManager
    {
        Task<bool> Exists(string VaultName);
        Task<String> GetSecret(string keyVaultName, string name);
        Task LockSecrets(string keyVaultName, Guid userId);
        Task SetSecret(string keyVaultName, string name, string value);
        Task UnlockSecrets(string keyVaultName, Guid userId);
        Task UnlockSecretsRead(string keyVaultName, Guid userId);
    }
}