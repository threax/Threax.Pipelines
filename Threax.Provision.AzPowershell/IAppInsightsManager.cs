using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IAppInsightsManager
    {
        Task<string> GetAppInsightsInstrumentationKey(string Name, string ResourceGroupName);
    }
}