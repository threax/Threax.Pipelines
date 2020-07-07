using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IKeyVaultAccessManager
    {
        void Dispose();
        Task Unlock(string keyVaultName, Guid userId);
    }
}