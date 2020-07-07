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

        public CreateSqlDatabase(ISqlServerManager sqlServerManager, Config config, IKeyVaultManager keyVaultManager, ICredentialLookup credentialLookup, IArmTemplateManager armTemplateManager, ISqlServerFirewallRuleManager sqlServerFirewallRuleManager, IKeyVaultAccessManager keyVaultAccessManager)
        {
            this.sqlServerManager = sqlServerManager;
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.credentialLookup = credentialLookup;
            this.armTemplateManager = armTemplateManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
        }

        public async Task Execute(SqlDatabase resource)
        {
            var credKeyBase = $"{resource.Name}-db" ?? throw new InvalidOperationException($"You must include a '{nameof(SqlDatabase.Name)}' property on '{nameof(SqlDatabase)}' types.");

            //In this setup there is actually only 1 db to save money.
            //So both the sql server and the db will be provisioned in this step.
            //You would want to have separate dbs in a larger setup.
            await keyVaultAccessManager.Unlock(config.KeyVaultName, config.UserId);

            var saCreds = await credentialLookup.GetCredentials(config.KeyVaultName, "sqlsrv-sa", FixPass, FixUser);

            //Setup logical server
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlServer(config.SqlServerName, saCreds.Username, saCreds.Password));
            var saConnectionString = await keyVaultManager.GetSecret(config.KeyVaultName, config.SaConnectionStringSecretName);
            if (saConnectionString == null || saCreds.Created)
            {
                saConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, saCreds.Username, saCreds.Password);
                await keyVaultManager.SetSecret(config.KeyVaultName, config.SaConnectionStringSecretName, saConnectionString);
            }

            //Setup shared sql db
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlDb(config.SqlServerName, config.SqlDbName));

            //Setup user in new db
            await sqlServerFirewallRuleManager.Unlock(config.SqlServerName, config.ResourceGroup, config.MachineIp, config.MachineIp);
            var dbContext = new ProvisionDbContext(saConnectionString);
            var appCreds = await credentialLookup.GetCredentials(config.KeyVaultName, credKeyBase, FixPass, FixUser);
            int result;
            try
            {
                result = await dbContext.Database.ExecuteSqlRawAsync($"CREATE USER {appCreds.Username} WITH PASSWORD = '{appCreds.Password}';");
            }
            catch (SqlException)
            {
                //This isn't great, but just ignore this exception for now.
                //If the user isn't created the lines below will fail.
            }
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER USER {appCreds.Username} WITH PASSWORD = '{appCreds.Password}';");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE db_owner ADD MEMBER {appCreds.Username}");

            var appConnectionString = await keyVaultManager.GetSecret(config.KeyVaultName, resource.ConnectionStringName);
            if (appConnectionString == null || appCreds.Created)
            {
                appConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, appCreds.Username, appCreds.Password);
                await keyVaultManager.SetSecret(config.KeyVaultName, resource.ConnectionStringName, appConnectionString);
            }
        }

        private String FixPass(String input)
        {
            return $"{input}!2Ab";
        }

        private String FixUser(String input)
        {
            return input.Replace('+', 'g').Replace('/', 'e').Replace('=', 'p');
        }
    }
}
