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

namespace Threax.Provision.CheapAzure.Controller.Deploy
{
    class DeployCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly BuildConfig buildConfig;
        private readonly DeploymentConfig deployConfig;
        private readonly ILogger<DeployCompute> logger;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IImageManager imageManager;
        private readonly IProcessRunner processRunner;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ISqlServerFirewallRuleManager sqlServerFirewallRuleManager;
        private readonly AzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IVmCommands vmCommands;
        private readonly IConfigFileProvider configFileProvider;

        public DeployCompute(
            Config config,
            BuildConfig buildConfig,
            DeploymentConfig deployConfig,
            ILogger<DeployCompute> logger,
            IKeyVaultManager keyVaultManager,
            IImageManager imageManager,
            IProcessRunner processRunner,
            IKeyVaultAccessManager keyVaultAccessManager,
            ISqlServerFirewallRuleManager sqlServerFirewallRuleManager,
            AzureKeyVaultConfig azureKeyVaultConfig,
            IVmCommands vmCommands,
            IConfigFileProvider configFileProvider)
        {
            this.config = config;
            this.buildConfig = buildConfig;
            this.deployConfig = deployConfig;
            this.logger = logger;
            this.keyVaultManager = keyVaultManager;
            this.imageManager = imageManager;
            this.processRunner = processRunner;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.vmCommands = vmCommands;
            this.configFileProvider = configFileProvider;
        }

        public async Task Execute(Compute resource)
        {
            if (!String.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
            {
                logger.LogInformation($"Unlocking '{azureKeyVaultConfig.VaultName}' for user id '{config.UserId}'.");
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);
            }

            if (String.IsNullOrEmpty(resource.Name))
            {
                throw new InvalidOperationException("You must include a resource 'Name' property to deploy compute.");
            }

            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);
            var branchTag = $"{image}:{buildConfig.Branch}";
            int exitCode;

            //Push
            logger.LogInformation($"Pushing '{image}' for branch '{buildConfig.Branch}'.");

            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"tag {taggedImageName} {branchTag}"));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker tag.");
            }

            exitCode = processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"push {branchTag}"));
            if (exitCode != 0)
            {
                throw new InvalidOperationException("An error occured during the docker push.");
            }

            //Deploy
            logger.LogInformation($"Deploying '{image}' for branch '{buildConfig.Branch}'.");
            var jobj = configFileProvider.GetConfigJObject();

            var deploy = jobj["Deploy"];
            if(deploy == null)
            {
                deploy = new JObject();
                jobj["Deploy"] = deploy;
            }
            deploy["ImageName"] = branchTag;

            var fileName = Path.GetFileName(configFileProvider.GetConfigPath());
            var configJson = jobj.ToString(Newtonsoft.Json.Formatting.Indented);
            await vmCommands.ThreaxDockerToolsRun($"/app/{resource.Name}/{fileName}", configJson);
        }
    }
}
