using Threax.Provision.CheapAzure.ArmTemplates.AppServicePlan;
using Threax.Provision.CheapAzure.Resources;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;
using Microsoft.Extensions.Logging;
using Threax.Provision.CheapAzure.ArmTemplates.DockerWebApp;
using System;
using Threax.DockerBuildConfig;
using System.Linq;
using System.Threading;
using Threax.Azure.Abstractions;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly BuildConfig buildConfig;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly ILogger<CreateCompute> logger;
        private readonly IWebAppIdentityManager webAppIdentityManager;
        private readonly AzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IWebAppManager webAppManager;

        public CreateCompute(
            Config config, 
            BuildConfig buildConfig, 
            IAcrManager acrManager, 
            IArmTemplateManager armTemplateManager, 
            IKeyVaultManager keyVaultManager, 
            ILogger<CreateCompute> logger,
            IWebAppIdentityManager webAppIdentityManager,
            AzureKeyVaultConfig azureKeyVaultConfig,
            IWebAppManager webAppManager)
        {
            this.config = config;
            this.buildConfig = buildConfig;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.keyVaultManager = keyVaultManager;
            this.logger = logger;
            this.webAppIdentityManager = webAppIdentityManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.webAppManager = webAppManager;
        }

        public async Task Execute(Compute resource)
        {
            if (await this.acrManager.IsNameAvailable(config.AcrName))
            {
                await this.acrManager.Create(config.AcrName, config.ResourceGroup, config.Location, "Basic");
            }
            else
            {
                //This will fail if this acr isn't under our control
                await this.acrManager.GetAcr(config.AcrName, config.ResourceGroup);
            }

#if ENABLE_APP_SERVICE_DEPLOY
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmAppServicePlan(config.AppServicePlanName));
#else
            logger.LogWarning("Deployment of app service plans disabled during trial to keep it working.");
#endif

            var imageName = $"{buildConfig.ImageName}:{buildConfig.Branch}";
            var appName = resource.Name ?? throw new InvalidOperationException($"You must provide a '{nameof(Compute.Name)}' property on your '{nameof(Compute)}' resource.");
            logger.LogInformation($"Creating docker webapp '{appName}' with '{imageName}'.");

            var acrCreds = await acrManager.GetAcrCredential(config.AcrName, config.ResourceGroup);

            //Deploy app itself
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmDockerWebApp()
            {
                dockerRegistryPassword = acrCreds.Password.ToSecureString(),
                dockerRegistryUsername = acrCreds.Username,
                dockerRegistryUrl = $"{config.AcrName}.azurecr.io",
                alwaysOn = resource.AlwaysOn,
                nameFromTemplate = appName,
                hostingPlanName = config.AppServicePlanName,
                serverFarmResourceGroup = config.ResourceGroup,
                location = config.Location,
                subscriptionId = config.SubscriptionId,
                linuxFxVersion = $"DOCKER|{imageName}"
            });

            //Need to setup continuous deployment

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
            await this.webAppManager.SetHostnames(resource.Name, config.ResourceGroup, hostNames);

            if (!String.IsNullOrEmpty(config.SslCertThumb) && resource.DnsNames.Count > 0)
            {
                logger.LogInformation($"Creating SSL Bindings to '[{String.Join(", ", resource.DnsNames)}]' with thumb '{config.SslCertThumb}'.");
                foreach (var host in resource.DnsNames)
                {
                    await this.webAppManager.CreateSslBinding(resource.Name, config.ResourceGroup, config.SslCertThumb, host);
                }
            }
        }
    }
}
