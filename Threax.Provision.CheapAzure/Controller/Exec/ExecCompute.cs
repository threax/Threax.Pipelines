using Threax.Provision.CheapAzure;
using Threax.Provision.CheapAzure.ArmTemplates.DockerWebApp;
using Threax.Provision.CheapAzure.Resources;
using System;
using System.Security;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Docker;
using Microsoft.Extensions.Logging;
using System.Threading;
using Threax.Pipelines.Core;
using System.Diagnostics;
using Threax.Provision.CheapAzure.Services;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Threax.DeployConfig;
using Threax.Azure.Abstractions;
using System.IO;
using Newtonsoft.Json.Linq;
using Threax.ConsoleApp;

namespace Threax.Provision.CheapAzure.Controller.Deploy
{
    class ExecCompute : IResourceProcessor<Compute>
    {
        private readonly IVmCommands vmCommands;
        private readonly IConfigFileProvider configFileProvider;
        private readonly IArgsProvider argsProvider;

        public ExecCompute(
            IVmCommands vmCommands,
            IConfigFileProvider configFileProvider,
            IArgsProvider argsProvider)
        {
            this.vmCommands = vmCommands;
            this.configFileProvider = configFileProvider;
            this.argsProvider = argsProvider;
        }

        public async Task Execute(Compute resource)
        {
            if (String.IsNullOrEmpty(resource.Name))
            {
                throw new InvalidOperationException("You must include a resource 'Name' property to deploy compute.");
            }

            var command = argsProvider.Args[3];
            var args = argsProvider.Args.Skip(4).ToArray();
            var fileName = Path.GetFileName(configFileProvider.GetConfigPath());
            await vmCommands.ThreaxDockerToolsExec($"/app/{resource.Name}/{fileName}", command, args);
        }
    }
}
