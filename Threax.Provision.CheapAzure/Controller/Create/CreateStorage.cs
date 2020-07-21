using System;
using System.Threading.Tasks;
using Threax.Configuration.AzureKeyVault;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.ArmTemplates.StorageAccount;
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
            var nameCheck = resource.Name ?? throw new InvalidOperationException("You must provide a name for storage resources.");

            var storage = new ArmStorageAccount(resource.Name, config.Location);
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, storage);

            if (!String.IsNullOrWhiteSpace(resource.AccessCredsSecretName))
            {
                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

                var accessKey = await storageManager.GetAccessKey(resource.Name, config.ResourceGroup);

                if(accessKey == null)
                {
                    throw new InvalidOperationException("The access key returned from the server was null.");
                }

                var connectionString = storageManager.CreateConnectionString(resource.Name, accessKey);

                //Need to double check format here, assuming key is valid for now
                await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.AccessCredsSecretName, connectionString);
            }
        }
    }
}
