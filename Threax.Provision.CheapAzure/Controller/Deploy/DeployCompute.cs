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
using Threax.Configuration.AzureKeyVault;

namespace Threax.Provision.CheapAzure.Controller.Deploy
{
    class DeployCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly BuildConfig buildConfig;
        private readonly DeploymentConfig deployConfig;
        private readonly ILogger<DeployCompute> logger;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IWebAppIdentityManager webAppIdentityManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IImageManager imageManager;
        private readonly IProcessRunner processRunner;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ISqlServerManager sqlServerManager;
        private readonly ISqlServerFirewallRuleManager sqlServerFirewallRuleManager;
        private readonly ThreaxAzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IWebAppManager webAppManager;

        public DeployCompute(Config config, BuildConfig buildConfig, DeploymentConfig deployConfig, ILogger<DeployCompute> logger, IAcrManager acrManager, IArmTemplateManager armTemplateManager, IWebAppIdentityManager webAppIdentityManager, IKeyVaultManager keyVaultManager, IImageManager imageManager, IProcessRunner processRunner, IKeyVaultAccessManager keyVaultAccessManager, ISqlServerManager sqlServerManager, ISqlServerFirewallRuleManager sqlServerFirewallRuleManager, ThreaxAzureKeyVaultConfig azureKeyVaultConfig, IWebAppManager webAppManager)
        {
            this.config = config;
            this.buildConfig = buildConfig;
            this.deployConfig = deployConfig;
            this.logger = logger;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.webAppIdentityManager = webAppIdentityManager;
            this.keyVaultManager = keyVaultManager;
            this.imageManager = imageManager;
            this.processRunner = processRunner;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.sqlServerManager = sqlServerManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.webAppManager = webAppManager;
        }

        public async Task Execute(Compute resource)
        {
            var appName = resource.Name ?? throw new InvalidOperationException($"You must provide a '{nameof(Compute.Name)}' property on your '{nameof(Compute)}' resource.");

            if (!String.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
            {
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);
            }

            var image = buildConfig.ImageName;
            var currentTag = buildConfig.GetCurrentTag();
            var taggedImageName = imageManager.FindLatestImage(image, buildConfig.BaseTag, currentTag);

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
                    logger.LogInformation($"No 'KeyVault.{nameof(ThreaxAzureKeyVaultConfig.VaultName)}' property defined in config. Skipping secret load during deploy.");
                }

                processRunner.RunProcessWithOutput(psi);
            }

            //Deploy
            logger.LogInformation($"Deploying '{image}' with tag '{taggedImageName}'.");

            var acrCreds = await acrManager.GetAcrCredential(config.AcrName, config.ResourceGroup);

            //Deploy app itself
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmDockerWebApp()
            {
                dockerRegistryPassword = acrCreds.Password.ToSecureString(),
                dockerRegistryUsername = acrCreds.Username,
                dockerRegistryUrl = $"{config.AcrName}.azurecr.io",
                alwaysOn = false,
                nameFromTemplate = appName,
                hostingPlanName = config.AppServicePlanName,
                serverFarmResourceGroup = config.ResourceGroup,
                location = config.Location,
                subscriptionId = config.SubscriptionId,
                linuxFxVersion = $"DOCKER|{taggedImageName}"
            });

            //Update app permissions in key vault
            if (!string.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
            {
                var appId = await webAppIdentityManager.GetOrCreateWebAppIdentity(appName, config.ResourceGroup);

                try
                {
                    await keyVaultManager.UnlockSecretsRead(azureKeyVaultConfig.VaultName, appId);
                }
                catch (Exception ex)
                {
                    var delay = 8000;
                    logger.LogError(ex, $"An error occured setting the key vault permissions. Trying again after {delay}ms...");
                    Thread.Sleep(delay);
                    logger.LogInformation("Sleep complete. Trying permissions again.");
                    await keyVaultManager.UnlockSecretsRead(azureKeyVaultConfig.VaultName, appId);
                }
            }

            //Setup dns
            var hostNames = (resource.DnsNames ?? Enumerable.Empty<String>()).Concat(new string[] { $"{resource.Name}.azurewebsites.net" });
            logger.LogInformation($"Updating Host Names to '[{String.Join(", ", hostNames)}]'");
            await webAppManager.SetHostnames(resource.Name, config.ResourceGroup, hostNames);

            if (!String.IsNullOrEmpty(config.SslCertThumb) && resource.DnsNames.Count > 0)
            {
                logger.LogInformation($"Creating SSL Bindings to '[{String.Join(", ", resource.DnsNames)}]' with thumb '{config.SslCertThumb}'.");
                foreach (var host in resource.DnsNames)
                {
                    await webAppManager.CreateSslBinding(resource.Name, config.ResourceGroup, config.SslCertThumb, host);
                }
            }
        }
    }
}
