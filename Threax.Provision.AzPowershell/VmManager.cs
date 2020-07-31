using Microsoft.Extensions.Logging;
using System;
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

        /// <summary>
        /// Run a command from a string. Warning, the command will be written to a file so consider 
        /// that when using this with secrets. The file is erased when the function exits. If no file
        /// name is provided one will be generated using Path.GetTempFileName().
        /// </summary>
        /// <param name="Name">The vm name.</param>
        /// <param name="ResourceGroupName">The resource group name.</param>
        /// <param name="CommandId">The command to run. Ideally use RunShellScript.</param>
        /// <param name="command">The command to run.</param>
        /// <param name="tempFileName">Optional, the path to the temporary file to save.</param>
        /// <returns></returns>
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
