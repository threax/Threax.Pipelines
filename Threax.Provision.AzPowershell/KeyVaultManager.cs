using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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

        public async Task UnlockSecrets(String keyVaultName, Guid userId)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($userId, $keyVaultName)");
                pwsh.AddParameter("userId", userId.ToString());
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddScript("Set-AzKeyVaultAccessPolicy -ObjectId $userId -VaultName $keyVaultName -PermissionsToSecrets set,delete,get,list");

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

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
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

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
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

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
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

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
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

        public async Task<String> GetSecret(String keyVaultName, String name)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.AddScript("Set-ExecutionPolicy -ExecutionPolicy Unrestricted");
                pwsh.AddScript("Import-Module Az.KeyVault");
                pwsh.AddScript("param($keyVaultName, $name)");
                pwsh.AddParameter("keyVaultName", keyVaultName);
                pwsh.AddParameter("name", name);
                pwsh.AddScript("Get-AzKeyVaultSecret -VaultName $keyVaultName -Name $name");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error loading '{name}' from Key Vault '{keyVaultName}'.");
                var info = outputCollection.FirstOrDefault() as dynamic;
                return info?.SecretValueText;
            }
        }
    }
}
