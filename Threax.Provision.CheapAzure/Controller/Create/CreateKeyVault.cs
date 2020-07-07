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
        private readonly IKeyVaultManager keyVaultManager;

        public CreateKeyVault(IArmTemplateManager armTemplateManager, Config config, IKeyVaultManager keyVaultManager)
        {
            this.armTemplateManager = armTemplateManager;
            this.config = config;
            this.keyVaultManager = keyVaultManager;
        }

        public async Task Execute(KeyVault resource)
        {
            if (!await keyVaultManager.Exists(config.KeyVaultName))
            {
                var keyVaultArm = new ArmKeyVault(config.KeyVaultName, config.Location, config.TenantId.ToString());
                await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, keyVaultArm);
            }
        }
    }
}
