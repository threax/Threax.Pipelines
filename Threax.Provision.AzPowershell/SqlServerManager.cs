using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class SqlServerManager : ISqlServerManager
    {
        private readonly ILogger<SqlServerManager> logger;

        public SqlServerManager(ILogger<SqlServerManager> logger)
        {
            this.logger = logger;
        }

        public async Task Create(String name, String resourceGroupName, String location, String adminUser, String adminPass)
        {
            using var securePass = new SecureString();
            foreach (var c in adminPass)
            {
                securePass.AppendChar(c);
            }
            securePass.MakeReadOnly();
            var creds = new PSCredential(adminUser, securePass);

            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Sql");
            pwsh.AddParamLine(new { name, location, resourceGroupName, creds });
            pwsh.AddScript($"New-AzSqlServer -ServerName ${nameof(name)} -SqlAdministratorCredentials ${nameof(creds)} -Location ${nameof(location)} -ResourceGroupName ${nameof(resourceGroupName)}");

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error creating Sql Server '{name}' in Resource Group '{resourceGroupName}' at '{location}'.");
        }

        public async Task SetFirewallRule(String name, String serverName, String resourceGroupName, String startIp, String endIp)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Sql");
            var parm = new
            {
                FirewallRuleName = name,
                StartIpAddress = startIp,
                EndIpAddress = endIp,
                ServerName = serverName,
                ResourceGroupName = resourceGroupName
            };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("New-AzSqlServerFirewallRule", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error setting firewall rule for '{name}' on server {serverName}.");
        }

        public async Task RemoveFirewallRule(String name, String serverName, String resourceGroupName)
        {
            using var pwsh = PowerShell.Create()
                .PrintInformationStream(logger)
                .PrintErrorStream(logger);

            pwsh.SetUnrestrictedExecution();
            pwsh.AddScript("Import-Module Az.Sql");
            var parm = new
            {
                FirewallRuleName = name,
                ServerName = serverName,
                ResourceGroupName = resourceGroupName
            };
            pwsh.AddParamLine(parm);
            pwsh.AddCommandWithParams("Remove-AzSqlServerFirewallRule", parm);

            var outputCollection = await pwsh.RunAsync();

            pwsh.ThrowOnErrors($"Error removing firewall rule for '{name}' on server {serverName}.");
        }

        public String CreateConnectionString(String serverName, String initialCatalog, String user, String pass)
        {
            return $"Server={serverName}.database.windows.net,1433;Initial Catalog={initialCatalog};Persist Security Info=False;User ID={user};Password={pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }
    }
}
