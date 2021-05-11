using System;
using System.IO;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class KeyVaultManager : IKeyVaultManager
    {
        private readonly IShellRunner shellRunner;

        public KeyVaultManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        /// <summary>
        /// Unlock get, list, set and delete for secrets and certificates.
        /// </summary>
        /// <param name="keyVaultName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task UnlockSecrets(String keyVaultName, Guid userId)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddResultCommand($"Set-AzKeyVaultAccessPolicy -ObjectId {userId} -VaultName {keyVaultName} -PermissionsToSecrets set,delete,get,list -PermissionsToCertificates import,delete,get,list");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error unlocking secrets in Key Vault '{keyVaultName}'.");
        }

        /// <summary>
        /// Unlock get and list for secrets and certificates.
        /// </summary>
        /// <param name="keyVaultName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task UnlockSecretsRead(String keyVaultName, Guid userId)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddResultCommand($"Set-AzKeyVaultAccessPolicy -ObjectId {userId} -VaultName {keyVaultName} -PermissionsToSecrets get,list -PermissionsToCertificates get,list");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error unlocking secrets in Key Vault '{keyVaultName}'.");
        }

        public Task LockSecrets(String keyVaultName, Guid userId)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddResultCommand($"Remove-AzKeyVaultAccessPolicy -ObjectId {userId} -VaultName {keyVaultName}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error unlocking secrets in Key Vault '{keyVaultName}'.");
        }

        public Task SetSecret(String keyVaultName, String name, String value)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddCommand($"$secret = ConvertTo-SecureString {value} -AsPlainText -Force");
            pwsh.AddResultCommand($"Set-AzKeyVaultSecret -VaultName {keyVaultName} -Name {name} -SecretValue $secret");

            return shellRunner.RunProcessVoidAsync(pwsh,
               invalidExitCodeMessage: $"Error setting secret '{name}' in Key Vault '{keyVaultName}'.");
        }

        public async Task<String?> GetSecret(String keyVaultName, String name)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddCommand($"$secret = Get-AzKeyVaultSecret -VaultName {keyVaultName} -Name {name}");
            pwsh.AddCommand($"$final = $null");
            pwsh.AddCommand($"if($secret -ne $null) {{ $final = ConvertFrom-SecureString -SecureString $secret.SecretValue -AsPlainText }}");
            pwsh.AddResultCommand($"$final");

            dynamic info = await shellRunner.RunProcessAsync(pwsh,
               invalidExitCodeMessage: $"Error loading '{name}' from Key Vault '{keyVaultName}'.");

            return info;
        }

        public async Task<bool> Exists(String VaultName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddResultCommand($"Get-AzKeyVault -VaultName {VaultName}");

            dynamic info = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error finding key vault '{VaultName}'.");

            return info != null;
        }

        public Task ImportCertificate(String VaultName, String Name, String FilePath, String Password)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddCommand($"$securePass = ConvertTo-SecureString {Password} -AsPlainText -Force");
            pwsh.AddResultCommand($"Import-AzKeyVaultCertificate -VaultName {VaultName} -Name {Name} -FilePath {FilePath} -Password $securePass");

            return shellRunner.RunProcessVoidAsync(pwsh,
               invalidExitCodeMessage: $"Error Importing Certificate '{Name}' to Key Vault '{VaultName}'.");
        }

        public async Task ImportCertificate(String VaultName, String Name, byte[] cert, String Password)
        {
            var outFile = Path.GetTempFileName();
            try
            {
                using (var stream = File.Open(outFile, FileMode.Create))
                {
                    stream.Write(cert);
                }
                await ImportCertificate(VaultName, Name, outFile, Password);
            }
            finally
            {
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }
            }
        }

        public async Task<VaultCertificate?> GetCertificate(String VaultName, String Name)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.KeyVault");
            pwsh.AddResultCommand($"Get-AzKeyVaultCertificate -VaultName {VaultName} -Name {Name}");

            dynamic info = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error getting certificate '{Name}' from Key Vault '{VaultName}'.");

            return info != null ?
                new VaultCertificate(info.KeyId, info.SecretId, info.Thumbprint) : null;
        }
    }
}
