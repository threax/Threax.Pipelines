using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface ISqlServerFirewallRuleManager
    {
        Task Unlock(String serverName, String resourceGroupName, String startIp, String endIp);
    }
}