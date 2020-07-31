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

        public async Task RunCommand(String Name, String ResourceGroupName, String CommandId, string ScriptPath)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, CommandId, ScriptPath };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Invoke-AzVMRunCommand", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error running Invoke-AzVMRunCommand for VM '{Name}' in Resource Group '{ResourceGroupName}'.");
        }

        public async Task RunCommand(String Name, String ResourceGroupName, String CommandId, string ScriptPath, Hashtable Parameters)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.ContainerRegistry");
            var parm = new { Name, ResourceGroupName, CommandId, ScriptPath, Parameters };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Invoke-AzVMRunCommand", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error running Invoke-AzVMRunCommand for VM '{Name}' in Resource Group '{ResourceGroupName}'.");
        }

        public async Task RunCommandFromString(String Name, String ResourceGroupName, String CommandId, string command, String tempFileName = null)
        {
            if (tempFileName == null)
            {
                tempFileName = Path.GetTempFileName();
            }
            try
            {
                File.WriteAllText(tempFileName, command);
                await RunCommand(Name, ResourceGroupName, CommandId, tempFileName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
}
