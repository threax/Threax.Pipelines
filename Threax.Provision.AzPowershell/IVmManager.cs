using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IVmManager
    {
        Task RunCommand(string Name, string ResourceGroupName, string CommandId, string ScriptPath);
        Task RunCommandFromString(string Name, string ResourceGroupName, string CommandId, string command, string tempFileName = null);
    }
}