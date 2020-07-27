using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private ILogger<SubscriptionManager> logger;

        public SubscriptionManager(ILogger<SubscriptionManager> logger)
        {
            this.logger = logger;
        }

        public async Task SetContext(Guid subscriptionId)
        {
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.Accounts");
                pwsh.AddScript("param($sub) Set-AzContext -SubscriptionId $sub");
                pwsh.AddParameter("sub", subscriptionId.ToString());

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error setting context to '{subscriptionId}'.");
            }
        }
    }
}
