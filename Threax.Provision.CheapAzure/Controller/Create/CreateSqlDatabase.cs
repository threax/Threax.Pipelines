﻿using Microsoft.Data.SqlClient;
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
        private readonly Random rand = new Random();

        public CreateSqlDatabase(ISqlServerManager sqlServerManager, Config config, IKeyVaultManager keyVaultManager, ICredentialLookup credentialLookup, IArmTemplateManager armTemplateManager, ISqlServerFirewallRuleManager sqlServerFirewallRuleManager, IKeyVaultAccessManager keyVaultAccessManager, ILogger<CreateSqlDatabase> logger)
        {
            this.sqlServerManager = sqlServerManager;
            this.config = config;
            this.keyVaultManager = keyVaultManager;
            this.credentialLookup = credentialLookup;
            this.armTemplateManager = armTemplateManager;
            this.sqlServerFirewallRuleManager = sqlServerFirewallRuleManager;
            this.keyVaultAccessManager = keyVaultAccessManager;
            this.logger = logger;
        }

        public async Task Execute(SqlDatabase resource)
        {
            var credKeyBase = $"{resource.Name}-db" ?? throw new InvalidOperationException($"You must include a '{nameof(SqlDatabase.Name)}' property on '{nameof(SqlDatabase)}' types.");

            //In this setup there is actually only 1 db to save money.
            //So both the sql server and the db will be provisioned in this step.
            //You would want to have separate dbs in a larger setup.
            await keyVaultAccessManager.Unlock(config.KeyVaultName, config.UserId);

            var saCreds = await credentialLookup.GetOrCreateCredentials(config.KeyVaultName, config.SqlSaBaseKey, FixPass, FixUser);

            //Setup logical server
            logger.LogInformation($"Setting up SQL Logical Server '{config.SqlServerName}' in Resource Group '{config.ResourceGroup}'.");
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlServer(config.SqlServerName, saCreds.User, saCreds.Pass.ToSecureString()));
            var saConnectionString = await keyVaultManager.GetSecret(config.KeyVaultName, config.SaConnectionStringSecretName);
            if (saConnectionString == null || saCreds.Created)
            {
                saConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, saCreds.User, saCreds.Pass);
                await keyVaultManager.SetSecret(config.KeyVaultName, config.SaConnectionStringSecretName, saConnectionString);
            }

            //Setup shared sql db
            logger.LogInformation($"Setting up Shared SQL Database '{config.SqlDbName}' on SQL Logical Server '{config.SqlServerName}'.");
            await this.armTemplateManager.ResourceGroupDeployment(config.ResourceGroup, new ArmSqlDb(config.SqlServerName, config.SqlDbName));

            //Setup user in new db
            logger.LogInformation($"Setting up user for {credKeyBase} in Shared SQL Database '{config.SqlDbName}' on SQL Logical Server '{config.SqlServerName}'.");
            await sqlServerFirewallRuleManager.Unlock(config.SqlServerName, config.ResourceGroup, config.MachineIp, config.MachineIp);
            var dbContext = new ProvisionDbContext(saConnectionString);
            var appCreds = await credentialLookup.GetOrCreateCredentials(config.KeyVaultName, credKeyBase, FixPass, FixUser);
            int result;
            try
            {
                result = await dbContext.Database.ExecuteSqlRawAsync($"CREATE USER {appCreds.User} WITH PASSWORD = '{appCreds.Pass}';");
            }
            catch (SqlException)
            {
                //This isn't great, but just ignore this exception for now.
                //If the user isn't created the lines below will fail.
            }
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER USER {appCreds.User} WITH PASSWORD = '{appCreds.Pass}';");
            result = await dbContext.Database.ExecuteSqlRawAsync($"ALTER ROLE db_owner ADD MEMBER {appCreds.User}");

            var appConnectionString = await keyVaultManager.GetSecret(config.KeyVaultName, resource.ConnectionStringName);
            if (appConnectionString == null || appCreds.Created)
            {
                appConnectionString = sqlServerManager.CreateConnectionString(config.SqlServerName, config.SqlDbName, appCreds.User, appCreds.Pass);
                await keyVaultManager.SetSecret(config.KeyVaultName, resource.ConnectionStringName, appConnectionString);
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