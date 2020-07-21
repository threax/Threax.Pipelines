using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Threax.Configuration.AzureKeyVault;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.ArmTemplates.SqlServer;
using Threax.Provision.CheapAzure.Resources;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateStorage : IResourceProcessor<Storage>
    {
        private readonly Config config;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IStorageManager storageManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly ThreaxAzureKeyVaultConfig azureKeyVaultConfig;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;

        public CreateStorage(Config config, IArmTemplateManager armTemplateManager, IStorageManager storageManager, IKeyVaultAccessManager keyVaultAccessManager, IKeyVaultManager keyVaultManager, ThreaxAzureKeyVaultConfig azureKeyVaultConfig)
        {
            this.config = config;
            this.armTemplateManager = armTemplateManager;
            this.storageManager = storageManager;
            this.keyVaultManager = keyVaultManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.keyVaultAccessManager = keyVaultAccessManager;
        }

        public async Task Execute(Storage resource)
        {
            var storage = new ArmStorageAccount(resource.Name, config.Location);
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, storage);

            await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

            var accessKey = await storageManager.GetAccessKey(resource.Name, config.ResourceGroup);

            //Need to double check format here, assuming key is valid for now
            await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.AccessCredsSecretName, accessKey);
        }
    }
}
