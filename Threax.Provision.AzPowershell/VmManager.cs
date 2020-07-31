using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class VmManager : IVmManager
    {
        private readonly ILogger<VmManager> logger;

        public VmManager(ILogger<VmManager> logger)
        {
            this.logger = logger;
        }

        public async Task RunCommand(String Name, String ResourceGroupName, String CommandId, string ScriptPath, Hashtable Parameter)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, CommandId, ScriptPath, Parameter };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Invoke-AzVMRunCommand", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error running Invoke-AzVMRunCommand for VM '{Name}' in Resource Group '{ResourceGroupName}'.");

            dynamic result = outputCollection.FirstOrDefault();

            if (result != null)
            {
                foreach (dynamic value in result.Value)
                {
                    String message = $@"{value.DisplayStatus}\n{value.Message}";
                    if (message.Contains("Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"))
                    {
                        throw new InvalidOperationException($"An error occured running the server side script of Invoke-AzVMRunCommand for '{Name}' in '{ResourceGroupName}'");
                    }
                    logger.LogInformation(message);
                }
            }
        }
    }
}
