using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class AppInsightsManager : IAppInsightsManager
    {
        private readonly ILogger<AppInsightsManager> logger;

        public AppInsightsManager(ILogger<AppInsightsManager> logger)
        {
            this.logger = logger;
        }

        public async Task<String> GetAppInsightsInstrumentationKey(String Name, String ResourceGroupName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ApplicationInsights");
            var parm = new { Name, ResourceGroupName, };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Get-AzApplicationInsights", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error getting Azure Container Registry '{Name}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.First();
            return result.InstrumentationKey;
        }
    }
}
