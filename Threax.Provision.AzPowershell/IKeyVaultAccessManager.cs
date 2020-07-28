using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultAccessManager
    {
        Task Unlock(string keyVaultName, Guid userId);
    }
}