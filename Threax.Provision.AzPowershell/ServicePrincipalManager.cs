using System;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class ServicePrincipalManager : IServicePrincipalManager
    {
        private readonly IShellRunner shellRunner;

        public ServicePrincipalManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public async Task<bool> Exists(String DisplayName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Resources");
            pwsh.AddResultCommand($"Get-AzADServicePrincipal -DisplayName {DisplayName}");

            var result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error getting service principal '{DisplayName}'.");

            return result.Type != Newtonsoft.Json.Linq.JTokenType.Null;
        }

        public Task Remove(String DisplayName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Resources");
            pwsh.AddCommand($"Remove-AzADServicePrincipal -Force -DisplayName {DisplayName}");
            pwsh.AddResultCommand($"Remove-AzADApplication -Force -DisplayName {DisplayName}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error getting service principal '{DisplayName}'.");
        }

        public async Task<ServicePrincipal> CreateServicePrincipal(String displayName, String subscription, String resourceGroup, String role = "Reader")
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Resources");
            var scope = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}";
            pwsh.AddCommand($"$info = New-AzADServicePrincipal -DisplayName {displayName} -Role {role} -Scope {scope}");
            pwsh.AddCommand($"if($info -eq $null){{ throw }}");
            pwsh.AddCommand($"$secret = ConvertFrom-SecureString -SecureString $info.Secret -AsPlainText");
            pwsh.AddResultCommand($"@{{Secret = $secret;Id = $info.Id;ApplicationId = $info.ApplicationId;DisplayName = $info.DisplayName;}}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
                invalidExitCodeMessage: $"Error creating service principal '{displayName}' in Scope '{scope}' with role '{role}'.");

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
