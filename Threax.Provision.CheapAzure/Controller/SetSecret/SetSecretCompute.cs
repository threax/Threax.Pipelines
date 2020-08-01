using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Threax.ConsoleApp;
using Threax.DockerBuildConfig;
using Threax.Pipelines.Core;
using Threax.Provision.CheapAzure.Resources;
using Threax.Provision.CheapAzure.Services;

namespace Threax.Provision.CheapAzure.Controller.SetSecret
{
    class SetSecretCompute : IResourceProcessor<Compute>
    {
        private readonly ILogger logger;
        private readonly IVmCommands vmCommands;
        private readonly IArgsProvider argsProvider;
        private readonly Config config;
        private readonly IConfigFileProvider configFileProvider;

        public SetSecretCompute(
            ILogger<SetSecretCompute> logger, 
            IVmCommands vmCommands,
            IArgsProvider argsProvider,
            Config config,
            IConfigFileProvider configFileProvider)
        {
            this.logger = logger;
            this.vmCommands = vmCommands;
            this.argsProvider = argsProvider;
            this.config = config;
            this.configFileProvider = configFileProvider;
        }

        public async Task Execute(Compute resource)
        {
            var args = argsProvider.Args.Skip(3).ToList();
            if(args.Count < 2)
            {
                throw new InvalidOperationException("You must provide a name and source file to set a secret.");
            }

            var name = args[0];
            var source = args[1];

            var fileName = Path.GetFileName(configFileProvider.GetConfigPath());
            var configFilePath = $"/app/{resource.Name}/{fileName}";

            await vmCommands.SetSecretFromFile(config.VmName, config.ResourceGroup, configFileProvider.GetConfigPath(), configFilePath, name, source);
        }
    }
}
