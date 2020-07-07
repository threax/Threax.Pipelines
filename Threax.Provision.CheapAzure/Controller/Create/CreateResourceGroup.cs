using Threax.Provision.CheapAzure.ArmTemplates.KeyVault;
using Threax.Provision.CheapAzure.ArmTemplates.ResourceGroup;
using Threax.Provision.CheapAzure.HiddenResources;
using Threax.Provision.CheapAzure.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;

namespace Threax.Provision.CheapAzure.Controller.Create
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
