using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IStorageManager
    {
        Task<string> GetAccessKey(string AccountName, string ResourceGroupName);

        String CreateConnectionString(String accountName, String accountKey);
    }
}