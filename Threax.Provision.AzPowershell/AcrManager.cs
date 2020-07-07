using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class AcrManager : IAcrManager
    {
        private readonly ILogger<AcrManager> logger;

        public AcrManager(ILogger<AcrManager> logger)
        {
            this.logger = logger;
        }

        public async Task Create(String Name, String ResourceGroupName, string Location, String Sku)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, Sku, Location };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("New-AzContainerRegistry -EnableAdminUser", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error creating Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}' in '{Location}' with sku '{Sku}'.");
        }

        public async Task<bool> IsNameAvailable(String Name)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Test-AzContainerRegistryNameAvailability", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error testing Azure Container Registry name availability for '{Name}'.");

            dynamic result = outputCollection.First();
            return result.NameAvailable;
        }

        public async Task GetAcr(String Name, String ResourceGroupName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Get-AzContainerRegistry", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}'.");
        }

        public async Task<AcrCredential> GetAcrCredential(String Name, String ResourceGroupName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Get-AzContainerRegistryCredential", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.First();
            return new AcrCredential
            {
                Username = result.Username,
                Password = result.Password,
                Password2 = result.Password2,
            };
        }
    }
}
