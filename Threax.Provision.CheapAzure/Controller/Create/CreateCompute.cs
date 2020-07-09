﻿using Threax.Provision.CheapAzure.ArmTemplates.AppServicePlan;
using Threax.Provision.CheapAzure.Resources;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;
using Microsoft.Extensions.Logging;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IWebAppIdentityManager webAppManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly ILogger<CreateCompute> logger;

        public CreateCompute(Config config, IAcrManager acrManager, IArmTemplateManager armTemplateManager, IWebAppIdentityManager webAppManager, IKeyVaultManager keyVaultManager, ILogger<CreateCompute> logger)
        {
            this.config = config;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.webAppManager = webAppManager;
            this.keyVaultManager = keyVaultManager;
            this.logger = logger;
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
        }
    }
}