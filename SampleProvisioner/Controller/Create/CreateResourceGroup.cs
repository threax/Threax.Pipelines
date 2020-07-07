using SampleProvisioner.ArmTemplates.KeyVault;
using SampleProvisioner.ArmTemplates.ResourceGroup;
using SampleProvisioner.HiddenResources;
using SampleProvisioner.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;

namespace SampleProvisioner.Controller.Create
{
    class CreateResourceGroup : IResourceProcessor<ResourceGroup>
    {
        private readonly IArmTemplateManager armTemplateManager;
        private readonly Config config;

        public CreateResourceGroup(IArmTemplateManager armTemplateManager, Config config)
        {
            this.armTemplateManager = armTemplateManager;
            this.config = config;
        }

        public async Task Execute(ResourceGroup resource)
        {
            var armResourceGroup = new ArmResourceGroup(resource.Name);
            await armTemplateManager.SubscriptionDeployment(config.Location, armResourceGroup);
        }
    }
}
