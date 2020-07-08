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

namespace Threax.Provision.CheapAzure.Controller.Deploy
{
    class DeployCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly BuildConfig buildConfig;
        private readonly ILogger<DeployCompute> logger;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IWebAppIdentityManager webAppManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IImageManager imageManager;
        private readonly IProcessRunner processRunner;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ISqlServerManager sqlServerManager;
        private readonly ISqlServerFirewallRuleManager sqlServerFirewallRuleManager;

        public DeployCompute(Config config, BuildConfig buildConfig, ILogger<DeployCompute> logger, IAcrManager acrManager, IArmTemplateManager armTemplateManager, IWebAppIdentityManager webAppManager, IKeyVaultManager keyVaultManager, IImageManager imageManager, IProcessRunner processRunner, IKeyVaultAccessManager keyVaultAccessManager, ISqlServerManager sqlServerManager, ISqlServerFirewallRuleManager sqlServerFirewallRuleManager)
        {
            this.config = config;
            this.buildConfig = buildConfig;
            this.logger = logger;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.webAppManager = webAppManager;
            this.keyVaultManager = keyVaultManager;
            this.imageManager = imageManager;
            this.processRunner = processRunner;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.sqlServerManager = sqlServerManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
        }

        public async Task Execute(Compute resource)
        {
            var appName = resource.Name ?? throw new InvalidOperationException($"You must provide a '{nameof(Compute.Name)}' property on your '{nameof(Compute)}' resource.");

            await keyVaultAccessManager.Unlock(config.KeyVaultName, config.UserId);

            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);

            //Run Init Command if present
            if (resource.InitCommand != null)
            {
                logger.LogInformation($"Running Init Command for '{image}' with tag '{taggedImageName}'.");

                await sqlServerFirewallRuleManager.Unlock(config.SqlServerName, config.ResourceGroup, config.MachineIp, config.MachineIp);
                var envs = resource.InitEnv.Select(i => new KeyValuePair<string, string>(i.Key.Replace(".", "__"), i.Value)).ToList();

                var sb = new StringBuilder("run "); //Trailing space is important
                foreach (var env in envs)
                {
                    sb.Append($"--env {env.Key} "); //Trailing space is important
                }
                sb.Append($"{taggedImageName} {resource.InitCommand}");
                var psi = new ProcessStartInfo("docker", sb.ToString());

                foreach (var env in envs)
                {
                    var secret = await keyVaultManager.GetSecret(config.KeyVaultName, env.Value);
                    psi.EnvironmentVariables.Add(env.Key, secret);
                }

                processRunner.RunProcessWithOutput(psi);
            }

            //Deploy
            logger.LogInformation($"Deploying '{image}' with tag '{taggedImageName}'.");

            var acrCreds = await acrManager.GetAcrCredential(config.AcrName, config.ResourceGroup);

            var securePass = new SecureString();
            foreach(var c in acrCreds.Password)
            {
                securePass.AppendChar(c);
            }
            securePass.MakeReadOnly();

            //Deploy app itself
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmDockerWebApp()
            {
                 dockerRegistryPassword = securePass,
                 dockerRegistryUsername = acrCreds.Username,
                 dockerRegistryUrl = $"{config.AcrName}.azurecr.io",
                 alwaysOn = resource.AlwaysOn,
                 nameFromTemplate = appName,
                 hostingPlanName = config.AppServicePlanName,
                 serverFarmResourceGroup = config.ResourceGroup,
                 location = config.Location,
                 subscriptionId = config.SubscriptionId,
                 linuxFxVersion = $"DOCKER|{taggedImageName}"
            });

            //Update app permissions in key vault
            var appId = await webAppManager.GetOrCreateWebAppIdentity(appName, config.ResourceGroup);

            try
            {
                await keyVaultManager.UnlockSecrets(config.KeyVaultName, appId);
            }
            catch (Exception ex)
            {
                var delay = 3000;
                logger.LogError(ex, $"An error occured setting the key vault permissions. Trying again after {delay}ms...");
                Thread.Sleep(delay);
                logger.LogInformation("Sleep complete. Trying permissions again.");
                await keyVaultManager.UnlockSecrets(config.KeyVaultName, appId);
            }
        }
    }
}
