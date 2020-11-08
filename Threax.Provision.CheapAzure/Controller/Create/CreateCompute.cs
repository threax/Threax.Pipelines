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
using Threax.Provision.CheapAzure.ArmTemplates.ArmVm;
using Threax.Provision.CheapAzure.Services;
using System.Collections;
using System.Text;
using System.IO;
using Threax.Pipelines.Core;
using System.Collections.Generic;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ILogger<CreateCompute> logger;
        private readonly AzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IAppInsightsManager appInsightsManager;
        private readonly IServicePrincipalManager servicePrincipalManager;
        private readonly IVmCommands vmCommands;
        private readonly IConfigFileProvider configFileProvider;

        public CreateCompute(
            Config config,
            IKeyVaultManager keyVaultManager,
            IKeyVaultAccessManager keyVaultAccessManager,
            ILogger<CreateCompute> logger,
            AzureKeyVaultConfig azureKeyVaultConfig,
            IAppInsightsManager appInsightsManager,
            IServicePrincipalManager servicePrincipalManager,
            IVmCommands vmCommands,
            IConfigFileProvider configFileProvider)
        {
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.logger = logger;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.appInsightsManager = appInsightsManager;
            this.servicePrincipalManager = servicePrincipalManager;
            this.vmCommands = vmCommands;
            this.configFileProvider = configFileProvider;
        }

        public async Task Execute(Compute resource)
        {
            //Update app permissions in key vault
            if (!string.IsNullOrEmpty(azureKeyVaultConfig.VaultName))
            {
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

                var spName = $"{resource.Name}-app";
                if (!await servicePrincipalManager.Exists(spName))
                {
                    await CreateServicePrincipal(spName);
                }

                var id = (await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-id"))?.ToInsecureString();
                if (id == null)
                {
                    //If this id is null the service principal id was lost for this app. Recreate it
                    logger.LogInformation($"Cannot find service principal id for '{spName}'. Recreating it.");

                    if (await servicePrincipalManager.Exists(spName))
                    {
                        await servicePrincipalManager.Remove(spName);
                        await servicePrincipalManager.RemoveApplication(spName);
                    }

                    await CreateServicePrincipal(spName);

                    id = (await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-id"))?.ToInsecureString();
                }
                await keyVaultManager.UnlockSecretsRead(azureKeyVaultConfig.VaultName, Guid.Parse(id));

                //Setup App Connection String Secret
                logger.LogInformation("Setting app key vault connection string secret.");
                var vaultCs = (await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-connectionstring"))?.ToInsecureString();

                var fileName = Path.GetFileName(configFileProvider.GetConfigPath());
                var serverConfigFilePath = $"/app/{resource.Name}/{fileName}";

                await vmCommands.SetSecretFromString(config.VmName, config.ResourceGroup, configFileProvider.GetConfigPath(), serverConfigFilePath, "serviceprincipal-cs", vaultCs);
            }

            //Setup App Insights
            if (!String.IsNullOrEmpty(resource.AppInsightsSecretName))
            {
                logger.LogInformation($"Setting instrumentation key secret for App Insights '{config.AppInsightsName}' in Resource Group '{config.ResourceGroup}'");

                var instrumentationKey = await appInsightsManager.GetAppInsightsInstrumentationKey(config.AppInsightsName, config.ResourceGroup);
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);
                await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.AppInsightsSecretName, instrumentationKey);
            }
        }

        private async Task CreateServicePrincipal(string spName)
        {
            logger.LogInformation($"Creating service principal '{spName}'.");

            var sp = await servicePrincipalManager.CreateServicePrincipal(spName, config.SubscriptionId, config.ResourceGroup);
            await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-id", sp.Id);
            await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-appkey", sp.Secret);
            var appKey = (await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, "sp-appkey"))?.ToInsecureString();
            var spConnectionString = $"RunAs=App;AppId={sp.ApplicationId};TenantId={config.TenantId};AppKey={appKey}";
            await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, "sp-connectionstring", spConnectionString);
        }
    }
}
