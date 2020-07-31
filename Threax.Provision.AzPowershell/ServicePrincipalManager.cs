using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class ServicePrincipalManager : IServicePrincipalManager
    {
        private readonly ILogger<ServicePrincipalManager> logger;

        public ServicePrincipalManager(ILogger<ServicePrincipalManager> logger)
        {
            this.logger = logger;
        }

        public async Task<bool> Exists(String DisplayName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            var parm = new { DisplayName };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Get-AzADServicePrincipal", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting service principal '{DisplayName}'.");

            return outputCollection.Any();
        }

        public async Task Remove(String DisplayName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            var parm = new { DisplayName };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Remove-AzADServicePrincipal -Force", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting service principal '{DisplayName}'.");
        }

        public async Task RemoveApplication(String DisplayName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            var parm = new { DisplayName };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Remove-AzADApplication -Force", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting service principal '{DisplayName}'.");
        }

        public async Task<ServicePrincipal> CreateServicePrincipal(String displayName, String subscription, String resourceGroup, String role = "Reader")
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Resources");
            var parm = new { DisplayName = displayName, Role = role, Scope = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}" };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("New-AzADServicePrincipal", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error creating service principal '{displayName}' in Scope '{parm.Scope}' with role '{role}'.");

            dynamic result = outputCollection.First();
            return new ServicePrincipal
            {
                Id = result.Id,
                ApplicationId = result.ApplicationId,
                Secret = result.Secret,
                DisplayName = result.DisplayName
            };
        }
    }
}
