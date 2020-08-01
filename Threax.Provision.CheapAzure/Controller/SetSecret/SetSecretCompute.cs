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
using Threax.Provision.CheapAzure.Model;
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
            var secrets = new List<SetSecretModel>();

            for(var i = 0; i + 1 < args.Count; i += 2)
            {
                var name = args[i];
                var file = args[i + 1];

                secrets.Add(new SetSecretModel()
                {
                    File = $"/app/{resource.Name}/temp/{name}",
                    Name = name,
                    Content = File.ReadAllText(file)
                });
            }

            var fileName = Path.GetFileName(configFileProvider.GetConfigPath());
            var configFilePath = $"/app/{resource.Name}/{fileName}";
            var configContents = configFileProvider.GetConfigText();

            await vmCommands.SetSecrets(config.VmName, config.ResourceGroup, configFilePath, configContents, secrets);
        }
    }
}
