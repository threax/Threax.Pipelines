using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class StorageManager : IStorageManager
    {
        private readonly ILogger<StorageManager> logger;

        public StorageManager(ILogger<StorageManager> logger)
        {
            this.logger = logger;
        }

        public async Task<String> GetAccessKey(String AccountName, String ResourceGroupName)
        {
            var pwshArgs = new { AccountName, ResourceGroupName };

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Storage");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("Get-AzStorageAccountKey", pwshArgs);

            var outputCollection = await pwsh.RunAsync();
            pwsh.ThrowOnErrors($"Error getting storage account key for '{AccountName}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.First();

            return result.Value;
        }
    }
}
