using System;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface ISubscriptionManager
    {
        Task SetContext(Guid subscriptionId);
    }
}