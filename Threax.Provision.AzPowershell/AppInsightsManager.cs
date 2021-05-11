using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class AppInsightsManager : IAppInsightsManager
    {
        private readonly IShellRunner shellRunner;

        public AppInsightsManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public async Task<String> GetAppInsightsInstrumentationKey(String Name, String ResourceGroupName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.ApplicationInsights");
            pwsh.AddResultCommand($"Get-AzApplicationInsights -Name {Name} -ResourceGroupName {ResourceGroupName}");

            dynamic result = await shellRunner.RunProcessAsync(pwsh,
               invalidExitCodeMessage: $"Error getting App Insights instrumentation key for '{Name}' in Resource Group '{ResourceGroupName}'.");

            return result.InstrumentationKey;
        }
    }
}
