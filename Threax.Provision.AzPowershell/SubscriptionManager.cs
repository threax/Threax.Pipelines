using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IShellRunner shellRunner;

        public SubscriptionManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public Task SetContext(Guid subscriptionId)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Accounts");
            pwsh.AddResultCommand($"Set-AzContext -SubscriptionId {subscriptionId}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error setting context to '{subscriptionId}'.");
        }
    }
}
