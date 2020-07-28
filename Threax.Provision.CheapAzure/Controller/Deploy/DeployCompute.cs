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
        private readonly IWebAppManager webAppManager;

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
            IWebAppManager webAppManager)
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
            this.webAppManager = webAppManager;
        }

        public async Task Execute(Compute resource)
        {
            if (!String.IsNullOrEmpty(azureKeyVaultConfig.VaultName) && config.UserId != config.AzDoUser)
            {
                logger.LogInformation($"Unlocking '{azureKeyVaultConfig.VaultName}' for user id '{config.UserId}'.");
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);
            }

            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);
            var branchTag = $"{image}:{buildConfig.Branch}";

            //Run Init Command if present
            if (deployConfig.InitCommand != null)
            {
                logger.LogInformation($"Running Init Command for '{image}' with tag '{taggedImageName}'.");

                await sqlServerFirewallRuleManager.Unlock(config.SqlServerName, config.ResourceGroup, config.MachineIp, config.MachineIp);
                var secrets = deployConfig.InitSecrets?.Select(i => new KeyValuePair<string, string>(i.Key.Replace(".", "__"), i.Value))?.ToList()
                    ?? Enumerable.Empty<KeyValuePair<String, String>>();

                var sb = new StringBuilder("run "); //Trailing space is important
                foreach (var secret in secrets)
                {
                    sb.Append($"--env {secret.Key} "); //Trailing space is important
                }
                sb.Append($"{taggedImageName} {deployConfig.InitCommand}");
                var psi = new ProcessStartInfo("docker", sb.ToString());

                if (!String.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
                {
                    foreach (var secret in secrets)
                    {
                        var secretValue = await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, secret.Value);
                        psi.EnvironmentVariables.Add(secret.Key, secretValue);
                    }
                }
                else
                {
                    logger.LogInformation($"No 'KeyVault.{nameof(AzureKeyVaultConfig.VaultName)}' property defined in config. Skipping secret load during deploy.");
                }

                processRunner.RunProcessWithOutput(psi);
            }

            //Deploy
            logger.LogInformation($"Deploying '{image}' for branch '{buildConfig.Branch}'.");

            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"tag {taggedImageName} {branchTag}"));

            processRunner.RunProcessWithOutput(new ProcessStartInfo("docker", $"push {branchTag}"));

            if (!resource.EnableContinuousDeployment)
            {
                logger.LogInformation($"Restarting '{resource.Name}' in '{config.ResourceGroup}'.");

                await webAppManager.Restart(resource.Name, config.ResourceGroup);

                logger.LogInformation($"Restarted '{resource.Name}' in '{config.ResourceGroup}'.");
            }
        }
    }
}
