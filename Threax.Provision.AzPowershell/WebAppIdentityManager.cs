using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Threax.Provision.AzPowershell
{
    public class WebAppIdentityManager : IWebAppIdentityManager
    {
        private readonly IWebAppManager webAppManager;

        public WebAppIdentityManager(IWebAppManager webAppManager)
        {
            this.webAppManager = webAppManager;
        }

        public async Task<Guid> GetOrCreateWebAppIdentity(String webAppName, String resourceGroupName)
        {
            var appInfo = await webAppManager.GetWebApp(webAppName, resourceGroupName);
            if (appInfo.IdentityObjectId != null)
            {
                return appInfo.IdentityObjectId.Value;
            }

            appInfo = await webAppManager.CreateWebAppIdentity(webAppName, resourceGroupName);
            if (appInfo.IdentityObjectId != null)
            {
                return appInfo.IdentityObjectId.Value;
            }

            //Try one last time to lookup the id in case the above failed
            appInfo = await webAppManager.GetWebApp(webAppName, resourceGroupName);
            if (appInfo.IdentityObjectId != null)
            {
                return appInfo.IdentityObjectId.Value;
            }

            throw new InvalidOperationException("An unknown error occured creating a web app identity and one could not be found.");
        }
    }
}
