
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class ResourceGroupManager
    {
        private readonly ILogger<ResourceGroupManager> logger;

        public ResourceGroupManager(ILogger<ResourceGroupManager> logger)
        {
            this.logger = logger;
        }

        public async Task Create(String name, String location)
        {
            //This should work, but it doesn't load libraries correctly. The powershell context must be within this app
            using (var pwsh = PowerShell.Create())
            {
                pwsh.PrintInformationStream(logger);
                pwsh.PrintErrorStream(logger);

                pwsh.SetUnrestrictedExecution();
                pwsh.AddScript("Import-Module Az.Resources");
                pwsh.AddScript("param($name, $loc)");
                pwsh.AddParameter("name", name);
                pwsh.AddParameter("loc", location);
                pwsh.AddScript("New-AzResourceGroup -Name $name -Location $loc -Force");

                var outputCollection = await pwsh.RunAsync();

                pwsh.ThrowOnErrors($"Error creating Resource Group '{name}' in '{location}'.");
            }
        }
    }
}
