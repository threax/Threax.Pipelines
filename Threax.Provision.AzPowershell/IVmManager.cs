using System.Collections;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IVmManager
    {
        Task RunCommand(string Name, string ResourceGroupName, string CommandId, string ScriptPath, Hashtable Parameter);

        Task<string> GetPublicIp(string Name);

        Task SetSecurityRuleAccess(string NetworkSecurityGroup, string ResourceGroup, string Name, string Access);
    }
}