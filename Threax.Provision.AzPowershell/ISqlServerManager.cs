using System;
using System.Security;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface ISqlServerManager
    {
        Task Create(string name, string resourceGroupName, string location, string adminUser, string adminPass);
        
        Task RemoveFirewallRule(string name, string serverName, string resourceGroupName);
        
        Task SetFirewallRule(string name, string serverName, string resourceGroupName, string startIp, string endIp);

        String CreateConnectionString(String serverName, String initialCatalog, String user, String pass);
    }
}