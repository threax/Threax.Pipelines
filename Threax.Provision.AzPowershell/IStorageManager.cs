using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IStorageManager
    {
        Task<string> GetAccessKey(string AccountName, string ResourceGroupName);
    }
}