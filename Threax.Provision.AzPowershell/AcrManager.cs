using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class AcrManager : IAcrManager
    {
        private readonly IShellRunner shellRunner;

        public AcrManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public Task Create(String Name, String ResourceGroupName, string Location, String Sku)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.ContainerRegistry");
            pwsh.AddResultCommand($"New-AzContainerRegistry -EnableAdminUser -Name {Name} -ResourceGroupName {ResourceGroupName} -Location {Location} -Sku {Sku}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error creating Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}' in '{Location}' with sku '{Sku}'.");
        }

        public async Task<bool> IsNameAvailable(String Name)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.ContainerRegistry");
            pwsh.AddResultCommand($"Test-AzContainerRegistryNameAvailability -Name {Name}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error testing Azure Container Registry name availability for '{Name}'.");

            return result.NameAvailable;
        }

        public async Task GetAcr(String Name, String ResourceGroupName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.ContainerRegistry");
            pwsh.AddResultCommand($"Get-AzContainerRegistry -Name {Name} -ResourceGroupName {ResourceGroupName}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error getting Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}'.");

        }

        public async Task<AcrCredential> GetAcrCredential(String Name, String ResourceGroupName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.ContainerRegistry");
            pwsh.AddResultCommand($"Get-AzContainerRegistryCredential -Name {Name} -ResourceGroupName {ResourceGroupName}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error getting Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}'.");

            return new AcrCredential
            {
                Username = result.Username,
                Password = result.Password,
                Password2 = result.Password2,
            };
        }
    }
}
