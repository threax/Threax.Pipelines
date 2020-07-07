using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IWebAppIdentityManager
    {
        Task<Guid> GetOrCreateWebAppIdentity(string webAppName, string resourceGroupName);
    }
}