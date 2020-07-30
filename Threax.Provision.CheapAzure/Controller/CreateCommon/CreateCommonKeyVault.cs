using System.Threading.Tasks;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.ArmTemplates.KeyVault;
using Threax.Provision.CheapAzure.HiddenResources;

namespace Threax.Provision.CheapAzure.Controller.CreateCommon
{
    class CreateCommonKeyVault : IResourceProcessor<KeyVault>
    {
        private readonly IArmTemplateManager armTemplateManager;
        private readonly Config config;
        private readonly IKeyVaultManager keyVaultManager;

        public CreateCommonKeyVault(IArmTemplateManager armTemplateManager, Config config, IKeyVaultManager keyVaultManager)
        {
            this.armTemplateManager = armTemplateManager;
            this.config = config;
            this.keyVaultManager = keyVaultManager;
        }

        public async Task Execute(KeyVault resource)
        {
            if (!await keyVaultManager.Exists(config.InfraKeyVaultName))
            {
                var keyVaultArm = new ArmKeyVault(config.InfraKeyVaultName, config.Location, config.TenantId.ToString());
                await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, keyVaultArm);
            }

            //Allow AzDo user in the key vault if one is set.
            if (config.AzDoUser != null)
            {
                await keyVaultManager.UnlockSecretsRead(config.InfraKeyVaultName, config.AzDoUser.Value);
            }
        }
    }
}
