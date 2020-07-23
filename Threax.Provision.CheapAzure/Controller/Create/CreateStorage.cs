using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Threax.Azure.Abstractions;
using Threax.Provision.AzPowershell;
using Threax.Provision.CheapAzure.ArmTemplates.StorageAccount;
using Threax.Provision.CheapAzure.Resources;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateStorage : IResourceProcessor<Storage>
    {
        private readonly ILogger<CreateStorage> logger;
        private readonly Config config;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly IStorageManager storageManager;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly AzureKeyVaultConfig azureKeyVaultConfig;
        private readonly AzureStorageConfig azureStorageConfig;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;

        public CreateStorage(
            ILogger<CreateStorage> logger, 
            Config config, 
            IArmTemplateManager armTemplateManager, 
            IStorageManager storageManager, 
            IKeyVaultAccessManager keyVaultAccessManager, 
            IKeyVaultManager keyVaultManager, 
            AzureKeyVaultConfig azureKeyVaultConfig,
            AzureStorageConfig azureStorageConfig)
        {
            this.logger = logger;
            this.config = config;
            this.armTemplateManager = armTemplateManager;
            this.storageManager = storageManager;
            this.keyVaultManager = keyVaultManager;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
            this.azureStorageConfig = azureStorageConfig;
            this.keyVaultAccessManager = keyVaultAccessManager;
        }

        public async Task Execute(Storage resource)
        {
            var nameCheck = azureStorageConfig.StorageAccount ?? throw new InvalidOperationException("You must provide a name for storage resources.");

            var storage = new ArmStorageAccount(azureStorageConfig.StorageAccount, config.Location);
            await armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, storage);

            if (!String.IsNullOrWhiteSpace(resource.AccessKeySecretName))
            {
                logger.LogInformation($"Setting up connection string in Key Vault '{azureKeyVaultConfig.VaultName}'.");

                await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

                var accessKey = await storageManager.GetAccessKey(azureStorageConfig.StorageAccount, config.ResourceGroup);

                if(accessKey == null)
                {
                    throw new InvalidOperationException("The access key returned from the server was null.");
                }

                //Need to double check format here, assuming key is valid for now
                await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.AccessKeySecretName, accessKey);
            }
        }
    }
}
