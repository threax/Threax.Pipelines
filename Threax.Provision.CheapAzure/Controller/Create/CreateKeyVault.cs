using Threax.Provision.CheapAzure.ArmTemplates.KeyVault;
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
    class CreateKeyVault : IResourceProcessor<KeyVault>
    {
        private readonly IArmTemplateManager armTemplateManager;
        private readonly Config config;

        public CreateKeyVault(IArmTemplateManager armTemplateManager, Config config)
        {
            this.armTemplateManager = armTemplateManager;
            this.config = config;
        }

        public async Task Execute(KeyVault resource)
        {
            var keyVaultArm = new ArmKeyVault(config.KeyVaultName, config.Location, config.TenantId.ToString());
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, keyVaultArm);
        }
    }
}
