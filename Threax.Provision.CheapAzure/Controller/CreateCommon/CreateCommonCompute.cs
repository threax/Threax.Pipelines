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

namespace Threax.Provision.CheapAzure.Controller.CreateCommon
{
    class CreateCommonCompute : IResourceProcessor<Compute>
    {
        private readonly Config config;
        private readonly IAcrManager acrManager;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ILogger<CreateCommonCompute> logger;
        private readonly ICredentialLookup credentialLookup;
        private readonly IVmManager vmManager;
        private readonly IVmCommands vmCommands;
        private readonly ISshCredsManager sshCredsManager;

        public CreateCommonCompute(
            Config config,
            IAcrManager acrManager,
            IArmTemplateManager armTemplateManager,
            IKeyVaultManager keyVaultManager,
            IKeyVaultAccessManager keyVaultAccessManager,
            ILogger<CreateCommonCompute> logger,
            ICredentialLookup credentialLookup,
            IVmManager vmManager,
            IVmCommands vmCommands,
            ISshCredsManager sshCredsManager)
        {
            this.config = config;
            this.acrManager = acrManager;
            this.armTemplateManager = armTemplateManager;
            this.keyVaultManager = keyVaultManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.logger = logger;
            this.credentialLookup = credentialLookup;
            this.vmManager = vmManager;
            this.vmCommands = vmCommands;
            this.sshCredsManager = sshCredsManager;
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

            var acrCreds = await acrManager.GetAcrCredential(config.AcrName, config.ResourceGroup);

            //Setup Vm
            await keyVaultAccessManager.Unlock(config.InfraKeyVaultName, config.UserId);
            var vmCreds = await credentialLookup.GetOrCreateCredentials(config.InfraKeyVaultName, config.VmAdminBaseKey);

            if (String.IsNullOrEmpty(config.VmName))
            {
                throw new InvalidOperationException($"You must supply a '{nameof(Config.VmName)}' property in your config file.");
            }

            logger.LogInformation($"Creating virtual machine '{config.VmName}'.");
            var publicKey = await sshCredsManager.LoadPublicKey();
            var vm = new ArmVm(config.VmName, config.ResourceGroup, vmCreds.User, publicKey)
            {
                publicIpAddressName = config.PublicIpName,
                networkSecurityGroupName = config.NsgName,
            };
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, vm);

            logger.LogInformation("Running setup script on server.");
            await vmCommands.RunSetupScript(config.VmName, config.ResourceGroup, $"{config.AcrName}.azurecr.io", acrCreds);

            //Setup App Insights
            logger.LogInformation($"Creating App Insights '{config.AppInsightsName}' in Resource Group '{config.ResourceGroup}'");

            var armAppInsights = new ArmAppInsights(config.AppInsightsName, config.Location);
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, armAppInsights);
        }
    }
}
