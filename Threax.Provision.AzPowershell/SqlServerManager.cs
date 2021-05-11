using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Threax.ProcessHelper;

namespace Threax.Provision.AzPowershell
{
    public class SqlServerManager : ISqlServerManager
    {
        private readonly IShellRunner shellRunner;

        public SqlServerManager(IShellRunner shellRunner)
        {
            this.shellRunner = shellRunner;
        }

        public Task Create(String name, String resourceGroupName, String location, String adminUser, String adminPass)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Sql");
            pwsh.AddCommand($"$secStringPassword = ConvertTo-SecureString {adminPass} -AsPlainText");
            pwsh.AddCommand($"$credObject = New-Object System.Management.Automation.PSCredential ({adminUser}, $secStringPassword)");
            pwsh.AddResultCommand($"New-AzSqlServer -ServerName {name} -SqlAdministratorCredentials $credObject -Location {location} -ResourceGroupName {resourceGroupName}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error creating Sql Server '{name}' in Resource Group '{resourceGroupName}' at '{location}'.");
        }

        public Task SetFirewallRule(String name, String serverName, String resourceGroupName, String startIp, String endIp)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Sql");
            pwsh.AddResultCommand($"New-AzSqlServerFirewallRule -FirewallRuleName {name} -StartIpAddress {startIp} -EndIpAddress {endIp} -ServerName {serverName} -ResourceGroupName {resourceGroupName}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error setting firewall rule for '{name}' on server {serverName}.");
        }

        public Task RemoveFirewallRule(String name, String serverName, String resourceGroupName)
        {
            var pwsh = shellRunner.CreateCommandBuilder();

            pwsh.SetUnrestrictedExecution();
            pwsh.AddCommand($"Import-Module Az.Sql");
            pwsh.AddResultCommand($"Remove-AzSqlServerFirewallRule -FirewallRuleName {name} -ServerName {serverName} -ResourceGroupName {resourceGroupName}");

            return shellRunner.RunProcessVoidAsync(pwsh,
                invalidExitCodeMessage: $"Error removing firewall rule for '{name}' on server {serverName}.");
        }

        public String CreateConnectionString(String serverName, String initialCatalog, String user, String pass)
        {
            return $"Server={serverName}.database.windows.net,1433;Initial Catalog={initialCatalog};Persist Security Info=False;User ID={user};Password={pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }
    }
}
