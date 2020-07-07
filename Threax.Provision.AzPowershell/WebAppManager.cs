using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class WebAppManager : IWebAppManager
    {
        private readonly ILogger<WebAppManager> logger;

        public WebAppManager(ILogger<WebAppManager> logger)
        {
            this.logger = logger;
        }

        public async Task<WebAppInfo> GetWebApp(String Name, String ResourceGroupName)
        {
            var pwshArgs = new { Name, ResourceGroupName };

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Websites");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("Get-AzWebApp", pwshArgs);

            var outputCollection = await pwsh.RunAsync();
            pwsh.ThrowOnErrors($"Error getting WebApp '{Name}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.First();

            var principalId = result.Identity?.PrincipalId;

            return new WebAppInfo()
            {
                IdentityObjectId = principalId != null ? new Guid(principalId) : default(Guid?)
            };
        }

        public async Task<WebAppInfo> CreateWebAppIdentity(String Name, String ResourceGroupName)
        {
            var pwshArgs = new { Name, ResourceGroupName };

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Websites");
            pwsh.AddParamLine(pwshArgs);
            pwsh.AddCommandWithParams("Set-AzWebApp -AssignIdentity $true", pwshArgs);

            var outputCollection = await pwsh.RunAsync();
            pwsh.ThrowOnErrors($"Error creating WebApp Identity for '{Name}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.First();

            var principalId = result.Identity?.PrincipalId;

            return new WebAppInfo()
            {
                IdentityObjectId = principalId != null ? new Guid(principalId) : default(Guid?)
            };
        }
    }
}
