using System;
using System.Security;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultManager
    {
        Task<bool> Exists(string VaultName);
        Task<VaultCertificate> GetCertificate(string VaultName, string Name);
        Task<String> GetSecret(string keyVaultName, string name);
        Task ImportCertificate(string VaultName, string Name, byte[] cert, string Password);
        Task ImportCertificate(string VaultName, string Name, string FilePath, string Password);
        Task LockSecrets(string keyVaultName, Guid userId);
        Task SetSecret(string keyVaultName, string name, string value);
        Task UnlockSecrets(string keyVaultName, Guid userId);
        Task UnlockSecretsRead(string keyVaultName, Guid userId);
    }
}