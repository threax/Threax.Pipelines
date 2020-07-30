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
using Threax.Provision.CheapAzure.ArmTemplates.AppInsights;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly BuildConfig buildConfig;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ILogger<CreateCompute> logger;
        private readonly IWebAppIdentityManager webAppIdentityManager;
        private readonly AzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IWebAppManager webAppManager;
        private readonly IAppInsightsManager appInsightsManager;
        private readonly IServicePrincipalManager servicePrincipalManager;

        public CreateCompute(
            Config config, 
            BuildConfig buildConfig, 
            IAcrManager acrManager, 
            IArmTemplateManager armTemplateManager, 
            IKeyVaultManager keyVaultManager, 
            IKeyVaultAccessManager keyVaultAccessManager,
            ILogger<CreateCompute> logger,
            IWebAppIdentityManager webAppIdentityManager,
            AzureKeyVaultConfig azureKeyVaultConfig,
            IWebAppManager webAppManager,
            IAppInsightsManager appInsightsManager,
            IServicePrincipalManager servicePrincipalManager)
        {
            this.config = config;
            this.buildConfig = buildConfig;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.keyVaultManager = keyVaultManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.logger = logger;
            this.webAppIdentityManager = webAppIdentityManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.webAppManager = webAppManager;
            this.appInsightsManager = appInsightsManager;
            this.servicePrincipalManager = servicePrincipalManager;
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

            //Update app permissions in key vault
            if (!string.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
            {
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

                var spName = $"{resource.Name}-app";
                if (!await servicePrincipalManager.Exists(spName))
                {
                    var sp = await servicePrincipalManager.CreateServicePrincipal(spName, config.SubscriptionId, config.ResourceGroup);
                    await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-id", sp.Id);
                    await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-appkey", sp.Secret);
                    var appKey = await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-appkey");
                    var spConnectionString = $"RunAs=App;AppId={sp.ApplicationId};TenantId={config.TenantId};AppKey={appKey}";
                    await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-connectionstring", spConnectionString);
                }

                var id = await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-id");
                await keyVaultManager.UnlockSecretsRead(azureKeyVaultConfig.VaultName, Guid.Parse(id));
            }

            //Setup App Insights
            if (!String.IsNullOrEmpty(resource.AppInsightsSecretName))
            {
                logger.LogInformation($"Creating App Insights '{config.AppInsightsName}' in Resource Group '{config.ResourceGroup}'");

                var armAppInsights = new ArmAppInsights(config.AppInsightsName, config.Location);
                await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, armAppInsights);

                var instrumentationKey = await appInsightsManager.GetAppInsightsInstrumentationKey(config.AppInsightsName, config.ResourceGroup);
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);
                await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.AppInsightsSecretName, instrumentationKey);
            }
        }
    }
}
