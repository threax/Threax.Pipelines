using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class KeyVaultManager : IKeyVaultManager
    {
        private readonly ILogger<KeyVaultManager> logger;

        public KeyVaultManager(ILogger<KeyVaultManager> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Unlock get, list, set and delete for secrets and certificates.
        /// </summary>
        /// <param name="keyVaultName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task UnlockSecrets(String keyVaultName, Guid userId)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($userId, $keyVaultName)");
                pwsh.AddParameter("userId", userId.ToString());
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddScript("Set-AzKeyVaultAccessPolicy -ObjectId $userId -VaultName $keyVaultName -PermissionsToSecrets set,delete,get,list -PermissionsToCertificates import,delete,get,list");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error unlocking secrets in Key Vault '{keyVaultName}'.");
            }
        }

        /// <summary>
        /// Unlock get and list for secrets and certificates.
        /// </summary>
        /// <param name="keyVaultName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task UnlockSecretsRead(String keyVaultName, Guid userId)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($userId, $keyVaultName)");
                pwsh.AddParameter("userId", userId.ToString());
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddScript("Set-AzKeyVaultAccessPolicy -ObjectId $userId -VaultName $keyVaultName -PermissionsToSecrets get,list -PermissionsToCertificates get,list");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error unlocking secrets in Key Vault '{keyVaultName}'.");
            }
        }

        public async Task LockSecrets(String keyVaultName, Guid userId)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($userId, $keyVaultName)");
                pwsh.AddParameter("userId", userId.ToString());
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddScript("Remove-AzKeyVaultAccessPolicy -ObjectId $userId -VaultName $keyVaultName");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error locking secrets in Key Vault '{keyVaultName}'.");
            }
        }

        public async Task CreateVault(String keyVaultName, String resourceGroupName, String location)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($keyVaultName, $rgName, $loc) New-AzKeyVault -Name $keyVaultName -ResourceGroupName $rgName -Location $loc -DisableSoftDelete");
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddParameter("rgName", resourceGroupName);
                pwsh.AddParameter("loc", location);

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error creating Key Vault '{keyVaultName}'.");
            }
        }

        public async Task RemoveVault(String keyVaultName, String location)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($keyVaultName, $rgName, $loc) Remove-AzKeyVault -Name $keyVaultName -Location $loc -Force");
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddParameter("loc", location);

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error removing Key Vault '{keyVaultName}'.");
            }
        }

        public async Task SetSecret(String keyVaultName, String name, String value)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($keyVaultName, $name, $value)");
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddParameter("name", name);
                pwsh.AddParameter("value", value);
                pwsh.AddScript("$secret = ConvertTo-SecureString $value -AsPlainText -Force");
                pwsh.AddScript("Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $name -SecretValue $secret");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error setting secret '{name}' in Key Vault '{keyVaultName}'.");
            }
        }

        public async Task SetSecret(String VaultName, String Name, SecureString SecretValue)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                var parm = new { VaultName, Name, SecretValue };
                pwsh.AddParamLine(parm);
                pwsh.AddCommandWithParams("Set-AzKeyVaultSecret", parm);

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error setting secret '{Name}' in Key Vault '{VaultName}'.");
            }
        }

        public async Task<SecureString> GetSecret(String keyVaultName, String name)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($keyVaultName, $name)");
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddParameter("name", name);
                pwsh.AddScript("Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $name");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error loading '{name}' from Key Vault '{keyVaultName}'.");
                var info = outputCollection.FirstOrDefault() as dynamic;
                return info?.SecretValue;
            }
        }

        public async Task<bool> Exists(String VaultName)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.KeyVault");
                var parm = new { VaultName };
                pwsh.AddParamLine(parm);
                pwsh.AddCommandWithParams("Get-AzKeyVault", parm);

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error loading '{VaultName}' from Key Vault '{VaultName}'.");
                var info = outputCollection.FirstOrDefault() as dynamic;
                return info != null;
            }
        }

        public async Task ImportCertificate(String VaultName, String Name, String FilePath, SecureString Password)
        {
            var pwshArgs = new { VaultName, Name, FilePath, Password };

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.KeyVault");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("Import-AzKeyVaultCertificate", pwshArgs);

            var outputCollection = await pwsh.RunAsync();
            pwsh.ThrowOnErrors($"Error Importing Certificate '{Name}' to Key Vault '{VaultName}'.");
        }

        public async Task ImportCertificate(String VaultName, String Name, byte[] cert, SecureString Password)
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

        public async Task<VaultCertificate> GetCertificate(String VaultName, String Name)
        {
            var pwshArgs = new { VaultName, Name };

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.KeyVault");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("Get-AzKeyVaultCertificate", pwshArgs);

            var outputCollection = await pwsh.RunAsync();
            pwsh.ThrowOnErrors($"Error getting certificate '{Name}' from Key Vault '{VaultName}'.");

            var i = outputCollection.FirstOrDefault() as dynamic;
            return i != null ?
                new VaultCertificate()
                {
                    KeyId = i.KeyId,
                    SecretId = i.SecretId,
                    Thumbprint = i.Thumbprint
                } : null;
        }
    }
}
