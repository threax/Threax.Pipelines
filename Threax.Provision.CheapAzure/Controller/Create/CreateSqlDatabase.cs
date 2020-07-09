using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Threax.Provision.CheapAzure.ArmTemplates.SqlDb;
using Threax.Provision.CheapAzure.ArmTemplates.SqlServer;
using Threax.Provision.CheapAzure.Resources;
using Threax.Provision.CheapAzure.Services;
using System;
using System.Threading.Tasks;
using Threax.Provision;
using Threax.Provision.AzPowershell;
using Microsoft.Extensions.Logging;
using Threax.Configuration.AzureKeyVault;

namespace Threax.Provision.CheapAzure.Controller.Create
{
    class CreateSqlDatabase : IResourceProcessor<SqlDatabase>
    {
        private readonly ISqlServerManager sqlServerManager;
        private readonly Config config;
        private readonly IKeyVaultManager keyVaultManager;
        private readonly ICredentialLookup credentialLookup;
        private readonly IArmTemplateManager armTemplateManager;
        private readonly ISqlServerFirewallRuleManager sqlServerFirewallRuleManager;
        private readonly IKeyVaultAccessManager keyVaultAccessManager;
        private readonly ILogger<CreateSqlDatabase> logger;
        private readonly ThreaxAzureKeyVaultConfig azureKeyVaultConfig;
        private readonly Random rand = new Random();

        public CreateSqlDatabase(ISqlServerManager sqlServerManager, Config config, IKeyVaultManager keyVaultManager, ICredentialLookup credentialLookup, IArmTemplateManager armTemplateManager, ISqlServerFirewallRuleManager sqlServerFirewallRuleManager, IKeyVaultAccessManager keyVaultAccessManager, ILogger<CreateSqlDatabase> logger, ThreaxAzureKeyVaultConfig azureKeyVaultConfig)
        {
            this.sqlServerManager = sqlServerManager;
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.credentialLookup = credentialLookup;
            this.armTemplateManager = armTemplateManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.logger = logger;
            this.azureKeyVaultConfig = azureKeyVaultConfig;
        }

        public async Task Execute(SqlDatabase resource)
        {
            var readerKeyBase = $"threaxpipe-{resource.Name}-readwrite" ?? throw new InvalidOperationException($"You must include a '{nameof(SqlDatabase.Name)}' property on '{nameof(SqlDatabase)}' types.");
            var ownerKeyBase = $"threaxpipe-{resource.Name}-owner";

            //In this setup there is actually only 1 db to save money.
            //So both the sql server and the db will be provisioned in this step.
            //You would want to have separate dbs in a larger setup.
            await keyVaultAccessManager.Unlock(config.InfraKeyVaultName, config.UserId);
            await keyVaultAccessManager.Unlock(azureKeyVaultConfig.VaultName, config.UserId);

            var saCreds = await credentialLookup.GetOrCreateCredentials(config.InfraKeyVaultName, config.SqlSaBaseKey, FixPass, FixUser);
            var saConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, saCreds.User, saCreds.Pass);

            //Setup logical server
            logger.LogInformation($"Setting up SQL Logical Server '{config.SqlServerName}' in Resource Group '{config.ResourceGroup}'.");
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlServer(config.SqlServerName, saCreds.User, saCreds.Pass.ToSecureString()));
            await sqlServerFirewallRuleManager.Unlock(config.SqlServerName, config.ResourceGroup, config.MachineIp, config.MachineIp);

            //Setup shared sql db
            logger.LogInformation($"Setting up Shared SQL Database '{config.SqlDbName}' on SQL Logical Server '{config.SqlServerName}'.");
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlDb(config.SqlServerName, config.SqlDbName));

            //Setup user in new db
            logger.LogInformation($"Setting up user for {readerKeyBase} in Shared SQL Database '{config.SqlDbName}' on SQL Logical Server '{config.SqlServerName}'.");
            var dbContext = new ProvisionDbContext(saConnectionString);
            var readWriteCreds = await credentialLookup.GetOrCreateCredentials(azureKeyVaultConfig.VaultName, readerKeyBase, FixPass, FixUser);
            var ownerCreds = await credentialLookup.GetOrCreateCredentials(azureKeyVaultConfig.VaultName, ownerKeyBase, FixPass, FixUser);
            int result;
            //This isn't great, but just ignore this exception for now. If the user isn't created the lines below will fail.
            try
            {
                result = await dbContext.Database.ExecuteSqlRawAsync($"CREATE USER {readWriteCreds.User} WITH PASSWORD = '{readWriteCreds.Pass}';");
            }
            catch (SqlException) { }
            try
            {
                result = await dbContext.Database.ExecuteSqlRawAsync($"CREATE USER {ownerCreds.User} WITH PASSWORD = '{ownerCreds.Pass}';");
            }
            catch (SqlException) { }
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER USER {readWriteCreds.User} WITH PASSWORD = '{readWriteCreds.Pass}';");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER USER {ownerCreds.User} WITH PASSWORD = '{ownerCreds.Pass}';");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE db_datareader ADD MEMBER {readWriteCreds.User}");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE db_datawriter ADD MEMBER {readWriteCreds.User}");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE db_owner ADD MEMBER {ownerCreds.User}");

            var appConnectionString = await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, resource.ConnectionStringName);
            if (appConnectionString == null || readWriteCreds.Created)
            {
                appConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, readWriteCreds.User, readWriteCreds.Pass);
                await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.ConnectionStringName, appConnectionString);
            }

            if (!String.IsNullOrEmpty(resource.OwnerConnectionStringName))
            {
                var ownerConnectionString = await keyVaultManager.GetSecret(azureKeyVaultConfig.VaultName, resource.OwnerConnectionStringName);
                if (ownerConnectionString == null || ownerCreds.Created)
                {
                    ownerConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, ownerCreds.User, ownerCreds.Pass);
                    await keyVaultManager.SetSecret(azureKeyVaultConfig.VaultName, resource.OwnerConnectionStringName, ownerConnectionString);
                }
            }
        }

        private String FixPass(String input)
        {
            return $"{input}!2Ab";
        }

        private String FixUser(String input)
        {
            var output = input.Replace('+', RandomLetter()).Replace('/', RandomLetter()).Replace('=', RandomLetter());
            return RandomLetter() + output; //Ensure first character is a letter
        }

        private char RandomLetter()
        {
            return (char)rand.Next(97, 123);
        }
    }
}
