using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class SqlServerFirewallRuleManager : IDisposable, ISqlServerFirewallRuleManager
    {
        private readonly ISqlServerManager sqlServerManager;
        private List<FirewallRuleInfo> createdRules = new List<FirewallRuleInfo>();

        public SqlServerFirewallRuleManager(ISqlServerManager sqlServerManager)
        {
            this.sqlServerManager = sqlServerManager;
        }

        public async Task Unlock(String serverName, String resourceGroupName, String startIp, String endIp)
        {
            //If this rule is already created, skip it
            if(createdRules.Any(i => i.ServerName == serverName && i.ResourceGroupName == resourceGroupName && i.StartIp == startIp && i.EndIp == endIp))
            {
                return;
            }

            var ruleName = Guid.NewGuid().ToString();
            await sqlServerManager.SetFirewallRule(ruleName, serverName, resourceGroupName, startIp, endIp);
            this.createdRules.Add(new FirewallRuleInfo()
            {
                ResourceGroupName = resourceGroupName,
                RuleName = ruleName,
                ServerName = serverName,
                StartIp = startIp,
                EndIp = endIp
            });
        }

        public void Dispose()
        {
            var ruleTasks = createdRules.Select(i => Task.Run(() => this.sqlServerManager.RemoveFirewallRule(i.RuleName, i.ServerName, i.ResourceGroupName))).ToList();
            foreach (var task in ruleTasks)
            {
                task.GetAwaiter().GetResult();
            }
        }

        class FirewallRuleInfo
        {
            public String ServerName { get; set; }

            public String ResourceGroupName { get; set; }

            public String RuleName { get; set; }

            public String StartIp { get; set; }

            public String EndIp { get; set; }
        }
    }
}
