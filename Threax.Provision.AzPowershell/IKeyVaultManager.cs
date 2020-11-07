using System;
using System.Security;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultManager
    {
        Task CreateVault(string keyVaultName, string resourceGroupName, string location);
        Task<bool> Exists(string VaultName);
        Task<VaultCertificate> GetCertificate(string VaultName, string Name);
        Task<SecureString> GetSecret(string keyVaultName, string name);
        Task ImportCertificate(string VaultName, string Name, byte[] cert, SecureString Password);
        Task ImportCertificate(string VaultName, string Name, string FilePath, SecureString Password);
        Task LockSecrets(string keyVaultName, Guid userId);
        Task RemoveVault(string keyVaultName, string location);
        Task SetSecret(string keyVaultName, string name, string value);
        Task SetSecret(String VaultName, String Name, SecureString SecretValue);
        Task UnlockSecrets(string keyVaultName, Guid userId);
        Task UnlockSecretsRead(string keyVaultName, Guid userId);
    }
}