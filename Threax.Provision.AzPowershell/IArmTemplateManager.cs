using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public interface IArmTemplateManager
    {
        Task ResourceGroupDeployment(string resourceGroupName, ArmTemplate armTemplate);
        Task ResourceGroupDeployment(string resourceGroupName, string templateFile);
        Task ResourceGroupDeployment(string resourceGroupName, string templateFile, object args);
        Task ResourceGroupDeployment(string resourceGroupName, string templateFile, string? templateParametersFile);
        Task ResourceGroupDeployment(string resourceGroupName, string templateFile, string? templateParameterFile, object args);
        Task SubscriptionDeployment(string resourceGroupName, ArmTemplate armTemplate);
        Task SubscriptionDeployment(string location, string templateFile);
        Task SubscriptionDeployment(string location, string templateFile, object args);
        Task SubscriptionDeployment(string location, string templateFile, string? templateParametersFile);
        Task SubscriptionDeployment(string location, string templateFile, string? templateParameterFile, object args);
    }
}