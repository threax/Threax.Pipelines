using System;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class StorageManager : IStorageManager
    {
        private readonly IShellRunner shellRunner;

        public StorageManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public async Task<String> GetAccessKey(String AccountName, String ResourceGroupName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Storage");
            pwsh.AddResultCommand($"Get-AzStorageAccountKey -AccountName {AccountName} -ResourceGroupName {ResourceGroupName}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error getting storage account key for '{AccountName}' in Resource Group '{ResourceGroupName}'.");

            return result.Value;
        }
    }
}
