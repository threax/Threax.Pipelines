using SampleProvisioner;
using SampleProvisioner.ArmTemplates.DockerWebApp;
using SampleProvisioner.Resources;
using System;
using System.Security;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.Controller.Deploy
{
    class DeployCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IWebAppIdentityManager webAppManager;
        private readonly IKeyVaultManager keyVaultManager;

        public DeployCompute(Config config, IAcrManager acrManager, IArmTemplateManager armTemplateManager, IWebAppIdentityManager webAppManager, IKeyVaultManager keyVaultManager)
        {
            this.config = config;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.webAppManager = webAppManager;
            this.keyVaultManager = keyVaultManager;
        }

        public async Task Execute(Compute resource)
        {
            var appName = resource.Name ?? throw new InvalidOperationException($"You must provide a '{nameof(Compute.Name)}' property on your '{nameof(Compute)}' resource.");

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
                 linuxFxVersion = $"DOCKER|{config.AcrName}.azurecr.io/appdashboard:threaxpipe-20200707144622" //Right string mostly, but this is hardcoded. Just take image name from build config.
                //DOCKER|threaxtestacr.azurecr.io/hello-world:latest
            });

            var appId = await webAppManager.GetOrCreateWebAppIdentity(appName, config.ResourceGroup);
            await keyVaultManager.UnlockSecrets(config.KeyVaultName, appId);
        }
    }
}
